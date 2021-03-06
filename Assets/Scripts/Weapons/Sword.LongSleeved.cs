﻿using UnityEngine;
using System.Collections;

public partial class Sword : Weapon
{
    /// <summary>
    /// 長柄武器系モーション
    /// </summary>
    protected class LongSleeved : IMotion<Sword>
    {
        public IEnumerator BeginMotion(Sword sword, bool forward = true)
        {
            var sign = forward.ToSign();
            var spins = sword.turnoverRate;
            var startAngle = sword.nowLocalAngle.Compile();
            var endAngle = spins > 0 ? 180f : 0f;
            sword.SoundSE(sword.swingUpSE, 1, (float)sword.timeRequiredPrior / 20);
            yield return sword.SwingAction(endPosition: new Vector2(-1.5f, 0.5f * sign),
                timeLimit: sword.timeRequiredPrior,
                timeEasing: Easing.quadratic.Out,
                clockwise: !forward,
                midstreamProcess: (time, localTime, limit) => sword.SetAngle(startAngle + (Easing.quadratic.Out(endAngle - startAngle, time, limit))));
            yield break;
        }
        public IEnumerator MainMotion(Sword sword, bool forward = true)
        {
            if(sword.nowParent.GetComponent<Hand>() == null) yield break;

            var monoTime = sword.timeRequired / 4;
            var coreTime = monoTime * 4;
            var interval = Mathf.Max(coreTime / sword.density, 1);
            var sign = forward.ToSign();
            var spins = sword.turnoverRate;

            var fireNum = sword.fireNum;
            for(int fire = 0; fire < fireNum; fire++)
            {
                var startAngle = sword.nowLocalAngle.Compile();
                if(fire > 0)
                {
                    var endAngle = spins > 0 ? 180f : 0f;
                    sword.SoundSE(sword.swingUpSE, 1, (float)sword.timeRequiredPrior / 20);
                    yield return sword.SwingAction(endPosition: new Vector2(-1.5f, 0.5f * sign),
                        timeLimit: sword.timeRequiredPrior,
                        timeEasing: Easing.quadratic.Out,
                        clockwise: !forward,
                        midstreamProcess: (time, localTime, limit) => sword.SetAngle(startAngle + (Easing.quadratic.Out(endAngle - startAngle, time, limit))));
                }

                startAngle = sword.nowLocalAngle.Compile();
                yield return sword.SwingAction(endPosition: new Vector2(-1, 1 * sign),
                    timeLimit: monoTime,
                    timeEasing: Easing.quadratic.In,
                    clockwise: forward,
                    midstreamProcess: (time, localTime, limit) => AttackAction(sword, startAngle, sign, interval, time, localTime, limit, coreTime, 3));

                sword.SoundSE(sword.swingDownSE, 1, (float)monoTime / 5);
                yield return sword.SwingAction(endPosition: new Vector2(0.5f, 0),
                    timeLimit: monoTime,
                    timeEasing: Easing.liner.In,
                    clockwise: forward,
                    midstreamProcess: (time, localTime, limit) => AttackAction(sword, startAngle, sign, interval, time, localTime, limit, coreTime, 2));

                yield return sword.SwingAction(endPosition: new Vector2(-1, -1 * sign),
                    timeLimit: monoTime,
                    timeEasing: Easing.liner.Out,
                    clockwise: forward,
                    midstreamProcess: (time, localTime, limit) => AttackAction(sword, startAngle, sign, interval, time, localTime, limit, coreTime, 1));

                sword.SoundSE(sword.swingDownSE, 1, (float)monoTime / 5);
                yield return sword.SwingAction(endPosition: new Vector2(-1.5f, -0.5f * sign),
                    timeLimit: monoTime,
                    timeEasing: Easing.quadratic.Out,
                    clockwise: forward,
                    midstreamProcess: (time, localTime, limit) => AttackAction(sword, startAngle, sign, interval, time, localTime, limit, coreTime, 0));
            }
        }
        public IEnumerator EndMotion(Sword sword, bool forward = true)
        {
            if(sword.nowParent.GetComponent<Hand>() == null) yield break;
            float startAngle = sword.nowLocalAngle.Compile();
            float endAngle = sword.defAngle;
            yield return sword.SwingAction(endPosition: Vector2.zero,
              timeLimit: sword.timeRequiredARest * 2,
              timeEasing: Easing.quadratic.InOut,
              clockwise: !forward,
              midstreamProcess: (time, localTime, limit) => sword.SetAngle(startAngle + (Easing.quadratic.In(endAngle - startAngle, time, limit))));

            yield return Wait(sword.timeRequiredARest);
        }
        static void AttackAction(Sword sword, float startAngle, int sign, int interval, int time, float localTime, int limit, int coreTime, int reIndex)
        {
            var spins = sword.turnoverRate;
            var _time = time + coreTime - limit * (reIndex + 1);
            var swordAngle = 360f * spins * Easing.quintic.InOut(_time, coreTime - 1) * sign;
            sword.SetAngle(startAngle - swordAngle);

            var isTiming = coreTime * 1 / 3 < _time && _time < coreTime * 2 / 3
                && (coreTime - 1 - _time) % interval == 0;
            float center = coreTime / 2;
            var power = Easing.quintic.In(center - Mathf.Abs(center - _time), center);
            if(isTiming) sword.Slash(power);
        }
    }
}
