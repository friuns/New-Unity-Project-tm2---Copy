
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

#if !UNITY_FLASH || UNITY_EDITOR
using System.Linq;
#endif
#if UNITY_EDITOR
//using Exception = System.AccessViolationException;
#endif
using LitJson;
using UnityEngine;
using UnityEngine.Advertisements;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using gui = UnityEngine.GUILayout;

public partial class Loader
{

    public void WindowPool()
    {
        StartCoroutine(WindowPoolCor());
    }
    public IEnumerator WindowPoolCor()
    {
        Debug.Log("Window Pool");
        //if (!acceptLicense && android && !isDebug)
        //    win.ShowWindow(AgreelicenseWindow);
        //else 
        if (_Loader.levelEditor != null) yield break;

        if (online && PhotonNetwork.connected)
            win.ShowWindow(QuickConnectWindow);
        else if (_Game != null)
            win.ShowWindow(_Game.MenuWindow);
        //else if (urlReceived)
        //    ParseUrl();
        else if (curDict == -1)
            win.ShowWindow(SelectLangWindow);
        //else if ((playerName.Length < minLength && !loggedIn) && guest)
        //win.ShowWindow(SetNickNameWindow,win.act);
        else if (!loggedIn)
            win.ShowWindow(LoginWindow);
        //else if (!loggedIn && guest)
        //    win.ShowWindow(SetNickNameWindow);
        else if (avatar == -1)
            win.ShowWindow(SelectAvatar, win.act);
        else if (carSkin == -1)
            LoadLevel(Levels.carSelect);
        else if (WonCar != null)
            ShowWindowNoBack(WonCarWindow);
        else if (gamePlayed)
        {
            gamePlayed = false;
            if (sGameType == SGameType.Clan || sGameType == SGameType.VsPlayers)
                win.ShowWindow(SelectMapWindow);
            else
                win.ShowWindow(MenuWindow);

            StartCoroutine(SavePlayerPrefs());
        }
        else if (BonusCheck())
            ShowWindow(BonusWindow, win.act);
        else
        {
            win.ShowWindow(MenuWindow);
            SaveStrings();
        }
    }



    public void QuitWindow() { }
    public void CloseWindow()
    {
        win.showBackButton = false;
        Label("Do you want to exit?");
        gui.BeginHorizontal();
        if (Button("Yes") || Input.GetKeyDown(KeyCode.Escape))
        {
            print("Application QUIT!");
            Application.Quit();
        }
        if (Button("No"))
            win.Back();
        gui.EndHorizontal();
    }
    //private void AgreelicenseWindow()
    //{
    //    Setup(650, 300); skin.label.wordWrap = true;
    //    GUILayout.Label(GUIContent("Attention! This is free version, if you bought this game somewhere please report to soulkey4@gmail.com", res.attention));

    //    if (Button("Continue"))
    //    {
    //        acceptLicense = true;
    //        WindowPool();
    //    }
    //}
    private void WonCarWindow()
    {
        Setup(400, 230);
        LabelCenter("Congratulations! Unlocked new car, do you want to view it?", 16, true);
        GUILayout.BeginHorizontal();
        if (Button("No"))
            WindowPool();
        GUILayout.FlexibleSpace();
        if (Button("Yes"))
            LoadLevel(Levels.carSelect);
        GUILayout.EndHorizontal();
    }
    public void SelectLangWindow()
    {
        Setup(400, 500);
        BeginScrollView();
        for (int i = 0; i < setting.assetDictionaries.Length; i++)
        {
            if (ButtonTexture(setting.assetDictionaries[i].name, setting.flags[i]))
            {
                curDict = i;
                trcache.Clear();
                WindowPool();
            }
        }
        if (!_Loader.vk && !Beginner && Button("Help translate to other languages"))
            ShowWindow(HelpTranslateWindow, SelectLangWindow);
        gui.EndScrollView();
    }
    private void HelpTranslateWindow()
    {
        Setup(700, 300);
        win.addflexibleSpace = false;
        GUILayout.TextArea("Translate txt file here https://www.dropbox.com/sh/jhz61bchvbu1bd1/Amn12QIjrZ and send me to soulkey4@gmail.com, Thanks!", gui.ExpandHeight(true));

        //BeginScrollView();
        //skin.textArea.richText = false;
        //GUILayout.TextArea(setting.assetDictionaries[1].text);
        //GUILayout.EndScrollView();
    }

    private string PasswordField(string txt, char c)
    {
        var down = JoystickButton2(skin.textField);
        var s = "MyTextField" + CustomWindow.buttonId;
        if (down)
            GUI.FocusControl(s);
        GUI.SetNextControlName(s);
        return GUILayout.PasswordField(txt, c);

    }
    private string TextField(string txt, int i = -1)
    {
        var down = JoystickButton2(skin.textField);
        var s = "MyTextField" + CustomWindow.buttonId;
        if (down)
        {
            GUI.FocusControl(s);
            //TouchScreenKeyboard.Open(txt);
        }
        GUI.SetNextControlName(s);
        return GUILayout.TextField(txt, i);
    }
    internal void LoginWindow()
    {
        win.Setup(500, 400);

        if (LoadingScreen.block)
        {
            win.Setup(400, 200);
            skin.button.wordWrap = true;
            if (gui.Button(LoadingScreen.newVersionAvaibleText))
            {
                var substring = LoadingScreen.newVersionAvaibleText.Substring(LoadingScreen.newVersionAvaibleText.IndexOf("http", StringComparison.Ordinal));
                Application.OpenURL(substring);
            }
            return;
        }

        if (Event.current.isKey && Input.GetKeyDown(KeyCode.Escape))
            ApplicationQuit();
        gamePlayed = false;
        Label(_Loader.vk ? "Enter your nick:" : "Login:");
        playerName = TextField(Filter(playerName), maxLength);
        if (!_Loader.vk)
        {
            Label("password:");
            password = PasswordField(password, '*');
            rememberPassword = Toggle(rememberPassword, "Remember password");
            GUILayout.BeginHorizontal();
            if (Button("Register"))
                win.ShowWindow(RegisterWindow, LoginWindow);
        }
        else
            gui.BeginHorizontal();
        if (Button(_Loader.vk ? "ОК" : "Login"))
        {
            SaveStrings();
            if (vk && !_Integration.vkLoggedIn)
                GoOffline();
            else if (CheckLength())
            {
                Popup2("Logging in");
                Download(mainSite + "scripts/" + (_Loader.vk ? "loginRegister2.php" : "login3.php"), delegate (string text, bool success)
                {
                    if (text.StartsWith("registered") || text.StartsWith("login success") || setting.ForceLogin)
                        StartCoroutine(LoadPlayerPrefs(text));
                    else //if (text.StartsWith("wrong password") || text.StartsWith("user not exists") || text.StartsWith("user already registered"))
                         //{
                        OnLoginFailed(text);
                    //Popup2(text, LoginWindow);
                    //}
                    //else
                    //{
                    //    GoOffline();
                    //}
                }, false, "name", playerName, "password", password, "vkPassword", vkPassword, "deviceId", SystemInfo.deviceUniqueIdentifier
                //#if UNITY_EDITOR
                //                ,"a",1
                //#endif
                );
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        gui.BeginHorizontal();
        if (!_Loader.vk && Button("Play as guest", false))
            PlayGuest();

        if (!_Loader.vk && Button("Restore password", false))
            ShowWindow(RestorePasswordWindow, win.act);
        gui.EndHorizontal();
        if (_Loader.vk && !_Integration.vkLoggedIn)
            gui.Label("1");

    }

    private void OnLoginFailed(string text)
    {
        ShowWindow(delegate
                   {
                       if (Button("play as guest")) GoOffline();
                       Label(text);
                   }, LoginWindow);
    }


    private string showPass;

    private void RestorePasswordWindow()
    {
        Label("Enter your email");
        email = TextField(email.ToLower());
        if (Button("Send Password"))
            Download(mainSite + "/emails/" + GetMD5Hash(email) + ".txt", delegate (string s, bool b) { if (b) showPass = s; else Popup2("Email not found", win.act); }, false);
        if (showPass != null)
            gui.TextField(Tr("Your password:") + showPass);
    }
    private string email = "";
    private void RegisterWindow()
    {

        win.Setup(400, 400);
        Label("Name:");
        playerName = Filter(TextField(playerName, maxLength));

        Label("password:");
        password = PasswordField(password, '*');
        Label("confirm password:");
        confirmPassword = PasswordField(confirmPassword, '*');
        Label("Email");
        email = TextField(email.ToLower());
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (Button("Register"))
        {
            if (!Debug.isDebugBuild && !Regex.IsMatch(email, @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.IgnoreCase))
                Popup2("Please enter valid email address, required for password recovery", win.act);
            else if (confirmPassword != password)
                Popup2("Passwords not match", win.act);
            else if (CheckLength())
            {
                Popup2("Registering", RegisterWindow);
                Download(mainSite + "scripts/register.php", delegate (string text, bool success)
               {
                   if (text.StartsWith("registration success"))
                   {
                       StartCoroutine(LoadPlayerPrefs(text));
                       //statsSaved = true;
                       //OnLoggedIn();
                       //WindowPool();
                   }
                   else
                       Popup2(text, RegisterWindow);
               }, false, "name", playerName, "password", password, "email", email);
            }
        }
        GUILayout.EndHorizontal();
    }
    public void SetNickNameWindow()
    {
        win.Setup(300, 200);
        if (Input.GetKeyDown(KeyCode.Escape)) win.Back();
        win.showBackButton = false;
        LabelCenter("Enter Your nick name");
        playerName = TextField(Filter(playerName), maxLength);
        if (ButtonLeft("OK"))
            if (playerName.Length < minLength)
                Popup2("Enter Your Name", SetNickNameWindow);
            else
            {
                GoOffline();
            }
    }
    public void OnWindowShow(Action a)
    {
        if (a == MenuWindow)
            SetOffline(true);
        if (showBanner)
        {
            if (a == MenuWindow)
                _Loader.ShowBanner(true);
            //SamsungAd.Instance().ShowBannerAd();
            else
                _Loader.ShowBanner(false);
        }
    }
    public void MenuWindow()
    {
        if (!PhotonNetwork.isMessageQueueRunning)
            PhotonNetwork.isMessageQueueRunning = true;

        win.Setup(400, 550, "", Dock.Left, null, null);
        win.style = skin.label;
        replacementStyle = res.menuButton;

        gui.BeginHorizontal();
        if (GUILayout.Button(Avatar, _Loader.guiSkins.ramka, GUILayout.Width(64), GUILayout.Height(64), gui.ExpandWidth(false)))
            ShowWindow(_Loader.SelectAvatar, win.act);
        var r = GUILayoutUtility.GetLastRect();
        var c = r.center;
        r.width = res.ramka.width;
        r.height = res.ramka.height;
        r.center = c;
        GUI.DrawTexture(r, res.ramka);

        var last = r;
        var guiStyle = _Loader.guiSkins.ramka4;
        //guiStyle.contentOffset = new Vector2(60, -47);
        GUI.Label(last, _Awards.ranks[_Awards.GetRank(_Awards.xp)], guiStyle);

        gui.Label(Tr(" Welcome ") + Tr(playerNamePrefixed, false, false), res.labelGlow);
        gui.EndHorizontal();


        gui.Space(30);
        if (Button("Start Online Race"))
        {
            sGameType = SGameType.VsPlayers;
            ShowWindowNoBack(SelectMapWindow);
        }

        //if(assetsLoaded)
        if (Button("Multiplayer (beta)") || setting.autoConnect || setting.autoHost)
        {
            if (mh)
                Popup("hack detected, you have been banned");
            else
            {
                SetOffline(false);
                ShowWindow(QuickConnectWindow, win.act);
            }
        }
        //if (Button("Clans/Groups"))
        //    if (guest && !isDebug || mh)
        //        Popup("Please register first",MenuWindow);
        //    else
        //        ShowWindow(SearchClanWindow, MenuWindow);

        //if (Button("Play Vs Friends"))
        //{
        //    sGameType = SGameType.VsFriends;
        //    ShowWindow(SelectMapWindow);
        //}
        if (Button("2-player SplitScreen"))
        {
            sGameType = SGameType.SplitScreen;
            ShowWindowNoBack(SelectMapWindow);
        }


        if (resLoaded && Button("Select Car"))
            LoadLevel(Levels.carSelect);
        if (Button("Settings"))
            ShowWindow(SettingsWindow, MenuWindow);
        if (resLoaded && Button("Level Editor"))
            Application.LoadLevel(Levels.levelEditor);


        //if (Button("LogOut"))
        //{
        //    ResetSettings();
        //    Application.LoadLevel(Levels.menu);
        //}

        if (!android)
            _Integration.DrawButtons();

        if (ios && Button("Exit") || Event.current.isKey && Input.GetKeyDown(KeyCode.Escape) && !Application.isWebPlayer) //(android || standAlone) && 
            ApplicationQuit();
        if (Button("Achievements"))
        {
            _Awards.RefreshAwards();
            ShowWindow(_Awards.DrawAwardsWindow, win.act);
        }
        if (_Integration.site == Site.Kg && Button("Invite Friends"))
            ExternalEval(@"kongregateAPI.getAPI().services.showInvitationBox({  content: 'Come try out this awesome game!'});");
        if (_Integration.site == Site.VK && Button("Invite Friends"))
        {
            ExternalEval("VK.callMethod('showInviteBox')");
        }
        if (isDebug && Button("Edit"))
            ShowWindow(EditValuesWindow, win.act);
        if (isDebug)
            Label(Trs("Version: ") + setting.version);
        if (!resLoaded)
            LoadingLabelAssetBundle();
        gui.FlexibleSpace();
        replacementStyle = null;
    }
    private KeyValuePair<string, string>[] keyValuePairs;
    private void EditValuesWindow()
    {
        Setup(600, 1000);
        if (Button("Save"))
            StartCoroutine(SavePlayerPrefs());

        searchMap = gui.TextField(searchMap);

        BeginScrollView();
        if (keyValuePairs == null)
            keyValuePairs = PlayerPrefString.ToArray();
        foreach (var a in PlayerPrefString)
        {
            if (searchMap.Length > 3 && a.Key.Contains(searchMap))
            {
                gui.BeginHorizontal();
                gui.Label(a.Key, gui.ExpandWidth(false));
                var Value = gui.TextField(a.Value);
                if (Value != a.Value)
                    PlayerPrefString[a.Key] = Value;
                gui.EndHorizontal();
            }
        }
        gui.EndScrollView();
    }


    internal bool resLoaded2;
    //{
    //get { return wwwAssetBundle != null && wwwAssetBundle.isDone; }
    //}
    internal bool resLoaded;
    //{
    //get { return wwwAssetBundle == null || wwwAssetBundle.isDone; }
    //}

    public void SetOffline(bool offline)
    {
        if (PhotonNetwork.connected && offline && !offlineMode)
        {
            print("Disconnecting");
            PhotonNetwork.Disconnect();
        }
        if (offlineMode != offline)
        {
            offlineMode = offline;
            print("Set Offline " + offline);
            //if (offline && PhotonNetwork.room == null)
            //    PhotonNetwork.CreateRoom("test");
        }
        if (!offline)
        {
            ConnectToPhoton();
            print("Connecting");
        }
    }
    internal void OnConnectionFailed()
    {
        Debug.LogWarning("OnFailedToConnectToPhoton");
        PhotonNetwork.Disconnect();
        StartCoroutine(AddMethod(() => PhotonNetwork.connectionState == ConnectionState.Disconnected, delegate
        {
            appId++;
            if (appId >= setting.appIds.Length)
                appId = 0;
            else
                ConnectToPhoton();
        }));
    }


    public void ConnectToPhoton()
    {
        PhotonNetwork.Connect("app-eu.exitgamescloud.com", 5055, setting.appIds[appId], "tm" + (setting.oldVer ? 840 : 841));
    }

    private int appId;
#if old
    private void SearchClanWindow()
    {
        Label("Group name:");
        clanName = gui.TextField(Filter(clanName), 10);        
        if (Button("Search"))
        {
            if (clanName.Length < 1)
            {
                Popup2("Enter Clan Name", SearchClanWindow);
                return;
            }
            Popup2("Searching..");
            Download(mainSite + "clans/" + clanName + ".txt", delegate(string s, bool b)
            {
                clanMembers = b ? new List<string>(SplitString(s)) : new List<string>();
                inclan = contains();
                _ChatPhp = new ChatPhp();
                ShowWindowNoBack(ClanWindow);

            }, false);
        }
    }
    private bool contains()
    {
        foreach (string a in clanMembers)
        {
            if (a.ToLower() == playerNamePrefixed.ToString().ToLower()) return true;
        }
        return false;
    }
    private bool inclan;
    private void ClanWindow()
    {
        
        Setup(500, 400);
        var txt = (Texture2D)LoadRes("icons/clan" + clanName.ToLower());
        gui.Label(GUIContent(string.Format(Trs("Clan {0}, {1} members found"), clanName, clanMembers.Count), txt));
        //Label(string.Format(Trs("Clan {0}, {1} members found"), clanName, clanMembers.Count));
        if (clanMembers.Count > 0)
            Label(Trs("Clan creator: ") + clanMembers[0]);
        
        gui.BeginHorizontal();
        if (BackButton())
            ShowWindowNoBack(MenuWindow);
        
        if (Button(clanMembers.Count == 0 ? "Create Clan" : inclan ? "Play" : "Join"))
        {
            clanTag = clanName.Substring(0, Mathf.Min(clanName.Length, 3));
            if (!inclan)
                Download(mainSite + "scripts/clan.php", delegate { }, true, "clan", clanName, "playerName", playerNamePrefixed);
            sGameType = SGameType.Clan;
            ShowWindowNoBack(SelectMapWindow);
        }
        gui.EndHorizontal();


        gui.Space(10);
        string cls = "";
        if (clanMembers.Count > 1)
            for (int i = clanMembers.Count - 1, j = 0; i >= 0 && j < 20; i--, j++)
                cls += clanMembers[i] + ", ";
        //Label(Trs(cls), 16, true);
        skin.label.wordWrap = true;
        //gui.Label(cls);

        _ChatPhp.room = clanName;
        _ChatPhp.def = Tr("Clan chat ") + clanName + "\n" + cls;

        _ChatPhp.DrawChat();
    }
#endif
    ChatPhp _ChatPhp = new ChatPhp();

    public void MapSelectWindow() { }
    internal int tabSelected;
    WWW wwwUserMaps;

    //public bool advancedOnly;

    public void SelectMapWindow()
    {

        win.Setup(900, 600, Tr(sGameType.ToString()));
        LabelCenter("Choose map", 16, false);

        //int sceneCount = SceneCount;
        tabSelected = Mod(tabSelected, titles.Length);
        bool userTab = tabSelected == Tag.userTab;

        DrawTab();
        if (userTab && !resLoaded)
        {
            LoadingLabelAssetBundle();
            gui.FlexibleSpace();
            return;
        }

        //string[] tabnames = new string[(sceneCount / 8)+1];
        //for (int i = 0; i < tabnames.Length; i++)
        //    tabnames[i] = Tr("Stage ") + i;


        BeginScrollView(win.skin.box, !android);


        int need = 1;
        if (userTab)
        {
            GUI.enabled = wwwUserMaps != null && wwwUserMaps.isDone;
            gui.BeginHorizontal();
            gui.FlexibleSpace();
#if oldFlags
            if (online)
            {
                gui.BeginHorizontal(skin.box);
                if (Button("Game Type", false))
                    ShowWindow(SelectGameType, win.act);
                gui.EndHorizontal();
            }
#endif

            var toolbar = (TopMaps)Toolbar((int)topMaps, new[] { "Featured", "Top", "New" }, false);
            if (toolbar != topMaps)
            {
                topMaps = toolbar;
                DownloadUserMaps2(0);
            }
            for (int i = 0; i < 4; i++)
                if (GlowButton((i + 1) + "", i == page))
                    DownloadUserMaps2(i);
            gui.FlexibleSpace();
            gui.EndHorizontal();
            //if (isMod)

            //advancedOnly = Toggle(advancedOnly, "Advanced only");

            GUI.enabled = true;
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        //List<string> mapby = new List<string>();
        for (int i = 0, j = 0; i < scenes.Count; i++)
        {
            Scene scene = scenes[i];

            need += 1;//difficulty == Difficulty.Easy ? 1 : (i * 3 / sceneCount + 1);

            if (scene.j != tabSelected) { continue; }
            //if (!string.IsNullOrEmpty(scene.mapBy) && string.IsNullOrEmpty(searchMap))
            //{                
            //    string mapBy = scene.mapBy.TrimStart('-');
            //    if (mapby.Count(a => a == mapBy) > 1)
            //        continue;
            //    mapby.Add(mapBy);
            //}
            var loading = !scene.loaded && !scene.userMap; //!maps.ContainsKey(scene.name.ToLower()) && !CanStreamedLevelBeLoaded(scene.name) || !CanStreamedLevelBeLoaded(Levels.game);
            var bonus = (i + DateTime.Now.Day) % 10 == 0;
            bool unlocked = medals >= need || bonus || scene.userMap || i == 0 || isDebug;

            if (Time.time - levelLoadTime < 1.5f && medals >= need && medals - wonMedals < need)
                unlocked = Time.time % .4f < .2f;
            GUI.enabled = !loading && unlocked;
            //if (!unlocked) continue;

            GUILayout.Space(15);
            GUILayout.BeginVertical(userTab ? skin.box : GUIStyle.none);

            prefixMapPl = (scene.name + ";" + _Loader.playerName + ";");
            GUILayout.Label(GUIContent(record == float.MaxValue ? null : TimeToStr(record), place == 4 ? null : win.medals[place]), GUILayout.Height(20), GUILayout.Width(100));
            skin.button.wordWrap = true;
            var buttonPressed = scene.userMap ? SoundButton(gui.Button(new StringBuilder(Tr("Map by ")).Append(scene.mapBy).Append(scene.played ? "\n(played)" : "").ToString(), GUILayout.Height(100), GUILayout.Width(100)))
                : JoystickButton2(res.mapSelectButton) || SoundButton(GUILayout.Button(GUIContent(unlocked ? scene.texture : win.locked, string.Format(Tr("You need {0} medals to unlock"), Mathf.Max(0, need))), res.mapSelectButton, GUILayout.Height(100), GUILayout.Width(100)))
                || setting.autoHost && i == 0;

            if (userTab && scene.rating != 0)
                gui.Label(res.rating[Mathf.RoundToInt(scene.rating * 2)]);
            if (buttonPressed)
                OnMapSelected(scene);
            if (!scene.userMap)
            {
                var s = scene.name + "\n" + Tr(loading ? "not found" : medals >= need ? "" : bonus ? "map rotation" : "Locked", true);
                LabelCenter(s, 16, false, null, win.skin.label);
            }
            else
                gui.Label(scene.title, gui.Width(100));

            GUILayout.EndVertical();

            if ((++j) % 5 == 0 && j != 0)
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }


        }

        GUI.enabled = true;
        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.EndScrollView();

        DrawMedals();

    }
#if oldFlags
    private void SelectGameType()
    {
        LabelCenter("Game Type");
        //gui.BeginHorizontal();
        var o = mapSets.levelFlags;
        if (GlowButton("Race", mapSets.race))
            mapSets.levelFlags = LevelFlags.race;
        //if (!online && GlowButton("Coins", mapSets.enableCoins))
        //mapSets.levelFlags = LevelFlags.stunts;
        if (online)
        {
            if (GlowButton("DeatmMatch", mapSets.enableDm))
            {
                mapSets.levelFlags = LevelFlags.dm;
                gameType = GameType.dm;
            }
            if (GlowButton("Capture the flag", mapSets.enableCtf))
            {
                mapSets.levelFlags = LevelFlags.Ctf;
                gameType = GameType.ctf;
            }
        }
        if (o != mapSets.levelFlags)
        {
            topMaps = TopMaps.New;
            DownloadUserMaps2(0);
            win.Back();
        }
        //gui.EndHorizontal();
    }
#endif
    private void OnMapSelected(Scene scene)
    {
        mapName = scene.name;
        levelName = scene.name;
        //if (!online)

        _Loader.gameType = scene.mapSets.enableCtf ? GameType.ctf : scene.mapSets.enableDm ? GameType.dm : GameType.race;
        ResetServerSettings();

        if (sGameType == SGameType.Multiplayer)
            ShowWindow(HostGameWindow, win.act);
        else if (sGameType == SGameType.VsPlayers || sGameType == SGameType.SplitScreen)
            win.ShowWindow(PlayVsPlayersWindow, SelectMapWindow);
#if old
        else if (vsFriendsOrClan)
        {
            rain = night = false;
            StartCoroutine(LoadPlayerPreviews());
            win.ShowWindow(PlayVsFriendWindow, SelectMapWindow);
        }
#endif
    }
    private void ResetServerSettings()
    {

        airFactor = 0;
        rotationFactor = 1;
        ctfRandomSpawn = false;
        matchTimeLimit = 0;
        night = bombCar = rain = enableCollision = false;
        mapSets.levelFlags = LevelFlags.race;
        enableMatchTime = false;
        enableZombies = enableCollision = pursuit;
        if (!android)
        {
            rain = Random.value < .1f && !isDebug;
            topdown = false;
            //night = Random.value < .3f && !Beginner;
        }
        weaponEnum = 0;
        foreach (WeaponEnum a in Enum.GetValues(typeof(WeaponEnum)))
            weaponEnum |= a;
        //weaponEnum = (WeaponEnum)~0;
    }

    internal void LoadingLabelAssetBundle()
    {
        if (!resLoaded)
            LabelCenter("Game Loading " + (int)(wwwAssetBundle != null ? wwwAssetBundle.progress * 100 : 0) + "%");
    }

    internal enum TopMaps { Staff_Pick, Top, New }
    internal TopMaps topMaps;

    public void DownloadUserMaps2(int page)
    {

        StartCoroutine(DownloadUserMaps(topMaps == TopMaps.Staff_Pick ? 2 : 1, (int)mapSets.levelFlags, "", page, topMaps == TopMaps.Top));
    }
    private void DrawTab()
    {
        gui.BeginHorizontal();
        skin.button.wordWrap = false;
        if (JoystickButton2(skin.button) || SoundButton(gui.Button("<", gui.ExpandHeight(true), gui.Width(100))))
            tabSelected--;
        GUILayout.FlexibleSpace();
        tabSelected = Toolbar(tabSelected, titles, false, false);
        GUILayout.FlexibleSpace();
        if (JoystickButton2(skin.button) || SoundButton(gui.Button(">", gui.ExpandHeight(true), gui.Width(100))))
            tabSelected++;


        gui.EndHorizontal();
    }
    void SearchMapWindow()
    {

        //if (isDebug)
        //{
        //    LabelCenter("Game Type:", 16, false);
        //    gui.BeginHorizontal();
        //    var o = mapSets.levelFlags;
        //    if (GlowButton("Race", mapSets.race))
        //        mapSets.levelFlags = LevelFlags.race;
        //    if (GlowButton("Coins", mapSets.enableCoins))
        //        mapSets.levelFlags = LevelFlags.stunts;
        //    if (GlowButton("Dm", mapSets.enableDm))
        //        mapSets.levelFlags = LevelFlags.dm;
        //    if (o != mapSets.levelFlags)
        //    {
        //        StartCoroutine(DownloadUserMaps(0));
        //        ShowWindow(SelectMapWindow);
        //    }
        //    gui.EndHorizontal();
        //}

        searchMap = gui.TextField(searchMap);
        if (ButtonLeft("Search") && searchMap.Length > 2)
        {
            StartCoroutine(DownloadUserMaps(0, (int)mapSets.levelFlags, searchMap));
            tabSelected = Tag.userTab;
            ShowWindowNoBack(SelectMapWindow);
        }
        StringBuilder sb = new StringBuilder();

        if (favorites.Count > 0)
        {
            foreach (var a in favorites)
                sb.Append(a + ",");
            Label("Your Favorites: " + sb, 16, true);
        }
    }
    private string searchMap = "";
    public void DrawMedals()
    {
        skin.label.fontSize = 14;
        gui.BeginHorizontal();
        skin.button.fixedHeight = 40;
        if (warScore > 0 && Button(warScore.ToString(), false, 20, false, _Loader.guiSkins.warScore))
            Popup("warScoreText", _Loader.guiSkins.warScore);
        if (Button(medals.ToString(), false, 20, false, win.medalsCnt))
            Popup("medaltext", win.medalsCnt);
        if (!_Loader.disableRep)
            if (Button(reputation.ToString(), false, 20, false, win.reputation))
                Popup(Trs("reptext") + string.Format(", У вас {0} друзей вк", friendCount), win.reputation);
        //if (Button(score + Trs(" overtaken"), false, 20, false, win.score))
        //    Popup("tells you how many players you have been overtaken in scoreboard", win.score);
        skin.button.fixedHeight = 0;
        if (BackButtonLeft())
            ShowWindowNoBack(MenuWindow);
        if (Button("Search Map"))
            ShowWindow(SearchMapWindow, SelectMapWindow);
        gui.EndHorizontal();
    }

    private void SendLinkWindow()
    {
        Setup(600, 350);
        gui.Label(res.sendReplayIcon);
        LabelCenter("Send this link to friend so he can play vs you", 16, true);
        gui.TextArea(_Loader.replayLinkPrefix + _Loader.replayLink);
    }

    protected void PlayVsPlayersWindow()
    {
        Setup(400, 500); // поделится рекордом
        prefixMapPl = curScene.name + ";" + _Loader.playerName + ";";

        //if (Button("Share record (+3 rep)"))
        //{
        //    if (_Integration.vkLoggedIn)
        //        _Integration.wallPost(record);
        //    ShowWindow(SendLinkWindow, win.act);
        //}

        if (isMod && Button("Rate Map"))
            ShowWindow(_Loader.RateMapWindow, PlayVsPlayersWindow);

        LabelCenter(Trs("How Many Opponents?"));
        replacementStyle = res.menuButton;
        foreach (var i in new int[] { 0, 1, 3, 6 })
            if ((i != 1 || android) && (i != 30 || tester) && Button(i + Trs(" racers")))
            {
                PlayersCount = i;
                StartLoadReplays();
            }
        //if (Button("Show Scoreboard"))
        //{
        //    scoreBoard = null;
        //    ShowWindow(ScoreBoardWindow,win.act);
        //}
        GUILayout.BeginHorizontal();
        //gameType = Toggle(gameType == GameType.zombies, "Zombies") ? GameType.zombies : GameType.race;
        //if (resLoaded)
        //_Loader.enableZombies = Toggle(enableZombies, "Zombies"); 
        _Loader.rain = Toggle(rain, "Rain");
        _Loader.night = Toggle(night, "Night");

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        _Loader.enableCollision = Toggle(enableCollision, "Collision");
        _Loader.bombCar = Toggle(bombCar, "BombCar");
        GUILayout.EndHorizontal();
        replacementStyle = null;
        Label("Difficulty");
        _Loader.difficulty = (Difficulty)Toolbar((int)_Loader.difficulty, Enum.GetNames(typeof(Difficulty)), true);
    }
    internal string scoreBoard;



    private bool hideChat;
    public void LoadScoreBOard(bool hideChat = false)
    {
        this.hideChat = hideChat;
        if (online)
        {
            ParseOnlineScoreboard();
        }
        else if (scoreBoard == null)
        {
            scoreBoard = Tr("Loading..");
            if (!online)
                Download(mainSite + "scripts/getScoreboard3.php", ParseScoreboard, false, "map", curScene.mapId, "dm", _Loader.dmOrCoins ? 1 : 0, "flags", (int)replayFlags);
        }
        _ChatPhp = new ChatPhp();
        ShowWindow(ScoreBoardWindow, win.act);
    }
    private void ParseOnlineScoreboard()
    {
        scoreBoard = "Online\n";
        var table = GetTable();
        var t = CreateTable(table);
        scoreBoard += table + "\n";
        int i = 0;
        List<ServerScore> parsed = new List<ServerScore>();
        foreach (var a in PhotonNetwork.room.customProperties.Cast<KeyValuePair<object, object>>())
        {
            var pp = a.Key.ToString().Split(':');
            if (pp[0] == ServerProps.score2)
                parsed.Add(new ServerScore() { nick = pp[1], value = (float)a.Value });
        }
        foreach (var a in parsed.OrderBy(a => _Loader.dm ? -a.value : a.value))
        {
            i++;
            scoreBoard += string.Format(t, i, a.nick, _Loader.dmNoRace ? a.value.ToString() : TimeToStr(a.value), "", "") + "\n";
        }
    }
    public void ScoreBoardWindow()
    {
        Setup(800, 500);
        win.showBackButton = false;
        win.addflexibleSpace = false;


        LabelCenter(Trs("Map ") + _Loader.mapName);
        gui.BeginHorizontal();

        var guiLayoutOption = hideChat ? gui.ExpandWidth(true) : gui.Width(300);
        scroll = gui.BeginScrollView(scroll, guiLayoutOption);
        LabelCenter("Scoreboard");

        skin.label.alignment = TextAnchor.UpperLeft;
        gui.Label(scoreBoard);

        gui.EndScrollView();
        if (!hideChat)
        {
            _ChatPhp.room = _Loader.mapName;
            GUILayout.BeginVertical();
            LabelCenter("Chat");
            _ChatPhp.DrawChat();
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();

        gui.BeginHorizontal();
        if (_Game != null && isMod)
            _Game.RateButton(win.act);
        if (win.BackButton())
            win.Back();
        gui.EndHorizontal();
    }
    private void ParseScoreboard(string s, bool b)
    {
        scoreBoard = s;

        if (b)
        {
            var table = GetTable();
            scoreBoard = table + "\n";
            string t = CreateTable(table);
            int place = -1;
            var ss = SplitString(s);
            bool recordAdded = false;
            for (int i = 0; i < ss.Length; i++)
            {
                var sa = ss[i].Split(';');
                string retries = sa[2] == "-1" ? "" : sa[2];
                var tm = float.Parse(sa[1]);

                if (sa[0].ToLower() == _Loader.playerNamePrefixed.ToString().ToLower())
                    continue;

                if (_Loader.dmOrCoins)
                {
                    if (_Player.score > tm && !recordAdded)
                    {
                        recordAdded = true;
                        scoreBoard += string.Format(t, i + 1, _Loader.playerName, _Player.scoreInt, retries) + "\n";
                        place = i + 1;
                        i++;
                    }
                }
                else
                {
                    if (_Loader.record < tm && !recordAdded)
                    {
                        recordAdded = true;
                        scoreBoard += string.Format(t, i + 1, _Loader.playerName, TimeToStr(_Loader.record, false, _Loader.dmOrCoins), retries, _Loader.dmOrCoins) + "\n";
                        place = i + 1;
                        i++;
                    }
                }
                scoreBoard += string.Format(t, i + 1, sa[0], TimeToStr(tm, false, _Loader.dmOrCoins), retries) + "\n";
            }
            if (place != -1)
                scoreBoard = "Your Place is " + place + "\n" + scoreBoard;
        }
    }
    private static string GetTable()
    {
        string table = Tr("Place") + Tr(" Name") + "            " + Tr(_Loader.dmOrCoins ? "Score" : "Time") + "      " + Tr("Rewinds");
        return table;
    }
#if old
    private void PlayVsFriendWindow()
    {
        win.Setup(650, 550,levelName);
        if (sGameType != SGameType.Clan)
        {
            Label(string.Format("You have {0} friends", friends.Count - 1));
            Label("Enter Player name:");
            friendName = TextField(friendName);
            GUILayout.BeginHorizontal();
            if (_Loader.vk)
                guestGui = false;
            else
                guestGui = Toggle(guestGui, "Guest Account");
            if (ButtonLeft("Find"))
                if (friendName.Length < minLength)
                    Popup2("Enter Player Name", PlayVsFriendWindow);
                else
                {
                    string fr = ((guestGui ? "-" : "") + friendName).ToLower();
                    Popup2("Searching");
                    if (friends.Contains(fr))
                    {
                        Popup2("Friend Already added", PlayVsFriendWindow);
                        return;
                    }
                    Download(mainSite+"players2/" + fr + ".txt", delegate(string s, bool b)
                    {
                        if (b)
                        {
                            friends.Add(fr);
                            friends = friends;
                            StartCoroutine(LoadPlayerPreviews());
                            Popup2("Friend Added", PlayVsFriendWindow);
                        }
                        else
                            Popup2("Friend not found", PlayVsFriendWindow);
                    }, false);
                }
            GUILayout.EndHorizontal();
        }
        BeginScrollView(null, true, false);
        GUILayout.BeginHorizontal();
        skin.button.overflow.bottom = 22;
        int selected = 0;
        int i = 0;
        if (previews.Count == 0)
            LabelCenter("Your friend must play this map first");
        foreach (Replay pv in previews)
        {
            i++;
            GUILayout.BeginVertical();
            Texture2D old = skin.button.normal.background;
            skin.button.normal.background = pv.selected ? skin.button.active.background : skin.button.normal.background;
            if (GUILayout.Button(res.GetAvatar(pv.avatarId, pv.avatarUrl)))
                pv.selected = !pv.selected;
            if (pv.selected)
                selected++;
            skin.button.normal.background = old;
            LabelCenter(Trn(pv.playerName));
            LabelCenter(Trn(TimeToStr(pv.finnishTime)));
            LabelCenter(Trn(i + ""), 20);
            if (pv.backupRetries != -1)
                LabelCenter(Trn("Re:" + pv.backupRetries), 20);            
            if (pv.dif == Difficulty.Hard)
                LabelCenter("(Pro)", 12);
            GUILayout.EndVertical();
        }
        skin.button.overflow.bottom = 0;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        gui.EndScrollView();
        if ((selected > 0 || previews.Count == 0) && Button(Trs("Start (") + selected + ")"))
        {
            win.ShowWindow(LoadReplayWindow);
            List<string> ss = new List<string>();
            foreach (var a in previews)
                if (a.selected)
                    ss.Add("replays/" + a.playerName + "." + mapNamePrefixed + ".rep");
            StartCoroutine(LoadReadReplay(ss.ToArray()));

        }
    }
    //obsolete
    protected IEnumerator LoadPlayerPreviews()
    {
        var previews = new List<Replay>();
        this.previews = previews;

        List<string> list = sGameType == SGameType.Clan ? clanMembers : friends;

        if (sGameType == SGameType.Clan)
        {
            string s = mainSite + "/scripts/getClanMap.php?clan=" + clanName + "&map=" + mapNamePrefixed;
            var w = new WWW(s);
            yield return w;
            print(w.url + "\n" + w.text);
            //if (string.IsNullOrEmpty(w.error))

            list = new List<string>(SplitString(w.text));
        }

        LoadPlayerPreviews2(list, previews);
        yield return null;
    }
    //obsolete
    private void LoadPlayerPreviews2(List<string> list, List<Replay> previews)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var nick = list[i];
            if (Any(previews, nick)) continue;
            var path = "replays/" + nick + "." + mapNamePrefixed + ".info";
            Download(mainSite + path, delegate(string s, bool b)
            {
                if (!b)
                    return;
                var ss = s.Split(new[] { "\r", "\n", "\\n", "\\r\\n" }, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    var replay = new Replay();
                    replay.playerName = nick;
                    replay.finnishTime = float.Parse(ss[0]);
                    replay.avatarId = int.Parse(ss[1]);
                    replay.dif = ss.Length > 2 ? (Difficulty)int.Parse(ss[2]) : Difficulty.Easy;
                    replay.backupRetries = ss.Length > 3 ? int.Parse(ss[3]) : -1;
                    previews.Add(replay);
                } catch (Exception e) { Debug.LogError(e); }
                previews.Sort(previews[0]);
            }, false);
        }
    }
#endif
    private static bool Any(List<Replay> list, string nick)
    {
        foreach (Replay c in list)
        {
            if (c.playerName == nick) return true;
        }
        return false;
    }
    public void LoadReplayWindow()
    {
        Setup(600, 300, "Loading Replays");
        BeginScrollView(new Vector2());
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label(Avatar);
        Label(Trn(playerName));
        GUILayout.EndVertical();
        for (int i = tempReplays.Count - 1; i >= 0; i--)
        {
            Replay a = tempReplays[i];
            GUILayout.BeginVertical();
            GUILayout.Label(res.GetAvatar(a.avatarId, a.avatarUrl));
            Label(Trn(a.playerName));
            GUILayout.EndVertical();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        if (Button("Skip and play alone"))
        {
            StopAllCoroutines();
            replays = new List<Replay>();
            StartCoroutine(StartLoadLevel(mapName));
        }
    }
    internal bool advancedOptions;

    public void SettingsWindow()
    {
        win.Setup(500, 500, "Settings");
        BeginScrollView();
        gui.Label(Tr("version:") + LoadingScreen.version + " updated:" + setting.versionDate);
        if (!setting.vk2)
            advancedOptions = Toggle(advancedOptions, "Advanced settings");
        if (!android && Button("FullScreen"))
            FullScreen(true);
        var toggle = Toggle(_Loader.rearCamera, "Rear Camera");
        if (toggle != _Loader.rearCamera)
        {
            _Loader.rearCamera = toggle;
            StartCoroutine(_AutoQuality.SetQuality(_Loader.quality));
        }
        if (android)
            _Loader.showBanner = Toggle(_Loader.showBanner, "Show Banners (Support us)");

        if (_Game != null && android && Button("Edit Controls"))
        {
            _Game.editControls = true;
            ShowWindowNoBack(_Game.MenuWindow);
        }
        showYourGhost = Toggle(showYourGhost, "Show your ghost");

        if (android && advancedOptions)
            _Loader.scaleButtons = Toggle(_Loader.scaleButtons, "Scale buttons");



        if (!android)
        {
            controls = Toggle(enableMouse, "Enable Mouse") ? Contr.mouse : Contr.keys;
            if (!flash)
            {
                autoFullScreen = Toggle(autoFullScreen, "Auto Full Screen");
                if (advancedOptions)
                    Application.runInBackground = Toggle(Application.runInBackground, "Run In Background");
            }
            if (Button("Keyboard", false))
                ShowWindow(KeyboardSetup, SettingsWindow);
        }

        if (Button("Language", false))
            win.ShowWindow(SelectLangWindow, win.act);
        //if (!_Game)
        //{
        //    Label("Difficulty");
        //    _Loader.difficulty = (Difficulty)Toolbar((int)_Loader.difficulty, Enum.GetNames(typeof(Difficulty)), true);
        //}
        //if (enableMouse)
        //{

        //}
        if (android)
        {
            Label("Controls");
            controls = Toolbar(controls, Contr.names, true);
        }
        //if (_Game == null && Button("Enter Cheat"))
        //    ShowWindow(CheatWindow, MenuWindow);

        gui.BeginHorizontal();
        if (enableMouse)
            sensivity = HorizontalSlider("Mouse Sensivity", sensivity, .5f, 2f);

        soundVolume2 = HorizontalSlider("sound volume", soundVolume2, 0, 1);
        gui.EndHorizontal();

        //if (android)
        //{
        gui.BeginHorizontal();
        //if (_music.aus.Count > 0 || musicVolume == 0)
        musicVolume = HorizontalSlider("music volume", musicVolume, 0, 1);

        //}
        _Loader.voiceChatVolume = HorizontalSlider("Voice Chat Volume", _Loader.voiceChatVolume, 0, 1);
        gui.EndHorizontal();
        gui.BeginVertical(win.skin.box);

        _AutoQuality.DrawSetQuality();
        _AutoQuality.DrawDistance();

        if (advancedOptions)
        {
            Label("Cull mode");
            var cullMode = (CullEnum)GUILayout.Toolbar((int)_Loader.CullMode, new[] { "Sharp", "Smooth", "None" });
            if (_Loader.CullMode != cullMode)
            {
                _Loader.CullMode = cullMode;
                _AutoQuality.UpdateCull();
            }
        }
        gui.EndVertical();

        if (_Game != null && advancedOptions && !online)
        {
            Label("Time Scale: " + _Game.customTime);
            _Game.customTime = GUILayout.HorizontalSlider(_Game.customTime, .5f, 1);
        }

        if ((android || androidPlatform) && advancedOptions)
        {
            if (_Game == null)
                reverseSplitScreen = Toggle(reverseSplitScreen, "Reverse SplitScreen");
            setting.m_android = Toggle(android, "Mobile/Touch Screen");
        }
        //if (advancedOptions && isDebug)
        //setting.useKeysForGui = Toggle(setting.useKeysForGui, "Control GUI using Keys (buggy)");

        if (advancedOptions)
        {
            gui.BeginHorizontal();
            if (Button("Smaller Resolution"))
                Screen.SetResolution(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2, true);
            if (Button("Bigger Resolution"))
                Screen.SetResolution(Screen.currentResolution.width * 2, Screen.currentResolution.height * 2, true);
            gui.EndHorizontal();
            _Loader.enableChat = gui.Toggle(_Loader.enableChat, "Enable Chat");
            gui.BeginHorizontal();
            Label("Clan Tag");
            _Loader.clanTag = gui.TextField(_Loader.clanTag, 3);
            gui.EndHorizontal();
            if (Button("Log"))
            {
                consoleTxt = log.Append("\nerrors count:" + errors).ToString();
                ShowWindow(ConsoleWindow, win.act);
            }
            if (Application.isEditor || setting.debug)
                setting.debug = gui.Toggle(setting.debug, "Debug");
            else
                gui.Label("");

        }
        //if (_Integration.loggedIn)
        //postFeedBack = Toggle(postFeedBack, "Post Facebook Feed");
        if (isDebug)
        {
            setting.enableLog = Toggle(setting.enableLog, "Enable Log");
            setting.noLevelCache = Toggle(setting.noLevelCache, "noLevelCache");
        }

        if (Button("Credits"))
            ShowWindow(Credits);
        GUILayout.EndScrollView();
    }

    private void Credits()
    {
        Setup(600, 600);
        win.addflexibleSpace = false;
        gui.TextArea(res.credits);
    }




    string consoleTxt = "";
    Vector2 consoleScroll = new Vector2(0, MaxValue);
    public void ConsoleWindow()
    {
        Setup(1000, 1000);
        Label("Press ctrl+c to copy");
        consoleScroll = gui.BeginScrollView(consoleScroll);
        gui.TextField(consoleTxt);
        SelectAll(consoleTxt.Length);

        gui.EndScrollView();
    }
    
    internal bool tankCheat;

#if old
    private void CheatWindow()
    {
        cheat = TextField(cheat);
        if (Button("ok"))
        {
            string s;
            print(LoadingScreen.cheats.Count);

            if (LoadingScreen.cheats.TryGetValue(cheat.ToLower(), out s))
            {
                print("Cheat FOund " + s);
                if (s == "levels")
                    levelsCheat = true;
                else if (s == "cars")
                    carsCheat = true;
                else if (s == "tank")
                    tankCheat = true;
                else if (s == "medals")
                    medals = 999;
                else
                    print("cheat error ");
                Popup2("Cheat Activated", MenuWindow);
            }
            else
                Popup2("Cheat Not Found", MenuWindow);
        }
    }
#endif
    //private void SelectGameType()
    //{
    //    win.Setup(400, 550, Trs("Map: ") + levelName);
    //    if (Button("Play Online", true, 20, true, win.onePlayer))
    //    {
    //        gameType = GameType.VsPlayers;
    //        if (Beginner)
    //            StartLoadReplays();
    //        else
    //            win.ShowWindow(PlayVsPlayers, SelectGameType);
    //    }
    //    if (Beginner) GUI.enabled = false;
    //    if (Button("2-Player", true, 20, true, win.splitScreen))
    //    {
    //        gameType = GameType.SplitScreen;
    //        win.ShowWindow(PlayVsPlayers, SelectGameType);
    //    }
    //    if (Button("Play vs Friend", true, 20, true, win.vsFriend))
    //    {
    //        gameType = GameType.VsFriends;
    //        win.ShowWindow(PlayVsFriendWindow, SelectGameType);
    //        StartCoroutine(LoadFriendList());
    //    }
    //    Label("Difficulty");
    //    difficulty = (Difficulty)Toolbar((int)difficulty, Enum.GetNames(typeof(Difficulty)), true);
    //    if (medals < 1) GUI.enabled = true;
    //}
    public void SelectAvatar()
    {
        win.Setup(600, 300, "Select Character");
        win.showBackButton = false;
        BeginScrollView(null, true);
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        //if(!string.IsNullOrEmpty(avatarUrl))
        //    ButtonTexture(avatarUrl2)
        _Awards.xp.Calculate();
        for (int i = 0; i < res.avatars.Length; i++)
        {
            var i2 = res.avatars.Length - (_Awards.ranks.Length - 2);
            GUI.enabled = i < i2 + _Awards.xp.level;

            if (gui.Button(new GUIContent(null, res.GetAvatar(i, avatarUrl), GUI.enabled ? "" : string.Format("You need level {0}, your level is {1}", (i - i2) + 1, _Awards.xp.level))) || Input.GetKeyDown(KeyCode.Escape))
            {
                avatar = i;
                if (_CarSelectMenu != null)
                    ShowWindowNoBack(_CarSelectMenu.Window);
                else
                    WindowPool();
                break;
            }
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.EndScrollView();
        Label(win.tooltip);
    }
    protected bool CheckLength()
    {
        if (playerName.Length < minLength)
            Popup2("Enter Your Name", LoginWindow);
        else if (password.Length < minLength && !vk)
            Popup2("password is too short", LoginWindow);
        else
            return true;
        return false;
    }

    //public void InitCars()
    //{
    //    //var carSkins = _Loader.CarSkins;
    //    var carSkins = CarSkins;
    //    print(SceneCount);
    //    for (int i = 0, j = 0; i < carSkins.Count; i++)
    //    {
    //        var carSkin = carSkins[i];
    //        if (carSkin.allMedals) carSkin.medalsNeeded = (int)(SceneCount * 2.5f);
    //        if (carSkin.special) continue;
    //        carSkin.arrayId = i;
    //        //if (car == -1 && i == 0)
    //        //    car = i;
    //        carSkin.medalsNeeded = (int)((float)j / NotSpecialCount * (SceneCount * 3));
    //        j++;
    //        //carSkin.SetDirty();
    //    }
    //}

    //private int NotSpecialCount
    //{
    //    get
    //    {
    //        int count = 0;
    //        foreach (CarSkin a in CarSkins)
    //            if (!a.special) count++;
    //        return count;
    //    }
    //}

}
public static class Contr
{
    public static string[] names = new string[] { "Pad", "Keys", "Accelerometer" };
    public static int mouse = 0;
    public static int keys = 1;
    public static int acel = 2;
}

