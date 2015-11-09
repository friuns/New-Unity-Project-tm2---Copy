

//using System.Collections.Generic;
//using System.Linq;
//using ExitGames.Client.Photon;
//using UnityEngine;

//public class Test : MonoBehaviour
//{
    

//    public List<MeshFilter2> mfs2 = new List<MeshFilter2>();

//    //Dictionary<MeshFilter , Vector3[]> ver = new Dictionary<MeshFilter, Vector3[]>();
//    Dictionary<Vector3, List<MyVert>> space = new Dictionary<Vector3, List<MyVert>>();
   
//    public void Start()
//    {
//        int cellSize = 3;
//        foreach (MeshCollider mc in FindObjectsOfType<MeshCollider>())
//            mc.gameObject.AddComponent<MeshOutline>();
//        foreach (MeshOutline mo in FindObjectsOfType<MeshOutline>().Where(a => a.inited))
//        {
//            var meshFilter = new MeshFilter2() { m = mo.mf, verts = mo.mf.sharedMesh.vertices };
//            mfs2.Add(meshFilter);
//            foreach (int b in mo.outline)
//            {
//                if (meshFilter.verts.Length <= b)
//                    continue;
//                var v = new MyVert() { m = meshFilter, i = b }.v;
                
//                Vector3 vector3 = new Vector3((int)(v.x / cellSize), (int)(v.y / cellSize), (int)(v.z / cellSize));
//                List<MyVert> cell;
//                if (!space.TryGetValue(vector3, out cell))
//                    cell = space[vector3] = new List<MyVert>(); 
//                cell.Add(new MyVert() { m = meshFilter, i = b });
//            }
//        }
//        foreach (List<MyVert> l in space.Values)
//            if (l != null)
//            {
//                if (l.Count > 1)
//                {
//                    foreach (MyVert b in l)
//                    {
//                        MyVert v = l.Where(a => a.m.m != b.m.m && (b.v - a.v).magnitude < 1).OrderBy(a => (b.v - a.v).magnitude).FirstOrDefault();
//                        if (v != null)
//                        {
//                            //print("merged " + b.v + " " + v.v);
//                            Debug.DrawLine(b.v, v.v,Color.red,10);
//                            b.v = v.v;
//                            //ver[b.m] = b.verts;
//                        }
//                    }
//                }

//            }


//        foreach (var a in mfs2)
//        {
//            //print("asd");
//            var mesh = a.m.sharedMesh;
//            a.m.sharedMesh = new Mesh() { vertices = a.verts, triangles = mesh.triangles, normals = mesh.normals };
//            //mesh.vertices = a.Value;
//            //mesh.UploadMeshData(true);
//            //mesh.RecalculateBounds();
//        }

//    }
    

//}