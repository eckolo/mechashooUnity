﻿using UnityEngine;
using System.Collections;
using System.Linq;

public abstract class Kemi : Npc
{
    /// <summary>
    /// 本体設置武装で狙う場合の目標地点
    /// </summary>
    protected Vector2 bodyAimPosition =>
          Vector2.right * (nearTarget.position.x + viewSize.x * (position.x - nearTarget.position.x).toSign() / 2) +
          Vector2.up * (nearTarget.position.y + bodyWeaponRoot.y);
    /// <summary>
    /// 武装の接続基点
    /// </summary>
    protected Vector2 bodyWeaponRoot
    {
        get {
            var weaponBase = weaponBases.FirstOrDefault();
            if(weaponBase == null) return Vector2.zero;

            var slot = weaponBase.weaponSlots.FirstOrDefault();
            return weaponBase.position + (slot?.rootPosition ?? Vector2.zero);
        }
    }
    /// <summary>
    /// 照準位置を標準座標へ連続的に移動させる
    /// </summary>
    /// <param name="siteTweak">照準移動速度補正値</param>
    /// <returns>照準位置</returns>
    protected Vector2 setBaseAiming(float siteTweak = 1)
        => aiming(position + baseAimPosition, 0, siteTweak);
}
