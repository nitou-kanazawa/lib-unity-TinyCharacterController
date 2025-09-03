using UnityEngine;

namespace Nitou.NGizmos
{
    /// <summary>
    /// 直方体の<see cref="Gizmos"/>描画クラス．
    /// </summary>
    internal static class CubeDrawer
    {
        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawCube(Vector3 center, Quaternion rotation, Vector3 size, DrawMode mode = DrawMode.Wire)
        {
            if (size.x < 0 || size.y < 0 || size.z < 0)
                return;
                
            if (rotation.Equals(default))
            {
                rotation = Quaternion.identity;
            }

            var matrix = Matrix4x4.TRS(center, rotation, size);
            using var _ = new GizmoUtil.MatrixScope(matrix);
            switch (mode)
            {
                case DrawMode.Wire:
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                    break;
                case DrawMode.Surface:
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);
                    break;
                default:
                    throw new System.NotImplementedException();
            }
        }
    }
}