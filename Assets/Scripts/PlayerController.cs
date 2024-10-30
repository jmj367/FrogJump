using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Tooltip("�J�����̐ݒ�l")]
    [SerializeField] private Parameters param;
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

    //���͒l
    private bool isJumpStart = false;
    private Vector2 mouseDisplacement = Vector2.zero;

    private Rigidbody rb;
    //�W�����v�֘A
    private float currentJumpPower = 0;
    private float defferenceJumpPower = 0;
    private bool isGrounded = false;
    private Vector2 sumDisplacement = Vector2.zero;
    private Vector3 currentJumpDir = Vector3.zero;
    private Vector3 currentSpeed = Vector3.zero;

    //�X�e�[�g�֘A
    private bool isChangeState = false;
    private enum FrogState
    {
        Idle,
        JumpStart,
        Jump
    }
    private FrogState currentState = FrogState.Idle;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (currentState == FrogState.Jump)
        {
            isGrounded = true;
            Vector3 nor = collision.contacts[0].normal;
            Stick(nor);
        }
    }

    /// <summary>
    /// �ǁA�V��ɕ��������鏈��
    /// </summary>
    /// <param name="nor"></param>
    private void Stick(Vector3 nor)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, nor);
        transform.Rotate(rotation.eulerAngles, Space.World);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        isGrounded = true;
        
    }

    private void Update()
    {
        DecideJumpDIr();

        switch (currentState)
        {
            case FrogState.Idle: UpdateIdle(); break;
            case FrogState.JumpStart: UpdateJumpStart(); break;
            case FrogState.Jump: UpdateJump(); break;
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
        if (isJumpStart)
        {
            isChangeState = true;
            currentSpeed = Vector3.zero;
            currentState = FrogState.JumpStart;
        }
    }

    private void UpdateJumpStart()
    {
        if (isChangeState)
        {
            isChangeState = false;
            currentJumpPower = minJumpPower;
            defferenceJumpPower = maxJumpPower - minJumpPower;
        }

        TurnFwdSlowlyOnGround();
        currentJumpPower += defferenceJumpPower * Time.deltaTime / boostJumpTime;
        if(currentJumpPower > maxJumpPower)
        {
            currentJumpPower = maxJumpPower;
        }

        if (!isJumpStart)
        {
            isChangeState = true;
            currentState = FrogState.Jump;
        }
    }

    private void UpdateJump()
    {
        if (isChangeState)
        {
            isChangeState = false;
            isGrounded = false;
            TurnFwdQuickly();
            currentSpeed = currentJumpDir * currentJumpPower;
        }

        if (isGrounded)
        {
            currentState = FrogState.Idle;
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
