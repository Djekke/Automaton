namespace AtomicTorch.CBND.Automaton.ClientComponents.Actions
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ClientComponentAutomaton : ClientComponent
    {
        private static ClientComponentAutomaton instance;

        private static Settings settingsInstance;

        private PlayerCharacterPrivateState privateState;

        private static readonly IClientStorage SessionStorage;

        public static event Action IsAutoPickUpEnabledChanged;

        public static event Action IsAutoGatherEnabledChanged;

        public static event Action IsAutoLootEnabledChanged;

        public static event Action IsAutoWoodcuttingEnabledChanged;

        public static event Action IsAutoMiningEnabledChanged;

        // TODO: Add in options overlay
        public static double UpdateInterval = 0.5d;

        private double accumulatedTime = UpdateInterval;

        private ICharacter playerCharacter;

        private Vector2D weaponOffset;

        private List<IStaticWorldObject> interactionQueue = new List<IStaticWorldObject>();

        private IStaticWorldObject currentlyInteractingWith = null;

        private bool currentlyGathering = false;

        private Target currentTarget;

        private IItem selectedItem = null;

        static ClientComponentAutomaton()
        {
            SessionStorage = Api.Client.Storage.GetSessionStorage(
                nameof(ClientComponentAutomaton) + ".Settings");
            SessionStorage.RegisterType(typeof(Settings));
            if (SessionStorage.TryLoad(out settingsInstance))
            {
                IsAutoPickUpEnabled = settingsInstance.isAutoPickUpEnabled;
                IsAutoGatherEnabled = settingsInstance.isAutoGatherEnabled;
                IsAutoWoodcuttingEnabled = settingsInstance.isAutoWoodcuttingEnabled;
                IsAutoMiningEnabled = settingsInstance.isAutoMiningEnabled;
            }
            else
            {
                // Auto-actions are disabled by default
                settingsInstance.isAutoPickUpEnabled = IsAutoPickUpEnabled = false;
                settingsInstance.isAutoGatherEnabled = IsAutoGatherEnabled = false;
                settingsInstance.isAutoWoodcuttingEnabled = IsAutoWoodcuttingEnabled = false;
                settingsInstance.isAutoMiningEnabled = IsAutoMiningEnabled = false;
            }
        }

        public static bool IsAutoPickUpEnabled
        {
            get => settingsInstance.isAutoPickUpEnabled;
            set
            {
                if (IsAutoPickUpEnabled == value)
                {
                    return;
                }
                settingsInstance.isAutoPickUpEnabled = value;
                SessionStorage.Save(settingsInstance);

                IsAutoPickUpEnabledChanged?.Invoke();
            }
        }

        public static bool IsAutoGatherEnabled
        {
            get => settingsInstance.isAutoGatherEnabled;
            set
            {
                if (IsAutoGatherEnabled == value)
                {
                    return;
                }
                settingsInstance.isAutoGatherEnabled = value;
                SessionStorage.Save(settingsInstance);

                IsAutoGatherEnabledChanged?.Invoke();
            }
        }

        public static bool IsAutoLootEnabled
        {
            get => settingsInstance.isAutoLootEnabled;
            set
            {
                if (IsAutoLootEnabled == value)
                {
                    return;
                }
                settingsInstance.isAutoLootEnabled = value;
                SessionStorage.Save(settingsInstance);

                IsAutoLootEnabledChanged?.Invoke();
            }
        }

        public static bool IsAutoWoodcuttingEnabled
        {
            get => settingsInstance.isAutoWoodcuttingEnabled;
            set
            {
                if (IsAutoWoodcuttingEnabled == value)
                {
                    return;
                }
                settingsInstance.isAutoWoodcuttingEnabled = value;
                SessionStorage.Save(settingsInstance);

                IsAutoWoodcuttingEnabledChanged?.Invoke();
            }
        }

        public static bool IsAutoMiningEnabled
        {
            get => settingsInstance.isAutoMiningEnabled;
            set
            {
                if (IsAutoMiningEnabled == value)
                {
                    return;
                }
                settingsInstance.isAutoMiningEnabled = value;
                SessionStorage.Save(settingsInstance);

                IsAutoMiningEnabledChanged?.Invoke();
            }
        }

        public static bool IsAutoHarvestEnabled => IsAutoWoodcuttingEnabled || IsAutoMiningEnabled;

        public static bool IsAutomatonEnabled => IsAutoPickUpEnabled || IsAutoGatherEnabled ||
                    IsAutoLootEnabled || IsAutoWoodcuttingEnabled || IsAutoMiningEnabled;

        protected override void OnDisable()
        {
            if (ReferenceEquals(this, instance))
            {
                instance = null;
            }
        }

        protected override void OnEnable()
        {
            instance = this;
            this.playerCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            this.privateState = PlayerCharacter.GetPrivateState(this.playerCharacter);
            weaponOffset = new Vector2D(0, playerCharacter.ProtoCharacter.CharacterWorldWeaponOffset);
            currentTarget.targetObject = null;
            currentTarget.intersectionPoint = null;
            currentTarget.checkedForObstacles = false;
        }

        public override void Update(double deltaTime)
        {
            CheckInteractionQueue();

            this.accumulatedTime += deltaTime;
            if (this.accumulatedTime < UpdateInterval)
            {
                return;
            }
            this.accumulatedTime %= UpdateInterval;

            if (IsAutomatonEnabled)
            {
                if (IsAutoPickUpEnabled)
                {
                    AutoPickUp();
                }
                if (IsAutoGatherEnabled)
                {
                    if (!currentlyGathering && !(this.privateState.CurrentActionState is IActionState))
                    {
                        AutoGather();
                    }
                }
                if (IsAutoHarvestEnabled)
                {
                    FindAndAttackHarvestableObject();
                }
            }
        }

        private void AutoPickUp()
        {
            using (var objectsInCharacterInteractionArea = InteractionCheckerSystem.
                    SharedGetTempObjectsInCharacterInteractionArea(playerCharacter))
            {
                if (objectsInCharacterInteractionArea == null)
                {
                    return;
                }
                foreach (var testResult in objectsInCharacterInteractionArea)
                {
                    if (!(testResult.PhysicsBody?.AssociatedWorldObject is IStaticWorldObject staticWorldObject))
                    {
                        continue;
                    }
                    if (staticWorldObject == playerCharacter)
                    {
                        continue;
                    }
                    if (!IsAutoLootEnabled && staticWorldObject.ProtoGameObject is ProtoObjectLootContainer)
                    {
                        continue;
                    }
                    if ((staticWorldObject.ProtoGameObject is IProtoObjectLoot ||
                        staticWorldObject.ProtoGameObject is ObjectGroundItemsContainer) &&
                        staticWorldObject.ProtoStaticWorldObject.SharedCanInteract(playerCharacter, staticWorldObject, false))
                    {
                        if (!interactionQueue.Contains(staticWorldObject))
                        {
                            interactionQueue.Add(staticWorldObject);
                        }
                    }
                }
            }
        }

        private void CheckInteractionQueue()
        {
            if (interactionQueue.Count == 0)
            {
                return;
            }
            var staticWorldObject = interactionQueue.FirstOrDefault();
            
            if (staticWorldObject.IsDestroyed ||
                 !(staticWorldObject.ProtoStaticWorldObject
                   .SharedCanInteract(playerCharacter, staticWorldObject, false)))
            {
                interactionQueue.RemoveAt(0);
                currentlyInteractingWith = null;
            }
            else
            {
                if (currentlyInteractingWith == null)
                {
                    currentlyInteractingWith = staticWorldObject;
                    currentlyInteractingWith.ProtoWorldObject.ClientInteractStart(currentlyInteractingWith);
                    currentlyInteractingWith.ProtoWorldObject.ClientInteractFinish(currentlyInteractingWith);
                }
            }
        }

        private void AutoGather()
        {
            using (var objectsInCharacterInteractionArea = InteractionCheckerSystem
                    .SharedGetTempObjectsInCharacterInteractionArea(playerCharacter))
            {
                if (objectsInCharacterInteractionArea == null)
                {
                    return;
                }
                foreach (var testResult in objectsInCharacterInteractionArea)
                {
                    if (!(testResult.PhysicsBody?.AssociatedWorldObject is IStaticWorldObject staticWorldObject))
                    {
                        continue;
                    }
                    if (staticWorldObject == playerCharacter)
                    {
                        continue;
                    }
                    if (staticWorldObject.ProtoGameObject is IProtoObjectGatherableVegetation protoGatherable &&
                        protoGatherable.SharedCanInteract(playerCharacter, staticWorldObject, false))
                    {
                        currentlyGathering = true;
                        var request = new WorldActionRequest(playerCharacter, staticWorldObject);
                        GatheringSystem.Instance.SharedStartAction(request);
                        ClientComponentTimersManager.AddAction(
                            protoGatherable.DurationGatheringSeconds + Client.CurrentGame.PingGame * 2,
                            () => currentlyGathering = false);
                        return;
                    }
                }
            }
        }

        private void FindAndAttackHarvestableObject()
        {
            selectedItem = ClientHotbarSelectedItemManager.SelectedItem;
            // if can not start search or attack : return
            // check, that i have proper item for proper mode
            if (selectedItem == null ||
                !((IsAutoWoodcuttingEnabled && selectedItem.ProtoItem is IProtoItemToolWoodcutting) ||
                (IsAutoMiningEnabled && selectedItem.ProtoItem is IProtoItemToolMining)))
            {
                return;
            }
            // if no object found or object's physical body already destroyed
            // or if current object out of range
            // search for new one
            FindAndCheckIntersectionPoint();
            if (currentTarget.targetObject?.PhysicsBody == null ||
                !currentTarget.intersectionPoint.HasValue)
            {
                FindNextObjectToAttack();
            }
            // check if object valid
            // check if weapon valid for this object
            // check if no obstacles
            if (currentTarget.targetObject?.PhysicsBody != null &&
                IsAppropriateObject(currentTarget.targetObject) &&
                currentTarget.checkedForObstacles)
            {
                // attack
                AttackTarget();
                // add timer for next attack
                ClientComponentTimersManager.AddAction(
                    ((IProtoItemWeapon)selectedItem.ProtoItem).FireInterval,
                    () => FindAndAttackHarvestableObject());
            }
        }

        private void StopItemUse()
        {
            if (selectedItem != null)
            {
                selectedItem.ProtoItem.ClientItemUseFinish(selectedItem);
            }
        }

        private void FindNextObjectToAttack()
        {
            currentTarget.targetObject = null;
            var fromPos = playerCharacter.Position + weaponOffset;
            using (var objectsNearby = playerCharacter.PhysicsBody.PhysicsSpace
                    .TestCircle(position: fromPos,
                                radius: GetCurrentWeaponRange(),
                                collisionGroup: CollisionGroups.HitboxMelee))
            {
                if (objectsNearby == null)
                {
                    return; // do we need this?
                }
                foreach (var obj in objectsNearby)
                {
                    if (obj.PhysicsBody?.AssociatedWorldObject == null)
                    {
                        continue;
                    }
                    var testWorldObject = obj.PhysicsBody.AssociatedWorldObject;
                    if (!IsAppropriateObject(testWorldObject))
                    {
                        continue;
                    }
                    if (NoObstaclesBetween(testWorldObject, fromPos + obj.Penetration))
                    {
                        currentTarget.targetObject = testWorldObject;
                        currentTarget.intersectionPoint = fromPos + obj.Penetration;
                        currentTarget.checkedForObstacles = true;
                    }
                }
            }
        }

        public void AttackTarget()
        {
            if (currentTarget.targetObject != null)
            {
                var deltaPositionToMouseCursor = playerCharacter.Position + weaponOffset -
                                        (currentTarget.intersectionPoint);
                var rotationAngleRad = Math.Abs(Math.PI
                                    + Math.Atan2(deltaPositionToMouseCursor.Value.Y,
                                                 deltaPositionToMouseCursor.Value.X));
                CharacterMoveModes moveModes = this.privateState.Input.MoveModes;
                // TODO: dont prevent mooving
                var command = new CharacterInputUpdate(moveModes, (float)rotationAngleRad);
                ((PlayerCharacter)playerCharacter.ProtoCharacter).ClientSetInput(command);
                // TODO: prevent user mousemove to interrupt it
                selectedItem.ProtoItem.ClientItemUseStart(selectedItem);
                ClientComponentTimersManager.AddAction(
                    ((IProtoItemWeapon)selectedItem.ProtoItem).DamageApplyDelay,
                    () => StopItemUse());
            }
        }

        private bool IsAppropriateObject(IWorldObject worldObject)
        {
            if ((IsAutoWoodcuttingEnabled &&
                worldObject.ProtoGameObject is IProtoObjectTree &&
                selectedItem.ProtoItem is IProtoItemToolWoodcutting) ||
                (IsAutoMiningEnabled &&
                worldObject.ProtoGameObject is IProtoObjectMineral &&
                selectedItem.ProtoItem is IProtoItemToolMining))
            {
                return true;
            }
            return false;
        }

        private double GetCurrentWeaponRange()
        {
            var toolItem = selectedItem.ProtoItem as IProtoItemWeaponMelee;
            if (toolItem.OverrideDamageDescription != null)
            { // TODO: investigate this and rewrite it
                return toolItem.OverrideDamageDescription.RangeMax;
            }
            Api.Logger.Error("OverrideDamageDescription is null for " + toolItem);
            return 0d;
        }

        private bool NoObstaclesBetween(IWorldObject targetObject, Vector2D intersectionPoint)
        {
            // Check for obstacles in line between character and object
            var fromPos = playerCharacter.Position + weaponOffset;
            // Normalize vector and set it length to weapon range
            var toPos = (fromPos - intersectionPoint).Normalized * GetCurrentWeaponRange();
            // Check if in range
            bool canReachObject = false;
            using (var obstaclesOnTheWay = playerCharacter.PhysicsBody.PhysicsSpace.TestLine(
                fromPosition: fromPos,
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
                    if (testWorldObject == playerCharacter)
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

        private void FindAndCheckIntersectionPoint()
        {
            currentTarget.intersectionPoint = null;
            currentTarget.checkedForObstacles = false;
            if (currentTarget.targetObject?.PhysicsBody == null)
            {
                return;
            }
            var centerOffset = GetMeleeCenterOffset(currentTarget.targetObject);
            var point = centerOffset + currentTarget.targetObject.PhysicsBody.Position;
            if (NoObstaclesBetween(currentTarget.targetObject, point))
            {
                currentTarget.intersectionPoint = point;
                currentTarget.checkedForObstacles = true;
            }
            
        }

        private Vector2D GetMeleeCenterOffset(IWorldObject worldObject)
        {
            var testShape = worldObject.PhysicsBody.Shapes.FirstOrDefault(e =>
                    e.CollisionGroup == CollisionGroups.HitboxMelee);
            if (testShape.CollisionGroup == CollisionGroups.HitboxMelee &&
                testShape.ShapeType == ShapeType.Rectangle &&
                testShape is RectangleShape meleeShape)
            {
                return (meleeShape.Size / 2 + meleeShape.Position);
            }
            return worldObject.PhysicsBody.CenterOffset;
        }

        private struct Settings
        {
            public bool isAutoPickUpEnabled;
            public bool isAutoGatherEnabled;
            public bool isAutoLootEnabled;
            public bool isAutoWoodcuttingEnabled;
            public bool isAutoMiningEnabled;
        }

        private struct Target
        {
            public IWorldObject targetObject;
            public Vector2D? intersectionPoint;
            public bool checkedForObstacles;
        }
    }
}