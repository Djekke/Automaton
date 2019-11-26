namespace CryoFall.Automaton.Features
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using CryoFall.Automaton.ClientSettings;
    using CryoFall.Automaton.ClientSettings.Options;

    public abstract class ProtoFeature
    {
        // All enabled Entity from EntityList that enabled in settings.
        public List<IProtoEntity> EnabledEntityList { get; set; }

        public List<string> DefaultEnabledList { get; protected set; } = new List<string>();

        // All options for this feature.
        public List<IOption> Options { get; protected set; } = new List<IOption>();

        // Unique identifier representing this feature.
        public string Id { get; private set; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        // List of items which activate feature.
        public List<IProtoEntity> RequiredItemList;

        // Is this feature enabled.
        public virtual bool IsEnabled { get; set; }

        public string IsEnabledText => "Enable this feature";

        // List of all entity of interest for this feature.
        public List<IProtoEntity> EntityList;

        protected IItem SelectedItem => ClientHotbarSelectedItemManager.SelectedItem;

        protected ICharacter CurrentCharacter => Api.Client.Characters.CurrentPlayerCharacter;

        protected PlayerCharacterPrivateState PrivateState => PlayerCharacter.GetPrivateState(CurrentCharacter);

        public void PrepareProto()
        {
            var name = this.GetType().Name;
            Id = name.Replace("Feature", "");

            var entityList = new List<IProtoEntity>();
            var requiredItemList = new List<IProtoEntity>();

            PrepareFeature(entityList, requiredItemList);

            EntityList = entityList;
            RequiredItemList = requiredItemList;
        }

        protected abstract void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList);

        public virtual void PrepareOptions(SettingsFeature settingsFeature)
        {
            AddOptionIsEnabled(settingsFeature);
            Options.Add(new OptionSeparator());
            AddOptionEntityList(settingsFeature);
        }

        protected void AddOptionIsEnabled(SettingsFeature settingsFeature)
        {
            Options.Add(new OptionCheckBox(
                parentSettings: settingsFeature,
                id: "IsEnabled",
                label: IsEnabledText,
                defaultValue: false,
                valueChangedCallback: value =>
                {
                    settingsFeature.IsEnabled = IsEnabled = value;
                    settingsFeature.OnIsEnabledChanged(value);
                }));
        }

        protected void AddOptionEntityList(SettingsFeature settingsFeature)
        {
            Options.Add(new OptionEntityList(
                parentSettings: settingsFeature,
                id: "EnabledEntityList",
                entityList: EntityList.OrderBy(entity => entity.Id),
                defaultEnabledList: DefaultEnabledList,
                onEnabledListChanged: enabledList => EnabledEntityList = enabledList));
        }

        /// <summary>
        /// Called by client component every tick.
        /// </summary>
        public virtual void Update(double deltaTime)
        {
            if (!(IsEnabled && CheckPrecondition()))
            {
                return;
            }
            // Check queue and do something.
        }

        /// <summary>
        /// Called by client component on specific time interval.
        /// </summary>
        public virtual void Execute()
        {
            if (!(IsEnabled && CheckPrecondition()))
            {
                return;
            }
            // Check surrounding
            // Add action to queue
        }

        /// <summary>
        /// Check initial condition (right tool equipped or other checks).
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckPrecondition()
        {
            if (RequiredItemList?.Count == 0)
            {
                return true;
            }
            var selectedProtoItem = SelectedItem?.ProtoItem;
            if (selectedProtoItem != null && RequiredItemList.Contains(selectedProtoItem))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stop everything.
        /// </summary>
        public virtual void Stop()
        {
        }

        /// <summary>
        /// Init on component enabled.
        /// </summary>
        public virtual void Start(ClientComponent parentComponent)
        {
            SetupSubscriptions(parentComponent);
        }

        /// <summary>
        /// Setup any of subscriptions
        /// </summary>
        public virtual void SetupSubscriptions(ClientComponent parentComponent)
        {
        }
    }
}
