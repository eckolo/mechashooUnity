﻿using UnityEngine;
using System.Collections;

/// <summary>
/// 特に銃みたいなタイプの武装全般
/// </summary>
public class Gun : Weapon
{
    /// <summary>
    /// 連射数
    /// </summary>
    public int fireNum;
    /// <summary>
    /// 弾を撃つ間隔
    /// </summary>
    public int shotDelay;

    /// <summary>
    /// 発射システム
    /// </summary>
    protected override IEnumerator Motion(int actionNum)
    {
        for (int i = 0; i < fireNum; i++)
        {
            injection(i).velocity = transform.rotation * Vector2.right * getLossyScale(transform).x;

            //反動発生
            startRecoil(new Vector2(0, 0.1f));

            // shotDelayフレーム待つ
            yield return StartCoroutine(wait(shotDelay));
        }
        yield break;
    }
}
