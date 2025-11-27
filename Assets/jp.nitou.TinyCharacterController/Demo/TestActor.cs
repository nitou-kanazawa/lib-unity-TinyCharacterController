using Nitou.TCC.CharacterControl.Check;
using UnityEngine;
using Nitou.TCC.Integration;
using Nitou.TCC.CharacterControl.Control;
using Nitou.TCC.CharacterControl.Effect;


public class TestActor : MonoBehaviour {

    // Brain
    [SerializeField] ActorBrain _input;
    
    // Controls
    [SerializeField] MoveControl _move;
    [SerializeField] JumpControl _jump;
    [SerializeField] CursorLookControl _cursorControl;
    
    // Effects
    // [SerializeField] ExtraForce _extraForce;

    // Checks
    [SerializeField] GroundCheck _groundCheck;
    
    void Update() {
        
        // 移動処理
        var inputMoveAxis = _input.CharacterActions.movement.value;
        _move.Move(inputMoveAxis);

        // 接地状態ならカーソル方向を向く
        _cursorControl.LookTargetPoint(Input.mousePosition);
        _cursorControl.TurnPriority = _groundCheck.IsFirmlyOnGround ? 3 : -1;
        
        
        if(Input.GetKeyDown(KeyCode.Space))
            _jump.Jump();
    }
}
