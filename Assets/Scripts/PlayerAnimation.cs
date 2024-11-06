using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    /// <summary>
    /// �O�t���[����isJumpStart
    /// </summary>
    private bool isBeforeJump = false;

    //���͒l
    /// <summary>
    /// �W�����v�{�^���������ꂽ��
    /// </summary>
    private bool isJumpStart = false;

    /// <summary>
    /// �v���C���[�̏��
    /// </summary>
    private enum State
    {
        Idle,
        Jump,
    }

    /// <summary>
    /// �v���C���[�̌��݂̃X�e�[�^�X
    /// </summary>
    private State currentState = State.Idle;

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
        
    }
}
