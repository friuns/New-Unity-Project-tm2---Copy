using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleApplication1;
using UnityEditor;
using UnityEngine;
using System.Collections;
   [ExecuteInEditMode]
public class GbxTest : MonoBehaviour
{

    //// Use this for initialization
    //void OnValidate()
    //{
    //    Start();
    //}
    public StatDictonary stats = new StatDictonary();
    //public SerializableDictionary<string, float> test = new SerializableDictionary<string, float>();

    public ModelLibrary ml;
 
    void Awake()
    {
        Debug.Log("test");
        var gbxmap = new GbxMap();
        gbxmap.Load();
        foreach (var block in gbxmap.mapBlocks)
        {
            if (!stats.ContainsKey(block.BlockName))
                stats.Add(block.BlockName, null);
        }
        foreach (var b in stats.ToDictionary())
        {
            if (b.Value == null)
            {
                var o = ml.models.FirstOrDefault(a => a.name.ToLower().Contains(b.Key.ToLower().Substring(7)));
                if (o != null)
                {
                    var replace = o.path.Substring(0, o.path.LastIndexOf('.')).Replace('\\', '/');
                    stats[b.Key] = (GameObject)Resources.Load(replace);
                    Debug.Log("Found " + o.name);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    

}

[System.Serializable]
public class StatDictonary : SerializableDictionary<string, GameObject> { }
