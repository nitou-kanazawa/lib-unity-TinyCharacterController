using UnityEngine;
using System.Collections.Generic;

// [REF]
//  github: neuneu9/unity-gizmos-utility https://github.com/neuneu9/unity-gizmos-utility/blob/master/GizmosUtility.cs
//  github: code-beans/GizmoExtensions https://github.com/code-beans/GizmoExtensions/blob/master/src/GizmosExtensions.cs


namespace Nitou.Gizmo
{
    public enum DrawMode
    {
        /// <summary>ワイヤーフレーム </summary>
        Wire,

        /// <summary>サーフェイス </summary>
        Surface,
    }

    public static class NGizmo
    {
        #region 3D図形

        /// <summary>
        /// 線分を描画する
        /// </summary>
        public static void DrawRay(Vector3 pos, Vector3 direction, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            Gizmos.DrawRay(pos, direction);
        }

        /// <summary>
        /// 線分を描画する
        /// </summary>
        public static void DrawLine(Vector3 from, Vector3 to, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            Gizmos.DrawLine(from, to);
        }


        /// <summary>
        /// 折れ線を描画する
        /// </summary>
        public static void DrawLines(IReadOnlyList<Vector3> points, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            LineDrawer.DrawLines(points);
        }

        #endregion


        #region 3D図形 (Ray)

        /// <summary>
        /// 矢印を描画する
        /// </summary>
        public static void DrawRayArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            LineDrawer.DrawRayArrow(ArrowType.Solid, pos, direction, arrowHeadLength, arrowHeadAngle);
        }

        /// <summary>
        /// 矢印を描画する
        /// </summary>
        public static void DrawRayArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            LineDrawer.DrawRayArrow(ArrowType.Solid, pos, direction, arrowHeadLength, arrowHeadAngle);
        }

        // ----- 

        /// <summary>
        /// 矢印を描画する
        /// </summary>
        public static void DrawLineArrow(Vector3 from, Vector3 to, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            LineDrawer.DrawLineArrow(ArrowType.Solid, from, to, arrowHeadLength, arrowHeadAngle);
        }

        /// <summary>
        /// 矢印を描画する
        /// </summary>
        public static void DrawLineArrow(Vector3 from, Vector3 to, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            LineDrawer.DrawLineArrow(ArrowType.Solid, from, to, arrowHeadLength, arrowHeadAngle);
        }

        #endregion


        #region 3D図形 (Arc)

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawWireCircle(Vector3 center, float radius)
        {
            LineDrawer.DrawCircle(PlaneType.ZX, center, Quaternion.identity, radius);
        }

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawWireCircle(Vector3 center, float radius, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            LineDrawer.DrawCircle(PlaneType.ZX, center, Quaternion.identity, radius);
        }

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawWireCircle(Vector3 center, Quaternion rotation, float radius)
        {
            LineDrawer.DrawCircle(PlaneType.ZX, center, rotation, radius);
        }

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawWireCircle(Vector3 center, Quaternion rotation, float radius, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            LineDrawer.DrawCircle(PlaneType.ZX, center, rotation, radius);
        }

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawWireCircle(Vector3 center, float radius, PlaneType type)
        {
            LineDrawer.DrawCircle(type, center, Quaternion.identity, radius);
        }

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawWireCircle(Vector3 center, float radius, PlaneType type, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            LineDrawer.DrawCircle(type, center, Quaternion.identity, radius);
        }

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawWireCircle(Vector3 center, Quaternion rotation, float radius, PlaneType type)
        {
            LineDrawer.DrawCircle(type, center, rotation, radius);
        }

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawWireCircle(Vector3 center, Quaternion rotation, float radius, PlaneType type, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            LineDrawer.DrawCircle(type, center, rotation, radius);
        }

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawWireCircle(Transform transform, float radius)
        {
            LineDrawer.DrawCircle(PlaneType.ZX, transform.position, transform.rotation, radius);
        }

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawWireCircle(Transform transform, float radius, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            LineDrawer.DrawCircle(PlaneType.ZX, transform.position, transform.rotation, radius);
        }

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawWireCircle(Transform transform, float radius, PlaneType type)
        {
            LineDrawer.DrawCircle(type, transform.position, transform.rotation, radius);
        }

        /// <summary>
        /// 円を描画する
        /// </summary>
        public static void DrawWireCircle(Transform transform, float radius, PlaneType type, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            LineDrawer.DrawCircle(type, transform.position, transform.rotation, radius);
        }

        /// <summary>
        /// 円弧を描画する
        /// </summary>
        public static void DrawWireArc(Vector3 center, float radius, float angle)
        {
            LineDrawer.DrawWireArc(center, Quaternion.identity, radius, angle);
        }

        /// <summary>
        /// 円弧を描画する
        /// </summary>
        public static void DrawWireArc(Vector3 center, float radius, float angle, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            LineDrawer.DrawWireArc(center, Quaternion.identity, radius, angle);
        }

        /// <summary>
        /// 円弧を描画する
        /// </summary>
        public static void DrawWireArc(Vector3 center, Quaternion rotation, float radius, float angle)
        {
            LineDrawer.DrawWireArc(center, rotation, radius, angle);
        }

        /// <summary>
        /// 円弧を描画する
        /// </summary>
        public static void DrawWireArc(Vector3 center, Quaternion rotation, float radius, float angle, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            LineDrawer.DrawWireArc(center, rotation, radius, angle);
        }

        /// <summary>
        /// 円弧を描画する
        /// </summary>
        public static void DrawWireArc(Transform transform, float radius, float angle)
        {
            LineDrawer.DrawWireArc(transform.position, transform.rotation, radius, angle);
        }

        /// <summary>
        /// 円弧を描画する
        /// </summary>
        public static void DrawWireArc(Transform transform, float radius, float angle, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            LineDrawer.DrawWireArc(transform.position, transform.rotation, radius, angle);
        }

        #endregion


        #region 3D図形 (Cube)

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawWireCube(Vector3 center, Vector3 size)
        {
            CubeDrawer.DrawCube(center, Quaternion.identity, size, DrawMode.Wire);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawWireCube(Vector3 center, Vector3 size, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            CubeDrawer.DrawCube(center, Quaternion.identity, size, DrawMode.Wire);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawWireCube(Vector3 center, Quaternion rotation, Vector3 size)
        {
            CubeDrawer.DrawCube(center, rotation, size, DrawMode.Wire);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawWireCube(Vector3 center, Quaternion rotation, Vector3 size, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);

            CubeDrawer.DrawCube(center, rotation, size, DrawMode.Wire);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawWireCube(Transform transform, Vector3 size)
        {
            CubeDrawer.DrawCube(transform.position, transform.rotation, size, DrawMode.Wire);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawWireCube(Transform transform, Vector3 size, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);

            CubeDrawer.DrawCube(transform.position, transform.rotation, size, DrawMode.Wire);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawWireCube(BoxCollider collider)
        {
            CubeDrawer.DrawCube(collider.GetWorldCenter(), collider.transform.rotation, collider.size, DrawMode.Wire);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawWireCube(BoxCollider collider, Color color)
        {
            using (new GizmoUtility.ColorScope(color))
            {
                CubeDrawer.DrawCube(collider.GetWorldCenter(), collider.transform.rotation, collider.size, DrawMode.Wire);
            }
        }


        // -----

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawCube(Vector3 center, Vector3 size)
        {
            CubeDrawer.DrawCube(center, Quaternion.identity, size, DrawMode.Surface);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawCube(Vector3 center, Vector3 size, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            CubeDrawer.DrawCube(center, Quaternion.identity, size, DrawMode.Surface);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawCube(Vector3 center, Quaternion rotation, Vector3 size)
        {
            CubeDrawer.DrawCube(center, rotation, size, DrawMode.Surface);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawCube(Vector3 center, Quaternion rotation, Vector3 size, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            CubeDrawer.DrawCube(center, rotation, size, DrawMode.Surface);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawCube(Transform transform, Vector3 size)
        {
            CubeDrawer.DrawCube(transform.position, transform.rotation, size, DrawMode.Surface);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawCube(Transform transform, Vector3 size, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            CubeDrawer.DrawCube(transform.position, transform.rotation, size, DrawMode.Surface);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawCube(BoxCollider collider)
        {
            CubeDrawer.DrawCube(collider.GetWorldCenter(), collider.transform.rotation, collider.size, DrawMode.Surface);
        }

        /// <summary>
        /// キューブを描画する
        /// </summary>
        public static void DrawCube(BoxCollider collider, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            CubeDrawer.DrawCube(collider.GetWorldCenter(), collider.transform.rotation, collider.size, DrawMode.Surface);
        }

        #endregion


        #region 3D図形 (Sphere)

        /// <summary>
        /// 球を描画する
        /// </summary>
        public static void DrawWireSphere(Vector3 position, float radius, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            Gizmos.DrawWireSphere(position, radius);
        }

        /// <summary>
        /// 球を描画する
        /// </summary>
        public static void DrawWireSphere(SphereCollider collider, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            Gizmos.DrawWireSphere(collider.GetWorldCenter(), collider.radius);
        }

        /// <summary>
        /// 球を描画する
        /// </summary>
        public static void DrawSphere(Vector3 position, float radius, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            Gizmos.DrawSphere(position, radius);
        }

        /// <summary>
        /// 球を描画する
        /// </summary>
        public static void DrawSphere(SphereCollider collider, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            Gizmos.DrawSphere(collider.GetWorldCenter(), collider.radius);
        }

        #endregion


        #region 3D図形 (Cylinder)

        /// <summary>
        /// 円柱を描画する
        /// </summary>
        public static void DrawWireCylinder(Vector3 center, float radius, float height)
        {
            CylinderDrawer.DrawWireCylinder(PlaneType.ZX, center, Quaternion.identity, radius, height);
        }

        /// <summary>
        /// 円柱を描画する
        /// </summary>
        public static void DrawWireCylinder(Vector3 center, float radius, float height, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            CylinderDrawer.DrawWireCylinder(PlaneType.ZX, center, Quaternion.identity, radius, height);
        }

        /// <summary>
        /// 円柱を描画する
        /// </summary>
        public static void DrawWireCylinder(Transform transform, float radius, float height)
        {
            CylinderDrawer.DrawWireCylinder(PlaneType.ZX, transform.position, transform.rotation, radius, height);
        }

        /// <summary>
        /// 円柱を描画する
        /// </summary>
        public static void DrawWireCylinder(Transform transform, float radius, float height, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            CylinderDrawer.DrawWireCylinder(PlaneType.ZX, transform.position, transform.rotation, radius, height);
        }

        #endregion


        #region 3D図形 (Cone)

        /// <summary>
        /// 円錐を描画する
        /// </summary>
        public static void DrawWireCone(Vector3 center, float radius, float height)
        {
            CylinderDrawer.DrawWireCone(PlaneType.ZX, center, Quaternion.identity, radius, height);
        }

        /// <summary>
        /// 円錐を描画する
        /// </summary>
        public static void DrawWireCone(Transform transform, float radius, float height)
        {
            CylinderDrawer.DrawWireCone(PlaneType.ZX, transform.position, transform.rotation, radius, height);
        }

        /// <summary>
        /// 円錐を描画する
        /// </summary>
        public static void DrawWireCone(Transform transform, float radius, float height, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            CylinderDrawer.DrawWireCone(PlaneType.ZX, transform.position, transform.rotation, radius, height);
        }

        #endregion


        #region 3D図形 (Capsule)

        /// <summary>
        /// カプセルを描画する
        /// </summary>
        public static void DrawWireCapsule(Vector3 center, float radius, float height)
        {
            CapcelDrawer.DrawWireCapsule(center, Quaternion.identity, radius, height);
        }

        /// <summary>
        /// カプセルを描画する
        /// </summary>
        public static void DrawWireCapsule(Vector3 center, float radius, float height, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            CapcelDrawer.DrawWireCapsule(center, Quaternion.identity, radius, height);
        }

        /// <summary>
        /// カプセルを描画する
        /// </summary>
        public static void DrawWireCapsule(Vector3 center, Quaternion rotation, float radius, float height)
        {
            CapcelDrawer.DrawWireCapsule(center, rotation, radius, height);
        }

        /// <summary>
        /// カプセルを描画する
        /// </summary>
        public static void DrawWireCapsule(Vector3 center, Quaternion rotation, float radius, float height, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            CapcelDrawer.DrawWireCapsule(center, rotation, radius, height);
        }

        /// <summary>
        /// カプセルを描画する
        /// </summary>
        public static void DrawWireCapsule(Transform transform, float radius, float height)
        {
            CapcelDrawer.DrawWireCapsule(transform.position, transform.rotation, radius, height);
        }

        /// <summary>
        /// カプセルを描画する
        /// </summary>
        public static void DrawWireCapsule(Transform transform, float radius, float height, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            CapcelDrawer.DrawWireCapsule(transform.position, transform.rotation, radius, height);
        }

        /// <summary>
        /// カプセルを描画する
        /// </summary>
        public static void DrawWireCapsule(CapsuleCollider collider)
        {
            CapcelDrawer.DrawWireCapsule(collider);
        }

        /// <summary>
        /// カプセルを描画する
        /// </summary>
        public static void DrawWireCapsule(CapsuleCollider collider, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            CapcelDrawer.DrawWireCapsule(collider);
        }

        #endregion


        #region 3D図形 (Mesh)

        /// <summary>
        /// メッシュを描画する
        /// </summary>
        public static void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            Gizmos.DrawMesh(mesh, position, rotation);
        }

        /// <summary>
        /// メッシュを描画する
        /// </summary>
        public static void DrawMesh(Mesh mesh, Transform transform, Color color)
        {
            using var _ = new GizmoUtility.ColorScope(color);
            Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
        }

        #endregion


        #region 3D図形 (Misc)

        /// <summary>
        /// 指定したコライダーの形状をギズモで描画する
        /// </summary>
        /// <param name="collider">描画対象のコライダー</param>
        /// <param name="color">描画色</param>
        public static void DrawCollider(in Collider collider, Color color)
        {
            if (collider == null)
                return;

            var trs = collider.transform;
            var position = trs.position;
            var rotation = trs.rotation;
            var scale = trs.localScale;

            switch (collider)
            {
                case MeshCollider meshCollider:
                    if (meshCollider.sharedMesh != null)
                        DrawMesh(meshCollider.sharedMesh, trs, color);
                    break;
                case BoxCollider boxCollider:
                    DrawCube(boxCollider, color);
                    break;
                case SphereCollider sphereCollider:
                    DrawSphere(sphereCollider, color);
                    break;
                case CapsuleCollider capsuleCollider:
                    DrawWireCapsule(capsuleCollider, color);
                    break;
                case CharacterController characterController:
                    // CharacterControllerをカプセルとして描画
                    var characterCenter = characterController.transform.TransformPoint(characterController.center);
                    DrawWireCapsule(characterCenter, characterController.transform.rotation,
                        characterController.radius, characterController.height, color);
                    break;
            }
        }

        #endregion
    }
}