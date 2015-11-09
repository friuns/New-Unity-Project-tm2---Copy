using System;
using System.Linq;
using System.Reflection;
using gui = UnityEngine.GUILayout;
using UnityEditor;
using UnityEngine;
using MonoBehaviour = Photon.MonoBehaviour;

public class Tools3 : EditorWindow
{
    

    [MenuItem("Window/Rtools", false, 0)]
    static void rtoolsclick()
    {
        GetWindow<Tools3>();
    }
    private string search = "";
    protected virtual void OnGUI()
    {
        OnGUIMono();
        search = gui.TextField(search).ToLower();
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        if (search.Length > 2)
        {
            var ago = Selection.activeGameObject;
            if (ago == null) return;
            var ms = ago.GetComponents<MonoBehaviour>();
            foreach (var m in ms)
            {
                if (m == null) return;
                Type type = m.GetType();
                foreach (var a in type.GetFields(flags))
                {
                    if (a.Name.ToLower().Contains(search))
                    {
                        object value = a.GetValue(m);
                        if (value is float)
                        {
                            float floatField = EditorGUILayout.FloatField(a.Name, (float) value);
                            if (floatField != (float) value)
                                a.SetValue(m, floatField);
                        }
                        else if (value is int)
                        {
                            int floatField = EditorGUILayout.IntField(a.Name, (int)value);
                            if (floatField != (int)value)
                                a.SetValue(m, floatField);
                        }
                        else if (value is bool)
                        {
                            bool floatField = EditorGUILayout.Toggle(a.Name, (bool)value);
                            if (floatField != (bool)value)
                                a.SetValue(m, floatField);
                        }
                        else
                            gui.Label(a.Name + ":" + value);
                    }
                }
                foreach (var a in type.GetProperties(flags))
                {

                    if (a.Name.ToLower().Contains(search))
                        gui.Label(a.Name + ":" + a.GetValue(m, null));
                }
            }
            Repaint();
        }

    }
    public Base lastObj;
    private GameObject activeGameObject;
    private bool lck;
    
    public Tools3()
    {
        SceneView.onSceneGUIDelegate = new SceneView.OnSceneFunc(OnSceneGui);
    }
    
    private void OnSceneGui(SceneView s)
    {
        if (lastObj != null)
        {
            lastObj.OnSceneGui(s);
        }
    }


    private void OnGUIMono()
    {
        //scroll = EditorGUILayout.BeginScrollView(scroll);
        lck = gui.Toggle(lck, "lock");
        foreach (var a in GameObject.FindGameObjectsWithTag("EditorGUI"))
            a.GetComponent<Base>().OnEditorGui();
        if (lck && activeGameObject == null || !lck)
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Base>() != null)
                activeGameObject = Selection.activeGameObject;
        if (activeGameObject != null)
        {
            if (activeGameObject.GetComponent<Base>() != null)
                lastObj = activeGameObject.GetComponent<Base>();

            if (activeGameObject.tag != "EditorGUI" && lastObj != null)
            {
                lastObj.OnEditorGui();
            }
        }
        //EditorGUILayout.EndScrollView();
    }
    public void OnSelectionChange()
    {
        Repaint();
    }
}