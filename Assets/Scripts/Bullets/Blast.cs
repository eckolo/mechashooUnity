﻿using UnityEngine;
using System.Collections;

public class Blast : Bullet
{
    /// <summary>
    /// 最大サイズ
    /// </summary>
    public float maxSize = 1;

    protected override IEnumerator Motion(int actionNum)
    {
        for (int time = 0; time < destroyLimit; time++)
        {
            transform.localScale = Vector2.one * easing.quintic.Out(maxSize, time, destroyLimit - 1);
            setAlpha(1 - easing.quadratic.In(time, destroyLimit - 1));
            yield return null;
        }

        selfDestroy();
        yield break;
    }
    public override float getPower()
    {
        return basePower * getAlpha();
    }
}
