using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

public class InputManager : bs
{
    internal List<KeyValue> keys = new List<KeyValue>();
    public bool secondPlayer { get { return pl != null && pl.secondPlayer; } }
    public Player pl;
    internal KeyValue[] alternatives { get { return m_alternatives == null ? InitDict() : m_alternatives; } }
    private static KeyValue[] m_alternatives;
    public void Start()
    {
        if (m_alternatives == null)
            InitDict();
        if (pl != null && pl.androidHud != null && pl.androidHud)
            dict = pl.androidHud.dict;
    }
    public KeyValue[] InitDict()
    {
        m_alternatives = new KeyValue[400];
        KeyCode joystick1Button2 = splitScreen ? KeyCode.Joystick1Button2 : KeyCode.JoystickButton2;
        KeyCode joystick1Button0 = splitScreen ? KeyCode.Joystick1Button0 : KeyCode.JoystickButton0;
        KeyCode joystick1Button1 = splitScreen ? KeyCode.Joystick1Button1 : KeyCode.JoystickButton1;
        KeyCode joystick1Button3 = splitScreen ? KeyCode.Joystick1Button3 : KeyCode.JoystickButton3;
        Add(KeyCode.E, "Revert Time", KeyCode.Backspace, KeyCode.Mouse2, joystick1Button2, KeyCode.Joystick2Button2, KeyCode.Insert);
        Add(KeyCode.Escape, "Menu", KeyCode.M, KeyCode.F1, KeyCode.F10, KeyCode.JoystickButton4);
        Add(KeyCode.R, "Restart Level", KeyCode.Delete, KeyCode.JoystickButton7);
        Add(KeyCode.F, "Horn", KeyCode.JoystickButton6);
        Add(KeyCode.LeftShift, "Nitro", KeyCode.RightControl, KeyCode.Mouse4, KeyCode.JoystickButton5);
        //Add(KeyCode.RightControl, "Drift", KeyCode.LeftControl,KeyCode.J);
        Add(KeyCode.S, "Down/Brake", KeyCode.DownArrow, joystick1Button0, KeyCode.Joystick2Button0, KeyCode.Space);
        Add(KeyCode.Space, "Brake", KeyCode.End, joystick1Button1, KeyCode.Joystick2Button1, KeyCode.Mouse1);
        Add(KeyCode.W, "Up", KeyCode.UpArrow, joystick1Button3, KeyCode.Joystick2Button3, _Loader.dm ? KeyCode.JoystickButton9 : KeyCode.Mouse0);
        Add(KeyCode.A, "Left", KeyCode.LeftArrow);
        Add(KeyCode.D, "Right", KeyCode.RightArrow);
        Add(KeyCode.Q, "Look Back");
        Add(KeyCode.Mouse1, "Zoom");
        Add(KeyCode.Mouse2, "Zoom Hold");
        Add(KeyCode.Return, "Chat");
        Add(KeyCode.Y, "Voice Chat");
        Add(KeyCode.U, "Headlights");
        Add(KeyCode.X, "Jump", KeyCode.G);
        Add(KeyCode.F1, "Screenshot");
        Add(KeyCode.F12, "FullScreen");
        return m_alternatives;
    }
    public void Add(KeyCode key, string descr, params KeyCode[] alt)
    {
        var l = new List<KeyCode>(alt.Take(1));
        l.Insert(0, key);
        foreach (KeyCode a in l)
            KeyBack[a] = key;
        var keyValue = new KeyValue() { descr = descr, keyCodeAlt = l.ToArray(), main = key };
        keyValue.Load();
        keys.Add(keyValue);
        m_alternatives[(int)key] = keyValue;
    }
    public Dictionary<KeyCode, KeyCode> KeyBack = new Dictionary<KeyCode, KeyCode>();
    public void Update()
    {
    }
    public bool GetKey(KeyCode key)
    {
        return GetKey(key, KeyAction.Key);
    }
    public enum KeyAction { KeyDown, KeyUp, Key }
    public bool GetKeyDown(KeyCode key)
    {
        return GetKey(key, KeyAction.KeyDown);
    }
    public bool GetKeyUp(KeyCode key)
    {
        return GetKey(key, KeyAction.KeyUp);
    }
    public bool GetKey(KeyCode key, KeyAction GetKey)
    {
        
        KeyValue kv = alternatives[(int)key];
        if (kv == null) return ParseAction(GetKey, key);
        KeyCode[] keyCodeAlt = kv.keyCodeAlt;
        for (int i = 0; i < keyCodeAlt.Length; i++)
        {
            if (ParseAction(GetKey, keyCodeAlt[i]) && (!bs.splitScreen || i % 2 == 0 && !secondPlayer || i % 2 == 1 && secondPlayer))
                return true;
        }
        return false;
    }
    public Dictionary<KeyCode, AndroidHud.KeyHudBool> dict;
    bool ParseAction(KeyAction a, KeyCode key)
    {
        if (android)
        {
            AndroidHud.KeyHudBool val;
            if (dict != null && dict.TryGetValue(key, out val))
            {
                AndroidHud.KeyHudBool keyHudBool = val;
                if (keyHudBool.down && a == KeyAction.KeyDown)
                    return true;
                if (keyHudBool.up && a == KeyAction.KeyUp)
                    return true;
                if (keyHudBool.hold && a == KeyAction.Key)
                    return true;
            }
            if (!(key < KeyCode.Mouse0 || key > KeyCode.Mouse6))
                return false;
        }
        return a == KeyAction.Key ? InputGetKey(key) : a == KeyAction.KeyDown ? Input.GetKeyDown(key) : Input.GetKeyUp(key);
    }
    public static bool enableKeys = true;
    static bool InputGetKey(KeyCode key)
    {
        if (!enableKeys) return false;
        Axis s = Buttons[(int)key];
        if (s != null)
        {
            var axis = Input.GetAxis(s.s);
            if (axis < s.min && s.min < 0 || axis > s.min && s.min > 0)
            {
                return true;
            }
        }
        return Input.GetKey(key);
    }
    public struct DR
    {
        public KeyCode key;
        public Func<KeyCode, bool> func;
        public DR(KeyCode a, Func<KeyCode, bool> b)
        {
            key = a;
            func = b;
        }
    }
    private static Axis[] m_buttons;
    public static Axis[] Buttons
    {
        get
        {
            if (m_buttons == null)
            {
                m_buttons = new Axis[459];
                m_buttons[(int)KeyCode.A] = new Axis() { s = "Horizontal", min = -.1f };
                m_buttons[(int)KeyCode.D] = new Axis() { s = "Horizontal", min = +.1f };
                m_buttons[(int)KeyCode.LeftArrow] = new Axis() { s = "Horizontal2", min = -.1f };
                m_buttons[(int)KeyCode.RightArrow] = new Axis() { s = "Horizontal2", min = +.1f };
            }
            return m_buttons;
        }
    }
    public class Axis
    {
        public string s;
        public float min;
    }
}
[Serializable]
public class KeyValue
{
    public string descr;
    public KeyCode main;
    public KeyCode[] keyCodeAlt;
    
    public void Load()
    {
        for (int i = 0; i < keyCodeAlt.Length; i++)
            keyCodeAlt[i] = (KeyCode)bs.PlayerPrefs.GetInt(main + "" + i, (int)keyCodeAlt[i]);
    }
    public void Save()
    {
        for (int i = 0; i < keyCodeAlt.Length; i++)
            bs.PlayerPrefs.SetInt(main + "" + i, (int)keyCodeAlt[i]);
    }
    public void Reset()
    {
        for (int i = 0; i < keyCodeAlt.Length; i++)
            bs.PlayerPrefs.DeleteKey(main + "" + i);
    }
}
