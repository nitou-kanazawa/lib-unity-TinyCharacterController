using Unity.Mathematics;
using UnityEngine;

namespace Nitou.TCC.Controller.Core
{
    /// <summary>
    /// <see cref="BrainBase"/>に関連する汎用的なメソッド集．
    /// </summary>
    public static class BrainUtils {

        /// <summary>
        /// 
        /// </summary>
        public static Vector3 LimitAxis(in Vector3 currentPosition, Vector3 newPosition, in bool3 freezeAxis) {
            // Correct for position offset due to component pushing.
            // Calculate this only if one of the axes is locked.
            if (freezeAxis.x || freezeAxis.y || freezeAxis.z) {
                // Reset the position to the initial value before calculation.
                if (freezeAxis.x)
                    newPosition.x = currentPosition.x;
                if (freezeAxis.y)
                    newPosition.y = currentPosition.y;
                if (freezeAxis.z)
                    newPosition.z = currentPosition.z;
            }

            return newPosition;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool ClosestHit(RaycastHit[] hits, ActorSettings settings, out RaycastHit closestHit, int count, float maxDistance) {
            var min = maxDistance;
            closestHit = new RaycastHit();
            var isHit = false;

            for (var i = 0; i < count; i++) {
                var hit = hits[i];
                if (hit.distance > min || hit.collider == null || settings.IsOwnCollider(hit.collider))
                    continue;

                min = hit.distance;
                closestHit = hit;
                isHit = true;
            }

            return isHit;
        }
    }
}