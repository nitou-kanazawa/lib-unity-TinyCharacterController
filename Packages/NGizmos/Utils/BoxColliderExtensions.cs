using UnityEngine;

namespace Nitou.Gizmo
{
    /// <summary>
    /// <see cref="BoxCollider"/>型の基本的な拡張メソッド集．
    /// </summary>
    internal static class BoxColliderExtensions
    {
        /// <summary>
        /// グローバル座標に変換したコライダー中心座標を取得する拡張メソッド．
        /// </summary>
        public static Vector3 GetWorldCenter(this BoxCollider box)
        {
            return box.transform.TransformPoint(box.center);
        }

        /// <summary>
        /// 親階層を考慮したスケールを取得する拡張メソッド．
        /// </summary>
        public static Vector3 GetScaledSize(this BoxCollider box)
        {
            return Vector3.Scale(box.transform.lossyScale, box.size);
        }
    }
}