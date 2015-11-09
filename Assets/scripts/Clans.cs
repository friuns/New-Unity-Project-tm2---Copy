//using System;
//using System.Collections;
//using System.Collections.Generic;
////using System.Linq;
//using LitJson;
//using UnityEngine;
//using Object = UnityEngine.Object;
//using Random = UnityEngine.Random;
//using gui = UnityEngine.GUILayout;


//public class Clans : GuiClasses
//{
//    public void SearchClan()
//    {
//        Label("Enter Clan Name:");
//        clanName = gui.TextField(clanName, 6);
//        if (ButtonLeft("Search"))
//        {
//            Popup("getting clan info");
//            Download("scripts/clan.php", delegate(string text, bool success)
//            {
//                if (!success) Popup("error", _Loader.MenuWindow);
//                else if (text == "notFound")
//                    ShowWindow(ClanNotFoundWindow);
//                else
//                {
//                    clanMembers = SplitString(text);
//                    ShowWindow(DrawClan);
//                }
//            }, true, "clanName", clanName);
//        }
//    }
//    private void ClanNotFoundWindow()
//    {
//        Label("Clan not found, do you want to create it?");
//        gui.BeginHorizontal();
//        if (Button("Yes"))
//        {
//            ShowWindow(CreateClan);
//            CreateClan();
//        }
//        if (Button("No"))
//            ShowWindow(_Loader.MenuWindow);
//    }
//    internal string clan { get { return PlayerPrefsGetString("clan"); } set { PlayerPrefsSetString("clan", value); } }
//    private void CreateClan()
//    {
//        Label("Enter new clan Password:");
//        clanPassowrd = gui.TextField(clanPassowrd, 20);
//        Download("scripts/clan.php", delegate(string text, bool b)
//        {            
//            var ss = SplitString(text);
//            if (ss[0] == "success")
//            {
//                clan = clanName;
//                ShowWindow(DrawClan);
//            }

//        }, true, "createClan", clanPassowrd);
//    }
//    private void DrawClan()
//    {
//        Label("Clan: " + clan);
//        if (Button("Join Clan"))
//        {
//            ShowWindow(JoinClan);
//        }
//        gui.BeginVertical();
//        foreach (string a in clanMembers)
//        {
//            gui.BeginHorizontal();
//            var na = a.Split(':');
//            gui.Label(res.GetAvatar(int.Parse(na[1])));
//            Label(na[0]);
//            gui.EndHorizontal();
//        }
//        gui.EndVertical();
//    }
//    private string[] clanMembers;

    

//    private void JoinClan()
//    {
//        Label("Clan Password:");
//        clanPassowrd = gui.TextField(clanPassowrd);
//        if (Button("Join"))
//        {
//            Download("scripts/clan.php", delegate(string t, bool b)
//            {
//                if (t != "success")
//                {
//                    Popup(b ? "wrong Password" : "failed", _Loader.MenuWindow);
//                }
//            }, true, "joinClan", clanPassowrd);
//        }
//    }
//    private string clanPassowrd;
//    private string clanName;
//}