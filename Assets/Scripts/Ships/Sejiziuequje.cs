﻿using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public partial class Sejiziuequje : Boss
{
    /// <summary>
    /// 真形態に到達するフラグ
    /// </summary>
    [SerializeField]
    private bool reachTrueFigure = false;
    /// <summary>
    /// 真形態時専用BGM
    /// </summary>
    [SerializeField]
    public AudioClip trueFigureBgm = null;
    /// <summary>
    /// 真形態
    /// </summary>
    bool trueFigure => reachTrueFigure && palamates.nowArmor < maxArmor / 2;

    /// <summary>
    /// 最大装甲値
    /// </summary>
    public override float maxArmor => base.maxArmor * (reachTrueFigure ? 1 : 0.5f);

    uint attackCount = 0;

    public override void Start()
    {
        base.Start();
        hands = allLanges.Select(weapon => new HandControler(weapon)).ToList();
        foreach(var hand in hands) hand.BeginMotion(this);
    }
    public override void Update()
    {
        base.Update();
        if(!trueFigure) foreach(var weaponBase in weaponBases) weaponBase.nowColor = nowColor;
        if(alreadyOnceReaction && !isReaction) foreach(var hand in hands) hand.EndMotion(this);
    }

    /// <summary>
    /// 本気モードフラグ
    /// </summary>
    protected override bool seriousMode => reachTrueFigure ?
        trueFigure ? palamates.nowArmor < maxArmor / 4 : palamates.nowArmor < maxArmor * 3 / 4 :
        base.seriousMode;

    List<HandControler> hands = new List<HandControler>();

    List<AllLange> allLanges => allWeapons.Take(2).Select(weapon => weapon.GetComponent<AllLange>()).ToList();
    Weapon grenade => allWeapons[2];
    Weapon laser => allWeapons[3];
    List<Weapon> guss => allWeapons.Skip(4).ToList();

    enum BodyMotionType
    {
        HAND_GRENADE_BURST,
        HAND_LASER_BURST,
        HUGE_GRENADE,
        HUGE_MISSILE,
        HUGE_MISSILE_CHARGE,
        CRUISE,
        GRENADE_VOLLEY,
        GRENADE_BURST,
        HUGE_LASER_CHARGE,
        HUGE_LASER
    }
    enum HandMotionType
    {
        GRENADE,
        GRENADE_FIXED,
        GRENADE_BURST,
        LASER,
        LASER_FIXED,
        LASER_BURST,
        LASER_SPIN,
        LASER_CLOSEUP
    }

    /// <summary>
    /// 移動時行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns>コルーチン</returns>
    protected override IEnumerator MotionMove(int actionNum)
    {
        nextActionState = ActionPattern.AIMING;
        var reasonableSpeed = (maximumSpeed + lowerSpeed) / 2;
        AlwaysAttack();

        for(int time = 0; time < interval * 2; time++)
        {
            if(isDestroied) yield break;
            var direction = nearTarget.position - position;
            if(direction.magnitude > grappleDistance) Thrust(direction, targetSpeed: reasonableSpeed);
            Aiming(nearTarget.position);
            yield return Wait(1);
        }
        AlwaysAttack();
        yield return StoppingAction();
        AlwaysAttack();

        nextActionIndex = trueFigure ?
            (int)new[] {
                BodyMotionType.HAND_GRENADE_BURST,
                BodyMotionType.HAND_LASER_BURST,
                BodyMotionType.HUGE_GRENADE,
                BodyMotionType.HUGE_MISSILE,
                BodyMotionType.HUGE_MISSILE_CHARGE,
                BodyMotionType.CRUISE,
                BodyMotionType.GRENADE_VOLLEY,
                BodyMotionType.GRENADE_BURST,
                BodyMotionType.HUGE_LASER_CHARGE,
                BodyMotionType.HUGE_LASER
            }.SelectRandom() :
            onTheWay ?
            (int)new[] {
                BodyMotionType.HUGE_MISSILE,
                BodyMotionType.HUGE_GRENADE,
                BodyMotionType.HUGE_MISSILE_CHARGE,
                BodyMotionType.CRUISE
            }[attackCount % 4] :
            (int)new[] {
                BodyMotionType.HAND_GRENADE_BURST,
                BodyMotionType.HAND_LASER_BURST,
                BodyMotionType.HUGE_GRENADE,
                BodyMotionType.HUGE_MISSILE,
                BodyMotionType.HUGE_MISSILE_CHARGE,
                BodyMotionType.CRUISE
            }.SelectRandom(new[] { 1, 1, 6, 6, 3 });
        yield break;
    }
    /// <summary>
    /// 照準操作行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns>コルーチン</returns>
    protected override IEnumerator MotionAiming(int actionNum)
    {
        nextActionState = ActionPattern.ATTACK;
        var motion = actionNum.Normalize<BodyMotionType>();
        AlwaysAttack();

        switch(motion)
        {
            case BodyMotionType.HAND_GRENADE_BURST:
                {
                    foreach(var hand in hands) hand.SetMotionType(HandMotionType.GRENADE_BURST);
                    AlwaysAttack();
                    yield return Wait(() => hands.All(hand => hand.isStandby));
                    AlwaysAttack();
                    yield return StoppingAction();
                }
                break;
            case BodyMotionType.HAND_LASER_BURST:
                {
                    foreach(var hand in hands) hand.SetMotionType(HandMotionType.LASER_BURST);
                    AlwaysAttack();
                    yield return Wait(() => hands.All(hand => hand.isStandby));
                    AlwaysAttack();
                    yield return StoppingAction();
                }
                break;
            case BodyMotionType.HUGE_GRENADE:
                {
                    var targetPosition = nearTarget.position;
                    for(int time = 0; time < interval * 2; time++)
                    {
                        if(isDestroied) yield break;
                        var direction = nearTarget.position - position;
                        if(direction.magnitude > gunDistance) Thrust(direction, targetSpeed: maximumSpeed);
                        else ThrustStop();
                        Aiming(targetPosition);
                        yield return Wait(1);
                    }
                    AlwaysAttack();
                    yield return StoppingAction();
                }
                break;
            case BodyMotionType.HUGE_MISSILE:
                {
                    for(int time = 0; time < interval; time++)
                    {
                        if(isDestroied) yield break;
                        var direction = nearTarget.position - position;
                        if(direction.magnitude > gunDistance) Thrust(direction, targetSpeed: maximumSpeed);
                        else ThrustStop();
                        Aiming(nearTarget.position + nearTarget.nowSpeed);
                        yield return Wait(1);
                    }
                    AlwaysAttack();
                    yield return StoppingAction();
                }
                break;
            case BodyMotionType.HUGE_MISSILE_CHARGE:
                {
                    for(int time = 0; time < interval; time++)
                    {
                        if(isDestroied) yield break;
                        var direction = nearTarget.position - position;
                        if(direction.magnitude > grappleDistance) Thrust(direction, targetSpeed: lowerSpeed);
                        else ThrustStop();
                        Aiming(nearTarget.position);
                        yield return Wait(1);
                    }
                    AlwaysAttack();
                    yield return StoppingAction();
                }
                break;
            case BodyMotionType.CRUISE:
                {
                    var destination = new Vector2(nearTarget.position.x, viewPosition.y * 2 - nearTarget.position.y);
                    yield return HeadingDestination(destination, maximumSpeed, grappleDistance);
                    AlwaysAttack();
                    yield return StoppingAction();
                }
                break;
            case BodyMotionType.GRENADE_VOLLEY:
                break;
            case BodyMotionType.GRENADE_BURST:
                break;
            case BodyMotionType.HUGE_LASER_CHARGE:
                break;
            case BodyMotionType.HUGE_LASER:
                break;
            default:
                break;
        }

        AlwaysAttack();
        yield break;
    }
    /// <summary>
    /// 攻撃行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns>コルーチン</returns>
    protected override IEnumerator MotionAttack(int actionNum)
    {
        nextActionState = ActionPattern.MOVE;
        var motion = actionNum.Normalize<BodyMotionType>();
        var finishMotion = true;
        AlwaysAttack();

        switch(motion)
        {
            case BodyMotionType.HAND_GRENADE_BURST:
                {
                    hands.First().isStandby = false;
                    yield return Wait(() => allLanges.First().onAttack);
                    hands.Last().isStandby = false;
                }
                break;
            case BodyMotionType.HAND_LASER_BURST:
                {
                    hands.First().isStandby = false;
                    yield return Wait(() => allLanges.First().onAttack);
                    yield return Wait(() => !allLanges.First().onAttack);
                    hands.Last().isStandby = false;
                }
                break;
            case BodyMotionType.HUGE_GRENADE:
                {
                    const int fireNum = 2;
                    for(int fire = 0; fire < fireNum; fire++)
                    {
                        yield return Wait(() => grenade.canAction);
                        var longRange = (position - nearTarget.position).magnitude > gunDistance;
                        SetFixedAlignment(position + siteAlignment);
                        grenade.Action(longRange ? Weapon.ActionType.NOMAL : Weapon.ActionType.NPC);

                        if(fire < fireNum)
                        {
                            AlwaysAttack();
                            yield return Wait(() => grenade.onAttack);
                            AlwaysAttack();
                            var targetPosition = nearTarget.position;
                            for(int time = 0; !grenade.canAction; time++)
                            {
                                if(isDestroied) yield break;
                                Aiming(targetPosition);
                                yield return Wait(1);
                            }
                        }

                        AlwaysAttack();
                    }
                    yield return Wait(() => grenade.canAction);
                }
                break;
            case BodyMotionType.HUGE_MISSILE:
                {
                    yield return Wait(() => grenade.canAction);
                    AlwaysAttack();
                    grenade.Action(Weapon.ActionType.SINK);
                    for(int time = 0; !grenade.onAttack; time++)
                    {
                        if(isDestroied) yield break;
                        Aiming(nearTarget.position + nearTarget.nowSpeed);
                        yield return Wait(1);
                    }
                    AlwaysAttack();
                    yield return Wait(() => grenade.canAction);
                }
                break;
            case BodyMotionType.HUGE_MISSILE_CHARGE:
                {
                    for(int time = 0; time < interval; time++)
                    {
                        if(isDestroied) yield break;
                        var direction = position - nearTarget.position;
                        Thrust(direction, targetSpeed: lowerSpeed);
                        Aiming(nearTarget.position);
                        AlwaysAttack(Weapon.ActionType.SINK);
                        yield return Wait(1);
                    }
                    yield return StoppingAction();
                    AlwaysAttack(Weapon.ActionType.SINK);
                    yield return Wait(() => grenade.canAction);
                    AlwaysAttack(Weapon.ActionType.SINK);
                    grenade.Action(Weapon.ActionType.SINK);
                    for(int time = 0; !grenade.onAttack; time++)
                    {
                        if(isDestroied) yield break;
                        Aiming(nearTarget.position);
                        yield return Wait(1);
                    }
                    AlwaysAttack();
                    yield return Wait(() => grenade.canAction);
                }
                break;
            case BodyMotionType.CRUISE:
                {
                    AlwaysAttack();
                    var direction = new[] { 90f, -90f }.SelectRandom();
                    var timelimit = Random.Range(2, shipLevel) * 5 * interval;
                    for(int time = 0; time < timelimit; time++)
                    {
                        if(isDestroied) yield break;
                        var directionTweak = (Vector2)(direction.ToRotation() * (position - nearTarget.position));
                        var destination = (position + directionTweak).Within(fieldLowerLeft, fieldUpperRight);
                        Thrust(destination - position, reactPower, maximumSpeed);
                        Aiming(destination);
                        AlwaysAttack(Weapon.ActionType.SINK);
                        yield return Wait(1);
                    }
                    yield return StoppingAction();
                }
                break;
            case BodyMotionType.GRENADE_VOLLEY:
                break;
            case BodyMotionType.GRENADE_BURST:
                break;
            case BodyMotionType.HUGE_LASER_CHARGE:
                break;
            case BodyMotionType.HUGE_LASER:
                break;
            default:
                break;
        }

        AlwaysAttack();
        for(int time = 0; finishMotion && time < interval; time++)
        {
            Aiming(nearTarget.position);
            SetBaseAimingAll();
            ThrustStop();
            yield return Wait(1);
        }
        if(onTheWay && ++attackCount >= 4)
        {
            foreach(var hand in hands) hand.EndMotion(this);
            if(allLanges.All(allLange => allLange.isFixed)) nextActionState = ActionPattern.ESCAPE;
        }
        yield break;
    }

    /// <summary>
    /// 常に行われる攻撃行動
    /// </summary>
    private void AlwaysAttack(Weapon.ActionType? setAction = null)
    {
        foreach(var weapon in guss)
        {
            if(weapon == null) continue;
            weapon.Action(setAction ?? new[] {
                Weapon.ActionType.NOMOTION,
                Weapon.ActionType.NOMAL,
                Weapon.ActionType.SINK
            }.SelectRandom(seriousMode ? new[] { 5, 3, 1 } : new[] { 72, 12, 1 }));
            if(!gusAlignment.ContainsKey(weapon)) gusAlignment.Add(weapon, null);
            if(gusAlignment[weapon] == null && weapon.nextAction == Weapon.ActionType.SINK)
            {
                var parent = weapon.nowConnectParent;
                var targetPosition = parent.position
                    + (Vector2)(parent.nowAngle.ToRotation() * weapon.position)
                    + (parent.nowAngle + weapon.nowAngle).ToVector(gunDistance);
                gusAlignment[weapon] = SetFixedAlignment(targetPosition, true);
            }
        }
    }
    Dictionary<Weapon, Effect> gusAlignment = new Dictionary<Weapon, Effect>();
    protected override IEnumerator SinkingMotion()
    {
        foreach(var hand in hands) hand.EndMotion(this);
        if(reachTrueFigure)
        {
            yield return base.SinkingMotion();
        }
        else
        {
            Escape();
            ableEnter = false;
            for(var time = 0; Mathf.Abs(position.x) < fieldUpperRight.x * 2 && Mathf.Abs(position.y) < fieldUpperRight.y * 2; time++)
            {
                if(time % interval == 0)
                {
                    var setPosition = new Vector2(Random.Range(-spriteSize.x / 2, spriteSize.x / 2), Random.Range(-spriteSize.y / 2, spriteSize.y / 2));
                    OutbreakExplosion(1, setPosition, 1);
                }
                Thrust(normalCourse = normalCourse + position - nearTarget.position, targetSpeed: maximumSpeed);
                Aiming(position + normalCourse);
                yield return Wait(1);
            }
        }
        foreach(var hand in hands) hand.DestroyMyself();
        yield break;
    }

    /// <summary>
    /// 射撃適正距離
    /// </summary>
    protected override float gunDistance => viewSize.x / 2;

    public override void DestroyMyself(bool system)
    {
        foreach(var hand in hands) hand.DestroyMyself();
        base.DestroyMyself(system);
    }
}
