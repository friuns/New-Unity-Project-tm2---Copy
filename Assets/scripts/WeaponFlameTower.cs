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

public class WeaponFlameTower : WeaponBase
{
    public ParticleSystem[] emitors;
    public Light fireLight;
    public AudioSource fireSound;
    public float soundStart = 1.3f;
    public float soundEnd=2f;
    public override void Update()
    {
        foreach (ParticleSystem a in emitors)
            a.enableEmission = shooting;
        fireLight.enabled = shooting;
        if (pl.dead || pl.froozen)
        {
            shooting = false;
            return;
        }

        if (shooting)
        {
            fireLight.range = Mathf.Lerp(fireLight.range, Random.Range(4, 7), Time.deltaTime*50);
            if (!fireSound.isPlaying)
            {
                fireSound.time = 0; 
                fireSound.Play();
            }
            if (fireSound.time > soundEnd)
                fireSound.time = soundStart;
        }
        else if (fireSound.isPlaying)
        {
            fireSound.Stop();
            
        }

        base.Update();
    }
    public override void SetShoot(bool b)
    {
        shooting = b;
        base.SetShoot(b);
    }
    private ParticleSystem.CollisionEvent[] collisionEvents = new ParticleSystem.CollisionEvent[16];
    public new ParticleSystem particleSystem;
    public void OnParticleCollision(GameObject other)
    {
        int safeLength = particleSystem.safeCollisionEventSize;
        if (collisionEvents.Length < safeLength)
            collisionEvents = new ParticleSystem.CollisionEvent[safeLength];

        int numCollisionEvents = particleSystem.GetCollisionEvents(other, collisionEvents);

        var h = collisionEvents[Random.Range(0, numCollisionEvents)];
        if (Random.value < 0.1f && other.tag == Tag.Untagged)
        {
            Destroy(Instantiate(res.fire, h.intersection, Quaternion.identity), 5);            
            Bullet.Hole(res.hole2,h.intersection,h.normal);

        }
        else if (pl.IsMine && other.layer == Layer.hitBox)
        {
            var enemy = other.transform.root.GetComponent<Player>();
            if (enemy && enemy != _Player && !enemy.sameTeam)
            {
                enemy.life -= 5;
                if (Time.time - enemy.lastHitTime > .1f)
                {
                    enemy.CallRPC(enemy.SetLife, (float)enemy.life, pl.playerId);
                    enemy.CallRPC(enemy.SetOnFire);
                }
            }
        }



        //int i = 0;
        //while (i < numCollisionEvents)
        //{
        //    //if (other.rigidbody)
        //    //{
        //    Vector3 pos = collisionEvents[i].intersection;
        //    //    Vector3 force = collisionEvents[i].velocity * 10;
        //    //    other.rigidbody.AddForce(force);
        //    //}
        //    Debug.DrawLine(_Player.pos, pos);
        //    i++;
        //}
    }

}