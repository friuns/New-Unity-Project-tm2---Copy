using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TerrainHelper : bs {

    public TerrainData td
    {
        get
        {
            if (!m_td) m_td = terrain.terrainData;
            return m_td;
        }
    }
    internal float[,] oldHeights;
    
    private float[, ,] oldalphamaps;
    private TreeInstance[] oldtrees;
    List<int[,]> olddetails = new List<int[,]>();
    public TerrainData m_td;
    private Terrain terrain
    {
        get
        {
            if(!m_terrain)
                m_terrain = (Terrain)FindObjectOfType(typeof(Terrain));
            return m_terrain;
        }
    }
    public Terrain m_terrain;
    public override void Awake()
    {
        _TerrainHelper = this;
        base.Awake();
    }
    public void Start()
    {
    }
    public void OnEnable()
    {
        if (wp8) return;
        //var b = gameObject.activeSelf;
        //Debug.LogWarning("OnEnable "+b);
        //if (b) 
        //{
            oldHeights = td.GetHeights(0, 0, td.heightmapWidth, td.heightmapHeight);
            oldalphamaps = td.GetAlphamaps(0, 0, td.alphamapWidth, td.alphamapHeight);
            for (int i = 0; i < td.detailPrototypes.Length; i++)
                olddetails.Add(td.GetDetailLayer(0, 0, td.detailWidth, td.detailHeight, i));
            oldtrees = td.treeInstances;
        //}
    }
    //public void OnApplicationQuit()
    //{
    //    OnDisable();
    //}
    public void OnDisable()
    {
        if (wp8) return;
        Debug.LogWarning( "Terrain Helper OnDisable");
        //var b = gameObject.activeSelf;
        //Debug.LogWarning("OnDisable " + b);
        //if (b)
        //{
            td.SetHeights(0, 0, oldHeights);
            td.SetAlphamaps(0, 0, oldalphamaps); 
            for (int i = 0; i < olddetails.Count; i++)
                td.SetDetailLayer(0, 0, i, olddetails[i]);
            td.treeInstances = oldtrees;
        //}
    }
}
