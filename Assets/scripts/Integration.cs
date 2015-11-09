using System;
using System.Collections.Generic;
using System.IO;
//using LitJson;
using System.Security.Policy;
using LitJson;

using UnityEngine.Flash;
using gui = UnityEngine.GUILayout;
using System.Text;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public partial class Integration : GuiClasses
{
    internal bool fbLoggedIn;
    public bool loggedIn { get { return fbLoggedIn || vkLoggedIn; } }
    internal string userId;
    
    public int FbFriendsInGame;
    internal int pendingFriends;
    internal string userName = "";

    
    public override void Awake()
    {
        if (!Application.isEditor)
            vkLoggedIn = false;
        base.Awake();
    }
    public void Start()
    {
        print("Start Integration");
#if UNITY_WEBPLAYER


        var unity = Application.absoluteURL.Contains("kongregate") ? "kongregateUnitySupport.getUnityObject()" : "u.getUnity()";
        string s = "typeof GetUrl==='undefined'?document.location.toString(): GetUrl()";
        ExternalEval(unity + ".SendMessage('!Loader', 'Url'," + s + ");");
        ExternalEval(unity + ".SendMessage('Integration', 'UrlOdnoklasniki'," + s + ");");
        if (setting.testVK)
            _Loader.Url("http://server.critical-missions.com/tm/web2/kong.html?DO_NOT_SHARE_THIS_LINK=1&kongregate_username=SoulKey&kongregate_user_id=12399663&kongregate_game_auth_token=e27a256515c68fdd165b6520a8f3d3b1d837a1618337fe2fde9638d914ac43a4&kongregate_game_id=182469&kongregate_host=http%3A%2F%2Fwww.kongregate.com&kongregate_game_url=http%3A%2F%2Fwww.kongregate.com%2Fgames%2FSoulKey%2Ftr-online&kongregate_api_host=http%3A%2F%2Fapi.kongregate.com&kongregate_channel_id=1390039898416&kongregate_api_path=http%3A%2F%2Fchat.kongregate.com%2Fflash%2FAPI_AS3_efeff9e3f4f6350255b95aa31ba0ef32.swf&kongregate_ansible_path=chat.kongregate.com%2Fflash%2Fansible_5cae4aad0aa719bffe641383dd1d3178.swf&kongregate_preview=true&kongregate_language=en&preview=true&kongregate_split_treatments=none&kongregate=true&KEEP_THIS_DATA_PRIVATE=1");
        Facebook.onLogin += Facebook_onLogin;
#endif
        //if (setting.vk) return;        

        StartVk();
        FacebookLogin();
        StartCoroutine( StartLoadNews());
    }

    private IEnumerator StartLoadNews()
    {
        if (Application.isWebPlayer)
            yield return new WaitForSeconds(1);
        
        var w = new WWW("https://graph.facebook.com/trackracingonline/feed?access_token=261172830687941|d5mN7DtR-LQoNlBW9JFBL7gM_h4");
        yield return w;
        if (posts.Count >0 && !isDebug)
            yield break;
        posts.Clear();
        print(w.url);
        IEnumerable d = JsonMapper.ToObject(w.text)["data"];
        foreach (JsonData a in d)
        {
            var type = a["type"].ToString();
            if (a["from"]["name"].ToString() == "Trackracing Online")
            if (type == "video" || type == "status" || type == "link")
            {
                JsonData jsonData = a["desctiption"] ?? a["message"] ?? a["name"];
                if (jsonData != null)
                {
                    var post = new Posts();
                    posts.Add(post);
                    //post.title = (a["name"]) + "";
                    post.id = a["id"].ToString().Split('_')[1];
                    post.imageUrl = Uri.UnescapeDataString(a["picture"] + "");
                    //print(post.imageUrl);
                    post.date = DateTime.Parse(a["created_time"].ToString());
                    post.msg = jsonData + "";
                    if (a["comments"] != null)
                        post.comments = a["comments"]["data"].Count;
                    StartCoroutine(post.Load());
                }
            }

        }
        
    }
    
    public void OnLoggedIn()
    {
        if (_Loader.isOdnoklasniki)
            PlayerPrefs.SetString(_Loader.vkPassword, _Loader.playerName);
        ExternalCall("VkFriends");
        //if (isDebug)
            //VkFriends("68269033,76593928,100247077,113309694,136198806,179393664,182744378,189810914,193428338,201013801,209440834,219149208,219149234");

        //print("OnLoggedIn");
#if UNITY_WEBPLAYER
        if (fbLoggedIn)
            FacebookGetInfo();
#endif
        //_Loader.StartCoroutine(bs.ExternalCall("GetInfo"));
        

    }
    public bool facebookLinkPressed { get { return PlayerPrefsGetBool(_Loader.playerName + "facebookLink"); } set { PlayerPrefsSetBool(_Loader.playerName + "facebookLink", value); } }
    public void DrawButtons()
    {

        if (platformPrefix != "flash")
        {
            if (_Integration.fbLoggedIn)
            {
                if (Button("Invite Friends (unlock new car)", res.faceBook))
                {
                    Screen.fullScreen = false;
                    win.ShowWindow(delegate
                    {
                        if (ButtonLeft("Refresh"))
                            StartCoroutine(_Integration.getInfoFriends());
                        Label(String.Format("Invitations sent :{1}\r\nFriends Added    :{0}\n\n Add more friends to unlock new cars!", _Integration.FbFriendsInGame, _Integration.pendingFriends), 16, true);
                    }, _Loader.MenuWindow);
                    LogEvent("FacebookInviteFriends");
                    Facebook.uiAppRequest("TrackMania Online", "Play Popular 3D Racing multiplayer game!");
                }
            }
        }
        if (platformPrefix == "flash")
        {
            //if (!_Loader.Beginner)
                if (Button("Play Full Version on Facebook" + (facebookLinkPressed ? "" : "\n and unlock new car!"), res.faceBook))
                {                    
                    LogEvent("Facebook");
                    win.ShowWindow(GoToFbLink);
                    if (alternativeOpen)
                    {
                        ActionScript.Import("flash.net.URLLoader");
                        ActionScript.Import("flash.net.URLRequest");
                        ActionScript.Import("flash.net.URLRequestMethod");
                        ActionScript.Import("flash.net.URLLoaderDataFormat");
                        ActionScript.Import("flash.net.URLVariables");
                        ActionScript.Import("flash.net.navigateToURL");
                        ActionScript.Statement(@"var url:URLRequest = new URLRequest(""{0}""); navigateToURL(url);", facebookLink);//, ""_blank""
                    }
                }
        }
    }
    string facebookLink = "http://apps.facebook.com/TrackRacing";
    public bool alternativeOpen;
    private void GoToFbLink()
    {
        Setup(700, 400);
        LabelCenter("Latest version found at", 16, true);
        if (Button(Trs(facebookLink))) Application.OpenURL(facebookLink);
        GUILayout.FlexibleSpace();
        if (BackButtonLeft())
        {
            if (!facebookLinkPressed)
            {
                facebookLinkPressed = true;
                _Loader.wonMedals = 10;
                _Loader.medals += _Loader.wonMedals;
            }
            _Loader.WindowPool();
            //ShowWindow(_Loader.MenuWindow);
        }
    }
    public void FacebookLogin()
    {
#if UNITY_WEBPLAYER
        Facebook.login("user_about_me,publish_actions,publish_stream");
#endif
    }
    internal bool feedPosted;
    public void wallPost(float FinnishTime)
    {

        print("<<<<<<<<<<<<<<<<<<<<<postFeed>>>>>>>>>>>>>>>");
#if UNITY_WEBPLAYER
        var message = String.Format("Click to play vs {2}'s replay!\r\nTrack:{0} Time:{1}", _Loader.mapName, TimeToStr(FinnishTime), _Integration.userName);
        Screen.fullScreen = false;
        feedPosted = true;
        if (fbLoggedIn)
            Facebook.postFeed(message, "http://206.190.128.180/tm/docs/150.jpg", _Loader.replayLinkPrefix + _Loader.replayLink);
        else
            ExternalCall("PostWall2", _Loader.replayLink, _Loader.mapName, TimeToStr(FinnishTime));
#endif
    }
}