
using System.Globalization;
using System.Security.Cryptography;
#if !UNITY_WP8
using CodeStage.AntiCheat.ObscuredTypes;
#else
using ObscuredInt = System.Int32;
using ObscuredFloat = System.Single;
#endif
#if !UNITY_FLASH || UNITY_EDITOR
using System.Linq;
#endif
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public partial class Player : bsNetwork, IComparer<Player>
{
    public FreeCamera freeCamera2;
    internal bool cop { get { return replay.cop; } }
    public bool testPlayer { get { return this == _Game.testPlayer; } }
    public bool testBot { get { return this == _Game.testBot; } }
    public bool test { get { return testPlayer || testBot; } }
    public Light muzzleFlashLight;
    public Vector3 distanceToCursor = Vector3.one * 99999;
    public Vector3 distanceToCursor2;
    public int TargetPlayerId = -1;
    public Transform FlagPlaceHolder;
    private const int minVel = (100 / 4);
    public Hud hud;
    public AndroidHud androidHud;
    public Skidmarks SkidMarks;
    public Transform rainfall;
    public Texture2D avatar { get { return res.GetAvatar(replay.avatarId,replay.avatarUrl); } }
    public Transform cam;
    public bool brake;
    private float groundedTime;
    private bool movingBack;
    private bool movingBack2;
    //private float movingBackf;
    internal Transform leftWhell;
    internal Transform rightWhell;
    internal Transform upLeftWhell;
    internal Transform upRightWhell;
    private float tireRot;
    private float tireRot2;
    public Replay replay = new Replay();
    internal List<PosVel> posVels { get { return replay.posVels; } }
    private bool grounded = true;
    internal bool grounded2 = true;
    public float Friq = 0.06f;
    private float rotVel;
    private SecureInt rewinds=9;
    public new InputManager input;
    private AudioSource idleAudio;
    private AudioSource stopAudio;
    private AudioSource motorAudio;
    private AudioSource windAudio;
    private AudioSource backTimeAudio;
    //private AudioSource hornAudio;
    private AudioSource nitroAudio;
    private int[] skidMarksArray = new int[4];
    internal Vector3 oldpos;
    public float totalMeters;
    public float checkPointMeters;
    internal bool up;
    internal bool down;
    internal bool left;
    internal bool right;
    private bool groundHit = true;
    public Transform[] camPoses;
    public new Camera camera;
    public Transform model;
    public bool ghost;
    public bool ghost2 { get { return _Loader.mpCollisions ? !IsMine : ghost; } }
    private Vector3 deltaPos;
    private Vector3 ghostVel;
    //private Vector3 lastGroundPos;
    public ParticleEmitter[] emiters;
    internal new Rigidbody rigidbody;
    internal new Transform transform;
    public Transform flashLight;
    private float hitForce;
    public int randomHash;
    public Collider carBox;
    public MeshTest meshTest;
    //public ParticleEmitter[] fire2;
    //public GameObject[] fire;
    //public GameObject[] smoke;
    public override void Awake()
    {

        resetTime = Time.time;
#if VOICECHAT
        voiceChatPlayer = GetComponent<VoiceChatPlayer>();
#endif
        rigidbody = base.rigidbody;
        transform = base.transform;
        SkidMarks.transform.parent = null;
        SkidMarks.transform.position = Vector3.zero;
        SkidMarks.transform.rotation = Quaternion.identity;
        if (_Player == null && !testBot || testPlayer) _Player = this;
        _Game.listOfPlayers.Add(this);
        camera = cam.GetComponentInChildren<Camera>().camera;
        cam.gameObject.SetActive(false);
        replay.carSkin = _Loader.carSkin;
        //replay.playerName = _Loader.playerNamePrefixed;
        replay.contry = _Loader.Country;
        replay.avatarId = _Loader.avatar;

        if (!android && !ios)
            Destroy(androidHud.gameObject);
        randomHash = GetHashCode();
        
        if (testPlayer)
        {
            PhotonNetwork.player.actorID = 0;
            _Game.photonPlayers.Add(photonView.ownerId = 0, this);
        }
        else if (testBot)
        {
            m_IsMine = 0;
            ghost = true;
            _Game.photonPlayers.Add(photonView.ownerId = 1, this);
        }
        else if (online)
        {
            enabled = false;
            if (!_Game.photonPlayers.ContainsKey(photonView.ownerId))
                _Game.photonPlayers.Add(photonView.ownerId, this);
            else
                Debug.LogError(photonView.ownerId + " ID Already exists");

        } 

        //DeactiveWeps();
        
        carBox.isTrigger = true;
        if (_Loader.dm)
        {
            DeactiveOrInitWeps();
            WeaponBase b = weapons.FirstOrDefault(a => GetFlag((int)_Loader.weaponEnum, (int)a.weaponEnum));
            if (b != null)
                SelectWeapon(b.id);
            else
                SelectWeapon(0);
        }
        else
            Destroy(dmTr);
        base.Awake();
    }
    
    private void DeactiveOrInitWeps()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].id = i;
            weapons[i].pl = this;
            weapons[i].gameObject.SetActive(false);
        }
    }

    public bool haveFlag { get { return _Loader.ctf && _MpGame.flags.Any(a => a.pl == this); } }
    public GameObject dmTr;
    public bool sameTeam { get { return _Loader.teamOrPursuit &&  teamEnum == _Player.teamEnum; } }
    List<PosVel> oldPosVels;
    public void Start()
    {
        carBox.isTrigger = false;
        if (online)
            ghost = _Loader.mpCollisions ? false : !photonView.isMine;
        
        name = "Player " + replay.playerName;
        replay.pl = this;
        firstPlayer = this == _Player;
        
        secondPlayer = this == _Player2;
        if (!_Loader.pursuit)
            LoadSkin();
        

        if (this == _Player)
        {
            _Loader.prefixMapPl = _Loader.mapName + ";" + _Loader.playerName + ";";
#if GA
            GA.SettingsGA.TrackTarget = transform;
#endif
        }
        Reset();
        playerNameTxt.enabled = false;
        //playerNameTxt.transform.parent = null;
        InitInput();
        InitSounds();
        var b = highQuality && !android || !ghost;
        flashLight.gameObject.SetActive(_Loader.night && b);
        flashLight.GetChild(0).gameObject.SetActive(!ghost);


        var colliders = GetComponentsInChildren<Collider>();
        if (!_Loader.enableCollision || _Loader.dm)
        {
            if (ghost)
                foreach (var a in colliders)
                    if (a.tag != Tag.HitBox || a == carBox)
                        DestroyImmediate(a);
        }
        else
        {
            foreach (var a in colliders)
                a.enabled = !ghost;
        }
        if (_Loader.topdown)
            ChangeCamera(CurCam.topdown);
        if (!_Loader.dm)
            ChangeCamera(CurCam.norm2);
        if (this == _Player)
            Physics.gravity = res.gravitation * Vector3.down;
        
        
        //turret.parent.gameObject.SetActive(_Loader.dm);
        //shield.gameObject.SetActive(_Loader.dm);

        
        if (_Loader.enableCollision && ghost)
            oldPosVels = new List<PosVel>(posVels);
        //if (!_Loader.enableCollision && !_Loader.dm)
        //    Destroy(fire[0].transform.parent.gameObject);
        if (playerNameTxt is GUIText)
            playerNameTxt.transform.parent = null;


        if (ghost2)
            Destroy(freeCamera2.gameObject);
        else
        {
            freeCamera2.transform.parent = null;
            freeCamera2.guiLayer = _Player.camera.GetComponent<GUILayer>();
        }        
    }
    public Renderer shield;
    public override void OnPlConnected()
    {
        Debug.LogWarning("OnPlConnected");
        InitNetwork();
    }
    public void InitNetwork()
    {
        //if (!online) return; 
        
        CallRPC(SetFinnishTime, finnishTime);
        //print("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<" + _Loader.serverVersion);
        CallRPC(SetNick2, IsMine ? _Loader.playerNamePrefixed : replay.playerName);
        CallRPC(SetAvatarUrl, IsMine ? _Loader.avatarUrl : replay.avatarUrl);
        
        var clantag = (_Loader.clanTag.Length > 0 ? "[" + _Loader.clanTag + "]" : "");
        CallRPC(SetClanTag, IsMine ? clantag : replay.clanTag );

        CallRPC(SetAvatar, replay.avatarId);
        CallRPC(SetCarId, replay.carSkin);
        if (_Loader.dm)
            CallRPC(SetLife2, life + 0);
        if(_Loader.dmOrPursuit)
            CallRPC(SetScore2, score + 0);
        if (_Loader.enableZombies)
            CallRPC(SetZombieKills, zombieKills);

        if (replay.color != null)
            SetColorRPC(replay.color.Value);
        else if (IsMine && carSkin.haveColor)
            SetColorRPC(carSkin.color);
        CallRPC(SetKills, (int)kills);
        CallRPC(SetDeaths, (int)deaths);
        CallRPC(SetTeam, (int)replay.teamEnum);
        CallRPC(SetEnabled);
        CallRPC(SetRank, IsMine ? _Awards.GetRank(_Awards.xp) : replay.rank);
        CallRPC(SetMod, IsMine ? (int)_Loader.modType: (int)replay.modType);
        if (_Loader.dm)
            CallRPC(SelectWeapon, curWeaponId);
        if(_Loader.dmOrPursuit)
            CallRPC(SetDead, dead);
        if (_Loader.pursuit)
            CallRPC(SetWasCop, wasCop);
    }
    [RPC]
    void SetDead(bool dead)
    {
        this.dead = dead;
    }
    [RPC]
    void SetRank(int rank)
    {
        replay.rank = rank;
    }
    [RPC]
    void SetMod(int mod)
    {
        replay.modType = (ModType)mod;
    }
    public static void SetFlag(GameObject car, CountryCodes cc)
    {
        if (car.name.StartsWith("carF1"))
        {
            print("Setting Flag for car " + cc);
            foreach (var a in car.GetComponentsInChildren<Renderer>())
                if (a.name == "DrawCall_0013")
                {

                    var m = bs.LoadRes("Flags/" + cc.ToString().ToLower() + "Flag") as Material;
                    //print("bum"+m);
                    if (m != null)
                        a.sharedMaterial = m;
                    else
                        print("Flag not found");

                }
        }
    }

    [RPC]
    private void SetClanTag(string s)
    {
        replay.clanTag = s;
    }
    [RPC]
    private void SetColor(float r, float g, float b)
    {
        var color = new Color(r, g, b);
        print("Set Color "+color);
        replay.color= color;
    }
    private void SetColorRPC(Color c)
    {
        CallRPC(SetColor, c.r, c.g, c.b);
    }

    public ParticleEmitter[] capsule;

    internal int playerId
    {
        get { return photonView.ownerId; }
    }
    

    //public bool IsMine{get { return photonView.isMine; }}


    private bool carIdSet;
    [RPC]
    private void SetCarId(int obj)
    {
        carIdSet = true;
        replay.carSkin = obj;
        carSkin = _Loader.GetCarSkin(replay.carSkin, IsMine);
    }
    [RPC]
    private void SetAvatar(int Obj)
    {
        replay.avatarId = Obj;
    }
    
    [RPC]
    public void SetEnabled()
    {
        enabled = true;
    }
    [RPC]
    public void SetAvatarUrl(string s)
    {
        print(s);
        replay.avatarUrl = s;
    }
    [RPC]
    public void SetNick2(string Obj)
    {
        playerNameTxt.text = Trn(Obj);
        replay.playerName = Obj;
    }
    [RPC]
    private void SetFinnishTime(float fin)
    {
        replay.finnishTime = finnishTime = fin;
    }
    internal CarSkin carSkin;
    private void LoadSkin()
    {
        if (!carIdSet)
            Debug.LogWarning("LoadCarSkin Before CarSet! " + playerNameClan);
        replay.carSkin = Mathf.Max(replay.carSkin, 0);
        

        //if (replay.carSkin != -1)
        //{


        carSkin = _Loader.GetCarSkin(_Loader.pursuit ? (replay.cop ? 9 : replay.carSkin == 9 ? 3 : replay.carSkin) : _Loader.dm ? 0 : replay.carSkin, firstPlayer);
        if (carSkin != null )
        {
            DestroyImmediate(model.gameObject);
            GameObject car = (GameObject)Instantiate(carSkin.model);
            if (replay.contry != CountryCodes.fi)
                SetFlag(car, replay.contry);

            car.transform.parent = transform;
            car.transform.localPosition = car.transform.position + Vector3.down * 0.37f;
            car.transform.localRotation = Quaternion.identity;
            
            model = car.transform;
        }
        if ((_Loader.autoQuality || !lowQualityAndAndroid))
            meshTest = model.GetComponentInChildren<MeshTest>();
     if(meshTest!=null)
         meshTest.Start2();
        //else
        //    print("Couldn't load car " + replay.carSkin);
        //}
        //else
        //    print("car skin not found " + replay.carSkin);
        renderers = model.GetComponentsInChildren<Renderer>();

        if (replay.color != null)
            carSkin.SetColor(renderers, replay.color);
        materials.Clear();
        foreach (var a in renderers)
            materials.Add(a.sharedMaterials);

        //if (!ghost && !lowestQuality && carSkin.haveColor)
        

        carDamage = GetComponent<CarDamage>();
#if !UNITY_FLASH
        if (carDamage != null)
        {
            carDamage.meshFilters = renderers.Where(a => a.tag == Tag.damage).Select(a => a.GetComponent<MeshFilter>()).ToArray();
            if (carDamage.meshFilters.Length == 0) carDamage.enabled = false;
        }
#endif




        Destroy(model.GetComponentInChildren<MeshCollider>());
        //var carbox2 = model.GetComponentInChildren<MeshCollider>();
        
        //if (carbox2 && carbox2.convex)
        //{            
        //    Destroy(carBox.gameObject);
        //    carBox = carbox2;

        //    carBox.transform.parent = transform;
        //    //rigidbody.centerOfMass = carBox.bounds.center;
        //    var v = pos - carBox.bounds.center;
        //    print("mass center Offset " + v.magnitude);
        //    //foreach (Transform a in transform)
        //    //    a.position += v;
        //}

       

        //var trs = model.GetComponentsInChildren<Transform>();
        foreach (Transform a in model.Cast<Transform>().ToArray())
        {
            if (a.name == "ult" || a.name == "ul")
                upLeftWhell = a;
            else if (a.name == "urt" || a.name == "ur")
                upRightWhell = a;
            else if (a.name == "lt" || a.name == "dl")
                leftWhell = a;
            else if (a.name == "rt" || a.name == "dr")
                rightWhell = a;
            //else //if (a.renderer != null)
            //corpus2 = carBox.transform;
            else if (a.name == "corpus")
                corpus2 = a;
        }
        animateSpring = corpus2 != null && !ghost;

        wheels = null;
        BlackReset();
#if blur
        AmplifyMotionEffect.RegisterRecursivelyS(gameObject);
#endif
        if (ghost2)
            foreach (var a in model.GetComponentsInChildren<Light>())
                Destroy(a);
    }
    private CarDamage carDamage;
    public Renderer[] renderers;
    public List<Material[]> materials = new List<Material[]>();
    private void InitInput()
    {
        if (!ghost2 || _Player == this)
        {
            cam.gameObject.SetActive(true);
        }
        if (splitScreen)
        {
            hud.camera.rect = camera.rect = _Player2 == this ? new Rect(0, .5f, 1, 1) : new Rect(0, 0, 1, .5f);
            if (secondPlayer)
                camera.GetComponent<GUILayer>().enabled = camera.GetComponent<AudioListener>().enabled = false;
        }
        if (ghost2)
            Destroy(hud.gameObject);
        else
        {
            hud.transform.parent = null;
            hud.transform.position = secondPlayer ? Vector3.left * 50 : Vector3.zero;
            hud.transform.rotation = Quaternion.identity;
        }
        cam.transform.parent = null;
        if(testBot)
            cam.gameObject.SetActive(false);
        if (!ghost)
        {
            input = (InputManager)Instantiate(_Loader.inputManger);
            input.pl = this;
        }
        if (secondPlayer)
            replay.playerName = Tr("Second Player");
        if (secondPlayer && _Loader.reverseSplitScreen)
        {
            List<Transform> tt = GetTrs(hud.transform);
            foreach (Transform a in tt)
                a.parent = null;
            hud.transform.localEulerAngles = camera.transform.localEulerAngles = new Vector3(0, 0, 180);
            foreach (Transform a in tt)
                a.parent = hud.transform;
        }
        if (ghost)
        {
            //if (!_Loader.enableZombies)
                Destroy(rigidbody);
            isKinematic = true;
        }
        if (splitScreen && _Loader.reverseSplitScreen)
            Screen.orientation = ScreenOrientation.AutoRotation;
    }
    private void InitSounds()
    {
        dopler(audio);
        idleAudio = InitSound(res.idle);
        stopAudio = InitSound(res.brake);
        AudioClip engineSound = res.CarSkins[Mathf.Max(0, replay.carSkin)].engineSound;
        if (!engineSound)
            engineSound = res.engineSound;
        motorAudio = InitSound(engineSound);        
        backTimeAudio = InitSound(res.backTime, false);
        windAudio = InitSound(res.wind);
        if (!firstPlayer)
            windAudio.enabled = false;
        nitroAudio = InitSound(res.nitro, false);
        if (this == _Player)
            audio.priority = 0;
        else
            windAudio.enabled = idleAudio.enabled = stopAudio.enabled = backTimeAudio.enabled = nitroAudio.enabled = false;
        
    }
    private float oldTime;
    private float deltaTime;
    public void Update()
    {        
        Update2();
        UpdateText();
    }
    private void DamageText(string Text)
    {
        var d = (DamageText) Instantiate(res.damageText, pos, Quaternion.identity);
        d.tmMesh.text = Text;
    }
    public bool forceRig;
    public float nearTime;
    public void Update2()
    {
        if (_Loader.pursuit && !IsMine && setting.optimization)
        {
            bool n = (_Player.pos - pos).magnitude < 50 || forceRig;
            if (n)
                nearTime = Time.time;
            bool k = Time.time - nearTime > 3;
            rigidbody.isKinematic = isKinematic = k;
            carBox.enabled = !k;
            if (k != isKinematic && k)
            {
                rigidbody.velocity = ghostVel;
                transform.position = syncPos;
                transform.rotation = syncRot;                
                //rigidbody.solverIterationCount = 6;
                //rigidbody.WakeUp();
                //rigidbody.detectCollisions = false;
                //rigidbody.detectCollisions = true;
                //rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                //rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
            if (isKinematic)
            {
                transform.position = Vector3.Lerp(transform.position, syncPos + vel * ping, Time.deltaTime * 3);
                transform.rotation = Quaternion.Lerp(transform.rotation, syncRot, Time.deltaTime * 3);
            }
        }

        if (KeyDebug(KeyCode.Tab))
            Freeze();
        UpdateConstrains();
        if (oldTime != 0)
            deltaTime = Time.realtimeSinceStartup - oldTime;
        oldTime = Time.realtimeSinceStartup;
        if (Time.deltaTime > 1)
            return;
        if (oldpos != Vector3.zero)
            deltaPos = pos - oldpos;
        UpdateSounds();
        if (!_Game.started && ghost && !online)
        {
            pos = _Player.pos;
            rot = _Player.rot;
        }
        if (!_Game.started)
            groundHit = grounded = grounded2 = true;
        var speedTrack = speed > 0 &&wKey;
        if (!(android && ghost && lowQuality))
            foreach (var a in emiters)
                a.emit = _Game.backTime2 ? false : speedTrack;
        UpdateHelpText();

        if ((!_Game.started || _Game.editControls || finnished) && !online)
            return; /*!game.started*/
        UpdateTimeRevert();
        if (_Loader.dm)
            UpdatePlayerDm();
        if (dead && !ghost) return;
        UpdatePursuit();
        FallCheck();
        UpdateInput();
        UpdateRecordBackupReplay2();
        UpdateGhost();
        UpdateTriggers();
        UpdateFixed();
        UpdateWhell();
        UpdateSkidMarks();
#if oldStunts
        if (_Loader.stunts)
            UpdateStunts();
#endif
        if (!ghost)
        {
            
            //if (angleTest)
            //    rigidbody.angularDrag = 5;
            //else
            //    rigidbody.angularDrag = Time.time - upsideDown < .1f ? 5 : 20;
            //coll.material = UpsideDown ? res.border : null;
            totalMeters += deltaPos.magnitude * (_Game.backTime ? -1 : 1);
        }
        //if(!_Loader.dm)
        if (!_Loader.topdown && GetKeyDown(KeyCode.C) || fps && _Game.finnish)
            ChangeCamera(curCam + 1);


        if (GetKeyDown(KeyCode.F) && Time.time - hornPlay > .5f)
        {
            AudioClip hornAudioClip = carSkin.horn.Length > 0 ? carSkin.horn[Random.Range(0, carSkin.horn.Length)] : res.horn;
            hornPlay = Time.time;
            PlayOneShot(hornAudioClip);
        }
        //if (!hornAudio.enabled && horn)
        //{
        //    hornAudio.volume = 1;                
        //    hornAudio.enabled = true;
        //    hornAudio.Play();
        //}
        //hornAudio.enabled = horn;
        
        //if (GetKey(KeyCode.F))
        //{
            //if (hornAudio.enabled && !hornAudio.isPlaying)
            //{
            //    hornAudio.volume = 1;                
                //hornAudio.Play();
            //}
            
        //}
        //else
            //if (hornAudio.isPlaying)
                //hornAudio.Stop();
        if (!ghost && _Loader.rain)
        {
            _Game.rainfall.localVelocity = ZeroY(rigidbody.velocity / 2) + Vector3.down * 40;
        }
        if (nitroAudio.enabled)
        {
            if (speedTrack && !nitroAudio.isPlaying)
                nitroAudio.Play();
            if (!speedTrack && nitroAudio.time > .3f && nitroAudio.isPlaying)
                nitroAudio.Stop();
        }
        if (hud != null)
        {
            var text = new StringBuilder();
            if (!_Loader.dmNoRace)
                text.Append(Mathf.Max(0, rewinds).ToString());
            if (_GameSettings.laps > 1 && !_Loader.dmNoRace)
                text.Append(Tr("\nLap: ") + Mathf.Min(lap, _GameSettings.laps) + "/" + _GameSettings.laps);
            if (_Loader.dmOrPursuit)
                text.Append(Tr("\nLife:")).Append((int)life);
            if (nitro > 0)
                text.Append(Tr("\nNitro:") + (int)(nitro * 100));

            hud.backup.text = text.ToString();
        }
        //Log("Vel " + vel.magnitude);
        //UpdateFire2();        
        oldpos = pos;
    }

    private void UpdatePursuit()
    {
        if (_Loader.pursuit && !cop && TimeElapsed(10) && !dead)
            score += 1;
    }
    private float hornPlay;

    private void UpdateHelpText()
    {
        if (!_Game.backTime && _Game.started)
        {
            //if (Physics.Raycast(pos + Vector3.up, Vector3.down, 5, 1 << Layer.stadium))
            //    dead = true;
            if (avrgVelSlow < 30 / 4 && _Game.timeElapsed > 3 || dead)
                ShowCenterTextReset();
            if (avrgVelSlow < 20)
                _GameGui.ShowHelpScreen(.1f);
        }
    }
    private void UpdateConstrains()
    {
        if (!ghost)
            rigidbody.constraints = _Game.started ? RigidbodyConstraints.None : RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
    }
    private Vector3 splashTime;
    private Transform[] wheels;
    private Vector3[] wheelPos = new Vector3[4];
    private void UpdateSkidMarks()
    {

        if (!medium || _Game.backTime2 || rightWhell == null)
            return;
        if(wheels==null)
        {
            wheels = new[] { leftWhell, rightWhell, upLeftWhell, upRightWhell };
            for (int i = 0; i < wheels.Length; i++)
                wheelPos[i] = wheels[i].localPosition;
        }
        if (_Game.isCustomLevel)
            UpdateWater();

        for (int i = 0; i < wheels.Length; i++)
        {
            RaycastHit h;
            if (animateSpring)
                wheels[i].localPosition = Vector3.Lerp(wheels[i].localPosition, wheelPos[i], Time.deltaTime*30);
            else
            {
                if ((i > 1 || stopAudio.volume < .8f) && !animateSpring)
                {
                    skidMarksArray[i] = -1;
                    continue;
                }
            }
            if (Physics.Raycast(wheels[i].position, -transform.up, out h, .7f, Layer.levelMask))
            {
                if (animateSpring)
                {
                    wheels[i].position = Vector3.Lerp(wheels[i].position,
                        wheels[i].position - transform.up*(Mathf.Min(.4f, h.distance) - .35f), Time.deltaTime*30);
                    //print(wheels[i].position);                    
                }

                //if (h.transform.gameObject.layer == Layer.water)
                //{
                //    Instantiate(res.waterSplashPrefab, h.point, Quaternion.identity);
                //    continue;
                //}
                if (i > 1) continue;
                if (stopAudio.volume > .8f)
                {
                    if (!flash && !dirtMaterial)
                    {
                        var position = h.point + transform.up * .05f;
                        skidMarksArray[i] = SkidMarks.AddSkidMark(position, h.normal, skid / 2, skidMarksArray[i]);
                    }
                    SkidSmoke(i);
                }
                else
                    skidMarksArray[i] = -1;
                //for (int i2 = 0; i2 < 4; i2++)
                //skidMarksArray[i2] = -1;
            }
            else
            {
                stopAudio.volume = 0;
            }
        }

    }

    private void SkidSmoke(int i)
    {
        var o = oldpos;
        for (int j = 0; j < 5; j++)
        {
            o = Vector3.MoveTowards(o, pos, dirtMaterial ? 1 : .5f);
            if (dirtMaterial)
            {
                foreach (var a in _Game.dirt)
                {
                    a.transform.position = wheels[i].position + (oldpos - o);
                    a.Emit(1);
                }
            }
            else
            {
                _Game.smoke.transform.position = wheels[i].position + (oldpos - o);
                _Game.smoke.Emit(1);
            }
            if (o == pos) break;
        }
    }

    private void UpdateWater()
    {
        var wpos = pos ;
        //foreach (var wheel in wheels)
        //{
        float distance;
        Ray ray = new Ray(wpos + Vector3.up, Vector3.down);
        //Debug.DrawRay(ray.origin, ray.direction * 3);
        var watery = _MapLoader.water.position.y;
        if (pos.y - .3f < watery && new Plane(Vector3.down, watery).Raycast(ray, out distance) && velm > 1f)
        {
            var point = ray.GetPoint(distance); 
            var o = splashTime;

            o.y = point.y;
            //print("o:"+o);
            //print("point:" + point);
            for (int j = 0; j < 5; j++)
            {
                o = Vector3.MoveTowards(o, point, 1);
                if (o == point) break;
                if (j == 0 && !audio.isPlaying)
                {
                    if (audio.clip != res.waterSound)
                        AudioSource.PlayClipAtPoint(res.waterSound2, pos, _Loader.soundVolume);
                    audio.clip = res.waterSound;
                    audio.Play();
                }
                splashTime = wpos;
                _Game.splash[0].transform.position = o;
                foreach (var a in _Game.splash)
                    if (a.particleCount < 50)
                        a.Emit();
                //Destroy(Instantiate(res.waterSplashPrefab, o, Quaternion.identity), 1);
            }
            //splashTime = Time.time;
        }
        else
        {
            if (audio.clip == res.waterSound)
                audio.clip = null;
            splashTime = pos;
        }
    }
    private void UpdateFixed()
    {
        if (isKinematic) return;
        if (topdown)
            camHited = Physics.Linecast(camera.transform.position, pos + transform.up * 2, out camHit, Layer.levelMask);
        else
            camHited = false;

        if (FramesElapsedA(3, randomHash))
        {
            //var groundMaterial = "";
            groundHit = false;
            if(!ghost2)
            dirtMaterial = false;
            if (speed == 3.01f)
                speed = 0;
            RaycastHit raycastHit;

            //foreach (RaycastHit raycastHit in Physics.RaycastAll(pos, -transform.up * 2, 2, Layer.levelMask))
            if (Physics.Raycast(pos, -transform.up * 2, out raycastHit, 2, Layer.levelMask))
            {
                //if (!groundHit)
                //{
                groundHit = true;
                if (ghost)
                {
                    grounded2 = true;
                    groundedTime = Time.time;
                }
                if (!ghost2)
                    if (raycastHit.collider.tag == Tag.Dirt)
                        dirtMaterial = true;
                    else if (raycastHit.collider != null && raycastHit.collider.renderer != null && raycastHit.collider.renderer.sharedMaterial != null)
                    {
                        dirtMaterial = raycastHit.collider.renderer.sharedMaterials.Any(a => res.dirtMaterials.Contains(a.name));
                    }
                    else if (raycastHit.collider.name == "Terrain(Clone)" || raycastHit.collider.name == "Terrain")
                    {
                        dirtMaterial = true;
                    }
                //groundMaterial = "ground";

                //if (res.dirtMaterials.Contains(groundMaterial))
                //    dirtMaterial = true;
                //}
                
                if (raycastHit.collider.tag == RoadType.Speed.ToString() && input.GetKey(KeyCode.W))
                    speed = 3.01f;
                //break;
            }
        }
    }
    private void UpdateRotation()
    {
        if (isKinematic) return;
        if (!ghost)
        {
            flashMouse = _Loader.enableMouse ? android ? androidHud.mouse.x : firstPlayer ? ((Input.mousePosition.x / Screen.width) - .5f) * 2 : 0 : 0;
            if (_Loader.enableMouse)
                flashMouse += Input.GetAxis(firstPlayer ? "Horizontal" : "Horizontal2");
            flashMouse = Mathf.Clamp(flashMouse, -1, 1) * carSkin.getRotationMouse();
        }
        float rotate = (left ? -1f : right ? 1f : flashMouse * _Loader.sensivity * 2);
        bool hit = groundHit && Time.time - groundedTime < .5f;
        if (hit)
        {
            angularVel = Mathf.MoveTowards(angularVel, rotVel, Time.deltaTime * 5);
            if (angularVel > 0 && !right || angularVel < 0 && !left)
                angularVel *= .95f;
        }
        else
        {
            if (rotVel < 0 && angularVel > 0 || rotVel > 0 && angularVel < 0 || carSkin.flyRotation)
                angularVel = Mathf.Lerp(angularVel, rotVel, Time.deltaTime * .6f);
        }
        if (_Loader.enableMouse)
            rotVel = rotate * 3f * (_GameSettings.Rotation);
        else
        {
            var i = (left && rotVel > 0 || right && rotVel < 0 || !right && !left ? 5 : _Loader.rotationFactor*3);
            //i = 3;
            var time = Time.deltaTime * carSkin.getRotation() * i * Mathf.Min(1, .5f + velm/100);
            if (setting.zanos)
                time /= Mathf.Clamp(skid2 * 3, 1, 2.3f);
            var to = (1 + (skid / 2)) * rotate * res.RotCurve.Evaluate(velm * 4) * 2.5f * (_GameSettings.Rotation);
            rotVel = Mathf.Lerp(rotVel, to, time);
        }
        var rotVelFactor = Mathf.Min(1, velm / res.rotationStart);
        if (_Game.started && !isKinematic)
        {
            var rotVelTest = (movingBack ? -1 : 1)  * rotVelFactor * (hit ? rotVel : angularVel);
            rigidbody.MoveRotation(rigidbody.rotation * Quaternion.Euler(0, Mathf.Clamp(rotVelTest, -7, 7) * Time.deltaTime * 17, 0));
        }
    }
    private void UpdateWhell()
    {
        if (rightWhell != null)
        {
            if (!brake)
            {
                var f = velm * (movingBack2 ? -1 : 1) * 3.14f * 20 * Time.deltaTime;
                tireRot += f;
                tireRot2 += f * (up && velm < minVelForSkid ? 5 : 1);
            }
            leftWhell.localEulerAngles = rightWhell.localEulerAngles = new Vector3(tireRot2, 0, 0);
            upRightWhell.localEulerAngles =
                upLeftWhell.localEulerAngles = new Vector3(tireRot, Mathf.Clamp(rotVel * 10, -45, 45), 0);
        }
    }
    internal bool isKinematic = false;
    private Vector3 springAng;
    private Transform corpus2;
    private bool animateSpring;
    private Vector3 sprintOldVel;
    private Vector3 springAngVel;
    //public Collider coll;
    private float velSmooth;
    private float lastJump;
    //private bool oldGronded;
    public bool driftKey;
    private bool zoom;
    private float zoomRange = .5f;
    public void FixedUpdate()
    {

        if (online && _Loader.mpCollisions && !IsMine)
        {
            //var mt = Vector3.Lerp(Vector3.zero, offsetPos, Time.deltaTime * setting.lerpFactorPos);
            var mt = Vector3.MoveTowards(Vector3.zero, offsetPos, Time.deltaTime * setting.lerpFactorPos);
            offsetPos -= mt;
            rigidbody.MovePosition(pos + mt);

            var rt = Quaternion.Slerp(Quaternion.identity, offsetRot, Time.deltaTime * setting.lerpFactorRot);
            //offsetRot = Quaternion.Lerp(offsetRot, Quaternion.identity, 1 - Time.deltaTime * 10);
            offsetRot *= Quaternion.Inverse(rt);
            rigidbody.MoveRotation(rt * rot);
        }
        velm = vel.magnitude;
        if (ghost)
            FixedUpdateGhost();

        //UpdateDeadCam();
        if (isKinematic || dead || ghost) return;                  /* ghost return */
     
        //if (_Game.isCustomLevel && oldGronded != grounded && !grounded && velSmooth < vel.y && velSmooth > -3)
        //{
        //    var vv = vel;
        //    vv.y = velSmooth;
        //    print("Jump Smooth" + (vel - vv).magnitude);
        //    vel = vv;
        //}
        //velSmooth = Mathf.Lerp(velSmooth, vel.y, Time.deltaTime * (_Game.isCustomLevel ? 7 : 20));
        //oldGronded = grounded;

        if (animateSpring)
        {
            var vel2 = Quaternion.Inverse(rot) * vel;
            var angularVelocity = (vel2 - sprintOldVel) * 5;
            angularVelocity = new Vector3(angularVelocity.z * 1, angularVelocity.x * .2f, angularVelocity.x * .5f);
            springAngVel = Vector3.Lerp(springAngVel, (angularVelocity - springAng), Time.deltaTime);
            springAng += springAngVel;
            springAng = Vector3.Lerp(springAng, angularVelocity, 10 * Time.deltaTime);
            //(angularVelocity.magnitude > ang.magnitude ? 10 : 1) 
            sprintOldVel = vel2;
            corpus2.localEulerAngles = Vector3.ClampMagnitude(-springAng * 2, 5);
        }

        upAngle = Vector3.Angle(transform.up, Vector3.up);
        UpdateCamPos();
        if (froozen)
        {
            //vel *= .996f;
            return;
        }
        if (setting.AngleTest)
            rigidbody.angularDrag = 5;
        else
            rigidbody.angularDrag = Time.time - upsideDown < .1f ?5 : 20;

        avrgVelSlow = Mathf.Lerp(avrgVelSlow, velm, avrgVelSlow < velm ? 100 : Time.deltaTime * .5f);
        avrVel = Mathf.Lerp(avrVel, velm, Time.deltaTime);

        if (!ghost)
        {
            RaycastHit h;
            bool ownGravity = Physics.Raycast(pos, Vector3.down, out h, _Game.isCustomLevel ? 4 : 2, Layer.levelMask) /*groundHit  && upAngle < 60*/; /*!(Time.time - groundedTime < .1f && groundHit) &&  */
            if (ownGravity)
            {
                //print("grodas");
                //rigidbody.useGravity = false;
                //var normal = ZeroY(h.normal);
                //var f = 1 - Vector3.Dot(transform.up, h.normal);
                //Log("ownGravity:" + ownGravity + " Dot:" + f);

                //rigidbody.AddForceAtPosition(res.gravitation * Vector3.down, Vector3.Dot(transform.forward, normal) < 0 ? rigidbody.worldCenterOfMass : rigidbody.worldCenterOfMass + normal * f * (setting.AngleTest ? .2f : 1));
                Vector3 eulerAngles = Quaternion.FromToRotation(transform.up, h.normal).eulerAngles;
                eulerAngles = ClampAngle(eulerAngles);
                rigidbody.AddTorque(eulerAngles * .4f);
                if (Time.time - groundedTime < .1f && groundHit && _GameSettings.gravitationAntiFly > 1)
                    rigidbody.AddForce(res.gravitation * Vector3.down * (_GameSettings.gravitationAntiFly - 1));

                //rigidbody.AddForceAtPosition(res.gravitation * Vector3.down * (_GameSettings.gravitationAntiFly), rigidbody.worldCenterOfMass);
            }
            else
                rigidbody.useGravity = true;
        }
      
        
            AirResistenseAndMass();
        if (camera != null)
        {
            var fieldOfView = ((curCam == CurCam.norm2 || curCam == CurCam.fps) && !splitScreen ? 80 : 65) + _Loader.fieldOfView;
            
            if (_Loader.dm)            
                camera.fieldOfView = fieldOfView * (zoom ? zoomRange: 1f);            
            else
                camera.fieldOfView = Mathf.Min(fieldOfView * 1.5f, Mathf.Lerp(camera.fieldOfView, fieldOfView + Mathf.Max(0, velm - avrVel) * .5f + speed * 3, Time.deltaTime));
        }
        if (!ghost2)
            UpdateRecordBackupReplay();
        if (finnished) return;
        grounded = Time.time - groundedTime < .2f && groundHit;
        if (rigidbody.IsSleeping())
            rigidbody.WakeUp();
        if (grounded)
        {
            if (brake)
            {
                if (!brakeKey)
                    vel = Vector3.Lerp(vel, Vector3.zero, fvBrake * Time.deltaTime * .3f * _GameSettings.Brake);
                vel = Vector3.MoveTowards(vel, Vector3.zero, fvBrake * Time.deltaTime * 30);
            }
            else if ((up || down))
            {
                float max = res.ForceCurve.Evaluate(velm * 4) + skid * 5;
                //print(res.ForceCurve.Evaluate(velm * 4) + "+" + skid);
                float sp = max * (up ? 1 : -1) * .65f * _GameSettings.speed * carSkin.getSpeed() * (_Loader.pursuit && !cop ? Mathf.Lerp(.9f, .5f, (float)_MpGame.redTeam.count / (_MpGame.blueTeam.count + 1)) : 1);
                if (setting.speedTweak)
                    sp *= .7f;
                //if (_Loader.difficulty >= Difficulty.Normal)
                //    sp *= hard ? 1.5f : 1.1f;
                rigidbody.AddForce(ZeroY2(transform.forward) * sp);
            }
            else if (!engineOff)
                vel = Vector3.Lerp(vel, Vector3.zero, Time.deltaTime * .1f);
            movingBack2 = Vector3.Dot(transform.forward, vel) < 0;
            if (down && movingBack2)
                movingBack = true;
            if(up && !movingBack2)
                movingBack=false;
            //movingBack = back;
            //movingBackf = Mathf.Lerp(movingBackf, movingBack ? -1 : 1, Time.deltaTime*10);
        }

        if (speed > 0 && wKey)
            rigidbody.AddForce(transform.forward * (!_Game.isCustomLevel && !nitroDown ? speed * 15 : Mathf.Max(0, (speed * 15) - velm / 3)));
        
        fvBrake = Mathf.Lerp(fvBrake, brake ? 1 : 0, Time.deltaTime * 3);
        
        bool isgrounded = Time.time - groundedTime < .05f;
        Log("isgrounded " + isgrounded);
        if (isgrounded)
        {
            var fv = Vector3.Project(vel, transform.forward);
            var dot = upAngle > 70? upAngle * Mathf.Deg2Rad : 0;
            
            float fvDirt = (dirtMaterial ? .5f : (brakeKey || driftKey ? carSkin.getBrakeDrift(): 1)*carSkin.getDrift()) + fv.magnitude/450f + /*dot **/ dot * 6;
            Log("fvskid:" + fvDirt);
            if (velm > 1 || upAngle > 10)
                fv = Vector3.Slerp(vel, fv, fvDirt * Friq * (_Loader.snow ? .2f : _Loader.rain ? .5f : 1) * fx2 / Mathf.Min(skid / 2 + .5f, 2));
            //if (_Loader.enableMouse)
                //fv = fv.normalized * (fv.magnitude - (fv - vel).magnitude * (res.speedSubOnRotate / f));/* mouse */
            vel = new Vector3(fv.x, (velm < minVel ? vel.y : fv.y), fv.z);

        }
        UpdateRotation();
        if (!grounded)
            Stabilize();
        if (!grounded && easy && !_GameSettings.disableFlyDir)
        {
            Vector3 v = Vector3.Project(vel, transform.forward);
            Debug.DrawRay(pos, v);
            vel = Vector3.Lerp(vel, new Vector3(v.x, vel.y, v.z), Time.deltaTime).normalized * velm;
        }
        grounded2 = false;
        hitForce = Mathf.Lerp(hitForce, 0, Time.deltaTime * 10);
        //Mathf.Lerp(mag2, (oldVel - vel).magnitude, Time.deltaTime);
        //if (oldVel.magnitude < vel.magnitude) damage=Mathf.Max((oldVel - vel).magnitude, damage);

        if (setting.AngleTest)
        {
            var a = rigidbody.angularVelocity;
            a.y = Mathf.Lerp(a.y, 0, Time.deltaTime * 10);
            a.z = Mathf.Lerp(a.z, 0, Time.deltaTime * 10f);
            a.x = Mathf.Lerp(a.x, 0, Time.deltaTime * 10f);
            rigidbody.angularVelocity = a;
        }
        oldVel2 = oldVel;
        oldVel = vel;
        oldAng = rigidbody.angularVelocity;
        
    }
    //private void UpdateCollision(Vector3 pos)
    //{
    //    if (_Loader.enableCollision)
    //    {
    //        if ((pos - _Game.StartPos.position).magnitude < 20)
    //            carBoxEnabled = false;
    //        carBoxEnabled = (pos - _Player.pos).magnitude > 8 || carBoxEnabled;
    //        carBox.enabled = true;
    //        if (this != _Player && _Loader.enableCollision)
    //            Physics.IgnoreCollision(carBox, _Player.carBox, !carBoxEnabled);
    //    }
    //}


    private void UpdateDeadCam()
    {        
        if (dead && !ghost && killedBy != null && !killedBy.dead)
        {
            cam.LookAt(killedBy.transform);
            if (Vector3.Distance(cam.position, killedBy.pos) > 10)
                cam.position = Vector3.Lerp(cam.position, killedBy.pos, Time.deltaTime*5);
        }
    }

    //public float damage;
    private float fvBrake;
    private void Stabilize()
    {
        var f = .5f;
        if (transform.forward.y > -f)
        {
            var d = transform.forward.y + f;
            rigidbody.AddForceAtPosition(Vector3.down * (setting.AngleTest ? 3f : 9) * d, rigidbody.worldCenterOfMass + transform.forward);
            rigidbody.AddForceAtPosition(Vector3.up * (setting.AngleTest ? 3f : 9) * d, rigidbody.worldCenterOfMass - transform.forward);
        }
    }

    
    
    private void AirResistenseAndMass()
    {
        if (_Loader.airFactor != 0)
        {
            float frictionDrag = 30.0f;
            float airDrag = 3.0f * 2.2f;

            float Cdrag = 0.5f * airDrag * 1.29f;
            float Crr = frictionDrag * Cdrag;
            Vector3 Fdrag = -Cdrag * vel * velm;
            Vector3 Frr = -Crr * vel;
            rigidbody.AddForce((Fdrag + Frr) / 2000f * _Loader.airFactor);
        }

        if (Time.time - groundedTime > .5f)
        {
            //if (carSkin.getMass() > 0)
            //rigidbody.AddForce(Vector3.down * (carSkin.getMass()), ForceMode.Acceleration);
            if (_GameSettings.gravitationFactor > 1)
                rigidbody.AddForce(Vector3.down*(_GameSettings.gravitationFactor - 1)*10, ForceMode.Acceleration);
            //var vv = vel;
            //if (vv.y - oldVel2.y > 0)
            //    vv.y = oldVel2.y;
            //vel = vv;



            if (vel.y > 15)
            {
                vel *= .999f;
                //if(isDebug)
                //print(vel.y);
            }
        }
        //if (isDebug && grounded)
        //{
        //    var vv = vel;
        //    if (vel.normalized.y > 0)
        //        vv.y *= .998f;
        //    vel = vv;
        //}
    }

    private Vector3 oldVel2;
    private Vector3 oldVel;
    private Vector3 oldAng;
    internal float upAngle;
    internal float upsideDown=MinValue;
    public float acumulateDamage;
    
    public bool m_dead;
    internal bool dead
    {
        get { return m_dead || !enabled ; }
        set
        {
            speed = 0; m_dead = value;
            if(value)
                deathTime=_Game.timeElapsed;
            if (!ghost)
            {
                if(value)
                    rigidbody.angularDrag = 5;
                rigidbody.useGravity = true;
                rigidbody.drag = value ? 1: 0;
            }
            else
            {
                if (dead && !rigidbody)
                {
                    rigidbody = this.gameObject.AddComponent<Rigidbody>();
                    rigidbody.velocity = this.ghostVel;
                    rigidbody.mass = 1;                    
                }
                if(!dead)
                {
                    Destroy(rigidbody);
                }
            }
        }
    }
    public float suspendTime;
    float meshHitTime;
    //public void Damage(float cnt)
    //{
    //    life -= (int)cnt;
    //    RefreshFire();
    //    if (life < 0)
    //        Die();
    //}

    //private void UpdateFire2()
    //{
    //    if (!android)
    //    {
    //        if (fire[0].activeSelf)
    //            foreach (var a in fire2)
    //                a.worldVelocity = vel;

    //        if (life < 100)
    //            foreach (var a in fire)
    //                a.SetActive(true);
    //        else
    //            foreach (var a in fire)
    //                a.SetActive(false);
    //    }
    //}

    //private void RefreshFire()
    //{
    //    //if (!fire[0]) return;

        
    //}


    public void HitMesh(Collision col,int cnt,int max)
    {
        if (lowQualityAndAndroid) return;
        //foreach(var a in col.contacts)
        //    Debug.DrawRay(a.point, a.normal, Color.blue, 5);
        if (meshTest && cnt > 0 && Time.time - meshHitTime > .5f)
        {
            printLog("hitForce;" + cnt);            
            meshHitTime = Time.time;
            var cp = col.contacts[0];
            meshTest.Hit(cp.point, vel * .5f, cnt, max);
            //foreach (var cp in contactPoints)
            //{
                meshTest.Damage(cp.point, cp.normal);
            //}
        }
    }
    float deathTime;
    //private float oldmag;

    public void OnCollisionEnter2(Collision collisionInfo)
    {
        if (isKinematic || _Game.backTime || dead || ghost2) return;
        //UpdateGhostColl(collisionInfo);
        //var magnitude = (collisionInfo.relativeVelocity + vel).magnitude;
        float mag2 = (vel - oldVel2).magnitude;
        var mag1 = (collisionInfo.relativeVelocity + rigidbody.velocity).magnitude;
        
        
        hitForce += mag2;
        //if (isDebug)
        //{
        //    if (Mathf.Abs(mag1 - oldmag) > 5f)
        //        print(oldmag + " " + mag1);
        //    if (mag1 - oldmag > 10)
        //    {
        //        vel = oldVel;
        //        rigidbody.angularVelocity = oldAng;               
        //        print("newCrash");
        //        return;
        //    }
        //    oldmag = mag1;
        //}
        if (_Loader.race)
            Crash(mag2, collisionInfo);

        mag2 = Mathf.Max(mag1, mag2);

        //if (mag1 > 20 && mag2 < 20)
        //print("hitfail:" + mag2);
        if (mag2 > 15 && Time.time - hitTime > .1f && crCount == 0)
        {
            hitTime = Time.time;            
            foreach (var a in collisionInfo.contacts)
                Sparks(a.point, a.normal);
            if (!(audio.isPlaying && res.hitSoundBig.Contains(audio.clip)))
                PlayCrashHitSound(true);
        }
    }
    //public void OnCollisionEnter2(Collision collisionInfo)
    //{
    //    if (isKinematic || _Game.backTime||dead ) return;
    //    //UpdateGhostColl(collisionInfo);
    //    //var magnitude = (collisionInfo.relativeVelocity + vel).magnitude;
    //    float mag2 = (vel - oldVel2).magnitude;
    //    var mag1 = (collisionInfo.relativeVelocity+ rigidbody.velocity).magnitude;

    //    hitForce += mag2;
    //    if (!_Loader.dm)
    //        Crash(mag2,collisionInfo);

    //    mag2 = Mathf.Max(mag1, mag2);

    //    //if (mag1 > 20 && mag2 < 20)
    //        //print("hitfail:" + mag2);
    //    if (mag2 > 15 && Time.time - hitTime > .1f)
    //    {
    //        hitTime=Time.time;
    //        HitMesh(collisionInfo, (int)((mag2 - 20) / 2));
    //        foreach (var a in collisionInfo.contacts)
    //            Sparks(a.point,a.normal);
    //        if (!(audio.isPlaying && res.hitSoundBig.Contains(audio.clip)))
    //            PlayCrashHitSound(true);
    //    }
    //}

    

    float hitTime;
    private float crashTime;
    private int crCount;
    
    
    private void Crash(float magnitude, Collision collisionInfo)
    {
        //if (crCount > 0)
        //crCount--;
        if (magnitude > 10 && !dead)
        {
            if (crCount < 3)
            //if (Time.time - crashTime > .1f)
            {
                crCount++;
                printLog("Crash detected " + magnitude + " " + crCount);
                Rewind(crCount + 1, true);
                //rigidbody.MovePosition(transform.position + transform.forward* .1f);
                crashTime = Time.time;
            }
            else if (magnitude > (80 * (_GameSettings.gravitationAntiFly + _GameSettings.gravitationFactor) / 2))
            {
                print(magnitude);
                meshHitTime = -1;
                //HitMesh(collisionInfo, 20, 100);
                Die();
            }
            //else
            //    print("Crash Fixed" + (int)Time.time);

        }
        if (Time.time - crashTime > .1f)
            crCount = 0;

    }

    private void Die(bool water = false, int killer = -1)
    {
        if (dead) return;
        

        if (_Loader.dmOrPursuit)
            CallRPC(Die2, killer == -1 ? playerId : killer);
        vel = oldVel2;
        if (water)
        {
            PlayOneShotGui(res.waterSplash);
        }
        else
        {
            //PlayOneShotGui(res.carCrash);
            CallRPC(Explode, pos);
        }
        dead = true;
        if (_Loader.pursuit)
        {
            StartCoroutine(AddMethod(5, delegate
            {
                if (dead)
                {
                    CallRPC(SetTeam, (int)TeamEnum.Blue);
                    CallRPC(Reset);
                }
            }));
        }
        else if (_Loader.dmNoRace)
        {            
            StartCoroutine(AddMethod(5, Reset));
        }
        print("Bum");
    }
    
    private void BlackReset()
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].sharedMaterials = materials[i];
    }
    private void BlackCar()
    {
        foreach (Renderer r in renderers)
        {
            Material[] materials = new Material[r.materials.Length];
            for (int i = 0; i < r.materials.Length; i++)
            {
                Material a = materials[i] = new Material(r.materials[i]);
                a.shader = res.diffuse;
                a.color = Color.black;
            }
            r.materials = materials;
        }
    }
    //private float lastHIt;
    private int lastCheckFrame;
    private float sparkTime;
    public float lastBorderHit;

    public static float Angle(Vector3 from, Vector3 to)
    {
        return (Mathf.Acos(Mathf.Abs(Mathf.Clamp(Vector3.Dot(from.normalized, to.normalized), -1f, 1f))) * 57.29578f);
    }



    private void OnBorderCheck(Collision collisionInfo)
    {

        if (ghost) return;
        if (collisionInfo.collider.tag == Tag.HitBox)
            return;
        if (lastCheckFrame != Time.frameCount)
        {
            lastCheckFrame = Time.frameCount;
            for (int i = 0; i < collisionInfo.contacts.Length; i++)
            {
                ContactPoint a = collisionInfo.contacts[i];


                //float angle = Mathf.Acos((Mathf.Clamp(Vector3.Dot(transform.up, a.normal), -1f, 1f)) * 57.29578f);
                float angle = Angle(transform.up, a.normal);
                if (Application.isEditor && angle > 30)
                    Debug.DrawRay(a.point, a.normal, Color.red, 10);
                
                bool terrain = collisionInfo.collider is TerrainCollider;

                if (angle > (/*Time.time - groundedTime > .8f && upAngle < 60 ? 15 : */40) * (terrain ? .3f : 1)) //(a.point - transform.position).magnitude < 3
                {
                    var m = oldVel2.magnitude;
                    rigidbody.angularDrag = 0;
                    //position.y = pos.y+.5f;
                    if (meshTest)
                    {
                        float mag2 = collisionInfo.relativeVelocity.magnitude;
                        //(vel - oldVel2).magnitude; //(collisionInfo.relativeVelocity + vel).magnitude;
                        //print(mag2);
                        //var isPl = collisionInfo.collider.tag != Tag.HitBox;
                        if (mag2 > 15)
                            HitMesh(collisionInfo, Mathf.Min(5, (int)((mag2 - 20) / 5)), 20);
                        //Damage((mag2 - 20) / 5);
                    }
                    if (setting.enableDrag)
                        vel = Vector3.MoveTowards(vel, Vector3.zero, .05f);
                    //if(angl)
#if oldStunts
                    tempScore = 0;
#endif
                    if (m > 120 / 4 && (_GameSettings.enableBorderHit || _Loader.bombCar) && Time.time - lastBorderHit > .1f)
                    {
                        if (!terrain)
                            Sparks(a.point, a.normal);
                        //oldVel2 = vel;
                        //lastHIt = Time.time;

                        printLog("angle " + angle + " vel " + m * 4);
                        if (!audio.isPlaying)
                            PlayCrashHitSound(false);
                        lastBorderHit = Time.time;
                        rigidbody.AddForceAtPosition(a.normal * Mathf.Min(velm, 30) * (setting.AngleTest ? 40 / rigidbody.angularVelocity.magnitude : 40), rigidbody.worldCenterOfMass + transform.forward * 1);
                        vel *= .8f;
                        //vel *= setting.hitTestForce*(terrain ? .9f : 1);
                        if (_Loader.bombCar)
                            Die();

                        break;
                    }
                }
            }
        }        
    }
    public void Sparks(Vector3 point,Vector3 normal)
    {
        if (Time.time - sparkTime > .1f)
        {
            _Game.Emit(point, normal, _Game.sparks);
            //_Game.sparks[0].Emit();
            //_Game.sparks.Emit();
            //_Game.sparks.Emit();
            sparkTime = Time.time;
        }
    }
    
    private bool nitroDown;
    private bool wKey;
    private bool sKey;
    private bool brakeKey;
    private void UpdateInput()
    {        
        
        if (!visible) return;
        down = brake = up = false;
        sKey = GetKey(KeyCode.S);
        wKey = GetKey(KeyCode.W) || GetKey(KeyCode.LeftShift);
        //if (android)
        //    wKey = wKey || GetKey(KeyCode.LeftShift);
        if (sKey) wKey = false;
        brakeKey = GetKey(KeyCode.Space);
        if (!_Game.started || brakeKey)
            brake = true;
        else if (sKey || wKey)
        {
            if (sKey && (velm < 5 || movingBack))
                down = !engineOff;
            else if (wKey && (velm < 5 || !movingBack))
                up = !engineOff;
            else
                brake = true;
        }
        if (flashMouse == 0)
        {
            left = GetKey(KeyCode.A);
            right = GetKey(KeyCode.D);
        }

        if (GetKeyDown(KeyCode.U))
            flashLight.gameObject.SetActive(!flashLight.gameObject.activeSelf);
        nitroDown = (nitro > 0 || ghost) && GetKey(KeyCode.LeftShift);
        if (nitroDown)
        {
            speed = 3.02f;
            nitro -= Time.deltaTime;
        }
        else if (speed == 3.02f)
            speed = 0;
        //if (GetKeyUp(KeyCode.LeftShift) || nitro < 0)
        //{
        //    speed = 0;
        //    if (nitro < 0)
        //        nitro = 0;
        //}
        if (IsMine && GetKeyDown(KeyCode.X) && grounded && Time.time - lastJump > 1 && (_Loader.dm || cop ||isDebug|| _Game.isCustomLevel && !_Loader.curSceneDef.disableJump))
        {
            lastJump = Time.time;
            CallRPC(Jump);
            rigidbody.transform.position += Vector3.up * .1f;
            rigidbody.AddForceAtPosition(Vector3.up * 1500, rigidbody.worldCenterOfMass + transform.forward * 3);
        }
        //if (IsMine && GetKeyDown(KeyCode.RightControl) && !ghost)
        //{
        //    if (carSkin.Drift > 2)
        //        _Game.centerText("this car dont have drift");
        //    else
        //    {
        //        driftKey = !driftKey;
        //        _Game.centerText("Drift " + (driftKey ? "Enabled" : "Disabled"), 1);
        //    }
        //}
        if (_Loader.dm)
        {
            if (GetKeyDown(KeyCode.Mouse1))
                zoom = !zoom;
            if (GetKeyDown(KeyCode.Mouse2))
                zoom = true;
            if (GetKeyUp(KeyCode.Mouse2))
                zoom = false;
            if (zoom)
                zoomRange = Mathf.Clamp(zoomRange + Input.GetAxis("Mouse ScrollWheel") , .2f, 1.2f);
        }
    }
    
    [RPC]
    private void Jump()
    {
        PlayOneShot(res.jump);
    }

    private void PlayCrashHitSound(bool b)
    {

        var audioClips = b ? res.hitSoundBig : res.hitSound;
        audio.clip = audioClips[Random.Range(0, audioClips.Length)];
        audio.Play();
    }
    internal CurCam curCam;
    
    internal void ChangeCamera(CurCam cam)
    {
        print("change camera");
        curCam=cam;
        curCam = (CurCam)((int)curCam % camPoses.Length);
        model.gameObject.SetActive(!fps);
        camera.transform.parent = camPoses[(int)curCam];
        camera.transform.localPosition = Vector3.zero;
        camera.transform.localRotation = Quaternion.identity;
        //hud.camera.enabled=(!freeCamera);
        //camera.GetComponent<GUILayer>().enabled = true;
        
        //if (freeCamera)
        //    camera.cullingMask &= ~(1 << Layer.nGui);
        //else
        //    camera.cullingMask |= 1 << Layer.nGui;
    }

    public bool forwardMode { get { return (curCam != CurCam.norm2 || _Loader.dmRace) && !freeCamera; } }
    private bool fps { get { return curCam == CurCam.fps; } }
    public bool topdown { get { return curCam == CurCam.topdown; } }
    public bool freeCamera { get { return curCam == CurCam.freecamera; } }
    public enum CurCam { norm, norm2, fps, fps2, topdown, freecamera }
    private void UpdateTimeRevert()
    {
        if (_Loader.dmRace)
        {
            return;
        }
        if (ghost || _Loader.dmOrPursuit)
            return;        
        if (!_Game.started || finnished) return;
        if ((input.GetKeyDown(KeyCode.W) || input.GetKeyDown(KeyCode.S) || input.GetKeyDown(KeyCode.A) || input.GetKeyDown(KeyCode.D)) && _Game.backTime)
        {
            ClearLog();
            _Game.SendWmp("goto", (_Game.timeElapsed + _GameSettings.sendWmpOffset).ToString());
            _Game.backTime = false;
            if (rewinds >= 0)
            {
                replay.backupRetries++;
#if GA
                GA.API.Quality.NewEvent("FlashBack " + _Loader.mapName, _Player.pos);                
#endif
            }
            if (_Game.rewindingPlayer != null)
            {
                _Game.rewindsUsed++;
                _Game.rewindingPlayer.rewinds -= 1;
            }
            foreach (var a in replay.keyDowns.ToArray())
                if (a.time >= _Game.timeElapsedLevel)
                    replay.keyDowns.Remove(a);
        }
        int cut = Mathf.Max(1, (int)(deltaTime * 100));
        if (GetKey(KeyCode.E) && posVels.Count > cut)
        {
            if (rewinds > 0)
            {
                if (backTimeAudio != null && !backTimeAudio.isPlaying && backTimeAudio.enabled)
                    backTimeAudio.Play();
                if (!splitScreen)
                {
                    if (!_Game.backTime)
                    {
                        PlayOneShotGui(_Loader.click);
                        if (meshTest)
                            meshTest.Reset();
                        BlackReset();
                    }
                    _Game.backTime = true;
                }
                if (_Loader.rain)
                    _Game.rainfall.Simulate(deltaTime);
                life += 100*Time.deltaTime;

                Rewind(splitScreen ? 2 : cut);
                //if (splitScreen)
                //    _Player2.Rewind(cut);
                _Game.rewindingPlayer = this;
            }
            else
                _Game.centerText("You dont have flashbacks");
        }
        else
        {
            if (backTimeAudio != null && backTimeAudio.isPlaying)
                backTimeAudio.Stop();
        }
        
        if (_Game.backTime)
            _Game.centerText(string.Format(Tr("Hold {0} button to rewind time, Press {1} to resume"), Tr(android ? "this" : "e key"), Tr(android ? "Acelerate" : "any key")), .1f, false);
    }
    private void Rewind(int cut,bool skip=false)
    {        
        if (_Loader.quality == Quality2.Lowest) return;
        if(posVels.Count<1)return;
        if (!skip)
        posVels.RemoveRange(posVels.Count - cut, cut);
        var last = posVels[posVels.Count - 1];
        if (!skip)
        {
            pos = last.pos;
            cam.position = last.camPos;
            cam.rotation = last.camRot;
        }
        rot = last.rot;
        lap = last.lap;
        if (dead)
        {
            dead = false;
            if (IsMine)
                CallRPC(Reset2);
        }
        rotVel = last.mouserot;
        groundedTime = last.groundTime;
        nitro = last.nitro;
        oldVel = oldVel2 = vel = last.vel;
        engineOff = last.engineOff;
        totalMeters = last.meters;
        checkPointsPass = last.checkPointsPass;
        rigidbody.angularVelocity = last.angVel;
        velSmooth = last.velSmooth;
        if (!online && !setting.timeLapse && !splitScreen)
        {
            _Game.timeElapsedLevel = last.time;
            _Game.FrameCount = last.frameCount;
        }
        upAngle = Vector3.Angle(transform.up, Vector3.up);
        //if (_Game.backTime)
        hitForce = 0;
    }
    internal SecureFloat nitro = 0;
    private static void ShowCenterTextReset()
    {
        if (_Loader.race)
            _Game.centerText(Tr(android ? "Go to Menu->Restart" : "Press R Key to Restart, Hold E key to rewind time"), .1f, false);
    }
    private void UpdateTriggers()
    {
        for (int i = 0; i < _Game.triggers.Count; i++)
        {
            var trigger = _Game.triggers[i];

            bool b = trigger && trigger.collider.bounds.Contains(pos);
            //if (!triggerColOld[i] && b && trigger.tag == Tag.CheckPoint)
            //    b = Vector3.Dot((pos - trigger.transform.position).normalized, transform.forward) > 0;

            if (triggerColOld[i] != b)
                if (b)
                    OnTriggerEnter2(trigger);
                else
                    OnTriggerExit2(trigger);
            triggerColOld[i] = b;
        }
    }
    //internal void UpdateFlying()
    //{
    //    if (_Game.backTime || _Player != this) { lastGroundPos = Vector3.zero; return; }
    //    float metersFlying = lastGroundPos == Vector3.zero ? 0 : (lastGroundPos - pos).magnitude;
    //    if (metersFlying > 200)
    //    {
    //        string str = metersFlying > 1000 ? Mathf.Round(metersFlying / 1000) + "km" : (int)metersFlying + "m";
    //        if (grounded)
    //        {
    //            if (metersFlying > _Loader.flying)
    //            {
    //                _Loader.flying = metersFlying;
    //                _Game.centerText(string.Format(Trs("New Record! {0} flying!"), str), 2, false);
    //            }
    //        }
    //        else
    //            _Game.centerText(string.Format(Trs("{0} flying!"), str), .1f, false);
    //    }
    //    if (grounded)
    //        lastGroundPos = pos;
    //}

    private void FallCheck()
    {

        if (pos.y < _GameSettings.miny && !ghost2 && !dead)
        {
            Die(true);
            //if (_Loader.dm)
            //    CallRPC(SetScore2, score - 5);
            //if (splitScreen)
            //{
            //    pos = _Game.StartPos;
            //    rot = _Game.StartRot;
            //    vel = Vector3.zero;
            //}
            //else
            //    _Game.RestartLevel();
        }
    }
    private float flashMouse;
    public float angularVel;
    private float avrgVelSlow = 50;
    internal float avrVel;
    bool[] triggerColOld = new bool[100];
    //private RaycastHit raycastHit;
    public Vector3 ZeroY2(Vector3 v)
    {
        if (v.y < 0)
        {
            //v = v.normalized*(v.magnitude + -v.y*.5f);
        }
        else if (velm < minVel)
            v.y = 0;
        return v;
    }
    public bool GetKeyUp(KeyCode key)
    {
        if (ghost2)
            return replay.OnKeyUp[(int)key];
        return input.GetKeyUp(key);
    }
    bool GetKey(KeyCode key)
    {
        if (ghost2)
            return replay.OnKey[(int)key];
        return input.GetKey(key);
    }
    bool GetKeyDown(KeyCode key)
    {
        if (ghost2)
            return replay.OnKeyDown[(int)key];
        return input.GetKeyDown(key);
    }
    //internal bool noSkid;
    private float speed;
    public void OnTriggerExit2(Collider other)
    {
        //if (other.tag == Tag.noSkid)
        //    noSkid = false;
        if (other.tag == Tag.speed)
            speed = 0;        
    }
    public bool engineOff;
    public void OnTriggerEnter2(Collider other)
    {
        if (_Loader.race && other.tag == Tag.engineOff)
        {
            if (!ghost)
                _Game.centerText("Engine turned off");
            
            engineOff = true;
        }
        if (other.tag == Tag.speed)
        {
            audio.clip = res.speedUp;
            audio.Play();
            var a = other.GetComponent<Item>();
            speed = a == null ? 10 : a.speed;
        }
        //if (other.tag == Tag.noSkid)
        //    noSkid = true;
        if (other.tag == Tag.CheckPoint)
            engineOff = false;

        if (!_Game.backTime && _Loader.race)
            if (other.tag == Tag.CheckPoint && !checkPointsPass.Contains(other.transform) && (_Game.timeElapsed > 20 || other.name != "Finnish" && other.name != "MultiLapAir" || checkPointsPass.Count == _Game.checkPoints.Length - 1))
                OnFinnishedCheckpoint(other);
        
    }
    
    private AudioClip oldStopAudio;
    private float skid;
    private float oldSpeed;
    
    private void UpdateSounds()
    {
        if (!visible) return;
        var audioClip = dirtMaterial ? res.mudBrake : res.brake;
        if (oldStopAudio != audioClip && stopAudio.enabled)
        {
            stopAudio.clip = oldStopAudio = audioClip;
            stopAudio.Play();
        }        
        var speed = velm * 4 % 70;
        if (!ghost2 && oldSpeed - speed > 10 && velm > oldVel.magnitude)
        {
            PlayOneShot(res.gearChange[Random.Range(0, res.gearChange.Length)], .5f);
            suspendTime = Time.time;
        }
        oldSpeed = speed;
        bool speedUp = (up || down);
        var susp = Time.time - suspendTime < 1 / velm && velm > 1;
        motorAudio.volume = Mathf.Lerp(motorAudio.volume, up && !_Game.started ? 1 : speedUp || down ? Mathf.Min(1, velm / 5) : .5f, Time.deltaTime * 10);
        
        idleAudio.volume = ghost ? 0 : 1 - motorAudio.volume;
        var f = res.gears.Evaluate((velm * 4 / 70) / 10);
        if (grounded && _Game.started)
            motorAudio.pitch = f + speed / (speedUp ? 70 : 140) * (2 - f * 2) + velm * .005f;
        else
            motorAudio.pitch = Mathf.Lerp(motorAudio.pitch, up || down ? 1.5f + velm * .005f : .1f, Time.deltaTime);
        if (windAudio != null)
            windAudio.volume = finnished ? 0 : Mathf.Max(0, velm - 70) / 70;
        if (_Game.started && !ghost)
        {
            skid = skid2;
            skid = stopAudio.volume = !grounded || velm < 2 ? 0 : Mathf.Max(0, skid);
            if (velm < minVelForSkid && !brake && up && !ghost && _Game.started)
                skid = stopAudio.volume = 1;
            if (brake && velm > minVelForSkid && grounded)
                skid = stopAudio.volume = 1;
        }
        if ((!_Game.started || _Game.editControls || finnished || dead || _Game.backTime2))
        {
            motorAudio.volume = idleAudio.volume = stopAudio.volume = 0;
            nitroAudio.Stop();
            if (windAudio != null)
                windAudio.volume = 0;
        }
        if (engineOff|| susp)
            idleAudio.volume = motorAudio.volume = 0;
    }

    private float skid2
    {
        get { return (1 - Mathf.Pow(Mathf.Abs(Vector3.Dot(transform.forward, vel.normalized)), velm / 3)) * 2; }
    }

    public void PlayOneShot(AudioClip AudioClip, float volume=1)
    {        
        if (checkVisible(pos))
            audio.PlayOneShot(AudioClip, volume*_Loader.soundVolume);
    }

    public static bool dirtMaterial;
    RaycastHit camHit;
    bool camHited;
    internal Vector3 turretDirection = Vector3.forward;
    internal Vector3 shootDirection = Vector3.forward;
    public AudioReverbFilter reverbZone;
    private void UpdateCamPos()
    {
        if (finnished || dead) return;
        if ((!firstPlayer && !secondPlayer) && _Loader.sGameType != SGameType.Replay) return;
        if (topdown)
        {
            cam.position = Vector3.Lerp(cam.position, pos, Time.deltaTime * 10);
            var mc = Camera.main.transform;
            cam.transform.localRotation = Quaternion.identity;
            var f = transform.forward;
            mc.localPosition = Vector3.Lerp(mc.localPosition, new Vector3(f.x, f.z, -1.5f) * Mathf.Clamp(velm * 1, 30, 100), Time.deltaTime * 3);
            if (camHited)
            {
                var r = camHit.collider.renderer;
                if (!Nancl && r != null && !res.levelMaterialsTxt.Contains(r.sharedMaterial.name))
                {
                    r.material.shader = res.transparment2;
                    var color = r.material.color;
                    color.a = .5f;
                    r.material.color = color;
                }
            }
        }
        if (!topdown)
        {
            var position = Vector3.Lerp(cam.position, pos, Time.deltaTime * 60);
            if (_Game.isCustomLevel)
                position.y = Mathf.Max(_MapLoader.water.transform.position.y, position.y);
            cam.position = position;


            var lookBack = input.GetKey(KeyCode.Q);
            if (lookBack)
                cam.Rotate(Vector3.up, 180);
            if (UpsideDown)
                cam.Rotate(Vector3.forward, 180);

            cam.rotation = Quaternion.Lerp(cam.rotation, rot, Time.deltaTime * (3 + Quaternion.Angle(cam.rotation, rot) / 10));
            //if (_Loader.dm && !curWeapon.shootForward)
            //{
                UpdateCamDm();

                //cam.Rotate(-ms.y, -ms.x, 0);
            //}
            //else
            //{
            //    //camrot = transform.eulerAngles;
            //    //turretEuler = transform.eulerAngles;
            //}
            //smoothMouseDelta += MouseDelta2()*0.05f;
            //smoothMouseDelta = Vector3.Lerp(smoothMouseDelta, Vector3.zero, Time.deltaTime * 3);
            //cam.Rotate(smoothMouseDelta);
            if (UpsideDown)
                cam.Rotate(Vector3.forward, 180);
            if (lookBack)
                cam.Rotate(Vector3.up, 180);
            if (!ghost && camHited)
                cam.position = camHit.point - (camera.transform.position - cam.position) + camHit.normal * 0.01f;
            //if (curCam == 1)
            //{
            //    camera.transform.localEulerAngles = Vector3.zero;
            //    var eulerAngles = camera.transform.eulerAngles;
            //    eulerAngles.z = Mathf.LerpAngle(eulerAngles.z, 0, 1);
            //    camera.transform.eulerAngles = eulerAngles;
            //}
        }
    }
    //Vector2 smoothMouseDelta;
    //private Vector2 MouseDelta2()
    //{
    //    var mouseDelta2 = getMouseDelta() * (_Loader.sensivity * _Loader.sensivity) * .3f;

    //    return new Vector2(-mouseDelta2.y, mouseDelta2.x);
    //}

    
    private Vector3 camrot;
    private bool UpsideDown
    {
        get { return Time.time - upsideDown < .1f && upAngle > 140; }
    }

    internal string playerNameClan { get { return replay.playerNameClan; } }
    //public Renderer playerNameTxt;
    //public TextMesh playerNameTxt2;
    public GUIText playerNameTxt;
    //internal float playBackTime = 1;
    public int Compare(Player a, Player b)
    {
        if (_Loader.dmOrPursuit)
        {
            if (b.score > a.score)
                return 1;
            if (b.score < a.score)
                return -1;
        }
        float atime = online ? a.replay.finnishTime : a.finnishTime;
        float btime = online ? b.replay.finnishTime : b.finnishTime;
        float at = atime == 0 ? MaxValue : atime;
        float bt = btime == 0 ? MaxValue : btime;
        if (at > bt)
            return 1;
        if (at < bt)
            return -1;
        if (a.lap < b.lap)
            return 1;
        if (a.lap > b.lap)
            return -1;
        var ac = a.checkPointsPass.Count;
        var bc = b.checkPointsPass.Count;
        if (ac > bc)
            return -1;
        if (ac < bc)
            return 1;
        var ameters = a.totalMeters - a.checkPointMeters;
        var bmeters = b.totalMeters - b.checkPointMeters;
        if (ameters > bmeters)
            return -1;
        if (ameters < bmeters)
            return 1;
        if (a.playerNameClan != b.playerNameClan)
            return a.playerNameClan.CompareTo(b.playerNameClan);
        return a.GetHashCode().CompareTo(b.GetHashCode());
    }

#if VOICECHAT
    private VoiceChatPlayer voiceChatPlayer;
#endif
    protected void VoiceChatSend() {  }
    private bool voiceChatFirst;
    internal bool ignoreVoted;
    [RPC]
    public void SendAudio(byte[] data, int length, int id)
    {
#if VOICECHAT
        replay.voiceChatTime = Time.realtimeSinceStartup;
        if (Input.GetKey(KeyCode.F3))
        {
            replay.ignored = true;
            if (!ignoreVoted)
            {
                ignoreVoted= true;
                CallRPC(VoteMute);
            }
        }
        if (Input.GetKey(KeyCode.F4))
            replay.ignored = false;

        if (_Loader.voiceChatVolume < 0.01 || replay.ignored) return;
        voiceChatPlayer.audio.volume = _Loader.voiceChatVolume;
        //if (isDebug)
            //print("receive Audio " + data.Length);
        if (!voiceChatFirst)
        {
            voiceChatFirst = true;
            _Game.centerText(Tr("press F3 To mute user\n") + Tr("press Y To use voice chat"), 2);
        }
        
        voiceChatPlayer.OnNewSample(new VoiceChatPacket() { Compression = VoiceChatCompression.Speex, Data = data, Length = length, NetworkId = id });
#endif
    }
    [RPC]
    public void Reset()
    {
        if(IsMine)
        Debug.Log("Reset");
        //life = _Loader.lifeDef;
        coins = 0;        
        if (_Loader.enableCollision && ghost)
        {
            Destroy(rigidbody);
            carBox.enabled = false;
            if (oldPosVels != null)
                replay.posVels = new List<PosVel>(oldPosVels);
        }
        if (IsMine)
        {
            CallRPC(Reset2);
            ChangeCamera(CurCam.norm2);
        }
        //print(_Game.startPosOffset);
#if oldStunts
        tempScore = oldTempScore = 0;
#endif
        if (!test)
        {
            oldpos = pos = _Game.StartPos.position + _Game.startPosOffset + (cop && isDebug ? Vector3.forward * 5 : Vector3.zero);
            rot = _Game.StartPos.rotation;
        }
        dead = false;
        if (!online)
            score = 0;
        hitForce = 0;
        //print("Reset");

        nitro = online?_Loader.nitro:_Loader.pursuit?3:_Loader.dmRace ? 0 : _Game.isCustomLevel ? _MapLoader.nitro : _Loader.curScene != null ? _Loader.curScene.nitro : 0;
        if (isDebug)
            nitro = 999;
        
        //print("Reset nitro " + nitro + " " + (_MapLoader != null));
  
        speed = 0;
        keys = new bool[330];
        replay.OnKey = new bool[500];
        up = down = left = right = false;
        flashMouse = 0;
        rewinds = online ? _Loader.rewinds : _Loader.levelEditor != null || isMod ? 60 : easy || splitScreen ? 60 : 3;
        engineOff = false;
        //oldGronded=
            grounded = true;
        totalMeters = 0;
        if (!online)
            finnishTime = 0;
        finnished = false;
        checkPointsPass.Clear();
        checkPointMeters = 0;
        if (hud != null)
            hud.checkPointRed.text = hud.checkPoint.text = "";
        grounded = grounded2 = groundHit = true;
        lap = 1;
        if (SkidMarks != null)
            SkidMarks.Reset();
        oldVel2 =oldVel=vel = Vector3.zero;
        if (!isKinematic)
            rigidbody.angularVelocity = rigidbody.velocity = Vector3.zero;
        
        //RefreshFire();
#if oldStunts
        oldRot = transform.eulerAngles;
#endif
        fireTime = MinValue;
        if (!ghost && _Loader.race)
            _Awards.ResetAwards();
    }
    internal int lap = 1;
    public bool visible
    {
        get
        {
            if (!ghost || !lowOrAndorid || (_Player.pos - pos).magnitude < 30)
                return true;
            return visible2;
        }
    }

    private bool visible2
    {
        get
        {
            var wp = _Player.camera.WorldToViewportPoint(pos);
            return new Rect(0, 0, 1, 1).Contains(wp) && wp.z > 0;
        }
    }
    private float seeing = MinValue;
    public bool Seeing(Player pl)
    {
        if (Time.time - pl.seeing < 3) return true;
        Vector3 a = pos + Vector3.up;
        Vector3 b = pl.pos + Vector3.up;
        bool v = Vector3.Dot(transform.forward, (b - a).normalized) > .3 && !Physics.Linecast(b, a, Layer.levelMask);
        if (v)
            pl.seeing = Time.time;
        return v;
    }

    public void UpdateText()
    {
        if (ghost2)
        {
            var highq = (!lowQuality || _Loader.dmOrPursuit);
            
            var mag = (_Player.pos - pos).magnitude;
            var dist = 200;
            if ((playerNameTxt.enabled || FramesElapsed(10)) && highq)
            {
                var position = _Player.camera.WorldToViewportPoint(pos + Vector3.up) + Vector3.up * .05f;
                if (playerNameTxt.transform.position.z < 0)
                {                    
                    position.y = .1f;
                    position.x = Mathf.Clamp(1f - position.x, .2f, .7f);
                    playerNameTxt.transform.position = position;
                }
                playerNameTxt.transform.position = position;
                playerNameTxt.fontSize = (int)Mathf.Lerp(16, 8, mag / dist);
            }
            if (_Loader.pursuit)
                playerNameTxt.enabled = sameTeam || Time.time - _MpGame.stateChangeTime < 10 || _Player.team.players.Any(a => a.Seeing(this));
            else
            {
                playerNameTxt.enabled = (highq); //&& playerNameTxt.transform.position.z > -100

                var allyVisible = _Loader.dm && _MpGame.allyVisible.Contains(playerId) || sameTeam || haveFlag || Time.time - _Player.spawnTime < 7;
                if (!allyVisible)
                    playerNameTxt.enabled = playerNameTxt.enabled && ( /*mag < 100*/ mag < dist && !_Loader.dm || Time.time - showTextTime < 3 && !Physics.Linecast(_Player.camera.transform.position, pos, Layer.levelMask));
            }
            //if (_Loader.team)
            if (_Loader.dmOrPursuit)
                playerNameTxt.color = (teamEnum == TeamEnum.Red ? Color.red : Color.blue);// -new Color(0, 0, 0, Mathf.Min(.7f, mag / 400f));

            
            //if (_Loader.team)
            //{
            //    if (!teamGuiTextRenderer)
            //        teamGuiTextRenderer = teamGuiText.renderer;
            //    teamGuiTextRenderer.enabled = team == _Player.team;
            //    if (teamGuiTextRenderer.enabled)
            //        teamGuiText.transform.LookAt(_Player.camera.transform);
            //}
        }
    }
    public TeamEnum teamEnum {get { return replay.teamEnum; } set { replay.teamEnum = value; }}
    public Team team { get { return _MpGame.teams[(int)replay.teamEnum]; } }
    public TextMesh teamGuiText;
    private Renderer teamGuiTextRenderer;
    internal float showTextTime;
    internal struct State
    {
        internal double timestamp;
        internal Vector3 pos;
        internal Quaternion rot;
    }
    State[] m_BufferedState = new State[20];
    int m_TimestampCount;
    public float interpolationBackTime;
    private void FixedUpdateGhost()
    {
        if (dead && !_Game.backTime2 && _Loader.enableCollision)
            if (_Game.FrameCount % _Game.frameInterval == 0 && _Game.started && quality != Quality2.Lowest)
            {

                var posVel = CrPosVel();
                posVels.Insert(_Game.FrameCount / _Game.frameInterval, posVel);
            }
    }
    

    private void GhostDie(Collision collisionInfo)
    {
        if (!isKinematic) return;
        print("GhostDie " + collisionInfo.gameObject.name);
        this.dead = true;
        
        //this.rigidbody.AddForceAtPosition(collisionInfo.contacts[0].point,
        //    collisionInfo.relativeVelocity + rigidbody.velocity);
    }
    //private bool carBoxEnabled;
    void UpdateGhost()
    {        
        if (!ghost||test)
            return;
        if (_Loader.enableCollision)
        {
            if (dead && !_Game.backTime2)
            {
                if (!lowQuality && Mathf.Abs(Vector3.Dot(transform.forward, rigidbody.velocity.normalized)) < .5f)
                {
                    SkidSmoke(0);
                    SkidSmoke(1);
                }
                if (!visible2 && _Game.timeElapsed - deathTime > 2)
                    dead = false;
                return;
            }
            carBox.enabled = _Game.timeElapsed > 2 && ((pos - _Player.pos).magnitude > 8 || carBox.enabled) && !finnished;
        }
        rotVel = Mathf.Lerp(rotVel, (left ? -3f : right ? 3f : 0), deltaTime * 10);

        if (online)
        {
            double currentTime = PhotonNetwork.time;
            double interpolationTime = currentTime - interpolationBackTime;
            if (m_BufferedState[0].timestamp > interpolationTime)
            {
                for (int i = 0; i < m_TimestampCount; i++)
                {
                    if (m_BufferedState[i].timestamp <= interpolationTime || i == m_TimestampCount - 1)
                    {
                        State rhs = m_BufferedState[Mathf.Max(i - 1, 0)];
                        State lhs = m_BufferedState[i];
                        double length = rhs.timestamp - lhs.timestamp;
                        float t = 0.0F;
                        if (length > 0.0001)
                            t = (float)((interpolationTime - lhs.timestamp) / length);
                        
                        pos= Vector3.Distance(lhs.pos, rhs.pos) > 50 ? rhs.pos : Vector3.Lerp(lhs.pos, rhs.pos, t);
                        rot = Quaternion.Slerp(lhs.rot, rhs.rot, t);
                        return;                        
                    }
                }

                //interpolationBackTime = Mathf.Lerp(interpolationBackTime, d, Time.deltaTime*0.001f);
            }
            else
            {
                //interpolationBackTime = Mathf.Lerp(interpolationBackTime, d, .5f);
                //print("Set interpolationBackTime " + interpolationBackTime);
                State latest = m_BufferedState[0];
                pos = Vector3.Lerp(pos, latest.pos, deltaTime * 20);
                rot = latest.rot;
            }
            
            foreach (var b in recordKeys)
            {
                int c = (int)b;
                replay.OnKeyDown[c] = false;
                replay.OnKeyUp[c] = false;
            }
            return;
        }
        float frameCount = _Game.FrameCount;
        var a = (int)(frameCount / _Game.frameInterval);
        float ostatok = (frameCount / _Game.frameInterval) - a;
        a = Mathf.Min(posVels.Count - 1, a);
        if (a >= posVels.Count - 1 && !finnished)
            EndGame();

        if (a < posVels.Count && a != (int)((frameCount - 1) / _Game.frameInterval))
        {
            vel = posVels[a].vel;
            //rotVel = posVels[a].mouserot;
            if (!_Game.backTime2)
                stopAudio.volume = posVels[a].skid;
            totalMeters = posVels[a].meters;
#if oldStunts
            score = posVels[a].score;
#endif
        }
        if (a < posVels.Count-1 && a != 0)
        {
            pos = Vector3.Lerp(posVels[a-1].pos, posVels[a].pos, ostatok);
            rot = Quaternion.Lerp(posVels[a-1].rot, posVels[a].rot, ostatok);
            //pos = Vector3.Lerp(pos, posVels[a].pos , deltaTime * 10f);
            //rot = Quaternion.Lerp(rot, posVels[a].rot, deltaTime * 15f);
        }
        foreach (var b in recordKeys)
        {
            replay.OnKeyDown[(int)b] = false;
            replay.OnKeyUp[(int)b] = false;
        }
        foreach (KeyDown key in replay.keyDowns)
        {
            if (key.time >= oldTimeElapsed && key.time < _Game.timeElapsedLevel)
            {
                if (_Loader.inputManger.KeyBack.ContainsKey(key.keyCode))
                {
                    int keyCode = (int) _Loader.inputManger.KeyBack[key.keyCode];
                    replay.OnKeyDown[keyCode] = key.down;
                    replay.OnKeyUp[keyCode] = !key.down;
                    replay.OnKey[keyCode] = key.down;
                }
                else 
                    print("keynotfound "+key.keyCode);
            }
        }
        if (ghost)
        {
            RaycastHit h;
            if (Physics.Linecast(pos + transform.up * 2, pos, out h, Layer.levelMask))
            {
                //Debug.Log(h.collider.name,h.collider);
                pos = h.point;
            }
        }
        oldTimeElapsed = _Game.timeElapsedLevel;
    }
    private float oldTimeElapsed;
    internal bool[] keys = new bool[330];
    private void UpdateRecordBackupReplay()
    {
        
        if (_Game.FrameCount % _Game.frameInterval == 0 && _Game.started && quality != Quality2.Lowest)
            posVels.Add(CrPosVel());
    }
    public int coins;

    private PosVel CrPosVel()
    {
        return new PosVel { camRot = cam.rotation, camPos = cam.position, pos = transform.position, rot = transform.rotation, vel = vel, angVel = rigidbody.angularVelocity, time = _Game.timeElapsedLevel, frameCount = _Game.FrameCount, meters = totalMeters, mouserot = rotVel, checkPointsPass = new List<Transform>(checkPointsPass), lap = lap, groundTime = groundedTime, engineOff = engineOff, skid = stopAudio.volume, nitro = nitro, velSmooth = velSmooth };
    }


    private void UpdateRecordBackupReplay2()
    {
        if (ghost2)
            return;
        for (int i = 0; i < recordKeys.Count; i++)
        {
            KeyCode keyCode = recordKeys[i];
            bool down = input.GetKey(keyCode) && (keyCode != KeyCode.LeftShift || nitro > 0) || keyCode == KeyCode.LeftShift && speed > 0;
            if (keys[(int)keyCode] != down)
            {
                keys[(int)keyCode] = down;
                replay.keyDowns.Add(new KeyDown()
                {
                    time = _Game.timeElapsedLevel,
                    down = down,
                    keyCode = (KeyCode)keyCode
                });
                CallRPC(SetKey, (int)keyCode, down);
                
            }
        }
    }
    [RPC]
    public void SetKey(int keyCode, bool down)
    {
        //print("SetKey " + (KeyCode)keycode + " " + down);

        

        if (down)
        {
            replay.OnKeyDown[keyCode] = true;
            replay.OnKey[keyCode] = true;
        }
        if (!down)
        {
            replay.OnKeyUp[keyCode] = true;
            replay.OnKey[keyCode] = false;
        }

    }
    public bool finnished;
    internal float m_finnishTime;
    internal SecureFloat m_finnishTimeSec;
    internal float finnishTime
    {
        get { return online && IsMine ? (float)m_finnishTimeSec : m_finnishTime; }
        set
        {
            if (online && IsMine) m_finnishTimeSec = value;
            else m_finnishTime = value;
        }
    }

    internal int m_IsMine=-1;
    public bool IsMine
    {
        get
        {
            //return photonView.isMine;
            if (offlineMode) return !ghost;
            if (m_IsMine==-1)
                m_IsMine = photonView.isMine ? 1 : 0;
            return m_IsMine == 1;
        }
    }



    //public Vector3 m_vel;
    public Vector3 vel
    {
        get { return isKinematic ? ghostVel : rigidbody.velocity; }
        set
        {
            if (!isKinematic) rigidbody.velocity = value; 
            ghostVel = value;
        }
    }
    internal List<Transform> checkPointsPass = new List<Transform>();
    private static int minVelForSkid = 5;
    Dictionary<int, float> checkPointTime = new Dictionary<int, float>();
    private void OnFinnishedCheckpoint(Collider other)
    {
        if (!ghost)
        {
            //var s = _Loader.prefixMapPl + lap + ":" + checkPointsPass.Count;
            var key = _Game.checkPoints.Length * lap + checkPointsPass.Count;
            var r = checkPointTime.TryGetDontSet(key, float.MaxValue);// PlayerPrefsGetFloat(s, float.MaxValue);
            if (_Game.timeElapsed < r && !ghost)
                checkPointTime[key] = _Game.timeElapsed;
            hud.checkPoint.text = hud.time.text;
            hud.checkPointRed.renderer.material.color = (_Game.timeElapsed > r ? Color.red : Color.blue);
            hud.checkPointRed.text = r == float.MaxValue ? "" : TimeToStr(_Game.timeElapsed - r, true);
            StartCoroutine(AddMethod(2, delegate { hud.checkPointRed.text = hud.checkPoint.text = ""; }));
            PlayOneShotGui(res.checkPoint);
        }
        checkPointMeters = totalMeters;
        checkPointsPass.Add(other.transform);
        if (checkPointsPass.Count == _Game.checkPoints.Length && _Game.checkPoints.Length > 0)
        {
            if (lap >= _GameSettings.laps && !ghost)
                EndGame();
            else
                checkPointsPass.Clear();
            lap++;
        }
    }

    
    public void EndGame()
    {
        if (!_Loader.dm)
            finnished = true;
        if (_Loader.enableCollision&&ghost)
            carBox.enabled = false;
        //carBoxEnabled = false;
        finnishTime = _Game.timeElapsed;
        if (replay.finnishTime > finnishTime || replay.finnishTime == 0)
        {
            replay.finnishTime = finnishTime;
            if (online)
            {
                CallRPC(SetFinnishTime, finnishTime);
                SetCustomProperty(ServerProps.score2 + ":" + playerNameClan, finnishTime);
            }
        }
        _Game.finnishedPlayers.Add(this);
        //if (ghost && this == _Player)
        //    _Game.EndGame(this);
        if (!ghost)
        {
            _Game.finnishedPlayer = this;
            _Game.EndGame();
        }
    }
    
    private AudioSource InitSound(AudioClip audioClip, bool play = true)
    {
        var au = gameObject.AddComponent<AudioSource>();
        au.clip = audioClip;
        au.loop = true;
        dopler(au);
        if (this == _Player)
            au.priority = 0;
        if (play)
        {
            au.Play();
            au.volume = 0;
        }
        return au;
    }
    private void dopler(AudioSource au)
    {
        au.dopplerLevel = ghost ? 1 : 0;
    }
    public float fx2 { get { return Time.fixedDeltaTime / 0.02f; } }
    private static List<Transform> GetTrs(Transform tr)
    {
        var tt = new List<Transform>();
        foreach (Transform a in tr)
            tt.Add(a);
        return tt;
    }
    public bool firstPlayer;
    public bool secondPlayer;
    internal float velm;
    
    public void OnDestroy()
    {
        if (Loader.loadingLevelQuit) return;
        if (hud != null)
            Destroy(hud.gameObject);
        if (androidHud != null)
            Destroy(androidHud.gameObject);
        if (input != null && input != _Loader.inputManger)
            Destroy(input.gameObject);
        if (cam != null)
            Destroy(cam.gameObject);
        if (SkidMarks != null)
            Destroy(SkidMarks.gameObject);
        if (playerNameTxt != null)
            Destroy(playerNameTxt.gameObject);
        if (_Game != null)
            _Game.listOfPlayers.Remove(this);
    }
    //private int step;
    //private float LastTimeSend;
    [RPC]
    public void Kick()
    {
        if (this == _Player)
        {
            print("<<<<<<<speedhack>>>>>>>>>");
            //_Loader.StartCoroutine(AddMethod(() => Application.loadedLevelName == Levels.menu && win.act != null,delegate { _Loader.ShowPopup("Error 666", win.act); }));
            PhotonNetwork.LeaveRoom();
        }
    }
    [RPC]
    internal void VoteMute()
    {
        var count = _Game.listOfPlayers.Count;
        int f = (int)(count * .4f);
        
        if (count > 2 && voted > f && IsMine)
            CallRPC(Mute, 10);

        if (isMod || Application.isEditor)
            _GameGui.Chat(string.Format(Tr("ban {0} from chat, voted:{1}/{2}"), replay.playerName, voted, count));
    }
    public int voted;
    
    [RPC]
    internal void Mute(int time)
    {
        if (_Player == this)
        {
            _Game.centerText("You have been muted for " + time + " seconds");
            _Loader.banTime = Loader.totalSeconds + time * 60;
        }
        if (time < 1)
            _GameGui.Chat(replay.playerName + Tr(" unbanned from chat"));
        else
            _GameGui.Chat(replay.playerName + Tr(" banned from chat"));

    }
}