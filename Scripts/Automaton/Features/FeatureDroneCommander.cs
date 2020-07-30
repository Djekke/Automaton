namespace CryoFall.Automaton.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDroneControl;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using CryoFall.Automaton.ClientSettings;
    using CryoFall.Automaton.ClientSettings.Options;

    public class FeatureDroneCommander : ProtoFeature<FeatureDroneCommander>
    {
        private FeatureDroneCommander() { }

        public override string Name => "DroneCommander";

        public override string Description => "Automatically commands drones to harvest selected ressources";

        public double AllowedTreeGrowthFractionLevel { get; set; }

        public string AllowedTreeGrowthFractionLevelText => "Allowed tree growth level";

        public double DroneDurabilityThreshold { get; set; }

        public string DroneDurabilityThresholdText => "Max allowed drone durability";

        protected override void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList)
        {
            entityList.AddRange(Api.FindProtoEntities<IProtoObjectMineral>().Where(m => m.IsAllowDroneMining));
            entityList.AddRange(Api.FindProtoEntities<IProtoObjectTree>());

            requiredItemList.AddRange(Api.FindProtoEntities<IProtoItemDroneControl>());
        }

        public override void PrepareOptions(SettingsFeature settingsFeature)
        {
            AddOptionIsEnabled(settingsFeature);
            Options.Add(new OptionSeparator());
            Options.Add(new OptionSlider(
                parentSettings: settingsFeature,
                id: "AllowedTreeGrowthFractionLevel",
                label: AllowedTreeGrowthFractionLevelText,
                defaultValue: 1.0,
                valueChangedCallback: value =>
                {
                    AllowedTreeGrowthFractionLevel = value;
                }));
            Options.Add(new OptionSlider(
                parentSettings: settingsFeature,
                id: "DroneDurabilityThreshold",
                label: DroneDurabilityThresholdText,
                defaultValue: 0.1,
                valueChangedCallback: value =>
                {
                    DroneDurabilityThreshold = value;
                }));
            Options.Add(new OptionSeparator());
            AddOptionEntityList(settingsFeature);
        }

        public override void Execute()
        {
            if (!(IsEnabled && CheckPrecondition()))
            {
                return;
            }
            if (!CharacterDroneControlSystem.SharedIsMaxDronesToControlNumberExceeded(
                    CurrentCharacter,
                    clientShowErrorNotification: false))
            {
                TrySendDrone();
            }
        }

        private void TrySendDrone()
        {
            var targetsList =
                Api.Client.World.GetStaticWorldObjectsOfProto<IProtoStaticWorldObject>()
                    .Where(IsValidObject)
                    .OrderBy(o => CurrentCharacter.Position.DistanceTo(o.TilePosition.ToVector2D()))
                    .ToList();
            if (targetsList.Count == 0)
            {
                return;
            }
            int targetN = 0;
            int droneControlLimit = ((IProtoItemDroneControl)SelectedItem.ProtoGameObject).MaxDronesToControl;
            int droneNumberToSend = Math.Min(
                droneControlLimit - CurrentCharacter.SharedGetCurrentControlledDronesNumber(),
                targetsList.Count);
            using var tempExceptDrones = Api.Shared.GetTempList<IItem>();
            for (var index = 0; index < droneNumberToSend; index++)
            {
                IItem itemDrone;
                do
                {
                    itemDrone = CharacterDroneControlSystem.ClientSelectNextDrone(tempExceptDrones.AsList());
                    if (itemDrone is null)
                    {
                        return;
                    }
                    tempExceptDrones.Add(itemDrone);
                } while (ItemDurabilitySystem.SharedGetDurabilityFraction(itemDrone) < DroneDurabilityThreshold);

                if (!CharacterDroneControlSystem.ClientTryStartDrone(itemDrone,
                                                                     targetsList[targetN].TilePosition,
                                                                     showErrorNotification: false))
                {
                    return;
                }
                targetN++;
            }
        }

        private bool IsValidObject(IStaticWorldObject staticWorldObject)
        {
            return EnabledEntityList.Contains(staticWorldObject.ProtoStaticWorldObject)
                   && CharacterDroneControlSystem.SharedIsValidStartLocation(
                       CurrentCharacter,
                       staticWorldObject.TilePosition,
                       out bool hasObstacle)
                   && WorldObjectClaimSystem.SharedIsAllowInteraction(
                       CurrentCharacter,
                       staticWorldObject,
                       showClientNotification: false)
                   && !(staticWorldObject.ProtoStaticWorldObject is IProtoObjectTree tree
                        && tree.SharedGetGrowthProgress(staticWorldObject) < AllowedTreeGrowthFractionLevel)
                   && !CharacterDroneControlSystem.SharedIsTargetAlreadyScheduledForAnyActiveDrone(
                       CurrentCharacter,
                       staticWorldObject.TilePosition,
                       logError: false);
        }
    }
}
