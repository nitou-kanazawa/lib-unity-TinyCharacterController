using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using System.Linq;

namespace Nitou.NGizmos
{
    internal enum ArrowType
    {
        Line,
        Solid
    }

    internal static class LineDrawer
    {
        #region Line

        /// <summary>
        /// 点列上に折れ線を描画する
        /// </summary>
        public static void DrawLines(IReadOnlyList<Vector3> points)
        {
            if (points is not { Count: > 1 })
                return;

            var from = points.First();
            foreach (var point in points.Skip(1))
            {
                Gizmos.DrawLine(from, point);
                from = point;
            }
        }

        /// <summary>
        /// ２グループの各対応点を結ぶ線を描画する
        /// </summary>
        public static void DrawLineSet(IReadOnlyList<Vector3> points1, IReadOnlyList<Vector3> points2)
        {
            if (points1 == null || points2 == null || points1.Count != points2.Count)
                return;

            for (int i = 0; i < points1.Count; i++)
            {
                Gizmos.DrawLine(points1[i], points2[i]);
            }
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region 円弧

        /// <summary>
        /// 円弧を描画する（改良版）
        /// </summary>
        public static void DrawWireArc(float radius, float angle, int segments = 20)
        {
            if (radius <= 0f || Mathf.Abs(angle) < 0.1f)
                return;

            var angleRad = angle * Mathf.Deg2Rad;
            var stepCount = Mathf.Max(2, (int)(segments * Mathf.Abs(angle) / 360f));
            var deltaAngle = angleRad / stepCount;

            for (int i = 0; i < stepCount; i++)
            {
                var angle1 = i * deltaAngle;
                var angle2 = (i + 1) * deltaAngle;

                var point1 = new Vector3(
                    radius * Mathf.Cos(angle1),
                    0f,
                    radius * Mathf.Sin(angle1)
                );
                var point2 = new Vector3(
                    radius * Mathf.Cos(angle2),
                    0f,
                    radius * Mathf.Sin(angle2)
                );

                Gizmos.DrawLine(point1, point2);
            }
        }

        /// <summary>
        /// 円弧を描画する（改良版）
        /// </summary>
        public static void DrawWireArc(Vector3 center, Quaternion rotation, float radius, float angle, int segments = 20)
        {
            if (radius <= 0f || Mathf.Abs(angle) < 0.1f)
                return;

            if (rotation.Equals(default))
            {
                rotation = Quaternion.identity;
            }

            var matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            using (new GizmoUtil.MatrixScope(matrix))
            {
                DrawWireArc(radius, angle, segments);
            }
        }

        /// <summary>
        /// 円弧を描画する
        /// </summary>
        public static void DrawWireArc(Matrix4x4 matrix, float radius, float angle, int segments)
        {
            using (new GizmoUtil.MatrixScope(matrix))
            {
                DrawWireArc(radius, angle, segments);
            }
        }


        // ※↓親の回転を考慮

        /// <summary>
        /// 円弧を描画する
        /// </summary>
        public static void DrawWireArc(Vector3 center, float radius, float angle, int segments, Quaternion rotation, Vector3 centerOfRotation)
        {
            if (rotation.Equals(default))
            {
                rotation = Quaternion.identity;
            }

            var matrix = Matrix4x4.TRS(centerOfRotation, rotation, Vector3.one);
            using (new GizmoUtil.MatrixScope(matrix))
            {
                var deltaTranslation = centerOfRotation - center;
                Vector3 from = deltaTranslation + Vector3.forward * radius;
                var step = Mathf.RoundToInt(angle / segments);
                for (int i = 0; i <= angle; i += step)
                {
                    var to = new Vector3(
                        radius * Mathf.Sin(i * Mathf.Deg2Rad),
                        0,
                        radius * Mathf.Cos(i * Mathf.Deg2Rad)
                    ) + deltaTranslation;

                    Gizmos.DrawLine(from, to);
                    from = to;
                }
            }
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region 円

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawCircle(PlaneType type, float radius, int segments = 20)
        {
            var points = ListPool<Vector3>.Get();

            MathUtils.FillCirclePoints(points, radius, segments, type: type);
            DrawLines(points);

            ListPool<Vector3>.Release(points);
        }

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawCircle(PlaneType type, Vector3 center, Quaternion rotation, float radius, int segments = 20)
        {
            if (rotation.Equals(default))
            {
                rotation = Quaternion.identity;
            }

            var matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            using var _ = new GizmoUtil.MatrixScope(matrix);
            DrawCircle(type, radius, segments);
        }

        #endregion


        // ----------------------------------------------------------------------------

        #region 矢印

        /// <summary>
        /// 位置，方向を指定して矢印を描画する
        /// </summary>
        public static void DrawRayArrow(ArrowType type, Vector3 pos, Vector3 direction,
                                        float arrowHeadLength = 0.08f, float arrowHeadAngle = 20.0f)
        {
            // 長さ０なら終了
            if (direction == Vector3.zero) return;

            // Arrow shaft
            Gizmos.DrawRay(pos, direction);

            // Arrow head
            switch (type)
            {
                case ArrowType.Line:
                {
                    var (right, left) = CalcArrowHeadDirection(direction, arrowHeadAngle);
                    Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
                    Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
                    break;
                }
                case ArrowType.Solid:
                {
                    float radius = 0.025f;
                    CylinderDrawer.DrawWireCone(
                        PlaneType.XY, // ※Z方向を基準に回転
                        pos + direction,
                        Quaternion.LookRotation(direction),
                        radius,
                        arrowHeadLength,
                        GizmoConstans.ARROW_SEGMENTS);
                    break;
                }
            }
        }

        /// <summary>
        /// ２点を指定して矢印を描画する
        /// </summary>
        public static void DrawLineArrow(ArrowType type, Vector3 from, Vector3 to,
                                         float arrowHeadLength = 0.2f, float arrowHeadAngle = 20.0f)
        {
            // 長さ０なら終了
            if (Mathf.Approximately(Vector3.Distance(from, to), 0f)) return;

            // Arrow shaft
            Gizmos.DrawLine(from, to);

            // Arrow head
            var direction = to - from;
            switch (type)
            {
                case ArrowType.Line:
                {
                    var (right, left) = CalcArrowHeadDirection(direction, arrowHeadAngle);
                    Gizmos.DrawRay(to, right * arrowHeadLength);
                    Gizmos.DrawRay(to, left * arrowHeadLength);
                    break;
                }
                case ArrowType.Solid:
                {
                    float radius = 0.025f;
                    CylinderDrawer.DrawWireCone(
                        PlaneType.XY, // ※Z方向を基準に回転
                        from + direction,
                        Quaternion.LookRotation(direction),
                        radius,
                        arrowHeadLength,
                        GizmoConstans.ARROW_SEGMENTS);
                    break;
                }
            }
        }


        /// <summary>
        /// 矢印先端の２方向を計算する
        /// </summary>
        private static (Vector3 right, Vector3 left) CalcArrowHeadDirection(Vector3 direction, float arrowHeadAngle) =>
        (
            right: Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward,
            left: Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward
        );

        #endregion
    }
}