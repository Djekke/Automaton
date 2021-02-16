namespace CryoFall.Automaton.Features
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Systems.FishingBaitReloadingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.FishingSystem;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;
    using CryoFall.Automaton.ClientSettings;

    public class FeatureAutoFishing: ProtoFeature<FeatureAutoFishing>
    {
        private FeatureAutoFishing() { }

        public override string Name => "AutoFishing";

        public override string Description => "AutoFishing near water if fishing rod in hands.";

        private List<IProtoEntity> PermittedTiles = new List<IProtoEntity>();

        private bool isAlreadyPulling = false;

        protected override void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList)
        {
            PermittedTiles.AddRange(Api.FindProtoEntities<IProtoTileWater>().Where(w => w.IsFishingAllowed));

            requiredItemList.AddRange(Api.FindProtoEntities<IProtoItemToolFishing>());
        }

        public override void PrepareOptions(SettingsFeature settingsFeature)
        {
            AddOptionIsEnabled(settingsFeature);
        }

        /// <summary>
        /// Called by client component every tick.
        /// </summary>
        public override void Update(double deltaTime)
        {
            // Check if fish biting, try to pull
            if (IsEnabled
                && !isAlreadyPulling
                && PrivateState.CurrentActionState != null
                && PrivateState.CurrentActionState is FishingActionState fishingActionState)
            {
                var fishingSession = fishingActionState.SharedFishingSession;
                if (!(fishingSession is null) && FishingSession.GetPublicState(fishingSession).IsFishBiting)
                {
                    // fish baiting, request pulling
                    fishingActionState.ClientOnItemUse();
                    isAlreadyPulling = true;
                }
            }
        }

        /// <summary>
        /// Called by client component on specific time interval.
        /// </summary>
        public override void Execute()
        {
            //Try to start fishing
            if (IsEnabled
                && CheckPrecondition()
                && FindPermittedTilesNearby.Any()
                && PrivateState.CurrentActionState == null)
            {
                isAlreadyPulling = false;
                if(ComponentFishingVisualizer.TryGetFor(CurrentCharacter, out _))
                {
                    // Wait for animation
                    return;
                }
                
                if(!IsPlayerHasBait())
                {
                    // No bait found
                    return;
                }

                var fishingTargetPosition = FindPermittedTilesNearby
                    .Select(GetTileCenterPosition)
                    .OrderBy(t => t.DistanceTo(CurrentCharacter.Position))
                    .First();

                var request = new FishingActionRequest(CurrentCharacter, SelectedItem, fishingTargetPosition);
                FishingSystem.Instance.SharedStartAction(request);
            }
        }

        private bool IsPlayerHasBait()
        {
            var fishingRodPublicState = SelectedItem.GetPublicState<ItemFishingRodPublicState>();

            if (fishingRodPublicState.CurrentProtoBait is null
                || FishingSystem.SharedFindBaitItem(CurrentCharacter, fishingRodPublicState.CurrentProtoBait) is null)
            {
                // no bait selected - try switch it
                FishingBaitReloadingSystem.ClientTrySwitchBaitType();
                if (fishingRodPublicState.CurrentProtoBait is null
                    || FishingSystem.SharedFindBaitItem(CurrentCharacter, fishingRodPublicState.CurrentProtoBait) is null)
                {
                    // no bait selected after a switch attempt
                    return false;
                }
            }
            return true;
        }

        private IEnumerable<Tile> FindPermittedTilesNearby
            => CurrentCharacter.Tile.EightNeighborTiles.Where(t => PermittedTiles.Contains(t.ProtoTile));

        private Vector2D GetTileCenterPosition(Tile tile) => tile.Position.ToVector2D() + (0.5, 0.5);

        /// <summary>
        /// Stop everything.
        /// </summary>
        public override void Stop()
        {
            if (PrivateState.CurrentActionState != null
                && PrivateState.CurrentActionState is FishingActionState fishingActionState)
            {
                fishingActionState.Cancel();
                isAlreadyPulling = false;
            }
        }
    }
}
