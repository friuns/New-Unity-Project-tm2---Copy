//using System.Linq;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
public class Settings{}

public class ResLoader : bs
{
    //public bool enableMatchTime = true;
    public bool changeSkin;
    public int lerpFactorPos = 3;
    public int lerpFactorRot = 3;
    //public bool useTowards;
    public bool optimization;
    public bool dontLoadAssets;
    public bool disablePlayerPrefs ;//{ get { return _Loader.disablePlayerPrefs; } set { _Loader.disablePlayerPrefs = value; } }
    public bool disPlayerPrefs2;
    public bool disableTranslate;
    public bool wwwCache;
    public bool lagPerf;
    public bool lagNetw;
    public bool autoConnect;
    public bool autoHost;
    public int packageVersion;
    public int packageVersion2;
    public bool enableGuiEdit;
    public bool m_android;
    public bool m_ios;
    public int version = 20;
    public int multiplayerVersion;
    public int replayVersion = 1205;
    public int levelEditorVersion = 20;
    public int physicVersion = 6;
    public int MapVersion = 2;
    public int androidMapVersion = 3;
    public bool debug = true;
    //public bool debug2 = true;
    public bool enableLog=true;
    public bool localhost = true;
    //public bool noWindowAnim = true;
    public bool noLevelCache;
    public bool skipLogin;
    public bool DontWait { get { return skipLogin; } }
    internal bool inited;
    public bool delayLoading;
    internal bool vk2; 
    public bool testVK;
    public bool offline;
    public bool fps10;
    internal bool unitTest;

    //public bool enableCollisionCleanup;
    public List<Map> maps = new List<Map>();
    //public bool includeRes;
    public void Disable() { }
    public void InitSettings()
    {
        //if (!Debug.isDebugBuild)
            //debug = false;
        //noWindowAnim = debug;
        inited = true;

        //skipLogin = skipLogin | autoHost | autoConnect;
        if (!Debug.isDebugBuild)
            changeSkin=optimization = ForceLogin = disablePlayerPrefs = disPlayerPrefs2 = AngleTest = disableTranslate = testVK = dontLoadAssets = debug = lagNetw = lagPerf = autoHost = autoConnect = unitTest = wwwCache = enableGuiEdit = fps10 = offline = enableLog = m_ios = m_android = noLevelCache = hideCull = skipLogin = delayLoading = localhost = false;            
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            m_ios = true;
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.WP8Player)
            m_android = true;
        //if (android)
        //    vk = false;
        

    }


    public Texture2D[] flags = new Texture2D[5];
    public TextAsset[] assetDictionaries;
    public TextAsset carSkinsTxt;

    public bool useKeysForGui;
    public bool sendWmp=true;
    public bool hideCull = false;
    
    internal string hostTHisGame = @"You are allowed to share, link, download, publish, or embed this game on your website or other platforms, provided that the games are republished in exactly the same manner as provided on this web page. Games may not be modified in any way whatsoever, and all branding and hyperlinks included therein must be maintained.";
    public float hitTestForce = .99f;

    internal bool playerPrefSecurity=true;


    public bool ForceLogin;

    public bool AngleTest;
    public bool enableDrag = true;
    public bool zanos=false;
    public bool timeLapse;
    public bool speedTweak = true;
    public string[] backups;
    public string[] appIds;
    [EnumFlagsAttribute]
    public LogLevel logLevel;
    public string versionDate;
    public bool disablePool;

    public void SetFlag(LogLevel Level, bool value)
    {
        if (value)
            logLevel |= Level;
        else
            logLevel &= ~Level;
    }
    public bool GetFlag(LogLevel Level)
    {
        return (logLevel & Level) != 0;
    }
    public List<string> parsedLevels = new List<string>();
    private Dictionary<string, float> popularity;
    public Dictionary<string, float> Popularity
    {
        get
        {
            if (popularity == null)
            {
                popularity = new Dictionary<string, float>();
                for (int i = 0; i < populartyKeys.Count; i++)
                    popularity.Add(populartyKeys[i], populartyValues[i]);
            }
            return popularity;
        }
    }
    public List<string> populartyKeys = new List<string>();
    public List<float> populartyValues = new List<float>();
}

