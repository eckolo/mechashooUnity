﻿using UnityEngine;
using System.Collections;
using System.Linq;

public class Luwucijiqi : Npc
{
    /// <summary>
    /// 画面内に位置強制するフラグ
    /// </summary>
    protected override bool forcedInScreen
    {
        get {
            if(onTheWay && normalCourse.normalized == nowSpeed.normalized) return false;
            return base.forcedInScreen;
        }
    }

    /// <summary>
    /// 非反応時行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns></returns>
    protected override IEnumerator motionNonCombat(int actionNum)
    {
        yield return nomalMoving((maximumSpeed + lowerSpeed) / 2);
        yield break;
    }
    /// <summary>
    /// 移動時行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns></returns>
    protected override IEnumerator motionMove(int actionNum)
    {
        nextActionState = ActionPattern.AIMING;
        var wraps = Random.Range(2, (3 + (int)shipLevel) * (onTheWay.toInt() + 1));
        var baseSpeed = normalCourse;

        for(int wrap = 0; wrap < wraps && inField; wrap++)
        {
            var nowAngle = (baseSpeed.magnitude > 0 ? baseSpeed : siteAlignment).toAngle();
            var correctAngle = !onTheWay ? Random.Range(90, 270) : Random.Range(-60, 60);
            var direction = (nowAngle + correctAngle).compile();
            yield return aimingAction(nearTarget.position,
                !onTheWay ? interval : interval * 2,
                () => exertPower(direction, reactPower, maximumSpeed));
            if(!onTheWay) baseSpeed = nowSpeed;
            yield return nomalAttack();
        }
        yield break;
    }
    /// <summary>
    /// 照準操作行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns></returns>
    protected override IEnumerator motionAiming(int actionNum)
    {
        nextActionState = ActionPattern.ATTACK;
        nextActionIndex = (nearTarget.position.magnitude < arms[1].tipReach).toInt();
        yield return aimingAction(() => nearTarget.position,
            interval,
            () => thrust(!onTheWay ? nearTarget.position - position : normalCourse, reactPower, lowerSpeed));
        yield break;
    }
    /// <summary>
    /// 攻撃行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns></returns>
    protected override IEnumerator motionAttack(int actionNum)
    {
        nextActionState = !onTheWay ? ActionPattern.MOVE : ActionPattern.NON_COMBAT;
        yield return nomalAttack();
        yield return wait(() => !arms.Any(arm => !arm.tipHand.takeWeapon.canAction));
        yield break;
    }
    /// <summary>
    /// 逃走時行動
    /// </summary>
    /// <param name="actionNum">行動パターン識別番号</param>
    /// <returns></returns>
    protected override IEnumerator motionEscape(int actionNum)
    {
        yield return nomalMoving(maximumSpeed);
        yield break;
    }

    IEnumerator nomalMoving(float speed)
    {
        var direction = Random.Range(-10f, 10f).toRotation() * normalCourse;
        for(int time = 0; time < interval; time++)
        {
            thrust(direction, reactPower, speed);
            aiming(position + baseAimPosition);
            yield return wait(1);
        }
        yield return stoppingAction();
        yield break;
    }
    IEnumerator nomalAttack()
    {
        if(!inField) yield break;
        int armNum = (siteAlignment.magnitude > arms[1].tipReach).toInt();
        arms[armNum].tipHand.actionWeapon(Weapon.ActionType.NOMAL);
        yield return stoppingAction();
    }
}
