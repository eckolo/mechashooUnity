﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract partial class Methods : MonoBehaviour
{
    /// <summary>
    /// フィールドサイズ
    /// </summary>
    protected static Vector2 fieldArea
    {
        get {
            if(sys.nowStage == null) return viewSize * 2;
            return viewSize.Scaling(sys.nowStage.fieldSize);
        }
    }
    /// <summary>
    /// フィールド左下端
    /// </summary>
    protected static Vector2 fieldLowerLeft => -fieldArea / 2;
    /// <summary>
    /// フィールド右上端
    /// </summary>
    protected static Vector2 fieldUpperRight => fieldArea / 2;
    /// <summary>
    /// フィールド視野サイズ
    /// </summary>
    public static Vector2 viewSize
    {
        get {
            return Camera.main.ViewportToWorldPoint(Vector2.one) - Camera.main.ViewportToWorldPoint(Vector2.zero);
        }
    }
    /// <summary>
    /// フィールド視点位置
    /// </summary>
    protected static Vector2 viewPosition
    {
        get {
            return Camera.main.transform.localPosition;
        }
        set {
            var edge = (fieldArea - viewSize) / 2;
            Vector3 setPosition = value.Within(-edge, edge);
            setPosition.z = 0;
            Camera.main.transform.localPosition = setPosition;
            setPosition.z = 1;
            sysView.transform.localPosition = setPosition;
        }
    }
    /// <summary>
    /// ピクセル単位のキャンバスサイズ
    /// </summary>
    protected static Vector2 screenSize
    {
        get {
            return sysCanvas.GetComponent<CanvasScaler>().referenceResolution;
        }
    }
    /// <summary>
    /// 1マス当たりのピクセルサイズ
    /// </summary>
    public static Vector2 baseMas
    {
        get {
            return screenSize.Rescaling(viewSize);
        }
    }
}
