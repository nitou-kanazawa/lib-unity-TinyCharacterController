using System.Collections.Generic;
using UnityEngine;

namespace Nitou.Gizmo
{
    public static class SphereColliderExtensions
    {
        /// <summary>
        /// グローバル座標に変換したコライダー中心座標を取得する拡張メソッド．
        /// </summary>
        public static Vector3 GetWorldCenter(this SphereCollider self)
        {
            return self.transform.TransformPoint(self.center);
        }

        /// <summary>
        /// 親階層を考慮した半径を取得する拡張メソッド．
        /// </summary>
        public static float GetScaledRadius(this SphereCollider sphere)
        {
            // (※Sphereコライダーは常に球形を維持して，半径に各軸の最大スケールが適用される)
            var scale = sphere.transform.localScale;
            return sphere.radius * Mathf.Max(scale.x, scale.y, scale.z);
        }


        /// ----------------------------------------------------------------------------
        /// <summary>
        /// 指定座標が<see cref="SphereCollider"/>の内部に含まれるか判定する拡張メソッド．
        /// </summary>
        public static bool Contains(this SphereCollider sphere, Vector3 point)
        {
            var localPoint = sphere.transform.InverseTransformPoint(point);
            var scaledRadius = sphere.GetScaledRadius();

            return localPoint.sqrMagnitude <= scaledRadius * scaledRadius;
        }
    }
}
