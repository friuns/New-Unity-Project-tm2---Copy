
//using UnityEditor;

using System.Diagnostics;
using System.Text.RegularExpressions;
using gui = UnityEngine.GUILayout;
//using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class GuiClasses : bs
{
    public float HorizontalSlider(string label,float value, float leftValue, float rightValue)
    {
        gui.BeginVertical();
        gui.Label(Tr(label) + ":" + Math.Round(value,2));
        var horizontalSlider = GUILayout.HorizontalSlider(value, leftValue, rightValue);
        gui.EndVertical();
        return horizontalSlider;
    }
    public static string CreateTable(string source)
    {
        string table = "";
        MatchCollection m = Regex.Matches(source, @"\w*\s*");
        for (int i = 0; i < m.Count - 1; i++)
            table += "{" + i + ",-" + m[i].Length + "}";
        return table;
    }
    protected static int cnt;
    private bool[] toggledFlags2 = new bool[50];
    private string[] toggledFlagsstr = new string[50];
    public void ToggleTab(string name,bool b = true)
    {
        for (int i = 0; i < 50; i++)
        {
            if (toggledFlagsstr[i] == name)
                toggledFlags2[i] = b;
        }
    }
    public bool BeginVertical(string name)
    {
        cnt++;
        toggledFlags2[cnt] = gui.Toggle(toggledFlags2[cnt], Tr(name), skin.GetStyle("ToolbarDropDown"));
        toggledFlagsstr[cnt] = name;
        if (toggledFlags2[cnt])
            gui.BeginVertical(skin.GetStyle("CN Box"));
        return toggledFlags2[cnt];
    }

    //public bool BeginVertical(string name)
    //{
    //    cnt++;
    //    //gui.BeginVertical("Terrain", skin.window);
    //    var cnt2 = cnt;

    //    //gui.BeginVertical(name, skin.window);
    //    var toggle = (cnt2 & toggledFlags) == cnt2;

    //    if (gui.Toggle(toggle, name))
    //        toggledFlags |= cnt2;
    //    else
    //        toggledFlags &= cnt2;

    //    return toggle;
    //}
    public bool BackButton(string s = "Back")
    {
        return Button(s,false) || Input.GetKeyDown(KeyCode.Escape);
    }
    public bool BackButtonLeft()
    {
        return ButtonLeft("Back") || Input.GetKeyDown(KeyCode.Escape);
    }
    public static void LoadTranslate()
    {

        dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        tooltips = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (setting.disableTranslate)
            return;
        Profiler.BeginSample("LoadTranslate");
        //Load(setting.assetDictionaries[0]);
        Load(assetDictionary);
        Profiler.EndSample();
    }
    private static void Load(TextAsset AssetDictionary)
    {
        var lines = AssetDictionary.text.Split(new[] { ";\n", "; \n", ";\r\n", "; \r\n" }, StringSplitOptions.RemoveEmptyEntries);
        print("LoadTranslate " + AssetDictionary.name + lines.Length);
        foreach (var a in lines)
        {
            var ss = a.Split(';');
            string key = ss[0];
            if (ss.Length == 1)
                dict[key] = key;
            if (ss.Length >= 2)
                dict[key] = ss[1];
            if (ss.Length >= 3)
            {
                tooltips[key] = ss[2];
            }
        }
    }
    public static TextAsset assetDictionary { get { return setting.assetDictionaries[Mathf.Max(0, curDict)]; } }
    protected static int curDict
    {
        get
        {
            return PlayerPrefs.GetInt("Dict", _Loader.vkSite ? 1 : 0);
        }
        set
        {
            if (curDict != value)
            {
                PlayerPrefs.SetInt("Dict", value);
                dict = null;
                trcache.Clear();
            }
        }
    }

    public bool GlowButton(string s,bool glow)
    {

        Texture2D old = skin.button.normal.background;
        skin.button.normal.background = glow ? skin.button.active.background : skin.button.normal.background;
        bool pressed = SoundButton(gui.Button(Tr(s), gui.MinWidth(50)));
        skin.button.normal.background = old;
        return pressed;
    }
    public bool GlowButton(GUIContent s, bool glow)
    {
        Texture2D old = skin.button.normal.background;
        skin.button.normal.background = glow ? skin.button.active.background : skin.button.normal.background;
        bool pressed = SoundButton(gui.Button(s, gui.MinWidth(50)));
        skin.button.normal.background = old;
        return pressed;
    }

    public int Toolbar(int id, IList<string> getNames, bool expand, bool center = false, int limit = 99, int hor = -1, bool useSkin = true)
    {
        if (getNames[Mod(id, getNames.Count)] == null)
            for (int i = 0; i < getNames.Count && i < limit && getNames[i] == null; i++)
                id = i;
        if(useSkin)
        gui.BeginVertical(skin.box);
        gui.BeginHorizontal();
        if (center)
            gui.FlexibleSpace();
        for (int i = 0,j=0; i < getNames.Count && i < limit; i++)
        {
            if (hor != -1 && j % hor == 0 && j != 0)
            {
                gui.EndHorizontal();
                gui.BeginHorizontal();
            }
            if (getNames[i] == null)
                continue;
            j++;
            Texture2D old = skin.button.normal.background;
            skin.button.normal.background = id == i ? skin.button.active.background : skin.button.normal.background;
            if (JoystickButton2(skin.button, id == i) || SoundButton(gui.Button(GUIContent(Tr(getNames[i]), null, Tp(getNames[i])), gui.ExpandWidth(expand), gui.MinWidth(50))))
                id = i;
            skin.button.normal.background = old;
        }
        if (center)
            gui.FlexibleSpace();
        gui.EndHorizontal();
        if (useSkin)
        gui.EndVertical();
        return id;
    }

    //public int Toolbar(int id, string[] getNames, bool expand, bool center = false)
    //{
    //    gui.BeginHorizontal();
    //    if(center)
    //    gui.FlexibleSpace();
    //    //skin.button.normal = skin.button.onNormal;
    //    for (int i = 0; i < getNames.Length; i++)
    //    {
    //        //Texture2D old = skin.button.normal.background;
    //        //skin.button.normal = id == i ? skin.button.active : skin.button.normal;
    //        if (JoystickButton2(skin.button, id == i) || SoundButton(gui.Button(Tr(getNames[i]), gui.ExpandWidth(expand), gui.MinWidth(50))))
    //            id = i;
    //        //skin.button.normal.background = old;
    //    }
    //    if (center)
    //        gui.FlexibleSpace();
    //    gui.EndHorizontal();
    //    return id;
    //}
    //public static List<string> keys;
    private static void SaveTr(string s)
    {
#if UNITY_EDITOR

        if (!dict.ContainsKey(s))
        {
            dict.Add(s, "");
            Debug.LogWarning("Write " + s);
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(assetDictionary);
            var sb = new StringBuilder();
            foreach (var a in dict)
                sb.AppendLine(a.Key + ";" + a.Value + ";");
            WriteAllText(assetPath, sb.ToString());
            
        }
#endif
    }
    private static void WriteAllText(string assetPath, string s)
    {
#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(assetPath))
            using (var f = new FileStream(assetPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var bytes = Encoding.UTF8.GetBytes(s);
                f.Write(bytes, 0, bytes.Length);
            }
#endif
    }

    
    public string Tp(string s)
    {
        if (string.IsNullOrEmpty(s) || s.Length > 300) return "";
        if (setting.disableTranslate)
            return s;
        string d;
        tooltips.TryGetValue(s, out d);
        return d;//string.IsNullOrEmpty(d) ? "" : "["+s+"]\n" + d;
    }
    public static bool skipNext;
    public string Trs(string s)
    {
        return Tr(s, true);
    }
    public string Trn(string s)
    {
        return Tr(s, false, false);
    }
    public static Dictionary<string, string> trcache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public static string Tr(string s, bool sk = false, bool save = true)
    {
        if (setting.disableTranslate || string.IsNullOrEmpty(s))
            return s;
        if (skipNext)
        {
            skipNext = sk;
            return s;
        }
        skipNext = sk;
        if (string.IsNullOrEmpty(s) || s.Length > 300 ) return s;
        string h;
        if (trcache.TryGetValue(s, out h)) { return h; }
        string trim = s.Replace("\r", "\\r").Replace("\n", "\\n");
        string end;
        string start;
        trim = Trim(trim, out start, out end);
        if (string.IsNullOrEmpty(trim)) return s;
        if (Application.isEditor && resEditor.saveTr && save)
            SaveTr(trim);
        string d;
        dict.TryGetValue(trim, out d);
        
        return trcache[s] = string.IsNullOrEmpty(d) ? s : start + unescape(d) + end;
    }

    private static string Trim(string s, out string start, out string end)
    {
        var c = new char[] { ':', ' ', '\t', '.', ',', '>', '<', '\n' };
        start = s.TrimStart(c);
        start = s.Substring(0, s.Length - start.Length);
        end = s.TrimEnd(c);
        end = s.Substring(end.Length);
        s = s.Substring(start.Length, Mathf.Max(0, s.Length - end.Length - start.Length));
        return s;
    } 
    
    
    public void Label(string s, int fontSize = 16, bool wrap = true, bool expand = false)
    {
        skin.label.imagePosition = ImagePosition.ImageLeft;
        skin.label.alignment = TextAnchor.UpperLeft;
        skin.label.wordWrap = wrap;
        skin.label.fontSize = fontSize;
        gui.Label(Tr(s), gui.ExpandWidth(expand));
    }
    public bool Toggle(bool toggle, string text)
    {
        bool b = gui.Toggle(toggle, GUIContent(Tr(text), null, Tp(text)), gui.ExpandWidth(false));
        SoundButton(toggle != b);
        return b;
    }
    public void LabelCenter(string s, int fontSize = 16, bool wrap = false, Texture2D txt = null, GUIStyle style = null)
    {
        if (style == null) style = skin.label;
        style.wordWrap = wrap;
        style.fontSize = fontSize;
        style.alignment = TextAnchor.UpperCenter;
        gui.Label(GUIContent(Tr(s), txt), style);
        skin.label.alignment = TextAnchor.UpperLeft;
    }
    public static GUIStyle replacementStyle;
    public bool ButtonTexture(string s, Texture2D texture, int height)
    {
        return gui.Button(new GUIContent(s, texture), gui.Height(height));
    }
    public bool ButtonTexture(string s, Texture2D texture, bool expandWidth = true, int font = 14, bool bold = false)
    {
        return Button(s, expandWidth, font, bold, texture);
    }

    public bool ButtonLeft(string s, Texture2D txt = null, float height = 0, GUIStyle style = null, int fontsize = 12)
    {
        gui.BeginHorizontal();
        gui.FlexibleSpace();
        skin.button.fontSize = fontsize;
        GUI.SetNextControlName(s);
        if (JoystickButton2(skin.button)) return true;
        var button = gui.Button(GUIContent(Tr(s), txt), style ?? skin.button, gui.ExpandWidth(false), height == 0 ? gui.MinWidth(100) : gui.Height(height));
        SoundButton(button);
        gui.EndHorizontal();
        return button;
    }
    
    public bool Button(string s, bool expandWidth = true, int font = 14, bool bold = false, Texture2D texture = null, bool wrap = false)
    {
        var guiStyle = replacementStyle == null ? skin.button : replacementStyle;
        guiStyle.fontSize = font;
        guiStyle.wordWrap = wrap;
        guiStyle.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        GUI.SetNextControlName(s);
        if (JoystickButton2(guiStyle)) return true;
        var button = gui.Button(GUIContent(Tr(s), texture, Tp(s)), guiStyle, gui.ExpandWidth(false), gui.MinWidth(100), gui.ExpandWidth(expandWidth));
        return SoundButton(button);
    }
    //public static bool JoystickButton(ref string s)
    //{

    //    if (CustomWindow.buttonId++ == CustomWindow.curButton)
    //    {
    //        if (CustomWindow.backSpaceDown)
    //        {
    //            CustomWindow.backSpaceDown = false;
    //            return SoundButton(true);
    //        }
    //        s = ">" + s;
    //        print(s);
    //    }
    //    return false;
    //}

    public static bool JoystickButton2(GUIStyle style, bool sel=false)
    {
        if (!setting.useKeysForGui) return false;
        if (CustomWindow.buttonId == -1) 
            return false;
        var selected = CustomWindow.buttonId++ == CustomWindow.curButton;
        style.normal = selected || sel ? style.hover : style.onNormal;
        if (selected && CustomWindow.backSpaceDown)
        {
            CustomWindow.backSpaceDown = false;
            return SoundButton(true);
        }
        return false;
    }


    public void ShowWindow(Action func, Action back = null)
    {
        win.ShowWindow(func, back ?? win.act);
    }
    //public void ShowWindow(Action func, Action back)
    //{
    //    win.ShowWindow(func, back);
    //}
    public void ShowWindowNoBack(Action func)
    {
        win.ShowWindow(func, null);
    }
    public void Setup(int x = 400, int y = 300, string tittle = "", Dock dock = Dock.Center)
    {
        win.Setup(x, y, tittle, dock);
    }
    public static bool SoundButton(bool button)
    {
        if (UnityEngine.Input.GetMouseButtonUp(0) && button)
            PlayOneShotGui(_Loader.pushButton);
        return button;
    }
    
    public void Popup(string msg, Action back = null, string button = null, bool skip = true)
    {
        Popup2(msg, back ?? win.act, button, skip);
    }
    public void Popup(string msg,Action back, int width,int height)
    {
        Popup2(msg, back, null, false, width, height);
    }
    public void Popup(string msg, Texture2D txt)
    {
        Popup2(msg, win.act, null, false, 500, 250, txt);
    }
    internal string popupText;
    //public void Popup2(string msg, int width = 500, int height = 250, Texture2D txt = null)
    //{
    //    print(msg);
    //    popupText = msg;
    //    win.ShowWindow(delegate
    //    {
    //        win.Setup(width, height, "", Dock.Center, txt);
    //        LabelCenter(popupText, 20, true);
    //        if (BackButtonLeft())
    //            win.Back();
    //    }, null, true);
    //}
    [DebuggerStepThrough]
    public void Popup2(string msg, Action back = null, string button = null, bool skip = true, int width = 500, int height = 250, Texture2D txt = null)
    {
        print(msg);
        popupText = msg;
        if (button == null) button = "back";
        win.ShowWindow(delegate
        {
            win.Setup(width, height, "", Dock.Center, txt);
            BeginScrollView() ;
            LabelCenter(popupText, 20, true);
            gui.EndScrollView();
            if (back != null && ButtonLeft(button))
                win.ShowWindow(back, null, skip);
        }, null, skip);
    }


    private static Dictionary<string, string> m_dict;
    internal static Dictionary<string, string> dict
    {
        get
        {
            if (m_dict == null)
                LoadTranslate();
            return m_dict;
        }
        set { m_dict = value; }
    }
    private static Dictionary<string, string> tooltips = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public GUISkin skin
    {
        get { if (win.skin == null) win.skin = GUI.skin; return win.skin; }
    }
    internal static Vector2 scroll;
    public void BeginScrollView(GUIStyle style = null, bool showHorizontal = false,bool showVertical=true)
    {

        if (style == null) style = skin.scrollView;
        skin.scrollView.fixedWidth = 0;
        Vector2 bsv = gui.BeginScrollView(scroll, false, false, showHorizontal ? skin.horizontalScrollbar : GUIStyle.none, showVertical? skin.verticalScrollbar:GUIStyle.none, style);
        if (bsv == scroll)
        {
            Vector2 mouseDelta = getMouseDelta(false);
            bsv += new Vector2(-mouseDelta.x, mouseDelta.y) * 3;
        }
        scroll = bsv;
    }
    public Vector2 BeginScrollView(Vector3 v)
    {
        return gui.BeginScrollView(v);
    }

    

}
