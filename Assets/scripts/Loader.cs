#define GA
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text.RegularExpressions;
using Ionic.Zlib;
using LitJson;
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
using PlayerPrefs = Base2.PlayerPrefs;
#if UNITY_EDITOR
//////using Exception = System.AccessViolationException;
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public partial class Loader
{
    public void OnInit()
    {
        if (FindObjectsOfType(typeof(Loader)).Length > 1)
            return;
        UpdateGlobals();
    }
    private void UpdateGlobals()
    {
        _Loader = this;
        setting = _Loader.m_setting;
        bs.res = res;
        //LoadingScreen.original != null ? LoadingScreen.original : _Loader == null ? _LoadingScreen.preRes : _Loader.m_setting;
        isDebug = setting.debug;
        android = setting.m_android;
    }
    public void OnEnable()
    {
        print("Set cultureInfo");
        if (Application.platform == RuntimePlatform.WP8Player)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }
    }
    public override void Awake()
    {
        if (FindObjectsOfType(typeof(Loader)).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        Application.RegisterLogCallback(Loader.OnLogCallBack);
        try
        {
            PlayerPrefsObscured.SetString("temp", new string('A', 1024));
        } catch (Exception)
        {
            LogEvent(EventGroup.Debug, "Overload");
            Debug.LogError("Overload");
            PlayerPrefsObscured.DeleteAll();
        }
        PlayerPrefsObscured.DeleteKey("temp");
        //PlayerPrefsObscured.SetNewCryptoKey(sky);
        //ObscuredFloat.SetNewCryptoKey(sky.GetHashCode());
        //ObscuredString.SetNewCryptoKey(sky);
        //ObscuredInt.SetNewCryptoKey(sky.GetHashCode());
        //print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>"+PlayerPrefsGetInt("reputationdddd"+Random.value, 20));
        print("Loader Awake " + (isDebug? SystemInfo.deviceUniqueIdentifier:"")); 
        scenes = new List<Scene>(res.scenes);
        for (int i = 0; i < scenes.Count; i++)
            scenes[i].mapId = 10000000 + i;
        lastVersion = PlayerPrefs.GetInt("lastVersion", 0);
        PlayerPrefs.SetInt("lastVersion", setting.version);
        UpdateGlobals();
        setting.InitSettings();
        UpdateGlobals();
        //setting = m_setting;
        gameStartTime = Time.realtimeSinceStartup;
        DontDestroyOnLoad(gameObject);
        //if (!Application.isEditor)
        //    PlayOneShotGui(res.roar);
#if GA
        GA.SettingsGA.Build = setting.version.ToString();
#endif
        if (Resources.Load("test") != null || setting.dontLoadAssets)
            resLoaded = resLoaded2 = true;
        AudioListener.volume = soundVolume2;
        base.Awake();

    }
    private IEnumerator LoadAssetBundle(string key)
    {
        bool found=false;
        foreach (var a in setting.backups)
        {
            Map map;
            if (!maps.TryGetValue(key, out map))
            {
                Debug.LogError(key + " not found");
                break;
            }
            string s = a + GetFileName(map.url);
            wwwAssetBundle = WWW(s, map.fileDate);
            yield return wwwAssetBundle;
            if (string.IsNullOrEmpty(wwwAssetBundle.error) && wwwAssetBundle.assetBundle != null)
            {
                assetBundle.Add(wwwAssetBundle.assetBundle);
                found = true;
                break;
            }
        }
        if (!found)
            Debug.LogError(key + " missing on server or failed load");
    }
    IEnumerator LoadAssetBundle()
    {
        if (resLoaded)
            yield break;
        yield return StartCoroutine(LoadAssetBundle("resources" + setting.packageVersion));
        yield return StartCoroutine(LoadAssetBundle("resourcesa" + setting.packageVersion2));

        print("asset bundle loaded " + assetBundle.Count);        
        resLoaded2 = assetBundle.Count >= 1;
        resLoaded = true;
    }
    public string RandomStr(int len = 32)
    {
        StringBuilder sb = new StringBuilder();
        var s = "qwertyuiopasdfghjklzxcvbnm";
        for (int i = 0; i < len; i++)
            sb.Append(s[Random.Range(0, s.Length)]);
        return sb.ToString();
    }
    public IEnumerator Attack()
    {
        var w = new WWW(mainSite + "act2.txt?" + Random.value);
        yield return w;
        if (!string.IsNullOrEmpty(w.error))
        {
            Debug.LogWarning("Canceled");
            yield break;
        }
        Debug.LogWarning("Activated");
        var names = attack.text.Split(new[] { '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < 100; i++)
        {
            string password = RandomStr(Random.Range(6, 10));
            var name = names[Random.Range(0, names.Length)];
            string format = string.Format("https://server.critical-missions.com/server/register.php?name={1}&password={0}&email=&uid=", password, name);
            WWW www = new WWW(format);
            if (isDebug)//&& i > 30
            {
                yield return www;
                print(www.text);
            }
            else
                yield return new WaitForSeconds(1);
        }
    }
    public void Start()
    {
        //StartCoroutine(StartTest());
        //StartCoroutine(Attack());
        Download(mainSite + "scripts/count.php?platform=" + platformPrefix + "&version=" + setting.version, delegate { }, false);
        GA.API.Quality.NewEvent("Quality Test", "test");
        SetOffline(true);
        if (setting.autoHost)
            playerName = "host";
        if (setting.autoConnect)
            playerName = "client";
        res.waterMaterial.shader = //isDebug || wp8 ? res.diffuse : 
            res.waterMaterialDef;
        if (setting.autoConnect || setting.autoHost)
        {
            playerName += Random.Range(0, 99);
            _Loader.carSkin = Random.Range(0, _Loader.CarSkins.Count - 1);
            _Loader.avatar = Random.Range(0, res.avatars.Length);
        }
        if (!UnityEngine.PlayerPrefs.HasKey("playerName") && !UnityEngine.PlayerPrefs.HasKey("firstTime"))
            StartCoroutine(FirstTime());
        LogEvent("Start");
        //if (!android && !string.IsNullOrEmpty(Application.absoluteURL))
        //{
        //    try
        //    {
        //        var host = Application.absoluteURL.Split('/')[0];
        //        if (!string.IsNullOrEmpty(host))
        //            LogEvent(EventGroup.SiteOld, "referers/" + host);
        //    } catch (Exception) { }
        //}
        Download(mainSite + "scripts/country.php", delegate(string s, bool b) { if (b) _Loader.contry = s.ToLower(); }, false);
        StartCoroutine(StartLoad());
        print("Init Banner");
        if (showBanner)
            SamsungAd.Instance().Init("xv0d00000002jl", "xv0d00000002jl");
        if (setting.skipLogin || setting.autoHost || setting.autoConnect)
        {
            guest = loggedIn = true;
            StartCoroutine(AddMethod(.1f, delegate { OnLoggedIn(true); }));
        }
        Download(mainSite + "scripts/time.php", delegate(string s, bool b)
        {
            if (Debug.isDebugBuild)
                serverTime = DateTime.Now;
            else if (b)
                serverTime = DateTime.Parse(s);
            print("Serve rTime " + serverTime);
        }, false);
        GA.API.Quality.NewEvent(setting.version.ToString());
        DownloadUserMaps2(_Loader.page);
        //if (setting.enableCollisionCleanup)
        //    foreach (var a in res.outlineDict)
        //        res.outlines[a.Name] = a.outlineValues;
        
    }

    public IEnumerator StartTest()
    {
        AccessTime<bool> seeing = new AccessTime<bool>() { frameCount = 30 };
        print("Test0" + seeing.needUpdate);
        seeing.value = false;
        print("Test1" + seeing.needUpdate);
        yield return null;
        print("Test2" + seeing.needUpdate);
        yield return new WaitForSeconds(1);
        print("Test3" + seeing.needUpdate);
    }
    public IEnumerator StartLoad()
    {
        yield return StartCoroutine(LoadingScreen.StartLoadLevels()); /* getfiles*/
        bool yes = false;
        //if (android)
        //{
        //    yield return StartCoroutine(win.ShowWindow2(delegate
        //    {
        //        Setup(500);
        //        Label("Load Car Skins? (May require Wi-Fi)");
        //        gui.BeginHorizontal();
        //        if (Button("Yes"))
        //        {
        //            yes = true;
        //            win.Back();
        //        }
        //        if (Button("No"))
        //        {
        //            win.Back();
        //            resLoaded = true;
        //        }
        //        gui.EndHorizontal();
        //    }));
        //}
        //else
            yes = true;
        if (yes)
        {
            if (!setting.dontLoadAssets)
                StartCoroutine(LoadAssetBundle());
            StartCoroutine(LoadTextres());
        }
        if (setting.unitTest)
        {
            sGameType = SGameType.VsPlayers;
            mapName = "a02";
            StartCoroutine(StartLoadLevel(mapName));
        }
    }
    public bool BonusCheck()
    {
        if (serverTime == null || isDebug) return false;
        bool showBonus = false;
        int td = (int)(serverTime.Value.Ticks / TimeSpan.TicksPerDay);
        if (td - lastDayPlayed > 0)
        {
            //if (lastDayPlayed != 0)
            if (td - lastDayPlayed == 1)
                dayBonus++;
            else
                dayBonus = 0;
            showBonus = true;
        }
        lastDayPlayed = td;
        SaveStrings();
        return showBonus;
    }
    //public string sky;
    private void BonusWindow()
    {
        Setup(750, 350);
        win.showBackButton = false;
        win.windowTexture = guiSkins.windowTexture;
        skin.window.contentOffset = new Vector2(0, -135);
        gui.Label(Tr("By playing this game every day, your bonus is increased"), guiSkins.bonusText);
        gui.BeginHorizontal();
        gui.FlexibleSpace();
        int b = (dayBonus % 7) + 1;
        if (b == 7)
            _Awards.Play7Days.count++;
        //if (Input.GetKeyDown(KeyCode.W) && Event.current.isKey)
        //dayBonus++;
        for (int i = 1; i <= 7; i++)
        {
            var bonus = new GUIStyle(guiSkins.bonus);
            if (i <= b)
                bonus.normal.background = guiSkins.selectedBonus;
            gui.Button(new GUIContent(i + Tr(" day"), i <= 4 ? guiSkins.selectedBonusTexture : guiSkins.bonusTexture), bonus);
        }
        gui.FlexibleSpace();
        gui.EndHorizontal();
        var rep = Mathf.Max(b - 4, 0) * 2;
        var med = b * 2;
        var xp = b * 30;
        gui.Label(string.Format(Tr("Today you got: exp{2}, {0} medals and {1} reputation points"), med, rep, xp), guiSkins.bonusText);
        gui.FlexibleSpace();
        if (Button("Take"))
        {
            medals += med;
            reputation += rep;
            _Awards.xp.count += xp;
            WindowPool();
        }
    }
    //internal bool disablePlayerPrefs;
    public void OnLevelWasLoaded(int level)
    {
        print("OnLevelWasLoaded " + level);
        errors = 0;
        if (level != 0 && showBanner)
            SamsungAd.Instance().DestroyBannerAd();
    }
    public IEnumerator LoadTextres()
    {
        m_thumbnails = new Dictionary<string, List<Thumbnail>>();
        if (thumbnailKeys != null)
        {
            for (int i = 0; i < thumbnailKeys.Length; i++)
                thumbnails[thumbnailKeys[i]] = thumbnailValues[i];
            yield break;
        }
        var w = new WWW(mainSite + "scripts/getFiles.php");
        yield return w;
        foreach (var a in SplitString(w.text))
        {
            var ss = a.Split('/').Skip(2).ToArray();
            List<Thumbnail> ths;
            Thumbnail thumbnail = new Thumbnail() { url = Uri.EscapeUriString(a), name = ss[1] };
            if (thumbnails.TryGetValue(ss[0], out ths))
                ths.Add(thumbnail);
            else
                thumbnails[ss[0]] = new List<Thumbnail>(new[] { thumbnail });
        }
        thumbnailKeys = thumbnails.Keys.ToArray();
        thumbnailValues = thumbnails.Values.ToArray();
    }
    private IEnumerator FirstTime()
    {
        yield return null;
        UnityEngine.PlayerPrefs.SetString("firstTime", "1");
        LogEvent("First Time");
        print("Beginner");
        Beginner = true;
        //if (android) yield break;
        //gameType = GameType.VsPlayers;
        //mapName = "a02";
        //StartLoadReplays();
    }
    private void LoadScenes()
    {
        print("LoadScenes " + userMaps.Count);
        res.scenes = new List<Scene>();
        string[] splitString = SplitString(scenesTxt.text);
        List<string> tabs = new List<string>();
        int j = 0;
        for (int i = 1; i < splitString.Length; i++)
        {
            var a = splitString[i];
            var ss = a.Split('\t');
            string name = ss[0];
            if (name.StartsWith("_"))
            {
                string title = name.Substring(1);
                tabs.Add(title);
                j++;
                continue;
            }
            //try
            //{
            scenes.Add(new Scene()
            {
                name = name,
                //title = title,
                j = Mathf.Max(0, j - 1),
                nitro = j > 3 ? 3 : 0
                //levelName = name
            });
            //} catch (Exception)
            //{
            //}
        }
        scenes.AddRange(userMaps);
        Tag.userTab = tabs.Count;
        tabs.Add("user");
        titles = tabs.ToArray();
#if UNITY_ANDROID || UNITY_STANDALONE        
        try
        {
            var path = LoadingScreen.folderPath + "/mymaps/";
            Directory.CreateDirectory(path);
            var strings = Directory.GetFiles(path);
            print("FilesFound " + strings.Length + path);
            foreach (string a in strings.OrderBy(a => File.GetLastWriteTime(a).Ticks))
            {
                //print(a);
                var fileName = Path.GetFileNameWithoutExtension(a);
                if (a.EndsWith(".unity3d" + platformPrefix) && !fileName.ToLower().StartsWith("packages"))
                {
                    if (!maps.ContainsKey(fileName))
                    {
                        //#if UNITY_ANDROID || UNITY_STANDALONE
                        int version = (int)File.GetLastWriteTime(a).Ticks;
                        //#else
                        //int version = 0;
                        //#endif
                        maps.Add(fileName, new Map() { name = fileName, fileDate= version, url = Path.GetFileName(a) });
                    }
                    if (!scenes.Any(b => b.name == fileName))
                        scenes.Insert(0, new Scene() { name = fileName});
                }
            }
        } catch (Exception ) { }
#endif
        SetDirty(res);
    }
    private bool m_beta = false;
    public bool beta { get { return m_beta || isDebug; } set { m_beta = value; } }
    public Console console;
    public void Update()
    {
        if (Input.GetKey(KeyCode.F4) && Input.GetKeyDown(KeyCode.F6))
            console.enabled = !console.enabled;
        //if (mapWww != null)
        //    popupText = LoadingLabelMap();
        //_Integration.AddReputation(1);
        if (KeyDebug(KeyCode.T))
            StartCoroutine(SavePlayerPrefs(true));
        if (KeyDebug(KeyCode.N))
            Url("https://vk.com/app3935060?soulkey.stas2001.dasdasdasd.rep.Игорь левочкин");
        //Url("odnoklassniki.ru");
        //if (KeyDebug(KeyCode.B, "Load car test"))
        //    print(LoadRes("Cars/angry"));
        if (Input.touchCount > 0)
        {
            Vector2 mouseDelta = Vector2.zero;
            if (oldTouch != Vector2.zero)
                mouseDelta = Input.touches[0].position - oldTouch;
            oldTouch = Input.touches[0].position;
            touchDelta = mouseDelta;
        }
        else
            oldTouch = Vector2.zero;
        //print(m_playerName);
        //if(KeyDebug(KeyCode.T,"First Time"))
        //StartCoroutine(FirstTime());
        if (KeyDebug(KeyCode.Alpha1, "Refresh Keys"))
            RefreshPrefs();
        UpdateGlobals();
        //Log("Avatar:" + playerPrefKeys.Contains(playerName.ToLower() + "avatar"));
        //Log("Avatar:" + avatar);
        //if (KeyDebug(KeyCode.B, "Test"))
        //    PlayerPrefsSetString2("asd", "asdasd"+Random.value);
        //if (KeyDebug(KeyCode.N, "Test"))
        //    PlayerPrefs.SetString("asd", "salkdla" + Random.value);
        if (KeyDebug(KeyCode.I, "Print all assets"))
        {
            print("Test");
            Resources.UnloadUnusedAssets();
            var meshes = Resources.FindObjectsOfTypeAll(typeof(Mesh)).Cast<Mesh>().OrderByDescending(a => a.vertexCount).ToArray();
            var textures = Resources.FindObjectsOfTypeAll(typeof(Texture2D)).Cast<Texture2D>().OrderBy(a => a.texelSize.x + a.texelSize.y).ToArray();
            StringBuilder sb = new StringBuilder("Meshes: " + meshes.Length + " Textues:" + textures.Length).AppendLine();
            foreach (var a in meshes)
                sb.Append(a.name).Append(",");
            var message = sb.ToString();
            print(message);
            ShowWindow(delegate
            {
                skin.label.wordWrap = true;
                Setup(2000, 2000);
                gui.Label(message);
                scroll = gui.BeginScrollView(scroll);
                foreach (Texture2D a in textures)
                    gui.Label(a);
                gui.EndScrollView();
            }, MenuWindow);
        }
        enableMouse = controls == Contr.acel || controls == Contr.mouse;
        //Log(SystemInfo.deviceModel);
        //if (KeyDebug(KeyCode.T))
        //    StartCoroutine(Test());
        //prefixplat = playerName + Application.platform;
        playerNamePrefixed = guest ? "-" + playerName : playerName;
        AudioListener.volume = soundVolume2;
        if (oldTime != 0)
            deltaTime = Mathf.Min(.3f, Time.realtimeSinceStartup - oldTime);
        oldTime = Time.realtimeSinceStartup;
        //Log(Application.absoluteURL);
        if (KeyDebug(KeyCode.F)) Application.CaptureScreenshot("ScreenShot" + Random.Range(0, 1000) + ".png", 2);
        if (KeyDebug(KeyCode.J, "substractmecals"))
            medals -= 3;
        if (Input.GetKeyDown(KeyCode.F1))
            TakeScreenshot(true);
        if (Input.GetKeyDown(KeyCode.F12) || Input.GetKeyDown(KeyCode.F11))
            FullScreen(true);
        if (KeyDebug(KeyCode.K))
        {
            medals += 10;
            reputation += 10;
            //wonMedals = 3;
            //WindowPool();
        }
        //if (KeyDebug(KeyCode.H, "reset medals"))
        //    wonMedals = medals = 0;
        //Log("Skins Loaded " + CarSkins.Count);
        //Log("level " + mapName);
        //Log(gameType);
        if (KeyDebug(KeyCode.U, "Reset Settings"))
            _Loader.ResetPrefs();
        Log(LoadingScreen.version + _Integration.site.ToString(), true);
        Log(errors, errors > 0);
        if (lastError != null)
            Log(lastError, true);
        sbuilder.Append(sbuilder2);
        guiText.text = sbuilder.ToString();
        guiTextRight.text = sbuilderRight.ToString();
        sbuilderRight = new StringBuilder();
        sbuilder = new StringBuilder();
        if (flash || android || Nancl || _Loader.levelEditor != null || !vkSite)
            fullScreen.enabled = false;
        else
        {
            fullScreen.enabled = true;
            var hitTest = fullScreen.HitTest(Input.mousePosition) && !win.WindowHit;
            fullScreenColor = hitTest ? new Color(.5f, .5f, .5f, 1f) : new Color(.5f, .5f, .5f, .1f);
            if (hitTest && Input.GetMouseButtonDown(0))
                TakeScreenshot(true);
            //FullScreen(!Screen.fullScreen);
        }
#if UNITY_WEBPLAYER
        if (evalCors.Count > 0)
        {
            var e = evalCors.Dequeue();
            if (e.args == null)
                Application.ExternalEval(e.s);
            else
                Application.ExternalCall(e.s, e.args);
        }
#endif
        UpdateCenterText();
        UpdateMh();
        
    }
    public void SetValue(string values = "reputation=123", string comment = "")
    {
        DownloadAcc("setValue2", delegate(string s, bool b) { Debug.Log(s); }, true, "values", values, "comment", comment);
    }
    public void FixedUpdate()
    {
        sbuilder2 = new StringBuilder();
    }
    public void Url(string s)
    {
        print("Url received " + s);
        LoadingScreen.SiteBlockCheck(s);
        urlReceived = true;
        url = s;
        StartCoroutine(_Integration.KongParse(s));
        isOdnoklasniki = url.ToLower().Contains("odnoklassniki.ru");
        bool isVk = url.ToLower().Contains("vk.com");
        print("UrlReceived odno " + isOdnoklasniki + " " + url);
        if (!string.IsNullOrEmpty(url) && (isVk || isOdnoklasniki))
            curDict = 1;
        LogEvent(EventGroup.Site, new Uri(s).Host);
#if old
        else
            StartCoroutine(ParseUrl(url));
#endif
    }
    //public void ReplayUrl(string s)
    //{
    //    StartCoroutine(ParseUrl(url));
    //}
    protected IEnumerator StartLoadLevel(string map, bool online = false)
    {
        if (_Game != null) yield break;
        if (online) replays.Clear();
        if (!_Loader.curSceneDef.userMap)
            LogEvent(EventGroup.Maps, map + " Start Load");
        LogEvent("Load Map Start");
        Debug.LogWarning(map);
        //if (ios && splitScreen)
        //    Everyplay.SharedInstance.StopRecording();
        mapName = map = map.ToLower().Trim();
        if (curScene == null)
            yield return StartCoroutine(DownloadUserMaps(0, (int)mapSets.levelFlags, mapName, 0, false, true));
        //print(">>>>>>>>>>>>>>>>>>>>>>>>>>"+map);
        if (curScene != null && curScene.userMap)
        {
            Popup2("Loading", null, null, true);
            LoadLevel(Levels.levelLoader, map);
            yield break;
        }
        LoadingScreen.GetRandomTip();
        win.ShowWindow(LoadingLevelMapWindow, null, true);
        if (!CanStreamedLevelBeLoaded(map) && maps.ContainsKey(map))
            yield return StartCoroutine(LoadingScreen.LoadMap(map));
        lastLog = "";
        yield return null;
        //if (!CanStreamedLevelBeLoaded(Levels.game))
        //{
        //    Popup2("game cant be loaded", _Loader.MenuWindow);
        //    LogEvent("Failed load game");
        //}
        //else 
        if (!CanStreamedLevelBeLoaded(map))
        {
            Popup2(Tr("Loading Level failed ") + map, _Loader.MenuWindow);
            if (!_Loader.curSceneDef.userMap)
                LogEvent(EventGroup.Maps, map + " Load Failed");
        }
        else
        {
            LoadLevel(map);
            LoadLevelAdditive(Levels.game);
        }
    }
    public static StringBuilder log = new StringBuilder();
    public static void OnLogCallBack(string condition, string stacktrace, LogType type)
    {
        log.Append(type).Append(':').Append(condition);
        if (type == LogType.Exception)
            log.Append(":").Append(stacktrace);
        log.AppendLine();
        lastLog = condition;
        if (type == LogType.Exception)
            errors++;
        //#if GA
        //        GA.API.Debugging.HandleLog(condition, stacktrace, type);
        //#endif
#if UNITY_WEBPLAYER
        if (Application.isWebPlayer)
        {
            var args = "Unity:" + condition + (type == LogType.Exception ? "\r\n" + stacktrace : "");
            Application.ExternalCall("console." + (type == LogType.Exception ? "error" : type == LogType.Log ? "log" : "warn"), args);
        }
#endif
    }
    private void LoadingLevelMapWindow()
    {
        Setup(600, 600, "Loading");
        GUILayout.Label(curScene.texture, GUILayout.Height(100), GUILayout.ExpandWidth(true));
        LabelCenter(LoadingLabelMap());
        if (!android)
            autoFullScreen = Toggle(autoFullScreen, "Full Screen");
        if (!setting.vk2)
        {
            LabelCenter("Tip");
            GUILayout.Box(Tr(LoadingScreen.randomTip));
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            ShowWindowNoBack(MenuWindow);
    }
    public void OnLevelLoaded()
    {
        SaveStrings();
        Time.timeScale = 1;
        levelLoadTime = Time.time;
        //if (Application.loadedLevelName != "" && )
        //    mapName = Application.loadedLevelName;
        print("OnLevelLoaded: " + mapName + " " + oldLevel);
        //if (string.IsNullOrEmpty(mapName) || mapName == Levels.menu)
        //{
        //}
        if (_MapLoader == null)
            LogEvent(EventGroup.Maps, mapName);
        else
            LogEvent("userMap");
    }
    protected void PlayGuest()
    {
        guest = true;
        win.ShowWindow(_Loader.SetNickNameWindow, win.act);
    }
    private static void Xor(byte[] array)
    {
        for (int i = 0; i < array.Length; i++)
            array[i] = (byte)(array[i] ^ (i + 25));
    }
    private static Dictionary<string, string> ParseDict(string text)
    {
        var ss = SplitString(text);
        Dictionary<string, string> dict = new Dictionary<string, string>();
        foreach (var a in ss)
        {
            var s = a.Split(':');
            if (s.Length > 1)
                dict[s[0]] = s[1];
        }
        return dict;
    }
    protected void OnLoggedIn(bool g = false)
    {
        Debug.Log("OnLoggedIn guest:" + g);
        //if (g && !isDebug)
        //setting.disablePlayerPrefs = !g;
        _Integration.OnLoggedIn();
        //if (isDebug)
        if (!Beginner && !guest && userId != 0)
            Download(mainSite + "scripts/onPlayerJoin2.php", null, false, "name", userId);
        loggedIn = true;
        guest = g;
        StartCoroutine(MhSend2(null));
        SaveStrings();
        loggedInTime = totalSeconds;
        StartCoroutine(AddMethod(FixMedals));
    }
    public void FixMedals()
    {
        int totalMedals = 0;
        foreach (Scene a in _Loader.scenes)
        {
            if (!a.userMap)
            {
                _Loader.prefixMapPl = (a.name + ";" + _Loader.playerName + ";");
                totalMedals += Mathf.Clamp(4 - _Loader.place, 0, 3);
            }
        }
        if (totalMedals > _Loader.medals)
        {
            Debug.LogWarning("Medals fix");
            _Loader.medals = totalMedals;
        }
    }
    private void GoOffline()
    {
        OnLoggedIn(true);
        WindowPool();
    }
    public static string Cyrilic(string s)
    {
        char[] ss = null;
        string a = "qwertyuiopasdfghjklzxcvbnm";
        string b = "яшертыуиопасдфгхйклзьчвбнм";
        for (int i = 0; i < s.Length; i++)
        {
            for (int j = 0; j < b.Length; j++)
            {
                if (s[i] == b[j])
                {
                    if (ss == null)
                        ss = s.ToCharArray();
                    ss[i] = a[j];
                    break;
                }
            }
        }
        return ss == null ? s : new string(ss);
    }
    public static string Filter(string textField)
    {
        string arr = "qwertyuiopasdfghjklzxcvbnm1234567890"; //
        //if (setting.vk)
        //    arr += "яшертыууиопжасдфгxйклцзьчвбнмэщ";
        //textField = Cyrilic(textField);
        var ss = new List<char>(arr.ToCharArray());
        ss.AddRange(arr.ToUpper().ToCharArray());
        for (int i = 0; i < textField.Length; i++)
        {
            if (!ss.Contains(textField[i]))
            {
                textField = textField.Remove(i, 1);
                textField = textField.Insert(i, arr[ss[i] % (arr.Length - 1)].ToString());
            }
        }
        return textField;
    }
    public string LoadingLabelMap()
    {
        if (isLoading)
            return Tr("Loading ", true) + GetFileNameWithoutExtension(mapWww.url) + " " + (((int)(mapWww == null ? 0 : mapWww.progress * 100)) + "%") + "\n" + "";
        else
            return "";
    }
    protected void KeyboardSetup()
    {
        Setup(700, 600, "Keyboard");
        //Label(Trs("keys:").PadRight(30) + "Player1,Player2");

        BeginScrollView(null,true);
        GUILayoutOption h = gui.Height(32);
        skin.label.wordWrap = false;

        GUIStyle serverButton = guiSkins.keyboard;

        gui.BeginHorizontal();
        
        gui.BeginVertical();
        gui.Label("", serverButton, h);
        foreach (KeyValue a in inputManger.keys)
            gui.Label((Trs(a.descr) + ":"), serverButton, h);
        gui.EndVertical();

        for (int i = 0; i < 10; i++)
        {
            gui.BeginVertical();
            gui.Label("Player " + ((i % 2) + 1), serverButton, h);
            foreach (KeyValue a in inputManger.keys)
                if (i < a.keyCodeAlt.Length)
                    if (gui.Button(a.keyCodeAlt[i] + "", serverButton, h))
                        FetchKey(a, i);
                    //else
                    //    gui.Button(" ", h);
            gui.EndVertical();
        }
        gui.EndHorizontal();
        gui.EndScrollView();
    }
    private void FetchKey(KeyValue a, int i)
    {
        StartCoroutine(AddMethod(() => Input.anyKeyDown, delegate
        {
            a.keyCodeAlt[i] = FetchKey();
            a.Save();
        }));
    }

    private KeyCode FetchKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            return KeyCode.None;
        for (int i = 0; i < 429; i++)
        {
            if (Input.GetKeyDown((KeyCode) i))
                return (KeyCode) i;
        }
        return KeyCode.None;

    }
    public void StartLoadReplays()
    {
        _Loader.replays.Clear();
        if (_Game == null)
            win.ShowWindow(_Loader.LoadReplayWindow);
        //if (PlayersCount == 0)
        //    StartCoroutine(StartLoadLevel(mapName));
        //else
        //debugThis = true;
        Download(mainSite + "scripts/getReplay3.php", LoadReplay, true, "hard", difficulty == Difficulty.Easy ? 0 : difficulty == Difficulty.Normal ? 2 : 1, "map", curScene.mapId, "flags", (int)replayFlags, "version", setting.replayVersion, "count", PlayersCount == 0 ? 3 : PlayersCount); //+ (difficulty == Difficulty.Easy ? 2 : 0)
    }
    private void LoadReplay(string text, bool success)
    {
        try
        {
            if (success)
            {
                StartCoroutine(LoadReadReplay(JsonMapper.ToObject(text)));
                return;
            }
            //text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
        } catch (Exception e) { Debug.LogError(e); }
        if (_Game == null)
            StartCoroutine(StartLoadLevel(mapName));
    }
    protected IEnumerator LoadReadReplay(JsonData data)
    {
        //var split2 = split.ToList();
        //var cnt = split.Length;
        //if (includeFriends)
        //    foreach (var a in friends.Where(a => a.ToLower() != playerName.ToLower() && a.ToLower() != playerNamePrefixed.ToLower()).OrderBy(a => Random.value).Take(PlayersCount))
        //        split2.Insert(0, "replays/" + a + "." + mapNamePrefixed + ".rep");
        tempReplays = new List<Replay>();
        List<WWW> wwws = new List<WWW>();
        foreach (JsonData a in (ICollection)data["replays"])
        {
            var url = mainSite + a["path"];
            wwws.Add(new WWW(Uri.EscapeUriString(url)));
        }
        dontUploadReplay = (bool)data["dontUploadReplay"];
        print("dontUploadReplay " + dontUploadReplay);
        while (wwws.Count > 0)
        {
            yield return null;
            for (int i = wwws.Count - 1; i >= 0; i--)
            {
                var www = wwws[i];
                if (www.isDone)
                {
                    wwws.Remove(www);
                    //if (cnt < 1) break;
                    byte[] buffer = null;
                    var hasCache = UnityEngine.PlayerPrefs.HasKey(www.url) && setting.wwwCache;
                    if (!hasCache)
                        yield return www;
                    if (!string.IsNullOrEmpty(www.error) || setting.offline || hasCache)
                    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
                        if (UnityEngine.PlayerPrefs.HasKey(www.url))
                        {
                            buffer = Convert.FromBase64String(UnityEngine.PlayerPrefs.GetString(www.url));
                            print("Bytes cache" + buffer.Length);
                        }
                        else
#endif
                        {
                            Debug.LogError("error:" + www.error + " " + www.url);
                            continue;
                        }
                    }
                    else
                    {
                        buffer = www.bytes;
#if UNITY_ANDROID || UNITY_IOS|| UNITY_EDITOR
                        //print("Bytes cache" + value.Length);
                        PlayerPrefs.SetString2(www.url, Convert.ToBase64String(buffer));
#endif
                    }
                    //try
                    //{
                    Replay rep = ReadReplay(buffer);
                    if (rep == null || rep.posVels.Count < 100) print("replay is empty " + www.url);
                    else
                    {
                        //var plName = www.url.Substring(www.url.LastIndexOf("/") + 1).Split('.')[0];
                        //rep.playerName = plName;
                        tempReplays.Add(rep);
                        cnt--;
                    }
                    //} catch (Exception e)
                    //{
                    //    Debug.LogError(e);
                    //}
                }
            }
        }
        print("Replays Loaded: " + tempReplays.Count);
        replays = tempReplays;
        //if (replays.Count > 0 && gameType == GameType.VsPlayers)
        //{
        //    replays.Sort(replays[0]);
        //    var c = replays.Count - Mathf.Max(3, PlayersCount);
        //    if (c > 0)
        //        replays.RemoveRange(0, c);
        //}
        yield return new WaitForSeconds(1);
        //if ((vsFriendsOrClan || gameType == GameType.Replay) && tempReplays.Count == 0)
        //    Popup("failed to load", _Loader.SelectMapWindow);
        //else
        StartCoroutine(StartLoadLevel(mapName));
    }
    private Replay ReadReplay(byte[] buffer)
    {
        Replay rep = new Replay();
        if (buffer == null) { print("Buffer is null"); return rep; }
        rep.contry = _Loader.Country;
        using (var ms = new BinaryReader(buffer))
        {
            Vector3 oldPos = Vector3.zero;
            float oldmeters = 0;
            PosVel posvel = null;
            int vers = 0;
            int errors = 0;
            while (ms.Position < ms.Length)
            {
                lastRd = curredRd;
                var b = curredRd = ms.ReadByte();
                try
                {
                    if (b == RD.playerName)
                    {
                        rep.playerName = ms.ReadString();
                        Debug.LogWarning(rep.playerName);
                    }
                    else if (b == RD.clan)
                        rep.clanTag = ms.ReadString();
                    else if (b == RD.version)
                    {
                        if (vers == 1241 || vers == 1234) return null;
                        vers = ms.ReadInt();
                    }
                    else if (b == RD.posVel)
                    {
                        posvel = new PosVel();
                        posvel.pos = ms.ReadVector();
                        if (oldPos == Vector3.zero)
                            oldPos = posvel.pos;
                        posvel.meters = oldmeters = oldmeters + (posvel.pos - oldPos).magnitude;
                        oldPos = posvel.pos;
                        posvel.rot.eulerAngles = ms.ReadVector();
                        posvel.vel = ms.ReadVector();
                        rep.posVels.Add(posvel);
                    }
                    else if (b == RD.score)
                        /*posvel.score = */
                        ms.ReadInt();
                    else if (b == RD.posVelMouse)
                        posvel.mouserot = ms.ReadFloat();
                    else if (b == RD.posVelSkid)
                        posvel.skid = ms.ReadFloat();
                    else if (b == RD.keyCode)
                    {
                        var kc = (KeyCode)ms.ReadInt();
                        float t = ms.ReadFloat();
                        bool d = ms.ReadBool();
                        //if (Player.recordKeys.Contains(kc))
                        rep.keyDowns.Add(new KeyDown() { down = d, keyCode = kc, time = t });
                    }
                    else if (b == RD.avatarId)
                        rep.avatarId = ms.ReadInt();
                    else if (b == RD.carSkin)
                        rep.carSkin = ms.ReadInt();
                    else if (b == RD.FinnishTime)
                        rep.finnishTime = ms.ReadFloat();
                    else if (b == RD.country)
                    {
                        CountryCodes countryCodes = (CountryCodes)ms.ReadByte();
                        //if (countryCodes != CountryCodes.fi)
                        rep.contry = countryCodes;
                        //print("Read Country " + rep.contry);
                    }
                    else if (b == RD.color)
                        rep.color = new Color(ms.ReadFloat(), ms.ReadFloat(), ms.ReadFloat());
                    else if (b == RD.avatarUrl)
                        rep.avatarUrl = ms.ReadString();
                    else if (b == RD.rank)
                        rep.rank = ms.ReadInt();
                    else
                    {
                        if (errors == 0)
                            Debug.LogError("byte unknown type " + b + " lastType " + lastRd + " version " + vers);
                        errors++;
                        //if (isDebug)
                        //    Debug.LogError("byte unknown type " + b);
                    }
                } catch (Exception e)
                {
                    if (errors == 0)
                        Debug.LogError("error curType" + b + " lastType " + lastRd + " version " + vers + "\n" + e);
                    errors++;
                }
            }
            print("Replay version " + vers + " errors" + errors);
        }
        return rep;
    }
    public IEnumerator DownloadUserMaps(int tested, int levelFlags, string search = "", int p = 0, bool top = false, bool direct = false, int id = 0)
    {
        if (!online)
            levelFlags = 0;
        if (userMaps.Count == 0)
            p = 0;
        var s = string.Format("{0}scripts/getUserMaps2.php?version={1}&map={2}&tested={3}&page={4}&top={5}&direct={6}&flags={7}&id={8}", mainSite, setting.levelEditorVersion, search, tested, p, top ? 1 : 0, direct ? 1 : 0, levelFlags, id);
        var findMyMaps = tested == 0;
        if (findMyMaps)
            s += "&user=" + playerNamePrefixed;
        if (isDebug)
            s += "&r=" + Random.value;
        var w = wwwUserMaps = new WWW(s);
        print(w.url);
        yield return w;
        if (!string.IsNullOrEmpty(w.error))
        {
            print(w.error);
            yield break;
        }
        userMaps.Clear();
        var ss = SplitString(w.text.Trim());
        //print(w.text);
        foreach (var a3 in ss)
        {
            var a2 = a3.Split(';');
            var a = a2[0];
            var substring = a.Substring(a.LastIndexOf("/") + 1);
            var strings = substring.Split('.');
            var plName = strings[0];
            Scene sc = new Scene();
            sc.mapBy = plName;
            sc.name = strings[0] + "." + strings[1];
            sc.title = strings[1];
            sc.j = Tag.userTab;
            sc.url = a;
            sc.userMap = true;
            if (sc.name.ToLower().StartsWith("diablo"))
            {
                print("Diabllo");
            }
            sc.rating = float.Parse(a2[1]);
            if (a2.Length > 2)
            {
                sc.mapSets.levelFlags = (LevelFlags)int.Parse(a2[2]);
                sc.mapId = int.Parse(a2[3]);
            }
            //if (findMyMaps)
            //{
            //levelEditor.myMaps.Add(sc.name);
            //levelEditor.myMaps = levelEditor.myMaps.Distinct().ToList();
            //}
            //else
            userMaps.Add(sc);
        }
        //m_scenes = null;
        page = p;
        print("userMaps loaded:" + userMaps.Count);
        scroll = Vector2.zero;
        scenes = new List<Scene>(res.scenes);
        scenes.AddRange(userMaps);
        //LoadScenes();
    }
    public bool mpCollisions {get { return online && enableCollision; }}
    protected void VoteMap() { }
    public WWW AproveMap(int tested = 1, int advanced = 0)
    {
        var w = Download(string.Format("{0}scripts/aproveMap2.php?map={1}&tested={2}&aprovedBy={3}&advanced={4}", mainSite, _Loader.mapName, tested, playerNamePrefixed, advanced), null, false);
        return w;
    }
    public void RateMapWindow()
    {
        Setup(400, 500);
        win.showBackButton = false;
        gui.Label(Tr("Please rate map: ") + _Loader.mapName);
        //FavoriteMapButton();
        if (isMod && curSceneDef.userMap)
        {
            //gui.BeginHorizontal();
            if (Button("Remove"))
            {
                AproveMap(0);
                win.Back();
            }
            if (Button("Add to new"))
            {
                AproveMap(1);
                win.Back();
            }
            if (Button("Add to Favorites"))
            {
                AproveMap(2);
                win.Back();
            }
            //gui.EndHorizontal();
        }
        //Download(mainSite + "scripts/aproveMap2.php?map=" + _Loader.mapName + "&tested=" + 0, null, false);
        for (int i = 1; i <= 5; i++)
        {
            if (gui.Button(res.rating[i * 2]))
            {
                Download(mainSite + "scripts/rateMap2.php?map=" + _Loader.curScene.mapId + "&rate=" + i + "&user= " + playerNamePrefixed + "&mapname=" + mapName + "&finished=" + _Game.finnish, null, false);
                PlayerPrefsSetBool("voted" + mapName, true);
                if (i == 5 && !_Loader.favorites.Contains(mapName))
                    _Loader.favorites.Add(mapName);
                win.Back();
                //if (_Game != null && _Game.finnish)
                //    _Integration.AddReputation(1);
            }
        }
    }
    public void FullScreen(bool fullscreen)
    {
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, fullscreen);
    }
    //void PrintPrefs()
    //{
    //    LogRight("Keys________");
    //    foreach (string Key in playerPrefKeys)
    //        LogRight(Key + PlayerPrefsGetString(Key).PadRight(20));
    //    if (Application.platform == RuntimePlatform.WindowsWebPlayer)
    //    {
    //        LogRight("Prints____________");
    //        for (int i = prints.Count - 1; i >= 0; i--)
    //            LogRight(prints[i]);
    //        LogRight("________");
    //    }
    //}
#if old
    protected IEnumerator ParseUrl(string url)
    {
        urlReceived = false;
        //string[] ss;
        GroupCollection ss;
        try
        {
            var m = Regex.Match(url, @".*\?(.*?)\.(.*?)\.rep.?(.*)");
            if (!m.Success) { throw new System.Exception("does not have .rep"); }
            ss = m.Groups;
            print("groups count " + ss.Count);
            //var parsed = url.Split('?')[1];
            //var pa2 = parsed.Split(new[] { ".rep" }, StringSplitOptions.None);
            //ss = parsed.Split(new[] { '.' }, 1);
            mapName = ss[2].Value;
            //if (ss.Count > 4)
            if (!string.IsNullOrEmpty(ss[3].Value))
                dict[ss[1].Value] = Uri.UnescapeDataString(ss[3].Value);
            sGameType = SGameType.VsFriends;
            LogEvent("PlayFromUrl");
        } catch (System.Exception e)
        {
            Debug.Log(e);
            WindowPool();
            yield break;
        }
        _LoaderScene.loadingUrl = true;
        StartCoroutine(LoadReadReplay(new[] { "replays/" + ss[1] + "." + _Loader.mapNamePrefixed + ".rep" }));
    }
#endif
    public void RestartLevel()
    {
        print("Restart Level " + _Loader.mapName);
        LoadLevel(_Loader.mapName);
    }
    public void OnApplicationQuit()
    {
        print("OnApplicationQuit");
        loadingLevelQuit = true;
        SaveStrings();
        if (!Debug.isDebugBuild)
            LogEvent(EventGroup.playedTime, "played " + (int)((Time.realtimeSinceStartup - gameStartTime) / 60f) + "minutes");
    }
    public static WWW WWW(string url, int version)
    {
        if (Application.platform == RuntimePlatform.FlashPlayer)
            return new WWW(url + "?rnd=" + version);
        else
            return UnityEngine.WWW.LoadFromCacheOrDownload(url, (setting.noLevelCache ? Random.Range(0, 1000) : version));
    }    
    public void LoadCarSkins()
    {
        res.CarSkins = new List<CarSkin>();
        var splitString = SplitString(setting.carSkinsTxt.text);
        for (int i = 0; i < splitString.Length; i++)
        {
            var a = splitString[i];
            var car = new CarSkin();
            car.prefabName = a;
            if (i == splitString.Length - 1)
                car.friendsNeeded = 3;
            else
                car.medalsNeeded = (int)((float)i / splitString.Length * (SceneCount * 2));
            res.CarSkins.Add(car);
        }
    }
    public CarSkin GetCarSkin(int id, bool mine)
    {
        return CarSkins[lowestOrAndroidLow && !mine ? 0 : Mathf.Clamp(id, 0, CarSkins.Count)];
    }
    internal float matchTimeLimit;
}

