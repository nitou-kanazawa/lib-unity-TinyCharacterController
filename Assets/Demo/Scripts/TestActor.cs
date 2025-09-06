using UnityEngine;
using Nitou.TCC.Inputs;
using Nitou.TCC.Controller.Control;
using Nitou.TCC.Controller.Effect;


public class TestActor : MonoBehaviour {

    [SerializeField] ActorBrain _input;
    [SerializeField] MoveControl _move;
    [SerializeField] CursorLookControl _cursorControl;
    [SerializeField] ExtraForce _extraForce;


    void Update() {
        var inputMoveAxis = _input.CharacterActions.movement.value;
        _move.Move(inputMoveAxis);

        _cursorControl.LookTargetPoint(Input.mousePosition);
    }
}
