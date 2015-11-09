#if UNITY_EDITOR
using Curvy.Utils;
using UnityEditor;
//using Exception = System.AccessViolationException;
#endif
using gui = UnityEngine.GUILayout;
#if !UNITY_FLASH || UNITY_EDITOR
using System.Linq;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Prefs = UnityEngine.PlayerPrefs;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Reflection;

public partial class MapLoader 
{
    internal bool hideTerrain
    {
        get
        {
            return terrain == null || !(terrain.enabled && terrain.gameObject.activeSelf);
        }
        set
        {
            if (terrain == null) return;
            terrain.collider.enabled = terrain.enabled = !value;
        }
    }
    public void UpdateTerrain(CurvySplineSegment segment, bool refresh = false, bool refreshTrees = false,ModelObject sgo=null)
    {
        if (!refresh && _Loader.levelEditor != null && !_Loader.levelEditor.autoRefreshTerrain || lowQualityAndAndroid)
            return;
        //refreshTrees = highOrNotAndroid && refreshTrees;

        Random.seed = 0;
        
        if (hideTerrain || segment == null && sgo==null && !refresh)
            return;
        if (refresh)
            ((TerrainHelper)FindObjectOfType(typeof(TerrainHelper))).OnDisable();

        Debug.Log("update Terrain " + refresh + " " + refreshTrees);

        if (heights == null || refresh)
        {

            //if (flatTerrain)
            //{                
            //    heights = oldHeightsFlat
            //}
            //else
            if (flatTerrain)
                td.SetHeights(0, 0, oldHeightsFlat);
            heights = td.GetHeights(0, 0, td.heightmapHeight, td.heightmapWidth);
            alphas = td.GetAlphamaps(0, 0, td.alphamapHeight, td.alphamapWidth);
            if (miny == MaxValue)
                for (int i = 0; i < td.heightmapHeight; i++)
                    for (int j = 0; j < td.heightmapWidth; j++)
                        miny = Mathf.Min(heights[i, j], miny);
        }

        var points = new List<Vector5>();
        const int minDist = 50;
        foreach (CurvySpline2 a in FindObjectsOfType(typeof(CurvySpline2)))
            if (!a.shape)
            {
                for (int i = 0; i < a.Length; i += refresh ? 3 : 10)
                {
                    var sg = a.DistanceToSegment(i);
                    if (sg.flying) continue;
                    Vector2 bounds = sg.GetBounds();
                    //bounds.x *= sg.scale;
                    //bounds.y;
                    //bounds.x = Mathf.Abs(bounds.x);
                    //bounds.y = Mathf.Abs(bounds.y);
                    var v = a.InterpolateByDistance(i) + Vector3.down * bounds.y;
                    if (refresh)
                        points.Add(new Vector5(v.x, v.y, v.z, 0, bounds.x));
                    else if(segment!=null)
                    {
                        var w = CurvySpline2.DistancePointLine(ZeroY(v), ZeroY(segment.Position), ZeroY(segment.NextControlPoint.Position));
                        if (w < minDist)
                            points.Add(new Vector5(v.x, v.y, v.z, w, bounds.x));
                    }
                }
            }
        if (refresh)
            foreach (ModelObject a in FindObjectsOfType(typeof(ModelObject)))
                UpdateModelObject(a, points);
        else if (sgo)
            UpdateModelObject(sgo, points);
        print("Update Terrain " + points.Count);
        var changed = new List<Vector2>();
        if (refresh || used == null)
            used = new bool[td.heightmapWidth, td.heightmapHeight];
        bool[,] used2 = new bool[td.heightmapWidth, td.heightmapHeight];
        print(td.heightmapScale.y);
        foreach (Vector5 v5 in points.OrderBy(a => a.dist > minDist / 2f).ThenByDescending(a => a.v.y))
        {
            Vector3 p = v5.v;
            Debug.DrawRay(p, Vector3.up, Color.red, 1);
            RaycastHit h;
            if (terrain.collider.Raycast(new Ray(p + Vector3.up * 100, Vector3.down), out h, 1000))
            {
                var y = (int)(h.textureCoord.x * td.heightmapWidth);
                var x = (int)(h.textureCoord.y * td.heightmapHeight);
                int sz = (int)((v5.width * 2f + 20) / td.heightmapScale.x);

                var th = (TerrainHelper)FindObjectOfType(typeof(TerrainHelper));

                var oldHeights = flatTerrain ? oldHeightsFlat : th.oldHeights;

                for (int j = -sz; j < sz; j++)
                    for (int k = -sz; k < sz; k++)
                    {
                        float f = 1f - new Vector2(j, k).magnitude / sz;
                        f = Mathf.Clamp01(f * 1.3f);
                        int x2 = x + j;
                        int y2 = y + k;

                        if (x2 > 0 && x2 < td.heightmapWidth && y2 > 0 && y2 < td.heightmapHeight)
                        {
                            if (f >= 1)
                                used[x2, y2] = true;
                            if (!used2[x2, y2])
                            {
                                changed.Add(new Vector2((float)x2 / td.heightmapWidth, (float)y2 / td.heightmapHeight));
                                used2[x2, y2] = true;
                            }
                            var h2 = (p.y - terrain.transform.position.y) / td.heightmapScale.y;
                            var f2 = Mathf.Lerp(oldHeights[x2, y2], h2, f);
                            if (f2 > heights[x2, y2] && (!used[x2, y2] || f >= 1) && v5.dist < minDist / 2f || f >= 1 && f2 <= heights[x2, y2])
                                heights[x2, y2] = f2;
                        }
                        //else if (!warkingShown)
                        //{
                        //    warkingShown = true;
                        //    if (_Loader.levelEditor != null)
                        //        ShowPopup("Warning you are out of terrain bounds, visual bugs will apear");
                        //}
                    }
            }
        }
        //if (refresh)
        //    foreach (ModelObject a in FindObjectsOfType(typeof(ModelObject)))
        //    {
        //        var bounds = a.renderer.bounds;
        //        var e = bounds.center - terrain.GetPosition();
        //        var p = td.heightmapWidth / td.size.x;
        //        var py = 1f / td.size.y;
        //        e.x *= p;
        //        e.z *= p;
        //        e.y *= py;
        //        bounds.center = e;
        //        e = bounds.extents;
        //        e.x *= p;
        //        e.z *= p;
        //        e.y *= py;
        //        bounds.extents = e;
        //        for (int i = Mathf.Max(0, (int)bounds.min.x); i < Mathf.Min(bounds.max.x, td.heightmapWidth); i++)
        //            for (int j = Mathf.Max(0, (int)bounds.min.z); j < Mathf.Min(td.heightmapWidth, bounds.max.z); j++)
        //                heights[j, i] = bounds.min.y;
        //    }
        td.SetHeights(0, 0, heights);

        foreach (Vector2 a in changed)
        {

            var n = td.GetInterpolatedNormal(a.y, a.x);
            var x = (int)(a.x * td.alphamapWidth);
            var y = (int)(a.y * td.alphamapHeight);
            var f = 1 - (n.y * n.y * n.y);
            if (f > .96f)
            {
                if (Mathf.Abs(n.x) > Mathf.Abs(n.z))
                    SetAlpha(x, y, f, 4);
                else
                    SetAlpha(x, y, f, 5);
            }
            else
                SetAlpha(x, y, f, 1);
        }


        if (refreshTrees && !flatTerrain)
        {
            List<TreeInstance> trees = new List<TreeInstance>(oldTreeInstances);

            td.SetAlphamaps(0, 0, alphas);

            terrain.collider.enabled = false;
            StartCoroutine(AddMethod(delegate { terrain.collider.enabled = true; }));

            RefreshTrees(used, trees);
            Debug.LogWarning("Set Trees " + trees.Count);
            //terrain.Flush();
            //DestroyImmediate(terrain.collider);
            //terrain.gameObject.AddComponent<TerrainCollider>().terrainData = td;
            td.treeInstances = trees.ToArray();
        }


        //terrain.enabled = true;
        if (!refresh)
            try
            {
                td.GetType().GetMethod("SetBasemapDirty", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(td, new object[] { false });
            }
            catch (Exception e) { Debug.Log(e.Message); }
    }
    private static void UpdateModelObject(ModelObject a, List<Vector5> points)
    {
        if (!a.flying)
        {
            var v = a.renderer.bounds.center;
            v.y = a.renderer.bounds.min.y;
            if (a.renderer == null)
                Destroy(a.gameObject);
            else
            {
                var size = a.renderer.bounds.size;

                points.Add(new Vector5(v.x, v.y - .1f, v.z, 0, Mathf.Min(size.magnitude, 3)));
                var width = 20;
                if (size.magnitude < width)
                    points.Add(new Vector5(v.x, v.y - .1f, v.z, 0, size.magnitude));
                else
                {
                    for (float i = width/2f; i < size.x + width/2f; i += width)
                        for (float j = width/2f; j < size.z + width/2f; j += width)
                            points.Add(new Vector5(v.x + (i - size.x/2f), v.y - .1f, v.z + (j - size.z/2f), 0, width + 10));
                }
            }
        }
    }
    private void SetAlpha(int x, int y, float f, int layer)
    {
        float total = 0;
        for (int i = 0; i < td.alphamapLayers; i++)
            if (i != layer)
                total += alphas[x, y, i];
        if (total == 0)
            f = 1;
        else
            for (int i = 0; i < td.alphamapLayers; i++)
                if (i != layer)
                    alphas[x, y, i] *= (1 - f) / total;

        alphas[x, y, layer] = f;

    }
    private void RefreshTrees(bool[,] used, List<TreeInstance> trees)
    {
        for (int i = 0; i < td.alphamapWidth; i++)
        {
            for (int j = 0; j < td.alphamapHeight; j++)
            {
                var y = terrain.transform.position.y +
                        td.GetInterpolatedHeight((float)j / td.alphamapWidth, (float)i / td.alphamapHeight) -
                        Random.Range(0, 2);
                if (y < water.transform.position.y)
                {
                    for (int k = 0; k < td.alphamapLayers; k++)
                        alphas[i, j, k] = 0;
                    alphas[i, j, 0] = 1;
                    used[i, j] = true;
                }
            }
        }
        Vector3[][] treeGroups = new Vector3[td.treePrototypes.Length][];
        for (int i = 0; i < treeGroups.Length; i++)
        {
            treeGroups[i] = new Vector3[4];
            for (int j = 0; j < 4; j++)
                treeGroups[i][j] = new Vector3(Random.value, Random.value);
        }
        SetGrass(used);

        int cnt = (android ? 1000 : 3000);
        for (int i = 0; i < cnt; i++)
        {
            Vector3 p = new Vector3(Random.value, Random.value, Random.value);
            var x = (int)(p.x * td.heightmapWidth);
            var z = (int)(p.z * td.heightmapHeight);
            if (!used[z, x] && td.GetInterpolatedNormal(p.x, p.z).y > .96f)
            {
                p.y = td.GetInterpolatedHeight(p.x, p.z) / td.heightmapScale.y;
                float md = MaxValue;
                int tree = 0;
                if (Random.value < .3f)
                    tree = Random.Range(0, treeGroups.Length);
                else
                    for (int j = 0; j < treeGroups.Length; j++)
                    {
                        foreach (var a in treeGroups[j])
                        {
                            var distance = Vector3.Distance(a, p);
                            if (distance < md)
                            {
                                tree = j;
                                md = distance;
                            }
                        }
                    }
                trees.Add(CreateTree(p, tree));
            }
        }
    }
    private void SetGrass(bool[,] used)
    {
        var ln = td.detailPrototypes.Length;
        float[] detailsFactor = new float[ln];
        var details = new int[td.detailWidth, td.detailHeight];
        for (int id = 0; id < ln; id++)
        {
            for (int i = 0; i < td.detailWidth; i++)
            {
                for (int j = 0; j < td.detailHeight; j++)
                {
                    if (!used[i, j])
                    {
                        float total = 0;
                        for (int index0 = 0; index0 < ln; index0++)
                            total += (detailsFactor[index0] = (Random.value * (3 + ln - index0)));
                        for (int index0 = 0; index0 < ln; index0++)
                            detailsFactor[index0] /= total;
                        var h = Mathf.Clamp(alphas[i, j, 3] - .5f, 0, .3f);
                        details[i, j] = (int)(h * (android ? 15 : 30) * detailsFactor[id]);
                    }
                }
            }
            td.SetDetailLayer(0, 0, id, details);
        }
    }


    public Transform water;
    private TreeInstance CreateTree(Vector3 Position, int PrototypeIndex)
    {
        var value = Random.Range(.7f, 1);
        TreeInstance treeInstance = new TreeInstance()
        {
            position =
                Position,
            heightScale = Random.Range(.7f, 1),
            widthScale = 1,
            color = new Color(value, value, value, 1),
            lightmapColor = Color.white,
            prototypeIndex = PrototypeIndex
        };
        return treeInstance;
    }
    public Terrain terrain;
}