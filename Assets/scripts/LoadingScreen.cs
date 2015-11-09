//using UnityEditor;

using System.Linq;
using gui = UnityEngine.GUILayout;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;
public class LoadingScreen : bs
{
    public static ResLoader original;
    public static int version;
    public ResLoader preRes;
    public GUISkin loadingBar;
    private static WWW www { get { return Loader.mapWww; } set { Loader.mapWww = value; } }
    public float x = 400;
    private static bool newVersionAvaible;
    public static bool block;
    public static string newVersionAvaibleText = "Please Update Game at http://trackracingonline.com/";
    private string error;
    public static string folderPath="./";
    public static string folderUrl = "file://";
    internal static string loadingText = ".you can download free android version here!\n<color=#00F2FF>http://trackracingonline.com/</color>";
    public static Dictionary<string, string> webSettings = new Dictionary<string, string>();
    public static Dictionary<string, string> cheats = new Dictionary<string, string>();
    public static string[] tips = new string[] { "none" };
    public static bool mapsLoaded;
    private static Map packageMap;
#if old
    public void Start()
    {
        //setting = preRes;
        version = setting.version;
        original = setting;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        preRes.InitSettings();
        if (android)
            loadingText = "Attention! This is beta version, if you got this game from store please report to soulkey4@gmail.com";
        StartCoroutine(StartLoadPackages());
        LogEvent("StartLoading");
    }
    private IEnumerator StartLoadPackages()
    {
        string asd = webPlayer ? ", try to refresh page (F5)" : "\nPlease update game at http://trackracingonline.com";
        var gameStartTime = Time.realtimeSinceStartup;
        print("StartLoadLevels ");
        yield return StartCoroutine(StartLoadLevels());
        
        if (!Application.CanStreamedLevelBeLoaded(Levels.menu))
        {
            foreach (var a in new[] { "packages", "assets" })
            {
                print("Loading " + a);
                string pcgName = a + setting.packageVersion;
                if (!Loader.maps.ContainsKey(pcgName)) { print(error = "Key Not Found " + pcgName + asd); LogEvent("Failed Key Not Found"); yield break; }
                yield return StartCoroutine(LoadMap(pcgName));
                halfDownloaded = .5f;
            }

            if (!Application.CanStreamedLevelBeLoaded(Levels.menu))
            {
                error = "Couldn't Load Packages" + asd;
                LogEvent("Failed Packages");
                yield break;
            }
            //if (assetBundle == null)
            //{
            //    error = "Couldn't Load AssetBundle " + asd;
            //    LogEvent("Failed AssetBundle");
            //    yield break;
            //}
        }
        else
            yield return new WaitForSeconds(2);
        

        Application.LoadLevel(Levels.menu);
        LogEvent(EventGroup.LoadedIn, "loaded in " + (int)((Time.realtimeSinceStartup - gameStartTime) / 60f) + "minutes");
    }
    private float halfDownloaded;

    public void OnGUI()
    {
        if (www == null)
            www = new WWW("");
        GUILayout.Label("Version:" + version + " Pkg:" + setting.packageVersion);
        if (packageMap != null)
            GUILayout.Label("Package " + packageMap.version);
        GUI.skin = loadingBar;
        GUILayout.BeginArea(new Rect(Screen.width / 2f - (x / 2), Screen.height / 2f - 50, x, 600));

        if (newVersionAvaible)
        {
            loadingBar.window.wordWrap = true;
            if (GUILayout.Button(newVersionAvaibleText, loadingBar.window, GUILayout.MinHeight(100)))
                Application.OpenURL(newVersionAvaibleText.Substring(newVersionAvaibleText.IndexOf("http", System.StringComparison.Ordinal)));
        }
        else
        {
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            var progress = (www.progress / 2f) + halfDownloaded;
            loadingBar.horizontalSliderThumb.fixedWidth = Mathf.Clamp(progress * (x - 10), 35, x - 10);
            GUILayout.HorizontalSlider(0, 0, 1);

            GUILayout.Label(GuiClasses.Tr("Loading ") + (int)(progress * 100) + "%");

            GUILayout.Label(loadingText, loadingBar.window);
            GUI.skin.box.wordWrap = true;
            if (!string.IsNullOrEmpty(www.error))
            {
                GUILayout.Box(www.error);
                //Retry();
            }
            else if (!string.IsNullOrEmpty(error))
            {
                GUILayout.Box(error);
                //Retry();
            }
        }
        GUILayout.EndArea();
    }
    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
            ApplicationQuit();
    }
#endif
    public static string GetRandomTip()
    {
        return randomTip;
        //return randomTip = tips[Random.Range(0, tips.Length)].Replace("\\r", "\r").Replace("\\n","\n");
    }
    public static string randomTip { get { return tips[(int)Time.time / 10 % tips.Length].Replace("\\r", "\r").Replace("\\n", "\n"); } }
    public static string[] SplitSpecial(string s)
    {
        
        List<string> ss = new List<string>();
        int io = 0;
        for (int i = 0; i < 100; i++)
        {
            var indexOf = s.IndexOf("##", io, System.StringComparison.Ordinal);
            if (indexOf == -1)
            {
                ss.Add(s.Substring(io, s.Length - io));
                break;
            }
            ss.Add(s.Substring(io, indexOf - io));
            io = Math.Min(s.Length - 1, indexOf + 2);
        }

        return ss.ToArray();
    }
    public static string[] SplitOnce(string s)
    {
        var a = s.IndexOf("\r\n", System.StringComparison.Ordinal);
        var b = a + 2;
        if (a == -1)
        {
            a = s.IndexOf("\n", System.StringComparison.Ordinal);
            b = a + 1;
        }
        if (a == -1) return new string[] { s };
        return new string[] { s.Substring(0, a), s.Substring(b, s.Length - b) };
    }
    public static IEnumerator StartLoadLevels()
    {
        if (mapsLoaded) yield break;
        
        mapsLoaded = true;
        try
        {
            InitFolder();
        } catch (Exception e2)
        {
            print(e2.Message);
        }

        if (LoadingScreen.version == 0)
            LoadingScreen.version = setting.version;

        print("Loading Maps");

        www = new WWW(http + "://tmrace.net/tm/tm.txt" + (isDebug ? "?" + Random.value : ""));
        yield return www;

        string txt;
        if (string.IsNullOrEmpty(www.error))
        {
            Debug.Log(www.url);
            PlayerPrefs.SetString("tm.txt", txt = www.text);
        }
        else
        {
            txt = PlayerPrefs.GetString("tm.txt");
            Debug.LogWarning(www.url + www.error);
            LogEvent("Failed tm.txt");
        }

        if (!string.IsNullOrEmpty(txt))
        {
//#if !UNITY_FLASH
            //string[] ss = www.text.Split(new string[] { "##" }, StringSplitOptions.None);
            string[] ss = SplitSpecial(txt);
            //print(ss.Length);
            foreach (var s in ss)
            {
                //var sp =  s.Split(new char[] { '\r', '\n' }, 2, StringSplitOptions.RemoveEmptyEntries);
                var sp = SplitOnce(s);
                if (sp.Length > 1)
                {
                    //print("Parsed " + sp[0]);
                    webSettings[sp[0].Trim()] = sp[1].Trim();
                }
            }

            if (webSettings.ContainsKey("tips"))
            {
                try
                {
                    List<string> t = new List<string>();
                    foreach (string a in SplitString(webSettings["tips"]))
                    {
                        string[] ar = new string[] {"(android)", "(pc)"};
                        foreach (string b in ar)
                        {
                            var i = a.IndexOf(b, StringComparison.Ordinal);
                            if (i != -1)
                            {
                                if (!(b == ar[0] && !android || b == ar[1] && android))
                                    t.Add(a.Substring(i + b.Length));
                                break;
                            }
                            else if (b == ar[ar.Length - 1])
                                t.Add(a);
                        }

                    }

                    tips = t.ToArray();
                } catch (Exception e) { Debug.LogError(e); }
            }

            SiteBlockCheck(Application.absoluteURL);
            if (webSettings.ContainsKey("ads"))
                setting.appIds = webSettings["ads"].SplitString();
            if (webSettings.ContainsKey("hostthisgame"))
                setting.hostTHisGame = webSettings["hostthisgame"];

            if (webSettings.ContainsKey("credits"))
                res.credits = webSettings["credits"];
            if (webSettings.ContainsKey("blockversions"))
                foreach (var a in SplitString(webSettings["blockversions"]))
                {
                    if (version.ToString().StartsWith(a))
                        block = newVersionAvaible = true;
                }
            
            try
            {
                foreach (var s in SplitString(webSettings["cheats"]))
                {
                    var a = s.Split('=');
                    cheats[a[0].Trim().ToLower()] = a[1].Trim();
                }
            } catch (Exception e) { Debug.LogWarning(e); }


            try
            {
                if (!isDebug)
                    m_mainSite = webSettings["server"].Trim();


                _Loader.beta = webSettings.ContainsKey("beta4");

                if (webSettings.ContainsKey("backup"))
                    setting.backups = SplitString(webSettings["backup"]).Select(a => a.Replace("https://", http + "://")).ToArray();
                else
                    setting.backups = new[] {mainSite};

                var split = SplitString(webSettings["versions"]);
                for (int i = 0; i < split.Length; i++)
                {
                    if (split[i] == platformPrefix)
                    {
                        newVersionAvaible = int.Parse(split[i + 1].Trim()) > setting.version;
                        newVersionAvaibleText = unescape(split[i + 2]);
                        loadingText = unescape(split[i + 3]);
                    }
                }
            } catch (Exception e) { Debug.LogWarning(e); }

//#endif
        }
      
        if (newVersionAvaible && _Loader != null)
        {
            win.ShowWindow(delegate
            {
                win.Setup(600,300);
                win.skin.button.wordWrap = true;
                if (GUILayout.Button(newVersionAvaibleText, GUILayout.MinHeight(100)))
                    Application.OpenURL(newVersionAvaibleText.Substring(newVersionAvaibleText.IndexOf("http", System.StringComparison.Ordinal)));
            }, win.act);
        }
        string getMapsUrl = mainSite + "scripts/getMaps.php" + (isDebug ? "?" + Random.value : "");
        print(getMapsUrl);
        www = new WWW(getMapsUrl);
        yield return www;
        try
        {
            string text = string.IsNullOrEmpty(www.error) || isDebug ? www.text : PlayerPrefs.GetString("backup");
            //print(text);

            List<Map> maps = new List<Map>();
            var splitString = SplitString(text);
            foreach (var path2 in splitString)
            {
                var ss = path2.Split(':');
                var path = ss[0];
                if (ss.Length <= 1) continue;

                if (path.EndsWith(platformPrefix2))
                {
                    var fileName = GetFileNameWithoutExtension(path).Trim();
                    var fileDate = Mathf.Abs(int.Parse(ss[1]));
                    var url = path;
                    maps.Add(new Map() { name = fileName, url = GetFileName(url), fileDate = fileDate });
                }
            }
            PlayerPrefs.SetString("backup", text);
            if (maps.Count > 3)
                setting.maps = maps;
        } catch (Exception e) { LogEvent("Failed getMaps"); Debug.LogError(e); }
    }

    public static void SiteBlockCheck(string url)
    {
        if (webSettings.ContainsKey("blocksites"))
            foreach (var a in SplitString(webSettings["blocksites"]))
            {
                if (url != null && url.Contains(a.Trim()))
                    block = newVersionAvaible = true;
            }
    }
    
    private static void InitFolder()
    {
        if (Application.platform == RuntimePlatform.LinuxPlayer)
            return;
#if UNITY_ANDROID || UNITY_STANDALONE

        if (standAlone)
            InitFolder("./");
        else
        {
            try
            {
                print("initing folder");
                InitFolder("/sdcard/external_sd/tmrace/");
            } catch (Exception e)
            {
                print(e.Message);
                try
                {
                    InitFolder("/sdcard/tmrace/");
                } catch (Exception)
                {
                    InitFolder("/tmrace/");
                }
            }
        }

    }
    private static void InitFolder(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        //Directory.SetCurrentDirectory(path);
        folderUrl = "file://" + path.Replace("./", "");
        folderPath = path;
        print("Folder Inited! " + path);
#endif
    }
    public static IEnumerator LoadMap(string package)
    {        
        packageMap = Loader.maps[package];
        www = null;
        if (android || standAlone)
        {
            print(folderUrl + GetFileName(packageMap.url));
            www = Loader.WWW(folderUrl + GetFileName(packageMap.url), packageMap.fileDate);
            yield return www;
            Debug.Log(www.error);
        }

        if (www == null || !string.IsNullOrEmpty(www.error))
        {
            foreach (var a in setting.backups)
            {
                var url = a + GetFileName(packageMap.url);
                print(url);
                www = Loader.WWW(url, packageMap.fileDate);
                yield return www;

                if (string.IsNullOrEmpty(www.error) || CanStreamedLevelBeLoaded(package)) break;
                else
                    Debug.LogError(www.error);
            }
        }
        //LogEvent(Regex.Replace(www.error, @"[^\w ]", ""));
        if (package.StartsWith("asset"))
            Debug.LogWarning("obsolete AssetBundleSet");
        print("Loaded AssetBundle:" + www.assetBundle);
        
    }
}