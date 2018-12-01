namespace CryoFall.Automaton.UI.Controls.Core.Automaton.Features
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using System.Collections.Generic;

    public class ProtoFeatureAutoGather: ProtoFeatureWithInteractionQueue
    {
        public override string Name => "AutoGather";

        public override string Description => "Gather berry, herbs and other vegetation, harvest corpses, loot radtowns.";

        private IStaticWorldObject lastInteractedObject = null;

        protected override void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList)
        {
            entityList.AddRange(Api.FindProtoEntities<IProtoObjectGatherable>());
        }

        protected override void CheckInteractionQueue()
        {
            while (interactionQueue.Count != 0 &&
                   EnabledEntityList.Contains(interactionQueue[0].ProtoGameObject) &&
                   (interactionQueue[0].IsDestroyed ||
                    !(interactionQueue[0].ProtoGameObject is IProtoObjectGatherable protoGatherable) ||
                    !protoGatherable.SharedCanInteract(CurrentCharacter, interactionQueue[0], false) ||
                    !protoGatherable.SharedIsCanGather(interactionQueue[0])))
            {
                if (lastInteractedObject == interactionQueue[0])
                {
                    lastInteractedObject = null;
                }
                interactionQueue.RemoveAt(0);
            }

            if (interactionQueue.Count == 0)
            {
                return;
            }

            var currentInteraction = InteractionCheckerSystem.GetCurrentInteraction(CurrentCharacter);
            if (currentInteraction != null &&
                currentInteraction.ProtoGameObject is ProtoObjectLootContainer &&
                currentInteraction.ClientHasPrivateState)
            {
                // Force cancel interaction with lootContainer
                InteractionCheckerSystem.CancelCurrentInteraction(CurrentCharacter);
            }

            if (lastInteractedObject == null)
            {
                lastInteractedObject = interactionQueue[0];
                var request = new WorldActionRequest(CurrentCharacter, interactionQueue[0]);
                GatheringSystem.Instance.SharedStartAction(request);
            }
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
                lastInteractedObject = null;
            }
        }
    }
}
