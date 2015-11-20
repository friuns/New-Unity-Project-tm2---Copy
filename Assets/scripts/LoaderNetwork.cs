#define GA

using System.Globalization;
using System.Text.RegularExpressions;
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
    private string roomName;
    public float lifeDef = 300;
    bool havePassword;
    string gamePassword = "";

    internal WeaponEnum weaponEnum = (WeaponEnum)~0;
    public void EnableWeaponWindow()
    {
        var wep = Enum.GetValues(typeof(WeaponEnum)).Cast<WeaponEnum>().ToArray();
        for (int i = 0; i < wep.Length; i++)
            weaponEnum = (WeaponEnum)SetFlag((int)weaponEnum, (int)wep[i], Toggle(GetFlag((int)weaponEnum, (int)wep[i]), wep[i].ToString()));
    }
    private int playerCountGui = 20;
    
    public void HostGameWindow()
    {
        Setup(600, 500);
        var autoHost = setting.autoHost;
        BeginScrollView();
        Label("Game Name:");
        if (roomName == null)
            roomName = playerNamePrefixed + "'s ";
        roomName = gui.TextField(roomName, 20);

        Label("Game Type:");
        skin.button.wordWrap = true;
        skin.button.fixedHeight = 50;
        GameType toolbar = (GameType)Toolbar((int)_Loader.gameType, new[] { "Race", "DeathMatch", null, "Capture The Flag", null, "DeathRace", beta ? "Pursuit" : null }, true);
        if (_Loader.gameType != toolbar && !autoHost)
        {
            _Loader.gameType = toolbar;
            ResetServerSettings();
        }
        skin.button.fixedHeight = 0;
        if (Button("Start") || autoHost)
            StartHost();
        GUILayout.BeginHorizontal();

        //gameType = Toggle(team || dm, "Battle") || setting.autoHost ? GameType.dm : GameType.race;
        rain = Toggle(rain, "Rain");
        night = Toggle(night, "Night");
        bool toggle = Toggle(havePassword, "Password");
        if (toggle != havePassword)
            ResetServerSettings();
        havePassword = toggle;
        if (havePassword)
            gamePassword = gui.TextField(gamePassword, 20);
        //topdown = Toggle(topdown, "topdown");
        GUILayout.EndHorizontal();
        _Loader.rotationFactor = HorizontalSlider("Тurnability ", _Loader.rotationFactor, .2f, 1);
        _Loader.airFactor = HorizontalSlider("Density of air", _Loader.airFactor, 0, 1);
        _Loader.nitro = HorizontalSlider("Nitro", nitro, 0, 10);
        if (havePassword)
            matchTimeLimit = HorizontalSlider("Match Time Limit", matchTimeLimit / 60, 0, 10) * 60;

        if ((_Loader.dmOrPursuit || _Loader.dmRace) && !curScene.userMap)
            _Loader.enableZombies = Toggle(_Loader.enableZombies, "Zombies");

        if (_Loader.race)
        {
            if (_Loader.medals > 10)
                bombCar = Toggle(bombCar, "BombCar");
            rewinds = (int)HorizontalSlider("Rewinds:", rewinds, 0, 60);
            if (!_Loader.dmNoRace)
                waitTime = (int)HorizontalSlider("wait time players on start:", waitTime, 3, 10);
        }

        if (_Loader.dm)
        {

            if (_Loader.ctf)
                ctfRandomSpawn = Toggle(ctfRandomSpawn, "Random Spawn");
            dmShootForward = Toggle(dmShootForward, "Shoot Forward Only");
            if (_Loader.dmNoRace)
            {
                //if (_Loader.dmOnly)
                //    enableCollision = Toggle(enableCollision, "EnableCollision");

                //speedLimitEnabled = Toggle(speedLimitEnabled, "Car speed");
                //if (speedLimitEnabled)
                //{
                //    Label("Speed Limit:" + speedLimit);
                //    speedLimit = gui.HorizontalSlider(speedLimit, 50, 1000);
                //}
                if (Button("Select weapons"))
                    ShowWindow(EnableWeaponWindow, win.act);
            }
        }
        if (_Loader.dmOrPursuit)
        {
            _Loader.matchTime = HorizontalSlider("Match Time", _Loader.matchTime / 60, 0, 10) * 60;
            lifeDef = HorizontalSlider("Life:", lifeDef, 100, 500);
        }

        gui.BeginHorizontal();
        Label(Trs("Player count:") + playerCountGui);
        playerCountGui = (int)gui.HorizontalSlider(playerCountGui, 2, havePassword ? 100 : 20);
        gui.EndHorizontal();

        //_Loader.difficulty = (Difficulty)Toolbar((int)_Loader.difficulty, Enum.GetNames(typeof(Difficulty)), true);

        gui.EndScrollView();
    }
    private void StartHost()
    {
        Popup("hosting");
        var ht = new ExitGames.Client.Photon.Hashtable();
        ht.Add(props.mapname.ToString(), mapName);
        ht.Add(props.difficulty.ToString(), Difficulty.Normal);
        ht.Add(props.rain.ToString(), _Loader.rain);
        ht.Add(props.night.ToString(), _Loader.night);
        ht.Add(props.topdown.ToString(), false);
        ht.Add(props.rewinds.ToString(), rewinds);
        ht.Add(props.wait.ToString(), waitTime);
        ht.Add(props.dm.ToString(), _Loader.dm);
        ht.Add(props.version.ToString(), setting.multiplayerVersion);
        ht.Add(props.life.ToString(), lifeDef);
        ht.Add(props.wallCollision.ToString(), true);
        ht.Add(props.gametype.ToString(), (int)_Loader.gameType);
        ht.Add(props.bombCar.ToString(), bombCar);
        ht.Add(props.weapons.ToString(), (int)weaponEnum);
        ht.Add(props.mapId.ToString(), curScene.mapId);
        ht.Add(props.dmLockForward.ToString(), dmShootForward);
        ht.Add(props.zombies.ToString(), _Loader.enableZombies);
        ht.Add(props.enableCollision.ToString(), enableCollision);
        ht.Add(props.enableMatchTimeLimit.ToString(), enableMatchTime);
        ht.Add(props.randomSpawn.ToString(), _Loader.ctfRandomSpawn);
        ht.Add(props.airResistence.ToString(), _Loader.airFactor);
        ht.Add(props.nitro.ToString(), _Loader.nitro);
        ht.Add(props.rotationFactor.ToString(), _Loader.rotationFactor);
        //if (_Loader.speedLimitEnabled)
        //ht.Add(props.speedLimit, speedLimit);
        if (havePassword)
            ht.Add(props.havePassword.ToString(), gamePassword);
        PhotonNetwork.CreateRoom(roomName, true, true, havePassword ? 100 : playerCountGui, ht, Enum.GetNames(typeof(props)));
    }
    internal int waitTime = 3;
    internal int rewinds = 3;
    //public void OnCreatedRoom()
    //{
    //    if (!online) return;
    //    StartCoroutine(StartLoadLevel(mapName, true));
    //}

    private string tableFormat;
    public void QuickConnectWindow()
    {
        Setup(400, 500);
        sGameType = SGameType.Multiplayer;
        if (PhotonNetwork.connectionStateDetailed != PeerState.JoinedLobby)
        {
            if (!online) Label("offline error");
            Label("" + PhotonNetwork.connectionState);
            if (isDebug && Button("disc")) PhotonNetwork.Disconnect();
            if (isDebug && Button("connect")) ConnectToPhoton();
            return;
        }
        gui.Label("Players: " + PhotonNetwork.countOfPlayers + " Games:" + PhotonNetwork.countOfRooms + " Ping:" + PhotonNetwork.GetPing() + (isDebug ? " Server:" + appId : ""));

        gui.BeginVertical(skin.box);
        if (_Loader.beta)
            JoinButton2(GameType.pursuit, new GUIContent(Tr("Pursuit")));
        JoinButton2(GameType.race, new GUIContent(Tr("Race"), res.race));
        if (_Loader.resLoaded2)
            JoinButton2(GameType.race, new GUIContent(Tr("User Map"), res.userMap), true);
        JoinButton2(GameType.dm, new GUIContent(Tr("DeathMatch"), res.deathMatch));
        JoinButton2(GameType.ctf, new GUIContent(Tr("Capture The Flag"), res.captureFlag));

        //else
        //    JoinButton2(GameType.dmRace, new GUIContent(Tr("Death Race"), res.deathMatch));


        gui.EndVertical();
        gui.BeginHorizontal();
        if (isDebug && Button("") || setting.autoConnect)
            ShowWindow(MultiplayerWindow);
        if (Button("Server List"))
            ShowWindow(MultiplayerWindow2);
        if (Button("Host Game") || setting.autoHost)
            ShowWindowNoBack(SelectMapWindow);

        gui.EndHorizontal();
    }

    private void JoinButton2(GameType GameType, GUIContent GuiContent, bool CustomMap = false)
    {

        RoomInfo r = FindRoom(GameType, CustomMap);
        GUI.enabled = r != null;
        if (gui.Button(GuiContent))
            JoinButton(r, true);
        GUI.enabled = true;
    }

    private RoomInfo FindRoom(GameType GameType, bool customMap = false)
    {

        var rooms = roomInfos.Where(a =>
            a.maxPlayers - 2 > a.playerCount &&
            string.IsNullOrEmpty(CustomProperty(a, props.havePassword, "")) && CustomProperty(a, props.version, 0) <= setting.multiplayerVersion);

        if (!_Loader.resLoaded2)
            rooms = rooms.Where(a => CustomProperty(a, props.mapname, "").Length == 3);
        if (customMap)
            rooms = rooms.Where(a => CustomProperty(a, props.mapname, "").Length > 3);
        else
            rooms = rooms.Where(a => CustomProperty(a, props.gametype, GameType.race) == GameType);

        var rooms2 = rooms.Where(a => CustomProperty(a, props.version, 0) == setting.multiplayerVersion);
        if (rooms2.Any())
            rooms = rooms2;
        return rooms.OrderBy(a => Random.value).FirstOrDefault();
    }
    private RoomInfo[] roomInfos { get { return PhotonNetwork.GetRoomList(); } }
    protected void SelectServer() { }
    protected void ServerList() { }

    public void MultiplayerWindow2()
    {
        Setup(800, 500);
        if (mpTitlte != "")
            gui.Label(mpTitlte);
        if (Button("Host Game", false) || setting.autoHost)
            ShowWindowNoBack(SelectMapWindow);
        BeginScrollView();
        gui.BeginHorizontal("server list", win.editorSkin.window);
        var ar = roomInfos.OrderByDescending(a => CustomProperty(a, props.version, 0)).ThenByDescending(a => a.playerCount).ToArray();
        skin.label.wordWrap = false;
        gui.BeginVertical();
        var c = gui.Height(50);
        gui.Label(Tr("Game Name"));
        foreach (var a in ar)
            gui.Label(a.name, guiSkins.serverButton, c);
        gui.EndVertical();
        gui.BeginVertical();
        gui.Label(Tr("Map Name"));
        foreach (var a in ar)
            gui.Label(CustomProperty(a, props.mapname, ""), guiSkins.serverButton, c);
        gui.EndVertical();

        gui.BeginVertical();
        gui.Label(Tr("Game Type"));
        foreach (var a in ar)
            gui.Label((GameType)CustomProperty(a, props.gametype, 0) + "", guiSkins.serverButton, c);
        gui.EndVertical();

        gui.BeginVertical();
        gui.Label(Tr("Players"));
        foreach (var a in ar)
            gui.Label(a.playerCount + "/" + a.maxPlayers, guiSkins.serverButton, c);
        gui.EndVertical();

        gui.BeginVertical();
        gui.Label("");
        foreach (var a in ar)
        {
            StringBuilder sb = new StringBuilder();
            if (CustomProperty(a, props.bombCar, false))
                sb.AppendLine("BombCar");
            int vrs = CustomProperty(a, props.version, 0);
            if (vrs != setting.multiplayerVersion)
                sb.AppendLine("v" + vrs);
            string pass = CustomProperty(a, props.havePassword, "");
            if (!string.IsNullOrEmpty(pass))
                sb.AppendLine(Tr("Password"));
            gui.Label(sb.ToString(), guiSkins.serverButton, c);
        }
        gui.EndVertical();

        gui.BeginVertical();
        gui.Label("");
        foreach (var a in ar)
            if (CustomProperty(a, props.version, 0) > setting.multiplayerVersion)
            {
                mpTitlte = string.Format(Tr("new version available: {0} your version is: {1}"), CustomProperty(a, props.version, 0), setting.multiplayerVersion);
                gui.Label("", c);
            }
            else if (gui.Button("Join", guiSkins.serverButton2, c))
                JoinButton(a, true);
        gui.EndVertical();

        gui.EndHorizontal();
        gui.EndScrollView();
    }
    private string mpTitlte = "";

    private void JoinButton(RoomInfo a, bool join = false)
    {
        var map = CustomProperty(a, props.mapname, "");
        var diff = CustomProperty(a, props.difficulty, Difficulty.Easy);
        var r = CustomProperty(a, props.rain, false);
        var n = CustomProperty(a, props.night, false);
        var rew = CustomProperty(a, props.rewinds, 3);
        var wt = CustomProperty(a, props.wait, 4);
        var dt = CustomProperty(a, props.dm, false);
        int vrs = CustomProperty(a, props.version, 0);
        var bc = CustomProperty(a, props.bombCar, false);
        var gt = (GameType)CustomProperty(a, props.gametype, dt ? 1 : 0);
        var pass = CustomProperty<string>(a, props.havePassword, null);

        //var wallColl = CustomProperty(a, props.wallCollision, false);

        //Label(table);
        //var s = string.Format(tableFormat, "", map, a.playerCount + "/" + a.maxPlayers, topdown ? "TopDown" : diff != Difficulty.Easy ? diff.ToString() : "", a.name);
        string s = a.name + " " + map;

        if (pass != null)
            s += Tr("(Password)");
        else if (gt == GameType.dmRace)
            s += Tr("(Death Race)");
        else if (gt == GameType.dm)
            s += Tr("(DeathMatch)");
        else if (gt == GameType.ctf)
            s += Tr("(Capture Flag)");
        else if (gt == GameType.zombies)
            return;
        else if (bc)
            s += Tr("(BombCar)");
        //if (!wallColl && isDebug)
        //s += "(Old)";
        if (vrs != setting.multiplayerVersion)
            s += isDebug ? "(v" + vrs + ")" : "-";
        s += " " + a.playerCount + "/" + a.maxPlayers;
        if (s.Length > maxServerLen)
            s = s.Substring(0, Mathf.Min(maxServerLen, s.Length));
        if (join || gui.Button(s, vrs > setting.multiplayerVersion ? skin.label : skin.button) || (setting.autoConnect && a.name.Contains("-host") && vrs == setting.multiplayerVersion))
        {

            if (vrs > setting.multiplayerVersion)
                Popup("you have old version, please update", win.act);
            else
            {
                Action act = delegate
                {
                    mapName = map;
                    difficulty = diff;
                    rain = r;
                    night = n;
                    rewinds = rew;
                    waitTime = wt;
                    _Loader.gameType = gt;
                    //wallCollision = wallColl;
                    //if (!isDebug)
                    weaponEnum = (WeaponEnum)CustomProperty(a, props.weapons, 0);
                    lifeDef = CustomProperty(a, props.life, 300f);
                    bombCar = bc;
                    _Loader.gameType = gt;
                    //speedLimit = CustomProperty(a, props.speedLimit, 0);
                    //speedLimitEnabled = speedLimit != 0;
                    _Loader.enableZombies = CustomProperty(a, props.zombies, false);
                    dmShootForward = CustomProperty(a, props.dmLockForward, false);
                    enableMatchTime = CustomProperty(a, props.enableMatchTimeLimit, false);
                    matchTimeLimit = CustomProperty(a, props.timeLimit, 0f);
                    enableCollision = CustomProperty(a, props.enableCollision, false);
                    ctfRandomSpawn = CustomProperty(a, props.randomSpawn, false);
                    _Loader.airFactor = CustomProperty(a, props.airResistence, 0f);
                    _Loader.rotationFactor = CustomProperty(a, props.rotationFactor, 1f);
                    _Loader.nitro = CustomProperty(a, props.nitro, 0f);
                    Popup("Connecting");
                    PhotonNetwork.JoinRoom(a);
                };
                if (pass == null)
                    act();
                else
                {
                    ShowWindow(delegate
                    {
                        Label("Enter Password");
                        gamePassword = gui.TextField(gamePassword);
                        if (Button("Ok"))
                            if (pass == gamePassword)
                                act();
                            else
                                Popup("Wrong Password");
                    }, win.act);
                }
            }
        }
    }
    public void MultiplayerWindow()
    {
        Setup(600, 500);
        if (Button("Host Game", false) || setting.autoHost)
            ShowWindowNoBack(SelectMapWindow);
        BeginScrollView();
        gui.BeginVertical("server list", win.editorSkin.window);
        //string table = "MapName        Players        Settings    GameName";
        //if (tableFormat == null)
        //tableFormat = CreateTable(table);
        if (PhotonNetwork.countOfRooms == 0)
            Label("No Games Found..");
        //else
        //    Label("Game Name, Map Name, Player Count");
        if (roomInfos.Length > 0)
        {
            foreach (var a in roomInfos.OrderByDescending(a => CustomProperty(a, props.version, 0)).ThenByDescending(a => a.playerCount))
            {
                //try
                //{
                JoinButton(a);
                //} catch (Exception)
                //{
                //    Label(a.name);
                //}
            }
            //skin.button.alignment = old;
        }
        gui.FlexibleSpace();
        gui.EndVertical();
        gui.EndScrollView();
    }
    internal int serverVersion { get { return PhotonNetwork.room == null ? setting.version : CustomProperty(PhotonNetwork.room, props.version, 0); } }
    public int maxServerLen = 100;

    public static T CustomProperty<T>(RoomInfo a, props p, T def = default(T))
    {
        var key = p.ToString();
        if (!a.customProperties.ContainsKey(key))
            return def;
        return (T)a.customProperties[key];
    }
    public void OnPhotonCreateRoomFailed()
    {
        popupText = "create room failed";
    }
    public void OnPhotonJoinRoomFailed()
    {
        popupText = "room is full";
    }
    public void OnFailedToConnectToPhoton(object status)
    {
        Popup("Failed to connect to Photon: " + status, win.act);
    }
    public void OnJoinedRoom()
    {
        if (!online) return;
        print("OnJoinedRoom");
        PhotonNetwork.isMessageQueueRunning = false;
        StartCoroutine(StartLoadLevel(mapName, true));
    }

}



