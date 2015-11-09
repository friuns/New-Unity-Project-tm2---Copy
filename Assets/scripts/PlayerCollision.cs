
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


public partial class Player
{
    public float collisionTime;
    public void OnCollisionEnter(Collision collisionInfo)
    {

        if ((_Loader.enableCollision || splitScreen) && !dead)
        {
            var player = collisionInfo.transform.root.GetComponent<Player>();
            //if (player != null && collisionInfo.rigidbody != null /*&& collisionInfo.rigidbody.velocity.magnitude < rigidbody.velocity.magnitude*/)
            //_Player.AddScore(100 + (int)collisionInfo.relativeVelocity.magnitude, Color.blue);
            if (player != null && !player.dead && !player.finnished)//&& Vector3.Dot(transform.forward, player.pos - pos) > 0
            {

                if (player.isKinematic && player.ghost)
                    player.GhostDie(collisionInfo);

                if(!isKinematic)
                {
                    collisionTime = Time.time;
                    //HitMesh(collisionInfo, 10, 10);
                    float mag = (collisionInfo.relativeVelocity - rigidbody.velocity).magnitude;
                    if (mag > 8)
                    {
                        bool b = _Loader.pursuit && ((IsMine || player.IsMine) && cop && player.teamEnum != teamEnum) || IsMine && _Loader.dmOnly && _Loader.enableCollision && player.vel.magnitude < vel.magnitude;
                        //if (b || offlineMode)
                        player.HitMesh(collisionInfo, 20, 20);
                        if (b)
                        {
                            player.acumulateDamage += mag / 3;
                            if (player.acumulateDamage > 10)
                            {
                                player.CallRPC(SetLife, player.life - player.acumulateDamage, playerId);
                                player.acumulateDamage = 0;
                            }
                        }
                        if (!audio.isPlaying)
                            PlayCrashHitSound(true);
                    }
                }

            }
            if (!isKinematic)
            {
                vel = oldVel;
                rigidbody.angularVelocity = oldAng;
            }
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        //if (_Loader.enableZombies)
        //{
        if (IsMine)
        {
            var z = other.transform.root.GetComponent<Zombie>();
            if (z && !z.dead)
            {
                Zombie.zombieKills++;
                _Awards.ZombieKills.Add();
                if (cop || !_Loader.pursuit)
                    nitro += 1;
                ZombieHit(z, _Game.listOfPlayers.Count > 1 || isDebug ? _Loader.pursuit ? 1 : 3 : 0);
            }
            //}
        }
    }
    public void ZombieHit(Zombie z, int points = 3)
    {
        if (_Loader.dmOrPursuit && points > 0)
        {
            //_Game.centerText(string.Format(Tr("You killed {1} {2} Points"), "", "zombie", points), 3);
            CallRPC(SetZombieKills, zombieKills + 1);
            CallRPC(SetScore2, score + points);            
        }
        z.CallRPC(z.Hit, z.pos, vel, playerId);

    }
    //float groundedTime2 ;
    public void OnCollisionStay(Collision collisionInfo)
    {

        //if (ghost)
        //{
        //if (visible2)
        //HitMesh(collisionInfo, (int)(collisionInfo.relativeVelocity + rigidbody.velocity).magnitude * 3, 20);
        //return;
        //}
        if (isKinematic || _Game.backTime || dead || ghost || froozen) return;
        if (collisionInfo.collider.tag == "zombie") return;
#if oldStunts
        groundedTime2 = Time.time;
#endif
        //Debug.Log(collisionInfo.gameObject.name, collisionInfo.gameObject);
        OnCollisionEnter2(collisionInfo);
        OnBorderCheck(collisionInfo);        
        if (groundHit)
        {
            grounded2 = true;
            groundedTime = Time.time;
        }
        else if (Time.time - groundedTime > .2f)
        {
            float f = .99f;
            upsideDown = Time.time;
            if (UpsideDown)
                vel = new Vector3(vel.x * f, vel.y, vel.z * f);
            /* UPSIDEDOWN*/
        }
        //lastGroundPos = pos;
        if (!setting.AngleTest && setting.enableDrag)
        {
            var m = (vel - oldVel2).magnitude;
            rigidbody.angularDrag = Mathf.Min(rigidbody.angularDrag, Mathf.Max(0, 20 - m));
        }
    }
}
