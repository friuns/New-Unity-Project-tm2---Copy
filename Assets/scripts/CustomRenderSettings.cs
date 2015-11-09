using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CustomRenderSettings
{
    public string name;
    public Color ambientLight;
    public float flareStrength;
    public bool fog;
    public Color fogColor;
    public float fogDensity;
    public float fogEndDistance;
    public FogMode fogMode;
    public float fogStartDistance;
    public float haloStrength;
    public Material skybox;
    public List<MyProperty> properties = new List<MyProperty>();
    public void Active()
    {
        var r = this;
        RenderSettings.fog = r.fog;
        RenderSettings.ambientLight = r.ambientLight;
        RenderSettings.flareStrength = r.flareStrength;
        RenderSettings.fogColor = r.fogColor;
        RenderSettings.fogDensity = r.fogDensity;
        RenderSettings.fogEndDistance = r.fogEndDistance;
        RenderSettings.fogMode = r.fogMode;
        RenderSettings.fogStartDistance = r.fogStartDistance;
        RenderSettings.haloStrength = r.haloStrength;
        RenderSettings.skybox = r.skybox;

        //var monoBehaviours = Camera.main.GetComponents<MonoBehaviour>();
        //foreach (var b in monoBehaviours)
        //{
        //    foreach (MyProperty a in properties)
        //        if (a.monoName == b.GetType().Name)
        //        b.GetType().GetField(a.fieldName).SetValue(b, a.getValue());
        //}
    }
}