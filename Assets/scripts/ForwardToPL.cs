using UnityEngine;

public class ForwardToPL:bs
{
    public Player pl;
    public bool remove;
    public override void Awake()
    {
        if (remove || pl !=_Player)
        {
            Destroy(GetComponent<ConfigurableJoint>());
            Destroy(rigidbody);
        }
        //else
        //{
        //    transform.parent = null;
        //}

    }
    public void Start()
    {
        
    }
    public void OnCollisionEnter(Collision collisionInfo)
    {
        pl.OnCollisionEnter(collisionInfo);
    }
    public void OnCollisionStay(Collision collisionInfo)
    {
        pl.OnCollisionStay(collisionInfo);
    }
    public void OnTriggerEnter(Collider other)
    {
        pl.OnTriggerEnter(other);
    }
}