using UnityEngine;

namespace Nitou.AnimationModule
{
    /// <summary>
    /// 2D BlendTreeの方向を表す列挙型．
    /// </summary>
    public enum QuadDirection
    {
        /// <summary>
        /// <see cref="Vector2.up"/>.
        /// </summary>
        Up,

        /// <summary>
        /// <see cref="Vector2.right"/>.
        /// </summary>
        Right,

        /// <summary>
        /// <see cref="Vector2.down"/>.
        /// </summary>
        Down,

        /// <summary>
        /// <see cref="Vector2.left"/>.
        /// </summary>
        Left,
    }


    /// <summary>
    /// BlendTree関連の汎用メソッド集．
    /// </summary>
    public static class BlendTreeUtil
    {
        /// <summary>
        /// <see cref="QuadDirection"/>に対応した<see cref="Vector2"/>の値を返す．
        /// </summary>
        public static Vector2 ToVector2(this QuadDirection direction) =>
            direction switch
            {
                QuadDirection.Up => Vector2.up,
                QuadDirection.Right => Vector2.right,
                QuadDirection.Down => Vector2.down,
                QuadDirection.Left => Vector2.left,
                _ => throw new System.NotImplementedException()
            };

        /// <summary>
        /// Returns the direction closest to the specified `vector`.
        /// </summary>
        public static QuadDirection ToDirection(this Vector2 vector)
        {
            float angle = Vector2.SignedAngle(Vector2.up, vector);

            return angle switch
            {
                >= -45 and < 45 => QuadDirection.Up,
                >= 45 and < 135 => QuadDirection.Right,
                >= -135 and < -45 => QuadDirection.Left,
                _ => QuadDirection.Down,
            };
        }


        /// <summary>
        /// Returns a copy of the `vector` pointing in the closest direction this set type has an animation for.
        /// </summary>
        public static Vector2 SnapVectorToDirection(Vector2 vector)
        {
            var magnitude = vector.magnitude;
            var direction = ToDirection(vector);
            vector = ToVector2(direction) * magnitude;
            return vector;
        }
    }
}