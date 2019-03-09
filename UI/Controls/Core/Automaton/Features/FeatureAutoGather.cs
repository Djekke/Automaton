namespace CryoFall.Automaton.UI.Controls.Core.Automaton.Features
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using System.Collections.Generic;

    public class FeatureAutoGather: ProtoFeatureWithInteractionQueue
    {
        public override string Name => "AutoGather";

        public override string Description => "Gather berry, herbs and other vegetation, harvest corpses, loot radtowns.";

        private ProtoObjectLootContainer openedLootContainer = null;

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
                if (!openedLootContainer.SharedCanInteract(CurrentCharacter, interactionQueue[0], false))
                {
                    openedLootContainer = null;
                } else if (interactionQueue[0].ClientHasPrivateState)
                {
                    // Take all items from container.
                    var q = lastActionState.TargetWorldObject.GetPrivateState<LootContainerPrivateState>();
                    CurrentCharacter.ProtoCharacter.ClientTryTakeAllItems(CurrentCharacter, q.ItemsContainer, true);
                    InteractionCheckerSystem.CancelCurrentInteraction(CurrentCharacter);
                    openedLootContainer = null;
                }
                else
                {
                    return;
                }
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
                lastActionState = null;
                readyForInteraction = true;
                openedLootContainer = null;
            }
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

        private void OnActionStateChanged()
        {
            if (PrivateState.CurrentActionState != null)
            {
                readyForInteraction = false;
                openedLootContainer = null;
                lastActionState = PrivateState.CurrentActionState;
            }
            else
            {
                if (lastActionState.IsCompleted &&
                    !lastActionState.IsCancelled && !lastActionState.IsCancelledByServer &&
                    lastActionState.TargetWorldObject.ProtoGameObject is ProtoObjectLootContainer lootContainer)
                {
                    openedLootContainer = lootContainer;
                }
                readyForInteraction = true;
            }
        }
    }
}
