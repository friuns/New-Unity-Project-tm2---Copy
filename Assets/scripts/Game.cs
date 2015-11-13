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
public partial class Game : GuiClasses
{
    public GUITexture cursorTexture2;
    public GUITexture cursorTexture;
    public Player testPlayer;
    public Player testBot;
    //public GUIText scoreText;
    public ParticleEmitter smoke;
    public ParticleSystem smoke2;
    public float[] lightninigs = new float[0];
    public ParticleSystem[] dirt;
    public float customTime = 1;
    public new Res res;
    public bool startedOrFinnished { get { return gameState != GameState.none; } }
    public bool started { get { return gameState == GameState.started; } }
    internal bool finnish { get { return gameState == GameState.finnish; } }
    internal bool none { get { return gameState == GameState.none; } }
    public GameState gameState;
    public GameObject ctf;
    internal List<Zombie> zombies = new List<Zombie>();
    public GUITexture iosMenu;
    internal Transform m_StartPos;
    internal Vector3 startPosOffset;
    [Obsolete]
    private Vector3 SpawnDmSpawn;
    internal Transform StartPos
    {
        get
        {
            startPosOffset = Vector3.zero;
            if (_Loader.team && !_Loader.ctfRandomSpawn)
                return setting.autoConnect || setting.autoHost || !_Player || _Player.replay.red ? redFlagPos : blueFlagPos;
            if (_Loader.dmNoRace || _Loader.pursuit)
            {
                var spawns = GameObject.FindGameObjectsWithTag(Tag.Spawn);
                if (spawns.Length > 0)
                    return spawns[Random.Range(0, spawns.Length)].transform;
                if (setting.autoConnect || setting.autoHost)
                    return m_StartPos;
                var b = GetLevelBounds();
                for (int i = 0; i < 50; i++)
                {
                    var p = b.min + new Vector3(Random.Range(0, b.size.x), b.size.y, Random.Range(0, b.size.z));
                    RaycastHit h;
                    if (Physics.Raycast(p, Vector3.down, out h, 1000, Layer.levelMask))
                    {
                        if (h.transform.renderer && !dontSpawnMaterials.Contains(h.transform.renderer.sharedMaterial.name))
                        {
                            Debug.DrawRay(p, Vector3.down * 1000, Color.red, 10);
                            var g = new GameObject("st");
                            g.transform.position = h.point + Vector3.up * 4;
                            g.transform.rotation = Quaternion.LookRotation(ZeroY(Random.insideUnitSphere));
                            Destroy(g, .1f);
                            return g.transform;
                        }
                    }
                }
                return m_StartPos;
                //if (Random.value < 1f / (checkPoints.Length + 2) || isDebug)
                //    return m_StartPos;
                //startPosOffset = Vector3.up * 4;
                //return checkPoints[Random.Range(0, checkPoints.Length)].transform;
            }
            return m_StartPos;
            //return _Loader.dm
            //    ? Random.value < 1f/(checkPoints.Length + 2) || isDebug
            //        ? m_StartPos
            //        : checkPoints[Random.Range(0, checkPoints.Length)].transform.position
            //    : m_StartPos;
        }
        set { m_StartPos = value; }
    }
    public Transform blueFlagPos
    {
        get
        {
            var blue = GameObject.FindGameObjectWithTag(Tag.blueSpawn);
            if (blue)
                return blue.transform;
            var flagPos =
                checkPoints.OrderByDescending(a => (m_StartPos.position - a.transform.position).magnitude).First().transform;
            startPosOffset = Vector3.up * 4;
            return flagPos;
        }
    }
    public Transform redFlagPos
    {
        get
        {
            GameObject red = GameObject.FindGameObjectWithTag(Tag.redSpawn);
            if (red)
                return red.transform;
            return m_StartPos;
        }
    }
    //internal Quaternion StartRot;
    private float lastRecord;
    public int place;
    internal Player finnishedPlayer;
    internal bool backTime;//{ get { return m_backTime || Time.timeScale <= 0; } set { m_backTime = value; } }
    internal bool backTime2 { get { return Time.timeScale <= 0 || !_Game.started; } }
    //internal bool m_backTime;
    private bool wonRecord;
    private bool wonMedal;
    //private AudioReverbZone reverbZone;
    private WWW www;
    internal Player m_Player2;
    internal Player m_Player;
    public List<Player> listOfPlayers = new List<Player>();
    public Dictionary<int, Player> photonPlayers = new Dictionary<int, Player>();
    internal List<bs> finnishedPlayers = new List<bs>();
    public List<Collider> triggers = new List<Collider>();
    public GUITexture textureCenter;
    public GUIText centerText2;
    public GUIText voiceChatText;
    internal int FrameCount;
    private bool replaysLoaded;
    internal bool editControls;
    internal bool enableKeys = true;
    public ParticleEmitter rainfall;
    public ParticleEmitter[] splash;
    public ParticleEmitter[] sparks;
    public ParticleEmitter[] bulletHit;
    public void Emit(Vector3 point, Vector3 normal, ParticleEmitter[] ParticleEmitters)
    {
        for (int i = 0; i < ParticleEmitters.Length; i++)
        {
            var b = ParticleEmitters[i];
            if (i == 0)
            {
                b.transform.position = point;
                b.transform.up = normal + Random.insideUnitSphere * .1f;
            }
            b.Emit();
        }
    }
    public void OnEnable()
    {
        print("Game OnEnable");
        _Game = this;
    }
    public override void Awake()
    {
        Debug.LogWarning("Game Awake");
        m_StartPos = new GameObject("StartPos").transform;
        checkPoints = GameObject.FindGameObjectsWithTag(Tag.CheckPoint);
        _Game = this;
        _Loader.game = this;
        //_Game = this;
        if (!string.IsNullOrEmpty(Application.loadedLevelName) && string.IsNullOrEmpty(_Loader.mapName))
            _Loader.mapName = Application.loadedLevelName;
        print(_Loader.mapName);
        _Loader.prefixMapPl = _Loader.mapName + ";" + _Loader.playerName + ";";
        _Player = testPlayer ? testPlayer : (Player)FindObjectOfType(typeof(Player));
        if (_Player != null)
            _Player.gameObject.SetActive(false);
        //var mus = GameObject.FindGameObjectWithTag("music");
        //if (mus != null)
        //{
        //    music = mus.audio;
        //    music.volume = _Loader.musicVolume;
        //    music.Play();
        //}
        if (!isDebug)
            customTime = 1;
        var specular = Shader.Find("Specular");
        if (!specular)
            print("Warning specular is null");
        var dif = Shader.Find("Diffuse");
        foreach (Material m in Resources.FindObjectsOfTypeAll(typeof(Material)))
        {
            if (new[] { "42494742" }.Contains(m.name))
            {
                m.shader = dif;
            }
            if (res.animatedTextures.Contains(m.name))
                animMaterials.Add(m);
            if (specular && res.specularTextures.Contains(m.name))
            {
                m.shader = specular;
                m.SetFloat("_Shininess", .4f);
                //Debug.LogWarning("Set specular" + specular);
            }
        }
        //if(splitScreen)
        Physics.IgnoreLayerCollision(Layer.car, Layer.car, splitScreen);
        //if (!isDebug)
        //setting.useTowards = _Loader.pursuit;
    }
    public Transform cursor;
    //VectorLine line;
    public void Start()
    {
        AutoQuality.InitSkybox();
        _Loader.playedTimes++;
        cursor.parent.gameObject.SetActive(_Loader.dm);
        isCustomLevel = _MapLoader != null;
        PhotonNetwork.isMessageQueueRunning = true;
        print("!Game Start server version " + _Loader.serverVersion);
        ResetCoins();
        _Loader.wonMedals = 0;
        print("Game Loaded");
        if (_Loader.levelEditor == null)
            _Loader.gamePlayed = true;
        if (_Loader.autoFullScreen && !Screen.fullScreen)
            _Loader.FullScreen(true);
        SetStartPoint();
        win.CloseWindow();
        if (_Loader.sGameType != SGameType.Replay)
        {
            _Player = InstanciatePlayer();
            _Player.InitNetwork();
            print("set player name " + _Loader.playerName);
        }
        if (_Loader.sGameType == SGameType.Cops)
        {
            var pl = ((GameObject)Instantiate(Resources.Load("Player"), StartPos.position + startPosOffset, StartPos.rotation)).GetComponent<Player>();
            pl.teamEnum = TeamEnum.Blue;
            pl.InitNetwork();
            _Player.SetTeam((int)TeamEnum.Red);
        }
        if (_Loader.sGameType == SGameType.SplitScreen)
            _Player2 = InstanciatePlayer();
        InitLevel();
        LoadReplays();
        if (!_Loader.dmNoRace || PhotonNetwork.isMasterClient)
            RestartTime();
        StartCoroutine(AddMethod(10, delegate
        {
            if (_AutoQuality.fps <= 100)
                LogEvent(EventGroup.Fps, _Loader.quality + "." + (int)(Mathf.Min(70, _AutoQuality.fps) / 10) + "0fps");
        }));
        LogEvent(EventGroup.GameType, _Loader.sGameType + "");
        LogEvent("Load Map Finnish");
        if (splitScreen) _GameGui.enabled = false;
        if (_Loader.rain)
            rainSound.Play();
        if (_Loader.night)
        {
            Material material = new Material(RenderSettings.skybox);
            //material.color = Color.black;
            material.SetColor("_Tint", Color.black);
            RenderSettings.skybox = material;
            RenderSettings.fogColor = Color.black;
            RenderSettings.ambientLight = Color.black;
        }
        sunLight.enabled = !_Loader.night;
        foreach (var a in GameObject.FindGameObjectsWithTag(Tag.node))
            a.collider.enabled = a.renderer.enabled = false;
        //if (_MapLoader != null)
        //    _GameSettings.enableBorderHit = false;
        //reverbZone = (AudioReverbZone)FindObjectOfType(typeof(AudioReverbZone));
        StartCoroutine(_AutoQuality.OnLevelWasLoaded2(0));
        print("Set gravity " + Physics.gravity);
        Resources.UnloadUnusedAssets();
        GC.Collect();
        //green = MakeMaterial(new Color(0, 0, 1, 1));
        if (_Loader.dm && !_Loader.enableMatchTime)
            gameState = GameState.started;
        
        _Loader.scoreBoard = null;
        ctf.SetActive(_Loader.ctf);
        StartTexts();
        if ((!_Loader.screenshotTaken) && Random.value < .3f)
            StartCoroutine(AddMethod(5, delegate { _Loader.TakeScreenshot(false); }));
        //if (_Loader.enableZombies && PhotonNetwork.isMasterClient)
        SpawnZombies();
        if (!_Loader.levelEditor)
        {
            foreach (GameObject a in new[] { Tag.redSpawn, Tag.blueSpawn, Tag.Spawn, Tag.zombieSpawn }.SelectMany(a => GameObject.FindGameObjectsWithTag(a)))
                foreach (var b in a.GetComponentsInChildren<Renderer>())
                    b.enabled = false;
        }
        coins = GameObject.FindGameObjectsWithTag("coin");
        //VectorLine.SetCamera(_Player.camera);
        //line = VectorLine.SetLine(Color.green, new Vector2[60]);
        //line.MakeCircle(new Vector3(Screen.width / 2f, Screen.height / 2f), 100);
        //InitDecal();
        //Everyplay.SharedInstance.SetMetadata("map", _Loader.mapName);
        if (_Loader.dm)
            _Loader.enableMouse = false;
        finnishedPlayer = _Player;
        _Loader.curSceneDef.played = true;
        //if (setting.enableCollisionCleanup && _Loader.curSceneDef.userMap)
        //    gameObject.AddComponent<Test>();
    }
    private void StartTexts()
    {
        if (_Player.nitro > 0)
            _Game.centerText(string.Format(Tr("You have {0} seconds long Nitro, Press Shift to Use"), _Player.nitro), 3, true);
        if (_Loader.dmRace)
            _Game.centerText(Tr("Shoot others to get Nitro"), 3, true);
        if (_Loader.bombCar)
            centerText(Tr("You have BombCar, if you collide you will explode!"));
        if (_Loader.levelEditor)
            centerText(Tr("Press T to add coins"));
        if (_Loader.dmNoRace)
            centerText(string.Format(Tr("Press 1-3 to select weapon\n Press {0} to jump"), "g"));
        if (_Loader.pursuit)
        {
            centerText(Tr("Cops should catch all bandits in given time"));
            centerText(Tr("If your time is red color cops seeing found you"));
        }
    }
    //private void InitDecal()
    //{
    //    m_DecalsMesh = new DecalsMesh(m_DecalsInstance);
    //    m_DecalsMeshCutter = new DecalsMeshCutter();
    //    m_WorldToDecalsMatrix = m_DecalsInstance.CachedTransform.worldToLocalMatrix;
    //}
    //public DS_Decals m_DecalsInstance;
    //internal DecalsMesh m_DecalsMesh;
    //internal Matrix4x4 m_WorldToDecalsMatrix;
    //internal DecalsMeshCutter m_DecalsMeshCutter;
    private void SpawnZombies()
    {
        var spawns = GameObject.FindGameObjectsWithTag(Tag.zombieSpawn);
        print("zombe spawns:" + spawns.Length);
        if (_Loader.levelEditor != null) return;
        if (spawns.Length > 0)
        {
            foreach (var a in spawns)
            {
                var zombie = (GameObject)Instantiate(LoadRes("zombie"));
                zombie.transform.position = a.transform.position + Vector3.up;
                zombie.transform.forward = ZeroY(Random.insideUnitSphere);
            }
        }
        else if (_Loader.enableZombies && PhotonNetwork.isMasterClient && _Loader.menuLoaded)
        {
            print("Spawn Zombies");
            var b = GetLevelBounds();
            Random.seed = 0;
            for (int i = 0, j = 0; i < 50 && j < 1000; j++)
            {
                var posvel = b.min + new Vector3(Random.Range(0, b.size.x), b.size.y, Random.Range(0, b.size.z));
                RaycastHit h;
                if (Physics.Raycast(posvel, Vector3.down, out h, 1000, Layer.levelMask))
                {
                    if (!dontSpawnMaterials.Contains(h.transform.renderer.sharedMaterial.name))
                    {
                        i++;
                        Debug.DrawRay(posvel, Vector3.down * 1000, Color.green, 10);
                        PhotonNetwork.InstantiateSceneObject("zombie", h.point + Vector3.up, Quaternion.LookRotation(ZeroY(Random.insideUnitSphere)), 0, null);
                        //var zombie = (GameObject)Instantiate(LoadRes("zombie"));
                        //zombie.transform.position = h.point + Vector3.up;
                        //zombie.transform.forward = ZeroY(Random.insideUnitSphere);
                    }
                }
            }
        }
    }
    string[] dontSpawnMaterials = new[] { "ec324e91", "0d119bcc", "9eadb28a", "42494742" };
    private static Bounds GetLevelBounds()
    {
        Bounds b = new Bounds();
        foreach (var a in FindObjectsOfType<MeshCollider>())
            b.Encapsulate(a.bounds);
        return b;
    }
    public void centerText(string s, float seconds = 4, bool noAndroid = true)
    {
        if (noAndroid && android) return;
        _Loader.centerText(s, Mathf.Max(.3f, seconds));
    }
    public void ResetCoins()
    {
        //Debug.LogError("<<<<<<<<<<<<<<<<<<<<<<");
        foreach (var a in coins)
            a.gameObject.SetActive(true);
        textureCenter.enabled = false;
    }
    public void SetStartPoint()
    {
        if (_MapLoader != null)
        {
            _MapLoader.UpdateMinimap();
            _GameSettings.miny = _MapLoader.water.transform.position.y - 4;
        }
        triggers = new List<Collider>();
        TagMe(Tag.CheckPoint);
        TagMe(Tag.speed);
        TagMe(Tag.engineOff);
        TagMe(Tag.noSkid);
        print("SetStartPoint");
        var st = GameObject.FindGameObjectWithTag(Tag.Start);
        if (st == null) st = GameObject.Find("Start");
        if (st != null)
        {
            st.SetActive(false);
            StartPos = st.transform;
        }
        else
            Debug.LogWarning("Start not found");
        if (_MapLoader != null && _MapLoader.terrain != null)
        {
            RaycastHit h2;
            _MapLoader.terrain.collider.Raycast(new Ray(StartPos.position + Vector3.up * 500, Vector3.down), out h2, 1000);
            var startPos = StartPos.position;
            startPos.y = Mathf.Max(h2.point.y + 3, StartPos.position.y);
            StartPos.position = startPos;
        }
        checkPoints = GameObject.FindGameObjectsWithTag(Tag.CheckPoint);
        //if (_MapLoader != null)
        //    _MapLoader.RefreshSplines();
        //foreach (MeshCollider a in FindObjectsOfType(typeof (MeshCollider)))
        //    a.smoothSphereCollisions = true;
    }
    //public float nitro = 0;
    public Light sunLight;
    List<Material> animMaterials = new List<Material>();
    public void SendWmp(string c, string p)
    {
#if UNITY_EDITOR && !UNITY_WEBPLAYER
        if (setting.sendWmp)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                TcpClient tcp = new TcpClient("127.0.0.1", 3405);
                var s = tcp.GetStream();
                var sw = new StreamWriter(s);
                {
                    sw.WriteLine(c);
                    sw.WriteLine(p);
                    sw.Flush();
                }
                //tcp.Close();
            }
            );
        }
#endif
    }
    public bool isCustomLevel;
    private bool splitScreenCollision;
    // { get { return isDebug; } }
    public void Update()
    {
        mainCamera = Camera.main;
        mainCameraTransform = mainCamera.transform;
        if (Loader.errors > 100 && !Debug.isDebugBuild && !_Loader.levelEditor)
        {
            if (online)
                PhotonNetwork.LeaveRoom();
            _Game.Exit();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
            ShowScoreBoard();
        if (KeyDebug(KeyCode.E))
            _Player.EndGame();
        if (online)
        {
            //AudioListener.pause = Time.timeSinceLevelLoad < 2;
            if (listOfPlayers.Where(a => a != null).Count(a => !a.enabled) > 2 && Time.timeSinceLevelLoad > 5)
                Debug.LogException(new System.Exception("server sync error"));
        }
        if (setting.timeLapse)
            FrameCount = (int)(timeElapsed / Time.fixedDeltaTime);
        if (splitScreen)
        {
            splitScreenCollision = started && _Game.timeElapsed > 2 && ((_Player2.pos - _Player.pos).magnitude > 5 || splitScreenCollision);
            Physics.IgnoreLayerCollision(Layer.car, Layer.car, !splitScreenCollision);
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) && _Loader.levelEditor == null)
            Debug.Break();
        if (_Loader.levelEditor && Input.GetKeyDown(KeyCode.T))
        {
            var g = (GameObject)Instantiate(res.coin, _Player.pos + Vector3.up, Quaternion.identity);
            g.name = res.coin.name;
            g.AddComponent<ModelObject>();
        }
        if (Input.GetMouseButtonDown(0) && !win.enabled && (_Loader.dm || _Player.freeCamera) && !_Player.forwardMode)
            Screen.lockCursor = true;
        if (setting.lagPerf)
            Thread.Sleep(Random.Range(0, 100));
        _Game.timeElapsedLevel = _Game.timeElapsedLevel + (online && backTime || setting.timeLapse ? _Loader.deltaTime : Time.deltaTime);
        //print(_Loader.mapName);
        //foreach (AudioReverbZone a in FindObjectsOfType(typeof(AudioReverbZone)))
        //{
        //    //print(a.density);
        //    a.density = .1f;
        //}     
        if (KeyDebug(KeyCode.B, "Enable Keys"))
            InputManager.enableKeys = !InputManager.enableKeys;
        //print(Input.GetAxis("Vertical"));
        //print(Input.GetAxis("Vertical2"));
        //_Game = this;
        if (!win.enabled)
            editControls = false;
        if (_Loader.race && !splitScreen)
            Time.timeScale = win.enabled || _GameGui.chatEnabled || _Game.backTime || finnish ? 0 : customTime;
        if (Time.timeScale != 0 && Time.timeScale != 1)
            Time.fixedDeltaTime = _AutoQuality.fixedDeltaTime * Time.timeScale;
        //if (KeyDebug(KeyCode.G, " EndGame"))AGAGAGAGAG
        //if (Hotkey())
        //{
        //    wonRecord = true;
        //    debugEndGame = true;
        //    _Player.EndGame();
        //}
        //if (_Loader.stunts && !finnish && timeElapsed > _GameSettings.levelTime && PhotonNetwork.isMasterClient ||KeyDebug(KeyCode.T))
        //photonView.RPC("EndGame3",PhotonTargets.All);
        //CallRPC(EndGame3);
        //if (_Loader.levelEditor && _Player.score > 10 && _Loader.levelEditor.submitMapPublish)
        //EndGame(_Player);
        //if(KeyDebug(KeyCode.R))
        //    Application.LoadLevel(_Loader.mapName);
        //else
        if (input.GetKeyDown(KeyCode.R))
        {
            if (_Loader.dmNoRace && online)
            {
                if (Time.time - _Player.upsideDown < .1f)
                {
                    RaycastHit h;
                    if (Physics.Raycast(_Player.pos, Vector3.down, out h, 3, Layer.levelMask))
                        _Player.transform.up = h.normal;
                }
                _Player.SetLife(_Player.life - 50, myId);
            }
            else if (startedOrFinnished && _Loader.race)
                RestartLevel();
        }
        iosMenu.enabled = !win.enabled || editControls;
        var iosEsc = _Player != null && iosMenu.enabled && iosMenu.HitTest(Input.mousePosition, _Player.hud.camera) &&
                     Input.GetMouseButtonDown(0);
        if (iosEsc || input.GetKeyDown(KeyCode.Escape))
        {
            if (win.act != MenuWindow)
                win.ShowWindow(MenuWindow, null, true);
            else
                win.CloseWindow();
        }
        if (input.GetKeyDown(KeyCode.O))
            _Loader.fieldOfView += 5;
        if (input.GetKeyDown(KeyCode.P))
            _Loader.fieldOfView -= 5;
        if (TimeElapsed(1))
            _Game.listOfPlayers.Sort(_Player);
        //if (music != null)
        //music.audio.volume = _Loader.musicVolume;
        //int i = 0;
        var low = _Loader.quality < Quality2.High && android || lowQuality;
        var online1 = online && !android;
        leftText.enabled = low && !splitScreen && !online1;
        _GameGui.enabled = !low && !splitScreen || online1;
        if (low && FramesElapsed(10))
        {
            var text = "";
            var b = _Loader.PlayersCount < 1 && !online;
            for (int i = 0; i < (b ? _Loader.replays.Count : _Game.listOfPlayers.Count); i++)
            {
                var r = b ? _Loader.replays[i] : _Game.listOfPlayers[i].replay;
                r.place = i + 1;
                text += r.getText() + "\n";
            }
            //foreach (Player pl in _Game.players)
            //{
            //    i++;
            //    text += string.Format("{0}. {1} {2}\r\n", i, pl.playerName, (pl.finnished ? TimeToStr(pl.finnishTime) : ""));
            //    pl.place = i;
            //}
            leftText.text = text;
        }
        foreach (var a in animMaterials)
            if (a.HasProperty("_MainTex"))
            {
                Vector2 mainTextureOffset = a.mainTextureOffset;
                mainTextureOffset.x += Time.deltaTime;
                a.mainTextureOffset = mainTextureOffset;
            }
        UpdateRain();
        UpdateAwards();
        if (terrainBig && _Loader.mapLoader)
            terrainBig.position = _Player.pos;
    }
    public void ShowScoreBoard()
    {
        if (offlineMode)
            _Loader.LoadScoreBOard();
        else
        {
            var playersWindow = scoreBoardWindow;
            if (win.act == playersWindow)
            {
                win.CloseWindow();
                Screen.lockCursor = true;
            }
            else
                ShowWindow(playersWindow);
        }
    }
    public Action scoreBoardWindow { get { return _Loader.pursuit ? _MpGame.PursuitWindow : _Loader.dmOrPursuit ? _MpGame.PlayersWindow : (Action)_MpGame.DrawScoreBoard; } }
    private bool gotDmAward;
    internal float topDownTime;
    private void UpdateAwards()
    {
        if (_Player.topdown)
            topDownTime += Time.deltaTime;
        if (online && !gotDmAward && _Loader.PlayersCount > 2 && listOfPlayers.IndexOf(_Player) == 0 && (_Player.score > 0 || _Player.finnishTime > 0))
        {
            gotDmAward = true;
            if (_Loader.dmNoRace)
                _Awards.DeathMatchOrCtf.Add();
            else if (_Loader.race)
                _Awards.WinInMultiplayerRace.Add();
        }
    }
    public int rewindsUsed;
    internal GameObject[] coins = new GameObject[0];
    private void UpdateRain()
    {
        RaycastHit h;
        var raycast = Physics.Raycast(_Player.pos + Vector3.up, Vector3.up, out h, 300, Layer.levelMask);
        var reverbZone = _Player.reverbZone;
        if (reverbZone.enabled)
        {
            var audioReverbPreset = raycast ? h.distance > 50 ? AudioReverbPreset.Hangar : AudioReverbPreset.User : AudioReverbPreset.Generic;
            reverbZone.decayHFRatio = 0;
            if (audioReverbPreset != reverbZone.reverbPreset)
            {
                if (audioReverbPreset == AudioReverbPreset.User)
                {
                    reverbZone.reverbPreset = AudioReverbPreset.Drugged;
                    //reverbZone.decayHFRatio = .35f;
                }
                reverbZone.reverbPreset = audioReverbPreset;
                //reverbZone.maxDistance = 3000 + Random.value;
            }
        }
        rainfall.enabled = _Loader.rain;
        if (_Loader.rain)
        {
            if (_Player != null && _Loader.rain)
                rainfall.renderer.material.shader = raycast ? res.rainShader2 : res.rainShader;
            foreach (float key in lightninigs)
            {
                var time = rainSound.time % rainSound.clip.length;
                if (key >= time - Time.deltaTime && key < time)
                    StartCoroutine(Lightning());
            }
            rainfall.transform.position = _Player.rainfall.position;
        }
    }
    static Vector3[] triangle = { new Vector3(-1.5f, -2), new Vector3(1.5f, -2), new Vector3(0, 2) };
    static Vector3[] quad = { new Vector3(-1, -1), new Vector3(1, -1), new Vector3(1, 1), new Vector3(-1, 1) };
    public void OnRenderObject()
    {
        if (!isCustomLevel) return;
        if (_Player == null || _Player.camera != Camera.current) return;
        GL.PushMatrix();
        res.lineMaterialYellow.SetPass(0);
        GL.LoadOrtho();
        GL.Color(android ? Color.black : Color.yellow);
        GL.Begin(GL.LINES);
        Vector3? last = null;
        for (int i = 0; i < _MapLoader.minimap.Count; i++)
        {
            Vector2 a = _MapLoader.minimap[i];
            if (last != null && a != Vector2.zero && last != Vector3.zero)
            {
                GL.Vertex(last.Value);
                GL.Vertex(a);
            }
            last = a;
        }
        GL.End();
        GL.Begin(GL.TRIANGLES);
        var min = 300;//Mathf.Min(300, _GameSettings.levelBounds.size.magnitude);
        var ratio = ((float)Screen.width / Screen.height);//(_MapLoader.levelBounds.size.x / _MapLoader.levelBounds.size.z) 
        foreach (Player a in listOfPlayers)
        {
            GL.Color(GameGui.colors[a.replay.textColor]);
            DrawQuad(a.transform, ratio, min, triangle);
        }
        GL.End();
        GL.Begin(GL.QUADS);
        //print(checkPoints.Length);
        GL.Color(Color.yellow);
        foreach (GameObject a in checkPoints)
            DrawQuad(a.transform, ratio, min, quad);
        GL.Color(Color.green);
        foreach (var a in _Player.checkPointsPass)
            DrawQuad(a, ratio, min, quad);
        GL.End();
        GL.PopMatrix();
    }
    private static void DrawQuad(Transform a, float ratio, float min, Vector3[] vertex)
    {
        var p = MapLoader.ResizeToMinimap(a.position);
        for (int i = 0; i < vertex.Length; i++)
        {
            var v2 = a.TransformDirection(new Vector3(vertex[i].x, 0, vertex[i].y));
            //v2 = _Player.camera.transform.worldToLocalMatrix* a.transform.localToWorldMatrix * new Vector3(vct[i].x, 0, vct[i].y);
            v2 = new Vector3(v2.x, v2.z * ratio);
            GL.Vertex((Vector3)p + v2 / min);
        }
    }
    internal GameObject[] checkPoints;
    public Light lightning;
    public AudioSource rainSound;
    public IEnumerator Lightning()
    {
        print("lightbning");
        var turn = (Random.value > .6f ? 2 : 1);
        lightning.transform.forward = Vector3.down + Random.insideUnitSphere;
        for (int i = 0; i < turn; i++)
        {
            lightning.enabled = true;
            yield return new WaitForSeconds(.2f);
            lightning.enabled = false;
            yield return new WaitForSeconds(.2f);
        }
    }
    public GUIText leftText;
    //private AudioSource music;
    public void FixedUpdate()
    {
        if (setting.lagPerf)
            Thread.Sleep(Random.Range(0, 10));
        if (started && !setting.timeLapse)
            FrameCount++;
    }
    public void OnLeftRoom()
    {
        if (_Loader.dmNoRace)
        {
            _Loader.warScore += Mathf.Max((int)_Player.score, 0);
            _Awards.xp.Add((int)(_Player.score / 3));
            SendReplay(null, _Player.score);
        }
        //_Awards.OnEndGame();
        Exit();
    }
    public void MenuWindow()
    {
        Setup(300, 500, Trs("Menu") + " " + _Loader.mapName, Dock.Center);
        if (Button("Resume"))
            win.CloseWindow();
        if (Button("FullScreen"))
            _Loader.FullScreen(true);
        if (editControls)
        {
            win.style = GUIStyle.none;
            Label("Now Drag and Scale Icons, press back button when done", 16, true);
            return;
        }
        if (online)
            _MpGame.MenuWindow();
        if (Button("Settings"))
            win.ShowWindow(_Loader.SettingsWindow, MenuWindow);
        if (!_Loader.dmNoRace && Button("Restart"))
            _Game.RestartLevel();
        if (Button("Scoreboard"))
            ShowScoreBoard();
        DrawPlayMusicButton();
        if (isMod)
            RateButton(MenuWindow);
        if (_Loader.levelEditor != null && Button("Back To Editor"))
            StartCoroutine(_Loader.levelEditor.Resume());
        if (_Loader.levelEditor == null && Button("Quit"))
            if (!_Loader.menuLoaded)
                ApplicationQuit();
            else
            {
                if (online)
                    PhotonNetwork.LeaveRoom();
                else
                    Exit();
            }
        //if (isDebug && Button("Replay"))
        //    EndGameReplay();
        if (android && Button("Change Camera") && !_Loader.topdown)
            _Player.ChangeCamera(_Player.curCam + 1);
        if (android && online && Button("Chat"))
            StartCoroutine(_GameGui.OpenAndroidChat());
        _AutoQuality.DrawDistance();
    }
    private void DrawPlayMusicButton()
    {
        if ((Time.time - Music.broadCastTime > 60 || isDebug) && Button("Play music"))
        {
            string musicUrl = "";
            ShowWindow(delegate
            {
                musicUrl = TextArea("mp3 url:", musicUrl);
                if (Button("Load"))
                {
                    _music.LoadMusic(musicUrl, true);
                    win.CloseWindow();
                }
            });
        }
    }
    internal Player rewindingPlayer;
    public void OnDisconnectedFromPhoton()
    {
        if (online)
            Exit();
    }
    private void Exit()
    {
        LoadLevel(Levels.menu);
    }
    private Player InstanciatePlayer()
    {
        if (_Player != null /*&& !_Player.gameObject.activeSelf */&& !_Loader.menuLoaded)
        {
            _Player.gameObject.SetActive(true);
            StartPos.position = _Player.pos;
            return _Player;
        }
        else
        {
            //var instanciatePlayer = PhotonNetwork.Instantiate("Player", StartPos, StartRot, 0).GetComponent<Player>();
            //print("var instanciatePlayer = PhotonNetwork.Instantiate, StartPos, StartRot, 0).GetComponent<Player>();"+instanciatePlayer);
            Player pl = online ? PhotonNetwork.Instantiate("Player", StartPos.position + startPosOffset, StartPos.rotation, 0).GetComponent<Player>() :
                ((GameObject)Instantiate(Resources.Load("Player"), StartPos.position + startPosOffset, StartPos.rotation)).GetComponent<Player>();
            return pl;
        }
    }
    public List<Replay> ghosts = new List<Replay>();
    private void LoadReplays()
    {
        if (replaysLoaded) return;
        print("Ghosts " + ghosts.Count);
        if (_Loader.replays.Count > 0)
        {
            _Loader.replays.Sort(_Loader.replays[0]);
            for (int i = _Loader.replays.Count - 1, j = 1; i >= 0; i--, j++)
            {
                var replay = _Loader.replays[i];
                if (j > _Loader.PlayersCount && vsPlayersOrSplitscreen) break;
                AddGhost(replay);
            }
        }
        if (_Loader.showYourGhost)
            foreach (var a in ghosts)
                AddGhost(a);
    }
    private void AddGhost(Replay replay)
    {
        if (Any(replay))
            return;
        replaysLoaded = true;
        Player gh = InstanciatePlayer();
        gh.playerNameTxt.text = Trn(replay.playerName);
        gh.ghost = true;
        //var carSkin = Random.Range(0, _Loader.CarSkins.Count);
        gh.replay = replay;
        //if (gameType == GameType.VsPlayers && replay.playerName != _Loader.playerName && !_Loader.GetCarSkin(carSkin).special && replay.carSkin == 0)
        //replay.carSkin = carSkin;
        //gh.playBackTime = Random.Range(.9f, 1f);
    }
    private bool Any(Replay replay)
    {
        foreach (Player a in listOfPlayers)
            if (a.replay == replay) return true;
        return false;
    }
    public override void OnEditorGui()
    {
#if UNITY_EDITOR
        if (gui.Button("Set Finnish Pos"))
        {
            _Loader.curScene.FinnishPos = Selection.activeTransform.position;
            SetDirty(res);
        }
#endif
        if (GUILayout.Button("Clear Player Prefs"))
            PlayerPrefs.DeleteAll();
        base.OnEditorGui();
    }
    private void InitLevel()
    {
        //if (!isCustomLevel)
        foreach (MeshCollider a in FindObjectsOfType(typeof(MeshCollider)))
        {
            a.smoothSphereCollisions = true;
            _GameSettings.levelBounds.Encapsulate(a.bounds);
            //a.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
        //if (!isDebug)
        Initbackground();
        //print("NINIANSDISJHDAFSFJ;BGAHSFJKLAHSFBGA");
        //if (!android && !flash)
        try
        {
            //InitMarmoset(); 
            InitSpecular();
        }
        catch (Exception e) { Debug.LogError(e.Message); }
    }
    private void Initbackground()
    {
        if (_Loader.autoQuality || mediumAndroidHigh)
        {
            var original = (GameObject)bs.LoadRes("TerrainBig");
            if (original != null)
            {
                Vector3 position;
                if (_Loader.mapLoader != null)
                    position = Vector3.up * 108.1349f;
                else
                {
                    RaycastHit h;
                    var hit = Physics.SphereCast(StartPos.position + Vector3.down * 1000, 100, Vector3.up, out h, 2000,
                        Layer.levelMask);
                    Vector3 terrainOffset = new Vector3(-1000, -300, -1000);
                    position = (hit ? h.point + Vector3.up : StartPos.position) + terrainOffset;
                }
                terrainBig = ((GameObject)Instantiate(original, position, Quaternion.identity)).transform;
            }
        }
    }
    public Transform terrainBig;
    private void InitSpecular()
    {
        print("Init specular");
        //        if (!flash && !isDebug)
        //        {
        //            var sk = RenderSettings.skybox;
        //            Texture2D texture2D = ((Texture2D)sk.GetTexture("_FrontTex"));
        //            cubeMap = new Cubemap(texture2D.width, TextureFormat.RGB24, true);
        //            cubeMap.SetPixels(getPixels(sk, "_FrontTex"), CubemapFace.PositiveZ);
        //            cubeMap.SetPixels(getPixels(sk, "_BackTex"), CubemapFace.NegativeZ);
        //            cubeMap.SetPixels(getPixels(sk, "_LeftTex"), CubemapFace.NegativeX);
        //            cubeMap.SetPixels(getPixels(sk, "_RightTex"), CubemapFace.PositiveX);
        //            cubeMap.SetPixels(getPixels(sk, "_UpTex"), CubemapFace.PositiveY);
        //            cubeMap.SetPixels(getPixels(sk, "_DownTex"), CubemapFace.NegativeY);
        //            cubeMap.mipMapBias = 4;
        //            cubeMap.Apply(true, true);
        //#if UNITY_EDITOR
        //            AssetDatabase.CreateAsset(cubeMap, "Assets/cubemaps/SkyCubeMap.cubemap");
        //#endif
        //        } 
        //RenderSettings.skybox.mainTexture = cubeMap;
        foreach (Renderer r in FindObjectsOfType(typeof(MeshRenderer)))
        {
            var layer = r.gameObject.layer;
            if ((layer == Layer.level || layer == Layer.cull || layer == Layer.block))
                LevelRenderers.Add(r);
        }
        _AutoQuality.UpdateMaterials();
    }
    internal List<Renderer> LevelRenderers = new List<Renderer>();
    private string olds;
    public void RestartLevel()
    {
        if (!startedOrFinnished) return;
        if (splitScreen)
            Physics.IgnoreLayerCollision(Layer.car, Layer.car);
        Debug.LogWarning("RestartLevel");
        //var f = _Player.finnishTime;
        wonRecord = false;
        Time.timeScale = 1;
        wonMedal = false;
        ClearLog();
        //foreach (var a in players)
        //    Destroy(a.gameObject);
        //players.Clear();
        finnishedPlayers.Clear();
        //finnish = false;
        //started = false;
        gameState = GameState.none;
        if (online)
            _Player.Reset();
        else
            foreach (Player a in listOfPlayers)
            {
                a.Reset();
                a.finnished = false;
                a.oldpos = pos;
                a.checkPointsPass.Clear();
                backTime = false;
            }
        if (_Loader.dm)
            foreach (Player a in listOfPlayers)
                a.score = 0;
        //_Player.replay.posVels.Clear();
        _Player.replay = _Player.replay.CloneAndClear(); // new Replay() { avatarId = _Player.replay.avatarId, playerName = _Player.playerName, pl = _Player, carSkin = _Player.replay.carSkin, contry = _Player.replay.contry };
        //if (!online)
        //    _Player.replay.finnishTime = 0;
        replaysLoaded = false;
        if (_Loader.enableCollision && _Loader.showYourGhost && _Loader.levelEditor == null && !online)
            LoadReplays();
        backTime = false;
        if (!_Loader.dmNoRace)
            RestartTime();
        FrameCount = 0;
        //Start();
        //    _Player.replay.finnishTime = _Player.finnishTime = f;
        win.CloseWindow();
        //LoadLevel(_Loader.mapName);
        //if (_Loader.menuLoaded)
        //    LoadLevelAdditive(Levels.game);
        ResetCoins();
#if GA
        GA.API.Quality.NewEvent("Restart " + _Loader.mapName, _Player.pos);
#endif
        //#if oldSunts
        if (!online)
        {
            //if (_Loader.enableZombies)
            foreach (var a in zombies)
                a.ResetZombie();
            Zombie.zombieKills = 0;
        }
        //#endif
        if (_Loader.dm)
            _Awards.ResetAwards();
    }
    private List<Replay> endgamereplays;
    //[RPC]
    //public void EndGame3()
    //{
    //    EndGame(_Player);
    //}
    public void EndGame()
    {
        if (!online)
            _Game.ghosts.Add(_Player.replay);
#if oldStunts
        if (_Loader.stunts)
        {
            if (online)
                _Player.CallRPC(_Player.SetScore2, (float) _Player.score);
            else
                foreach (var a in listOfPlayers.Where(a => a.ghost))
                    a.score = a.replay.posVels[a.replay.posVels.Count - 1].score;
        }
#endif
        _Game.listOfPlayers.Sort(_Player);
        gameState = GameState.finnish;
        lastRecord = _Loader.record;
        endgamereplays = online ? _Game.listOfPlayers.Select(a => a.replay).ToList() : _Loader.replays;
        if (_Loader.race)
        {
            if (_MapLoader == null && !_Loader.curSceneDef.userMap)
                LogEvent(EventGroup.Maps, _Loader.mapName + "Finnished");
            if (finnishedPlayer.finnishTime < _Loader.record || _Loader.levelEditor != null || isDebug)
            {
                if (online)
                    SendReplay(null, _Player.replay.finnishTime);
                else if (!splitScreen && !guest && !_Loader.enableCollision)
                    try
                    {
                        WriteSaveReplay();
                    }
                    catch (Exception e) { Debug.LogError(e); }
                _Loader.record = timeElapsed;
                wonRecord = true;
            }
            if (listOfPlayers.Count > 1 && !online)
                place = finnishedPlayers.Count;
            else
            {
                place = endgamereplays.Count(a => a.finnishTime != 0 && a.finnishTime < _Player.finnishTime && a != _Player.replay) + 1;
            }
            var place2 = place;
            if (place2 > 3) place2 = 3;
            if (endgamereplays.Count == 0) place2 = 3;
            if (place2 < 4)
            {
                PlayOneShotGui(bs.res.winSound);
                if (place2 < _Loader.place || _Loader.place == 4 || isDebug || online)
                {
                    wonMedal = true;
                    var medals = Mathf.Abs(_Loader.place - place2);
                    print("won " + medals);
                    _Loader.wonMedals = medals;
                    _Loader.medals += medals;
                    _Loader.place = place2;
                }
            }
        }
        //#if oldStunts
        else if (_Loader.dmNoRace)
        {
            place = listOfPlayers.Count(a => a.score > _Player.score && a != _Player) + 1;
            if (place < 4)
            {
                if (place < _Loader.place)
                {
                    wonMedal = true;
                    var medals = Mathf.Abs(_Loader.place - place);
                    print("won " + medals);
                    _Loader.wonMedals = medals;
                    _Loader.medals += medals;
                    _Loader.place = place;
                }
            }
        }
        //#endif
        _Game.listOfPlayers.Sort(_Player);
        print("wonRecord" + wonRecord);
        StartCoroutine(EndGame2());
        SaveStrings();
    }
    public void OnDisable()
    {
        //if (!isDebug)
        //SavePlayerPrefs(); 
    }
    //internal int coinsTime = 10;
    public IEnumerator EndGame2()
    {
        print("_Loader.levelEditor " + _Loader.levelEditor);
        print((!PlayerPrefsGetBool("voted" + _Loader.mapName) || isDebug));
        _Awards.UpdateXpOnEndGame();
        if (isCustomLevel && _Loader.levelEditor == null && (!PlayerPrefsGetBool("voted" + _Loader.mapName) && !guest || isDebug))
            yield return StartCoroutine(win.ShowWindow2(_Loader.RateMapWindow));
        //if (isDebugAwards && _Awards.awards.Any(a => a.local > 0))
        //yield return StartCoroutine(win.ShowWindow2(_Awards.WonAwardsWindow));
        if (_Loader.levelEditor != null)
            StartCoroutine(_Loader.levelEditor.Resume());
        else if (wonMedal || _Awards.awards.Any(a => a.local > 0))
            win.ShowWindow(WonMedalWindow);
        else
            win.ShowWindow(EndGameWindow);
        yield return null;
    }
#if old
    private void EndGameReplay()
    {
        started = false;
        win.CloseWindow();
        finnish = false;
        finnishedPlayers.Clear();
        foreach (var a in listOfPlayers)
            a.StartCoroutine(a.Reset());
        FrameCount = 0;
        timeElapsedLevel = 0;
        StartCoroutine(RestartTime());
    }
#endif
    private void EndGameWindow()
    {
        win.Setup(550, 450, string.Format(Tr("Track {0} Complete!"), _Loader.mapName), Dock.Center, null, null);
        //BeginScrollView();
        if (!android)
            gui.Space(50);
        if (www != null && !setting.vk2)
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                //Label(Trn("error: ") + www.error, 10);
            }
            else if (www.isDone)
                Label(Trn("Done: ") + www.text, 10);
            else
                Label(Trn("uploading: ") + (int)(www.uploadProgress * 100f), 10);
        }
        skin.label.fontSize = 20;
        gui.Label(GUIContent(finnishedPlayer.playerNameClan, finnishedPlayer.avatar));
        if (endgamereplays.Count > 0)
            win.LabelAnim(Tr(Ordinal(place)) + " " + Trs("place") + " /" + (endgamereplays.Count + 1), 18, true);
        Label(Trs("Retries: ") + finnishedPlayer.replay.backupRetries);
        if (_Loader.race)
        {
            if (wonRecord)
                win.LabelAnim("You Have new personal record", 14, true);
            win.LabelAnim(Trs("Current Time:").PadRight(20) + TimeToStr(finnishedPlayer.finnishTime));
            if (lastRecord != float.MaxValue)
                win.LabelAnim(Trs("Last Time:").PadRight(20) + TimeToStr(lastRecord));
        }
        if (_Player.score > 0)
            win.LabelAnim(Trs("Score:").PadRight(20) + _Player.scoreInt);
        if (_Loader.dmRace)
            win.LabelAnim(Trs("Damage dealt:").PadRight(20) + _Awards.damageDeal);
        if (_Loader.enableZombies)
            win.LabelAnim(Trs("Zombies Killed:").PadRight(20) + Zombie.zombieKills + "/" + _Game.zombies.Count);
        if (_Player.coins > 0)
            win.LabelAnim(Trs("Coins collected:").PadRight(20) + _Player.coins + "/" + _Game.coins.Length);
        gui.FlexibleSpace();
        gui.BeginHorizontal();
        if (!_Loader.dmNoRace && Button("Restart"))
            _Game.RestartLevel();
        if (_Loader.dmNoRace && Button("Ready"))
            win.CloseWindow();
        if (!setting.vk2 && Button("Scoreboard"))
            ShowScoreBoard();
        //if (_Loader.vsFriendsOrPlayers && isDebug && Button("play Replay"))
        //    EndGameReplay();
        //RateButton(EndGameWindow);
        if (!online && Button("Next Track"))
        {
            LoadLevel(Levels.menu);
        }
        gui.EndHorizontal();
        //gui.EndScrollView();
    }
    //[RPC]
    //public void SetLevelTime(float time)
    //{
    //    //print("Set Start Time "+time);
    //    timeElapsedLevel = time;
    //}
    public void RateButton(Action a)
    {
        if (_Loader.levelEditor == null && (!PlayerPrefsGetBool("voted" + _Loader.mapName) || !Button("AlreadyVoted")) && Button("Rate this map"))
            ShowWindow(_Loader.RateMapWindow, a);
    }
    private void WonMedalWindow()
    {
        Setup(600, 450);
        _Awards.DrawReward(_Awards.xp);
        LabelCenter("Achievements");
        gui.BeginHorizontal();
        if (wonMedal)
        {
            //gui.BeginHorizontal();
            for (int i = _Loader.place; i <= 3; i++)
                gui.Label(bs.res.medals[i]);
            //gui.EndHorizontal();
        }
        foreach (Award a in _Awards.awards)
            if (a.local > 0)
            {
                skin.label.imagePosition = ImagePosition.ImageAbove;
                gui.Label(new GUIContent(a.local + "", a.texture, Tr(a.title)));
            }
        gui.FlexibleSpace();
        gui.EndHorizontal();
        gui.Label(GUI.tooltip);
        gui.FlexibleSpace();
        if (ButtonLeft("Continue"))
        {
            win.ShowWindow(EndGameWindow);
        }
    }
    private void WriteSaveReplay()
    {
        //Debug.LogError("WriteSaveReplay");
        if (_Player.posVels.Count < 100)
            print("cannot save Empty replay");
        BinaryWriter ms = null;
        if (!_Loader.dontUploadReplay)
            using (ms = new BinaryWriter())
            {
                ms.Write((byte)RD.version);
                ms.Write(setting.version);
                foreach (PosVel a in _Player.posVels)
                {
                    ms.Write((byte)RD.posVel);
                    ms.Write(a.pos);
                    ms.Write(a.rot.eulerAngles);
                    ms.Write(a.vel);
                    ms.Write((byte)RD.posVelMouse);
                    ms.Write(a.mouserot);
                    ms.Write((byte)RD.posVelSkid);
                    ms.Write(a.skid);
#if oldStunts
                    ms.Write((byte)RD.score);
                    ms.Write(a.score);
#endif
                }
                foreach (KeyDown a in _Player.replay.keyDowns)
                {
                    ms.Write((byte)RD.keyCode);
                    ms.Write((int)a.keyCode);
                    ms.Write(a.time);
                    ms.Write(a.down);
                }
                ms.Write((byte)RD.avatarId);
                ms.Write(_Player.replay.avatarId);
                print(ms.Length);
                ms.Write((byte)RD.carSkin);
                ms.Write(_Player.replay.carSkin);
                ms.Write((byte)RD.FinnishTime);
                ms.Write(_Player.replay.finnishTime);
                ms.Write((byte)RD.country);
                ms.Write((byte)_Player.replay.contry);
                if (!string.IsNullOrEmpty(_Loader.avatarUrl))
                {
                    ms.Write((byte)RD.avatarUrl);
                    ms.Write(_Loader.avatarUrl);
                }
                if (_Player.carSkin.haveColor)
                {
                    ms.Write((byte)RD.color);
                    var c = _Player.carSkin.color;
                    ms.Write(c.r);
                    ms.Write(c.g);
                    ms.Write(c.b);
                }
                ms.Write((byte)RD.playerName);
                ms.Write(_Player.replay.playerName);
                ms.Write((byte)RD.clan);
                ms.Write(_Player.replay.clanTag);
                ms.Write((byte)RD.rank);
                ms.Write(_Player.replay.rank);
            }
        SendReplay(ms, _Loader.dmOrCoins ? (float)_Player.score : _Player.replay.finnishTime);
    }
    private void SendReplay(MemoryStream ms, float score)
    {
        if (_Loader.mh || guest)
            return;
        if (_Loader.userId == 0)
        {
            Debug.LogError("User id is zero");
            return;
        }
        // "info",score + "\n" + _Player.replay.avatarId + "\n" + (int) _Loader.difficulty + "\n" +_Player.replay.backupRetries,
        www = Download(mainSite + "scripts/sendReplay2.php", null, true,
            "map", _Loader.curScene.mapId, "user", _Loader.userId, "flags", (int)_Loader.replayFlags, "playerName", _Loader.playerNamePrefixed, "mapName", _Loader.mapNamePrefixed
            , "file", (ms == null ? (object)"" : ms.ToArray()),
            "time", score, "version", setting.replayVersion, "fps", (int)_AutoQuality.fps, "retries",
            _Player.replay.backupRetries, "scoreOnly", _Loader.dmOrCoins ? 1 : 0);
    }
    //private void FetchResult(string s, bool b)
    //{
    //    var ss = SplitString(s);
    //    int i = int.Parse(ss[1]);
    //    Popup("You overtaken " + i + " players in scoreboard", win.act);
    //    _Loader.score += i;
    //    _Loader.SetValue("score", _Loader.score, i + " won " + _Loader.mapName);
    //}
    private void TagMe(string tag)
    {
        foreach (GameObject a in GameObject.FindGameObjectsWithTag(tag))
        {
            if (tag != Tag.CheckPoint)
                a.GetComponentInChildren<Renderer>().enabled = false;
            triggers.Add(a.collider);
        }
    }
    internal int frameInterval
    {
        get { return (int)(2 * 0.01f / Time.fixedDeltaTime); }
    }
    internal float timeElapsed
    {
        //get { return timeElapsed2; }
        get
        {
            return _Game.timeElapsedLevel - _Game.spStartTime;
        }
    }
    //#if UNITY_EDITOR
    //    void OnApplicationQuit()
    //    {
    //        if (bs.resEditor.enableLevelEdit)
    //            EditorApplication.delayCall += OnApplicationQuitDelayed;
    //    }
    //    private void OnApplicationQuitDelayed()
    //    {
    //        EditorApplication.delayCall -= OnApplicationQuitDelayed;
    //        if (this == null) return;
    //        var instanceIdToObject = EditorUtility.InstanceIDToObject(this.GetInstanceID());
    //        Game a = (Game)instanceIdToObject;
    //        if (a.ghosts.Count == 0)
    //        {
    //            Undo.RegisterUndo(a, "rtools");
    //            a.ghosts = ghosts;
    //            EditorUtility.SetDirty(a);
    //        }
    //    }
    //#endif
    //const float d = 3, len = 5;
    //Vector3[] _list = new[]{ 
    //        Vector3.left *d, Vector3.left*(d+len),
    //        Vector3.up *d, Vector3.up*(d+len),
    //        Vector3.right *d, Vector3.right*(d+len),
    //        Vector3.down *d, Vector3.down*(d+len)};
    //static Material green;
    //private static Material MakeMaterial(Color c)
    //{
    //    return new Material("Shader \"Lines/Colored Blended\" {SubShader { Pass {     Blend SrcAlpha OneMinusSrcAlpha    ZWrite Off Cull Off Fog { Mode Off }    Color(" + c.r + "," + c.b + "," + c.g + "," + c.a + ") } } }") { hideFlags = HideFlags.HideAndDontSave, shader = { hideFlags = HideFlags.HideAndDontSave } };
    //}
    //public void OnPostRender()
    //{
    //    if (!_Loader.dm) return;
    //    print("asd");
    //    green.SetPass(0);
    //    GL.LoadOrtho();
    //    GL.Begin(GL.LINES);
    //    var ScreenCursor = new Vector3(Screen.width, Screen.height) / 2;
    //    ScreenCursor.z = .7f;
    //    foreach (var a in _list)
    //    {
    //        var v = ScreenCursor + new Vector3(a.x, a.y);
    //        v += a.normalized * (1 + 1);
    //        v.x /= Screen.width;
    //        v.y /= Screen.height;
    //        GL.Vertex(v);
    //    }
    //    GL.End();
    //}
    //private void InitMarmoset()
    //{
    //    RenderSettings.ambientLight = Color.black;
    //    var sk = RenderSettings.skybox;
    //    Texture2D texture2D = ((Texture2D)sk.GetTexture("_FrontTex"));
    //    Cubemap cb = new Cubemap(texture2D.width, TextureFormat.RGB24, false);
    //    cb.SetPixels(getPixels(sk, "_FrontTex"), CubemapFace.PositiveZ);
    //    cb.SetPixels(getPixels(sk, "_BackTex"), CubemapFace.NegativeZ);
    //    cb.SetPixels(getPixels(sk, "_LeftTex"), CubemapFace.NegativeX);
    //    cb.SetPixels(getPixels(sk, "_RightTex"), CubemapFace.PositiveX);
    //    cb.SetPixels(getPixels(sk, "_UpTex"), CubemapFace.PositiveY);
    //    cb.SetPixels(getPixels(sk, "_DownTex"), CubemapFace.NegativeY);
    //    cb.Apply();
    //    var sky = new GameObject("MAROOOOOOOOOOOMSET").AddComponent<Sky>();
    //    //sky.diffIntensity = 0;
    //    //sky.masterIntensity = 2;
    //    sky.hdrDiff = sky.hdrSky = sky.hdrSpec = true;
    //    sky.diffuseCube = cb;
    //    sky.specularCube = cb;
    //    sky.skyboxCube = cb;
    //    sky.ApplySkyTransform();
    //    sky.showSkybox = true;
    //    RenderSettings.skybox.mainTexture = cb;
    //    AssetDatabase.CreateAsset(cb, "Assets/SkyCubeMap.cubemap");
    //    var shader = Shader.Find("Marmoset/Specular IBL");
    //    foreach (Renderer r in FindObjectsOfType(typeof (Renderer)))
    //    {
    //        var layer = r.gameObject.layer;
    //        if (layer == Layer.level || layer == Layer.cull)
    //        {
    //            r.material.shader = shader;
    //            r.material.SetTexture("_SpecTex", res.noise);
    //            r.material.SetFloat("_SpecInt", 0.05f);
    //            r.material.SetFloat("_Fresnel", 0);
    //        }
    //    }
    //}
    //private static Color[] getPixels(Material sk, string name)
    //{
    //    Texture2D texture2D = (Texture2D) sk.GetTexture(name);
    //    if (texture2D.format != TextureFormat.RGBA32) throw new Exception("Incorrect texture format " + texture2D.format);
    //    Color[] pixels = texture2D.GetPixels();
    //    var pixels2 = new Color[pixels.Length];
    //    for (int i = pixels.Length - 1, j = 0; i >= 0; i--, j++)
    //        pixels2[j] = pixels[i];
    //    return pixels2;
    //}
}