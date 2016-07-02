﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

/// <summary>
///あらゆるオブジェクトの基底関数とか
/// </summary>
public class Roots : MonoBehaviour
{
    /// <summary>
    ///各種カウンター
    /// </summary>
    [SerializeField]
    protected Dictionary<string, int> counterList = new Dictionary<string, int>();
    /// <summary>
    ///イージング関数群
    /// </summary>
    protected Easing easing = new Easing();

    protected delegate bool Terms(Roots target);
    protected delegate float Rank(Roots target);

    // Update is called once per frame
    public virtual void Start()
    {
        baseStart();
    }
    protected virtual void baseStart() { }

    // Update is called once per frame
    public virtual void Update()
    {
        baseUpdate();
        foreach (var counterName in new List<string>(counterList.Keys))
        {
            counterList[counterName]++;
        }
    }
    protected virtual void baseUpdate() { }

    protected List<Roots> getAllObject(Terms map = null)
    {
        var returnList = new List<Roots>();
        foreach (Roots value in FindObjectsOfType(typeof(Roots)))
        {
            if (map == null || map(value)) returnList.Add(value);
        }
        return returnList;
    }
    protected List<Roots> searchMaxObject(Rank refine, Terms map = null)
    {
        List<Roots> returnList = new List<Roots>();
        foreach (var value in getAllObject(map))
        {
            if (returnList.Count <= 0)
            {
                returnList.Add(value);
            }
            else if (refine(value) > refine(returnList[0]))
            {
                returnList = new List<Roots> { value };
            }
            else if (refine(value) == refine(returnList[0]))
            {
                returnList.Add(value);
            }
        }

        return returnList;
    }
    protected List<Roots> getNearObject(Terms map = null)
    {
        return searchMaxObject(target => -(target.transform.position - transform.position).magnitude, map);
    }

    public virtual bool Action(int? actionNum = null)
    {
        StartCoroutine(baseMotion(actionNum ?? 0));
        return true;
    }
    protected virtual IEnumerator baseMotion(int actionNum)
    {
        yield return Motion(actionNum);
        yield break;
    }

    protected virtual IEnumerator Motion(int actionNum)
    {
        yield break;
    }

    /// <summary>
    ///システムテキストへの文字設定
    /// </summary>
    protected void setSysText(string setText)
    {
        GameObject.Find("SystemText").GetComponent<Text>().text = setText;
    }
    /// <summary>
    ///システムテキストの取得
    /// </summary>
    protected string getSysText()
    {
        return GameObject.Find("SystemText").GetComponent<Text>().text;
    }

    /// <summary>
    ///指定フレーム数待機する関数
    ///yield returnで呼び出さないと意味をなさない
    /// </summary>
    protected IEnumerator wait(int delay)
    {
        for (var i = 0; i < delay; i++) yield return null;
        yield break;
    }

    protected static float compileMinusAngle(float angle)
    {
        while (angle < 0) angle += 360;
        while (angle >= 360) angle -= 360;
        return angle;
    }
    protected static float toAngle(Vector2 targetVector)
    {
        return Vector2.Angle(Vector2.right, targetVector) * (Vector2.Angle(Vector2.up, targetVector) <= 90 ? 1 : -1);
    }
    protected void setAngle(Vector2 targetVector, bool widthPositive = true)
    {
        transform.rotation = Quaternion.FromToRotation(widthPositive ? Vector2.right : Vector2.left, targetVector);
        return;
    }
    public float setAngle(float settedAngle, bool widthPositive = true)
    {
        if (!widthPositive) settedAngle = 180 - compileMinusAngle(settedAngle);
        var finalAngle = compileMinusAngle(settedAngle);
        transform.localEulerAngles = new Vector3(0, 0, finalAngle);

        return finalAngle;
    }
    public Vector2 invertVector(Vector2 inputVector)
    {
        return new Vector2(inputVector.x * -1, inputVector.y);
    }

    /// <summary>
    ///オブジェクトが可動範囲内にいるかどうか
    /// </summary>
    protected bool inScreen()
    {
        // 画面左下のワールド座標をビューポートから取得
        var lowerLeft = Camera.main.ViewportToWorldPoint(new Vector2(-1, -1));
        // 画面右上のワールド座標をビューポートから取得
        var upperRight = Camera.main.ViewportToWorldPoint(new Vector2(2, 2));

        if (transform.position.x < lowerLeft.x) return false;
        if (transform.position.x > upperRight.x) return false;
        if (transform.position.y < lowerLeft.y) return false;
        if (transform.position.y > upperRight.y) return false;
        return true;
    }

    /// <summary>
    ///オブジェクトの移動関数
    /// </summary>
    public void setVerosity(Vector2 verosity, float speed = 0, bool inScreen = false)
    {
        // 実移動量を計算
        var innerVerosity = verosity.normalized * speed;

        if (inScreen)
        {
            // オブジェクトの座標を取得
            var self = transform.position;

            // 画面左下のワールド座標をビューポートから取得
            var lowerLeft = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));

            // 画面右上のワールド座標をビューポートから取得
            var upperRight = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

            // オブジェクトの位置が画面内に収まるように制限をかける
            innerVerosity.x = Mathf.Clamp(
                innerVerosity.x,
                (lowerLeft.x - self.x) * 100,
                (upperRight.x - self.x) * 100);
            innerVerosity.y = Mathf.Clamp(
                innerVerosity.y,
                (lowerLeft.y - self.y) * 100,
                (upperRight.y - self.y) * 100);
        }

        //速度設定
        GetComponent<Rigidbody2D>().velocity = innerVerosity;

        //移動時アクション呼び出し
        setVerosityAction(GetComponent<Rigidbody2D>().velocity, speed);
    }
    protected virtual void setVerosityAction(Vector2 verosity, float speed) { }

    protected class Easing
    {
        public Linear liner = new Linear();
        public Quadratic quadratic = new Quadratic();
        public Cubic cubic = new Cubic();
        public Quartic quartic = new Quartic();
        public Quintic quintic = new Quintic();
        public Sinusoidal sinusoidal = new Sinusoidal();
        public Exponential exponential = new Exponential();
        public Circular circular = new Circular();

        public class BaseEaaing
        {
            public virtual float In(float max, float time, float limit)
            {
                Debug.Log(max);
                return max;
            }
            public float Out(float max, float time, float limit)
            {
                return max - In(max, limit - time, limit);
            }
            public float InOut(float max, float time, float limit)
            {
                return time < limit / 2
                    ? In(max / 2, time, limit / 2)
                    : Out(max / 2, time - limit / 2, limit / 2);
            }
        }
        public class Linear : BaseEaaing
        {
            public override float In(float max, float time, float limit)
            {
                return max * time / limit;
            }
        }
        public class Quadratic : BaseEaaing
        {
            public override float In(float max, float time, float limit)
            {
                return max * time * time / limit / limit;
            }
        }
        public class Cubic : BaseEaaing
        {
            public override float In(float max, float time, float limit)
            {
                return max * time * time * time / limit / limit / limit;
            }
        }
        public class Quartic : BaseEaaing
        {
            public override float In(float max, float time, float limit)
            {
                return max * time * time * time * time / limit / limit / limit / limit;
            }
        }
        public class Quintic : BaseEaaing
        {
            public override float In(float max, float time, float limit)
            {
                return max * time * time * time * time * time / limit / limit / limit / limit / limit;
            }
        }
        public class Sinusoidal : BaseEaaing
        {
            public override float In(float max, float time, float limit)
            {
                return -max * Mathf.Cos(time * Mathf.PI / limit / 2) + max;
            }
        }
        public class Exponential : BaseEaaing
        {
            public override float In(float max, float time, float limit)
            {
                return max * Mathf.Pow(2, 10 * (time - limit) / limit);
            }
        }
        public class Circular : BaseEaaing
        {
            public override float In(float max, float time, float limit)
            {
                return -max * (Mathf.Sqrt(1 - time * time / limit / limit) - 1);
            }
        }
    }
}
