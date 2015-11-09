#if UNITY_WP8
using ObscuredInt = System.Int32;
#else
using CodeStage.AntiCheat;
using CodeStage.AntiCheat.ObscuredTypes;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
#if UNITY_EDITOR
//using Exception = System.AccessViolationException;
using UnityEditor;
#endif

using UnityEngine;
using Random = UnityEngine.Random;


public partial class Loader:GuiClasses
{
    internal string prefixMapPl;
    internal string prefixplat { get { return playerName + platformPrefix; } }

    internal string playerNamePrefixed = "guest888";
    internal string contry { get { return PlayerPrefsGetString("country"); } set { PlayerPrefsSetString("country", value); } }

    public CountryCodes Country { get { return (CountryCodes)PlayerPrefsGetInt("country2", (int)Country2); } set { PlayerPrefsSetInt("country2", (int)value); } }
    private CountryCodes Country2
    {

        get
        {
            try
            {
                return string.IsNullOrEmpty(contry) ? CountryCodes.fi : (CountryCodes)Enum.Parse(typeof(CountryCodes), contry.ToLower());
            }
            catch (ArgumentException)
            {
            }
            return CountryCodes.fi;
        }
    }

    internal string m_clanTag;
    internal string clanTag { get { return m_clanTag ?? (m_clanTag = PlayerPrefsGetString("clanTag")); } set { PlayerPrefsSetString("clanTag", m_clanTag = value); } }

    internal string m_playerName;
    internal string playerName { get { return m_playerName ?? (m_playerName = PlayerPrefsGetString("playerName")); } set { PlayerPrefsSetString("playerName", m_playerName = value); } }
    internal string clanName { get { return PlayerPrefsGetString("clanName"); } set { PlayerPrefsSetString("clanName", value); } }

    protected bool rememberPassword = true;
    private string m_password = "";
    internal string password { get { return rememberPassword ? PlayerPrefs.GetString(playerName + "password") : m_password; } set { if (rememberPassword) PlayerPrefs.SetString(playerName + "password", value); else m_password = value; } }
    internal string vkPassword { get { return PlayerPrefs.GetString("vkpassword"); } set { PlayerPrefs.SetString("vkpassword", value); } }

    

    //public bool reverseSplitScreen { get { return PlayerPrefsGetBool(playerName + "reverseSplitScreen", android); } set { PlayerPrefsSetBool(playerName + "reverseSplitScreen", value); } }
    
    public bool accelometer { get { return controls == Contr.acel; } }
    public bool autoFullScreen { get { return PlayerPrefsGetBool("autoFullScreen", Application.isWebPlayer && !Nancl); } set { PlayerPrefsSetBool("autoFullScreen", value); } }
    //public static bool webPlayer { get { return Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer; } }
    public bool showYourGhost { get { return PlayerPrefsGetBool(prefixplat + "showYourGhost", !android && !Application.isEditor); } set { PlayerPrefsSetBool(prefixplat + "showYourGhost", value) ; } }
    //public bool statsSaved { get { return PlayerPrefsGetBool(playerName + "StatsSaved2"); } set { PlayerPrefsSetBool(playerName + "StatsSaved2", value); } }
    public float record { get { return PlayerPrefsGetFloat(6 + prefixMapPl + "record", float.MaxValue); } set { PlayerPrefsSetFloat(6 + prefixMapPl + "record", value); } }
    public int avatar { get { return PlayerPrefsGetInt(playerName + "avatar", isDebug ? 0 : -1); } set { PlayerPrefsSetInt(playerName + "avatar", value); } }
    public Texture2D Avatar { get { return res.GetAvatar(avatar,avatarUrl); } }
    internal string avatarUrl { get { return PlayerPrefsGetString("avatarUrl", ""); } set { PlayerPrefsSetString("avatarUrl", value); } }
    //public bool useUrlAvatar { get { return PlayerPrefsGetBool("useUrlAvatar"); } set { PlayerPrefsSetBool("useUrlAvatar", value); } }

    public int carSkin { get { return PlayerPrefsGetInt(playerName + "car", 0); } set { PlayerPrefsSetInt(playerName + "car", value); } }
    

    public int place
    {
        get
        {
            return PlayerPrefsGetInt(prefixMapPl + "medal", 4);
        }
        set
        {
            PlayerPrefsSetInt(prefixMapPl + "medal", value);
        }
    }

    internal float voiceChatVolume=1;
    internal bool enableChat = true;
    
    public int PlayersCount { get { return PlayerPrefsGetInt(prefixplat + "PlayersCount", android ? 0 : 3); } set { PlayerPrefsSetInt(prefixplat + "PlayersCount", value); } }
    
    public bool scaleButtons { get { return PlayerPrefsGetBool(prefixplat + "scaleButtons", android); } set { PlayerPrefsSetBool(prefixplat + "scaleButtons", value); } }

    public bool enableBloom { get { return PlayerPrefsGetBool(prefixplat + "enableBloom", false); } set { PlayerPrefsSetBool(prefixplat + "enableBloom", value); } }

    

    public List<string> favorites { get { return PlayerPrefsGetStrings("favs"); } set { PlayerPrefsSetStringList("favs", value); } }

    public List<string> friends { get { return PlayerPrefsGetStrings(playerName + "friends", playerNamePrefixed); } set { PlayerPrefsSetStringList(playerName + "friends", value); } }
    //public static int cache { get { return PlayerPrefsGetInt("cache"); } set { PlayerPrefsSetInt("cache", value); } }

    public SecureInt medals
    {
        get { return PlayerPrefsGetInt(playerName + "medals"); }
        set { PlayerPrefsSetInt(playerName + "medals", value); }
    }

    public SecureInt warScore
    {
        get { return PlayerPrefsGetInt(playerName + "warPoints"); }
        set { PlayerPrefsSetInt(playerName + "warPoints", value); }
    }

    public SecureInt reputation
    {
        get { return PlayerPrefsGetInt(playerName + "reputation"); }
        set { PlayerPrefsSetInt(playerName + "reputation", value); }
    }

    public SecureInt friendCount
    {
        get { return PlayerPrefsGetInt("friendCount", -1); }
        set { PlayerPrefsSetInt("friendCount", value); }
    }


    public SecureInt score
    {
        get { return PlayerPrefsGetInt(playerName + "score"); }
        set { PlayerPrefsSetInt(playerName + "score", value); }
    }

    public int? m_controls;
    public int controls { get { return m_controls ?? (m_controls = PlayerPrefsGetInt(prefixplat + "controls2", Contr.keys)).Value; } set { PlayerPrefsSetInt(prefixplat + "controls2", (m_controls = value).Value); } }

    public bool m_enableMouse;
    public bool enableMouse { get { return m_enableMouse; } set { m_enableMouse = value; } }
    //{ get { return controls == ; } set { controls = value ? Contr.mouse : Contr.keys; } }

    
    public int? m_drawDistance;
    public int drawDistance { get { return m_drawDistance ?? (m_drawDistance = PlayerPrefsGetInt(prefixplat + "drawDistance", 1000)).Value; } set { PlayerPrefsSetInt(prefixplat + "drawDistance", (m_drawDistance = value).Value); } }

    public int? m_modType;
    public int modTypeInt { get { return m_modType ?? (m_modType = PlayerPrefsGetInt(prefixplat + "modType", 0)).Value; } set { PlayerPrefsSetInt(prefixplat + "modType", (m_modType = value).Value); } }
    public ModType modType { get { return isDebug ? (ModType)100 : (ModType)modTypeInt; } }

    public Quality2 _quality { get { return m_quality ?? (m_quality = (Quality2)PlayerPrefsGetInt(prefixplat + "quality", (int)(isDebug ? Quality2.Medium : Quality2.Low))).Value; } set { PlayerPrefsSetInt(prefixplat + "quality", (int)(m_quality = value)); } }
    public Quality2? m_quality;

    public Difficulty? m_difficulty;
    public Difficulty difficulty { get { return m_difficulty ?? (m_difficulty = (Difficulty)PlayerPrefsGetInt(prefixplat + "difficulty", 0)).Value; } set { PlayerPrefsSetInt(prefixplat + "difficulty", ((int)(m_difficulty = value).Value)); } }
    

    public bool? m_autoQuality;
    public bool autoQuality { get { return (m_autoQuality ?? (m_autoQuality = PlayerPrefsGetBool(prefixplat + "autoQuality", !androidPlatform && !isDebug)).Value); } set { PlayerPrefsSetBool(prefixplat + "autoQuality", (m_autoQuality = value).Value); } }

    public bool? m_rearCamera;
    public bool rearCamera { get { return (m_rearCamera ?? (m_rearCamera = PlayerPrefsGetBool(prefixplat + "rearCamera", false)).Value); } set { PlayerPrefsSetBool(prefixplat + "rearCamera", (m_rearCamera = value).Value); } }
    public int playedTimes { get { return PlayerPrefsGetInt("playedTimes"); } set { PlayerPrefsSetInt("playedTimes", value); } }
    public int msgId { get { return PlayerPrefsGetInt("msgId"); } set { PlayerPrefsSetInt("msgId", value); } }

    //public bool? m_rain;
    internal bool rain;
    //internal float speedLimit=MaxValue;
    //internal bool speedLimitEnabled;
    //public bool wallCollision = true;
    //{ get { return m_rain ?? (m_rain = PlayerPrefsGetBool(prefixplat + "rain", false)).Value; } set { PlayerPrefsSetBool(prefixplat + "rain", (m_rain = value).Value); } }
    //public bool? m_night;
    public bool night;
    internal bool topdown;
    //{ get { return m_night ?? (m_night = PlayerPrefsGetBool(prefixplat + "night", false)).Value; } set { PlayerPrefsSetBool(prefixplat + "night", (m_night = value).Value); } }


    public bool? m_reverseSplitScreen;
    public bool reverseSplitScreen { get { return m_reverseSplitScreen ?? (m_reverseSplitScreen = PlayerPrefsGetBool(prefixplat + "reverseSplitScreen", android && !ouya)).Value; } set { PlayerPrefsSetBool(prefixplat + "reverseSplitScreen", (m_reverseSplitScreen = value).Value); } }
    private static bool ouya
    {
        get
        {
#if UNITY_FLASH
            return false;
#else
            return !string.IsNullOrEmpty(SystemInfo.deviceModel) && SystemInfo.deviceModel.ToLower().Contains("ouya");
#endif
        }
    }

    protected float? m_audioVolume;
    internal float soundVolume2 { get { return m_audioVolume ?? (m_audioVolume = flash && !isDebug ? 0 : PlayerPrefsGetFloat("audioVolume2", .5f)).Value; } set { PlayerPrefsSetFloat("audioVolume2", (m_audioVolume = value).Value); } }
    internal float soundVolume = 1;
    protected float? m_musicVolume;
    public float musicVolume { get { return m_musicVolume ?? (m_musicVolume = PlayerPrefsGetFloat(prefixplat + "musicVolume3", isDebug?0:1)).Value; } set { PlayerPrefsSetFloat(prefixplat + "musicVolume3", (m_musicVolume = value).Value); } }    
    public float? m_flying;
    public float flying { get { return m_flying ?? (m_flying = PlayerPrefsGetFloat(prefixMapPl + "flying")).Value; } set { PlayerPrefsSetFloat(prefixMapPl + "flying", (m_flying = value).Value); } }
    //public float? m_fieldOfView;
    internal float fieldOfView;//{ get { return m_fieldOfView ?? (m_fieldOfView = PlayerPrefsGetFloat(prefixplat + "fieldOfView", 65)).Value; } set { PlayerPrefsSetFloat(prefixplat + "fieldOfView", (m_fieldOfView = value).Value); } }
    public float? m_sensivity;
    public float sensivity { get { return m_sensivity ?? (m_sensivity = PlayerPrefsGetFloat(prefixplat + "sensivity", 1)).Value; } set { PlayerPrefsSetFloat(prefixplat + "sensivity", (m_sensivity = value).Value); } }
    public bool shadows { get { return PlayerPrefsGetBool(prefixplat + "shadows", false) && !androidPlatform; } set { PlayerPrefsSetBool(prefixplat + "shadows", value); } }
    public bool enableBlur { get { return PlayerPrefsGetBool(prefixplat + "motionblur", true); } set { PlayerPrefsSetBool(prefixplat + "motionblur", value); } }

    //public bool postFeedBack { get { return PlayerPrefsGetBool(prefixplat + "postFeedBack", true); } set { PlayerPrefsSetBool(prefixplat + "postFeedBack", value); } }


    public void RefreshPrefs()
    {
        SaveStrings();
        PlayerPrefsRefresh();
        //m_medals = null;//m_playerName= m_reverseSplitScreen=m_musicVolume=m_difficulty=m_quality=m_controls=m_audioVolume=m_flying= m_sensivity = 
    }
}