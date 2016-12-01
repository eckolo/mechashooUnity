﻿using UnityEngine;
using System.Collections;

/// <summary>
/// 爆破エフェクトクラス
/// </summary>
public class Explosion : Effect
{
    /// <summary>
    /// 消滅までのフレーム数
    /// </summary>
    [SerializeField]
    protected int destroyLimit = 120;
    /// <summary>
    /// 最大サイズ
    /// </summary>
    [SerializeField]
    protected float maxSize = 2;
    /// <summary>
    /// サイズ変動逆転フラグ
    /// </summary>
    [SerializeField]
    protected bool reverse = false;
    /// <summary>
    ///炸裂時SE
    /// </summary>
    public AudioClip explodeSE = null;

    protected override IEnumerator motion(int actionNum)
    {
        Vector3 baseScale = transform.localScale;
        soundSE(explodeSE);

        for(int time = 0; time < destroyLimit; time++)
        {
            transform.localScale = baseScale * (!reverse
                ? easing.exponential.Out(maxSize, time, destroyLimit - 1)
                : easing.exponential.SubOut(maxSize, time, destroyLimit - 1));

            setAlpha(nowAlpha * (easing.quadratic.SubIn(time, destroyLimit - 1)));

            yield return wait(1);
        }
        Destroy(gameObject);
        yield break;
    }
}
