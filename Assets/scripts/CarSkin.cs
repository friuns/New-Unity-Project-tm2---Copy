using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
[Serializable]
public class CarSkin
{
    public string prefabName;
    public AudioClip[] horn = new AudioClip[0];
    private static AnimationCurve speedCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(4, 1.3f));
    //private static AnimationCurve massCurve = new AnimationCurve(new Keyframe(0, 5), new Keyframe(4, 20));
    private static AnimationCurve antiFlyCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(4, 1));
    private static AnimationCurve distanceToGround = new AnimationCurve(new Keyframe(0, 2), new Keyframe(4, 4));
    private static AnimationCurve rotCurve = new AnimationCurve(new Keyframe(0, 1.3f), new Keyframe(4, 2.5f));
    private static AnimationCurve rotCurveMouse = new AnimationCurve(new Keyframe(0, .5f), new Keyframe(1, 4f));
    private static AnimationCurve driftCurve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(4, .5f));
    private static AnimationCurve driftBrakeCurve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(4, 0));
    public float getDistanceToGround()
    {
        return
            //bs._Game.isCustomLevel ? 1 : 
            distanceToGround.Evaluate(mass);
    }
    public float getAntiFly()
    {
        return 
            //bs._Game.isCustomLevel ? 1 : 
            antiFlyCurve.Evaluate(mass);
    }
    public float getMass()
    {
        return 0;
        //return massCurve.Evaluate(mass);
        //return bs._Game.isCustomLevel ? massCurve.Evaluate(mass) : 0;
    }

    public float getSpeed()
    {
        return speedCurve.Evaluate(Speed);
    }

    public float getBrakeDrift()
    {
        var brakeDrift = driftBrakeCurve.Evaluate(Drift);
        //Debug.Log(brakeDrift);
        return brakeDrift;
    }

    public float getDrift()
    {
        return driftCurve.Evaluate(Drift);
    }
    public float getRotationMouse()
    {
        /*(easy ? 3 : normal ? 2 : 1)*/
        //Debug.Log(Rotation + ";" + rotCurve.Evaluate(Rotation));

        return rotCurveMouse.Evaluate(Rotation);
    }
    public float getRotation()
    {
        /*(easy ? 3 : normal ? 2 : 1)*/
        //Debug.Log(Rotation + ";" + rotCurve.Evaluate(Rotation));

        return rotCurve.Evaluate(Rotation);
    }
    public int TopSpeed = 4;
    public int Speed = 2;
    public int Rotation = 2;
    public int Drift = 0;
    public int mass = 0;

    public bool flyRotation;
    //public int arrayId;
    public int friendsNeeded;
    public int medalsNeeded;
    public bool unlocked { get { return repUnlocked && medalsUnlocked; } }
    public bool medalsUnlocked { get { return medalsNeeded <= bs._Loader.medals; } }
    public int repNeeded;
    public bool repUnlocked { get { return repNeeded == 0 || bs._Loader.disableRep || bs.PlayerPrefsGetBool(prefix + "repUnlocked"); } set { bs.PlayerPrefsSetBool(prefix + "repUnlocked", value); } }
    public bool paintUnlocked { get { return bs.PlayerPrefsGetBool(prefix + "paintUnlocked"); } set { bs.PlayerPrefsSetBool(prefix + "paintUnlocked", value); } }
    //public bool special;
    public bool allMedals;
    public AudioClip engineSound;
    //public Texture[] flags;
    //public Renderer[] rs;
    public GameObject model
    {
        get
        {
            var loadRes = bs.LoadRes("Cars/" + prefabName);
            //Debug.Log(loadRes);
            if(loadRes==null)
                loadRes = bs.LoadRes("Cars/carF1");
            return (GameObject)loadRes;
        }
    }
    

    public bool hidden;
    public bool canPickColor;
    public Color color
    {
        get
        {
            return new Color(bs.PlayerPrefsGetFloat(prefix + "r"), bs.PlayerPrefsGetFloat(prefix + "g"), bs.PlayerPrefsGetFloat(prefix + "b"));
        }
        set
        {
            bs.PlayerPrefsSetFloat(prefix + "r", value.r);
            bs.PlayerPrefsSetFloat(prefix + "g", value.g);
            bs.PlayerPrefsSetFloat(prefix + "b", value.b);
            bs.PlayerPrefsSetBool(prefix + "color", true);
        }
    }
    public bool haveColor { get { return bs.PlayerPrefsGetBool(prefix + "color"); } }

    private string prefix
    {
        get { return bs._Loader.playerName + prefabName; }
    }
    public void SetColor(Renderer[] renderers, Color? c = null)
    {
        if (!canPickColor) return;
        //bool ed = bs.isDebug && !bs._Game;
        foreach (var a in renderers.SelectMany(a => /*ed ? a.sharedMaterials : */a.materials))
            if (a.shader.name == "Car/CarPain2 Bump" || a.shader.name == "Car/CarPain2")
            {
                a.SetFloat("_CandyScale", .7f);
                var color1 = c.HasValue ? c.Value : color;
                a.SetColor("_AmbientColor2", color1);
                //a.color = color1;
            }
    }
}