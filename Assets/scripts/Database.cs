
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
//using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public enum CountryCodes { fi, us, ru, ua, cz, de, es, nl, tr, no }
public enum EventGroup { Other,SiteOld,
    Maps,
    Fps,
    playedTime,
    LoadedIn,
    Site,
    GameType,
    LevelEditor,
    Debug
}

public class Database : MonoBehaviour
{
    //public List<Replay> Replays = new List<Replay>();
    
}

[Flags]
public enum WeaponEnum
{
    machinegun =1, freezer = 2, fire = 4
}

public enum RoadType { Road, Dirt, Speed }
[Serializable]
public class Thumbnail
{
    public string url;
    public string name;
    public WWW w;
    public Material m;
    public Material material
    {
        get
        {
            if (m == null)
                bs._Loader.StartCoroutine(startDownload());
            return m;
        }
    }
    public Vector2 tile { get { return material.mainTextureScale; } set { material.mainTextureScale = value; } }
    static Dictionary<string, Material> list = new Dictionary<string, Material>();

    IEnumerator startDownload()
    {
        Material tmp;
        if (list.TryGetValue(url, out tmp))
            m = tmp;
        else
        {
            m = new Material(bs.res.roadMaterial);
            m.name = url;
            list.Add(url, m);
            if (string.IsNullOrEmpty(url)) yield break;
            w = new WWW(bs.mainSite + url);
            yield return w;
            if (w.error != null)
                Debug.Log(w.error + url);
            else
            {
                Texture2D t = w.textureNonReadable;
                m.mainTexture = t;
                //m.mainTextureScale = new Vector2(1, .5f);
                if (t.format == TextureFormat.ARGB32)
                    m.shader = bs.res.transparmentCutout;
                //Debug.LogWarning("Loaded ");
            }    
        }
    }
}

[Serializable]
public class KeyDown
{
    public KeyCode keyCode;
    public float time;
    public bool down;
}

//[Serializable]
public class MyProperty
{
    [HideInInspector]
    public AnimationCurve animationCurveValue;
    [HideInInspector]
    public bool boolValue;
    [HideInInspector]
    public Color colorValue;
    [HideInInspector]
    public float floatValue;
    [HideInInspector]
    public int intValue;
    [HideInInspector]
    public string stringValue;
    [HideInInspector]
    public Vector2 vector2Value;
    [HideInInspector]
    public Vector3 vector3Value;

    public enum ET
    {
        None, boola, color, floata, inta,
        AnimCurve
    }
    public ET et;
    public string monoName;
    public string fieldName;

    public MyProperty SetValue(string s, object v, string index)
    {
        fieldName = s;
        this.monoName = index;
        if (v is bool)
        {
            boolValue = (bool) v;
            et = ET.boola;
        }
        else if (v is Color)
        {
            colorValue = (Color) v;
            et = ET.color;
        }
        else if (v is int)
        {
            intValue = (int) v;
            et = ET.inta;
        }
        else if (v is float)
        {
            floatValue = (float) v;
            et = ET.floata;
        }
        else if (v is AnimationCurve)
        {
            animationCurveValue = (AnimationCurve)v;
            et = ET.AnimCurve;
        }
        else
            return null;

        return this;
    }
    public object getValue()
    {
        if (et == ET.AnimCurve)
            return animationCurveValue;
        else if (et == ET.boola)
            return boolValue;
        else if (et == ET.color)
            return colorValue;
        else if (et == ET.floata)
            return floatValue;
        else if (et == ET.inta)
            return intValue;
        else
        {
            Debug.Log("Value not found "+et);
            return null;
        }
    }
}
//public class NP
//{
//    public Vector3 a;
//    public Vector3 b;
//    public Vector3 point;

//    public float tf;
//    public CurvySpline2 spline;
//    public CurvySplineSegment segment;
//    private Camera camera;
//    public NP()
//    {

//    }
//    private Plane pl;
//    private CurvySplineSegment n1;
//    private CurvySplineSegment n2;
//    public NP(Camera camera, CurvySplineSegment n1, CurvySplineSegment n2)
//    {
//        this.n1 = n1;
//        this.n2 = n2;
//        a = camera.WorldToScreenPoint(n1.Position);
//        b = camera.WorldToScreenPoint(n2.Position);
        
//        segment = n1;
//        spline = n1.Spline2;

//        this.camera = camera;
//    }
//    public Vector3 dist;
//    public Vector3 pivot;
//    public float Dist(Vector3 mpos)
//    {
//        //pl = new Plane(Vector3.Cross(camera.transform.up, n1.Position - n2.Position), n1.Position);
//        //float f;
//        //pl.Raycast(camera.ScreenToWorldPoint(mpos + Vector3.forward), out f);

        
//        Vector3 project = CurvySpline2.ProjectPointLine2(mpos, a, b);
//        var p1 = pivot = camera.ScreenToWorldPoint(mpos + Vector3.forward * project.z);
//        var p2 = camera.ScreenToWorldPoint(project);
//        point = spline.Interpolate(spline.GetNearestPointTF(p2));
//        Debug.DrawLine(p1, p2, Color.blue);
//        dist = p1 - p2;
//        return dist.magnitude;
//        //Bounds screen = new Bounds();
//        //screen.SetMinMax(Vector3.zero, new Vector3(Screen.width, Screen.height, 10000));
//        //if (!screen.Contains(dist)) return float.MaxValue;
//        //if (dist.z < 0) return float.MaxValue;
//    }
//}
[Serializable]
public class PosVel
{
    public float time;
    public int frameCount;
    public float meters;
    public float velSmooth;
    public bool movingBack;
    public float nitro;
    public Vector3 pos;
    public Quaternion rot;
    public bool left;
    public bool up; 
    public float groundTime;
    public Vector3 vel;
    public Vector3 angVel;
    public bool engineOff;
    //internal float timeElapsed;
    internal Vector3 camPos;
    internal Quaternion camRot; 
    public bool right;
    public float mouserot;
    public float skid;
    public List<Transform> checkPointsPass;
    public int lap;
    //public int score;
}

public enum SGameType { VsFriends, VsPlayers, SplitScreen, Replay, Clan, Multiplayer, Cops }

public enum Quality2 { Lowest, Low, Medium, High,Ultra }
[Serializable]
public class Scene
{
    private bool? m_loaded;
    internal bool loaded
    {
        get
        {
            return m_loaded ?? (m_loaded = Loader.maps.ContainsKey(name.ToLower()) || bs.CanStreamedLevelBeLoaded(name)).Value;
        }
    }
    public string name;
    public string title;
    public int nitro;
    public int j;
    public int mapId;
    public bool played { get { return bs.PlayerPrefsGetBool("played" + name); } set { bs.PlayerPrefsSetBool("played" + name, value); } }    
    //internal string levelName;
    public Texture2D texture
    {
        get
        {
            return (Texture2D)bs.LoadRes("MapTextures/" + name);
            //if (texture2D == null) print("Res not found " + name);
        }
    }

    public string mapBy;
    internal string url;
    public bool userMap;
    public float rating;
    public Vector3 FinnishPos;
    public MapSets mapSets= new MapSets();
    internal bool disableJump=false;
}
public enum Difficulty { Easy, Normal, Hard }
public static class Levels
{
    public const string menu = "!1";
    public const string levelEditor = "level";
    public const string levelLoader = "levelLoader";
    public const string carSelect = "!2carselect";
    public const string game = "!2game";
}



public static class RD
{
    public const int posVel = 1;
    public const int keyCode = 2;
    public const int avatarId = 3;
    public const int carSkin = 4;
    public const int posVelMouse = 5;
    public const int FinnishTime = 6;
    public const int country = 7;
    public const int posVelSkid = 8;
    public const int version = 9;
    public const int color = 10;
    public const int avatarUrl = 11;
    public const int score = 12;
    public const int playerName=13;
    public const int clan=14;
    public const int rank= 15;
}

[Serializable]
public class Map
{
    public string name;
    public string url;
    public int fileDate;
}

public class AlphanumComparatorFast : IComparer<string>
{
    public int Compare(string x, string y)
    {
        string s1 = x;
        if (s1 == null)
        {
            return 0;
        }
        string s2 = y;
        if (s2 == null)
        {
            return 0;
        }

        int len1 = s1.Length;
        int len2 = s2.Length;
        int marker1 = 0;
        int marker2 = 0;

        // Walk through two the strings with two markers.
        while (marker1 < len1 && marker2 < len2)
        {
            char ch1 = s1[marker1];
            char ch2 = s2[marker2];

            // Some buffers we can build up characters in for each chunk.
            char[] space1 = new char[len1];
            int loc1 = 0;
            char[] space2 = new char[len2];
            int loc2 = 0;

            // Walk through all following characters that are digits or
            // characters in BOTH strings starting at the appropriate marker.
            // Collect char arrays.
            do
            {
                space1[loc1++] = ch1;
                marker1++;

                if (marker1 < len1)
                {
                    ch1 = s1[marker1];
                }
                else
                {
                    break;
                }
            } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

            do
            {
                space2[loc2++] = ch2;
                marker2++;

                if (marker2 < len2)
                {
                    ch2 = s2[marker2];
                }
                else
                {
                    break;
                }
            } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

            // If we have collected numbers, compare them numerically.
            // Otherwise, if we have strings, compare them alphabetically.
            string str1 = new string(space1);
            string str2 = new string(space2);

            int result;

            if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
            {
                int thisNumericChunk = int.Parse(str1);
                int thatNumericChunk = int.Parse(str2);
                result = thisNumericChunk.CompareTo(thatNumericChunk);
            }
            else
            {
                result = str1.CompareTo(str2);
            }

            if (result != 0)
            {
                return result;
            }
        }
        return len1 - len2;
    }
}
#if UNITY_FLASH
public class StringBuilder
{
    private List<string> ss;
    public StringBuilder()
    {
        ss= new List<string>();
    }
    public StringBuilder(string s)
    {
        ss = new List<string>();
        ss.Add(s);
    }
    public StringBuilder(int builderCapacity)
    {
        ss = new List<string>(builderCapacity);
    }
    public StringBuilder AppendLine(string s)
    {
        ss.Add(s+"\r\n");
        return this;
    }
    [Obsolete]
    public StringBuilder Append(char p0)
    {
        ss.Add(p0.ToString());
        return this;
    }

    public StringBuilder Append(object p0)
    {
        ss.Add(p0.ToString());
        return this;
    }
    public StringBuilder AppendFormat(string s, params object[] key)
    {
        ss.Add(string.Format(s, key));
        return this;
    }
    public override string ToString()
    {
        return string.Join("", ss.ToArray());
    }
}
#endif


public class BinaryWriter : MemoryStream
{

    public void Write(string a)
    {
        var str = Encoding.UTF8.GetBytes(a);
        Write(BitConverter.GetBytes(str.Length));
        Write(str);
    }

    public void Write(int b)
    {
        Write(BitConverter.GetBytes(b));
    }
    public void Write(bool b)
    {
        Write((byte)(b ? 1 : 0));
    }
    public void Write(byte b)
    {
        Write(new[] { b });
    }
    public void Write(Vector3 vector3)
    {
        Write(vector3.x);
        Write(vector3.y);
        Write(vector3.z);
    }
    public void Write(Color vector3)
    {
        Write(vector3.r);
        Write(vector3.b);
        Write(vector3.g);
        Write(vector3.a);
    }

    public void Write(float value)
    {
        Write(BitConverter.GetBytes(value));
    }
    public void Write(byte[] bts)
    {
        var bytes = bts;
        this.Write(bytes, 0, bytes.Length);
    }
}

public class BinaryReader : MemoryStream
{

    public BinaryReader(byte[] buffer)
        : base(buffer)
    {

    }


    public string ReadString()
    {
        var len = BitConverter.ToInt32(ReadBytes(4), 0);
        string readString = Encoding.UTF8.GetString(ReadBytes(len), 0, len);
        return readString;
    }

    public Vector3 ReadVector()
    {
        Vector3 v = new Vector3();
        v.x = ReadFloat();
        v.y = ReadFloat();
        v.z = ReadFloat();
        return v;
    }
    public Color readColor()
    {
        Color c = new Color();
        c.r = ReadFloat();
        c.b = ReadFloat();
        c.g = ReadFloat();
        c.a = ReadFloat();
        return c;
    }
    public int ReadInt()
    {
        return BitConverter.ToInt32(ReadBytes(4), 0);
    }
    public float ReadFloat()
    {
        return BitConverter.ToSingle(ReadBytes(4), 0);
    }
    public bool ReadBool()
    {
        return ReadByte2() == 1;
    }
    public byte ReadByte2()
    {
        var b = base.ReadByte();
        if (b == -1) throw new Exception("stream end");
        return (byte)b;
    }
    public byte[] ReadBytes(int len)
    {
        var b = new byte[len];
        int a = Read(b, 0, len);
        if (a != len) throw new Exception("stream ended");
        return b;
    }

    
}
public enum LevelPackets
{
    Spline, Point,
    CheckPoint,
    Start,
    scale,
    Laps,
    Material,
    Nitro,
    StartPos,
    Version,
    Finnish,
    shape,
    brush,
    heightOffset,
    AntiFly,
    Flying,
    shapeMaterial,
    roadtype,
    Wall,
    ClosedSpline,
    Block,
    disableTerrain,
    CheckPoint2,

    rotateTexture,
    textureTile,
    levelTime,
    unityMap,
    disableJump,
    FlyingModel,
    flatTerrain,Unknown
}
public static class Ext
{
    public static T Random<T>(this IList<T> ts)
    {
        if (ts.Count == 0) return default(T);
        return ts[UnityEngine.Random.Range(0, ts.Count)];
    }
    public static IEnumerable<Transform> GetTransforms(this Transform ts)
    {
        yield return ts;
        foreach (Transform t in ts)
        {
            foreach (var t2 in GetTransforms(t))
                yield return t2;
        }
    }

    public static string[] SplitString(this string text)
    {
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return lines;
    }

    public static T2 TryGetDontSet<T, T2>(this Dictionary<T, T2> dict, T key, T2 def)
    {
        T2 o;
        if (dict.TryGetValue(key, out o))
            return o;
        return def;
    }

    public static T2 TryGet<T, T2>(this Dictionary<T, T2> dict, T key, T2 def)
    {
        T2 o;
        if (dict.TryGetValue(key, out o))
            return o;
        return dict[key] = def;
    }

 }
public enum ModType { user = 0, tester = 1, mod = 5, admin = 10 }

public struct EvalCor
{
    public string s;
    public object[] args;
}
public static class ServerProps
{
    public static string score2 = "_score2_";
}
internal enum CullEnum { Sharp, Smooth, None }

[Flags]
public enum LogLevel
{
    Log = 1,
    Url = 8,
    temp = 32,
}

public class EnumFlagsAttribute : PropertyAttribute
{
}


public class Team
{
    public string teamName { get { return bs._Loader.pursuit ? (team == TeamEnum.Blue ? "Cops: " : "Bandits: ") : team + " Team: "; } }
    public int score;
    public int count;
    public TeamEnum team;
    public Team(TeamEnum t)
    {
        team = t;
    }
    public IEnumerable<Player> players
    {
        get { return bs._Game.listOfPlayers.Where(a => a.replay.teamEnum == team); }
    }


    public void Update()
    {
        count = players.Count();
    }
}
public enum GameState { none, started, finnish }
public enum props
{
    mapname, difficulty, rain, night, topdown, rewinds, wait, dm, version, life, wallCollision, team, gametype, bombCar, havePassword, weapons, speedLimit, mapId, zombies,
    dmLockForward,
    enableMatchTimeLimit, enableCollision, timeLimit,
    randomSpawn,
    airResistence,nitro,
    rotationFactor
}

public class ServerScore
{
    public string nick;
    public float value;
}

public partial class Room
{
    //public float matchTimeLimit { get { return Loader.CustomProperty(this, props.timeLimit, 0); } }
}

public class AccessTime<T>
{
    private T t;
    public int frameCount = 1;
    public T value
    {
        get { return t; }
        set
        {
            t = value;
            accessTime = Time.frameCount;
        }
    }
    private int accessTime =-999;
    public bool needUpdate
    {
        get
        {
            return Time.frameCount - accessTime >= frameCount;
        }
    }

    public override string ToString()
    {
        return value.ToString();
    }
}

//public class AccessTime<T>
//{
//    private T t;
//    public int accessTime;
//    public Func<T> a;

//    //public static implicit operator AccessTime<T>(T value)
//    //{
//    //    var s = new AccessTime<T>();
//    //    s.t = value;
//    //    s.accessTime = Time.frameCount;
//    //    return s;
//    //}
//    public static implicit operator T(AccessTime<T> value)
//    {
//        if (value.accessTime != Time.frameCount)
//            value.t = value.a();
//        return value.t;
//    }
//}

[Serializable]
public class SkinSet
{
    public string Name;
    public bool enable;
    public SkinType fr;
    public SkinType to;
    public GUISkin skin;
}
public enum SkinType
{
    None, Button, Window, Toggle, mapSelectButton, Label, LabelGlow, MenuButton, VerticalScrollbar, VerticalScrollBarThumb, TextField, asd, HorizontalScrollBar, HorizontalScrollBarThumb, VerticalScrollbarUpButton, VerticalScrollbarDownButton,
    HorizontalScrollbarDownButton,
    HorizontalScrollbarLeftButton,
    HorizontalScrollbarRightButton,
    horizontalSliderThumb,
    horizontalSlider,
    verticalSliderThumb,
    verticalSlider
}