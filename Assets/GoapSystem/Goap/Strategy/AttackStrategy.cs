using UnityEngine;

using Nitou.Goap.Utilities;

namespace Nitou.Goap
{
    public class AttackStrategy : IActionStrategy
    {
        private readonly CountdownTimer _timer;
        private readonly AnimationController
        
        public bool CanPerform => true;

        public bool Complete { get; private set; }
        
        
    }
}