namespace CryoFall.Automaton.Features
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoFeatureAutoHarvest: ProtoFeature
    {
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

            if (!attackInProgress)
            {
                FindAndAttackTarget();
            }
        }

        protected virtual bool AdditionalValidation(IStaticWorldObject testWorldObject)
        {
            return true;
        }

        private void FindAndAttackTarget( )
        {
            var fromPos = CurrentCharacter.Position + GetWeaponOffset();
            using var objectsNearby = this.CurrentCharacter.PhysicsBody.PhysicsSpace
                                          .TestCircle(position: fromPos,
                                                      radius: this.GetCurrentWeaponRange(),
                                                      collisionGroup: CollisionGroups.HitboxMelee);
            var objectOfInterest = objectsNearby.AsList()
                                   ?.Where(t => this.EnabledEntityList.Contains(t.PhysicsBody?.AssociatedWorldObject?.ProtoGameObject))
                                   .ToList();
            if (objectOfInterest == null || objectOfInterest.Count == 0)
            {
                return;
            }

            foreach (var obj in objectOfInterest)
            {
                var testWorldObject = obj.PhysicsBody.AssociatedWorldObject as IStaticWorldObject;
                var shape = obj.PhysicsBody.Shapes.FirstOrDefault(s =>
                                                                      s.CollisionGroup == CollisionGroups.HitboxMelee);
                if (shape == null)
                {
                    Api.Logger.Error("Automaton: target object has no HitBoxMelee shape " + testWorldObject);
                    continue;
                }
                if(!this.AdditionalValidation(testWorldObject))
                {
                    continue;
                }
                var targetPoint = this.ShapeCenter(shape) + obj.PhysicsBody.Position;
                if (this.CheckForObstacles(testWorldObject, targetPoint))
                {
                    this.AttackTarget(testWorldObject, targetPoint);
                    this.attackInProgress = true;
                    ClientTimersSystem.AddAction(this.GetCurrentWeaponAttackDelay(), () =>
                                                                                     {
                                                                                         if (this.attackInProgress)
                                                                                         {
                                                                                             this.attackInProgress = false;
                                                                                             this.StopItemUse();
                                                                                             this.FindAndAttackTarget();
                                                                                         }
                                                                                     });
                    return;
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
            // TODO: don't prevent moving
            var command = new CharacterInputUpdate(moveModes, (float)rotationAngleRad);
            ((PlayerCharacter)CurrentCharacter.ProtoCharacter).ClientSetInput(command);
            // TODO: prevent user mousemove to interrupt it
            SelectedItem.ProtoItem.ClientItemUseStart(SelectedItem);
        }

        protected virtual double GetCurrentWeaponRange()
        {
            if(SelectedItem.ProtoItem is IProtoItemWeaponMelee toolItem &&
               toolItem.OverrideDamageDescription != null)
            {
                return toolItem.OverrideDamageDescription.RangeMax;
            }
            Api.Logger.Error("Automaton: OverrideDamageDescription is null for " + SelectedItem);
            return 0d;
        }

        protected Vector2D GetWeaponOffset()
        {
            return new Vector2D(0, CurrentCharacter.ProtoCharacter.CharacterWorldWeaponOffsetMelee);
        }

        protected double GetCurrentWeaponAttackDelay()
        {
            var toolItem = SelectedItem.ProtoItem as IProtoItemWeaponMelee;
            return toolItem?.FireInterval ?? 0d;
        }

        private bool CheckForObstacles(IWorldObject targetObject, Vector2D intersectionPoint)
        {
            // Check for obstacles in line between character and object
            var fromPos = CurrentCharacter.Position + GetWeaponOffset();
            // Normalize vector and set it length to weapon range
            var toPos = (fromPos - intersectionPoint).Normalized * GetCurrentWeaponRange();
            // Check if in range
            bool canReachObject = false;
            using var obstaclesOnTheWay = this.CurrentCharacter.PhysicsBody.PhysicsSpace
                                              .TestLine(fromPosition: fromPos,
                                                        toPosition: fromPos - toPos,
                                                        collisionGroup: CollisionGroups.HitboxMelee);
            foreach (var testResult in obstaclesOnTheWay.AsList())
            {
                var testResultPhysicsBody = testResult.PhysicsBody;
                if (testResultPhysicsBody.AssociatedProtoTile != null)
                {
                    if (testResultPhysicsBody.AssociatedProtoTile.Kind != TileKind.Solid)
                    {
                        // non-solid obstacle - skip
                        continue;
                    }
                    // tile on the way - blocking damage ray
                    break;
                }

                var testWorldObject = testResultPhysicsBody.AssociatedWorldObject;
                if (testWorldObject == this.CurrentCharacter)
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

                if (this.EnabledEntityList.Contains(testWorldObject.ProtoWorldObject))
                {
                    // Another object to harvest in line - fire it anyway
                    continue;
                }
                // another object on the way
                return false;
            }

            return canReachObject;
        }

        private Vector2D ShapeCenter(IPhysicsShape shape)
        {
            if (shape != null)
            {
                switch (shape.ShapeType)
                {
                    case ShapeType.Rectangle:
                        var shapeRectangle = (RectangleShape)shape;
                        return shapeRectangle.Position + shapeRectangle.Size / 2d;
                    case ShapeType.Point:
                        var shapePoint = (PointShape)shape;
                        return shapePoint.Point;
                    case ShapeType.Circle:
                        var shapeCircle = (CircleShape)shape;
                        return shapeCircle.Center;
                    case ShapeType.Line:
                        break;
                    case ShapeType.LineSegment:
                        var lineSegmentShape = (LineSegmentShape)shape;
                        return new Vector2D((lineSegmentShape.Point1.X + lineSegmentShape.Point2.X) / 2d,
                                     (lineSegmentShape.Point1.Y + lineSegmentShape.Point2.Y) / 2d);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return new Vector2D(0, 0);
        }

        private void StopItemUse()
        {
            SelectedItem?.ProtoItem.ClientItemUseFinish(SelectedItem);
        }

        /// <summary>
        /// Stop everything.
        /// </summary>
        public override void Stop()
        {
            if (attackInProgress)
            {
                attackInProgress = false;
                StopItemUse();
            }
        }
    }
}
