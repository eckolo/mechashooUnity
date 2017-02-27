﻿using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

/// <summary>
/// 近接タイプの武装クラス
/// </summary>
public partial class Sword : Weapon
{
    [SerializeField]
    protected enum AttackType
    {
        SINGLE,
        NIFE,
        SPEAR,
        SPIN
    }
    private Dictionary<AttackType, IMotion<Sword>> _motionList = new Dictionary<AttackType, IMotion<Sword>>();
    protected Dictionary<AttackType, IMotion<Sword>> motionList
    {
        get {
            if(_motionList.Count <= 0)
            {
                _motionList.Add(AttackType.SINGLE, new OneShot());
                _motionList.Add(AttackType.NIFE, new Nife());
                _motionList.Add(AttackType.SPEAR, new Spear());
                _motionList.Add(AttackType.SPIN, new Spin());
            }
            return _motionList;
        }
    }

    /// <summary>
    /// 通常時モーション
    /// </summary>
    [SerializeField]
    protected AttackType nomalAttack = AttackType.SINGLE;
    /// <summary>
    /// Shiftモーション
    /// </summary>
    [SerializeField]
    protected AttackType sinkAttack = AttackType.SINGLE;
    /// <summary>
    /// 固定時モーション
    /// </summary>
    [SerializeField]
    protected AttackType fixedAttack = AttackType.SINGLE;
    /// <summary>
    /// NPC限定モーション
    /// </summary>
    [SerializeField]
    protected AttackType npcAttack = AttackType.SINGLE;

    /// <summary>
    /// 振り上げ時効果音
    /// </summary>
    [SerializeField]
    protected AudioClip swingUpSE = null;
    /// <summary>
    /// 振り下ろし時効果音
    /// </summary>
    [SerializeField]
    protected AudioClip swingDownSE = null;

    public float defaultSlashSize = 1;

    protected override IEnumerator motion(int actionNum)
    {
        yield return motionList[getAttackType(nowAction)].mainMotion(this);
        yield break;
    }
    protected override IEnumerator endMotion(int actionNum)
    {
        if(nextAction == nowAction) yield break;
        yield return motionList[getAttackType(nowAction)].endMotion(this);
        yield break;
    }

    AttackType getAttackType(ActionType action)
    {
        switch(action)
        {
            case ActionType.NOMAL: return nomalAttack;
            case ActionType.SINK: return sinkAttack;
            case ActionType.FIXED: return fixedAttack;
            case ActionType.NPC: return npcAttack;
            default: return AttackType.SINGLE;
        }
    }

    /// <summary>
    /// 汎用斬撃発生関数
    /// </summary>
    public void slash(float slashSize = 1)
    {
        foreach(var injection in onTypeInjections)
        {
            var finalSize = slashSize * defaultSlashSize * (1 + (injection.hole - selfConnection).magnitude);

            var slash = inject(injection).GetComponent<Slash>();
            if(slash == null) continue;

            slash.setParamate(finalSize);
        }
    }
}
