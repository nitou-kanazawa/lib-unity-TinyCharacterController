using UnityEngine;

namespace Nitou.TCC.Inputs
{
    public class EnemyBehaviour : EnemyBehaviourBase
    {
        [SerializeField] private Transform _target;



        /// ----------------------------------------------------------------------------
        // MonoBehaviour Method 
        private void Update()
        {
            if (_target != null)
            {
                SetMovement(_target.position - transform.position);
            }
        }
    }
}