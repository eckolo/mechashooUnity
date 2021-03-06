﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public static class EnumFunctions
{
    /// <summary>
    /// 数値が列挙型に含まれるか否か判定する
    /// </summary>
    /// <typeparam name="enumType">判定対象の列挙型</typeparam>
    /// <param name="value">判定対象の数値</param>
    /// <returns>数値が列挙型に含まれるか否か</returns>
    public static bool IsDefined<enumType>(this int value)
        where enumType : struct, IFormattable, IConvertible
        => Enum.IsDefined(typeof(enumType), value);
    /// <summary>
    /// 数値を列挙型の定義範囲内の値に変換する
    /// </summary>
    /// <typeparam name="enumType">変換先列挙型</typeparam>
    /// <param name="value">変換元数値</param>
    /// <returns>変換された列挙型</returns>
    public static enumType Normalize<enumType>(this int value)
        where enumType : struct, IFormattable, IConvertible
    {
        if(!IsDefined<enumType>(value)) return default(enumType);
        return Convert<enumType>(value);
    }
    /// <summary>
    /// 列挙型の要素数取得
    /// </summary>
    /// <typeparam name="enumType">列挙型</typeparam>
    /// <returns>要素数</returns>
    public static int GetLength<enumType>()
        where enumType : struct, IFormattable, IConvertible
        => Enum.GetValues(typeof(enumType)).Length;
    /// <summary>
    /// 列挙型の最大値取得
    /// </summary>
    /// <typeparam name="enumType">列挙型</typeparam>
    /// <returns>最大値</returns>
    public static enumType GetMax<enumType>()
        where enumType : struct, IFormattable, IConvertible
        => List<enumType>().Max();
    /// <summary>
    /// 列挙型の最小値取得
    /// </summary>
    /// <typeparam name="enumType">列挙型</typeparam>
    /// <returns>最小値</returns>
    public static enumType GetMin<enumType>()
        where enumType : struct, IFormattable, IConvertible
        => List<enumType>().Min();
    /// <summary>
    /// 数値から列挙型への変換
    /// </summary>
    /// <typeparam name="enumType">変換先列挙型</typeparam>
    /// <param name="value">変換元数値</param>
    /// <returns>変換された列挙型</returns>
    static enumType Convert<enumType>(int value)
        where enumType : struct, IFormattable, IConvertible
        => (enumType)Enum.ToObject(typeof(enumType), value);
    /// <summary>
    /// 列挙型の値を列挙したリストの生成
    /// </summary>
    /// <typeparam name="enumType">列挙型</typeparam>
    /// <returns>列挙型の値の列挙されたリスト</returns>
    static List<enumType> List<enumType>()
        where enumType : struct, IFormattable, IConvertible
        => ((IEnumerable<enumType>)Enum.GetValues(typeof(enumType))).ToList();
}
