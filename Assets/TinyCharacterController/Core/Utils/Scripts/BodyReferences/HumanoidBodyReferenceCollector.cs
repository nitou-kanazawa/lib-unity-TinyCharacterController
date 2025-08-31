using UnityEngine;
using Alchemy;
using Alchemy.Inspector;

namespace Nitou.TCC.Utils.Humanoid
{
    public enum BodyType
    {
        RightHand,
        LeftHand,
        Head,
    }

    /// <summary>
    /// 
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HumanoidBodyReferenceCollector : MonoBehaviour
    {
        [TabGroup("Hand")] [SerializeField, Indent]
        private HandReference _leftHand;

        [TabGroup("Hand")] [SerializeField, Indent]
        private HandReference _rightHand;

        [TabGroup("Foot")] [SerializeField, Indent]
        private FootReference _leftFoot;

        [TabGroup("Foot")] [SerializeField, Indent]
        private FootReference _rightFoot;

        [TabGroup("Head")] [SerializeField, Indent]
        private HeadReference _head;


        /// <summary> 左手 </summary>
        public Transform LeftHand => _leftHand.transform;

        /// <summary>右手</summary>
        public Transform RightHand => _rightHand.transform;

        /// <summary>頭 </summary>
        public Transform Head => _head.transform;


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            {
                if (_rightHand != null)
                    Gizmos.DrawWireSphere(_rightHand.transform.position, 0.1f);

                if (_leftHand != null)
                    Gizmos.DrawWireSphere(_leftHand.transform.position, 0.1f);
            }

            {
                if (_leftFoot != null)
                    Gizmos.DrawWireSphere(_leftFoot.transform.position, 0.1f);

                if (_rightFoot != null)
                    Gizmos.DrawWireSphere(_rightFoot.transform.position, 0.1f);
            }
        }
#endif
    }
}