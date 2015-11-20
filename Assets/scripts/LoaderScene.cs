
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if !UNITY_FLASH || UNITY_EDITOR
using System.Linq;
#endif
using LitJson;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using gui = UnityEngine.GUILayout;


public class LoaderScene : GuiClasses
{
    //public GUITexture hostThisGame;
    public GameObject support;
    public void Start()
    {
        _LoaderScene = this;
        MapLoader.unityMap = "";
        if (android || _Loader.vk)
        {
            //Destroy(hostThisGame.gameObject);
            Destroy(support);
        }
        //_Loader.dm = false;
        _Loader.lifeDef = 300;
        //_Loader.wallCollision = true;
        //Game.ghosts.Clear();
        _Loader.mapName = Levels.menu;
        _Loader.menuLoaded = true;
        print("LoaderScene Start version:" + setting.version);
        if (splitScreen && _Loader.reverseSplitScreen)
            Screen.orientation = ScreenOrientation.Landscape;
        Resources.UnloadUnusedAssets();


#if old
            bool everyplay = ios && oldLevel != Levels.carSelect && (Everyplay.SharedInstance.IsSupported() || isDebug);
            bool isRecording = Everyplay.SharedInstance.IsRecording();
            if (everyplay && isRecording)
            {
                Everyplay.SharedInstance.PlayLastRecording();
                Everyplay.SharedInstance.WasClosed += delegate { Everyplay.SharedInstance.StartRecording(); };
            }
            if (everyplay && !isRecording && !disableEveryPlay)
                ShowWindow(_Loader.EveryPlayRecordVideoWindow);
            else
                WindowPool();
#endif
        StartCoroutine(AddMethod(Debug.isDebugBuild ? 0 : .5f, _Loader.WindowPool));

        //StartCoroutine(_Loader.DownloadUserMaps(2,"",0,true));

        if (_Loader.loggedIn && !guest)
            _Loader.SetValue("reputation=" + _Loader.reputation + "&friends=" + _Loader.friendCount + "&medals=" + _Loader.medals + "&gameplays=" + _Loader.playedTimes);

        //if (!isDebug && _Integration.vkLoggedIn && PlayerPrefs.GetInt(_Loader.playerName + "groupJoin") == 0 && _Loader.loggedIn)
        //ShowWindow(JoinVkGroupWindow, win.act);

        //if (_music)
        //    _music.PlayRandom();
        _Awards.RefreshAwards();
        _Loader.guiText.anchor = TextAnchor.UpperLeft;
        _Loader.guiText.transform.position = new Vector3(0, 1, 0);
    }
    public void OnEnable()
    {
        _LoaderScene = this;
    }
    float width;
    public void OnGUI()
    {
        NewsWindow();
    }

    private void NewsWindow()
    {
        if (win.act != _Loader.MenuWindow || _Integration.posts.Count == 0) return;
        GUI.skin = skin;

        var rect = ConvertRect(new Rect(.5f, .1f, .5f, .8f));
        gui.BeginArea(rect, Tr("News"), win.editorSkin.window);
        scroll.x = 10;
        skin.scrollView.fixedWidth = rect.width;
        scroll = gui.BeginScrollView(scroll, false, false, GUIStyle.none, skin.verticalScrollbar);
        //gui.BeginArea(ConvertRect(new Rect(0, 0, .4f, 8f)));
        _Loader.LoadingLabelAssetBundle();
        foreach (var a in _Integration.posts)
        {
            skin.label.wordWrap = true;
            gui.Space(10);

            gui.BeginVertical(a.date.ToShortDateString() + " " + a.title, win.skin.window);

            skin.label.imagePosition = ImagePosition.ImageAbove;
            //gui.Label(a.image);
            skin.label.alignment = TextAnchor.UpperLeft;
            gui.Label(new GUIContent(a.image));
            gui.Label(new GUIContent(a.msg));
            skin.label.imagePosition = ImagePosition.ImageLeft;
            //if (a.image)
            //gui.Label(a.image);
            gui.BeginHorizontal();
            gui.FlexibleSpace();

            if (Button(Tr("Comments:") + a.comments))
            {

                string url = string.Format(_Loader.vkSite ? "https://vk.com/trackracing?w=wall-59755500_{0}%2Fall" : "https://www.facebook.com/trackracingonline/posts/{0}", a.id);
                if (Application.isWebPlayer)
                    ExternalEval(string.Format("window.top.location = '{0}';", url));
                else
                    Application.OpenURL(url);
            }

            gui.EndHorizontal();
            gui.EndVertical();
        }
        //gui.EndArea();
        gui.EndScrollView();
        gui.EndArea();
    }

    public Rect ConvertRect(Rect r)
    {
        return new Rect(r.x * Screen.width, r.y * Screen.height, r.width * Screen.width, r.height * Screen.height);
    }
    public void JoinVkGroupWindow()
    {
        win.showBackButton = false;
        LabelCenter("Вступи в группу игры vk (+20 репутации)", 16, true);
        gui.BeginHorizontal();
#if !UNITY_WP8
        if (gui.Button("Вступить"))
        {
            SaveStrings();
            _Loader.reputation += 20;
            PlayerPrefs.SetInt(_Loader.playerName + "groupJoin", 1);
            win.Back();
            _Loader.FullScreen(false);
            ExternalEval("OpenVkGroup()");

        }
#endif
        if (gui.Button("Не вступать"))
            win.Back();
        gui.EndHorizontal();
    }

    //public void Update()
    //{
    //if (hostThisGame != null)
    //{
    //    //hostThisGame.enabled = win.Window == "MenuWindow";
    //    //if (!hostThisGame.enabled) return;
    //    var hitTest = hostThisGame.HitTest(Input.mousePosition) && !win.WindowHit;
    //    hostThisGame.color = hitTest ? new Color(1, 1, 1, .5f) : Color.white/2;
    //    if (hitTest && Input.GetMouseButtonDown(0) && !android)
    //    {
    //        print(setting.hostTHisGame);
    //        win.ShowWindow(ShowHostTHisGameWindow, win.act);
    //    }

    //}
    //}
    public void ShowHostTHisGameWindow()
    {
        Setup(600, 400);
        BeginScrollView();
        skin.textArea.wordWrap = true;
        gui.TextArea(setting.hostTHisGame);
        gui.EndScrollView();
    }
    public GUIText medalText;
    public override void Awake()
    {

        _Loader.rain = _Loader.night = false;
        _AutoQuality.enabled = false;
        base.Awake();
    }
    static int medalsChangeSended;
    static bool carChangeSended;
    public void Update()
    {
        if (KeyDebug(KeyCode.T))
            _Loader.SetValue("reputation=" + _Loader.reputation + "&friends=" + _Loader.friendCount + "&medals=" + _Loader.medals + "&gameplays=" + _Loader.playedTimes);
        //if (medalText != null && _Loader.loggedIn)
        //medalText.text = Tr("Medals: ") + _Loader.medals + " Reputation: " + _Loader.reputation;
        //if (!online)
        //gameType = GameType.race;
    }


}