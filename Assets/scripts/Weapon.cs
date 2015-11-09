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

public class Weapon:WeaponBase
{
    
    public Bullet bullet;

    public float accuracy = .04f;
    public int bullets;
    public int bulletsTotal;
    //public float rotationSpeed = 1;
    public float damage=11;
    public Light muzzleFlashLight;
    public Renderer muzzleFlash;
    public ParticleEmitter[] capsule;

    public AudioClip[] shootSound;
    private float lastShoot;



    public override void Update()
    {
        //Debug.DrawRay(turretCannon.position, pl.turretDirection.normalized * 3);
        if (pl.dead||pl.froozen) return;
        if (_Loader.dmRace && pl.IsMine && (pl.pos - _Game.StartPos.position).magnitude < (checkVisible(_Game.StartPos.position) ? 500 : 100)) return;
        base.Update();

        if (muzzleFlash != null)
        {
            muzzleFlash.enabled = Time.time - lastShoot < .03f;
            muzzleFlashLight.enabled = Time.time - lastShoot < .05f;
        }
        //recoil = Vector3.Lerp(recoil, Vector3.zero, Time.deltaTime * 1);
       
        if (shooting)
        {
            if (shootTm >= shootInterval)
            {
                shootTm = shootTm % shootInterval;
                //recoilPos += new Vector3(Random.value * -recoil.y, Random.Range(-1, 2) * recoil.x) * (.5f+recoilPos.magnitude*.1f);
                lastShoot = Time.time;
                if (shootSound.Length > 0)
                    pl.PlayOneShot(shootSound[Random.Range(0, shootSound.Length)]);
                foreach (var a in capsule)
                    a.Emit();
                if (pl.IsMine)
                {
                    

                    //if (pl != null && pl.distanceToCursor.magnitude < 3)
                    //    print("ShootMg " + pl.distanceToCursor.magnitude);
                    //pl.CallRPC(Shoot, targetPlayer == null ? -1 : targetPlayer.playerId, targetPlayer == null ? Vector3.zero : targetPlayer.distanceToCursor);
                    pl.CallRPC(Shoot, pl.TargetPlayerId , pl.distanceToCursor2);
                }
            }
        }
        
    }
    public override void UpdateAlways()
    {
        shootTm += Time.deltaTime;
        base.UpdateAlways();
    }
    private int shootCount;
    public override void Shoot(int plId, Vector3 dist, PhotonMessageInfo info)
    {
        //print(plId+":"+dist);
        if (plId != -1 && !_Game.photonPlayers.ContainsKey(plId)) return;
        //pl.turretDirection = pl.turretDirection + Random.insideUnitSphere;
        if (bullet != null)
        {
            shootCount++;
            var t = barrels[shootCount%barrels.Length];
            //Quaternion r = plId == -1 ? Quaternion.LookRotation(turretCannon.forward + Random.insideUnitSphere * accuracy) : Quaternion.LookRotation(-(turretCannon.position - _Game.photonPlayers[plId].pos + dist));
            pl.distanceToCursor2 = dist;
            pl.TargetPlayerId = plId;
            pl.UpdateTurretEuler(true);

            var b = _Pool.Load(bullet.gameObject, t.position, Quaternion.LookRotation(pl.shootDirection.normalized)).GetComponent<Bullet>();
            //var b = (Bullet)Instantiate(bullet, t.position, Quaternion.LookRotation(pl.shootDirection.normalized));
            b.pl = pl;
            //b.gameObject.hideFlags = HideFlags.HideInHierarchy;
            //b.owner = info.sender;
            
            b.wep = this;
            b.extraTime = (float) (PhotonNetwork.time - info.timestamp);
            
        }
    }
    public override void SetShoot(bool b)
    {
        shootTm = shootInterval;
        shooting = b;
    }
}
