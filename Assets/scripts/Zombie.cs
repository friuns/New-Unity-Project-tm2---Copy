

using System.Collections;
using System.Linq;
using UnityEngine;

public class Zombie:bsNetwork
{
    //public AnimationClip walk;
    //public AnimationClip run;
    public new Animator animation;
    public bool dead;
    public float dieTime;

    Rigidbody[] rigidbodies;
    private Collider[] colliders;
    private Renderer[] renderers;
    Vector3 startPos;
    Quaternion startRot;
    public Collider trigger;
    private GameObject model;
    bool cop { get { return false; } }
    //public static Transform zombies;
    public IEnumerator Start()
    {
        
        enabled = false;
        while (model == null)
        {
            model = (GameObject)LoadRes(cop ? "copModel" : "zombieModel");
            if (model == null)
                yield return new WaitForSeconds(1);
            else
                model = (GameObject)Instantiate(model, pos + model.transform.position, rot);
        }
        enabled = true;
        //if (zombies == null)
        //    zombies = new GameObject("zombies").transform;
        //transform.parent = zombies;
        //model.transform.parent = zombies;
        //model.transform.parent = transform;
        model.hideFlags = gameObject.hideFlags = HideFlags.HideInHierarchy;
        animation = model.GetComponentInChildren<Animator>();
        hips = model.GetComponentsInChildren<Transform>().FirstOrDefault(a => a.tag == "hips");
        rigidbody = model.GetComponentInChildren<Rigidbody>();
        colliders = model.GetComponentsInChildren<Collider>().Where(a => a.name != "Cube" && a != cc.collider).ToArray();
        rigidbodies = model.GetComponentsInChildren<Rigidbody>();
        renderers = model.GetComponentsInChildren<Renderer>();
        
        foreach (var r in colliders)
        {
            r.material = new PhysicMaterial();
            r.tag = "zombie";
            r.enabled = false;
        }
        foreach (var r in rigidbodies)
        {
            r.mass *= .01f;
            r.isKinematic = true;
        }
        foreach (var a in renderers)
            a.enabled = false;
        dead = false;
        if (PhotonNetwork.isMasterClient)
            transform.forward = ZeroY(Random.insideUnitSphere).normalized;
        startPos = pos;
        startRot = rot;        
        _Game.zombies.Add(this);
    }
    public Transform hips;
    public new Rigidbody rigidbody;
    //public Transform hitVisible;
    public AudioClip[] zombieHit;
    public AudioClip[] zombieGroan;
    //void OnControllerColliderHit(ControllerColliderHit hit)
    //{
        
    //    //if(hit.normal.y<.5f)
    //        //forward = ZeroY(Random.insideUnitSphere).normalized;
    //    //Rigidbody body = hit.collider.attachedRigidbody;
    //    //if (body == null || body.isKinematic)
    //    //    return;

    //    //if (hit.moveDirection.y < -0.3F)
    //    //    return;

    //    //Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
    //    //body.velocity = pushDir * pushPower;
    //}
    //float visTime;
    public int sl = 0;
    public bool forceSleep;
    public void Update()
    {
        var wp = _Player.camera.WorldToViewportPoint(hips.position);
        
        var inCam = new Rect(0, 0, 1, 1).Contains(wp) && wp.z > 0;
        RaycastHit h2;
        visible = inCam && !Physics.Linecast(_Player.camera.transform.position, hips.position, out h2, Layer.levelMask);
        //hitVisible = h2.transform;
        if(visible)
            Debug.DrawLine(pos,_Player.pos,Color.white);
        bool ragdoll = visible && dead;
        cc.enabled = !dead;
        animation.enabled = visible && !dead;
        if (visible != oldVisible)
            foreach (var a in renderers)
                a.enabled = visible;
        if (ragdoll != oldRagDoll)
        {
            foreach (var a in rigidbodies)
            {
                if (a.isKinematic == ragdoll)
                    a.WakeUp();                    
                a.isKinematic = !ragdoll;                
            }
            
            foreach (var r in colliders)
                r.enabled = ragdoll;
        }
        if (ragdoll && visible && rigidbody.velocity.magnitude < .1 && Time.time - dieTime > 1)
            foreach(var a in rigidbodies)
                a.Sleep();
        if (isDebug)
        {
            sl = 0;
            foreach (var a in rigidbodies)
            {
                if (!a.IsSleeping())
                    sl++;
                //if(forceSleep)
                //    a.Sleep();
            }
        }
        if (visible && !dead&& Time.time - lastGroan > 3)
        {
            lastGroan = Time.time + Random.value * 3;
            audio.clip = zombieGroan[Random.Range(0, zombieGroan.Length)];
            audio.Play();
            if (PhotonNetwork.isMasterClient)
                CallRPC(SetPos, pos, ZeroY(Random.insideUnitSphere).normalized);
        }
        trigger.enabled=visible && !ragdoll;


        //cc.enabled = animation.enabled = !dead && visible;

        if (!dead)
        {
            float dt = visible ? 0.3f : .5f;
            if ((online || visible) && TimeElapsed(dt))
            {
                var deltaTime = Mathf.Max(Time.deltaTime, dt);
                var move = 1*transform.forward + vel;
                if (cc.Move(move*deltaTime) == CollisionFlags.None)
                    vel += Vector3.down*deltaTime*9.8f;
                else
                    vel = Vector3.zero;

                if (vel.y < -30)
                    ResetZombie();
            }
        }

        if (visible)
        {
            float f = Vector3.Distance(model.transform.position, transform.position) > 3 ? 1 : Time.deltaTime * 3;
            model.transform.position = Vector3.Lerp(model.transform.position, transform.position, f);
            model.transform.rotation = Quaternion.Lerp(model.transform.rotation, transform.rotation, f);
            //visTime += Time.deltaTime;
            //var running = (_Player.pos - pos).magnitude < 20;
            //if (animation.enabled)
            //{
            //    //if (running)
            //        //forward = ZeroY(pos - _Player.pos).normalized;
                
            //    //transform.forward = forward;
            //    animation.CrossFade(/*running ? run.name : */cop ? "run" : "walk");
            //}
        }
        oldVisible=visible;
        oldRagDoll=ragdoll;
    }
    private Vector3 vel;
    bool oldVisible;
    bool oldRagDoll;
    public CharacterController cc;
    
    public float timeVisible = MinValue;
    public bool visible { get { return Time.time - timeVisible < 1; } set { if (value)timeVisible = Time.time; } }
    //private Vector3 forward;


    
    float lastGroan;
    public static int zombieKills;
    

    public void ResetZombie()
    {
        vel = Vector3.zero;
        
        pos = startPos;
        hips.transform.position = pos + Vector3.up;
        rot = startRot;
        dead = false;
    }
    [RPC]
    public void Hit(Vector3 hitPos, Vector3 addForce, int killedBy)
    {
        if (dead || !enabled) return;
        StartCoroutine(Hit2(hitPos, addForce, killedBy));
    }
    public IEnumerator Hit2(Vector3 hitPos, Vector3 addForce, int killedBy)
    {
        if (_Game.photonPlayers.ContainsKey(killedBy))
            yield return new WaitForSeconds(_Game.photonPlayers[killedBy].interpolationBackTime);
        pos = hitPos;
        audio.clip = zombieHit[Random.Range(0, zombieHit.Length)];
        audio.Play();
        dead = true;
        dieTime = Time.time;
        Update();
        if (killedBy != myId && visible && online)
            hips.rigidbody.velocity = addForce * 10;

        if(online)
        StartCoroutine(AddMethod(60, ResetZombie));
    }
    
    //[RPC]
    //private void SetDead(bool dead)
    //{
    //    this.dead = dead;
    //    //animation.enabled = !dead;
    //    //foreach (var a in colliders)
    //    //    a.enabled = dead;
    //    //foreach (var a in rigidbodies)
    //    //    a.isKinematic = !dead;
    //}
    public override void OnPlConnected()
    {
        CallRPC(SetPos, pos, transform.forward);
        base.OnPlConnected();
    }
    
    [RPC]
    public void SetPos(Vector3 pos, Vector3 rot)
    {
        this.pos = pos;
        transform.forward = rot;
    }
}