using System.Globalization;
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

public partial class Player
{
    //public Transform turretPlace;
    public WeaponBase[] weapons = new WeaponBase[0];
    internal float spawnTime;
    public SecureFloat score;
    public int scoreInt { get { return (int)score; } }
    private Vector3? camDefPos;
    internal SecureInt deaths;
    internal SecureInt kills;
    //public Transform turret;
    //public Transform turretCannon;
    //public Renderer muzzleFlash;
    
    //public ParticleSystem damage;
    //public WeaponBase rocketWeapon;
    //[RPC]
    //public void ShootRocket(PhotonMessageInfo phm)
    //{

    //    UpdateTurretEuler();
    //    rocketWeapon.Shoot(TargetPlayerId, distanceToCursor2, phm);
    //}
    public void UpdatePlayerDm()
    {
        var onfire = Time.time - fireTime < 3;
        fire.SetActive(onfire);
        if (onfire)
            life -= 15 * Time.deltaTime;
        freeze = Mathf.Lerp(freeze, 0, Time.deltaTime);
        ice.SetActive(froozen);

        if(dead)return;
        if (TimeElapsed(.3f))
            SetLife2(Mathf.Min(life + 1, lifeDef));

        

        if (!froozen)
        {
            foreach (var a in weapons)
                a.UpdateAlways();
            if (IsMine && !_Loader.dmRace)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i < weapons.Length && weapons[i].show &&
                        curWeaponId != i)
                        CallRPC(SelectWeapon, i);
                }
                //if (Input.GetKeyDown(KeyCode.Alpha4))
                //{
                //    CallRPC(ShootRocket);
                //}

            }
        }
        if (this.IsMine)
        {
            var targetPlayer = _Game.listOfPlayers.Where(a => a != _Player && !a.dead && a.enabled && a.distanceToCursor.magnitude < 50).OrderBy(a => a.distanceToCursor.magnitude).FirstOrDefault();
            this.TargetPlayerId = targetPlayer != null ? targetPlayer.playerId : -1;
            this.distanceToCursor2 = targetPlayer != null ? targetPlayer.distanceToCursor : this.curWeapon.turretCannon.forward;
        }
        else
        {
            var plPos = pos;
            float dist;
            Plane plane = new Plane((_Player.curWeapon.turretCannon.position - plPos).normalized, plPos);
            tempTr.position = plPos;
            tempTr.forward = (_Player.curWeapon.turretCannon.position - plPos);


            Ray ray = new Ray(_Player.curWeapon.turretCannon.position, _Player.curWeapon.turretCannon.forward);

            if (!plane.Raycast(ray, out dist))
                this.distanceToCursor = _Player.curWeapon.turretCannon.forward * 99999;
            else
            {
                Vector3 p = ray.GetPoint(dist);
                Debug.DrawLine(p, plPos);
                this.distanceToCursor = tempTr.InverseTransformPoint(p);
            }

            UpdateTurretEuler();

            //print(pl.distanceToCursor);
        }
       
    }
    public bool froozen
    {
        get { return Time.time - freezeTime < 3f; }
    }
    public GameObject fire; 
    public void UpdateTurretEuler(bool shooting=false)
    {

        Transform turretCannon = this.curWeapon.turretCannon;
        if (this.distanceToCursor2 != Vector3.zero)
        {
            // Quaternion.LookRotation((pos - _Game.photonPlayers[pl.TargetPlayerId].pos + pl.distanceToCursor2)).eulerAngles;
            //print(pl.TargetPlayerId + " " + pl.distanceToCursor2);
            //if (pl.TargetPlayerId == -1)
            if (!_Game.photonPlayers.ContainsKey(this.TargetPlayerId) || _Game.photonPlayers[this.TargetPlayerId] == null)
            {
                this.turretDirection = this.shootDirection = this.distanceToCursor2;
            }
            else
            {
                
                tempTr.position = _Game.photonPlayers[this.TargetPlayerId].pos;
                tempTr.forward =turretCannon.position - tempTr.position;
                this.turretDirection = this.shootDirection = tempTr.TransformPoint(this.distanceToCursor2) - turretCannon.position;
                //if(shooting)
                //    Debug.DrawRay(turretCannon.position, shootDirection, Color.red, 5);
                //else
                //    Debug.DrawRay(turretCannon.position, shootDirection);
            }
            //pl.turretDirection = pl.TargetPlayerId == -1 ? pl.distanceToCursor2 : -(plPos - photonPlayer.pos + pl.distanceToCursor2);
        }
    }

    [RPC]
    public void Shoot(int plId, Vector3 dist, PhotonMessageInfo info)
    {
        curWeapon.Shoot(plId, dist, info);
    }
    public AudioSource sirene;
    
    [RPC]
    public void SetTeam(int team)
    {
        if (_Loader.dmOnly) return;
        replay.teamEnum = (TeamEnum)team;
        if (_Loader.pursuit)
        {
            
            LoadSkin();
            Destroy(sirene);
            if (cop)
            {                
                AudioSource cl = sirene = gameObject.AddComponent<AudioSource>();
                cl.clip = res.sirene;
                cl.loop = true;
                cl.Play();
            }
        }
    }
    public int wasCop;
    [RPC]
    public void SetKills(int i)
    {
        kills = i;
    }
    [RPC]
    public void SetDeaths(int i)
    {
        deaths = i;
    }
    [RPC]
    public void Die2(int k)
    {        
        dead = true;
        Player killerpl = _Game.photonPlayers[k];

        if (IsMine && killerpl != null)
        {
            freeCamera2.i = _Game.listOfPlayers.IndexOf(killerpl);
            ChangeCamera(CurCam.freecamera);
        }


        if (IsMine)
        {
            //if (_Loader.dm)
            //    CallRPC(SetScore2, score - 5);
            CallRPC(SetDeaths, deaths + 1);
            if (killerpl == this)
                _Game.centerText(Tr("Suicided"));
            else
                _Game.centerText(string.Format(Tr("{0} killed You"), killerpl.playerNameClan, ""), 3);
        }
        else if (k == myId)
        {
            if (_Loader.dm)
                killerpl.CallRPC(SetKills, killerpl.kills + 1);
            killerpl.CallRPC(SetScore2, killerpl.score + (_Loader.pursuit ? 3 : 10));
            _Game.centerText(string.Format(Tr("You killed {1} {2} Points"), "", playerNameClan, 10), 3);
        }
        

    }
    [RPC]
    public void Explode(Vector3 pos)
    {
        if (_Loader.dmOrPursuit)
            BlackCar();
        //{

            //var instantiate = (GameObject)Instantiate(res.turretExp, curWeapon.turret.parent.position, curWeapon.turret.parent.rotation);
            //Destroy(instantiate, 5);
            //if (rigidbody != null)
            //    rigidbody.AddExplosionForce(1000, pos + Vector3.down, 100);
            //foreach (Transform a in instantiate.transform)
            //    a.rigidbody.AddExplosionForce(1000, pos + Vector3.down, 100);
            //curWeapon.turret.parent.gameObject.SetActive(false);
        //}

        if(checkVisible(pos))
        meshTest.Hit(pos, Vector3.zero, 20, 100);
        StartCoroutine(AddMethod(_Loader.dm ? 0 : interpolationBackTime, delegate
        {
            var g = (GameObject)Instantiate(res.explosion);
            Destroy(g, 5);
            g.transform.position = pos;
            if (IsMine)
                g.audio.priority = 0;
        }));
    }
    public float resetTime;
    [RPC]
    public void Reset2(PhotonMessageInfo info)
    {
        killedBy = null;
        resetTime=Time.time;
        dead = false;
        SetLife2(lifeDef);
        //    curWeapon.turret.parent.gameObject.SetActive(true);
        BlackReset();
        if (meshTest)
            meshTest.Reset();

        spawnTime = Time.time;// + (float)(PhotonNetwork.time - info.timestamp);
        if (_Loader.enableCollision && ghost)
            carBox.enabled = false;
    }

    private float lifeDef
    {
        get
        {
            if (_Loader.team && _MpGame.blueTeam.players.Any() && _MpGame.redTeam.players.Any())
            {
                var fc = (float)_MpGame.redTeam.count / _MpGame.blueTeam.count;
                var f = Mathf.Lerp(1, (replay.red ? 1f / fc : fc), .4f);
                return _Loader.lifeDef * f;
            }
            return _Loader.lifeDef;

        }
    }
    internal int curWeaponId;
    public WeaponBase curWeapon { get { return weapons[curWeaponId]; } }
    
    private void UpdateCamDm()
    {
        if (!_Loader.dm) return;
        
    
        if (!_Loader.dmShootForward && !forwardMode)
        {
            Vector3 ms = getMouseDelta() * (_Loader.sensivity * _Loader.sensivity) * .3f * (zoom ? zoomRange: 1);
            if (!Screen.lockCursor)
                ms = Vector3.zero;
            camrot += new Vector3(-ms.y, ms.x * 2);
            camrot.x = Mathf.Clamp(camrot.x, -70, 85);
            cam.localEulerAngles = camrot;
            //if (curWeapon.addRotation)
            //cam.localEulerAngles += transform.eulerAngles;
        }
        if (forwardMode)
            Screen.lockCursor = false;
        RaycastHit h = new RaycastHit();
        if (camDefPos == null)
            camDefPos = camera.transform.localPosition;
        camera.transform.localPosition = camDefPos.Value;
        if (Physics.Linecast(camera.transform.position + camera.transform.forward * (4), camera.transform.position, out h, Layer.levelMask))
            camera.transform.position = h.point;
        if (zoom)
            camera.transform.position = curWeapon.turretCannon.position + camera.transform.up*.3f;

        var viewportPointToRay = camera.ViewportPointToRay(forwardMode  ? mousePosition2 : Vector3.one / 2);
        var turpos = curWeapon.turretCannon.position;
        if (!Physics.Raycast(viewportPointToRay, out h, 1000, Layer.levelMask))
            h.point = turpos + viewportPointToRay.direction * 1000;

        turretDirection = h.point - turpos;

            //Quaternion.LookRotation(transform.InverseTransformDirection(turretDirection)).eulerAngles;

        //if (forwardMode)
        //{
        //    ms
        //}
        //else
        {
            if (!Physics.Raycast(turpos, curWeapon.turretCannon.forward, out h, 1000, Layer.levelMask))
                h.point = turpos + curWeapon.turretCannon.forward*1000;


            var w = camera.WorldToViewportPoint(h.point);
            _Game.cursorTexture.enabled = w.z > 0;
            _Game.cursorTexture.color = curWeapon.shootTm > curWeapon.shootInterval || curWeapon.shootInterval < .2f
                ? new Color(.5f, .5f, .5f, .5f)
                : new Color(.1f, .1f, .1f, .5f);
            _Game.cursor.position = Vector3.Lerp(_Game.cursor.position, w, Time.deltaTime*20);
            //_Game.cursor.parent.guiText.enabled = !curWeapon.shootForward;
            _Game.cursorTexture2.enabled = false;
        }
        //if (Physics.SphereCast(turpos, 2, turretCannon.forward, out h, 1000, 1 << Layer.car))
        //{
        //    var pl = h.transform.root.GetComponent<Player>();
        //    if (pl != null)
        //        showTextTime = Time.time;
        //}
    }
    private static Vector3 mousePosition2
    {
        get
        {
            var v = Input.mousePosition;
            v.x /= Screen.width;
            v.y /= Screen.height;
            return v;
        }
    }
    [RPC]
    public void SelectWeapon(int id)
    {
        
        //var old = curWeapon;
        curWeaponId = id;
        DeactiveOrInitWeps();
        curWeapon.gameObject.SetActive(true);
        curWeapon.OnSelect();
        if (IsMine)
            _Game.cursorTexture.texture = curWeapon.cursor != null ? curWeapon.cursor : res.defCursor;

        //curWeapon.turret.rotation = old.turret.rotation;
        //curWeapon.turretCannon.rotation = old.turretCannon.rotation;

    }
    string ColorToHex(Color32 color)
    {
        string hex = "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
        return hex;
    }
    public GameObject ice;
    public int zombieKills;
    [RPC]
    public void SetZombieKills(int score)
    {
        zombieKills = score;
    }
    [RPC]
    public void SetScore2(float score)
    {
        if (_Loader.race) return;        
        //print(playerName + " Set Score " + score);
        //if ()
        //{
        //    if (firstPlace)
        //    {
        //        if (_Loader.dm)
        //            DeathMatchOrCtf.Add();
        //        else
        //            WinInMultiplayerRace.Add();
        //    }
        //}     
        if (IsMine)
        {
            SetCustomProperty(ServerProps.score2 + ":" + playerNameClan, score);
            _Player.hud.PlayScore((score - this.score).ToString("+0;-0"), Color.red);
        }
        this.score = score;
    }
    [RPC][Obsolete]
    public void SetScore(int score)
    {
        //print(playerName + " Set Score " + score);
        this.score = score;
    }
    
    public SecureFloat life = 100;
    //public float lifeDef = 300;
    Player killedBy;
    public float lastHitTime;

    [RPC]
    public void SetVel(Vector3 f)
    {
        vel = f;
    }
    [RPC]
    internal void SetLife(float nlife, int killer)
    {

        //print("SetLife");
        if (Time.time - spawnTime < 5 || dead)
            return;
        if (_Loader.pursuit && velm > 10f && IsMine)
            CallRPC(SetVel, vel * (velm > 30f ? .5f : .9f));
        lastHitTime = Time.time;
        //print("Set life " + playerName);
        //if (IsMine)
            //damage.Emit(1);
        var damage = life - nlife;
        SetLife2(nlife);
        
        if (killer == myId)
        {            
            showTextTime = Time.time;

            _Player.nitro += _Loader.dmRace ? 1 : _Loader.pursuit ? .1f * damage : 0;
            if (damage > 1 && _Loader.pursuit)
            {                
                _Player.hud.PlayScore(((int) damage).ToString(), Color.blue);
                _Player.score += damage / 50f;
            }

            _Awards.damageDeal += (int)damage;
        }
        
        if (IsMine && killer != -1 && killer != myId)
        {
            hud.damageAnim.Rewind();
            hud.damageAnim.Play();
            killedBy = _Game.photonPlayers[killer];
            killedBy.showTextTime = Time.time;
            var angle = Quaternion.LookRotation(killedBy.pos - pos).eulerAngles.y;
            angle = Mathf.DeltaAngle(angle, cam.eulerAngles.y);
            hud.damage.eulerAngles = new Vector3(0, 0, angle);
            //if (killedBy.curWeapon.weaponEnum == WeaponEnum.machinegun)
            //{
            //    PlayOneShot(res.lifeHitSound);
            //    PlayOneShot(res.bulletHitSound[Random.Range(0, res.bulletHitSound.Length)]);
            //}
        }
        //var c = Color.green * (life / 100f);
        //c.a = 1;
        //print((life / 100f) + c.ToString() + ColorToHex(c));
        if (killer == myId)
        {
            if (damage > 0)
                DamageText((-(int)damage).ToString());
            PlayOneShotGui(res.hitFeedback);
        }
        
        
        if (IsMine && life < 0)
        {
            if (!dead)
                Die(false, killer);
        }
        if (_Loader.pursuit)
            PlayCrashHitSound(true);
    }
    internal float freezeTime=MinValue;
    internal float freeze;
    [RPC]
    internal void Freeze()
    {
        freezeTime = Time.time;
    }
    [RPC]
    internal void SetOnFire()
    {
        fireTime = Time.time;
    }
    [RPC]
    internal void SetLife2(float obj)
    {
        if (float.IsNaN(obj))
        {
            Debug.LogError("Set Nan detected ");
            life = 66;
        }
        else
            life = obj;
        RefreshText();
    }
    public void RefreshText()
    {
        if (_Loader.dmNoRace)
        {
            var text = playerNameClan + "\n" + new string('▄', Mathf.Max(0, Mathf.CeilToInt(life/_Loader.lifeDef*10)));
            if (_Loader.team && teamEnum != bs._Player.teamEnum)
                text = "<" + bs._Loader.Trn("Enemy") + ">\n" + text;
            playerNameTxt.text = text;
        }
    }
    [RPC]
    public void SetShoot(bool b)
    {
        curWeapon.SetShoot(b);
    }
    private float fireTime = MinValue;
    [RPC]
    public void SetWasCop(int Obj)
    {
        wasCop = Obj;
    }

    private Vector3 syncPos;
    private Quaternion syncRot;
    protected void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        var v = vel;
        var sk = skid;
        syncPos = pos;
        syncRot = rot;
        stream.Serialize(ref v);
        stream.Serialize(ref sk);
        if (_Loader.dm)
        {
            stream.Serialize(ref distanceToCursor2);
            stream.Serialize(ref TargetPlayerId);
        }
        stream.Serialize(ref syncPos);
        stream.Serialize(ref syncRot);
        //}
        if (stream.isReading)
        {
            if (stopAudio != null)
                stopAudio.volume = sk; 
            vel = v;
            //ping = Mathf.Max(0, (float)(PhotonNetwork.time - info.timestamp));
            ping = Mathf.Lerp(ping, Mathf.Max(0, (float)(PhotonNetwork.time - info.timestamp)), .1f);
            if (_Loader.mpCollisions)
            {
                if (Vector3.Distance(rigidbody.position, syncPos) > clampMax)
                {
                    transform.position = syncPos;
                    transform.rotation = syncRot;
                }
                else
                {
                    var c = Time.time - collisionTime > 1;
                    if (Vector3.Distance(rigidbody.position, syncPos) > clampMax && c)
                    {
                        transform.position = syncPos;
                        transform.rotation = syncRot;
                    }
                    syncPos += vel * ping;
                    Debug.DrawLine(pos, syncPos, Color.yellow, 2);
                    rigidbody.velocity = vel;
                    offsetPos = syncPos - pos;
                    offsetRot = syncRot * Quaternion.Inverse(rot);
                }
                Debug.DrawLine(pos, syncPos, Color.yellow, 2);
            }
            else
            {
                for (int i = m_BufferedState.Length - 1; i >= 1; i--)
                    m_BufferedState[i] = m_BufferedState[i - 1];

                float f = Mathf.Min(2, (float)(PhotonNetwork.time - m_BufferedState[0].timestamp) + .2f);
                interpolationBackTime = setting.lagNetw ? 3 : Mathf.Lerp(interpolationBackTime, f, interpolationBackTime > f ? .01f : .5f);
                State state;
                state.timestamp = info.timestamp;
                state.pos = syncPos;
                state.rot = syncRot;
                m_BufferedState[0] = state;
                m_TimestampCount = Mathf.Min(m_TimestampCount + 1, m_BufferedState.Length);
                for (int i = 0; i < m_TimestampCount - 1; i++)
                    if (m_BufferedState[i].timestamp < m_BufferedState[i + 1].timestamp)
                        Debug.Log("State inconsistent");
            }

        }

    }
    private int clampMax = 10;
    private Vector3 offsetPos;
    private Quaternion offsetRot;
    private float ping;
}