#define GA

using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
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
#if UNITY_EDITOR
//using Exception = System.AccessViolationException;
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public partial class Loader
{
    public bool screenshotTaken { get { return PlayerPrefsGetBool("screenshotTaken"); } set { PlayerPrefsSetBool("screenshotTaken", value); } }
    private bool showScreenshotText;
    public void TakeScreenshot(bool showText)
    {
        showScreenshotText=showText;
        screenshotTaken = true;
        ExternalCall("Photo");
    }
    public void OnScreenshot(string uploadUrl)
    {
        print("upload url:" + uploadUrl);
        StartCoroutine(CaptureScreenshot(uploadUrl));
    }
    public IEnumerator CaptureScreenshot(string uploadUrl)
    {
        yield return new WaitForEndOfFrame();
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tx = new Texture2D(width, height, TextureFormat.RGB24, false);
        tx.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tx.Apply();
        yield return new WaitForEndOfFrame();
        byte[] screenshotBytes = tx.EncodeToPNG();
        Destroy(tx);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file1", screenshotBytes);
        var w = new WWW(uploadUrl, form);
        yield return w;
        print(w.text + w.error);
        ExternalCall("PhotoSave", w.text);
        if (showScreenshotText)
            centerText(Tr("ScreenshotUploadedText"));
    }
    protected IEnumerator ParseUrl(string url)
    {
        Match m = Regex.Match(url, @".*?load=(.*)");
        if (m.Success)
        {
            var values = m.Groups[1].Value.Split('/');
            mapName = values[0]; //load=mapname/gametype/roomname
            yield return StartCoroutine(DownloadUserMaps(0, 0, mapName, 0, false, true));
            if (values.Length > 2)
            {
                SetOffline(false);
                while (roomInfos.Length == 0)
                    yield return null;
                var room = roomInfos.OrderBy(a => a.name == values[2]).FirstOrDefault(a => CustomProperty(a, props.mapname, "") == mapName);
                if (room == null)
                {
                    this.gameType = (GameType)Enum.Parse(typeof(GameType), values[1]);
                    StartHost();
                }
                else
                    JoinButton(room, true);
            }
            else
                OnMapSelected(curScene);
        }

        //OnMapSelected(new Scene({}));
    }
    public void Url(string s)
    {
        print("call from JS Url received " + s);
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
        //else
        StartCoroutine(ParseUrl(url));
#endif
    }
}