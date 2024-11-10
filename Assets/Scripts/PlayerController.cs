using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Tooltip("�J�����̐ݒ�l")]
    [SerializeField] private Parameters param;
    [Tooltip("�e���\���V�~�����[�^�[")]
    [SerializeField] private TrajectotySimulator trajectorySim;
    [Tooltip("�ŏ��̃W�����v��")]
    [SerializeField] private float minJumpPower = 8;
    [Tooltip("�ő�̃W�����v��")]
    [SerializeField] private float maxJumpPower = 13;
    [Tooltip("�W�����v�͂��ő�ɂȂ�܂ł̎���(�b)")]
    [SerializeField] private float boostJumpTime = 2;
    [Tooltip("�W�����v�����̏�����̒����l")]
    [SerializeField] private float adjustJumpDir = 30;
    [Tooltip("�d��")]
    [SerializeField] private float gravity = 5;
    [Tooltip("�W�����v�p�x�̐���")]
    [SerializeField] private float jumpAngleLmt = 90;
    [Tooltip("�W�����v�ҋ@����1�b���̉�](�x)")]
    [SerializeField] private float angleRotateSpd = 90;
    [Tooltip("�ڒn������s���܂ł̎���")]
    [SerializeField] private float waitTime = 0.1f;

    //���͒l
    private bool isJumpStart = false;
    private Vector2 mouseDisplacement = Vector2.zero;

    //�W�����v�֘A
    private float currentJumpPower = 0;
    private float defferenceJumpPower = 0;
    private bool isGrounded = false;
    private Vector2 sumDisplacement = Vector2.zero;
    private Vector3 currentJumpDir = Vector3.zero;
    private Vector3 currentSpeed = Vector3.zero;

    //�X�e�[�g�֘A
    private enum FrogState
    {
        Idle,
        JumpStart,
        Jump,
        WhilwJump
    }
    private FrogState curState = FrogState.Idle;
    private FrogState prevState = FrogState.Idle;
    private float timer = 0;

    //���͎��
    public void OnJumpStart(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started)
        {
            isJumpStart = true;
        }
        else if(context.phase == InputActionPhase.Canceled)
        {
            isJumpStart = false;
        }
    }

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        mouseDisplacement = context.ReadValue<Vector2>();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (curState == FrogState.WhilwJump)
        {
            if (collision.gameObject.CompareTag("NonStickObj"))
            {
                currentSpeed.y *= -1;
            }
            else
            {
                isGrounded = true;
                Vector3 nor = collision.contacts[0].normal;
                Stick(nor);
            }
        }
    }

    /// <summary>
    /// �ǁA�V��ɕ��������鏈��
    /// </summary>
    /// <param name="nor"></param>
    private void Stick(Vector3 nor)
    {
        //�v���C���[�̏�������Փ˃I�u�W�F�N�g�̖@���x�N�g���Ɍ�����
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, nor);
        transform.Rotate(rotation.eulerAngles, Space.World);
    }

    private void Start()
    {
        isGrounded = true;
    }

    private void Update()
    {
        DecideJumpDIr();

        switch (curState)
        {
            case FrogState.Idle: UpdateIdle(); break;
            case FrogState.JumpStart: UpdateJumpStart(); break;
            case FrogState.Jump: UpdateJump(); break;
            case FrogState.WhilwJump: UpdateWhileJump(); break;
        }

        UpdateMove();
    }

    private void UpdateMove()
    {
        if (!isGrounded)
        {
            transform.position += currentSpeed * Time.deltaTime;
            currentSpeed.y -= gravity * Time.deltaTime;
        }
    }

    //�X�e�[�g����Update
    private void UpdateIdle()
    {
        if(curState != prevState)
        {
            prevState = curState;
        }

        if (isJumpStart)
        {
            prevState = curState;
            currentSpeed = Vector3.zero;
            curState = FrogState.JumpStart;
        }
    }

    private void UpdateJumpStart()
    {
        if (curState != prevState)
        {
            prevState = curState;
            currentJumpPower = minJumpPower;
            defferenceJumpPower = maxJumpPower - minJumpPower;
            trajectorySim.SetIsSim(true);
        }

        TurnFwdSlowlyOnGround();
        currentJumpPower += defferenceJumpPower * Time.deltaTime / boostJumpTime;
        if(currentJumpPower > maxJumpPower)
        {
            currentJumpPower = maxJumpPower;
        }

        trajectorySim.SetValue(transform.position, currentJumpDir * currentJumpPower, gravity);

        if (!isJumpStart)
        {
            curState = FrogState.Jump;
        }
    }

    private void UpdateJump()
    {
        if (curState != prevState)
        {
            prevState = curState;
            TurnFwdQuickly();
            currentSpeed = currentJumpDir * currentJumpPower;
            isGrounded = false;
            timer = 0;
            trajectorySim.SetIsSim(false);
        }

        timer += Time.deltaTime;

        if (timer > waitTime)
        {
            curState = FrogState.WhilwJump;
        }
    }

    private void UpdateWhileJump()
    {
        if(curState != prevState)
        {
            prevState = curState;
        }

        if (isGrounded)
        {
            curState = FrogState.Idle;
        }
    }

    /// <summary>
    /// �W�����v������������肷��
    /// </summary>
    private void DecideJumpDIr()
    {
        sumDisplacement.x += mouseDisplacement.x * param.cameraSensitivity.x;
        sumDisplacement.y -= mouseDisplacement.y * param.cameraSensitivity.y;
        if (Mathf.Abs(sumDisplacement.y) > param.limitOfVerticalRotation)
        {
            sumDisplacement.y = Mathf.Sign(sumDisplacement.y) * param.limitOfVerticalRotation;
        }

        float rotationY = Mathf.Clamp(sumDisplacement.y - adjustJumpDir, -jumpAngleLmt, jumpAngleLmt);
        Vector3 dir = Quaternion.Euler(rotationY, sumDisplacement.x, 0) * Vector3.forward;
        currentJumpDir = dir.normalized;
    }

    /// <summary>
    /// �W�����v�ҋ@���ɂ������O����������(����])
    /// </summary>
    private void TurnFwdSlowlyOnGround()
    {
        Vector3 cameraFwd = Camera.main.transform.forward;
        Vector3 myFwd = transform.forward;

        Vector3 planeFrom = Vector3.ProjectOnPlane(myFwd, Vector3.up);
        Vector3 planeTo = Vector3.ProjectOnPlane(cameraFwd, Vector3.up);

        float angle = Vector3.SignedAngle(planeFrom, planeTo, Vector3.up);

        Vector3 rotation = Vector3.zero;
        if(Mathf.Abs(angle) < angleRotateSpd * Time.deltaTime && angle != 0)
        {
            rotation.y = Mathf.Sign(angle) * (angleRotateSpd * Time.deltaTime - Mathf.Abs(angle));
        }
        else
        {
            rotation.y = Mathf.Sign(angle) * angleRotateSpd * Time.deltaTime;
        }

        transform.Rotate(rotation);
    }

    /// <summary>
    /// �W�����v���O�ɑO����������
    /// </summary>
    private void TurnFwdQuickly()
    {
        transform.rotation = Quaternion.identity;

        Vector3 cameraFwd = Camera.main.transform.forward;
        Vector3 myFwd = transform.forward;

        Vector3 planeFrom = Vector3.ProjectOnPlane(myFwd, Vector3.up);
        Vector3 planeTo = Vector3.ProjectOnPlane(cameraFwd, Vector3.up);

        float angle = Vector3.SignedAngle(planeFrom, planeTo, Vector3.up);
        Vector3 rotation = Vector3.zero;
        rotation.y = angle;

        transform.Rotate(rotation);
    }
}
