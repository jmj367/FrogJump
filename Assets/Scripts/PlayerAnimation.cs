using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    private bool isBeforeJump = false;
    private Vector3 beforeFwd = Vector3.zero;

    //入力値
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

    //ジャンプ開始入力受取
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
    /// Idle状態の更新
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
