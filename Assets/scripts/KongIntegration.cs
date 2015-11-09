using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
//using LitJson;
using System.Security.Policy;
using System.Text.RegularExpressions;
using LitJson;

using UnityEngine.Flash;
using gui = UnityEngine.GUILayout;
using System.Text;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


public enum Site { N, Kg, FB, VK }
public partial class Integration 
{

    internal Site site;
    public IEnumerator KongParse(string s)
    {
        var q = ParseQueryString(s);
        string name = q.Get("kongregate_username");
        string id = q.Get("kongregate_user_id");
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(id) && id != "0")
        {
            userId = id;
            _Loader.vkPassword = id;
            userName = name;
            Debug.LogWarning(userId);
            Debug.LogWarning(userName);
            if (string.IsNullOrEmpty(_Loader.playerName))
                _Loader.playerName = name;
            site= Site.Kg;
            ExternalEval(@"kongregateAPI.loadAPI();");
            vkLoggedIn = true;
            var url = "http://www.kongregate.com/api/user_info.json?user_id=" + userId;
            Debug.LogWarning(url);
            var w = new WWW(url);
            yield return w;
            if (!string.IsNullOrEmpty(w.error))
                Debug.LogWarning(w.url + w.error);
            else
            {
                var data = JsonMapper.ToObject(w.text);
                _Loader.avatarUrl = data["avatar_url"].ToString();
                Debug.LogWarning("avatar " + _Loader.avatarUrl);
            }
        }
    }
    public static NameValueCollection ParseQueryString(string s)
    {
        NameValueCollection nvc = new NameValueCollection();
        if (s.Contains("?"))
            s = s.Substring(s.IndexOf('?') + 1);
        foreach (string vp in Regex.Split(s, "&"))
        {
            string[] singlePair = Regex.Split(vp, "=");
            if (singlePair.Length == 2)
                nvc.Add(singlePair[0], singlePair[1]);
            else
                nvc.Add(singlePair[0], String.Empty);
        }
        return nvc;
    }
}