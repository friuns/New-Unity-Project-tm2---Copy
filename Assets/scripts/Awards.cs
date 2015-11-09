using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using UnityEngine.SocialPlatforms.Impl;
#if !UNITY_WP8
using CodeStage.AntiCheat;
using CodeStage.AntiCheat.ObscuredTypes;
#endif
using gui = UnityEngine.GUILayout;
#if !UNITY_FLASH || UNITY_EDITOR
using System.Linq;
#endif
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


public class Awards : GuiClasses
{

    public GUISkin loadingBar;
    public Award xp = new Award();
    public Award CompleteAllTracks = new Award();
    public Award Medals = new Award();
    public Award NoFlashbacks = new Award();
    public Award NoCollisions = new Award();
    public Award UnlockAllCars = new Award();
    internal Award Play7Days = new Award();
    public Award ZombieKills = new Award();
    //public Award Reputation;
    public Award WinInMultiplayerRace = new Award();
    public Award CustomLevel = new Award();
    //public Award zombieMode = new Award();
    public Award DeathMatchOrCtf = new Award();
    public Award HardCore = new Award();
    public Award LevelCreator = new Award();
    public Award topDown = new Award();
    public Award warScore = new Award();
    public Award coinsCollected = new Award();
    public Texture2D[] ranks;
    internal List<Award> awards = new List<Award>();
    public void OnEnable()
    {
        //awards = new Award[] { CompleteAllTracks, NoFlashbacks, NoCollisions, UnlockAllCars, Play7Days, ZombieKills, Reputation, WinInMultiplayerRace, CustomLevel, zombieMode, DeathMatchOrCtf };
        awards.Clear();
        var fieldInfos = GetType().GetFields().Where(a => a.FieldType == typeof(Award));
        foreach (var a in fieldInfos)
        {
            var aw = (Award)a.GetValue(this);
            aw.title = a.Name;
            awards.Add(aw);
        }
    }
    public void InitArray()
    {
    }
    internal void UpdateXpOnEndGame()
    {
        var place = _Game.place;
        var firstPlace = place == 1;
        float x = 5;
        var difFactor = (normal ? 2 : hard ? 3 : 1);
        if (_Game.listOfPlayers.Count > 3)
            x += (place == 1 ? 15 : place == 2 ? 10 : place == 3 ? 5 : 2) * difFactor;
        else
            x += 10;
        x += Mathf.Min(50, _Player.coins);
        if (!online)
        {         
            
            if (hard && firstPlace && _Game.listOfPlayers.Count > 1 && _Loader.PlayersCount > 1)
                HardCore.Add();
            if (_Loader.difficulty >= Difficulty.Normal && firstPlace && _Game.listOfPlayers.Count > 1 && _Loader.PlayersCount > 1)
            {
                if (_Game.rewindsUsed == 0)
                {
                    NoFlashbacks.Add();
                    x *= 1.5f;
                }
                if (_Loader.bombCar)
                {
                    NoCollisions.Add();
                    x += 5 * difFactor;
                }
                if (_Game.isCustomLevel)
                    CustomLevel.Add();                
                
            }
        }
        if (_Game.topDownTime > _Game.timeElapsedLevel/2)
        {
            x += 30;
            _Awards.topDown.Add();
        }

        xp.Add((int)x);
        //ShowWindow(WonAwardsWindow);
    }
    //public void WonAwardsWindow()
    //{
    //    Label("Awards won");
    //    win.showBackButton = false;
    //    foreach (var a in awards)
    //        if (a.local > 0)
    //            gui.Button(new GUIContent(a.local + "", a.texture, a.text));
    //    if (Button("Continue"))
    //        win.Back();
    //}
    public void RefreshAwards()
    {
        
        InitArray();
        print("RefreshAwards");
        ResetAwards();
        //this.GetType().GetFields()
        //awards = new List<Award>(new[] { CompleteAllTracks, NoFlashbacks, NoCollisions, UnlockAllCars, Play7Days });
        //for (int i = 0; i < awards.Count; i++)
            //awards[i].id = i;
        //if(setting.disablePlayerPrefs)return;
        //if (CompleteAllTracks.count == 0)
        //{
        int done = 0;
        foreach (Scene a in _Loader.scenes)
        {
            if (!a.userMap)
            {
                _Loader.prefixMapPl = (a.name + ";" + _Loader.playerName + ";");
                if (_Loader.record != float.MaxValue)
                    done++;
            }
        }
        //Debug.LogError(done);
        CompleteAllTracks.count = done;
        CompleteAllTracks.total = _Loader.scenes.Count;
        UnlockAllCars.count = _Loader.CarSkins.Count(a => a.unlocked && !a.hidden);
        UnlockAllCars.total = _Loader.CarSkins.Count(a => !a.hidden);
        Medals.count = _Loader.medals;
        warScore.count = _Loader.warScore;
        //}

    }
    public int damageDeal;
    public void ResetAwards()
    {
        damageDeal = 0;
        foreach (var a in awards)
            a.local = 0;
    }

    //public override void OnEditorGui()
    //{
    //    _Loader.guiSkins.award = skin.button;
    //    SetDirty(_Loader.guiSkins);
    //    base.OnEditorGui();

    //}
    public GUIStyle rankLabel;
    public void DrawAwardsWindow()
    {
        Setup(700, 600);
        //gui.Label(new GUIContent(_Loader.playerName, _Loader.Avatar));
        LabelCenter("Achievements");
        skin.label.alignment = TextAnchor.UpperLeft;
        
        BeginScrollView();
        LabelCenter("To win rewards you must play normal or hard difficulty",16,true);
        gui.BeginHorizontal();
        skin.label.imagePosition = ImagePosition.ImageAbove;
        for (int i = 1; i < ranks.Length-1; i++)
            //if (ranksTotal[i] > 0)
                gui.Label(new GUIContent(ranksTotal[i].ToString(), ranks[i]));
        ranksTotal = new int[ranks.Length];
        gui.EndHorizontal();

        foreach (var a in awards)
        {
            //GUI.enabled = cnt > 0 && cnt >= a.total;
            DrawReward(a);
            //GUILayout.Label("test", loadingBar.window);

        }
        //GUI.enabled = true;
        gui.EndScrollView();
    }
    public void DrawReward(Award a)
    {
        gui.BeginHorizontal();
        var text = a.count + "/" + (int)(a.total > 0 ? a.total : a.upper + 1);
        if (_Game == null)
            gui.Label(new GUIContent(a.texture, Tp(a.title)),gui.ExpandWidth(false));
        if (!string.IsNullOrEmpty(GUI.tooltip))
        {
            var lastRect = GUILayoutUtility.GetLastRect();
            lastRect.y -= 15;
            GUI.Label(lastRect, GUI.tooltip);
            GUI.tooltip = "";
        }
        gui.BeginVertical();
        gui.Label(text + " " + Tr(a.title));
        var lbar = loadingBar.label;
        lbar.alignment = TextAnchor.MiddleCenter;
        
        var rank = GetRank(a);
        var x = 505;
        loadingBar.horizontalSliderThumb.fixedWidth = Mathf.Clamp(a.progress * (x - 10), 35, x - 10);
        GUILayout.HorizontalSlider(0, 0, 1, loadingBar.horizontalSlider, loadingBar.horizontalSliderThumb);
        gui.EndVertical();
        gui.Label(new GUIContent(ranks[rank]), rankLabel, gui.ExpandWidth(false));
        ranksTotal[rank]++;
        gui.EndHorizontal();
        //GUILayout.Label(GuiClasses.Tr("Loading ") + (int)(progress * 100) + "%", loadingBar.label);

        gui.Space(10);
    }
    public int GetRank(Award a)
    {
        a.Calculate();
        return a.total != 0 ? (a.total == a.count ? 4 : 0) : Mathf.Min(a.level, ranks.Length - 2);
    }
    private int[] ranksTotal = new int[20];

}