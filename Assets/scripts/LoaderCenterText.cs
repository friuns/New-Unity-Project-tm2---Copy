#define GA

using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
#if !UNITY_WP8
using CodeStage.AntiCheat;
using CodeStage.AntiCheat.ObscuredTypes;
#endif
using gui = UnityEngine.GUILayout;
#if !UNITY_FLASH || UNITY_EDITOR
using System.Linq;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Prefs = UnityEngine.PlayerPrefs;
#if UNITY_EDITOR
//using Exception = System.AccessViolationException;
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


public partial class Loader
{

    List<StringTime> centerTextList = new List<Loader.StringTime>();
    class StringTime
    {
        public string s;
        public float f;
        public override bool Equals(object obj)
        {
            return ((StringTime)obj).s == s;
        }

        public override int GetHashCode()
        {
            return s.GetHashCode();
        }
    }
    internal float lastTextTime = MinValue;
    public GUIText CenterText;
    public GUITexture CenterTextBackground;
    public void UpdateCenterText()
    {
        CenterTextBackground.enabled = CenterText.enabled = centerTextList.Count > 0;
        if (centerTextList.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var a in centerTextList)
            {
                sb.AppendLine(a.s);
                if (a.f < 0)
                {
                    centerTextList.Remove(a);
                    break;
                }
                a.f -= Time.deltaTime;
            }
            CenterText.text = sb.ToString();
        }
        
        //if (Time.time - lastTextTime > 0 && centerTextList.Count > 0)
        //{
        //    var stringTime = centerTextList.Dequeue();
        //    CenterText.text = stringTime.s;
        //    lastTextTime = Time.time + stringTime.f;
        //}
    }
    [RPC]
    public void centerText(string s, float seconds = 4, bool fast = false)
    {
        //if (Time.time - lastTextTime < -1)
        //{

        var stringTime = new Loader.StringTime() { f = seconds, s = s };
        //if (fast)
        //{
        //    centerTextList.Clear();
        //    lastTextTime = float.MinValue;
        //}
        if (!centerTextList.Contains(stringTime))
            centerTextList.Add(stringTime);
        //}
        //lastTextTime = Time.time;
        //if (olds == s) return;
        //olds = s;
        //CenterText.text = s;
    }

}