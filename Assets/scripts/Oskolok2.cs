using UnityEngine;

public class Oskolok2 : bs
{
    public void Start()
    {
        old = pos;
        var i = 5;

        //Transform t = transform;
        //Transform child = t.GetChild(0);
        //child.parent = null;
        //t.position = child.collider.bounds.center;
        //child.parent = t;
        gameObject.hideFlags = HideFlags.HideInHierarchy;
        Destroy(constantForce, i);
        Destroy(rigidbody, i);
        Destroy(this, i);
        Destroy(gameObject, 15);
    }
    public AudioSource au;
    Vector3 old;

    public Vector3 vel;
    private bool played;
    void OnCollisionEnter(Collision collision)
    {
        if (!played)
        {
            au = gameObject.AddComponent<AudioSource>();
            played = true;
            au.clip = res.oskolok.Random();
            au.Play();
        }
    }
    public void Update()
    {
        //RaycastHit h;
        //Vector3 v = pos-old;
        if (Physics.Linecast(old, pos, Layer.levelMask))
        //if (rigidbody.SweepTest(v, out h, v.magnitude))
        {
            pos = pos - (pos - old) * 2;
            rigidbody.velocity *= 0;
        }
        //if (Physics.Linecast(old, pos, Layer.levelMask))
        //{
        //    pos = old;
        //    Destroy(constantForce);
        //    Destroy(rigidbody);
        //}

        old = pos;

        //else
        //{
        //    rota = Vector3.zero;
        //    transform.up = up;
        //}
    }
}