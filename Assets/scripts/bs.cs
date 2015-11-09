#define GA
#if UNITY_EDITOR
using UnityEditor;
//using Exception = System.AccessViolationException;
using Curvy.Utils;
#endif
//#define UNITY_WEBPLAYER
using System;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Ionic.Zlib;
using Microsoft.Win32;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using Random = System.Random;
using Debug = UnityEngine.Debug;


public partial class bs : Base2
{
    public static bool m_offlineMode;
    public static bool offlineMode { get { return m_offlineMode; } set { PhotonNetwork.offlineMode = m_offlineMode = value; } }

    public static bool online { get { return !offlineMode || _Loader.gameType != GameType.race; } }
    private static Transform m_tempTr;
    public static Transform tempTr { get { return m_tempTr ? m_tempTr : m_tempTr = new GameObject("tempTr").transform; } }
    public static InputManager input { get { return _Loader.inputManger; } }
    public static AutoQuality m_AutoQuality;
    public static AutoQuality _AutoQuality { get { return m_AutoQuality == null ? (m_AutoQuality = (AutoQuality)FindObjectOfType(typeof(AutoQuality))) : m_AutoQuality; } }
    public static Camera mainCamera;
    public static Transform mainCameraTransform;
    //public static MapLoader m_MapLoader; m_MapLoader == null ? (m_MapLoader = _Loader.levelEditor == null ? (MapLoader)FindObjectOfType(typeof(MapLoader)) : _Loader.levelEditor) : m_MapLoader; 
    public static MapLoader _MapLoader { get { return _Loader.mapLoader; } }

    //public static Game m_Game;
    public static Game _Game;
    public static Awards _Awards { get { return _Loader._Awards; } }
    public static TerrainHelper _TerrainHelper;
    public static LoaderScene _LoaderScene;
    public static GameSettings m_GameSettings;
    public static GameSettings _GameSettings
    {
        get
        {
            if (!m_GameSettings)
            {                
                m_GameSettings = (GameSettings) FindObjectOfType(typeof (GameSettings));
                if (!m_GameSettings)
                    m_GameSettings = (GameSettings) Instantiate(res.gameSettings);
            }
            return m_GameSettings;
        }
    }
    public static Pool _Pool;
    public static Player _Player { get { return _Game.m_Player; } set { _Game.m_Player = value; } }
    public static Player _Player2 { get { return _Game.m_Player2; } set { _Game.m_Player2 = value; } }
    public static Res res;//{ get { return _Loader.res; } }
    public static bool isDebug;//{ get { return setting.debug; } }
    public static bool isMod { get { return _Loader.modType >= ModType.mod; } }    
    public static bool ios { get { return setting.m_ios; } }
    public static Music _music;
    public static MpGame _MpGame;
    public static LoadingScreen m_LoadingScreen;
    public static LoadingScreen _LoadingScreen { get { return m_LoadingScreen == null ? (m_LoadingScreen = (LoadingScreen)FindObjectOfType(typeof(LoadingScreen))) : m_LoadingScreen; } }
    private static CustomWindow m_CustomWindow;
    public static CustomWindow win { get { return m_CustomWindow == null ? (m_CustomWindow = (CustomWindow)FindObjectOfType(typeof(CustomWindow))) : m_CustomWindow; } }
    public static bool android;//{ get { return setting.m_android; } }
    public static bool androidPlatform { get { return Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.WP8Player; } }
    private static Integration m_Integration;
    public static Integration _Integration { get { return m_Integration == null ? (m_Integration = (Integration)FindObjectOfType(typeof(Integration))) : m_Integration; } }
    //public static Loader m_Loader;
    public static Loader _Loader;// { get { return m_Loader == null ? (m_Loader = (Loader)FindObjectOfType(typeof(Loader))) : m_Loader; } }
    public static GameGui m_GameGui;
    public static GameGui _GameGui { get { return m_GameGui == null ? (m_GameGui = (GameGui)FindObjectOfType(typeof(GameGui))) : m_GameGui; } }
    public static CarSelectMenu m_CarSelectMenu;
    public static CarSelectMenu _CarSelectMenu { get { return m_CarSelectMenu == null ? (m_CarSelectMenu = (CarSelectMenu)FindObjectOfType(typeof(CarSelectMenu))) : m_CarSelectMenu; } }
    //private static ResLoader m_setting;
    public static ResLoader setting;
    //{
    //    get
    //    {
    //        if (!m_setting)
    //            m_setting = _Loader == null ? _LoadingScreen.preRes : _Loader.m_setting;
    //        return m_setting;
    //    }
    //}
    [Conditional("UNITY_EDITOR")]
    public static void Log(object s)
    {
        Log(s, false);
    }
    

    public static void Log(object s, bool important)
    {
        if (!setting.enableLog && !important) return;
        if (Time.deltaTime == Time.fixedDeltaTime)
            sbuilder2.Append(s + "\r\n");
        else
            sbuilder.Append(s + "\r\n");
    }
    
    [Conditional("UNITY_EDITOR")]
    public static void LogRight(object s)
    {
        //if (!setting.enableLog) return;
        sbuilderRight.Append(s + "\r\n");
        
    }
    public virtual void Awake()
    {

    }
    protected static StringBuilder sbuilder = new StringBuilder();
    protected static StringBuilder sbuilder2 = new StringBuilder();
    protected static StringBuilder sbuilderRight = new StringBuilder();
    //public float ClampAngle(float angle, float min, float max)
    //{
    //    if (angle < 90 || angle > 270)
    //    {
    //        if (angle > 180) angle -= 360; if (max > 180) max -= 360;
    //        if (min > 180) min -= 360;
    //    }
    //    angle = Mathf.Clamp(angle, min, max);
    //    if (angle < 0) angle += 360; return angle;
    //}
    public Vector3 pos { get { return transform.position; } set { transform.position = value; } }
    public virtual Quaternion rot { get { return transform.rotation; } set { transform.rotation = value; } }
    public static Vector3 ZeroZ(Vector3 v)
    {
        v.z = 0;
        return v;
    }
    public static Vector3 ZeroY(Vector3 v, float a = 0)
    {
        v.y *= a;
        return v;
    }
    public static WWW Download2(string s, Action<string> a, bool post, params object[] prms)
    {
        return Download(s, delegate(string txt, bool b) { a(b ? txt : ""); }, post, prms);
    }


    //public static bool debugThis;
    public static WWW DownloadAcc(string s, Action<string, bool> a, bool post, params object[] prms)
    {
        if (guest) return null;
        var objects = new object[] { "name", _Loader.playerName, "password", _Loader.password, "vkPassword", _Loader.vkPassword, "msgid", _Loader.msgId++ }.Concat(prms.ToArray()).ToArray();
        StringBuilder sb = new StringBuilder("dsajhdjehfjhghvcxu");
        for (int i = 0; i < objects.Length; i += 2)
            if (objects[i + 1] as byte[] == null)
            {
                sb.Append(objects[i]);
                sb.Append(objects[i + 1]);
            }
        objects = objects.Concat(new object[] { "md5", GetMD5Hash(sb.ToString()) }).ToArray();
        return Download(mainSite + "scripts/" + s + ".php", a, post, objects);
    }
    [Conditional("UNITY_EDITOR")]
    public void DebugPrint(object s)
    {
        print(s);
    }
    public static IEnumerable<Transform> GetTransforms(Transform ts)
    {
        yield return ts;
        foreach (Transform t in ts)
        {
            foreach (var t2 in GetTransforms(t))
                yield return t2;
        }
    }
    public static IEnumerable<T> GetComponents<T>(Transform ts) where T : Component
    {
        return GetTransforms(ts).Select(a => a.GetComponent<T>()).Where(a => a != null);
    }
    public static WWW Download(string s, Action<string, bool> a, bool post, params object[] prms)
    {
        if (setting.disPlayerPrefs2)
        {
            a("", false);
            return null;
        }
        s = Uri.EscapeUriString(s);
        
        WWW w;
        StringBuilder query = new StringBuilder();
        if (prms.Length > 0)
        {
            WWWForm form = new WWWForm();
            for (int i = 0; i < prms.Length; i += 2)
            {
                if (post)
                {
                    if (prms[i + 1] is byte[])
                    {
                        form.AddBinaryData(prms[i].ToString(), (byte[])prms[i + 1]);
                    }
                    else
                        form.AddField(prms[i].ToString(), prms[i + 1].ToString());
                }
                query.Append(i != 0 ? "&" : "?");
                query.Append(prms[i] + "=" + WWW.EscapeURL(prms[i + 1].ToString()));
            }
            w = post ? new WWW(s, form) : new WWW(s + query);            
        }
        else
            w = new WWW(s);
        var url = post ? w.url + query : w.url;
        print(url);
        if (_Loader != null)
            _Loader.StartCoroutine(DownloadCor(a, w,url));
        else
            _LoadingScreen.StartCoroutine(DownloadCor(a, w,url));
        return w;
    }
    //private static  string url;
    private static IEnumerator DownloadCor(Action<string, bool> a, WWW w,string url)
    {
        //if (debugThis)
        //print("URL "+url);
        //if (PlayerPrefs.HasKey(url) && isDebug)
        //{
        //    if(debugThis)
        //        print("Read Cache " + url + "\n" + PlayerPrefs.GetString(url));
        //    if (a != null)
        //        a(PlayerPrefs.GetString(url), true);
        //    yield break;
        //}

        bool hasCache = UnityEngine.PlayerPrefs.HasKey(url) && setting.wwwCache;
        //print(url+ " hasCache:" + hasCache);
        if (!hasCache)
            yield return w;
        if (setting.delayLoading) yield return new WaitForSeconds(1);
        string trim;

        //if (debugThis)
            //print("Preparing Cache "+url);
        if (!hasCache && String.IsNullOrEmpty(w.error) && !((trim = w.text.Trim()).StartsWith("<") && trim.EndsWith(">")) && !setting.offline )
        {
            if (a != null)
            {
                //print("Write Cache " + url + " " + w.text);
                if (!Application.isWebPlayer)
                    PlayerPrefs.SetString2(url, w.text);
                a(w.text, true);
            }
    
        }
        else
        {
            if (a != null)
            {
                print("Read Cache " + url + "\n" + UnityEngine.PlayerPrefs.GetString(url));
                if (UnityEngine.PlayerPrefs.HasKey(url) && (android || setting.wwwCache))
                    a(UnityEngine.PlayerPrefs.GetString(url), true);
                else
                    a(w.error == null ? "Failed to Parse"+w.text : w.error, false);
            }
            if (!hasCache)
                Debug.LogWarning(w.error + w.url);
        }
    }

    internal static string m_mainSite = "://tmrace.net/tm/";
    internal static string http
    {
        get { return (flash || Application.platform == RuntimePlatform.NaCl || Application.absoluteURL == null || !Application.absoluteURL.ToLower().StartsWith("https") ? "http" : "https"); }
    }
    internal static string _MainSite { get { return http + m_mainSite; } }
    public bool easy { get { return _Loader.difficulty == Difficulty.Easy; } }
    public bool normal { get { return _Loader.difficulty == Difficulty.Normal; } }
    public bool hard { get { return _Loader.difficulty == Difficulty.Hard; } }
    internal static string mainSiteHttps { get { return "https://" + m_mainSite; } }
    internal static string mainSite
    {
        get { return setting.localhost ? "http://localhost/" : _MainSite; }
    }
    public static string TimeToStr(float s, bool draw = false, bool skip = false, bool miliseconds = true)
    {
        if (skip) return ((int)s).ToString();
        var f = Mathf.Abs(s);
        var s1 = new StringBuilder().Append(s < 0 ? "-" : draw ? "+" : "").Append((int)f / 60).Append(":").Append(((int)(f % 60)).ToString().PadLeft(2, '0'));
        if (miliseconds)
            s1.Append("." + ((int)((f % 1) * 100)).ToString().PadLeft(2, '0'));
        return s1.ToString();
    }
    public static bool guest { get { return _Loader.m_guest; } set { _Loader.m_guest = value; } }
    public static bool serverOffline;
    public bool TimeElapsed(float seconds, float offset = 0)
    {
        var deltaTime = Time.deltaTime + offset;

        if (deltaTime > seconds || seconds == 0) return true;
        if (Time.time % seconds < (Time.time - deltaTime) % seconds)
            return true;
        return false;
    }
    public bool FramesElapsed(int tm,int random=0)
    {
        return (Time.frameCount + random) % tm == 0 || Time.timeScale == 0;
    }
    public bool FramesElapsedA(int tm, int random = 0)
    {
        return (Time.frameCount + random) % tm == 0 || !android && !isDebug;
    }
    public GUIContent GUIContent(Texture texture, string p2 = null)
    {
        return GUIContent(null, texture, p2);
    }
    public GUIContent GUIContent(string p1, Texture texture = null, string p2 = null)
    {
        if (p1 != null && texture != null)
            return new GUIContent(p1, texture);
        if (texture != null && p2 != null)
            return new GUIContent(texture, p2);
        if (texture != null)
            return new GUIContent(texture);
        if (p1 == null)
            return new GUIContent("");
        if (p2 != null)
            return new GUIContent(p1, p2);
        return new GUIContent(p1);
    }

    private static HashSet<string> eventsCommited = new HashSet<string>();
    //public static void LogEvent(EventGroup eg, string group, string name)
    //{
    //    GA.API.Design.NewEvent(String.Format("{0}:{1}:{2}", eg, @group.Replace(':', ' '), name.Replace(':', ' ')));
    //}
    private static void LogEvent(string group, string name)
    {

        if (!eventsCommited.Contains(name) && !isDebug)
        {
            Download(mainSite + "scripts/count.php", null, false, "submit", platformPrefix + setting.version + "/" + group + "/" + name);
            eventsCommited.Add(name);
        }

        GA.API.Design.NewEvent(String.Format("{0}:{1}", @group, name.Replace(':', ' ')));

    }
    
    public static void LogEvent(string name)
    {
        LogEvent(EventGroup.Other.ToString(), name);
    }
    public static void LogEvent(EventGroup eg, string name)
    {
        LogEvent(eg.ToString(), name);
    }
    public void LoadLevel(string s, string s2 = null)
    {
        Debug.LogWarning("Load level " + s);
        _Loader.oldLevel = _Loader.mapName;
        _Loader.mapName = s2 ?? s.ToLower();
        Loader.loadingLevelQuit = true;
        _Loader.StartCoroutine(AddMethod(.1f, delegate { Loader.loadingLevelQuit = false; }));
        //Debug.LogError("LoadLevel old:" + _Loader.oldLevel + " new:" + _Loader.mapName);
        Application.LoadLevel(s.ToLower());
    }
    public static bool CanStreamedLevelBeLoaded(string fileName)
    {
        return Application.CanStreamedLevelBeLoaded(fileName.ToLower());
    }
    public void LoadLevelAdditive(string fileName)
    {
        Application.LoadLevelAdditive(fileName.ToLower());
    }
    public static bool flash
    {
        get { return Application.platform == RuntimePlatform.FlashPlayer; }
    }

    public bool vsFriendsOrPlayers { get { return _Loader.sGameType == SGameType.VsFriends || _Loader.sGameType == SGameType.VsPlayers || _Loader.sGameType == SGameType.Clan; } }
    public bool vsFriendsOrClan { get { return _Loader.sGameType == SGameType.VsFriends || _Loader.sGameType == SGameType.Clan; } }
    public static bool splitScreen
    {
        get { return _Loader.sGameType == SGameType.SplitScreen; }
    }
    public static bool vsPlayersOrSplitscreen
    {
        get { return splitScreen || _Loader.sGameType == SGameType.VsPlayers; }
    }


    public static bool standAlone
    {
        get
        {
            return Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.LinuxPlayer
#if UNITY_EDITOR
 || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows
#endif
;
        }
    }


    public static string m_platformPrefix;
    
    
    public static string platformPrefix2
    {
        get { return platformPrefix + (platformPrefix == "android" ? setting.androidMapVersion : setting.MapVersion); }
    }

    public static string platformPrefix
    {
        get
        {

#if UNITY_EDITOR
            if (Application.isEditor)
            {
                if (isDebug && Application.isPlaying) return "web";
                var bt = EditorUserBuildSettings.activeBuildTarget;
                return (bt == BuildTarget.WP8Player ? "wp8" : bt == BuildTarget.iPhone ? "ios" : bt == BuildTarget.Android ? "android" : bt == BuildTarget.FlashPlayer ? "flash" : "web");
            }
#endif
            return m_platformPrefix != null ? m_platformPrefix : (m_platformPrefix = Application.platform == RuntimePlatform.WP8Player ? "wp8" : Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.IPhonePlayer ? "ios" : Application.platform == RuntimePlatform.Android ? "android" : Application.platform == RuntimePlatform.FlashPlayer ? "flash" : "web");
        }
    }

    
    public static ResEditor resEditor
    {
        get { return (ResEditor)Resources.LoadAssetAtPath("Assets/!Prefabs/resEditor.prefab", typeof(ResEditor)); }
    }
    public static string GetFileNameWithoutExtension(string path)
    {
        var a = path.LastIndexOf('/') + 1;
        var b = path.LastIndexOf('.');
        if (b == -1)
            b = path.Length;
        return path.Substring(a, b - a);
    }
    public static string[] SplitString(string text)
    {
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return lines;
    }
    public void ApplicationQuit()
    {
        SaveStrings();
        win.ShowWindow(_Loader.CloseWindow,win.act);
        
        //GUI.enabled = false;
        //Invoke("ApplicationQuit2", .1f);
    }
    //void ApplicationQuit2()
    //{
    //    print("Application QUIT!2");
    //    Application.Quit();
    //}
    public static bool lowOrAndorid
    {
        get { return android || _Loader.quality <= Quality2.Low; }
    }

    public static bool lowestOrAndorid
    {
        get { return android || _Loader.quality == Quality2.Lowest; }
    }

    public static bool medium
    {
        get { return _Loader.quality >= Quality2.Medium; }
    }

    public static bool UltraQuality
    {
        get { return _Loader.quality >= Quality2.Ultra; }
    }
    public static bool highQuality
    {
        get { return _Loader.quality >= Quality2.High; }
    }
    public Quality2 quality { get { return _Loader._quality; } set { _Loader._quality = value; } }

    public static bool highOrNotAndroid
    {
        get
        {
            return _Loader.quality >= Quality2.High || !android;
        }
    }
    public static bool mediumAndroidHigh
    {
        get
        {
            return android ? _Loader.quality >= Quality2.High : _Loader.quality >= Quality2.Medium;
        }
    }
    public static bool lowestOrAndroidLow
    {
        get { return android ? _Loader.quality <= Quality2.Low : _Loader.quality <= Quality2.Lowest; }
    }
    public static bool lowestQuality
    {
        get { return _Loader.quality <= Quality2.Lowest; }
    }

    public static bool lowQualityAndAndroid
    {
        get { return _Loader.quality <= Quality2.Low && android || _Loader.quality == Quality2.Lowest; }
    }

    public static bool lowQuality
    {
        get { return _Loader.quality <= Quality2.Low; }
    }

    public static bool Android2
    {
        get { return Application.platform == RuntimePlatform.Android; }
    }

    public static Vector2 touchDelta;
    public Vector2 getMouseDelta(bool returnMouse = true)
    {
        if (Input.touchCount > 0)
            return touchDelta / 10;
        return returnMouse ? new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) : Vector2.zero;

        //return android && Input.touchCount > 0 ? Input.touches[0].deltaPosition / 5f : returnMouse ? new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) : Vector2.zero;
    }
#if UNITY_EDITOR
    public void WriteAllLines(string getAssetPath, string[] dest)
    {
        using (var f = File.OpenWrite(getAssetPath))
            foreach (var a in dest)
            {
                var b = Encoding.UTF8.GetBytes(a + "\r\n");
                f.Write(b, 0, b.Length);
            }
    }
#endif
    public void SetCustomProperty(string Key, object Value)
    {
        if (PhotonNetwork.room != null)
        {
            PhotonNetwork.room.customProperties[Key] = Value;
            PhotonNetwork.room.SetCustomProperties(PhotonNetwork.room.customProperties);
        }
    }
    public static string GetFileName(string s)
    {
        return s.Substring(s.LastIndexOf('/') + 1);
    }
    public static string unescape(string d)
    {
        return d.Replace("\\r", "\r").Replace("\\n", "\n");
    }

    public static string GetMD5Hash(string input)
    {
#if !UNITY_WP8
        MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
        byte[] bs = Encoding.UTF8.GetBytes(input);
        bs = x.ComputeHash(bs);
        StringBuilder s = new StringBuilder();
        foreach (byte b in bs)
        {
            s.Append(b.ToString("x2").ToLower());
        }
        return s.ToString();
#else
        return "";
#endif
    }
    public static List<KeyCode> recordKeys = new List<KeyCode> { KeyCode.W, KeyCode.A, KeyCode.D, KeyCode.S, KeyCode.Space, KeyCode.LeftAlt, KeyCode.LeftShift, KeyCode.Tab, KeyCode.F, KeyCode.X };
    //public static bool Hotkey()
    //{
    //    return Input.GetKeyDown(KeyCode.G) && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift);
    //}
    public bool KeyDebug(KeyCode key, string desc = null)
    {
        if (setting.debug || Application.isEditor)
            return Input.GetKeyDown(key) && Input.GetKey(KeyCode.LeftShift);
        return false;
    }
    public static float Mod(float a, float n)
    {
        return ((a % n) + n) % n;
    }
    public static int Mod(int a, int n)
    {
        return ((a % n) + n) % n;
    }
    
    public static bool Nancl
    {
        get { return Application.platform == RuntimePlatform.NaCl; }
    }
    public static bool webPlayer
    {
        get { return Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer||Application.isEditor; }
    }
    public static List<AssetBundle> assetBundle = new List<AssetBundle>();
    
    public static Object LoadRes(string name)
    {
        foreach(var a in assetBundle)
        {
            var load = a.Load("@" + name);
            if(load!=null)
                return load;
        }
        return Resources.Load(name);

        //if (assetBundle == null || assetBundle.Load("@" + name) == null)
        //    return Resources.Load(name);
        //else
        //    return assetBundle.Load("@" + name);
    }

    public static void SaveStrings()
    {
        if (setting.disablePlayerPrefs)
        {
            SetStrings.Clear();
            return;
        }
        StringBuilder lg = new StringBuilder();
        lg.AppendLine("saved keys" + playerPrefKeys.Count);
        foreach (var a in SetStrings)
        {
            PlayerPrefs.SetString(a.Key.ToLower(), a.Value);
            lg.AppendLine(a.Key + "\t\t\t" + a.Value);
        }
        print("Save strings " + SetStrings.Count + "\n" + (Debug.isDebugBuild ? lg.ToString() : lg.Length.ToString()));
        SetStrings.Clear();

        StringBuilder sb = new StringBuilder();
        foreach (var a in playerPrefKeys)
            sb.Append(a).Append(",");

        var s = Convert.ToBase64String(GZipStream.CompressString(sb.ToString()));
        print(sb.Length + " vs " + s.Length);
        PlayerPrefs.SetString2("keysnew3", s);
        

        PlayerPrefs.Save();
    }

    public static int totalSeconds
    {
        get { return (int)(DateTime.Now.Ticks / TimeSpan.TicksPerSecond) - Int32.MaxValue; }
    }

    public static string Ordinal(int number)
    {
        
        string suffix = String.Empty;

        int ones = number % 10;
        int tens = (int)Math.Floor(number / 10M) % 10;

        if (tens == 1)
        {
            suffix = "th";
        }
        else
        {
            switch (ones)
            {
                case 1:
                    suffix = "st";
                    break;

                case 2:
                    suffix = "nd";
                    break;

                case 3:
                    suffix = "rd";
                    break;

                default:
                    suffix = "th";
                    break;
            }
        }
        return String.Format("{0}{1}", number, suffix);       
    }

    public void LogVar(string s)
    {
        var type = GetType();
        var propertyInfo = type.GetProperty(s, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (propertyInfo != null)
            print(propertyInfo.GetValue(this, null));
        else
            print(type.GetField(s, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(this));
    }
    public static void ExternalEval(string script)
    {
#if UNITY_WEBPLAYER
        _Loader.evalCors.Enqueue(new EvalCor() { s = script });
        //Application.ExternalEval(script);
#endif
    }
    public static void ExternalCall(string functionName, params object[] args)
    {
#if UNITY_WEBPLAYER
        _Loader.evalCors.Enqueue(new EvalCor() { s = functionName, args = args });
        //Application.ExternalCall(functionName, args);
#endif
    }
    
    public static bool wp8
    {
        get { return Application.platform == RuntimePlatform.WP8Player; }
    }
    public static int rnd = new Random().Next(9999) + 123;
    
    internal int myId
    {
        get { return PhotonNetwork.player.ID; }
    }
    public bool checkVisible(Vector3 point)
    {
        var campos = _Player.camera.transform.position;
        var wp =_Player.camera.WorldToViewportPoint(point);

        var vis = new Rect(0, 0, 1, 1).Contains(wp) && wp.z > 0 && !Physics.Linecast(campos, point, Layer.levelMask) || Vector3.Distance(campos, point) < 50;
        if(vis)
            Debug.DrawLine(campos, point);
        return vis;
    }
    
    public static void PlayOneShotGui(AudioClip sound,float volume=1)
    {
        _Loader.audio.PlayOneShot(sound, _Loader.soundVolume*volume);
    }

    public static int SetFlag(int levelFlags, int flag, bool value)
    {
        if (value)
            return levelFlags | flag;
        else
            return levelFlags & ~flag;
    }
    public static bool GetFlag(int levelFlags, int flag)
    {
        return (levelFlags & flag) != 0;
    }
    [Conditional("UNITY_EDITOR")]
    public void printLog(object s)
    {
        print(LogLevel.Log, s);
    }
    [Conditional("UNITY_EDITOR")]
    public void print(LogLevel level, params object[] o)
    {
        if (Debug.isDebugBuild && setting.GetFlag(level))
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(level).Append(":");
            foreach (var a in o)
                sb.Append(a).Append(", ");
            Debug.Log(sb.ToString(), this);
        }
    }

    //internal int rewinds2 { get { return CheckGet(rewinds, "rewinds"); } set { rewinds = CheckSet(value, "rewinds"); } }
    //private int CheckSet(int value, string s)
    //{
    //    mhCheck[s] = value ^ 2424;
    //    return value;
    //}
    //private int CheckGet(int value, string s)
    //{
    //    int o;
    //    if (mhCheck.TryGetValue(s, out o) && (o ^ 2424) != value) _Loader.MhSend("rewinds changed to " + value);
    //    return value;
    //}
    //Dictionary<string, int> mhCheck = new Dictionary<string, int>();
    public Vector3 ClampAngle(Vector3 angle)
    {
        return new Vector3(ClampAngle(angle.x), ClampAngle(angle.y), ClampAngle(angle.z));
    }
    public float ClampAngle(float angle)
    {
        if (angle > 180) angle = angle - 360;
        return angle;
    }
    public const int MinValue=-99999;
    public const int MaxValue = 99999;
}


