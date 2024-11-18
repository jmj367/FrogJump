using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    [Tooltip("�����A�j���[�V�������n�߂�܂ł�angle")]
    [SerializeField] private float turnLmt = 0.1f;

    private bool isBeforeJump = false;
    private Vector3 beforeFwd = Vector3.zero;

    //���͒l
    private bool isJump = false;

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
        if(context.phase == InputActionPhase.Canceled)
        {
            isJump = true;
        }
    }

    private void Update()
    {
        beforeFwd = transform.forward;
    }

    //�X�e�[�g����Update
    private void UpdateIdle()
    {
        //Start
        if(prevState != curState)
        {
            prevState = curState;
        }

        //Process

        float angle = GetAngleSub(beforeFwd, transform.forward, transform.up);

        //End
        if(angle > turnLmt)
        {
            curState = State.RightTurn;
        }

        if(angle < -turnLmt)
        {
            curState = State.LeftTurn;
        }

        if (isJump)
        {
            curState = State.Jump;
        }
    }

    private void UpdateRightTurn()
    {
        //Start
        if (prevState != curState)
        {
            prevState = curState;
        }

        //Process
        float angle = GetAngleSub(beforeFwd, transform.forward, transform.up);

        //End
        if(angle <= 0)
        {
            curState = State.Idle;
        }

        if (isJump)
        {
            curState = State.Jump;
        }
    }

    private void UpdateLeftTurn()
    {
        //Start
        if (prevState != curState)
        {
            prevState = curState;
        }

        //Process
        float angle = GetAngleSub(beforeFwd, transform.forward, transform.up);

        //End
        if(angle <= 0)
        {
            curState = State.Idle;
        }

        if (isJump)
        {
            curState = State.Jump;
        }
    }

    private float GetAngleSub(Vector3 fromFwd, Vector3 toFwd, Vector3 axis)
    {
        Vector3 planeFrom = Vector3.ProjectOnPlane(fromFwd, axis);
        Vector3 planeTo = Vector3.ProjectOnPlane(toFwd, axis);

        float angle = Vector3.SignedAngle(planeFrom, planeTo, transform.up);

        return angle;
    }

    private void UpdateJump()
    {

    }
}
