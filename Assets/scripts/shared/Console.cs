#define GA
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text.RegularExpressions;

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
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


public class Console:GuiClasses 
{

    private string search = "";
    public bs[] list;
    public void OnEnable()
    {
        //list = Resources.FindObjectsOfTypeAll<bs>();
        list = GameObject.FindObjectsOfType<bs>();
    }
    static BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
    public void OnGUI()
    {
        GUI.depth = -999;
        gui.BeginArea(new Rect(0,0,Screen.width,Screen.height),skin.window);
        
        StringBuilder sb = new StringBuilder();
        var ss = search.Split('.');
        if (ss[0].Length >= 2)
            foreach (var ago in list.Where(a => a != null && a.name.ToLower().Contains(ss[0])))
            {
                var q = new Queue<string>(ss);
                q.Dequeue();
                sb.AppendLine(ago.name + "(" + ago.GetType()+ ")" + "_________________________");
                if (q.Count > 0)
                    Mfsa(q, ago, sb);
            }
        BeginScrollView();
        gui.Label(sb.ToString(),skin.textArea);
        gui.EndScrollView();
        GUI.SetNextControlName("a");
        search = gui.TextField(search);
        if (gui.Button("close"))
            enabled = false;
        gui.EndArea();
        GUI.FocusControl("a");
     
    }
    private static void Mfsa(Queue<string> q, object ago, StringBuilder sb)
    {

        string ss1 = q.Dequeue();
        if (ss1.Length >= 2)
        {
            foreach (FieldInfo f in ago.GetType().GetFields(flags))
            {
                if (f.Name.ToLower().Contains(ss1))
                {
                    if (q.Count > 0)
                        Mfsa(q, f.GetValue(ago), sb);
                    else
                        sb.AppendLine(f.Name + ":" + f.GetValue(ago));
                }
            }
            foreach (PropertyInfo f in ago.GetType().GetProperties(flags))
            {
                try
                {
                    if (f.Name.ToLower().Contains(ss1))
                        if (q.Count > 0)
                            Mfsa(q, f.GetValue(ago, null), sb);
                        else
                            sb.AppendLine(f.Name + ":" + f.GetValue(ago, null));
                }catch (Exception){ }
            }
        }

    }
}