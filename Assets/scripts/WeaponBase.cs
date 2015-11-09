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

public class WeaponBase : bs
{
    public WeaponEnum weaponEnum;
    public float radiusLimit=0;
    public float rotationSpeed = 1;
    public bool slide;
    public bool show { get { return GetFlag((int)_Loader.weaponEnum, (int)weaponEnum); } }
    internal Player pl;
    internal int id;
    public AudioClip draw;
    public Transform[] barrels;
    public Texture2D cursor;
    public Transform turretCannon { get { return barrels[0]; } }
    internal bool shooting;
    public override void Awake()
    {
        if (turret == null)
        {
            turret = turretCannon;
            //var t = turret = new GameObject("turret").transform;
            //t.transform.parent = this.transform;
            //t.transform.localPosition = Vector3.zero;
            //t.transform.localRotation= Quaternion.identity;
        }
        base.Awake();
    }
    public Transform turret;
    public virtual void SetShoot(bool b)
    {
        
    }
    public AudioClip[] bulletHitSound;
    public void OnSelect()
    {
        pl.PlayOneShot(draw);
    }
    public virtual void Shoot(int plId, Vector3 dist, PhotonMessageInfo info)
    {
        
    }
    public Vector3 recoilPos;
    public Vector3 recoil;
    public float RecoilRev = 10;

    
    internal float shootTm = MaxValue;
    public float shootInterval = .100111f;
    public virtual void Update()
    {
        
        //shield.enabled = Time.time - spawnTime < 5;
        recoilPos = Vector3.ClampMagnitude(recoilPos, 5);
        recoilPos = Vector3.MoveTowards(recoilPos, Vector3.zero, Time.deltaTime * RecoilRev);
        recoilPos = Vector3.Lerp(recoilPos, Vector3.zero, Time.deltaTime * RecoilRev * .01f);

        
        
        if (pl.IsMine)
        {
            var mb = input.GetKey(KeyCode.Mouse0) && (Screen.lockCursor || pl.forwardMode) && !pl.freeCamera && _Game.started && !_Game.finnish;
            if (mb != shooting && (shootTm > shootInterval || this is WeaponFlameTower))
                pl.CallRPC(SetShoot, mb);
        }
    }

    public virtual void UpdateAlways()
    {
        
        var turretEuler = Quaternion.LookRotation(transform.InverseTransformDirection(pl.turretDirection)).eulerAngles;
        if (radiusLimit != 0)
        {
            turretEuler = ClampAngle(turretEuler);
            //turretEuler.x = Mathf.Min(0, turretEuler.x);
            turretEuler = Vector3.ClampMagnitude(turretEuler, radiusLimit);
        }
        if (_Loader.dmShootForward)
            turretEuler = Vector3.zero;
        if (turret != null)
        {

            {
                Vector3 eu = turret.localEulerAngles;
                eu.y = Mathf.MoveTowardsAngle(eu.y, turretEuler.y + recoilPos.y, Time.deltaTime*100*rotationSpeed);
                turret.localEulerAngles = eu;
            }
            {
                Vector3 eu = turretCannon.localEulerAngles;
                eu.x = Mathf.MoveTowardsAngle(eu.x, turretEuler.x + recoilPos.x, Time.deltaTime * 100 * rotationSpeed);
                foreach (var a in barrels)
                    a.localEulerAngles = eu;
                //turretCannon.localEulerAngles = eu;
            }
        }
    }
    public Vector3 plPos { get { return pl.pos; } }
    
}