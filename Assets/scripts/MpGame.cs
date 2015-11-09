#define GA

using System.Linq;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Security.Policy;
using System.Threading;
//using Edelweiss.DecalSystem;
//using Vectrosity;
using UnityEngine.Rendering;
#if !UNITY_WP8
using CodeStage.AntiCheat.ObscuredTypes;
#endif
using UnityEngine.SocialPlatforms.Impl;
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using System.Net.Sockets;
using UnityEditor;
using Object = UnityEngine.Object;
//using Exception = System.AccessViolationException;

#endif

public partial class MpGame : GameBase
{

    public List<Player> listOfPlayers { get { return _Game.listOfPlayers; } }
    public Dictionary<int, Player> photonPlayers { get { return _Game.photonPlayers; } }
    public override void Awake()
    {
        _MpGame = this;
        if (!online)
        {
            enabled = false;
            return;
        }
        teams = new Team[] { new Team(TeamEnum.Red), new Team(TeamEnum.Blue) };
        base.Awake();
    }
    public void OnEnable()
    {
        _MpGame = this;
    }
    public override void Start()
    {
        if (!online)
        {
            enabled = false;
            return;
        }   
        if (_Loader.pursuit)
        {
            _Loader.enableCollision = true;
            _Loader.enableMatchTime = true;
        }
        matchTimeLimit = _Loader.matchTimeLimit;
        if (_Loader.team)
            ShowWindowNoBack(SelectTeamWindow);
        
        base.Start();
    }
    public string scoreBoard;

    public void DrawScoreBoard()
    {
        Setup(600, 400);
        BeginScrollView();
        DrawMatchTimeLimit();
        var parsed = new List<ServerScore>();
        foreach (var a in PhotonNetwork.room.customProperties.Cast<KeyValuePair<object, object>>())
        {
            var pp = a.Key.ToString().Split(':');
            if (pp[0] == ServerProps.score2)
                parsed.Add(new ServerScore() { nick = pp[1], value = (float)a.Value });
        }
        parsed = parsed.OrderBy(a => a.value).ToList();
        //skin.label.alignment = TextAnchor.MiddleLeft;
        GUILayoutOption h = gui.Height(24);

        gui.BeginHorizontal();
        {
            gui.BeginVertical();
            Label("Name");
            for (int i = 0; i < parsed.Count; i++)
                gui.Label((i + 1) + " " + parsed[i].nick, h);
            gui.EndVertical();          
        }
        {
            gui.BeginVertical();
            Label("Score");
            foreach (var a in parsed)
                gui.Label(TimeToStr(a.value), h);
            gui.EndVertical();
        }
        gui.EndHorizontal();
        gui.EndScrollView();
    }
    private void DrawMatchTimeLimit()
    {
        if (matchTimeLimit > 0)
            LabelCenter(Trs("Match ends in ") + TimeToStr(matchTimeLimit));
    }


    public virtual void Update()
    {
        if (matchTimeLimit != 0)
            matchTimeLimit -= _Loader.deltaTime;
        if (matchTimeLimit < 0)
        {
            _Game.gameState = GameState.finnish;
            if (win.act == null)
                _Game.ShowScoreBoard();
            return;
        }


        if (input.GetKeyDown(KeyCode.B) && _Loader.dmOnly)
            win.ShowWindow(AllyWindow);


#if VOICECHAT
        if (Input.GetKeyDown(KeyCode.Y))
        {
            //_GameGui.CallRPC(_GameGui.Chat, _Loader.playerName + Tr(" voice chat"));
            print("Voice Chat: " + Application.HasUserAuthorization(UserAuthorization.Microphone));
            if (!VoiceChatRecorder.Instance.enabled)
                StartCoroutine(InitMicrophone());
        }
#endif

        if (_Loader.enableMatchTime && isMaster)
        {
            //if ((timeCountMatch < 0 || KeyDebug(KeyCode.F5, "EndMatch")) && !_Game.finnish)
            //{
            //    CallRPC(EndMatch);
            //}
            GameState gameState = timeCountMatch < 0 ? GameState.finnish : timeCountMatch > _Loader.matchTime ? GameState.none : GameState.started;
            if (gameState != _Game.gameState )
                CallRPC(SetGameState, (int)gameState);
            if (KeyDebug(KeyCode.F4, "set to 30 seconds"))
                CallRPC(SetTimeCount, 10f);
        }
        if (isMaster && _Loader.pursuit)
        {
            if (_Game.started && Time.time - stateChangeTime > 5 && _Game.listOfPlayers.Count > 1 && (redTeam.players.All(a => a.dead) || !blueTeam.players.Any()))
                CallRPC(SetGameState, (int)GameState.finnish);
        }
        if (_Loader.race)
        {
            if (!_Game.startedOrFinnished)
            {
                bool started1 = timeCountRace < 0 || _Loader.dmRace;                
                if (started1 != _Game.started && started1)
                    _Game.SetStartTrue();

                if (!_Game.started)
                {
                    _Game.centerText2.enabled = true;
                    _Game.centerText2.text = ((int)timeCountRace + 1) + "";
                    if (startTimeCnt != (int)timeCountRace)
                        PlayOneShotGui(res.bip, .1f);
                    startTimeCnt = (int)timeCountRace;
                }
                else
                    _Game.centerText2.enabled = false;
            }
        }
        if (_Loader.teamOrPursuit)
            foreach (var a in teams)
                a.Update();



        listOfPlayers.Remove(null);
        StringBuilder sb = new StringBuilder();
        foreach (var a in listOfPlayers)
        {
            if (Time.realtimeSinceStartup - a.replay.voiceChatTime < .3f)
                sb.Append(a.replay.playerNameClan).Append(a.replay.ignored ? Tr("(muted)") : Tr("(voice chat)")).AppendLine();
        }
        if (sb.Length > 0)
            _Game.voiceChatText.text = sb.ToString();
        else
            _Game.voiceChatText.text = string.Empty;

        timeCountMatch -= _Loader.deltaTime;
        timeCountRace -= _Loader.deltaTime;

    }
    public float stateChangeTime;
    [RPC]
    public void SetGameState(int state)
    {
        stateChangeTime = Time.time;
        _Game.gameState = (GameState)state;
        print("SetGameState " + _Game.gameState);
        if (_Game.gameState == GameState.started)
        {

            //_Player.Reset();
            if (_Loader.pursuit)
            {
                int i = 0;
                if (isMaster)
                    foreach (var a in listOfPlayers.OrderBy(a => Random.value))
                    {
                        bool cop = i < listOfPlayers.Count / 3 || i == 0;
                        a.CallRPC(a.SetTeam, (int)(cop ? TeamEnum.Blue : TeamEnum.Red));
                        a.CallRPC(a.Reset);
                        if (cop)
                            a.CallRPC(a.SetWasCop, a.wasCop +1);
                        i ++;
                    }
            }
            if (win.act == _Game.scoreBoardWindow)
                win.CloseWindow();
        }
        if (_Game.gameState == GameState.finnish)
        {
            if (isMaster && redTeam.count > 0 && blueTeam.count > 0)
            {
                Team t = redTeam.players.All(a => a.dead) ? blueTeam : redTeam;
                CallRPC(SetTeamScore, (int) t.team, t.score + 1);
                //foreach (var a in redTeam.players.Where(a => !a.dead))
                //    a.CallRPC(a.SetScore2, a.score + 50);
            }
            
            //if (redTeam.players.All(a => a.dead))
            //    winMessage = "Cops Win";
            //else
            //    winMessage = "Bandits Win";
            //if (!_Player.replay.cop && !_Player.dead)
            //    _Player.SetScore2(_Player.score + 50);

            //ShowWindow(PlayersWindow);
            _Game.ShowScoreBoard();
            SetTimeCount(_Loader.matchTime + (isDebug ? 6 : 10));
        }
    }
    public float matchTimeLimit;
    ////private string winMessage;

    public override void OnPlConnected()
    {
        
        CallRPC(SetTimeCount, timeCountMatch);
        CallRPC(SetTimeCountRace, timeCountRace);
        if (_Loader.teamOrPursuit)
        {
            CallRPC(SetTeamScore, 0, teams[0].score);
            CallRPC(SetTeamScore, 1, teams[1].score);
        }
        CallRPC(SetGameState, (int)_Game.gameState);
        CallRPC(TimeLimit, matchTimeLimit);
        //if (_Loader.dm)
        //    CallRPC(SetLevelTime, timeElapsedLevel);
    }
    [RPC]
    public void TimeLimit(float f)
    {
        matchTimeLimit = f;
    }

    [RPC]
    public void SetTimeCountRace(float time, PhotonMessageInfo m)
    {
        print("SetTimeCount");
        var ping = m == null ? 0 : Mathf.Abs((float)(PhotonNetwork.time - m.timestamp));
        print("Set Start Time " + time);
        timeCountRace= time - ping;
    }

    [RPC]
    public void SetTimeCount(float time)
    {
        print("Set Start Time " + time);
        timeCountMatch = time ;
    }
    
#if VOICECHAT
    private IEnumerator InitMicrophone()
    {
        if (_Loader.dm && !Application.HasUserAuthorization(UserAuthorization.Microphone))
            ShowWindowNoBack(_Game.MenuWindow);
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

        print("Microphones " + Microphone.devices.Length);
        if (Microphone.devices.Length > 0)
        {
            VoiceChatRecorder v = VoiceChatRecorder.Instance;
            v.enabled = true;
            v.NetworkId = PhotonNetwork.player.ID;
            v.Device = v.AvailableDevices[0];
            v.StartRecording();
            v.NewSample -= VoiceChat;
            v.NewSample += VoiceChat;
        }



        yield return null;
    }
#endif
    

#if VOICECHAT
    private void VoiceChat(VoiceChatPacket obj)
    {
        _Player.replay.voiceChatTime = Time.realtimeSinceStartup;
        if (!_Loader.banned)
            _Player.photonView.RPC("SendAudio", PhotonTargets.Others, obj.Data, obj.Length, PhotonNetwork.player.ID);
    }
#endif

    public Team redTeam { get { return teams[(int)TeamEnum.Red]; } }
    public Team blueTeam { get { return teams[(int)TeamEnum.Blue]; } }
    public Team[] teams;

    

    internal float timeCountMatch;
    internal float timeCountRace;

    private int startTimeCnt;

    public HashSet<int> allies = new HashSet<int>();
    public HashSet<int> allyVisible = new HashSet<int>();
    public void AllyWindow()
    {
        Setup(400, 700);
        Label("If you ally with player, player will see where you are");
        foreach (Player a in listOfPlayers.Where(a => a != _Player))
        {
            var contains = allies.Contains(a.playerId);
            var ally = gui.Toggle(contains, Tr("ally with ") + a.playerNameClan);
            if (ally != contains && (!ally || allies.Count < 4))
            {

                CallRPC(SetAlly, myId, a.playerId, ally);
                if (!ally && allyVisible.Contains(a.playerId))
                    CallRPC(SetAlly, a.playerId, myId, false);
            }
        }
        if (BackButton())
            win.CloseWindow();
    }
    [RPC]
    public void SetAlly(int from, int to, bool b)
    {
        if (from == myId)
            if (b)
                allies.Add(to);
            else
                allies.Remove(to);
        var photonPlayer = _Game.photonPlayers[@from];
        if (to == myId)
        {
            if (b)
            {
                allyVisible.Add(from);
                _Game.centerText(string.Format(Tr("To ally with {0} press b"), photonPlayer.playerNameClan), 4);
                //if (!PhotonNetwork.isMasterClient)
                photonPlayer.replay.ally = true;
                photonPlayer.RefreshText();
            }
            else
            {
                allyVisible.Remove(from);
                //if (!PhotonNetwork.isMasterClient)
                photonPlayer.replay.ally = false;
                photonPlayer.RefreshText();
            }

        }
        _GameGui.Chat(photonPlayer.replay.playerName + Tr(b ? " Allied with " : " UnAllied with ") + _Game.photonPlayers[to].replay.playerName);
    }

    public void MenuWindow()
    {

        if (_Loader.team && Button("Choose Team"))
        {
            //if (Time.time - selectTeamTime > 30)
            ShowWindowNoBack(_MpGame.SelectTeamWindow);
            //else
            //Popup("You cant change team too fast");
        }

        if (_Loader.dmOnly && !_Loader.team && Button("Ally"))
        {
            ShowWindowNoBack(AllyWindow);
        }
        if ((isMod || isDebug) && Button("Players"))
            if (isMod && !isDebug)
                ShowWindow(BanWindow, win.act);
            else
                ShowWindow(MuteWindow, win.act);
    }
}