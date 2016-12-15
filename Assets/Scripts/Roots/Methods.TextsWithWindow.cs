﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System;

public partial class Methods : MonoBehaviour
{
    protected class TextsWithWindow : IDisposable
    {
        public TextsWithWindow()
        {
            textNames = new List<string>();
        }
        public void Dispose()
        {
            selfDestroy(system: true);
        }

        //MEMO:デストラクタで呼ばせる
        public void selfDestroy(bool setMotion = true, bool system = false)
        {
            for(int index = 0; index < textNames.Count; index++) deleteSysText(textNames[index]);
            if(backWindow != null) deleteWindow(backWindow, setMotion ? Choice.WINDOW_MOTION_TIME : 0, system);
        }

        public List<string> textNames
        {
            get
            {
                return texts.Select(textObj => textObj.name).ToList();
            }
            set
            {
                texts = value.Select(name => GameObject.Find(name).GetComponent<Text>()).ToList();
            }
        }
        public Text text
        {
            set
            {
                texts = new List<Text> { value };
            }
        }
        public List<Text> texts { get; set; }
        public Window backWindow { get; set; }
        public Vector2 underLeft
        {
            get
            {
                if(backWindow == null) return position - textArea / 2;
                return backWindow.underLeft;
            }
        }
        public Vector2 upperRight
        {
            get
            {
                if(backWindow == null) return position + textArea / 2;
                return backWindow.upperRight;
            }
        }
        Vector2 position
        {
            get
            {
                if(backWindow == null)
                {
                    return texts
                         .Select(textObj => textObj.GetComponent<RectTransform>().localPosition)
                         .Aggregate((vec1, vec2) => vec1 + vec2) / texts.Count;
                }
                return MathV.scaling(backWindow.position, baseMas);
            }
        }
        Vector2 textArea
        {
            get
            {
                var positions = texts
                    .Select(text => text.GetComponent<RectTransform>().localPosition);
                var upper = positions.Max(position => position.y);
                var righter = positions.Max(position => position.x);
                var downer = positions.Min(position => position.y);
                var lefter = positions.Min(position => position.x);
                return new Vector2(righter - lefter, upper - downer);
            }
        }
    }
}