using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine.Flash;
using gui = UnityEngine.GUILayout;
using System.Text;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;








public partial class Integration
{


#if UNITY_WEBPLAYER
    void Facebook_onLogin(bool resultado, JsonData respuesta)
    {
        if (resultado)
        {
            fbLoggedIn = true;
            print("Facebook_onLogin");
            StartCoroutine(getInfoFriends());
            Facebook.onAppRequest += Facebook_onAppRequest;
            Facebook.onGraphRequest += Facebook_onGraphRequest;
            FacebookGetInfo();
        }
        else
            print("facebook login failed " + respuesta);
    }

    private static void FacebookGetInfo()
    {
        Facebook.graphRequest("/me/?fields=id,name,installed");
    }
    void Facebook_onGraphRequest(bool result, JsonData reply)
    {
        print("Facebook_onGraphRequest " + reply.ToJson());
        if (String.IsNullOrEmpty(userId) && _Loader.loggedIn)
        {
            userName = dict[_Loader.playerName.ToLower()] = reply["name"].ToString();
            site = Site.FB;
            userId = reply["id"].ToString();
            Download(mainSite + "scripts/dict.php", delegate { }, true, "key", "fb" + userId, "value", _Loader.playerNamePrefixed);
            print("App Installed " + reply["installed"]);
        }
    }
    void Facebook_onAppRequest(bool resultado, JsonData respuesta)
    {
        print("Facebook_onAppRequest");
        if (resultado && respuesta["to"] != null)
            pendingFriends += respuesta["to"].Count;
        else
            print("AppRequest failed " + respuesta);
    }
    public IEnumerator getInfoFriends()
    {
        var texto = Facebook.getInfo("/me/friends?fields=id,name,installed");
        var www = new WWW(texto);
        yield return www;
        print(www);
        JsonData friendsinfo = JsonMapper.ToObject(www.text);
        _Loader.friendCount = friendsinfo[0].Count;
        for (int i = 0; i < (int) _Loader.friendCount; i++)
            FbFriendsParse(friendsinfo[0][i]);
    }
    private void FbFriendsParse(JsonData jsonData)
    {
#if old
        if (Boolean.Parse(jsonData["installed"].ToString()))
        {
            print("friend found ");
            Download(mainSite + "/dict/fb" + userId + ".txt", delegate(string s, bool b)
            {
                if (b)
                {
                    print("friend added " + s);
                    AddFriend(s);
                    dict[s] = jsonData["name"].ToString();
                }
            }, false);
            _Loader.friends = _Loader.friends;
            FbFriendsInGame++;
        }
#endif
    }
#else
    private void GetInfo()
    {
    }
    private IEnumerator getInfoFriends()
    {
        yield break;
    }
    public class Facebook
    {
        public static void uiAppRequest(string trackmaniaOnline, string playPopularDRacingMultiplayerGame)
        {
        }
    }
#endif

}