using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    /// <summary>
    /// 前フレームのisJumpStart
    /// </summary>
    private bool isBeforeJump = false;

    //入力値
    /// <summary>
    /// ジャンプボタンが押されたか
    /// </summary>
    private bool isJumpStart = false;

    /// <summary>
    /// プレイヤーの状態
    /// </summary>
    private enum State
    {
        Idle,
        Jump,
    }

    /// <summary>
    /// プレイヤーの現在のステータス
    /// </summary>
    private State currentState = State.Idle;

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
        
    }
}
