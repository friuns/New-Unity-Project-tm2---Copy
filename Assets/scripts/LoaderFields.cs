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
    public GuiSkins guiSkins;
    internal LevelEditor levelEditor;
    internal MapLoader mapLoader;
    internal Game game;
    public AudioClip pushButton;
    public AudioClip click;
    public ResLoader m_setting;
    public new Res res;
    public new Awards _Awards;
    public new GUIText guiText;
    public GUIText guiTextRight;
    //public Database database;
    internal string url = "";
    internal bool urlReceived;
    public InputManager inputManger;
    internal float gameStartTime;
    protected int maxLength = 12;
    protected int minLength = 3;
    internal string confirmPassword = "";
    internal int wonMedals;
    public static WWW mapWww;
    internal static string lastLog = "";
    protected float levelLoadTime;
    internal string oldLevel;
    //protected bool acceptLicense;
    public TextAsset assetDictionaryMergeTo;
    public TextAsset assetDictionaryMerge;
    public TextAsset attack;
    internal bool gamePlayed;
    private bool wonCarShown;
    internal List<Replay> replays = new List<Replay>();
    protected List<Replay> tempReplays = new List<Replay>();
    public Font font;
    protected List<Replay> previews = new List<Replay>();
    public float matchTime = 3 * 60;
    public bool enableMatchTime;
    public bool dmShootForward = true;
    public SGameType sGameType = SGameType.VsPlayers;

#if oldStunts
    public bool stunts;
#endif
    //{ get { return gameType == GameType.zombies; } set { gameType = value ? GameType.zombies : GameType.race; } }
    internal bool loggedIn;
    internal int banTime { get { return PlayerPrefs.GetInt("ban", 0); } set { PlayerPrefs.SetInt("ban", value); } }
    public bool banned { get { return Loader.totalSeconds - banTime < 0; } }
    internal CullEnum CullMode = CullEnum.Smooth;
    public bool enableCollision;
    internal float airFactor = 0;
    internal float rotationFactor = 1;
    internal int lastVersion;
    internal DateTime? serverTime;
    internal bool menuLoaded;
    internal WWW wwwAssetBundle;
    public Dictionary<string, List<Thumbnail>> m_thumbnails;
    public Dictionary<string, List<Thumbnail>> thumbnails { get { if (m_thumbnails == null) StartCoroutine(LoadTextres()); return m_thumbnails; } }

    internal string[] thumbnailKeys;
    internal List<Thumbnail>[] thumbnailValues;
    public static string resourcesPath = "Assets/Resources/MapTextures/";
    private string[] titles { get { return res.titles; } set { res.titles = value; } }
    internal string lastError;
    internal bool isOdnoklasniki;
    internal bool vk { get { return _Integration.site == Site.Kg || _Integration.site == Site.VK; } }
    internal bool vkSite { get { return _Integration.site == Site.VK; } }
    public bool disableRep { get { return !_Loader.vkSite /*&& curDict != 1*/&& !isDebug; } }
    public float deltaTime = 0.01f;
    private Vector2 oldTouch;
    public Scene curSceneDef
    {
        get
        {
            if (curScene == null)
                return new Scene() { name = mapName };
            return curScene;
        }
    }
    public Scene curScene { get { return scenes.FirstOrDefault(a => a.name.ToLower() == mapName.ToLower()); } }
    
    private int lastRd;
    private int curredRd;
    public bool dontUploadReplay;
    public bool carsCheat2;
    public bool carsCheat;
    public static Dictionary<string, Map> m_maps;
    public static Dictionary<string, Map> maps
    {
        get
        {
            if (m_maps == null)
            {
                m_maps = new Dictionary<string, Map>();
                foreach (var a in setting.maps)
                    m_maps[a.name] = a;
            }
            return m_maps;
        }
    }
    public bool Beginner;//{ get { return medals < 1; } }
    //public bool wallCollision=true;
    public int SceneCount
    {
        get
        {
            return scenes.Count;
            //int count = 0;
            //foreach (Scene a in scenes)
            //    if (a.Enabled) count++;
            //return count;
        }
    }
    protected CarSkin WonCar
    {
        get
        {
            for (int i = 0; i < CarSkins.Count; i++)
            {
                CarSkin a = GetCarSkin(i, true);
                if (!wonCarShown)
                    if (a.medalsNeeded > medals - wonMedals && a.medalsNeeded <= medals)
                    {
                        wonCarShown = true;
                        return a;
                    }
            }
            return null;
        }
    }

    //internal bool enableKeys = true;
    public string replayLinkPrefix
    {
        get
        {
            //if (_Integration.vkLoggedIn || _Loader.url.ToLower().Contains("vk.com".ToLower()))
            //    return "http://vk.com/app3935060?";
            //if (_Integration.fbLoggedIn)
            //    return "https://apps.facebook.com/trackracing/?";
            //return "http://trackracingonline.com/web2/?";
            return _Loader.url.Split('?')[0] + "?";
        }
    }
    public string replayLink { get { return _Loader.playerNamePrefixed + "." + _Loader.mapName + ".rep"; } }


    public bool tester
    {
        get { return _Loader.modType >= ModType.tester; }
    }

    public MapSets mapSets = new MapSets();
    //public HashSet<string> voted = new HashSet<string>();
    public float lastVote;
    internal List<Scene> userMaps = new List<Scene>();
    internal int page;
    internal string mapName;
    internal string levelName;
    protected string friendName = "";
    //protected bool guestAccount;
    //internal Texture2D selectedSceneIcon;
    //internal List<Scene> m_scenes { get { return res.scenes; } set { res.scenes = value; } }

    internal List<Scene> scenes;//{ get { return res.scenes; } }
    public TextAsset scenesTxt;
    public GUITexture fullScreen;
    private Color m_fullScreenColor;
    private Color fullScreenColor { set { if (value != m_fullScreenColor)  m_fullScreenColor = fullScreen.color = value; } }
    internal static int errors;
    private static float oldTime;
    protected List<string> clanMembers = new List<string>();
    public int dayBonus { get { return PlayerPrefsGetInt(playerName + "daybonus", 0); } set { PlayerPrefsSetInt(playerName + "daybonus", value); } }
    public int lastDayPlayed { get { return PlayerPrefsGetInt(playerName + "lastDayPlayed"); } set { PlayerPrefsSetInt(playerName + "lastDayPlayed", value); } }
    protected bool disableEveryPlay;

    public bool isLoading
    {
        get { return mapWww != null && !mapWww.isDone; }
    }
    [Flags]
    public enum ReplayFlags { dm = 1, stunts = 2, online = 4, rain = 8, bombcar = 16 }

    public ReplayFlags replayFlags
    {
        get
        {
            ReplayFlags f = 0;
            if (_Loader.dm)
                f |= ReplayFlags.dm;
#if oldStunts
            if (stunts)
                f |= ReplayFlags.stunts;
#endif
            if (online)
                f |= ReplayFlags.online;
            if (rain)
                f |= ReplayFlags.rain;
            if (bombCar)
                f |= ReplayFlags.bombcar;
            return f;
        }
    }
    public string mapNamePrefixed { get { return mapName + "@" + setting.physicVersion + (_Loader.dm ? "@dm" : online ? "@online" : rain ? "@rain" : "") + (bombCar ? "@bombcar" : ""); } }

    public Queue<EvalCor> evalCors = new Queue<EvalCor>();
    public bool m_guest = true;
    public static bool loadingLevelQuit;
    public bool bombCar;
    public List<CarSkin> CarSkins { get { if (res.CarSkins.Count == 0) LoadCarSkins(); return res.CarSkins; } }
    public float nitro;
    
}