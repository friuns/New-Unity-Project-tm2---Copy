using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using Curvy.Utils;
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
#if UNITY_EDITOR
//using Exception = System.AccessViolationException;
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


public class GizmoArrows:bs
{
    public Vector3 direction;
    public void Start()
    {
        renderer.material.color = direction.x > 0 ? Color.red : direction.y > 0 ? Color.green : Color.blue;
    }
    //public float active;
    public void OnMouseDrag()
    {
        //active =Time.time;
        //var gizmoMove = Input.GetAxis("Mouse Y") * direction * Time.deltaTime * 30;
        //_Loader.levelEditor.gizmoMove += gizmoMove;
        //transform.root.Translate(gizmoMove);
    }
}