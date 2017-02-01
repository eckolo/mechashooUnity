﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// プレイヤーの操作機体クラス
/// </summary>
public class Player : Ship
{
    /// <summary>
    ///操作可能フラグ
    /// </summary>
    public bool canRecieveKey
    {
        get {
            if(onPause) return false;
            return _canRecieveKey;
        }
        set {
            _canRecieveKey = value;
            if(!value)
            {
                setVerosity(Vector2.zero);
                actionRight = false;
                actionLeft = false;
                actionBody = false;
                setParamate();
            }
        }
    }
    bool _canRecieveKey = false;
    /// <summary>
    ///デフォルト画像
    /// </summary>
    [SerializeField]
    private Sprite defaultImage = null;

    /// <summary>
    ///各種アクションのフラグ
    /// </summary>
    private bool actionRight = false;
    private bool actionLeft = false;
    private bool actionBody = false;

    public override void Start()
    {
        if(GetComponent<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if(canRecieveKey)
        {
            if(armorBar != null) armorBar.setAlpha(1);
        }
        else
        {
            if(armorBar != null) armorBar.setAlpha(0);
        }
        displayAlignmentEffect = canRecieveKey;

        keyAction();

        invertWidth(siteAlignment.x);

        if(sys.playerHPbar != null)
        {
            var hpLanges = sys.playerHPbar.setLanges(palamates.nowArmor, palamates.maxArmor + palamates.maxBarrier, viewSize.x, pibotView: true);
            var hpright = Vector2.right * hpLanges.x;

            if(sys.playerBRbar != null) sys.playerBRbar.setLanges(palamates.nowBarrier, palamates.maxArmor + palamates.maxBarrier, viewSize.x, hpright, true);

            if(sys.playerENbar != null) sys.playerENbar.setLanges(palamates.nowFuel, palamates.maxFuel, viewSize.x, Vector2.down * hpLanges.y, true);
        }
    }

    public override void selfDestroy(bool system = false)
    {
        transparentPlayer();
    }

    /// <summary>
    /// プレイヤーが初期状態か否かの判定関数
    /// </summary>
    public bool isInitialState
    {
        get {
            if(coreData == null) return true;
            if(coreData.image == null) return true;
            if(coreData.image.name == null) return true;
            return coreData.image.name == defaultImage.name;
        }
    }

    private void keyAction()
    {
        if(!canRecieveKey) return;

        // 移動する向きを求める
        Vector2 direction = new Vector2(keyValueX, keyValueY).normalized;
        // 移動する速度を求める
        float targetSpeed = Input.GetKey(Buttom.Sub) ? lowerSpeed : maximumSpeed;
        // 移動
        exertPower(direction, reactPower, targetSpeed);

        if(arms.Count >= 1) actionRight = handAction(getHand(arms[0]), actionRight, Buttom.Z);
        if(arms.Count >= 2) actionLeft = handAction(getHand(arms[1]), actionLeft, Buttom.X);

        if(Input.GetKeyDown(Buttom.C)) actionBody = !actionBody;
        if(actionBody)
        {
            foreach(var weapon in bodyWeapons)
            {
                if(weapon == null) continue;
                weapon.action(Weapon.ActionType.FIXED);
            }
        }

        manipulateAim();
    }
    float keyValueX
    {
        get {
            return toInt(Buttom.Right) - toInt(Buttom.Left);
        }
    }
    float keyValueY
    {
        get {
            return toInt(Buttom.Up) - toInt(Buttom.Down);
        }
    }

    private bool handAction(Hand actionHand, bool actionNow, KeyCode keyMain)
    {
        if(actionHand != null)
        {
            if(Input.GetKeyDown(keyMain))
            {
                actionNow = !actionNow;
                if(!actionNow) actionHand.actionWeapon(Weapon.ActionType.NOMOTION);
                if(Input.GetKey(Buttom.Sub))
                {
                    actionHand.actionWeapon(Weapon.ActionType.SINK);
                    actionNow = false;
                }
            }
            if(actionNow) actionHand.actionWeapon();
        }
        return actionNow;
    }
    private Vector2 manipulateAim()
    {
        var difference = Vector2.zero;
        if(Configs.AimingWsad)
        {
            difference += Vector2.up * toInt(Buttom.W);
            difference += Vector2.down * toInt(Buttom.S);
            difference += Vector2.left * toInt(Buttom.A);
            difference += Vector2.right * toInt(Buttom.D);
        }
        if(Configs.AimingShift && Input.GetKey(Buttom.Sub))
        {
            difference += Vector2.up * toInt(Buttom.Up);
            difference += Vector2.down * toInt(Buttom.Down);
            difference += Vector2.left * toInt(Buttom.Left);
            difference += Vector2.right * toInt(Buttom.Right);
        }

        siteAlignment = MathV.within(position + siteAlignment + difference * siteSpeed, fieldLowerLeft, fieldUpperRight) - position;
        var alignmentPosition = position + correctWidthVector(armRoot) + siteAlignment;
        viewPosition = alignmentPosition;
        return siteAlignment;
    }
}
