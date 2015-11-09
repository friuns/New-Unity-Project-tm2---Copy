using System.Collections.Generic;
using UnityEngine;

public class Sgo{}
public class ModelObject:bs
{
    public Material[] oldMaterials;
    public Vector3 initPos;
    public bool flying;
    public Vector3 key;
    public string name2;
    private float m_scale=1;
    public float scale
    {
        get
        {
            return m_scale;
        }
        set
        {
            if (value != m_scale)
            {
                m_scale = value;
                transform.localScale = Vector3.one * m_scale;
            }
        }
    }
    public static HashSet<Vector3> taken = new HashSet<Vector3>();
    public static List<Bounds> takenBounds= new List<Bounds>();
    public override void Awake()
    {
        if(string.IsNullOrEmpty(name2))
            name2 = name;
        taken.Add(transform.position);
        takenBounds.Add(renderer.bounds);
        //    _Loader.levelEditor.HideGroupAdd(gameObject);
        //if (renderer != null && oldMaterials==null)
            //oldMaterials = renderer.sharedMaterials;
        base.Awake();
    }
    
    public void SetColor(Color c)
    {
        foreach (var a in renderer.materials)
        {
            a.shader = bs.res.diffuse;
            a.color = new Color(c.r, c.g, c.b, .85f);
        }
    }
    public void ResetColor()
    {
        if(oldMaterials!=null)
        renderer.materials = oldMaterials;
        if (collider != null)
            collider.enabled = true;
    }
    private Renderer m_renderer;
    public new Collider collider { get { return renderer.collider; } }
    public new Renderer renderer
    {
        get
        {
            if (m_renderer == null)
                m_renderer = GetComponentInChildren<Renderer>();
            return m_renderer;
        }
    }
    public void OnDestroy()
    {
        taken.Remove(pos);
        takenBounds.Remove(renderer.bounds);
        if (_Loader.levelEditor != null)
            _Loader.levelEditor.selection.Remove(this);
    }
    public void Test()
    {
        
    }
}