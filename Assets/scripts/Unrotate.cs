using UnityEngine;

public class Unrotate:MonoBehaviour
{
    private Transform t;
    
    public void Start()
    {
        t = transform;
    }
    private Vector3 los;
    public void Update()
    {
        var s = Vector3.one*10;
        if (t.lossyScale != los)
        {
            los = t.lossyScale;
            var p = t.parent;
            t.parent = null;
            t.localScale = s;
            t.parent = p;
        }
    }
}