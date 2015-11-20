using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Ionic.Zlib;
#if !UNITY_WP8
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat;
#else
using ObscuredInt = System.Int32;
using ObscuredFloat = System.Single;
#endif
#if UNITY_EDITOR
//using Exception = System.AccessViolationException;
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
//using UnityEditor;
//using UnityEditorInternal;
using System.Threading;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class Base2 : Base
{
    public static void ClearLog()
    {
#if (UNITY_EDITOR)
        //Assembly assembly = Assembly.GetAssembly(typeof(UnityEditorInternal.Macros));
        //Type type = assembly.GetType("UnityEditorInternal.LogEntries");
        //MethodInfo method = type.GetMethod("Clear");
        //method.Invoke(new object(), null);
#endif
    }
    public void SetDirty(MonoBehaviour g = null)
    {
#if (UNITY_EDITOR)
        if (g == null) g = this;
        UnityEditor.EditorUtility.SetDirty(g);
#endif
    }
    public  static IEnumerator AddMethod(float seconds, Action act)
    {
        //StopAllCoroutines();
        yield return new WaitForSeconds(seconds);
        act();
    }
    public  IEnumerator AddMethodCor(float seconds, Func<IEnumerator> act)
    {
        //StopAllCoroutines();
        yield return new WaitForSeconds(seconds);
        yield return StartCoroutine(act());
    }
    public static IEnumerator AddMethod(Action act)
    {
        //StopAllCoroutines();
        yield return null;
        act();
    }
    public static IEnumerator AddMethod(YieldInstruction y, Action act)
    {
        //StopAllCoroutines();
        yield return y;
        act();
    }

    public static IEnumerator AddMethod(WWW y, Action act)
    {
        //StopAllCoroutines();
        yield return y;
        act();
    }
    public static IEnumerator AddMethod(Func<bool> y, Action act)
    {        
        //StopAllCoroutines();
        while (!y())
            yield return null;
        act();
    }
    public static IEnumerator AddUpdate(Func<bool> y)
    {
        //StopAllCoroutines();
        yield return null;
        while (y())
            yield return null;
    }



    static Dictionary<string, SecureInt> PlayerPrefInt = new Dictionary<string, SecureInt>(StringComparer.OrdinalIgnoreCase);
    static Dictionary<string, SecureFloat> PlayerPrefFloat = new Dictionary<string, SecureFloat>(StringComparer.OrdinalIgnoreCase);
    public static Dictionary<string, string> PlayerPrefString = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    static Dictionary<string, List<string>> PlayerPrefStringList = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
    static Dictionary<string, bool> PlayerPrefBool = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);


    public static void PlayerPrefsClear()
    {
        PlayerPrefsRefresh();
        PlayerPrefs.DeleteAll();
    }
    public static void PlayerPrefsRefresh() 
    {
        PlayerPrefStringList.Clear();
        PlayerPrefInt.Clear();
        PlayerPrefFloat.Clear();
        //PlayerPrefString.Clear();
        PlayerPrefBool.Clear();
        //playerPrefKeys.Clear();
    }
    public static bool PlayerPrefsGetBool(string s2, bool defValue = false)
    {
        bool v;
        if (PlayerPrefBool.TryGetValue(s2, out v))
            return v;
        return PlayerPrefBool[s2] = boolParse(PlayerPrefsGetString(s2), defValue);
    }
    public static void PlayerPrefsSetBool(string s, bool value)
    {
        bool v;
        if (PlayerPrefBool.TryGetValue(s, out v) && v == value) return;
        PlayerPrefBool[s] = value;
        PlayerPrefsSetString(s, value.ToString());
    }
    public static int PlayerPrefsGetInt(string s2, int defValue = 0)
    {
        SecureInt v;
        if (PlayerPrefInt.TryGetValue(s2, out v))
            return v;
        
        return PlayerPrefInt[s2] = intParse(PlayerPrefsGetString(s2),defValue);
    }
    public static void PlayerPrefsSetInt(string s, int value)
    {
        SecureInt v;
        if (PlayerPrefInt.TryGetValue(s, out v) && v == value) return;
        PlayerPrefInt[s] = value;
        PlayerPrefsSetString(s, value.ToString());
    }
    public static float PlayerPrefsGetFloat(string s2, float defValue = 0)
    {
        SecureFloat v;
        if (PlayerPrefFloat.TryGetValue(s2, out v))
            return v;
        return PlayerPrefFloat[s2] = floatParse(PlayerPrefsGetString(s2), defValue);
    }
    public static void PlayerPrefsSetFloat(string s, float value)
    {
        SecureFloat v;
        if (PlayerPrefFloat.TryGetValue(s, out v) && v == value) { /*Profiler.EndSample(); */return; }
        PlayerPrefFloat[s] = value;
        PlayerPrefsSetString(s, value.ToString());
    }
    public static string PlayerPrefsGetString(string s2, string defValue = "")
    {
        string v;
        if (PlayerPrefString.TryGetValue(s2, out v))
            return v;
        //if (bs.setting.disablePlayerPrefs)
            //return defValue;
        playerPrefKeys.Add(s2);
        return PlayerPrefString[s2] = PlayerPrefs.GetString(s2.ToLower(), defValue);
    }
    public static Dictionary<string, string> SetStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private static bool boolParse(string getString, bool def)
    {
        bool s;
        if (Boolean.TryParse(getString, out s))
            return s;
        return def;
        //try
        //{
        //    return bool.Parse(getString);
        //} catch (System.Exception)
        //{
        //    return def;
        //}
    }
    private static int intParse(string getString, int def)
    {
        int a;
        if (int.TryParse(getString, out a))
            return a;
        return def;
        //try
        //{
        //    return int.Parse(getString);
        //} catch (Exception)
        //{
        //    return def;

        //}
    }
    private static float floatParse(string s1, float def)
    {
        float f;
        if (float.TryParse(s1, out f))
            return f;
        return def;
    }
    public static void PlayerPrefsSetString(string s, string value)
    {
        string v;
        if (PlayerPrefString.TryGetValue(s, out v) && v == value) return;
        PlayerPrefString[s] = value;
        SetStrings[s] = value;
        
        //if (setKey)//&& !playerPrefKeys.Contains(s)
            playerPrefKeys.Add(s);
    }

    
    public static void PlayerPrefsSetStringList(string s, List<string> value)
    {
        PlayerPrefStringList[s] = value;
        PlayerPrefsSetString(s, string.Join("\n", value.ToArray()));
    }
    public static List<string> PlayerPrefsGetStrings(string s2, string def = "")
    {
        List<string> v;
        if (PlayerPrefStringList.TryGetValue(s2, out v))
            return v;
        return PlayerPrefStringList[s2] = new List<string>(PlayerPrefsGetString(s2, def).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
    }

    public static HashSet<string> m_playerPrefKeys;

    public static HashSet<string> playerPrefKeys
    {
        get
        {
            if (m_playerPrefKeys == null)
            {
                LoadKeys();
            }
            
            return m_playerPrefKeys;
        }
        set
        {
            m_playerPrefKeys = value;
        }
    }
    public const string keysNew3 = "keysnew3";
    public static void LoadKeys()
    {
        Profiler.BeginSample("LoadKeys");
        m_playerPrefKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (bs.setting.disablePlayerPrefs)
            return;
        var s = UnityEngine.PlayerPrefs.GetString(bs.keysNew3);
        if(!string.IsNullOrEmpty(s))
        {
            s = GZipStream.UncompressString(Convert.FromBase64String(s));
            m_playerPrefKeys = new HashSet<string>(s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
            Debug.Log(m_playerPrefKeys.Count + " Keys Loaded3");
            if(m_playerPrefKeys.Count >10)
                UnityEngine.PlayerPrefs.DeleteKey("keysnew2");
        }
        else if (!string.IsNullOrEmpty(s = UnityEngine.PlayerPrefs.GetString("keysnew2")))
        {
            m_playerPrefKeys = new HashSet<string>(s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
            print(m_playerPrefKeys.Count + " Keys Loaded");
        }
        else
        {
            bool hasKey = false;
            for (int i = 0, j = 0; j < 10 || hasKey; i++)
            {
                hasKey = PlayerPrefs.HasKey("keys" + i);
                if (hasKey)
                    m_playerPrefKeys.Add(PlayerPrefs.GetString("keys" + i));
                else
                    j++;
            }

            print("Old " + m_playerPrefKeys.Count + " Keys Loaded");
        }

        //if (!string.IsNullOrEmpty(value))
        //{
        //    var bts = Convert.FromBase64String(value);
        //    using (var ms = new BinaryReader(bts))
        //    {
        //        while (ms.Position < ms.Length)
        //        {
        //            m_playerPrefKeys.Add(ms.ReadString());
        //        }
        //    }
        //    print("New " + m_playerPrefKeys.Count + " Keys Loaded");
        //}

        Profiler.EndSample();
    }
    public class PlayerPrefs //: CodeStage.AntiCheat.PlayerPrefsObscured
    {
        public static bool HasKey(string key)
        {            
            //if (PlayerPrefString.ContainsKey(key))
                //return true;
            if (bs.setting.disPlayerPrefs2)
                return false;
#if !UNITY_WP8
            if (bs.setting.playerPrefSecurity)
                return PlayerPrefsObscured.HasKey(key);
#endif
            return UnityEngine.PlayerPrefs.HasKey(key);
        }
        public static void SetString(string key, string value)
        {

            if (bs.setting.disPlayerPrefs2)
                return;
            try
            {
#if !UNITY_WP8
                if (bs.setting.playerPrefSecurity)
                    PlayerPrefsObscured.SetString(key, value);
                else
#endif
                    UnityEngine.PlayerPrefs.SetString(key, value);
            }
            catch (Exception )
            {
                //Debug.LogError(e.Message + "(" + key + ":" + value.Length + ")");
                Loader.errors++;
            }
        }

        public static void SetString2(string key, string value)
        {
            if (bs.setting.disPlayerPrefs2)
                return;
            try
            {
                UnityEngine.PlayerPrefs.SetString(key, value);
            }
            catch (Exception)
            {
                //Debug.LogError(e.Message + "(" + key + ":" + value.Length + ")");
                Loader.errors++;
            }
        }
        public static string GetString(string key, string defaultValue = "")
        {
            if (bs.setting.disPlayerPrefs2)
                return defaultValue;
#if !UNITY_WP8
            if (bs.setting.playerPrefSecurity)
                return PlayerPrefsObscured.GetString(key, defaultValue);
#endif
            return UnityEngine.PlayerPrefs.GetString(key, defaultValue);
        }

        internal static float GetFloat(string p1, float p2=0)
        {
          
#if !UNITY_WP8
            if (bs.setting.playerPrefSecurity)
                return PlayerPrefsObscured.GetFloat(p1, p2);
#endif
            return UnityEngine.PlayerPrefs.GetFloat(p1, p2);
        }

        internal static void SetFloat(string p, float totalSeconds)
        {
            if (bs.setting.disPlayerPrefs2)
                return;
            try
            {
#if !UNITY_WP8
                if (bs.setting.playerPrefSecurity)
                    PlayerPrefsObscured.SetFloat(p, totalSeconds);
                else
#endif
                UnityEngine.PlayerPrefs.SetFloat(p, totalSeconds);
            }
            catch (Exception e) { Debug.LogError(e.Message); }
        }


        internal static int GetInt(string p, int p2 = 0)
        {
           
#if !UNITY_WP8
            if (bs.setting.playerPrefSecurity) 
                return PlayerPrefsObscured.GetInt(p, p2);
#endif
            return UnityEngine.PlayerPrefs.GetInt(p, p2);
        }

        internal static void SetInt(string p, int totalSeconds)
        {

            if (bs.setting.disPlayerPrefs2)
                return;
            try
            {
#if !UNITY_WP8
                if (bs.setting.playerPrefSecurity)
                    PlayerPrefsObscured.SetInt(p, totalSeconds);
                else
#endif
                    UnityEngine.PlayerPrefs.SetInt(p, totalSeconds);
            }
            catch (Exception e) { Debug.LogError(e.Message); }
        }

        internal static void DeleteAll()
        {
            UnityEngine.PlayerPrefs.DeleteAll();
#if !UNITY_WP8
            PlayerPrefsObscured.DeleteAll();
#endif
        }

        

        internal static void DeleteKey(string p)
        {
#if !UNITY_WP8
            if (bs.setting.playerPrefSecurity)
                PlayerPrefsObscured.DeleteKey(p);
            else
#endif
                UnityEngine.PlayerPrefs.DeleteKey(p);
        }

        internal static void Save()
        {
            try
            {
                if (bs.setting.playerPrefSecurity) 
                    PlayerPrefsObscured.Save();
                else
                    UnityEngine.PlayerPrefs.Save();
            }
            catch (Exception e) { Debug.LogError(e.Message); }
        }

        
    }

    public T SetSecure<T>(T value, string s)
    {
        PlayerPrefsSetBool(GetHash(value, s), true);
        return value;
    }
    private static string GetHash<T>(T value, string s)
    {
        return (bs._Loader.playerName.GetHashCode() ^ s.GetHashCode() ^ value.GetHashCode()).ToString();
    }
    public T GetSecureOff<T>(T o, string s, T def = default (T))
    {
        return o;
    }
    public T GetSecure<T>(T o, string s, T def = default(T))
    {
        if (PlayerPrefsGetBool(GetHash(o, s)))
            return o;
        return def;
    }
    
}