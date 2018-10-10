namespace CryoFall.Automaton.ClientComponents.Actions
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

        private static readonly IClientStorage Storage;

        private static bool PendingSettingsChanges = false;

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
            Storage = Api.Client.Storage.GetStorage("Mods/Automaton.Settings");
            Storage.RegisterType(typeof(Settings));
            if (Storage.TryLoad(out settingsInstance))
            {
                IsAutoPickUpEnabled = settingsInstance.IsAutoPickUpEnabled;
                IsAutoGatherEnabled = settingsInstance.IsAutoGatherEnabled;
                IsAutoWoodcuttingEnabled = settingsInstance.IsAutoWoodcuttingEnabled;
                IsAutoMiningEnabled = settingsInstance.IsAutoMiningEnabled;
            }
            else
            {
                // Auto-actions are disabled by default
                settingsInstance.IsAutoPickUpEnabled = IsAutoPickUpEnabled = false;
                settingsInstance.IsAutoGatherEnabled = IsAutoGatherEnabled = false;
                settingsInstance.IsAutoWoodcuttingEnabled = IsAutoWoodcuttingEnabled = false;
                settingsInstance.IsAutoMiningEnabled = IsAutoMiningEnabled = false;
            }
        }

        public static bool IsAutoPickUpEnabled
        {
            get => settingsInstance.IsAutoPickUpEnabled;
            set
            {
                if (IsAutoPickUpEnabled == value)
                {
                    return;
                }
                settingsInstance.IsAutoPickUpEnabled = value;
                PendingSettingsChanges = true;

                IsAutoPickUpEnabledChanged?.Invoke();
            }
        }

        public static bool IsAutoGatherEnabled
        {
            get => settingsInstance.IsAutoGatherEnabled;
            set
            {
                if (IsAutoGatherEnabled == value)
                {
                    return;
                }
                settingsInstance.IsAutoGatherEnabled = value;
                PendingSettingsChanges = true;

                IsAutoGatherEnabledChanged?.Invoke();
            }
        }

        public static bool IsAutoLootEnabled
        {
            get => settingsInstance.IsAutoLootEnabled;
            set
            {
                if (IsAutoLootEnabled == value)
                {
                    return;
                }
                settingsInstance.IsAutoLootEnabled = value;
                PendingSettingsChanges = true;

                IsAutoLootEnabledChanged?.Invoke();
            }
        }

        public static bool IsAutoWoodcuttingEnabled
        {
            get => settingsInstance.IsAutoWoodcuttingEnabled;
            set
            {
                if (IsAutoWoodcuttingEnabled == value)
                {
                    return;
                }
                settingsInstance.IsAutoWoodcuttingEnabled = value;
                PendingSettingsChanges = true;

                IsAutoWoodcuttingEnabledChanged?.Invoke();
            }
        }

        public static bool IsAutoMiningEnabled
        {
            get => settingsInstance.IsAutoMiningEnabled;
            set
            {
                if (IsAutoMiningEnabled == value)
                {
                    return;
                }
                settingsInstance.IsAutoMiningEnabled = value;
                PendingSettingsChanges = true;

                IsAutoMiningEnabledChanged?.Invoke();
            }
        }

        public static bool IsAutoHarvestEnabled => IsAutoWoodcuttingEnabled || IsAutoMiningEnabled;

        public static bool IsAutomatonEnabled => IsAutoPickUpEnabled || IsAutoGatherEnabled ||
                    IsAutoLootEnabled || IsAutoWoodcuttingEnabled || IsAutoMiningEnabled;

        public static void SaveSettings()
        {
            if (PendingSettingsChanges)
            {
                Storage.Save(settingsInstance);
                PendingSettingsChanges = false;
            }
        }

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
            playerCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            privateState = PlayerCharacter.GetPrivateState(playerCharacter);
            weaponOffset = new Vector2D(0, playerCharacter.ProtoCharacter.CharacterWorldWeaponOffset);
            currentTarget.TargetObject = null;
            currentTarget.IntersectionPoint = null;
            currentTarget.CheckedForObstacles = false;
        }

        public override void Update(double deltaTime)
        {
            CheckInteractionQueue();

            accumulatedTime += deltaTime;
            if (accumulatedTime < UpdateInterval)
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
                    if (!currentlyGathering && !(privateState.CurrentActionState is IActionState))
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
                            protoGatherable.DurationGatheringSeconds + Client.CurrentGame.PingGameSeconds * 2,
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
            if (currentTarget.TargetObject?.PhysicsBody == null ||
                !currentTarget.IntersectionPoint.HasValue)
            {
                FindNextObjectToAttack();
            }
            // check if object valid
            // check if weapon valid for this object
            // check if no obstacles
            if (currentTarget.TargetObject?.PhysicsBody != null &&
                IsAppropriateObject(currentTarget.TargetObject) &&
                currentTarget.CheckedForObstacles)
            {
                // attack
                AttackTarget();
                // add timer for next attack
                ClientComponentTimersManager.AddAction(
                    ((IProtoItemWeapon)selectedItem.ProtoItem).FireInterval, FindAndAttackHarvestableObject);
            }
        }

        private void StopItemUse()
        {
            selectedItem?.ProtoItem.ClientItemUseFinish(selectedItem);
        }

        private void FindNextObjectToAttack()
        {
            currentTarget.TargetObject = null;
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
                        currentTarget.TargetObject = testWorldObject;
                        currentTarget.IntersectionPoint = fromPos + obj.Penetration;
                        currentTarget.CheckedForObstacles = true;
                    }
                }
            }
        }

        public void AttackTarget()
        {
            if (currentTarget.TargetObject != null)
            {
                var deltaPositionToMouseCursor = playerCharacter.Position + weaponOffset -
                                        (currentTarget.IntersectionPoint);
                var rotationAngleRad = Math.Abs(Math.PI
                                    + Math.Atan2(deltaPositionToMouseCursor.Value.Y,
                                                 deltaPositionToMouseCursor.Value.X));
                CharacterMoveModes moveModes = privateState.Input.MoveModes;
                // TODO: dont prevent mooving
                var command = new CharacterInputUpdate(moveModes, (float)rotationAngleRad);
                ((PlayerCharacter)playerCharacter.ProtoCharacter).ClientSetInput(command);
                // TODO: prevent user mousemove to interrupt it
                selectedItem.ProtoItem.ClientItemUseStart(selectedItem);
                ClientComponentTimersManager.AddAction(
                    ((IProtoItemWeapon)selectedItem.ProtoItem).DamageApplyDelay, StopItemUse);
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
            if (toolItem?.OverrideDamageDescription != null)
            { // TODO: investigate this and rewrite it
                return toolItem.OverrideDamageDescription.RangeMax;
            }
            Api.Logger.Error("Automaton: OverrideDamageDescription is null for " + toolItem);
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
            currentTarget.IntersectionPoint = null;
            currentTarget.CheckedForObstacles = false;
            if (currentTarget.TargetObject?.PhysicsBody == null)
            {
                return;
            }
            var centerOffset = GetMeleeCenterOffset(currentTarget.TargetObject);
            var point = centerOffset + currentTarget.TargetObject.PhysicsBody.Position;
            if (NoObstaclesBetween(currentTarget.TargetObject, point))
            {
                currentTarget.IntersectionPoint = point;
                currentTarget.CheckedForObstacles = true;
            }
        }

        private Vector2D GetMeleeCenterOffset(IWorldObject worldObject)
        {
            var testShape = worldObject.PhysicsBody.Shapes.FirstOrDefault(e =>
                    e.CollisionGroup == CollisionGroups.HitboxMelee);
            if (testShape?.CollisionGroup == CollisionGroups.HitboxMelee &&
                testShape?.ShapeType == ShapeType.Rectangle &&
                testShape is RectangleShape meleeShape)
            {
                return (meleeShape.Size / 2 + meleeShape.Position);
            }
            return worldObject.PhysicsBody.CenterOffset;
        }

        private struct Settings
        {
            public bool IsAutoPickUpEnabled;
            public bool IsAutoGatherEnabled;
            public bool IsAutoLootEnabled;
            public bool IsAutoWoodcuttingEnabled;
            public bool IsAutoMiningEnabled;
        }

        private struct Target
        {
            public IWorldObject TargetObject;
            public Vector2D? IntersectionPoint;
            public bool CheckedForObstacles;
        }
    }
}