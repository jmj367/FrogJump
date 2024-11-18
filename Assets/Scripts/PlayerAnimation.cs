using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    private bool isBeforeJump = false;
    private Vector3 beforeFwd = Vector3.zero;

    //���͒l
    private bool isJumpStart = false;

    private enum State
    {
        Idle,
        Jump,
        RightTurn,
        LeftTurn
    }
    private State curState = State.Idle;
    private State prevState = State.Idle;

    //�W�����v�J�n���͎��
    public void OnJumpStart(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started)
        {
            isJumpStart = true;
        }

        if(context.phase == InputActionPhase.Canceled)
        {
            isJumpStart = false;
        }
    }

    private void Update()
    {
        
    }

    /// <summary>
    /// Idle��Ԃ̍X�V
    /// </summary>
    private void UpdateIdle()
    {
        //Start
        if(prevState != curState)
        {
            prevState = curState;
        }

        //Process

        //End
    }
}
