using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraScript : MonoBehaviour
{
    [Header("Elements")]
    [Tooltip("’Ç‚¢‚©‚¯‚éƒ^[ƒQƒbƒg")]
    [SerializeField] private Transform targetPos;
    [Tooltip("ƒJƒƒ‰‚Ìİ’è’l")]
    [SerializeField] private Parameters param;

    //ƒJƒƒ‰ˆÚ“®ŠÖŒW
    private Vector3 targetTrack = Vector3.zero;
    //ƒJƒƒ‰‰ñ“]ŠÖŒW
    private Vector2 mouseDisplacement = Vector2.zero;
    private Vector2 sumDisplacement = Vector2.zero;
    private Vector3 rotation = Vector3.zero;
    private float distance = 0.0f;

    //“ü—Íóæ
    public void OnMouseMove(InputAction.CallbackContext context)
    {
        mouseDisplacement = context.ReadValue<Vector2>();
    }

    private void LateUpdate()
    {
        CameraRotate();
    }

    private void FixedUpdate()
    {
        targetTrack = Vector3.Lerp(
            targetTrack, targetPos.position, Time.deltaTime * 10);
    }

    /// <summary>
    /// ƒJƒƒ‰‚Ì‰ñ“]ˆ—
    /// </summary>
    private void CameraRotate()
    {
        sumDisplacement.x += mouseDisplacement.x * param.cameraSensitivity.x;
        sumDisplacement.y -= mouseDisplacement.y * param.cameraSensitivity.y;
        if (Mathf.Abs(sumDisplacement.y) > param.limitOfVerticalRotation)
        {
            sumDisplacement.y = Mathf.Sign(sumDisplacement.y) * param.limitOfVerticalRotation;
        }

        rotation = Vector3.Normalize(new Vector3(0, 0.2f, -5));
        rotation = Quaternion.Euler(sumDisplacement.y, sumDisplacement.x, 0) * rotation;

        CameraPreventToSink();

        transform.rotation = Quaternion.Euler(sumDisplacement.y, sumDisplacement.x, 0);

        transform.position = targetPos.position;
        transform.position += rotation * distance;

        Vector3 vNeckLevel = Vector3.up * param.neckLevel;
        transform.position += vNeckLevel;
    }

    /// <summary>
    /// ƒJƒƒ‰‚Ì‚ß‚è‚İ–h~ˆ—
    /// </summary>
    private void CameraPreventToSink()
    {
        RaycastHit hit;
        int layermask = 1 << 6;
        distance = param.distanceBase;

        if(Physics.SphereCast(targetPos.position + Vector3.up * param.neckLevel, 0.1f, rotation, out hit, distance, layermask))
        {
            distance = hit.distance;
            Debug.Log(hit.distance);
        }
    }
}
