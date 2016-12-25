﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 概ね当たり判定を持つ物体全般
/// </summary>
public class Things : Materials
{
    /// <summary>
    ///制御下のPartsリスト
    /// </summary>
    private List<Parts> childPartsList = new List<Parts>();

    /// <summary>
    ///画面内に位置強制するフラグ
    /// </summary>
    [SerializeField]
    private bool forcedScreen = false;

    /// <summary>
    ///物体重量
    /// </summary>
    [SerializeField]
    private float _weight = 1;
    public float weight
    {
        get
        {
            return _weight;
        }
        protected set
        {
            _weight = value;
        }
    }

    protected override void updateMotion()
    {
        updatePosition();
        base.updateMotion();
    }

    public override void Start()
    {
        base.Start();
        attachPolygonCollider();
        foreach(Transform child in transform)
        {
            var childParts = child.GetComponent<Parts>();
            if(childParts != null) setParts(childParts);
        }
        foreach(var childParts in childPartsList) childParts.setParent(this);
    }

    public int setParts(Parts setedParts)
    {
        if(setedParts == null) return -1;

        childPartsList.Add(setedParts);
        setedParts.setParent(gameObject.GetComponent<Things>());

        return childPartsList.Count - 1;
    }
    public Parts getParts(int sequenceNum)
    {
        if(sequenceNum < 0) return null;
        if(sequenceNum >= childPartsList.Count) return null;

        return childPartsList[sequenceNum];
    }
    public List<Parts> getPartsList
    {
        get
        {
            return childPartsList;
        }
    }
    public int partsListCount
    {
        get
        {
            return childPartsList.Count;
        }
    }

    /// <summary>
    /// PolygonCollider2Dコンポーネントをアタッチするだけの関数
    /// </summary>
    protected PolygonCollider2D attachPolygonCollider()
    {
        if(GetComponent<PolygonCollider2D>() != null) Destroy(GetComponent<PolygonCollider2D>());
        var collider = gameObject.AddComponent<PolygonCollider2D>();

        collider.isTrigger = true;

        return collider;
    }

    /// <summary>
    ///オブジェクトが可動範囲内にいるかどうか
    /// </summary>
    protected bool inField
    {
        get
        {
            if(transform.position.x < fieldLowerLeft.x) return false;
            if(transform.position.x > fieldUpperRight.x) return false;
            if(transform.position.y < fieldLowerLeft.y) return false;
            if(transform.position.y > fieldUpperRight.y) return false;
            return true;
        }
    }

    /// <summary>
    ///強制停止関数
    /// </summary>
    public void stopMoving(float? acceleration = null)
    {
        if(acceleration == null) nowSpeed = Vector2.zero;
        else setVerosity(nowSpeed, 0, acceleration);
    }
    /// <summary>
    ///オブジェクトへ力を掛ける関数
    /// </summary>
    public Vector2 exertPower(Vector2 direction, float power, float targetSpeed)
    {
        return setVerosity(direction, targetSpeed, power / weight);
    }
    /// <summary>
    ///オブジェクトの移動関数
    /// </summary>
    public Vector2 setVerosity(Vector2 verosity, float speed, float? acceleration = null)
    {
        Vector2 originSpeed = nowSpeed;
        Vector2 degree = (verosity.normalized * speed) - originSpeed;
        float variation = degree.magnitude != 0
            ? Mathf.Clamp((acceleration ?? degree.magnitude) / degree.magnitude, -1, 1)
            : 0;

        // 実移動量を計算
        var innerVerosity = originSpeed + degree * variation;

        if(forcedScreen)
        {
            innerVerosity.x = Mathf.Clamp(
                innerVerosity.x,
                (fieldLowerLeft.x - transform.position.x) * baseMas.x,
                (fieldUpperRight.x - transform.position.x) * baseMas.x);
            innerVerosity.y = Mathf.Clamp(
                innerVerosity.y,
                (fieldLowerLeft.y - transform.position.y) * baseMas.y,
                (fieldUpperRight.y - transform.position.y) * baseMas.y);
        }

        //速度設定
        nowSpeed = innerVerosity;

        //移動時アクション呼び出し
        setVerosityAction(nowSpeed - originSpeed);
        return nowSpeed;
    }
    protected virtual void setVerosityAction(Vector2 acceleration) { }
    public Vector2 nowSpeed { private set; get; }
    void updatePosition()
    {
        transform.localPosition += (Vector3)MathV.rescaling(nowSpeed, baseMas);
    }

    /// <summary>
    ///最寄りの非味方機体検索関数
    /// </summary>
    protected Ship nowNearTarget
    {
        get
        {
            Terms term = target
                => target.GetComponent<Ship>() != null
                && target.gameObject.layer != gameObject.layer;
            List<Materials> shipList = getNearObject(term);

            if(shipList.Count <= 0) return null;
            return shipList[0].GetComponent<Ship>();
        }
    }

    /// <summary>
    ///PartsListの削除関数
    ///引数無しで全消去
    /// </summary>
    public void deleteParts(int? sequenceNum = null)
    {
        if(sequenceNum != null) deleteSimpleParts((int)sequenceNum);
        for(int partsNum = 0; partsNum < childPartsList.Count; partsNum++)
        {
            deleteSimpleParts(partsNum);
        }
        childPartsList = new List<Parts>();
    }
    /// <summary>
    ///PartsListから指定した番号のPartsを削除する
    /// </summary>
    private void deleteSimpleParts(int sequenceNum)
    {
        if(sequenceNum < 0) return;
        if(sequenceNum >= childPartsList.Count) return;

        childPartsList[sequenceNum].selfDestroy();
        childPartsList[sequenceNum] = null;
    }
}
