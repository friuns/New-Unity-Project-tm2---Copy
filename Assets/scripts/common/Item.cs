using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
//[ExecuteInEditMode]
public class Item : MonoBehaviour
{
    public float speed = 3.5f;
    public void Start()
    {
        if (speed == 0)
            speed = 3.5f;
    }
//#if UNITY_EDITOR
//    void OnApplicationQuit()
//    {
//        if (bs.resEditor.enableLevelEdit)
//            EditorApplication.delayCall += GetValue;
//    }

//    private void GetValue()
//    {
//        EditorApplication.delayCall -= GetValue;
//        if (this == null) return;
//        var instanceIdToObject = EditorUtility.InstanceIDToObject(this.GetInstanceID());
//        Item a = (Item)instanceIdToObject;
//        Undo.RegisterUndo(a, "rtools");
//        a.speed = speed;
//        EditorUtility.SetDirty(a);
//    }
//#endif
}

public static class Layer
{
    public static int fx = LayerMask.NameToLayer("TransparentFX");
    public static int car = LayerMask.NameToLayer("Car");
    [Obsolete]
    public static int carOnly = LayerMask.NameToLayer("CarOnly");
    public static int cull = LayerMask.NameToLayer("Cull");
    public static int model = LayerMask.NameToLayer("model");
    public static int border = LayerMask.NameToLayer("border");
    public static int nGui = LayerMask.NameToLayer("NGUI");
    public static int level = LayerMask.NameToLayer("Level");
    public static int def = LayerMask.NameToLayer("Default");
    public static int node = LayerMask.NameToLayer("node");
    public static int stadium = LayerMask.NameToLayer("Stadium");
    public static int water = LayerMask.NameToLayer("Water");
    public static int terrain = LayerMask.NameToLayer("terrain");
    public static int block = LayerMask.NameToLayer("block");
    public static int hitBox = LayerMask.NameToLayer("hitBox");
    public static int zombie = LayerMask.NameToLayer("zombie");
    public static int CheckPoint = LayerMask.NameToLayer("CheckPoint");
    public static int nodeLayer = 1 << node | 1 << block;
    public static int particles = LayerMask.NameToLayer("particles");
    public static int levelMask = 1 << level | 1 << def | 1 << stadium | 1 << border | 1 << cull | 1 << water | 1 << terrain | 1 << block;
    public static int allmask = levelMask | 1 << car | 1 << hitBox|1<<zombie;  
    //public static Layer
}

public class Tag
{
    public static int userTab = 5;
    public const string CheckPoint = "CheckPoint";
    public const string model = "model";
    public const string Untagged = "Untagged";
    public const string mainCamera = "MainCamera";
    public const string rearCamera = "RearCamera";
    public const string damage = "damage";
    public const string noSkid = "noSkid";
    public const string node = "node";
    public const string speed = "speed";
    public const string Speed = "Speed";
    public const string engineOff = "engineOff";
    public const string Road = "Road";
    public const string Dirt = "Dirt";
    public const string Start = "Start";
    public const string HitBox = "HitBox";
    public const string shield= "Shield";
    public const string redSpawn = "RedSpawn";
    public const string blueSpawn= "BlueSpawn";
    public const string Spawn = "Spawn";
    public const string zombieSpawn = "zombieSpawn";
    
}

