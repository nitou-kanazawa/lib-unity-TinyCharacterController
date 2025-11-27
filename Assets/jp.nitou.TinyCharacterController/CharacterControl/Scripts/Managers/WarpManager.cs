using UnityEngine;

namespace Nitou.TCC.CharacterControl.Core
{
    internal sealed class WarpManager
    {
        /// <summary>
        /// Warp Manager の現在位置を取得する．
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Warp Manager の現在の回転を取得する．
        /// </summary>
        public Quaternion Rotation { get; private set; }

        /// <summary>
        /// 位置がワープされたかどうかを示す．
        /// </summary>
        public bool WarpedPosition { get; private set; }

        /// <summary>
        /// 回転がワープされたかどうかを示す．
        /// </summary>
        public bool WarpedRotation { get; private set; }

        /// <summary>
        /// Warp Manager が移動中かどうかを示す．
        /// </summary>
        public bool IsMove { get; private set; }


        // ----------------------------------------------------------------------------
        // Public Method

        /// <summary>
        /// Warp Manager の位置を更新する．ワープ移動は IMove より優先される．
        /// </summary>
        /// <param name="position">移動先の新しい位置．</param>
        public void SetPosition(Vector3 position)
        {
            WarpedPosition = true;
            Position = position;
            IsMove = false;
        }

        /// <summary>
        /// Warp Manager の回転を更新する．ワープ移動は ITurn より優先される．
        /// </summary>
        /// <param name="rotation">新しい回転．</param>
        public void SetRotation(Quaternion rotation)
        {
            WarpedRotation = true;
            Rotation = rotation;
            IsMove = false;
        }

        /// <summary>
        /// キャラクターを新しい位置と回転にワープさせる．
        /// </summary>
        /// <param name="position">ワープ先の新しい位置．</param>
        /// <param name="rotation">ワープ先の新しい回転．</param>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            WarpedPosition = true;
            WarpedRotation = true;
            Position = position;
            Rotation = rotation;
            IsMove = false;
        }

        /// <summary>
        /// 障害物を考慮して Warp Manager の位置を更新する．
        /// </summary>
        /// <param name="position">障害物を考慮した新しい位置．</param>
        public void Move(Vector3 position)
        {
            WarpedPosition = true;
            Position = position;
            IsMove = true;
        }

        /// <summary>
        /// ワープ状態（位置、回転、移動状態）をリセットする．
        /// </summary>
        public void ResetWarp()
        {
            WarpedPosition = WarpedRotation = false;
            IsMove = false;
        }
    }
}