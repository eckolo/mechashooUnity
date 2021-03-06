﻿using UnityEngine;
using System.Collections;

public class Consolidated : Effect
{
    /// <summary>
    /// 消滅までのフレーム数
    /// </summary>
    [SerializeField]
    protected int destroyLimit = 120;
    /// <summary>
    /// 残滓オブジェクト
    /// </summary>
    [SerializeField]
    protected Charging residue = null;
    /// <summary>
    /// 残滓発生間隔
    /// </summary>
    [SerializeField]
    protected int residueInterval = 1;

    protected override IEnumerator Motion(int actionNum)
    {
        Vector3 initialScale = transform.localScale;
        for(int time = 0; time < destroyLimit; time++)
        {
            nowScale = initialScale * Easing.quadratic.SubIn(time, destroyLimit - 1);
            nowAlpha = nowAlpha * (Easing.quintic.SubIn(time, destroyLimit - 1));

            if(time % residueInterval == 0)
            {
                var effect = OutbreakEffect(residue, nowScale.x).GetComponent<Charging>();
                if(nowScale.y > 0) effect.initialScale.y = Mathf.Min(1, 1 / nowScale.y);
                effect.nowAngle = Random.Range(0, 359);
                effect.nowParent = transform;
            }

            yield return Wait(1);
        }
        Destroy(gameObject);
        yield break;
    }
}
