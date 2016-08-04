﻿using UnityEngine;
using System.Collections;

public class Leg : Accessory
{
    /// <summary>
    /// パーツモーションの基準位置
    /// </summary>
    [SerializeField]
    protected Vector2 baseVector;
    /// <summary>
    /// パーツモーションの現在位置
    /// </summary>
    protected Vector2 nowPosition;
    /// <summary>
    /// パーツモーションの現在位置
    /// </summary>
    [SerializeField]
    protected float limitRange;

    /// <summary>
    ///付属パーツ系の基本動作
    /// </summary>
    public override void accessoryMotion(Vector2 addVector, float correctionAngle = 0)
    {
        nowPosition = Vector2.right
           * ((addVector.x != 0) ? nowPosition.x + addVector.x : nowPosition.x * 9 / 10)
           + Vector2.up
           * ((addVector.y != 0) ? nowPosition.y + addVector.y : nowPosition.y * 9 / 10);
        Quaternion correctionRotation = Quaternion.Euler(0, 0, correctionAngle);

        nowPosition = MathV.Min(nowPosition, limitRange);
        setManipulatePosition(correctionRotation * (baseVector + nowPosition), false);
    }
}
