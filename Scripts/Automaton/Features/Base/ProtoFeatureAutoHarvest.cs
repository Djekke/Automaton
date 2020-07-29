namespace CryoFall.Automaton.Features
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoFeatureAutoHarvest<T> : ProtoFeature<T>
        where T : class
    {
        private bool attackInProgress = false;

        private IStaticWorldObject currentTargetObject;

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

            if (attackInProgress)
            {
                if (currentTargetObject?.PhysicsBody != null
                    && ValidateTarget(currentTargetObject, out Vector2D targetPoint))
                {
                    MovementManager.RotationTargetPos = targetPoint;
                }
                else
                {
                    StopAttack();
                    FindAndAttackTarget();
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
            using var objectsNearby = CurrentCharacter.PhysicsBody.PhysicsSpace
                                          .TestCircle(position: fromPos,
                                                      radius: GetCurrentWeaponRange(),
                                                      collisionGroup: CollisionGroups.HitboxMelee);
            var objectOfInterest = objectsNearby.AsList()
                                   ?.Where(t => EnabledEntityList.Contains(t.PhysicsBody?.AssociatedWorldObject?.ProtoGameObject))
                                   .ToList();
            if (objectOfInterest == null || objectOfInterest.Count == 0)
            {
                return;
            }

            foreach (var obj in objectOfInterest)
            {
                var testWorldObject = obj.PhysicsBody.AssociatedWorldObject as IStaticWorldObject;
                if (ValidateTarget(testWorldObject, out Vector2D targetPoint))
                {
                    AttackTarget(testWorldObject, targetPoint);
                    return;
                }
            }
        }

        private bool ValidateTarget(IStaticWorldObject targetObject, out Vector2D targetPoint)
        {
            targetPoint = Vector2D.Zero;
            var shape = targetObject.PhysicsBody.Shapes.FirstOrDefault(s => s.CollisionGroup == CollisionGroups.HitboxMelee);
            if (shape == null)
            {
                Api.Logger.Error("Automaton: target object has no HitBoxMelee shape " + targetObject);
                return false;
            }
            targetPoint = ShapeCenter(shape) + targetObject.PhysicsBody.Position;
            return AdditionalValidation(targetObject) && CheckForObstacles(targetObject, targetPoint);
        }

        private void AttackTarget(IStaticWorldObject targetObject, Vector2D intersectionPoint)
        {
            if (targetObject == null)
            {
                return;
            }
            currentTargetObject = targetObject;
            MovementManager.RelativeRotate(intersectionPoint, GetWeaponOffset());
            WeaponSystem.ClientChangeWeaponFiringMode(true);
            attackInProgress = true;
        }

        private void StopAttack()
        {
            if (attackInProgress)
            {
                MovementManager.OverrideRotate = false;
                attackInProgress = false;
                WeaponSystem.ClientChangeWeaponFiringMode(false);
            }
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
            using var obstaclesOnTheWay = CurrentCharacter.PhysicsBody.PhysicsSpace
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

                if (EnabledEntityList.Contains(testWorldObject.ProtoWorldObject))
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

        private void SelectedItemChanged(IItem item)
        {
            if (!CheckPrecondition())
            {
                StopAttack();
            }
        }

        /// <summary>
        /// Stop everything.
        /// </summary>
        public override void Stop()
        {
            StopAttack();

            ClientHotbarSelectedItemManager.SelectedItemChanged -= SelectedItemChanged;
        }

        /// <summary>
        /// Init on component enabled.
        /// </summary>
        public override void Start(ClientComponent parentComponent)
        {
            base.Start(parentComponent);

            ClientHotbarSelectedItemManager.SelectedItemChanged += SelectedItemChanged;
        }
    }
}
