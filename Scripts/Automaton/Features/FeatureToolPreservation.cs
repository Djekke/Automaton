namespace CryoFall.Automaton.Features
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using CryoFall.Automaton.ClientSettings;
    using CryoFall.Automaton.ClientSettings.Options;
    using JetBrains.Annotations;

    public class FeatureToolPreservation : ProtoFeature<FeatureToolPreservation>
    {
        private FeatureToolPreservation() { }

        public override string Name => "ToolPreservation";

        public override string Description => "Alert on item low durability and preserve it.";

        public bool IsAlertNotificationEnabled { get; set; }

        public string IsAlertNotificationEnabledText => "Show alert notification";

        public double AlertThreshold { get; set; }

        public string AlertThresholdText => "Alert below this point";

        public double AlertStep { get; set; }

        public string AlertStepText => "Alert every additional loss";

        public double AlertTimeout { get; set; }

        public string AlertTimeoutText => "Refresh alert every X seconds";

        public bool IsUnequipEnabled { get; set; }

        public string IsUnequipEnabledText => "Try to unequip item";

        public double UnequipThreshold { get; set; }

        public string UnequipThresholdText => "Unequip below this level";

        private IItem lastSelectedItem;

        private Dictionary<IItem, AlertDetails> itemAlerts = new Dictionary<IItem, AlertDetails>();

        protected override void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList)
        {
            entityList.AddRange(Api.FindProtoEntities<IProtoItemWithDurability>().Where(e => e.DurabilityMax > 0));
        }

        public override void PrepareOptions(SettingsFeature settingsFeature)
        {
            AddOptionIsEnabled(settingsFeature);
            Options.Add(new OptionSeparator());
            Options.Add(new OptionCheckBox(
                parentSettings: settingsFeature,
                id: "IsAlertNotificationEnabled",
                label: IsAlertNotificationEnabledText,
                defaultValue: true,
                valueChangedCallback: value => IsAlertNotificationEnabled = value));
            Options.Add(new OptionSlider(
                parentSettings: settingsFeature,
                id: "AlertThreshold",
                label: AlertThresholdText,
                defaultValue: 0.3,
                valueChangedCallback: value => AlertThreshold = value));
            Options.Add(new OptionSlider(
                parentSettings: settingsFeature,
                id: "AlertStep",
                label: AlertStepText,
                defaultValue: 0.05,
                valueChangedCallback: value => AlertStep = value));
            Options.Add(new OptionTextBox<double>(
                parentSettings: settingsFeature,
                id: "AlertTimeout",
                label: AlertTimeoutText,
                defaultValue: 30d,
                valueChangedCallback: value => AlertTimeout = value));
            Options.Add(new OptionSeparator());
            Options.Add(new OptionCheckBox(
                parentSettings: settingsFeature,
                id: "IsUnequipEnabled",
                label: IsUnequipEnabledText,
                defaultValue: true,
                valueChangedCallback: value => IsUnequipEnabled = value));
            Options.Add(new OptionSlider(
                parentSettings: settingsFeature,
                id: "UnequipThreshold",
                label: UnequipThresholdText,
                defaultValue: 0.1,
                valueChangedCallback: value => UnequipThreshold = value));
            Options.Add(new OptionSeparator());
            AddOptionEntityList(settingsFeature);
        }

        /// <summary>
        /// Called by client component on specific time interval.
        /// </summary>
        public override void Execute()
        {
            if (IsEnabled
                && EnabledEntityList.Any())
            {
                CheckSelectedItem();
                CheckEquipment();
            }
        }

        /// <summary>
        /// Init on component enabled.
        /// </summary>
        public override void Start(ClientComponent parentComponent)
        {
            base.Start(parentComponent);

            lastSelectedItem = SelectedItem;
            if (SelectedItem != null)
            {
                itemAlerts.Add(SelectedItem, new AlertDetails());
            }
            ClientHotbarSelectedItemManager.SelectedItemChanged += SelectedItemChanged;
        }

        /// <summary>
        /// Stop everything.
        /// </summary>
        public override void Stop()
        {
            itemAlerts.Clear();

            ClientHotbarSelectedItemManager.SelectedItemChanged -= SelectedItemChanged;
        }

        private void SelectedItemChanged(IItem item)
        {
            if (lastSelectedItem != null)
            {
                itemAlerts.Remove(lastSelectedItem);
            }
            lastSelectedItem = SelectedItem;
            if (item != null)
            {
                itemAlerts.Add(SelectedItem, new AlertDetails());
            }
            CheckSelectedItem();
        }

        private void CheckSelectedItem()
        {
            if (SelectedItem != null
                && EnabledEntityList.Contains(SelectedItem.ProtoItem))
            {
                CheckItemDurability(SelectedItem);
            }
        }

        private void CheckEquipment()
        {
            var equimpentItems = CurrentCharacter
                .SharedGetPlayerContainerEquipment()
                .Items.Where(item => EnabledEntityList.Contains(item.ProtoItem)).ToList();
            foreach (var item in equimpentItems)
            {
                if (item is null)
                {
                    continue;
                }
                CheckItemDurability(item);
            }
        }

        private void CheckItemDurability([NotNull] IItem item)
        {
            double itemDurabilityFraction = ItemDurabilitySystem.SharedGetDurabilityFraction(item);

            if (IsUnequipEnabled
                && itemDurabilityFraction <= UnequipThreshold)
            {
                if (TryToMoveItem(
                        item: item,
                        toContainer: CurrentCharacter.SharedGetPlayerContainerInventory()))
                {
                    if (item != null)
                    {
                        itemAlerts.Remove(item);
                    }
                    return;
                }
            }

            if (!itemAlerts.ContainsKey(item))
            {
                itemAlerts.Add(item, new AlertDetails());
            }

            if (IsAlertNotificationEnabled
                && itemDurabilityFraction <= AlertThreshold
                && (itemAlerts[item].Durability - itemDurabilityFraction >= AlertStep
                    || (AlertTimeout > 0
                        && itemAlerts[item].Time + AlertTimeout < Api.Client.Core.ClientRealTime)))
            {
                itemAlerts[item].Durability = itemDurabilityFraction;
                itemAlerts[item].Time = Api.Client.Core.ClientRealTime;

                NotificationSystem.ClientShowNotification(
                    title: "Item durability low!",
                    color: NotificationColor.Bad,
                    icon: item.ProtoItem.Icon);
            }
        }

        private bool TryToMoveItem(IItem item, IItemsContainer toContainer)
        {
            if (Api.Client.Items.MoveOrSwapItem(
                        item,
                        toContainer,
                        isLogErrors: false))
            {
                NotificationSystem.ClientShowNotification(
                title: "Item was unequipped. (Durability too low)",
                color: NotificationColor.Good,
                icon: item.ProtoItem.Icon);
                return true;
            }
            NotificationSystem.ClientShowNotification(
                title: "Cannot unequip item.",
                color: NotificationColor.Bad,
                icon: item.ProtoItem.Icon);
            return false;
        }

        private class AlertDetails
        {
            public double Durability;

            public double Time;

            public AlertDetails()
            {
                Durability = 10.0;
                Time = 0d;
            }
        }
    }
}
