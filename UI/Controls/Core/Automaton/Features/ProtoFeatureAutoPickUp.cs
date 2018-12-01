namespace CryoFall.Automaton.UI.Controls.Core.Automaton.Features
{
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using System.Collections.Generic;

    public class ProtoFeatureAutoPickUp: ProtoFeatureWithInteractionQueue
    {
        public override string Name => "AutoPickUp";

        public override string Description => "PickUp items from ground (including twigs, stones, grass).";

        protected override void PrepareFeature(List<IProtoEntity> entityList, List<IProtoEntity> requiredItemList)
        {
            entityList.AddRange(Api.FindProtoEntities<ProtoObjectLoot>());
            entityList.AddRange(Api.FindProtoEntities<ObjectGroundItemsContainer>());
        }

        protected override void CheckInteractionQueue()
        {
            while (interactionQueue.Count != 0)
            {
                if (!interactionQueue[0].IsDestroyed &&
                    interactionQueue[0].ProtoStaticWorldObject
                        .SharedCanInteract(CurrentCharacter, interactionQueue[0], false))
                {
                    interactionQueue[0].ProtoWorldObject.ClientInteractStart(interactionQueue[0]);
                    interactionQueue[0].ProtoWorldObject.ClientInteractFinish(interactionQueue[0]);
                }
                interactionQueue.RemoveAt(0);
            }
            // Known issue, cannot pickup ground container items while item in hands
            // \Scripts\StaticObjects\ObjectGroundItemsContainer.cs:409
        }
    }
}
