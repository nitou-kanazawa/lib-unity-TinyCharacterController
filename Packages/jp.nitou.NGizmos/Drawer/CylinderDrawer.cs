using UnityEngine;
using UnityEngine.Pool;

namespace Nitou.Gizmo
{
    internal static class CylinderDrawer
    {
        /// <summary>
        /// 円形を定義するパラメータ
        /// </summary>
        private struct DiscParam
        {
            public float radius;
            public int segments;
            public float offset;

            public DiscParam(float radius, int segments, float offset = 0)
            {
                this.radius = radius;
                this.segments = segments;
                this.offset = offset;
            }
        }


        #region 描画メソッド

        /// <summary>
        /// 円柱を描画する
        /// </summary>
        public static void DrawWireCylinder(PlaneType type, Vector3 center, Quaternion rotation, float radius, float height, int discSegments = 20)
        {
            if (rotation.Equals(default))
            {
                rotation = Quaternion.identity;
            }

            var matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            using (new GizmoUtility.MatrixScope(matrix))
            {
                var half = height / 2;

                // Outer lines
                int outerSegments = 5;
                DrawWireCylinderOuterLines(
                    type,
                    new DiscParam(radius, outerSegments, half),
                    new DiscParam(radius, outerSegments, -half)
                );

                // Disks
                DrawWireDisc(type, new DiscParam(radius, discSegments, half));
                DrawWireDisc(type, new DiscParam(radius, discSegments, -half));
            }
        }

        /// <summary>
        /// 円錐を描画する
        /// </summary>
        public static void DrawWireCone(PlaneType type, Vector3 center, Quaternion rotation, float radius, float height, int discSegments = 20)
        {
            if (rotation.Equals(default))
            {
                rotation = Quaternion.identity;
            }

            var matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            using (new GizmoUtility.MatrixScope(matrix))
            {
                // Outer lines
                int outerSegments = 5;
                DrawWireConeOuterLines(type, new DiscParam(radius, outerSegments), height);

                // Disks
                DrawWireDisc(type, new DiscParam(radius, discSegments));
            }
        }

        #endregion


        #region Private Method

        /// <summary>
        /// 円柱の側面部を描画する
        /// </summary>
        private static void DrawWireCylinderOuterLines(PlaneType planeType, DiscParam upperDisc, DiscParam lowerDisc)
        {
            if (upperDisc.segments != lowerDisc.segments) return;

            // リスト取得
            var upperPoints = ListPool<Vector3>.Get();
            var lowerPoints = ListPool<Vector3>.Get();
            {
                // 座標計算
                MathUtils.FillCirclePoints(
                    radius: upperDisc.radius,
                    resultPoints: upperPoints,
                    type: planeType,
                    segments: upperDisc.segments,
                    offset: upperDisc.offset * planeType.GetNormal(),
                    includeFullCircle: false
                );
                MathUtils.FillCirclePoints(
                    resultPoints: lowerPoints,
                    radius: lowerDisc.radius,
                    type: planeType,
                    segments: lowerDisc.segments,
                    offset: lowerDisc.offset * planeType.GetNormal(),
                    includeFullCircle: false
                );

                // 描画（※上下円の各点を結ぶ線分）
                LineDrawer.DrawLineSet(upperPoints, lowerPoints);
            }
            ListPool<Vector3>.Release(upperPoints);
            ListPool<Vector3>.Release(lowerPoints);
        }

        /// <summary>
        /// 円錐の側面部を描画する
        /// </summary>
        private static void DrawWireConeOuterLines(PlaneType type, DiscParam lowerDisc, float height)
        {
            var lowerPoints = ListPool<Vector3>.Get();
            {
                var top = (lowerDisc.offset + height) * type.GetNormal();
                MathUtils.FillCirclePoints(
                    resultPoints: lowerPoints,
                    radius: lowerDisc.radius,
                    type: type,
                    segments: lowerDisc.segments,
                    offset: lowerDisc.offset * type.GetNormal(),
                    includeFullCircle: false
                );

                // 描画（※底面(円)の各点から頂点への線分）
                lowerPoints.ForEach(p => Gizmos.DrawLine(p, top));
            }
            ListPool<Vector3>.Release(lowerPoints);
        }

        /// <summary>
        /// 円盤を描画する
        /// </summary>
        private static void DrawWireDisc(PlaneType planType, DiscParam disc)
        {
            var points = ListPool<Vector3>.Get();
            {
                // 座標計算
                MathUtils.FillCirclePoints(
                    resultPoints: points,
                    radius: disc.radius,
                    type: planType,
                    segments: disc.segments,
                    offset: disc.offset * planType.GetNormal(),
                    includeFullCircle: true
                );
                LineDrawer.DrawLines(points);
            }
            ListPool<Vector3>.Release(points);
        }

        #endregion
    }
}