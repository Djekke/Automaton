namespace CryoFall.Automaton.UI.Features
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using System.Collections.Generic;
    using System.Linq;

    public class FeatureAutoGather: ProtoFeatureWithInteractionQueue
    {
        public override string Name => "AutoGather";

        public override string Description => "Gather berry, herbs and other vegetation, harvest corpses, loot radtowns.";

        private IWorldObject openedLootContainer = null;

        private bool readyForInteraction = true;

        private IActionState lastActionState = null;

        protected override void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList)
        {
            entityList.AddRange(Api.FindProtoEntities<IProtoObjectGatherable>());
        }

        protected override void CheckInteractionQueue()
        {
            if (openedLootContainer != null)
            {
                if (InteractionCheckerSystem.HasInteraction(CurrentCharacter, openedLootContainer, true))
                {
                    // We get container private state, now take all items from container.
                    var q = openedLootContainer.GetPrivateState<LootContainerPrivateState>();
                    var result =
                        CurrentCharacter.ProtoCharacter.ClientTryTakeAllItems(CurrentCharacter, q.ItemsContainer, true);
                    if (result.MovedItems.Count > 0)
                    {
                        NotificationSystem.ClientShowItemsNotification(
                            itemsChangedCount: result.MovedItems
                                .GroupBy(p => p.Key.ProtoItem)
                                .ToDictionary(p => p.Key, p => p.Sum(v => v.Value)));
                    }
                    InteractionCheckerSystem.CancelCurrentInteraction(CurrentCharacter);
                }
                else if (openedLootContainer.ProtoWorldObject
                                            .SharedCanInteract(CurrentCharacter, openedLootContainer, false))
                {
                    // Waiting for container private state from server.
                    return;
                }
                openedLootContainer = null;
                readyForInteraction = true;
            }

            if (!readyForInteraction)
            {
                return;
            }

            // Remove from queue while it have object and they in our whitelist if:
            //  - object is destroyed
            //  - if object is container that we already have looted
            //  - if object not IProtoObjectGatherable
            //  - if we can not interact with object right now
            //  - if we can not gather anything from object
            while (interactionQueue.Count != 0 && EnabledEntityList.Contains(interactionQueue[0].ProtoGameObject) &&
                   (interactionQueue[0].IsDestroyed ||
                    (lastActionState?.TargetWorldObject == interactionQueue[0] &&
                     lastActionState.IsCompleted &&
                     !lastActionState.IsCancelled &&
                     !lastActionState.IsCancelledByServer) ||
                    !(interactionQueue[0].ProtoGameObject is IProtoObjectGatherable protoGatherable) ||
                    !protoGatherable.SharedCanInteract(CurrentCharacter, interactionQueue[0], false) ||
                    !protoGatherable.SharedIsCanGather(interactionQueue[0])))
            {
                interactionQueue.RemoveAt(0);
            }

            if (interactionQueue.Count == 0)
            {
                return;
            }

            var request = new WorldActionRequest(CurrentCharacter, interactionQueue[0]);
            GatheringSystem.Instance.SharedStartAction(request);
        }

        protected override bool TestObject(IStaticWorldObject staticWorldObject)
        {
            return staticWorldObject.ProtoGameObject is IProtoObjectGatherable protoGatherable &&
                   protoGatherable.SharedIsCanGather(staticWorldObject) &&
                   protoGatherable.SharedCanInteract(CurrentCharacter, staticWorldObject, false);
        }

        /// <summary>
        /// Stop everything.
        /// </summary>
        public override void Stop()
        {
            if (interactionQueue?.Count > 0)
            {
                interactionQueue.Clear();
                InteractionCheckerSystem.CancelCurrentInteraction(CurrentCharacter);
            }
            readyForInteraction = true;
            lastActionState = null;
            openedLootContainer = null;
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

            // Check if there an action in progress.
            if (PrivateState.CurrentActionState != null)
            {
                readyForInteraction = false;
                lastActionState = PrivateState.CurrentActionState;
            }

            // Check if we openned loot container before enabling component.
            var currentInteractionObject = InteractionCheckerSystem.GetCurrentInteraction(CurrentCharacter);
            if (currentInteractionObject != null &&
                currentInteractionObject.ProtoWorldObject is ProtoObjectLootContainer)
            {
                readyForInteraction = false;
                openedLootContainer = currentInteractionObject;
            }
        }

        private void OnActionStateChanged()
        {
            if (PrivateState.CurrentActionState != null)
            {
                // Action was started.
                readyForInteraction = false;
                openedLootContainer = null;
                lastActionState = PrivateState.CurrentActionState;
            }
            else
            {
                // Action is finished.
                // Check if we openned a loot container.
                if (lastActionState != null &&
                    lastActionState.IsCompleted &&
                    !lastActionState.IsCancelled && !lastActionState.IsCancelledByServer &&
                    lastActionState.TargetWorldObject?.ProtoGameObject is ProtoObjectLootContainer)
                {
                    openedLootContainer = lastActionState.TargetWorldObject;
                }
                else
                {
                    readyForInteraction = true;
                }
            }
        }
    }
}
