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
    [Tooltip("プレイヤーアニメーション")]
    [SerializeField] private PlayerAnimation pAnim;
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

    //ジャンプ関連
    private float curJumpPower = 0;
    private float diffJumpPower = 0;
    private bool isGrounded = false;
    private Vector2 sumDispl = Vector2.zero;
    private Vector3 curJumpDir = Vector3.zero;
    private Vector3 curSpeed = Vector3.zero;

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
    /// ジャンプする方向を決定する
    /// </summary>
    private void DecideJumpDir()
    {
        //カメラの位置の差を保持(yは移動量と逆の方向に回転させるので-)
        sumDispl.x += mouseDisplacement.x * param.cameraSensitivity.x;
        sumDispl.y -= mouseDisplacement.y * param.cameraSensitivity.y;
        //カメラの位置の差のyを上下の限界を超えないようにする
        if (Mathf.Abs(sumDispl.y) > param.limitOfVerticalRotation)
        {
            sumDispl.y = Mathf.Sign(sumDispl.y) * param.limitOfVerticalRotation;
        }

        //カメラ位置の差のy + 調整用変数が限界を超えないようにする
        float rotationY = Mathf.Clamp(sumDispl.y - adjustJumpDir, -jumpAngleLmt, jumpAngleLmt);
        //回転
        Vector3 dir = Quaternion.Euler(rotationY, sumDispl.x, 0) * Vector3.forward;
        curJumpDir = dir.normalized;
    }

    /// <summary>
    /// ジャンプ待機時にゆっくり前を向く処理
    /// </summary>
    private void TurnFwdSlowlyOnGround()
    {
        //カメラの前方向と自身の前方向の角度の差を求める
        Vector3 cameraFwd = Camera.main.transform.forward;
        Vector3 myFwd = transform.forward;
        //transform.upを軸として計算
        Vector3 planeFrom = Vector3.ProjectOnPlane(myFwd, transform.up);
        Vector3 planeTo = Vector3.ProjectOnPlane(cameraFwd, transform.up);

        float angle = Vector3.SignedAngle(planeFrom, planeTo, transform.up);

        //一定速度での回転処理
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
    /// ジャンプ直前に前を向く処理
    /// </summary>
    private void TurnFwdQuickly()
    {
        //足を下にしながら前を向かせる
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
