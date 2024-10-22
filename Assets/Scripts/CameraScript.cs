using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CameraScript : MonoBehaviour
{
    [Header("Elements")]
    [Tooltip("�ǂ�������^�[�Q�b�g")]
    [SerializeField] private Transform targetPos;
    [Tooltip("�J�����̐ݒ�l")]
    [SerializeField] private Parameters param;

    //�J�����ړ��֌W
    private Vector3 targetTrack = Vector3.zero;
    //�J������]�֌W
    private Vector2 mouseDisplacement = Vector2.zero;
    private Vector2 sumDisplacement = Vector2.zero;
    private Vector3 rotation = Vector3.zero;
    private float distance = 0.0f;

    //���͎��
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
    /// �J�����̉�]����
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
    /// �J�����̂߂荞�ݖh�~����
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
