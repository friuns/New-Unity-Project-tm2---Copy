#define GA

using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using Ionic.Zlib;
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
//using Exception = System.AccessViolationException;
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


public partial class Loader
{
    private const int MaxLength = 500;
    public int userId;
    protected IEnumerator LoadPlayerPrefs(string text)
    {

        WWW w = null;
        //if (!statsSaved)
        {
            var s = mainSite + "/players/" + playerName.ToLower() + "/prefs.txt";
            if (isDebug)
                s += "?" + Random.value;
            Debug.Log(s);
            w = new WWW(s);
            while (!w.isDone)
            {
                popupText = Tr("Logging in ") + ((int)w.progress * 100) + "%";
                yield return null;
            }
        }
        try
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, string> dict = ParseDict(text);
            modTypeInt = int.Parse(dict.TryGetDontSet("modType", "0"));
            //sb.AppendLine("parsed Rep: " + reputation);
            //sb.AppendLine("parsed medals: " + medals);
            //sb.AppendLine("parsed modType: " + modTypeInt);
            userId = int.Parse(dict.TryGetDontSet("id", "0"));
            print("parsed userId: " + userId);
            bool crypt = false;
            bool ovrd = false;
            var plnameToLower = playerName;
            if (string.IsNullOrEmpty(w.error))
            {
                var buffer = w.bytes;
                try
                {
                    Xor(buffer);
                    buffer = GZipStream.UncompressBuffer(buffer);
                }
                catch (System.Exception) { Debug.LogWarning("Failed uncompress"); Xor(buffer); }
                if (buffer.Length > 0)
                    using (var ms = new BinaryReader(buffer))
                    {

                        sb.AppendLine("loading stats ");
                        int i = 0;
                        var local = playerPrefKeys.Count;
                        while (ms.Position < ms.Length)
                        {
                            var key = ms.ReadString();
                            var value = ms.ReadString();
                            if (value.Length > MaxLength || key.Length > MaxLength)
                            {
                                Debug.LogError(string.Format("too big value {0} {1}", key, value));
                                continue;
                            }
#if !UNITY_WP8
                            if (crypt)
                            {
                                key = ObscuredString.EncryptDecrypt(key);
                                value = ObscuredString.EncryptDecrypt(value);
                            }
#endif
                            if (key == "Enc" && value == "Enc")
                            {
                                Debug.LogWarning("Encoded");
                                crypt = true;
                                continue;
                            }

                            if (key == _DefinePrefsTime && loggedInTime != 0)
                            {
                                ovrd = loggedInTime < int.Parse(value);

                                //if (ovrd)
                                //lastError = "Override Detected";
                                LogEvent(EventGroup.Debug, "Override Detected");
                                Debug.Log("Set override to: " + ovrd);
                                ovrd = false;
                                continue;
                            }
                            i++;

                            //if (!playerPrefKeys.Contains(key)) //may be incorrect
                            //{

                            //if (string.IsNullOrEmpty(PlayerPrefsGetString(key)))
                            var lowerKey = key.ToLower();
                            if (ResLoader.isEditor && !lowerKey.StartsWith(plnameToLower)) continue;

                            if (ovrd || !PlayerPrefs.HasKey(lowerKey))
                                PlayerPrefsSetString(key, value);
                            else
                                playerPrefKeys.Add(key);
                            //}
                            sb.Append(string.Format("{0}:{1},", key, value));
                        }

                        Debug.LogWarning("loading player prefs local:" + local + " remote:" + i + " \n" + (Debug.isDebugBuild ? sb.ToString() : sb.Length.ToString()));
                        SetStrings.Clear();
                        //statsSaved = true;
                    }
                else
                    Debug.LogWarning("player prefs empty");
                //}

            }
            else //if (!w.error.StartsWith("404") && w.error.ToLower().StartsWith("failed downloading"))
                throw new Exception(w.error);
            //if (!setting.disPlayerPrefs2)
            //{
            //print("reputation " + reputation);
            //if (reputation < 5)
            if (_Loader.vkSite)
                StartCoroutine(AddMethod(delegate
                {
                    reputation = Mathf.Max(int.Parse(dict.TryGetDontSet("reputation", "0")), reputation);
                    medals = Mathf.Max(int.Parse(dict.TryGetDontSet("medals", "0")), medals);
                    _Awards.xp.count = Mathf.Max(int.Parse(dict.TryGetDontSet("xp", "0")), _Awards.xp.count);
                }));

            allowSavePrefs = bool.Parse(dict.TryGetDontSet("allowSavePrefs", "true"));
            //}
        }
        catch (System.Exception e)
        {
            SetStrings.Clear();
            //if (!statsSaved)
            {
                Debug.LogError(e);
                lastError = e.Message.Replace("\r", "   ").Replace("\n", "    ");
                //GoOffline();
                LogEvent("Login Failed Critical Error");
                OnLoginFailed(e.Message + "\n" + Tr(" Critical Error"));
                yield break;
            }
        }
        RefreshPrefs();
        //SaveStrings();
        OnLoggedIn();
        WindowPool();
    }
    private const string _DefinePrefsTime = "savePrefsTime";
    internal bool allowSavePrefs = true;
    public IEnumerator SavePlayerPrefs(bool skip = false)
    {
        if (!allowSavePrefs || setting.disablePlayerPrefs)
            yield break;
        var act = win.act;
        SaveStrings();
        if (!skip)
            if (guest) yield break;

        //save= totalSeconds;
        win.ShowWindow(delegate { win.showBackButton = false; Label("Saving stats "); if (BackButtonLeft()) { ShowWindowNoBack(act); act = null; } }, act, true);
        yield return null;
        StringBuilder sb = new StringBuilder();
        byte[] array;
        using (var ms = new BinaryWriter())
        {
            //#if !UNITY_WP8
            ms.Write(_DefinePrefsTime);
            ms.Write(totalSeconds.ToString());
            //#endif
            var forb = new List<string> { "password", "Enc", _DefinePrefsTime };
            var plname = playerName;
            foreach (string key in playerPrefKeys)
            {
                if (!forb.Contains(key))
                {
                    if (ResLoader.isEditor && !key.StartsWith(plname)) continue;
                    var value = PlayerPrefsGetString(key);
                    //if (Key.EndsWith("reputation"))
                    //print("reputation:" + value);
                    if (value.Length < 200 && isDebug)
                        sb.AppendLine(key + "\t\t" + value);

                    if (value.Length > MaxLength || key.Length > MaxLength)
                    {
                        Debug.LogError(string.Format("too big value {0} {1}", key, value));
                        continue;
                    }
                    ms.Write(key);
                    ms.Write(value);
                }

            }
            array = GZipStream.CompressBuffer(ms.ToArray());
            Xor(array);
            print("saving stats_______ :" + ms.Length + " " + array.Length);
            if (isDebug)
                print(sb.ToString());
        }

        var w = DownloadAcc("savePrefs2", null, true, "file", array);
        yield return w;

        if (act != null)
            win.ShowWindow(act);

        if (!w.text.StartsWith("prefs uploaded"))
            Popup(_Loader.lastError = (w.text + w.error), MenuWindow);
    }
    public void ResetPrefs()
    {
        var n = _Loader.playerName;
        var p = _Loader.password;
        var o = _Loader.avatar;
        PlayerPrefsClear();
        //if (Application.isPlaying)
        //{
        //    OnLoggedIn();
        //    RestartLevel();
        //}
        if (!guest)
        {
            _Loader.playerName = n;
            _Loader.password = p;
            _Loader.avatar = o;
        }
    }
    //public int saveTime { get { return PlayerPrefs.GetInt(playerName + "SaveTime"); } set { PlayerPrefs.SetInt(playerName + "SaveTime", value); } }
    public int loggedInTime { get { return PlayerPrefs.GetInt(playerName + "SaveTime"); } set { PlayerPrefs.SetInt(playerName + "SaveTime", value); } }
    //public int uploadPrefsTime { get { return PlayerPrefs.GetInt(playerName + "UploadTime"); } set { PlayerPrefs.SetInt(playerName + "UploadTime", value); } }
}