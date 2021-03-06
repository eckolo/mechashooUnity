﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using System;
using System.Linq;

/// <summary>
/// NPC機体の制御クラス
/// </summary>
public class Npc : Ship
{
    /// <summary>
    /// 反応距離
    /// </summary>
    [SerializeField]
    private float _reactionDistance = 240;
    /// <summary>
    /// 装甲補正値
    /// </summary>
    [SerializeField]
    private float armorCorrectionRate = 0.5f;
    /// <summary>
    /// 障壁補正値
    /// </summary>
    [SerializeField]
    private float barrierCorrectionRate = 0.5f;
    /// <summary>
    /// 行動のテンポ
    /// </summary>
    [SerializeField]
    private int actionInterval = 100;
    /// <summary>
    /// 行動開始時のモーションを示す番号
    /// </summary>
    [SerializeField]
    private ActionPattern initialActionState = ActionPattern.NON_COMBAT;
    /// <summary>
    /// 専用BGM
    /// </summary>
    [SerializeField]
    public AudioClip privateBgm = null;

    /// <summary>
    /// 非戦闘時進路方向の角度指定
    /// </summary>
    public Vector2 normalCourse
    {
        get {
            return _normalCourse ?? (Vector2)(_normalCourse = nowForward);
        }
        set {
            _normalCourse = value;
        }
    }
    /// <summary>
    /// 非戦闘時進路方向の角度指定
    /// </summary>
    Vector2? _normalCourse = null;

    public override Vector2 baseAimPosition
    {
        get {
            if(wings.Any(wing => wing.rollable)) return normalCourse;
            return base.baseAimPosition;
        }
    }

    /// <summary>
    /// 反応距離
    /// </summary>
    protected float reactionDistance => (isReaction ? _reactionDistance * 2 : _reactionDistance) + (1 + shipLevel / 10);
    /// <summary>
    /// 反応状態
    /// </summary>
    protected bool isReaction
    {
        get {
            return nowActionState != ActionPattern.NON_COMBAT;
        }
        set {
            if(!value)
            {
                nowActionState = ActionPattern.NON_COMBAT;
            }
            else if(nowActionState == ActionPattern.NON_COMBAT)
            {
                if(!alreadyOnceReaction) sys.CountOpposeEnemy();
                alreadyOnceReaction = true;
                nowActionState = initialActionState;
            }
        }
    }
    public bool alreadyOnceReaction { get; private set; } = false;
    public bool alreadyOnceInField { get; private set; } = false;
    protected bool onAttack
    {
        get {
            if(!isReaction) return false;
            if(nowActionState == ActionPattern.ESCAPE) return false;
            return true;
        }
        set {
            if(!value && isReaction)
            {
                nowActionState = ActionPattern.ESCAPE;
            }
        }
    }

    protected int interval => Mathf.FloorToInt(Mathf.Max(actionInterval - shipLevel, 1));
    /// <summary>
    /// 最接近敵オブジェクトを取得orキャッシュ取得
    /// </summary>
    protected Ship nearTarget => _nearTarget ?? nowNearTarget ?? this;
    Ship _nearTarget = null;

    /// <summary>
    /// 画面内に位置強制するフラグ
    /// </summary>
    protected override bool forcedInScreen => onAttack;

    /// <summary>
    /// 最大装甲値
    /// </summary>
    public override float maxArmor => base.maxArmor * armorCorrectionRate * (onTheWay ? 0.8f : 1);
    /// <summary>
    /// 最大障壁値
    /// </summary>
    protected override float maxBarrier => base.maxBarrier * barrierCorrectionRate * (onTheWay ? 0.8f : 1);

    /// <summary>
    /// 射撃適正距離
    /// </summary>
    protected virtual float gunDistance => viewSize.x / 3;
    /// <summary>
    /// 格闘適正距離
    /// </summary>
    protected virtual float grappleDistance => spriteSize.x;
    /// <summary>
    /// 射撃を想定した待機位置
    /// </summary>
    protected virtual Vector2 standardPosition => nearTarget.position + Vector2.right * gunDistance * targetSign;
    /// <summary>
    /// 格闘を想定した待機位置
    /// </summary>
    protected virtual Vector2 approachPosition => nearTarget.position + Vector2.right * grappleDistance * targetSign;
    /// <summary>
    /// 攻撃目標が自身の左右どちらかにいるか符号
    /// →：1、←：-1
    /// </summary>
    protected int targetSign => (position.x - nearTarget.position.x).ToSign();
    /// <summary>
    /// 照準の平常時位置
    /// </summary>
    protected Vector2 standardAimPosition => position + baseAimPosition;

    /// <summary>
    /// 現在のモーションを示す番号
    /// </summary>
    public enum ActionPattern
    {
        NON_COMBAT,
        MOVE,
        AIMING,
        ATTACK,
        ESCAPE
    };

    /// <summary>
    /// 現在のモーションを示す番号
    /// </summary>
    protected ActionPattern nowActionState
    {
        get {
            return _nowActionState;
        }
        private set {
            _nowActionState = value;
            nextActionState = value;
        }
    }
    ActionPattern _nowActionState = ActionPattern.NON_COMBAT;
    /// <summary>
    /// 前回のモーションを示す番号
    /// </summary>
    protected ActionPattern preActionState { get; private set; }
    /// <summary>
    /// 次のモーション番号予約
    /// </summary>
    protected ActionPattern nextActionState { get; set; }
    /// <summary>
    /// 次のモーションの詳細識別番号
    /// </summary>
    protected int nextActionIndex { get; set; } = 0;
    /// <summary>
    /// モーションの切り替わりタイミングフラグ
    /// </summary>
    protected bool timingSwich { get; private set; } = true;
    /// <summary>
    /// 通りすがりモードフラグ
    /// </summary>
    public virtual bool onTheWay { get; set; } = false;

    /// <summary>
    /// 戦闘終了時限
    /// </summary>
    public int activityLimit { get; set; } = 0;
    const string NPC_TIMER_NAME = "NPC";

    /// <summary>
    /// 機体性能の基準値
    /// </summary>
    public float shipLevel
    {
        get {
            return Mathf.Log(_shipLevel + 1, 2) * (seriousMode ? 2 : 1);
        }
        set {
            _shipLevel = Mathf.Max(value, 0);
        }
    }
    /// <summary>
    /// 機体性能の基準値
    /// </summary>
    float _shipLevel = 1;

    /// <summary>
    /// 撃破時の獲得得点
    /// </summary>
    public int points = 0;

    public override void Start()
    {
        InvertWidth(false);
        InvertWidth(normalCourse);
        base.Start();
        timer.Start(NPC_TIMER_NAME);
    }

    public override void Update()
    {
        base.Update();
        Action(nextActionIndex);
        if(isDestroied) StopMotion();
        if(inField && !alreadyOnceInField) alreadyOnceInField = true;
    }

    protected void Escape()
    {
        nowActionState = ActionPattern.ESCAPE;
        preActionState = ActionPattern.ESCAPE;
        nowActionState = ActionPattern.ESCAPE;
    }

    public override bool Action(int? actionNum = null)
    {
        if(!timingSwich) return false;
        timingSwich = false;

        return base.Action(actionNum);
    }
    void StopMotion()
    {
        if(mainMotion != null) StopCoroutine(mainMotion);
        timingSwich = false;
    }
    protected override IEnumerator BaseMotion(int actionNum)
    {
        yield return base.BaseMotion(actionNum);

        if(!onAttack)
        {
            Aiming(position + baseAimPosition);
            ResetAllAim();
        }

        if(inField) isReaction = CaptureTarget(nowNearTarget);
        else if(!onAttack) isReaction = false;
        if(activityLimit > 0) onAttack = timer.Get(NPC_TIMER_NAME) < activityLimit;

        preActionState = nowActionState;
        nowActionState = nextActionState;
        timingSwich = true;

        yield break;
    }

    protected override void AutoClear()
    {
        if(!alreadyOnceInField) return;
        base.AutoClear();
    }

    IEnumerator mainMotion { get; set; } = null;
    protected override IEnumerator Motion(int actionNum)
    {
        _nearTarget = null;
        switch(nowActionState)
        {
            case ActionPattern.NON_COMBAT:
                yield return mainMotion = MotionNonCombat(actionNum);
                break;
            case ActionPattern.MOVE:
                yield return mainMotion = MotionMove(actionNum);
                break;
            case ActionPattern.AIMING:
                yield return mainMotion = MotionAiming(actionNum);
                break;
            case ActionPattern.ATTACK:
                yield return mainMotion = MotionAttack(actionNum);
                break;
            case ActionPattern.ESCAPE:
                yield return mainMotion = MotionEscape(actionNum);
                break;
            default:
                break;
        }

        yield break;
    }
    /// <summary>
    /// 非反応時行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns></returns>
    protected virtual IEnumerator MotionNonCombat(int actionNum)
    {
        Thrust(normalCourse, reactPower, (lowerSpeed + maximumSpeed) / 2);
        yield break;
    }
    /// <summary>
    /// 移動時行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns></returns>
    protected virtual IEnumerator MotionMove(int actionNum)
    {
        nextActionState = ActionPattern.NON_COMBAT;
        yield break;
    }
    /// <summary>
    /// 照準操作行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns></returns>
    protected virtual IEnumerator MotionAiming(int actionNum)
    {
        nextActionState = ActionPattern.NON_COMBAT;
        yield break;
    }
    /// <summary>
    /// 攻撃行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns></returns>
    protected virtual IEnumerator MotionAttack(int actionNum)
    {
        nextActionState = ActionPattern.NON_COMBAT;
        yield break;
    }
    /// <summary>
    /// 逃走時行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns></returns>
    protected virtual IEnumerator MotionEscape(int actionNum)
    {
        Thrust(normalCourse, reactPower, maximumSpeed);
        yield break;
    }

    protected override float selfPowerTotal => onAttack ? base.selfPowerTotal : 1;

    public override void DestroyMyself(bool system)
    {
        Debug.Log($"{displayName} Destroy.(system = {system})");
        if(!onTheWay && privateBgm != null)
        {
            var bgmEnemies = Stage.allEnemiesInField.Where(enemy => enemy.privateBgm != null);
            var setBGM = bgmEnemies.Any() ? bgmEnemies.FirstOrDefault()?.privateBgm : null;
            MainSystems.SetBGM(setBGM);
        }
        if(system) lastToHitShip = null;
        base.DestroyMyself(system);
    }
    /// <summary>
    /// 自身の削除実行関数
    /// </summary>
    protected override void ExecuteDestroy()
    {
        if(lastToHitShip?.GetComponent<Player>() != null) sys.CountShotsToKill();
        base.ExecuteDestroy();
    }

    protected override float siteSpeed
    {
        get {
            return base.siteSpeed + palamates.baseSiteSpeed * shipLevel;
        }
    }

    /// <summary>
    /// ダメージ受けた時の統一動作
    /// </summary>
    public override float ReceiveDamage(float damage, bool penetration = false, bool continuation = false)
    {
        Debug.Log($"{displayName} receive {damage}Damage.");
        isReaction = true;
        return base.ReceiveDamage(damage, penetration, continuation);
    }

    /// <summary>
    /// Shipの能動移動ラッパー関数
    /// normalCourseの更新込み
    /// </summary>
    /// <param name="direction">力のかかる方向</param>
    /// <param name="power">力の大きさ</param>
    /// <param name="targetSpeed">最終目標速度</param>
    /// <returns>結果速度</returns>
    public override Vector2 Thrust(Vector2 direction, float? power = null, float? targetSpeed = null)
    {
        var preSpeed = nowSpeed;
        var resultSpeed = base.Thrust(direction, power, targetSpeed);
        if(onAttack && resultSpeed.magnitude > preSpeed.magnitude) normalCourse = direction;
        return resultSpeed;
    }

    /// <summary>
    /// 本気モードフラグ
    /// </summary>
    protected virtual bool seriousMode
    {
        get {
            return palamates.nowArmor < maxArmor / 2;
        }
    }

    protected bool CaptureTarget(Things target, float? distance = null)
    {
        if(target == null) return false;
        return (target.position - position).Scaling(baseMas).magnitude <= (distance ?? reactionDistance);
    }

    /// <summary>
    /// 腕毎の照準位置を全体照準の位置にリセットする方向へ動かす
    /// </summary>
    /// <param name="armIndex">腕番号</param>
    /// <param name="siteSpeedTweak">照準移動速度補正</param>
    /// <returns>移動後の腕個別照準</returns>
    protected Vector2 ResetAim(int armIndex, float siteSpeedTweak = 1)
        => Aiming(position + siteAlignment, armIndex, siteSpeedTweak);
    /// <summary>
    /// 全腕の照準位置を全体照準の位置にリセットする方向へ動かす
    /// </summary>
    /// <param name="siteSpeedTweak">照準移動速度補正</param>
    protected void ResetAllAim(float siteSpeedTweak = 1)
    {
        for(int armIndex = 0; armIndex < armAlignments.Count; armIndex++)
        {
            ResetAim(armIndex, siteSpeedTweak);
        }
    }

    /// <summary>
    /// 偏差射撃の目標地点計算
    /// </summary>
    /// <typeparam name="Target">偏差射撃対象のクラス</typeparam>
    /// <param name="target">偏差射撃対象</param>
    /// <returns></returns>
    protected Vector2 GetDeviationTarget<Target>(Target target, float intensity = 1)
        where Target : Things
        => target.position + target.nowSpeed * Mathf.Log((target.position - position).Scaling(baseMas).magnitude + 2, 2) * intensity;

    /// <summary>
    /// 最適距離
    /// </summary>
    protected virtual float properDistance
    {
        get {
            return arms.Max(arm => arm.tipReach) * 2;
        }
    }
    /// <summary>
    /// 最適距離を維持するための移動目標地点の相対座標計算
    /// </summary>
    /// <param name="target">最適距離を維持したい対象</param>
    /// <param name="angleCorrection">目標と自身の直線状からの角度的なずれ（度）</param>
    /// <returns></returns>
    protected Vector2 GetProperPosition(Things target, float angleCorrection = 0)
        => GetProperPosition(target.position - position, angleCorrection);
    /// <summary>
    /// 最適距離を維持するための移動目標地点の相対座標計算
    /// </summary>
    /// <param name="direction">目的地の相対座標</param>
    /// <param name="angleCorrection">目的地と自身の直線状からの角度的なずれ（度）</param>
    /// <returns></returns>
    protected Vector2 GetProperPosition(Vector2 direction, float angleCorrection = 0)
    {
        var difference = -direction.ToVector(properDistance);
        var rotate = angleCorrection.ToRotation();

        return direction + (Vector2)(rotate * difference);
    }

    /// <summary>
    /// 攻撃予測照準表示関数
    /// 腕照準位置に出すバージョン
    /// </summary>
    /// <param name="armIndex">腕照準指定インデック</param>
    /// <returns></returns>
    protected Effect SetFixedAlignment(int armIndex)
    {
        if(armIndex < 0) return null;
        if(armIndex >= armAlignments.Count) return null;
        return SetFixedAlignment(position + armAlignments[armIndex]);
    }
    /// <summary>
    /// 各腕の照準位置を標準座標へ連続的に移動させる
    /// </summary>
    /// <param name="siteTweak">照準移動速度補正値</param>
    protected void SetBaseAimingAll(float siteTweak = 1)
    {
        for(int armIndex = 0; armIndex < arms.Count; armIndex++)
        {
            Aiming(standardAimPosition, armIndex, siteTweak);
        }
    }
    /// <summary>
    /// 目標地点への移動
    /// </summary>
    /// <param name="destination">目標地点</param>
    /// <param name="headingSpeed">速度指定値</param>
    /// <param name="endDistance">目標地点からの動作完了距離</param>
    /// <param name="concurrentProcess">同時並行で行う処理</param>
    /// <returns>コルーチン</returns>
    public override IEnumerator HeadingDestination(Vector2 destination, float headingSpeed, float endDistance, UnityAction concurrentProcess = null, Func<bool> suspensionTerm = null)
    {
        var time = 0;
        var distance = destination - position;
        suspensionTerm = suspensionTerm ?? (() => time++ > interval * Mathf.Max(distance.magnitude, 1));
        yield return base.HeadingDestination(destination, headingSpeed, endDistance, concurrentProcess, suspensionTerm);
        yield break;
    }

    /// <summary>
    /// 目標地点への円弧を描く連続移動
    /// </summary>
    /// <param name="destination">目標地点</param>
    /// <param name="headingSpeed">速度指定値</param>
    /// <param name="directionTweak">円弧補正</param>
    /// <param name="tweakDifference">円弧補正の収束速度</param>
    /// <param name="endDistance">目標地点からの動作完了距離</param>
    /// <param name="concurrentProcess">同時並行で行う処理</param>
    /// <returns>コルーチン</returns>
    public IEnumerator HeadingProperDestination(Vector2 destination, float headingSpeed, float directionTweak = 90, float tweakDifference = 1, float endDistance = 0, UnityAction concurrentProcess = null)
    {
        destination = destination.Within(fieldLowerLeft, fieldUpperRight);
        var tweak = new[] { -directionTweak, directionTweak }.SelectRandom(new[] { 1, 1 });
        while((destination - position).magnitude > actualSpeed.magnitude + endDistance)
        {
            if(isDestroied) yield break;
            if(tweak != 0) tweak = Mathf.Max(Mathf.Abs(tweak) - tweakDifference, 0) * tweak.ToSign();
            Thrust(tweak.ToRotation() * (destination - position - actualSpeed), reactPower, maximumSpeed);
            concurrentProcess?.Invoke();
            yield return Wait(1);
        }
        yield break;
    }
}
