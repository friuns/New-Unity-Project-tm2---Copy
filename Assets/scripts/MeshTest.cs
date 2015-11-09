using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class MeshTest: bs
{
    public MeshFilter mf;
    public class Tr
    {
        public int[] ind;
        public int[] pointers ;
        public int group;
        public bool went;
    }
    Dictionary<int, List<Tr>> lab;
    //internal static List<int> triangles;
    internal Vector3[] vertices;
    internal Vector3[] oldVertices;
    internal Vector3[] normals;
    //internal Color32[] crs;
    internal Vector2[] uvs;
    internal Vector4[] tangents;
    List<Tr> flat = new List<Tr>();
    public List<Element> elements = new List<Element>();
    public void Start2()
    {

        if (mf == null)
            mf = GetComponent<MeshFilter>();
        elements.Clear();
        //mf.gameObject.AddComponent<MeshCollider>();
        //mf.gameObject.layer = Layer.car;
        lab = new Dictionary<int, List<Tr>>();
        //triangles = mf.mesh.triangles.ToList();
        vertices = mf.mesh.vertices;
        oldVertices = (Vector3[])vertices.Clone();
        normals = mf.mesh.normals;
        tangents = mf.mesh.tangents;
        uvs = mf.mesh.uv;
        //crs = new Color32[mf.mesh.colors32.Length];
        for (int x = 0; x < mf.mesh.subMeshCount; x++)
        {
            var trs = mf.mesh.GetIndices(x);
            for (int i = 0; i < trs.Length; i += 3)
            {
                var tr = new Tr() { ind = new[] { trs[i], trs[i + 1], trs[i + 2] }, pointers = new[] { i, i + 1, i + 2 }, group = x };
                flat.Add(tr);
                for (int j = 0; j < 3; j++)
                {
                    List<Tr> lt;
                    if (!lab.TryGetValue(trs[i + j], out lt))
                        lab[trs[i + j]] = lt = new List<Tr>();

                    lt.Add(tr);
                }
            }
        }


        foreach (Tr t in flat)
        {
            if (!t.went)
            {
                Element element = new Element();
                element.MeshTest = this;
                elements.Add(element);
                var color32 = new Color32((byte)(Random.value * 255), (byte)(Random.value * 255), (byte)(Random.value * 255), 255);
                Go(t, color32, element);
            }

        }
        //mf.mesh.colors32 = crs;
    }
    public IEnumerator Start()
    {
        yield return null;
        yield return null;
        if (mf != null)
        {
            renderer.sharedMaterial.SetTexture("_Detail", res.scratches);
            ResetColor();
        }
    }
    private void ResetColor()
    {
        color32s = new Color32[mf.mesh.colors32.Length];
        for (int i = 0; i < color32s.Length; i++)
            color32s[i] = new Color32(255, 255, 255, 255);
        mf.mesh.colors32 = color32s;
    }
    bool changed;
    public void Reset()
    {
        if(!changed)
            return;
        changed = false;
        if (oldVertices != null)
        {
            vertices = mf.mesh.vertices = (Vector3[])oldVertices.Clone();
            foreach (var a in elements)
                a.detached = false;
        }
        ResetColor();
    }
    private void Go(Tr tr, Color32 color, Element element)
    {
        if (tr.went) return;
        tr.went = true;
        element.materialGroup = tr.group;
        element.pointers.AddRange(tr.pointers);
        element.list.AddRange(tr.ind);
        foreach (int a in tr.ind)
        {
            element.b.Encapsulate(vertices[a]);
            //crs[a] = color;
        }

        foreach(var i in tr.ind)
        {
            List<Tr> lt;
            if (lab.TryGetValue(i, out lt))
                foreach (Tr t in lt)
                    Go(t, color, element);
        }
    }
    
#if UNITY_EDITOR2
    public void Update()
    {
        if (_Game)
            return;
        //if (!collider)
        //    gameObject.AddComponent<MeshCollider>();
        
        var r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit h;
        if (Input.GetMouseButtonDown(1))
            Reset();
        if (Input.GetMouseButtonDown(0) && Physics.Raycast(r, out h))
        {
            //var point = mf.transform.InverseTransformPoint(h.point);
            //var direction= mf.transform.InverseTransformDirection(r.direction);
            
                Damage(h.point, r.direction);
            //mf.mesh.UploadMeshData(false);
            Debug.DrawRay(h.point, h.normal, Color.white, 10);
            //Hit(h.point);
        }
    }
#endif
    //int damage = 10;
    public void Damage(Vector3 nwPoint, Vector3 direction)
    {
        changed = true;
        //if (--damage < 0)
            //return;
        nwPoint = mf.transform.InverseTransformPoint(nwPoint);
        //direction = mf.transform.InverseTransformDirection(direction.normalized);
        if (color32s == null)
            color32s = mf.mesh.colors32;
        for (int j = 0; j < 3; j++)
        {
            var nextSearch = nwPoint + Random.insideUnitSphere;
            Vector3 r = Random.onUnitSphere;
            float oldM = MaxValue;
            var curPoint = nwPoint;
            for (int i = 0; i < vertices.Length; i++)
            {
                if ((vertices[i] - nextSearch).magnitude < oldM)
                {
                    nwPoint = vertices[i];
                    oldM = (vertices[i] - nextSearch).magnitude;
                }

                float d = (.7f - (vertices[i] - curPoint).magnitude);

                if (d > 0 && oldVertices[i] == vertices[i])
                    vertices[i] += Mathf.Sqrt(d)*r*.3f;
                if (d > 0 && i < color32s.Length)
                {
                    byte b = (byte)Mathf.Max(color32s[i].r - 1000 * d, 0);
                    color32s[i] = new Color32(b, b, b, b);
                }

            }
        }

        mf.mesh.colors32 = color32s;
        mf.mesh.vertices = vertices;
    }
    private Color32[] color32s;
    public void Hit(Vector3 point, Vector3 vel = default(Vector3), float cnt = 10, int max = 20)
    {
        //if(isDebug)return;
        changed = true;
        var po = mf.transform.InverseTransformPoint(point);
        //var color32 = new Color32((byte) (Random.value*255), (byte) (Random.value*255), (byte) (Random.value*255), 255);

        foreach (Element element in elements.OrderBy(a => (a.b.center - po).magnitude).Take(max))
        {
            if (!element.detached)
            {
               
                //foreach (var b in element.list)
                    //crs[b] = color32;

                if (element.list.Count > 10)
                {
                    
                    element.GenerateVertex();
                    Mesh m = new Mesh();
                    m.vertices = element.vertex.ToArray();
                    m.triangles = element.nwlist.ToArray();
                    m.uv = element.uvs.ToArray();
                    m.tangents = element.tangents.ToArray();
                    m.normals = element.normals.ToArray();
                    m.RecalculateBounds();
                    var magnitude = m.bounds.size.magnitude;
                    cnt -= magnitude;
                    //print(magnitude);
                    if (cnt < 0) break; 

                    var g = new GameObject();
                    g.transform.position = mf.transform.position;
                    g.transform.rotation = mf.transform.rotation;
                    var r = g.AddComponent<MeshRenderer>();
                        r.sharedMaterial = mf.renderer.sharedMaterials[element.materialGroup];
                    g.AddComponent<MeshFilter>().mesh = m;
                    g.transform.localScale = transform.lossyScale;
                    var c = g.AddComponent<BoxCollider>();
                    g.collider.material = new PhysicMaterial();
                    g.gameObject.layer = Layer.particles;
                    var g2 = new GameObject("oskolok");
                    g2.AddComponent<Oskolok2>();
                    g2.transform.position = c.bounds.center;
                    g.transform.parent = g2.transform;


                    g2.AddComponent<Rigidbody>();
                    //rig.useGravity = false;
                    //g2.AddComponent<ConstantForce>().force = Vector3.down * 15;
                    g2.rigidbody.velocity = (ZeroY(point - pos).normalized + Vector3.up + Random.insideUnitSphere) * 5 + vel;
                    g2.rigidbody.angularVelocity = Random.insideUnitSphere * 3;                                        
                }
                element.detached = true;
                foreach (var b in element.list)
                    vertices[b] = Vector3.zero;
            }
        }
        //mf.mesh.colors32 = crs;
        mf.mesh.vertices = vertices;

    }
}

[Serializable]
public class Element
{
    public MeshTest MeshTest;
    public List<int> list = new List<int>();
    public List<int> pointers = new List<int>();

    public List<int> nwlist = new List<int>();
    public List<Vector3> vertex = new List<Vector3>();
    public List<Vector2> uvs = new List<Vector2>();
    public List<Vector4> tangents = new List<Vector4>();
    public List<Vector3> normals = new List<Vector3>();

    public Bounds b = new Bounds();
    public bool detached;
    public int materialGroup;
    bool generated;
    public void GenerateVertex()
    {
        if (generated) return;
        generated = true;

        nwlist = new List<int>(new int[list.Count]);
        foreach (var a in list.Distinct())
        {
            vertex.Add(MeshTest.vertices[a]);
            uvs.Add(MeshTest.uvs[a]);
            normals.Add(MeshTest.normals[a]);
            if (a < MeshTest.tangents.Length)
            tangents.Add(MeshTest.tangents[a]);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == a)
                {
                    nwlist[i] = vertex.Count - 1;
                }
            }
        }
    }
}