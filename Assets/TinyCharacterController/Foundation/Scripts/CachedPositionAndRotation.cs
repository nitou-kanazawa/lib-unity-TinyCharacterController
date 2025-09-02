using UnityEngine;

namespace Nitou.TCC.Utils
{
    /// <summary>
    /// <see cref="Transform"/>の位置姿勢のキャッシュ
    /// </summary>
    public readonly struct CachedPositionAndRotation
    {
        public readonly Vector3 Position { get; }
        public readonly Quaternion Rotation { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CachedPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        /// <summary>
        /// キャッシュされた値でTransformを復元
        /// </summary>
        public void ApplyTo(Transform transform)
        {
            transform.position = Position;
            transform.rotation = Rotation;
        }
        
        public static CachedPositionAndRotation FromTransform(Transform transform) => new (transform.position, transform.rotation);
    }
}