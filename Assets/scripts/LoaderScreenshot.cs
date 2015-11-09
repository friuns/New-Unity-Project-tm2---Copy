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
    
}