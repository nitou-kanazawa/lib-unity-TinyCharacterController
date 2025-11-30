using Nitou.TCC.CharacterControl.Control;
using UnityEngine;

namespace Nitou.TCC.CharacterControl.Behavior
{
    public class Demo : MonoBehaviour
    {

        public MoveNavmeshControl moveControl;
        public GameObject Target;
        
        void Start()
        {
            
        
        }

        public void Update()
        {
            if(Target == null)
                return;
            
            var position = Target.transform.position;
            moveControl.SetTargetPosition(position);
        }
    }
}
