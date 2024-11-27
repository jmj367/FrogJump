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
    [Tooltip("�v���C���[�A�j���[�V����")]
    [SerializeField] private PlayerAnimation pAnim;
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
    private float curJumpPower = 0;
    private float diffJumpPower = 0;
    private bool isGrounded = false;
    private Vector2 sumDispl = Vector2.zero;
    private Vector3 curJumpDir = Vector3.zero;
    private Vector3 curSpeed = Vector3.zero;

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
                curSpeed.y *= -1;
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
        DecideJumpDir();

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
            transform.position += curSpeed * Time.deltaTime;
            curSpeed.y -= gravity * Time.deltaTime;
        }
    }

    //�X�e�[�g����Update
    private void UpdateIdle()
    {
        //Start
        if(curState != prevState)
        {
            prevState = curState;
        }

        //Process

        //End
        if (isJumpStart)
        {
            prevState = curState;
            curSpeed = Vector3.zero;
            curState = FrogState.JumpStart;
        }
    }

    private void UpdateJumpStart()
    {
        //Start
        if (curState != prevState)
        {
            prevState = curState;
            curJumpPower = minJumpPower;
            diffJumpPower = maxJumpPower - minJumpPower;
            trajectorySim.SetIsSim(true);
        }

        //Process
        TurnFwdSlowlyOnGround();
        curJumpPower += diffJumpPower * Time.deltaTime / boostJumpTime;
        if(curJumpPower > maxJumpPower)
        {
            curJumpPower = maxJumpPower;
        }

        trajectorySim.SetValue(transform.position, curJumpDir * curJumpPower, gravity);

        //End
        if (!isJumpStart)
        {
            curState = FrogState.Jump;
        }
    }

    private void UpdateJump()
    {
        //Start
        if (curState != prevState)
        {
            prevState = curState;
            TurnFwdQuickly();
            curSpeed = curJumpDir * curJumpPower;
            isGrounded = false;
            timer = 0;
            trajectorySim.SetIsSim(false);
        }

        //Process
        timer += Time.deltaTime;

        //End
        if (timer > waitTime)
        {
            curState = FrogState.WhilwJump;
        }
    }

    private void UpdateWhileJump()
    {
        //Start
        if(curState != prevState)
        {
            prevState = curState;
        }

        //Process

        //End
        if (isGrounded)
        {
            curState = FrogState.Idle;
        }
    }

    /// <summary>
    /// �W�����v������������肷��
    /// </summary>
    private void DecideJumpDir()
    {
        //�J�����̈ʒu�̍���ێ�(y�͈ړ��ʂƋt�̕����ɉ�]������̂�-)
        sumDispl.x += mouseDisplacement.x * param.cameraSensitivity.x;
        sumDispl.y -= mouseDisplacement.y * param.cameraSensitivity.y;
        //�J�����̈ʒu�̍���y���㉺�̌��E�𒴂��Ȃ��悤�ɂ���
        if (Mathf.Abs(sumDispl.y) > param.limitOfVerticalRotation)
        {
            sumDispl.y = Mathf.Sign(sumDispl.y) * param.limitOfVerticalRotation;
        }

        //�J�����ʒu�̍���y + �����p�ϐ������E�𒴂��Ȃ��悤�ɂ���
        float rotationY = Mathf.Clamp(sumDispl.y - adjustJumpDir, -jumpAngleLmt, jumpAngleLmt);
        //��]
        Vector3 dir = Quaternion.Euler(rotationY, sumDispl.x, 0) * Vector3.forward;
        curJumpDir = dir.normalized;
    }

    /// <summary>
    /// �W�����v�ҋ@���ɂ������O����������
    /// </summary>
    private void TurnFwdSlowlyOnGround()
    {
        //�J�����̑O�����Ǝ��g�̑O�����̊p�x�̍������߂�
        Vector3 cameraFwd = Camera.main.transform.forward;
        Vector3 myFwd = transform.forward;
        //transform.up�����Ƃ��Čv�Z
        Vector3 planeFrom = Vector3.ProjectOnPlane(myFwd, transform.up);
        Vector3 planeTo = Vector3.ProjectOnPlane(cameraFwd, transform.up);

        float angle = Vector3.SignedAngle(planeFrom, planeTo, transform.up);

        //��葬�x�ł̉�]����
        Vector3 rotation = Vector3.zero;
        if(Mathf.Abs(angle) < angleRotateSpd * Time.deltaTime && angle != 0)
        {
            rotation = transform.up * Mathf.Sign(angle) * (angleRotateSpd * Time.deltaTime - Mathf.Abs(angle));
        }
        else
        {
            rotation = transform.up * Mathf.Sign(angle) * angleRotateSpd * Time.deltaTime;
        }

        transform.Rotate(rotation, Space.World);
    }

    /// <summary>
    /// �W�����v���O�ɑO����������
    /// </summary>
    private void TurnFwdQuickly()
    {
        //�������ɂ��Ȃ���O����������
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
