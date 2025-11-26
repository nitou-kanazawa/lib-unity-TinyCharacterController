using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nitou.TCC.Implements
{
    /// <summary>
    /// Indicatorの計算用カメラユーティリティクラス。
    /// </summary>
    [BurstCompile]
    public static class CameraUtility
    {
        private static Camera _mainCamera;

        private static Matrix4x4 _projectionMatrix;
        private static float _fov;
        private static float _aspect;
        private static bool _isOrthographic;
        private static float _orthographicSize;

        /// <summary>
        /// 任意のNear/Far値でマトリクスを作成する。
        /// 作成したマトリクスをキャッシュし、FOVやAspectが変更されない限りキャッシュ値を使用する。
        /// </summary>
        /// <param name="camera">キャッシュするカメラ設定</param>
        /// <param name="nearDistance">カメラ範囲のニア距離</param>
        /// <param name="farDistance">カメラ範囲のファー距離</param>
        /// <returns>キャッシュされたマトリクス</returns>
        public static Matrix4x4 GetCachedProjectionMatrix(in Camera camera, float nearDistance, float farDistance)
        {
            // Orthographicモードかどうかもキャッシュ判定に含める
            if (camera.orthographic)
            {
                if (Mathf.Approximately(_orthographicSize, camera.orthographicSize) &&
                    Mathf.Approximately(_aspect, camera.aspect) &&
                    _isOrthographic)
                    return _projectionMatrix;

                _orthographicSize = camera.orthographicSize;
                _aspect = camera.aspect;
                _isOrthographic = true;

                // Orthographic用マトリクスを生成
                _projectionMatrix = Matrix4x4.Ortho(
                    -camera.orthographicSize * camera.aspect, // left
                    camera.orthographicSize * camera.aspect, // right
                    -camera.orthographicSize, // bottom
                    camera.orthographicSize, // top
                    nearDistance,
                    farDistance
                );
            }
            else
            {
                if (Mathf.Approximately(_fov, camera.fieldOfView) &&
                    Mathf.Approximately(_aspect, camera.aspect) &&
                    !_isOrthographic)
                    return _projectionMatrix;

                _fov = camera.fieldOfView;
                _aspect = camera.aspect;
                _isOrthographic = false;

                // Perspective用マトリクスを生成
                _projectionMatrix = Matrix4x4.Perspective(
                    camera.fieldOfView,
                    camera.aspect,
                    nearDistance,
                    farDistance
                );
            }

            return _projectionMatrix;
        }

        /// <summary>
        /// メインカメラを取得する。
        /// すでに取得済みの場合はキャッシュ値を使用する。
        /// </summary>
        /// <param name="camera">取得されたカメラ</param>
        /// <returns>カメラが取得できた場合はTrue</returns>
        public static bool TryGetMainCamera(out Camera camera)
        {
            if (_mainCamera != null)
            {
                camera = _mainCamera;
                return true;
            }

            camera = Camera.main;
            _mainCamera = camera;
            return camera != null;
        }

        /// <summary>
        /// ターゲットが視野内にあるかどうかをチェックする。
        /// このAPIはBurstコードから呼び出される。
        /// </summary>
        /// <param name="screenPosition">UIの位置</param>
        /// <param name="screenSize">スクリーンサイズ</param>
        /// <param name="bounds">スクリーンの有効領域</param>
        /// <param name="inRange">UIが画面内にある場合はTrue</param>
        [BurstCompile]
        public static void CalculateIsTargetVisible(in float3 screenPosition, in int2 screenSize,
                                                    in float bounds, out bool inRange)
        {
            inRange = true;
            var width = screenSize.x * 0.5f * (1 - bounds);
            var height = screenSize.y * 0.5f * (1 - bounds);
            inRange &= screenPosition.x - width > 0 && screenPosition.x + width < screenSize.x;
            inRange &= screenPosition.y - height > 0 && screenPosition.y + height < screenSize.y;
            inRange &= screenPosition.z > 0;
        }

        /// <summary>
        /// 画面外に出るUI要素の角度を計算する。
        /// </summary>
        /// <param name="screenPosition">スクリーン座標</param>
        /// <param name="screenSize">スクリーンサイズ</param>
        /// <param name="angle">UIの角度</param>
        [BurstCompile]
        public static void IndirectUiAngle(in float3 screenPosition, in int2 screenSize, out float angle)
        {
            var position = screenPosition - new float3(screenSize.x * 0.5f, screenSize.y * 0.5f, 0);
            angle = (Mathf.Atan2(position.y, position.x) + Mathf.PI * -0.5f) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// ワールド座標をスクリーン座標に変換する。
        /// このAPIはBurstコードから呼び出される。
        /// </summary>
        /// <param name="position">ワールド座標</param>
        /// <param name="matrix">カメラマトリクス</param>
        /// <param name="screenSize">スクリーンサイズ</param>
        /// <param name="screenPosition">スクリーン座標</param>
        [BurstCompile]
        public static void WorldToScreenPosition(in float3 position, in Matrix4x4 matrix, in int2 screenSize,
                                                 out float3 screenPosition)
        {
            var viewPort = matrix * new float4(position.x, position.y, position.z, 1.0f);
            screenPosition = new float3(
                (viewPort.x / viewPort.w + 1.0f) * 0.5f * screenSize.x,
                (viewPort.y / viewPort.w + 1.0f) * 0.5f * screenSize.y,
                viewPort.z / -viewPort.w + 1.0f);
        }
    }
}