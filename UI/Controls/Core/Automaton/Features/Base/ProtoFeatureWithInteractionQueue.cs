namespace CryoFall.Automaton.UI.Controls.Core.Automaton.Features
{
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.GameApi.Data.World;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class ProtoFeatureWithInteractionQueue: ProtoFeature
    {
        protected List<IStaticWorldObject> interactionQueue = new List<IStaticWorldObject>();

        /// <summary>
        /// Called by client component every tick.
        /// </summary>
        public override void Update(double deltaTime)
        {
            if (!(IsEnabled && CheckPrecondition()))
            {
                return;
            }
            CheckInteractionQueue();
        }

        protected abstract void CheckInteractionQueue();

        /// <summary>
        /// Called by client component on specific time interval.
        /// </summary>
        public override void Execute()
        {
            if (!(IsEnabled && CheckPrecondition()))
            {
                return;
            }
            FillInteractionQueue();
        }

        protected virtual void FillInteractionQueue()
        {
            using (var objectsInCharacterInteractionArea = InteractionCheckerSystem
                .SharedGetTempObjectsInCharacterInteractionArea(CurrentCharacter))
            {
                if (objectsInCharacterInteractionArea == null)
                {
                    return;
                }
                var objectOfInterest = objectsInCharacterInteractionArea
                    .Where(t => EnabledEntityList.Contains(t.PhysicsBody?.AssociatedWorldObject?.ProtoGameObject))
                    .ToList();
                if (!(objectOfInterest?.Count > 0))
                {
                    return;
                }
                foreach (var obj in objectOfInterest)
                {
                    var testObject = obj.PhysicsBody.AssociatedWorldObject as IStaticWorldObject;
                    if (TestObject(testObject))
                    {
                        if (!interactionQueue.Contains(testObject))
                        {
                            interactionQueue.Add(testObject);
                        }
                    }
                }
            }
        }

        protected virtual bool TestObject(IStaticWorldObject staticWorldObject)
        {
            return staticWorldObject.ProtoStaticWorldObject
                .SharedCanInteract(CurrentCharacter, staticWorldObject, false);
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
        }
    }
}
