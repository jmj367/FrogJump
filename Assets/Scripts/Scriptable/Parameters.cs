using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName ="ScriptableObjects/Parameters")]
public class Parameters : ScriptableObject
{
    [Tooltip("Target�ƃJ�����̋���")]
    public float distanceBase = 2.0f;
    [Tooltip("Target�ƃJ�����̏�����̊Ԋu")]
    public float neckLevel = 0.5f;
    [Tooltip("�J�������㉺�Ɍ�������")]
    public float limitOfVerticalRotation = 90.0f;
    [Tooltip("�J�����̊��x")]
    public Vector2 cameraSensitivity = new Vector2(0.5f, 0.5f);
}
