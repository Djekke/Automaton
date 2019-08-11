namespace CryoFall.Automaton.Features
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoFeatureAutoHarvest: ProtoFeature
    {
        private bool targetFound = false;

        private bool attackInProgress = false;

        /// <summary>
        /// Called by client component every tick.
        /// </summary>
        public override void Update(double deltaTime)
        {
            if (!(IsEnabled && CheckPrecondition()))
            {
                Stop();
                return;
            }

            if (targetFound)
            {
                FindAndAttackTarget();
            }
            else
            {
                Stop();
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

            FindAndAttackTarget();
        }

        private void FindAndAttackTarget( )
        {
            var fromPos = CurrentCharacter.Position + GetWeaponOffset();
            using (var objectsNearby = CurrentCharacter.PhysicsBody.PhysicsSpace
                                                       .TestCircle(position: fromPos,
                                                                   radius: GetCurrentWeaponRange(),
                                                                   collisionGroup: CollisionGroups.HitboxMelee))
            {
                if (objectsNearby == null)
                {
                    targetFound = false;
                    return; // do we need this?
                }
                var objectOfInterest = objectsNearby
                    .Where(t => EnabledEntityList.Contains(t.PhysicsBody?.AssociatedWorldObject?.ProtoGameObject))
                    .ToList();
                if (!(objectOfInterest.Count > 0))
                {
                    targetFound = false;
                    return;
                }
                foreach (var obj in objectOfInterest)
                {
                    var testWorldObject = obj.PhysicsBody.AssociatedWorldObject as IStaticWorldObject;
                    if (CheckForObstacles(testWorldObject, fromPos + obj.Penetration))
                    {
                        targetFound = true;
                        AttackTarget(testWorldObject, fromPos + obj.Penetration);
                        attackInProgress = true;
                        return;
                    }
                }
            }
        }

        public void AttackTarget(IWorldObject targetObject, Vector2D intersectionPoint)
        {
            if (targetObject == null)
            {
                return;
            }

            var deltaPositionToMouseCursor = CurrentCharacter.Position +
                                             GetWeaponOffset() -
                                             intersectionPoint;
            var rotationAngleRad =
                Math.Abs(Math.PI + Math.Atan2(deltaPositionToMouseCursor.Y, deltaPositionToMouseCursor.X));
            var moveModes = PlayerCharacter.GetPrivateState(CurrentCharacter).Input.MoveModes;
            // TODO: dont prevent mooving
            var command = new CharacterInputUpdate(moveModes, (float)rotationAngleRad);
            ((PlayerCharacter)CurrentCharacter.ProtoCharacter).ClientSetInput(command);
            // TODO: prevent user mousemove to interrupt it
            SelectedItem.ProtoItem.ClientItemUseStart(SelectedItem);
        }

        private void StopItemUse()
        {
            SelectedItem?.ProtoItem.ClientItemUseFinish(SelectedItem);
        }


        protected virtual double GetCurrentWeaponRange()
        {
            var toolItem = SelectedItem.ProtoItem as IProtoItemWeaponMelee;
            if (toolItem?.OverrideDamageDescription != null)
            {
                return toolItem.OverrideDamageDescription.RangeMax;
            }
            Api.Logger.Error("Automaton: OverrideDamageDescription is null for " + toolItem);
            return 0d;
        }

        protected Vector2D GetWeaponOffset()
        {
            return new Vector2D(0, CurrentCharacter.ProtoCharacter.CharacterWorldWeaponOffsetMelee);
        }

        private bool CheckForObstacles(IWorldObject targetObject, Vector2D intersectionPoint)
        {
            // Check for obstacles in line between character and object
            var fromPos = CurrentCharacter.Position + GetWeaponOffset();
            // Normalize vector and set it length to weapon range
            var toPos = (fromPos - intersectionPoint).Normalized * GetCurrentWeaponRange();
            // Check if in range
            bool canReachObject = false;
            using (var obstaclesOnTheWay = CurrentCharacter.PhysicsBody.PhysicsSpace
                                                           .TestLine(fromPosition: fromPos,
                                                                     toPosition: fromPos - toPos,
                                                                     collisionGroup: CollisionGroups.HitboxMelee))
            {
                foreach (var testResult in obstaclesOnTheWay)
                {
                    var testResultPhysicsBody = testResult.PhysicsBody;
                    var attackedProtoTile = testResultPhysicsBody.AssociatedProtoTile;
                    if (attackedProtoTile != null)
                    {
                        if (attackedProtoTile.Kind != TileKind.Solid)
                        {
                            // non-solid obstacle - skip
                            continue;
                        }
                        // tile on the way - blocking damage ray
                        break;
                    }

                    var testWorldObject = testResultPhysicsBody.AssociatedWorldObject;
                    if (testWorldObject == CurrentCharacter)
                    {
                        // ignore collision with self
                        continue;
                    }

                    if (!(testWorldObject.ProtoGameObject is IDamageableProtoWorldObject))
                    {
                        // shoot through this object
                        continue;
                    }

                    if (testWorldObject == targetObject)
                    {
                        canReachObject = true;
                        continue;
                    }
                    // another object on the way
                    return false;
                }
            }
            return canReachObject;
        }

        /// <summary>
        /// Stop everything.
        /// </summary>
        public override void Stop()
        {
            if (targetFound)
            {
                targetFound = false;
            }
            if (attackInProgress)
            {
                attackInProgress = false;
                StopItemUse();
            }
        }
    }
}
