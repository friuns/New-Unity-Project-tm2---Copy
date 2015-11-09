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

public class LevelLoader :MapLoader
{
    public new IEnumerator Start()
    {
        base.Start();
        //print(_Loader.mapName);
        //print(_Loader.curScene == null);                
        yield return StartCoroutine(LoadMap(_Loader.curScene.url));
        if (userMapSucces)
        {
            //yield return new WaitForSeconds(.1f);
            yield return null;
            yield return StartCoroutine(ActiveEditor(false));
            var splines = FindObjectsOfType(typeof(SplinePathMeshBuilder));
            foreach (SplinePathMeshBuilder a in splines)
                a.enabled= false;
            foreach (CurvySpline2 a in FindObjectsOfType(typeof(CurvySpline2)))
                a.AutoRefresh = false;
            
            var start = GameObject.FindGameObjectWithTag(Tag.Start);
            var checkpoint = GameObject.FindGameObjectWithTag(Tag.CheckPoint);
            print("Splines " + splines.Length);
            print(checkpoint);
            print(start);
            //if (start != null && checkpoint != null || _Loader.dm)
            //{
                Camera.main.gameObject.SetActive(false);
                LoadLevelAdditive(Levels.game);
                yield break;
            //}
                //Debug.LogWarning("Start or checkpoint not found");
        }

        LoadLevel(Levels.menu);
    }


   
}