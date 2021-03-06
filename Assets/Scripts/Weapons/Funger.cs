﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ファンガークラス
/// </summary>
public partial class Funger : Weapon
{
    List<Injection> _nowInjections = null;
    public override List<Injection> nowInjections
    {
        get {
            if(_nowInjections == null)
            {
                foreach(var injection in fung1.nowInjections)
                {
                    injection.hole += fung1.parentConnection - fung1.selfConnection;
                }
                foreach(var injection in fung2.nowInjections)
                {
                    injection.hole += fung2.parentConnection - fung2.selfConnection;
                }
                _nowInjections = base.nowInjections
                .Concat(fung1.nowInjections)
                .Concat(fung2.nowInjections)
                .ToList();
            }
            return _nowInjections;
        }
    }
    /// <summary>
    /// 斬撃の規模
    /// </summary>
    [SerializeField]
    private float defaultSlashSize = 1;

    /// <summary>
    /// 振り時効果音
    /// </summary>
    [SerializeField]
    protected AudioClip swingSE = null;
    /// <summary>
    /// 噛み合わせ時効果音
    /// </summary>
    [SerializeField]
    protected AudioClip biteSE = null;

    [SerializeField]
    protected enum AttackType
    {
        BITE,
        BIGBITE,
        BITE_AND_SLASH
    }
    private Dictionary<AttackType, IMotion<Funger>> _motionList = new Dictionary<AttackType, IMotion<Funger>>();
    protected Dictionary<AttackType, IMotion<Funger>> motionList
    {
        get {
            if(_motionList.Count <= 0)
            {
                _motionList.Add(AttackType.BITE, new Bite());
                _motionList.Add(AttackType.BIGBITE, new BigBite());
                _motionList.Add(AttackType.BITE_AND_SLASH, new BiteAndSlash());
            }
            return _motionList;
        }
    }

    /// <summary>
    /// 通常時モーション
    /// </summary>
    [SerializeField]
    protected AttackType nomalAttack = AttackType.BITE;
    /// <summary>
    /// Shiftモーション
    /// </summary>
    [SerializeField]
    protected AttackType sinkAttack = AttackType.BITE;
    /// <summary>
    /// 固定時モーション
    /// </summary>
    [SerializeField]
    protected AttackType fixedAttack = AttackType.BITE;
    /// <summary>
    /// NPC限定モーション
    /// </summary>
    [SerializeField]
    protected AttackType npcAttack = AttackType.BITE;

    public override void Start()
    {
        base.Start();
        AttachThings();
        fung1.defaultSlashSize = defaultSlashSize;
        fung2.defaultSlashSize = defaultSlashSize;
    }

    protected Things AttachThings()
    {
        foreach(var component in GetComponents<Things>()) Destroy(component);
        var things = gameObject.AddComponent<Things>();

        things.heightPositive = heightPositive;
        things.ableEnter = false;
        things.isSolid = false;
        things.Start();

        return things;
    }

    protected override IEnumerator BeginMotion(int actionNum)
    {
        fung1.SetActionType(nowAction);
        fung2.SetActionType(nowAction);

        yield return motionList[GetAttackType(nowAction)].BeginMotion(this);

        yield break;
    }
    protected override IEnumerator Motion(int actionNum)
    {
        fung1.SetActionType(nowAction);
        fung2.SetActionType(nowAction);

        yield return motionList[GetAttackType(nowAction)].MainMotion(this);
        yield break;
    }
    protected override IEnumerator EndMotion(int actionNum)
    {
        fung1.SetActionType(nowAction);
        fung2.SetActionType(nowAction);

        yield return motionList[GetAttackType(nowAction)].EndMotion(this);
        yield break;
    }

    AttackType GetAttackType(ActionType action)
    {
        switch(action)
        {
            case ActionType.NOMAL: return nomalAttack;
            case ActionType.SINK: return sinkAttack;
            case ActionType.FIXED: return fixedAttack;
            case ActionType.NPC: return npcAttack;
            default: return AttackType.BITE;
        }
    }

    /// <summary>
    /// 噛み付き動作
    /// </summary>
    /// <param name="timePar">所要時間比率</param>
    /// <param name="power">斬撃威力（サイズ）比率</param>
    /// <returns></returns>
    protected IEnumerator Engage(float timePar = 1, float power = 1)
    {
        SoundSE(swingSE);
        var limit = (int)(timeRequired * timePar);
        var startAngle1 = fung1.nowLocalAngle;
        var startAngle2 = fung2.nowLocalAngle;
        for(int time = 0; time < limit; time++)
        {
            SetEngage(startAngle1, startAngle2, time, limit);
            yield return Wait(1);
        }

        SoundSE(biteSE);
        fung1.Slash(power);
        fung2.Slash(power);

        yield break;
    }
    protected void SetEngage(float startAngle1, float startAngle2, int time, int limit)
    {
        fung1.SetAngle(startAngle1 - Easing.quintic.In(startAngle1, time, limit - 1));
        fung2.SetAngle(startAngle2 + Easing.quintic.In(360 - startAngle2, time, limit - 1));
        return;
    }

    /// <summary>
    /// 噛み付き状態からの戻り動作
    /// </summary>
    /// <param name="timePar">所要時間比率</param>
    /// <returns></returns>
    protected IEnumerator Reengage(float timePar = 1)
    {
        var limit = (int)(timeRequired * 2 * timePar);
        yield return Wait(limit);
        for(int time = 0; time < limit; time++)
        {
            SetReEngage(time, limit);
            yield return Wait(1);
        }

        yield break;
    }
    protected void SetReEngage(int time, int limit)
    {
        fung1.SetAngle(Easing.liner.In(fung1.defAngle, time, limit - 1));
        fung2.SetAngle(-Easing.liner.In(fung2.defAngle, time, limit - 1));
        return;
    }

    protected Fung fung1 => fungs.First();
    protected Fung fung2 => fungs.Last();
    protected List<Fung> fungs
    {
        get {
            var things = GetComponent<Things>();
            if(!_fungs.Any()) _fungs = things.getPartsList
                    .Select(parts => parts.GetComponent<Fung>())
                    .Take(2)
                    .ToList();
            return _fungs;
        }
    }
    List<Fung> _fungs = new List<Fung>();
}
