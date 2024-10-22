using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName ="ScriptableObjects/Parameters")]
public class Parameters : ScriptableObject
{
    [Tooltip("Targetとカメラの距離")]
    public float distanceBase = 2.0f;
    [Tooltip("Targetとカメラの上方向の間隔")]
    public float neckLevel = 0.5f;
    [Tooltip("カメラが上下に向ける上限")]
    public float limitOfVerticalRotation = 90.0f;
    [Tooltip("カメラの感度")]
    public Vector2 cameraSensitivity = new Vector2(0.5f, 0.5f);
}
