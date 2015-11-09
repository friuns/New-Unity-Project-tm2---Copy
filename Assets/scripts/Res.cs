using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
public class GameRes{}
public class ResGame { }
public class Res :MonoBehaviour
{
    public AnimationCurve ForceCurve;
    public AnimationCurve RotCurve;
    //public float hitTestForce = 1;
    //public int hitTestAngle = 10;    
    public float rotationStart = 3;
    //public float rotationLerp = 25;
    public float speedSubOnRotate = .05f;
    public Shader[] shaders;

    public Cubemap cubeMap;
    //public AudioClip roar;
    public AnimationCurve gears;
    public AudioClip[] hitSound;
    public AudioClip[] hitSoundBig;
    public Shader reflect;
    public Shader diffuse;
    public Material diffuseMat;    
    public Shader specular;
    public Material transparmentMat ;
    public Shader transparment2;
    public Shader transparmentCutout;
    public Shader transparmentEditor;
    public Texture2D[] go123;
    public Texture2D[] medals;
    public AudioClip speedUp;
    public AudioClip bip;
    public AudioClip start;
    public AudioClip checkPoint;
    public AudioClip idle;
    public AudioClip wind;
    public AudioClip winSound;
    public AudioClip backTime;
    public AudioClip horn;
    public AudioClip sirene;
    public AudioClip engineSound;
    //public Player _Player;
    public GameObject speedUpPrefab;
    public GameObject checkPointPrefab;
    public Game _Game;
    public AudioClip brake;
    public AudioClip mudBrake;
    public AudioClip[] motor;
    public List<CarSkin> CarSkins;
    public List<Scene> scenes;
    public string[] titles;

    public Texture2D sendReplayIcon;

    public List<string> levelMaterialsTxt;
    public List<string> dirtMaterials;
    public List<string> animatedTextures;
    public List<string> specularTextures;
    public GUIStyle mapSelectButton;
    public GUIStyle menuButton;
    public GUIStyle labelGlow;
    public Texture2D[] avatars;
    public Loader loaderPrefab;
    public Texture2D faceBook;
    //public GUIStyle arrow;
    public Texture2D attention;
    public Texture2D android;
    public  PhysicMaterial track;
    public PhysicMaterial border;

    Dictionary<string,WWW> avatarWww = new Dictionary<string, WWW>();
    Dictionary<string,Texture2D> avatarWT = new Dictionary<string, Texture2D>();
    public Texture2D GetAvatar(int avatarId,string avatar)
    {
        var def = avatars[Mathf.Clamp(avatarId, 0, avatars.Length)];
        if (avatarId == 0 && !string.IsNullOrEmpty(avatar))
        {
            if (!avatarWww.ContainsKey(avatar))
                avatarWww[avatar] = new WWW(avatar);

            Texture2D tx;
            if (avatarWT.TryGetValue(avatar, out tx))
                return tx;

            if (avatarWww[avatar].isDone)
            {
                if (string.IsNullOrEmpty(avatarWww[avatar].error))
                    return avatarWT[avatar] = avatarWww[avatar].texture;
                else
                {
                    Debug.LogWarning(avatarWww[avatar].error);
                    return avatarWT[avatar] = def;
                }
            }
        }
        return def;
    }
    public float minRotation2 = .5f;
    public float rotationAdd2 = 100;

    public bool projectFly;
    public float gravitation = 25;
    public GameSettings gameSettings;
    public float stabilize = 4;
    public float stabilizeSpeed = 4;
    public Texture2D noise;
    public Material roadMaterial;
    public Transform startPrefab;
    public Transform CheckPoint;
    public Transform dot;
    public Material[] levelTextures;
    public Material lineMaterialYellow;
    public int renderSettingsId;
    //public List<CustomRenderSettings> renderSettings = new List<CustomRenderSettings>();
    public GameObject explosion;
    public Texture2D[] rating;
    public Shader rainShader;
    public Shader rainShader2;
    public Bounds terrainBounds;
    public Material waterMaterial;
    public Shader waterMaterialDef;
    public Bullet bullet;
    //public GameObject bulletSpark;
    public GameObject hole;
    public GameObject hole2;
    public GameObject turretExp;
    public AudioClip nitro;
    public AudioClip waterSplash;
    public AudioClip waterSound;
    public AudioClip waterSound2;
    public AudioClip carCrash;
    public AudioClip chat;
    public AudioClip[] gearChange;
    public AudioClip[] shoot;
    public AudioClip[] bulletHitSound;
    public AudioClip[] oskolok;
    public AudioClip lifeHitSound;
    public AudioClip hitFeedback;


    public AudioClip youHaveFlag;
    public AudioClip yourTeamHaveFlag;
    public AudioClip enemyHaveYourFlag;

    public AudioClip redFlagReturn;
    public AudioClip blueFlagReturn;
    

    public AudioClip youLoose;
    public AudioClip youWin;
    public AudioClip youWin2;
    public AudioClip endGame;
    public AudioClip jump;
    public AudioClip coinSound;
    public GameObject coin;
    public DamageText damageText;
    public GameObject fire;
    public Texture2D defCursor;
    public Texture2D ramka;
    //public TextAsset outlineDict;
    public Dictionary<string, List<int>> outlines = new Dictionary<string, List<int>>();
    public List<OutlineDict> outlineDict;
    public Texture2D race;
    public Texture2D userMap;
    public Texture2D deathMatch;
    public Texture2D captureFlag;
    public string credits = "";
    public Texture2D scratches;

}
//[Serializable]
public class OutlineDict
{
    public string Name;
    public List<int> outlineValues;
}