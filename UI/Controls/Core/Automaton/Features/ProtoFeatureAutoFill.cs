namespace CryoFall.Automaton.UI.Controls.Core.Automaton.Features
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.BottleRefillSystem;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using System.Collections.Generic;
    using System.Linq;

    public class ProtoFeatureAutoFill: ProtoFeature
    {
        public override string Name => "AutoFill";

        public override string Description => "AutoFill empty bottles near sea.";

        private bool fillInProgress = false;

        private ushort itemCount = 0;

        private double finishingTime;

        protected override void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList)
        {
            entityList.AddRange(Api.FindProtoEntities<ItemBottleWaterSalty>());

            requiredItemList.AddRange(Api.FindProtoEntities<ItemBottleEmpty>());
        }

        /// <summary>
        /// Called by client component every tick.
        /// </summary>
        public override void Update(double deltaTime)
        {
            if (!(IsEnabled && CheckPrecondition()) ||
                finishingTime > 0.5)
            {
                Stop();
                return;
            }

            if (SelectedItem.Count > itemCount)
            {
                itemCount = SelectedItem.Count;
            }

            if (fillInProgress)
            {
                var characterPrivateState = PlayerCharacter.GetPrivateState(CurrentCharacter);
                if (characterPrivateState.CurrentActionState == null)
                {
                    finishingTime += deltaTime;

                    if (SelectedItem.Count < itemCount)
                    {
                        fillInProgress = false;
                        finishingTime = 0.0;
                        Fill();
                    }
                }
            }
        }

        /// <summary>
        /// Called by client component on specific time interval.
        /// </summary>
        public override void Execute( )
        {
            if (!(IsEnabled && CheckPrecondition()))
            {
                return;
            }

            Fill();
        }

        private void Fill()
        {
            if (!fillInProgress && IsWaterNearby())
            {
                fillInProgress = true;
                itemCount = SelectedItem.Count;
                finishingTime = 0.0;
                BottleRefillSystem.Instance.ClientTryStartAction();
            }
        }

        private bool IsWaterNearby()
        {
            var tile = CurrentCharacter.Tile;
            if (tile.ProtoTile is TileWaterSea)
            {
                return true;
            }

            if (tile.EightNeighborTiles.Where(t => t.ProtoTile is TileWaterSea)?.Count() > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stop everything.
        /// </summary>
        public override void Stop()
        {
            if (fillInProgress)
            {
                fillInProgress = false;
                var characterPrivateState = PlayerCharacter.GetPrivateState(CurrentCharacter);
                if (characterPrivateState.CurrentActionState is IActionState state)
                {
                    state.Cancel();
                }
            }
        }
    }
}
