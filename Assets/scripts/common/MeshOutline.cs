using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class MeshOutline:bs
{
    

    internal MeshFilter mf;

    internal List<int> outline;

    internal Vector3[] vert;
    private bool inited;
    public MeshOutline Init()
    {
        if (inited )
            return this;
        inited = true;
        mf = GetComponentInChildren<MeshFilter>();
        if (mf == null)
            return this;

        var m = mf.sharedMesh;
        vert = mf.sharedMesh.vertices;

        if (!res.outlines.TryGetValue(name, out outline))
        {
            res.outlines[name] = outline = new List<int>();
            //res.outlineDict.Add(new OutlineDict() { Name = name, outlineValues = outline });
            var cnt = new Dictionary<Vector3, MyStruct>();
            for (int i = 0; i < m.triangles.Length; i++)
            {
                var vector3 = m.vertices[m.triangles[i]];

                MyStruct myStruct;
                if (!cnt.TryGetValue(vector3, out myStruct))
                    myStruct = cnt[vector3] = new MyStruct();
                myStruct.id.Add(m.triangles[i]);
                myStruct.cnt++;
            }

            foreach (var a in cnt)
                if (a.Value.cnt < 4)
                    outline.AddRange(a.Value.id);
        }

        meshFilter = new MeshFilter2() { m = mf, verts = mf.sharedMesh.vertices };
        foreach (int b in this.outline)
        {
            if (meshFilter.verts.Length <= b)
                continue;
            meshFilter.myVerts.Add(new MyVert() { m = meshFilter, i = b });
        }
        return this;
    }
    public MeshFilter2 meshFilter;
}


public class MeshFilter2
{
    public MeshFilter m;
    public Vector3[]    verts;
    public List<MyVert> myVerts = new List<MyVert>();
}
public class MyVert
{
    public MeshFilter2 m;
    public int i;
    public Vector3[] verts { get { return m.verts; } set { m.verts = value; } }
    public AccessTime<Vector3> tmp = new AccessTime<Vector3>();
    
    public Vector3 v
    {
        get
        {
            //if (verts == null) verts = m.sharedMesh.vertices;
            return tmp.needUpdate ? tmp.value = m.m.transform.TransformPoint(verts[i]) : tmp.value;
        }
        set
        {
            verts[i] = m.m.transform.InverseTransformPoint(value);
        }
    }
}
public class MyStruct
{
    public HashSet<int> id = new HashSet<int>();
    public int cnt;

}