using UnityEngine;
using UnityEngine.Pool;

namespace Nitou.NGizmos
{
    internal static class CapcelDrawer
    {
        /// <summary>
        /// ワイヤーフレームでカプセルを描画する
        /// </summary>
        public static void DrawWireCapsule(Vector3 center, Quaternion rotation, float radius, float height, int segments = 20)
        {
            if (radius <= 0f || height <= 0f)
                return;
                
            if (rotation.Equals(default))
            {
                rotation = Quaternion.identity;
            }

            var matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            using (new GizmoUtil.MatrixScope(matrix))
            {
                DrawWireCapsuleInternal(radius, height, segments);
            }
        }

        /// <summary>
        /// CapsuleColliderを描画する
        /// </summary>
        public static void DrawWireCapsule(CapsuleCollider capsule)
        {
            if (capsule == null)
                return;
                
            var center = capsule.transform.TransformPoint(capsule.center);
            var rotation = capsule.transform.rotation;
            
            // CapsuleColliderの方向に応じて回転を調整
            var directionRotation = capsule.direction switch
            {
                0 => Quaternion.Euler(0, 0, 90), // X軸
                1 => Quaternion.identity,        // Y軸 (デフォルト)
                2 => Quaternion.Euler(90, 0, 0), // Z軸
                _ => Quaternion.identity
            };
            
            var finalRotation = rotation * directionRotation;
            var scaledRadius = GetScaledRadius(capsule);
            var scaledHeight = GetScaledHeight(capsule);
            
            DrawWireCapsule(center, finalRotation, scaledRadius, scaledHeight);
        }

        /// <summary>
        /// カプセルの内部実装
        /// </summary>
        private static void DrawWireCapsuleInternal(float radius, float height, int segments)
        {
            var cylinderHeight = Mathf.Max(0, height - 2 * radius);
            var halfCylinder = cylinderHeight / 2;

            // 円柱部分
            if (cylinderHeight > 0)
            {
                CylinderDrawer.DrawWireCylinder(PlaneType.ZX, Vector3.zero, Quaternion.identity, radius, cylinderHeight, segments);
            }

            // 上下の半球
            DrawWireHemisphere(Vector3.up * halfCylinder, radius, true, segments);
            DrawWireHemisphere(Vector3.down * halfCylinder, radius, false, segments);
        }

        /// <summary>
        /// 半球を描画する
        /// </summary>
        private static void DrawWireHemisphere(Vector3 center, float radius, bool upperHalf, int segments)
        {
            var points = ListPool<Vector3>.Get();
            
            try
            {
                // 縦の円弧（複数方向）
                int arcCount = segments / 4; // 4分割程度
                for (int i = 0; i < arcCount; i++)
                {
                    float angle = (360f / arcCount) * i;
                    var rotation = Quaternion.Euler(0, angle, 0);
                    
                    points.Clear();
                    
                    // 半球の弧を生成
                    var angleRange = upperHalf ? (0f, 90f) : (90f, 180f);
                    var deltaAngle = (angleRange.Item2 - angleRange.Item1) / (segments / 2);
                    
                    for (int j = 0; j <= segments / 2; j++)
                    {
                        var verticalAngle = (angleRange.Item1 + j * deltaAngle) * Mathf.Deg2Rad;
                        var point = new Vector3(
                            0,
                            radius * Mathf.Sin(verticalAngle),
                            radius * Mathf.Cos(verticalAngle)
                        );
                        points.Add(center + rotation * point);
                    }
                    
                    LineDrawer.DrawLines(points);
                }
            }
            finally
            {
                ListPool<Vector3>.Release(points);
            }
        }

        /// <summary>
        /// CapsuleColliderのスケール済み半径を取得
        /// </summary>
        private static float GetScaledRadius(CapsuleCollider capsule)
        {
            var scale = capsule.transform.lossyScale;
            return capsule.radius * (capsule.direction switch
            {
                0 => Mathf.Max(scale.y, scale.z), // X軸方向
                1 => Mathf.Max(scale.x, scale.z), // Y軸方向
                2 => Mathf.Max(scale.x, scale.y), // Z軸方向
                _ => Mathf.Max(scale.x, scale.y, scale.z)
            });
        }

        /// <summary>
        /// CapsuleColliderのスケール済み高さを取得
        /// </summary>
        private static float GetScaledHeight(CapsuleCollider capsule)
        {
            var scale = capsule.transform.lossyScale;
            return capsule.height * (capsule.direction switch
            {
                0 => scale.x, // X軸方向
                1 => scale.y, // Y軸方向
                2 => scale.z, // Z軸方向
                _ => Mathf.Max(scale.x, scale.y, scale.z)
            });
        }
    }
}