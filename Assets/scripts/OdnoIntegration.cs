
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using UnityEngine;

public partial class Integration
{

    public void UrlOdnoklasniki(string s)
    {
        print("UrlOdnoklasniki " + s);
        var url = s;
        if (_LoaderScene != null && _Loader.vk)
        {
            if (url.Contains("?") && _Loader.isOdnoklasniki)
            {
                var queryParameters = new Dictionary<string, string>();
                string[] querySegments = url.Split('?')[1].Split('&');
                foreach (string segment in querySegments)
                {
                    string[] parts = segment.Split('=');
                    if (parts.Length > 0)
                    {
                        string key = parts[0].Trim(new char[] { '?', ' ' });
                        string val = parts[1].Trim();

                        queryParameters.Add(key, val);
                    }
                }
                _Loader.vkPassword = queryParameters["logged_user_id"];
                print("odno id " + _Loader.password);
                _Loader.playerName = PlayerPrefs.GetString(_Loader.vkPassword);
                ShowWindowNoBack(_Loader.LoginWindow);
            }
            else
                _Loader.WindowPool();

            //if (_LoaderScene.hostThisGame != null)
            //Destroy(_LoaderScene.hostThisGame.gameObject);
            Destroy(_LoaderScene.support);
        }
    }
}