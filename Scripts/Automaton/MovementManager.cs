namespace CryoFall.Automaton
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class MovementManager
    {
        public static bool OverrideRotate = false;

        public static bool OverrideMovement = false;

        public static Vector2D RotationTargetPos;

        public static Vector2D RotationOffset;

        private static ICharacter CurrentCharacter => Api.Client.Characters.CurrentPlayerCharacter;

        public static void RelativeRotate(Vector2D toPosition, Vector2D offset)
        {
            RotationTargetPos = toPosition;
            RotationOffset = offset;
            OverrideRotate = true;
        }

        public static float GetRotationAngleRad()
        {
            var deltaPositionToTarget = CurrentCharacter.Position + RotationOffset - RotationTargetPos;
            return (float)Math.Abs(Math.PI + Math.Atan2(deltaPositionToTarget.Y, deltaPositionToTarget.X));
        }
    }
}
