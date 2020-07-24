namespace CryoFall.Automaton.Features
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.BottleRefillSystem;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class FeatureAutoFill: ProtoFeature<FeatureAutoFill>
    {
        private FeatureAutoFill() { }

        public override string Name => "AutoFill";

        public override string Description => "AutoFill empty bottles near water.";

        private readonly Dictionary<IProtoEntity, List<IProtoEntity>> requiredTilesDictionary =
            new Dictionary<IProtoEntity, List<IProtoEntity>>();

        private List<IProtoEntity> PermittedTiles =>
            requiredTilesDictionary.Where(entry => EnabledEntityList.Contains(entry.Key))
                .SelectMany(entry => entry.Value).ToList();

        private IItem usedItem = null;

        // Wait for server to update item count.
        private bool waitingForServer = false;

        // For hard reset failed action.
        private double fillingActionDuration = 0.0;

        private IActionState lastActionState = null;

        private IClientItemsContainer ContainerHotbar =>
            (IClientItemsContainer) CurrentCharacter.SharedGetPlayerContainerHotbar();

        protected override void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList)
        {
            entityList.AddRange(Api.FindProtoEntities<ItemBottleWaterSalty>());
            requiredTilesDictionary.Add(Api.GetProtoEntity<ItemBottleWaterSalty>(),
                new List<IProtoEntity>(Api.FindProtoEntities<TileWaterSea>()));
            entityList.AddRange(Api.FindProtoEntities<ItemBottleWaterStale>());
            requiredTilesDictionary.Add(Api.GetProtoEntity<ItemBottleWaterStale>(),
                new List<IProtoEntity>(Api.FindProtoEntities<TileWaterLake>()));

            requiredItemList.AddRange(Api.FindProtoEntities<ItemBottleEmpty>());
        }

        /// <summary>
        /// Called by client component every tick.
        /// </summary>
        public override void Update(double deltaTime)
        {
            fillingActionDuration += deltaTime;
            if (waitingForServer &&
                lastActionState is BottleRefillAction bottleRefillAction &&
                fillingActionDuration > bottleRefillAction.DurationSeconds * 2)
            {
                // Hard reset for cases of action never started on server or canceled with exception.
                waitingForServer = false;
            }
        }

        /// <summary>
        /// Called by client component on specific time interval.
        /// </summary>
        public override void Execute()
        {
            TryToStartFillAction();
        }

        private void TryToStartFillAction()
        {
            if (IsEnabled && CheckPrecondition() && IsWaterNearby() && !waitingForServer &&
                PrivateState.CurrentActionState == null)
            {
                BottleRefillSystem.Instance.ClientTryStartAction();
            }
        }

        private bool IsWaterNearby()
        {
            var tile = CurrentCharacter.Tile;
            if (PermittedTiles.Contains(tile.ProtoTile))
            {
                return true;
            }

            if (tile.EightNeighborTiles.Select(t => t.ProtoTile).Intersect(PermittedTiles).Any())
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
            if (lastActionState is BottleRefillAction)
            {
                lastActionState.Cancel();
            }

            waitingForServer = false;
            usedItem = null;

            ContainerHotbar.ItemRemoved -= ContainerHotbarOnItemRemoved;
            ContainerHotbar.ItemCountChanged -= ContainerHotbarOnItemCountChanged;
        }

        /// <summary>
        /// Setup any of subscriptions
        /// </summary>
        public override void SetupSubscriptions(ClientComponent parentComponent)
        {
            base.SetupSubscriptions(parentComponent);

            PrivateState.ClientSubscribe(
                s => s.CurrentActionState,
                OnActionStateChanged,
                parentComponent);
        }

        /// <summary>
        /// Init on component enabled.
        /// </summary>
        public override void Start(ClientComponent parentComponent)
        {
            base.Start(parentComponent);

            ContainerHotbar.ItemRemoved += ContainerHotbarOnItemRemoved;
            ContainerHotbar.ItemCountChanged += ContainerHotbarOnItemCountChanged;

            // Check if there an action in progress.
            if (PrivateState.CurrentActionState != null)
            {
                lastActionState = PrivateState.CurrentActionState;
            }
        }

        private void ContainerHotbarOnItemCountChanged(IItem item, ushort previousCount, ushort currentCount)
        {
            if (usedItem == item && currentCount < previousCount)
            {
                waitingForServer = false;
                TryToStartFillAction();
            }
        }

        private void ContainerHotbarOnItemRemoved(IItem item, byte slotId)
        {
            if (usedItem == item)
            {
                waitingForServer = false;
            }
        }

        private void OnActionStateChanged()
        {
            if (PrivateState.CurrentActionState != null)
            {
                // Action was started.
                lastActionState = PrivateState.CurrentActionState;
                if (lastActionState is BottleRefillAction bottleRefillAction)
                {
                    usedItem = bottleRefillAction.ItemEmptyBottle;
                    waitingForServer = true;
                    fillingActionDuration = 0.0;
                }
            }
            else
            {
                if (lastActionState is BottleRefillAction bottleRefillAction)
                {
                    if (!IsWaterNearby() ||
                        bottleRefillAction.IsCancelled || bottleRefillAction.IsCancelledByServer)
                    {
                        // Action failed: no water nearby or cancelled on server.
                        usedItem = null;
                        waitingForServer = false;
                    }
                }
                else
                {
                    // Other action finished - reset waiting status just in case.
                    usedItem = null;
                    waitingForServer = false;
                }
            }
        }
    }
}
