using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Tooltip("カメラの設定値")]
    [SerializeField] private Parameters param;
    [Tooltip("弾道予測シミュレーター")]
    [SerializeField] private TrajectotySimulator trajectorySim;
    [Tooltip("最小のジャンプ力")]
    [SerializeField] private float minJumpPower = 8;
    [Tooltip("最大のジャンプ力")]
    [SerializeField] private float maxJumpPower = 13;
    [Tooltip("ジャンプ力が最大になるまでの時間(秒)")]
    [SerializeField] private float boostJumpTime = 2;
    [Tooltip("ジャンプ方向の上方向の調整値")]
    [SerializeField] private float adjustJumpDir = 30;
    [Tooltip("重力")]
    [SerializeField] private float gravity = 5;
    [Tooltip("ジャンプ角度の制限")]
    [SerializeField] private float jumpAngleLmt = 90;
    [Tooltip("ジャンプ待機時の1秒毎の回転(度)")]
    [SerializeField] private float angleRotateSpd = 90;
    [Tooltip("接地判定を行うまでの時間")]
    [SerializeField] private float waitTime = 0.1f;

    //入力値
    private bool isJumpStart = false;
    private Vector2 mouseDisplacement = Vector2.zero;

    //ジャンプスタート関連
    private Vector3 turnAxis = Vector3.zero;

    //ジャンプ関連
    private float currentJumpPower = 0;
    private float defferenceJumpPower = 0;
    private bool isGrounded = false;
    private Vector2 sumDisplacement = Vector2.zero;
    private Vector3 currentJumpDir = Vector3.zero;
    private Vector3 currentSpeed = Vector3.zero;

    //ステート関連
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

    //入力受取
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
                turnAxis = nor;
            }
        }
    }

    /// <summary>
    /// 壁、天井に腹を向ける処理
    /// </summary>
    /// <param name="nor"></param>
    private void Stick(Vector3 nor)
    {
        //プレイヤーの上方向を衝突オブジェクトの法線ベクトルに向ける
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, nor);
        transform.Rotate(rotation.eulerAngles, Space.World);
    }

    private void Start()
    {
        isGrounded = true;
        turnAxis = Vector3.up;
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

    //ステート毎のUpdate
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
            currentSpeed = Vector3.zero;
            curState = FrogState.JumpStart;
        }
    }

    private void UpdateJumpStart()
    {
        //Start
        if (curState != prevState)
        {
            prevState = curState;
            currentJumpPower = minJumpPower;
            defferenceJumpPower = maxJumpPower - minJumpPower;
            trajectorySim.SetIsSim(true);
        }

        //Process
        TurnFwdSlowlyOnGround();
        currentJumpPower += defferenceJumpPower * Time.deltaTime / boostJumpTime;
        if(currentJumpPower > maxJumpPower)
        {
            currentJumpPower = maxJumpPower;
        }

        trajectorySim.SetValue(transform.position, currentJumpDir * currentJumpPower, gravity);

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
            currentSpeed = currentJumpDir * currentJumpPower;
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
    /// ジャンプする方向を決定する
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
    /// ジャンプ待機時にゆっくり前を向く処理(横回転)
    /// </summary>
    private void TurnFwdSlowlyOnGround()
    {
        Vector3 cameraFwd = Camera.main.transform.forward;
        Vector3 myFwd = transform.forward;

        Vector3 planeFrom = Vector3.ProjectOnPlane(myFwd, turnAxis);
        Vector3 planeTo = Vector3.ProjectOnPlane(cameraFwd, turnAxis);

        float angle = Vector3.SignedAngle(planeFrom, planeTo, turnAxis);

        Vector3 rotation = Vector3.zero;
        if(Mathf.Abs(angle) < angleRotateSpd * Time.deltaTime && angle != 0)
        {
            rotation = turnAxis * Mathf.Sign(angle) * (angleRotateSpd * Time.deltaTime - Mathf.Abs(angle));
        }
        else
        {
            rotation = turnAxis * Mathf.Sign(angle) * angleRotateSpd * Time.deltaTime;
        }

        transform.Rotate(rotation, Space.World);
    }

    /// <summary>
    /// ジャンプ直前に前を向く処理
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
