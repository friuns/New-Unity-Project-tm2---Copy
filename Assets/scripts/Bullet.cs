using System.Globalization;
//using Edelweiss.DecalSystem;
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

public class Bullet : bs
{
    public bool freeze;
    public float speedUp;
    private int maxSpeed2 = 150;
    public float bulletSpeed = 1500;
    private float bulletSpeed2;
    public int maxSpeed;
    public bool targetable;
    public GameObject explosion;
    //public PhotonPlayer owner;
    public Weapon wep;
    public float randomMax = 0;
    public Player pl;
    private Vector3 vel;
    internal float extraTime;
    internal float velm;
    private int frame;
    public bool rocket;
    public float explosionForce = 10000;
    public float explosionRadius = 25;
    public bool remote = true;

    public void Start()
    {
        bulletSpeed2 = bulletSpeed+pl.velm;
        maxSpeed2 = maxSpeed+(int)pl.velm;
        var r = Random.Range(0,randomMax);
        foreach (Transform a in transform)
            a.position += transform.forward * r;
        
    }
    public void OnEnable()
    {
        vel = Vector3.zero;
        extraTime = 0;
        frame = 0;
        velm = 0;
    }
    public void Destroy2(GameObject obj)
    {        
        _Pool.Save(obj.transform);
    }
    public void Update()
    {
        var time = Mathf.Min(extraTime, Time.deltaTime * 3);
        extraTime -= time;
        var deltaTime = Time.deltaTime + time;

        if (deltaTime == 0) return;
        frame++;

        //var emitter = _Game.smoke2;
        //emitter.transform.position = pos;
        //if (frame < 5)
        //    emitter.Emit(1);

        vel = transform.forward * bulletSpeed2 * deltaTime;
        if (velm > 1000)
            Destroy2(gameObject);
        velm += vel.magnitude;
        if (speedUp != 0)
        {
            bulletSpeed2 += speedUp * deltaTime;
            bulletSpeed2 = Mathf.Min(bulletSpeed2, maxSpeed2);
        }

        //RaycastHit h ;
        foreach (RaycastHit h in Physics.RaycastAll(transform.position, transform.forward, vel.magnitude, Layer.allmask).OrderBy(a => a.distance))
        {

            var IsMine = wep.pl == _Player;
            var bs = h.transform.root.GetComponent<bs>();
            var zombie = bs as Zombie;
            if (zombie != null && IsMine)
            {
                wep.pl.ZombieHit(zombie, 0);
            }
            var hitPl = bs as Player;
            //if (hitPl != null && b && online && h.collider.tag == Tag.HitBox)
            //if (hitPl.IsMine && !hitPl.dead)
            if (hitPl != null && online && h.collider.tag == Tag.HitBox)
            {
                var b = remote ? !IsMine && hitPl.IsMine : IsMine;
                var b2 = hitPl != wep.pl;
                if (b && b2 && !hitPl.dead)
                {
                    if (wep.pl != null && (wep.pl.teamEnum != hitPl.teamEnum || _Loader.dmNoCtf))
                    {
                        //var p = Mathf.Max(2f - CurvySpline2.DistancePointLine(pl.rigidbody.worldCenterOfMass, transform.position, transform.position + transform.forward * 10), 0) / 2;
                        //print(CurvySpline2.DistancePointLine(pl.rigidbody.worldCenterOfMass, transform.position, transform.position + transform.forward * 10));
                        hitPl.CallRPC(hitPl.SetLife, hitPl.life - wep.damage, wep.pl.playerId);

                        if (freeze)
                        {
                            //pl.freeze += .3f;
                            //if (pl.freeze > 1)
                            hitPl.CallRPC(hitPl.Freeze);
                        }
                    }                 
                }
                if (hitPl != wep.pl && !hitPl.sameTeam && _Player.checkVisible(h.point))
                {
                        hitPl.meshTest.Damage(h.point, h.normal);
                        hitPl.meshTest.Hit(h.point, vel);
                    _Game.Emit(h.point, h.normal, _Game.sparks);
                }
            }

            //CreateDecal(h);
            if ((hitPl != wep.pl || hitPl == null))
            {
                if (explosion != null)
                {
                    if (rocket && (IsMine || _Player.teamEnum != wep.pl.teamEnum))
                    {
                        _Player.rigidbody.AddExplosionForce(explosionForce, h.point, explosionRadius);
                        float damage = (explosionRadius - (h.point - _Player.pos).magnitude) / explosionRadius * wep.damage;
                        if (damage > 0)
                            _Player.CallRPC(_Player.SetLife, _Player.life - damage, wep.pl.playerId);
                        Destroy2(gameObject);
                    }
                    Destroy(Instantiate(explosion, h.point + h.normal, Quaternion.identity), 2);

                }

                if (wep.bulletHitSound.Length > 0 && checkVisible(pos) /*|| h.collider.tag == Tag.model*/&& Vector3.Distance(_Player.camera.transform.position, pos) < 100)
                {
                    var audioClip = wep.bulletHitSound[Random.Range(0, wep.bulletHitSound.Length)];
                    PlayAtPosition(h.point, audioClip, hitPl && hitPl.IsMine ? 0 : 200);
                }
            }
            if (bs == null)
            {
                if (!lowQuality)
                {
                    _Game.Emit(h.point, h.normal, _Game.bulletHit);
                    Hole(hole != null ? hole : res.hole, h.point, h.normal);
                }
                Destroy2(gameObject);
                break;
            }

        }
        if (wep.pl.IsMine && targetable)
        {
            _Game.cursorTexture.enabled = false;
            _Game.cursorTexture2.enabled = true;
            var r = _Player.camera.ViewportPointToRay(new Vector3(.5f, .5f));
            var lineStart = r.origin;
            var pointOnLine = CurvySpline2.ProjectPointLine(pos, lineStart, lineStart + r.direction * 1000);
            //Debug.DrawLine(pos, pointOnLine);
            transform.forward = Vector3.RotateTowards(transform.forward, (pointOnLine - lineStart).normalized, 1 * Time.deltaTime, 0);
            //transform.forward = Vector3.RotateTowards(transform.forward, (pointOnLine - (pos - r.direction * 10)).normalized, .5f * Time.deltaTime, 0);
        }

        if (wep.slide)
        {

            RaycastHit h2;
            if (Physics.Raycast(pos, -transform.up, out h2, 2, Layer.levelMask))
            {
                //if (pos.y - h2.point.y < 2)
                //pos = h2.point + Vector3.up*2;
                //pos = Vector3.Lerp(pos, h2.point + Vector3.up * 2, Time.deltaTime * 10);
                pos += Vector3.up * Time.deltaTime * 5;
            }
        }
        transform.position += vel;
    }
    
    public static void Hole(GameObject original, Vector3 position, Vector3 vector3)
    {
        var g = (GameObject)Instantiate(original, position, Quaternion.LookRotation(vector3) * Quaternion.Euler(0, 0, Random.Range(0, 360)));
        g.hideFlags = HideFlags.HideInHierarchy;
        Destroy(g, highQuality ? 100 : 20);
    }
    public GameObject hole;
    

    private static void PlayAtPosition(Vector3 position, AudioClip audioClip, int priority=200)
    {
        GameObject o = new GameObject();
        o.hideFlags = HideFlags.HideInHierarchy;
        var au = o.AddComponent<AudioSource>();
        au.transform.position = position;
        au.clip = audioClip;
        au.priority = priority;
        au.Play();
        Destroy(o, audioClip.length * Time.timeScale);
    }

    //private void CreateDecal(RaycastHit h)
    //{
    //    var m = h.collider as MeshCollider;
    //    if (m != null)
    //    {
    //        Mesh sharedMesh = m.sharedMesh;
    //        if (sharedMesh.isReadable)
    //        {
    //            Vector3 l_ProjectorPosition = h.point - (0.5f * transform.forward);
    //            Quaternion l_ProjectorRotation = ProjectorRotationUtility.ProjectorRotation(-h.normal, Vector3.up);
    //            Quaternion l_RandomRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
    //            l_ProjectorRotation = l_ProjectorRotation * l_RandomRotation;
    //            DecalProjector l_DecalProjector = new DecalProjector(l_ProjectorPosition, l_ProjectorRotation, Vector3.one * 4,
    //                80.0f, .1f, 0, 0);
    //            //m_DecalProjectors.Add(l_DecalProjector);
    //            _Game.m_DecalsMesh.AddProjector(l_DecalProjector);
    //            Matrix4x4 l_WorldToMeshMatrix = h.collider.renderer.transform.worldToLocalMatrix;
    //            Matrix4x4 l_MeshToWorldMatrix = h.collider.renderer.transform.localToWorldMatrix;
    //            _Game.m_DecalsMesh.Add(sharedMesh, l_WorldToMeshMatrix, l_MeshToWorldMatrix);
    //            _Game.m_DecalsMeshCutter.CutDecalsPlanes(_Game.m_DecalsMesh);
    //            _Game.m_DecalsMesh.OffsetActiveProjectorVertices();
    //            _Game.m_DecalsInstance.UpdateDecalsMeshes(_Game.m_DecalsMesh);
    //        }
    //    }
    //}
}
