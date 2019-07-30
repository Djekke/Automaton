namespace CryoFall.Automaton.Features
{
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using System.Collections.Generic;
    using System.Linq;

    public class FeatureAutoPickUp: ProtoFeatureWithInteractionQueue
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
                    if (interactionQueue[0].ProtoWorldObject is ObjectGroundItemsContainer)
                    {
                        var containerGround = interactionQueue[0]
                            .GetPublicState<ObjectGroundItemsContainer.PublicState>().ItemsContainer;

                        // try pickup all the items
                        var result = CurrentCharacter.ProtoCharacter.ClientTryTakeAllItems(
                            CurrentCharacter,
                            containerGround,
                            showNotificationIfInventoryFull: true);
                        if (result.MovedItems.Count > 0)
                        {
                            // at least one item taken from ground
                            NotificationSystem.ClientShowItemsNotification(
                                itemsChangedCount: result.MovedItems
                                    .GroupBy(p => p.Key.ProtoItem)
                                    .ToDictionary(p => p.Key, p => p.Sum(v => v.Value)));
                        }
                    }
                    else
                    {
                        interactionQueue[0].ProtoWorldObject.ClientInteractStart(interactionQueue[0]);
                        interactionQueue[0].ProtoWorldObject.ClientInteractFinish(interactionQueue[0]);
                    }
                }
                interactionQueue.RemoveAt(0);
            }
            // Known issue, cannot pickup ground container items while item in hands
            // \Scripts\StaticObjects\ObjectGroundItemsContainer.cs:409
        }
    }
}
