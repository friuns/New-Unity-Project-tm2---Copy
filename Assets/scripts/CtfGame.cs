#define GA

using System.Linq;
//using Edelweiss.DecalSystem;
//using Vectrosity;
#if !UNITY_WP8
#endif
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR

//using Exception = System.AccessViolationException;

#endif
public partial class MpGame
{
    
    
    
    public void UpdateOnline()
    {

        
    }

    
    public void MuteWindow()
    {
        Setup(600, 600);
        BeginScrollView();
        int i = 0;
        foreach (Player a in listOfPlayers)
        //if ((a != _Player||isDebug))
        {
            i++;
            gui.BeginHorizontal();
            gui.Label(new GUIContent(a.replay.getText(false), a.avatar));
            if (gui.Button("Mute"))
            {
                a.replay.ignored = true;
                if (Time.time - _Loader.lastVote > 60 * 3 && !a.ignoreVoted)
                {
                    a.ignoreVoted = true;
                    _Loader.lastVote = Time.time;
                    a.CallRPC(a.VoteMute);
                }
                win.CloseWindow();
            }
            GUI.enabled = true;
            gui.EndHorizontal();
        }
        gui.EndScrollView();
    }
    public void BanWindow()
    {
        Setup(600, 600);
        BeginScrollView();

        foreach (var a in listOfPlayers)
        //if ((a != _Player||isDebug))
        {
            gui.BeginHorizontal();

            gui.Button(a.replay.playerName);
            if (gui.Button("unban"))
                a.CallRPC(a.Mute, 0);
            if (gui.Button("10 min"))
                a.CallRPC(a.Mute, 10);
            if (gui.Button("12 h"))
                a.CallRPC(a.Mute, 12 * 60);
            gui.EndHorizontal();
        }
        gui.EndScrollView();
    }

    
    //private int[] teamScore = new int[2];


    private Team winTeam;
    [RPC]
    internal void SetTeamScore(int id, int score)
    {
        Team team = teams[id];
        winTeam = team;
        team.score = score;
    }
    public List<Flag> flags = new List<Flag>();
    [RPC]
    internal void FlagCaptured(int plid)
    {

        foreach (var a in flags)
            a.ResetPos(false);
        foreach (var a in _Game.listOfPlayers)
            a.resetTime = Time.time;
        print("FlagCaptured" + plid);
        var pl = photonPlayers[plid];
        CallRPC(SetTeamScore, (int)pl.teamEnum, pl.team.score + 1);
        ShowWindowNoBack(delegate
        {
            Setup(500, 300);
            var format = string.Format(Tr("{0} brought enemy flag home, {1} wins!"), pl.playerNameClan, Tr(pl.sameTeam ? "our team" : "enemy team"));
            skin.label.wordWrap = true;
            gui.Label(new GUIContent(format, pl.avatar));
            if (gui.Button(Tr("Continue"), gui.ExpandHeight(true)))
                win.CloseWindow();
        });
        _Player.Reset();

        PlayOneShotGui(pl.sameTeam ? res.youWin : res.youLoose);
        if (pl.sameTeam)
            PlayOneShotGui(res.youWin2);
        PlayOneShotGui(res.endGame);
        //started = false;
        //StartCoroutine(CountTo3());
    }
    //public bool delayed;

    //[RPC]
    //public void SetSpeedLimit(float limit)
    //{
    //    _Loader.speedLimitEnabled = true;
    //    _Loader.speedLimit = limit;
    //}

    public void SelectTeamWindow()
    {
        Setup(500, 300);

        LabelCenter("Select your team");
        gui.BeginHorizontal();


        if (gui.Button(Tr("Blue Team (") + blueTeam.players.Count(a => teamSElected || a != _Player) + ")", gui.Height(100)) || setting.autoConnect)
            SelectTeam(TeamEnum.Blue);
        if (gui.Button(Tr("Red Team (") + redTeam.players.Count(a => teamSElected || a != _Player) + ")", gui.Height(100)) || setting.autoHost)
            SelectTeam(TeamEnum.Red);

        gui.EndHorizontal();
    }



    private bool teamSElected;

    //private float selectTeamTime;
    public void SelectTeam(TeamEnum teamEnum)
    {
        teamSElected = true;
        win.CloseWindow();
        if (_Player.replay.teamEnum != teamEnum)
        {
            //selectTeamTime = Time.time;
            _Player.replay.teamEnum = teamEnum;
            _Player.CallRPC(_Player.SetTeam, (int)teamEnum);
            _Player.Reset();
        }
    }
}