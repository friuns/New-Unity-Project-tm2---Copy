#define blur
using System.Linq;
using System;
using System.Collections.Generic;
//using UnityEditor;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using gui = UnityEngine.GUILayout;

public class AutoQuality : GuiClasses
{
    internal float fps = 0;
    internal bool stop;
    public override void Awake()
    {
        
        base.Awake();
    }
    public IEnumerator Start()
    {
        //if (Application.isEditor)
        //    return OnLevelWasLoaded2(0);
        //print("asd");
        return null;
    }
    public bool autoQuality { get { return _Loader.autoQuality; } }
    //private bool notGame
    //{
    //    get { return _Game == null && _Loader.levelEditor == null; }
    //}
    
    public IEnumerator OnLevelWasLoaded2(int level)
    {
        print("public IEnumerator OnLevelWasLoaded2(int level)");
        yield return new WaitForSeconds(.1f);
        //if (notGame) yield break;
        //Debug.LogWarning("OnLevelWasLoaded " + _Loader.mapName);
        
        StartCoroutine(SetQuality(autoQuality ? Quality2.Low : _Loader.quality));
        stop = !autoQuality;
        enabled = true;
        fps = 0;
        yield return null;
    }
    public new bool enabled { get { return base.enabled; } set { base.enabled = value; } }
    public void Update()
    {
        //if (Application.isEditor)
        //{
        //    QualitySettings.vSyncCount = -1;
        //    Application.targetFrameRate = 60;
        //}
        //if (notGame) return;
        if(setting.fps10)
            Application.targetFrameRate = 10 + Random.Range(0, 30);
        
        //Log(QualitySettings.shadowDistance,false);
        //UpdateCull();
        //Log(_Loader.prefixplat);
        if (_Loader.deltaTime != 0)
            fps = Mathf.Lerp(fps, 1f / _Loader.deltaTime, _Loader.deltaTime * (1f / _Loader.deltaTime) > fps ? 1 : .01f);


        Log(isDebug ? fps : Mathf.Round(1f / _Loader.deltaTime), true);
        //Log("Quality: " + _Loader.quality);
        //if (_Player != null)
        //    Log("Renderer: " + _Player.camera.renderingPath);
        if (!stop && fps > 40 && autoQuality && _Loader.quality < Quality2.High)
            StartCoroutine(SetQuality(_Loader.quality + 1));
        if(KeyDebug(KeyCode.E,"Change Quality"))
            StartCoroutine(SetQuality(_Loader.quality + 1));
        if (KeyDebug(KeyCode.Q, "Change Quality") || fps < 35 && _Loader.quality > Quality2.Low && autoQuality)
        {
            StartCoroutine(SetQuality(_Loader.quality - 1));
            print("Auto quality stop");
            stop = true;
        }

    }
    internal float fixedDeltaTime = 0.005f;
    //private Material oldSky;
    public IEnumerator SetQuality(Quality2 q)
    {
        Debug.LogWarning("SetQuality:" + q);
        //return;
        //if (!webPlayer && q == Quality.Ultra)
        //{
        //    if (_Loader.quality == Quality.High) return;
        //    q = Quality.High;
        //}
        _Loader.quality = q;
        
        //if (Application.platform == RuntimePlatform.Android && _Game == null)
        //    q = Quality.Low;
        if (!drawDistanceSet)
        {
            if (android)
                _Loader.drawDistance = q == Quality2.Lowest ? 150 : q <= Quality2.Low ? 200 : q == Quality2.Medium ? 300 : 500;
            else
                _Loader.drawDistance = q == Quality2.Lowest ? 200 : q == Quality2.Low ? 1200 : 10000;
        }
        if (q < Quality2.Lowest) q = Quality2.Lowest;
        if (q > Quality2.Ultra) q = Quality2.Ultra;
        if (!android)
        {
            var qlevel = q >= Quality2.Medium ? 5 : q >= Quality2.Low && !android ? 3 : 0;
            if (QualitySettings.GetQualityLevel() != qlevel)
                QualitySettings.SetQualityLevel(qlevel, !autoQuality);
        }
        RenderSettings.fog = q > Quality2.Low;

        fixedDeltaTime = Time.fixedDeltaTime = q < Quality2.Low ? 0.01f : 0.005f;
        Shader.globalMaximumLOD = q > Quality2.Low ? 600 : 100;
        //if (android)
        //{
        //    QualitySettings.vSyncCount = 0;
        //    Application.targetFrameRate = -1;
        //}
        

        //if (autoQuality)
        _Loader.shadows = q >= Quality2.High && !(flash && splitScreen);
        
        UpdateShadows();
        //if (game)
        //{
        var findGameObjectWithTag = GameObject.FindGameObjectWithTag("RearCamera");
        if (findGameObjectWithTag != null)
            findGameObjectWithTag.camera.enabled = q > Quality2.Low && _Loader.rearCamera;
        foreach (Camera a in Camera.allCameras)
            if (a.tag == Tag.mainCamera || a.tag == Tag.rearCamera)
            //if (a.name != "Hud")
            {
                //print(a.clearFlags);

                if (q < Quality2.Low)
                    RenderSettings.skybox = null;
                
                
                a.renderingPath = q <= Quality2.Low ? RenderingPath.VertexLit : RenderingPath.DeferredLighting;
                print(a.renderingPath);
                //if (setting.hideCull || q == Quality2.Lowest && android)
                //    a.cullingMask &= ~(1 << Layer.cull);
                //else
                //    a.cullingMask |= 1 << Layer.cull;

                    if (android && lowQuality)
                        a.cullingMask &= ~(1 << Layer.stadium);
                    else
                        a.cullingMask |= 1 << Layer.stadium;

                    if (android && lowQuality)
                        a.cullingMask &= ~(1 << Layer.water);
                    else
                        a.cullingMask |= 1 << Layer.water;

                a.backgroundColor = RenderSettings.fogColor;
                a.farClipPlane = lowQuality && android ? 1000 : 10000;

                a.hdr = _Loader.enableBloom;

                //if (_Loader.levelEditor == null)
                    a.nearClipPlane = android ? .3f : .1f;
                
            }
        //}
        if (_MapLoader != null && _MapLoader.terrain != null)
        {
            _MapLoader.terrain.gameObject.SetActive(!lowQuality || _Loader.dm);
            //_MapLoader.terrain.collider.enabled = _MapLoader.terrain.enabled = (!lowQuality || _Loader.dm);
            if (android)
                _MapLoader.terrain.heightmapMaximumLOD = 1;
            _MapLoader.terrain.editorRenderFlags = highOrNotAndroid ? TerrainRenderFlags.all : TerrainRenderFlags.heightmap | TerrainRenderFlags.details;
        }
        foreach (AudioReverbFilter a in FindObjectsOfType(typeof(AudioReverbFilter)))
            a.enabled = medium; 

        foreach (MonoBehaviour a in FindObjectsOfType(typeof(PostEffectsBase)))
            EnablePostEffect(a);
        foreach (MonoBehaviour a in FindObjectsOfType(typeof(SSAOEffect)))
            EnablePostEffect(a);
#if blur
        foreach (MonoBehaviour a in FindObjectsOfType(typeof (AmplifyMotionEffect)))
            a.enabled = _Loader.enableBlur && highQuality;
#endif

        foreach (SunShafts efect in FindObjectsOfType(typeof(SunShafts)))
        {
            foreach (Light lt in FindObjectsOfType(typeof(Light)).Cast<Light>().Where(b => b.type == LightType.Directional))
                if (lt.flare != null)
                    lt.enabled = !UltraQuality;
                else
                    efect.sunTransform = lt.transform;
        }

        if (_Loader.levelEditor == null && _Game != null)
            UpdateMaterials();
        //if (android && _Game != null)
        //{
        //    if (_Player != null)
        //        _Player.hud.gameObject.SetActive(quality > Quality2.Medium);
        //    if (_Player2 != null)
        //        _Player2.hud.gameObject.SetActive(quality > Quality2.Medium);
        //}
        UpdateCull();

        if (!flash)
        {
            Application.targetFrameRate = -1;
            int vSyncCount = setting.fps10 ? 0 : isDebug || !lowQuality ? 1 : 0;
            QualitySettings.vSyncCount = vSyncCount;
            print("Set Vsync " + vSyncCount);
        }
        
        yield return null;
    }
    private static void EnablePostEffect(Behaviour efect)
    {
        efect.enabled = UltraQuality && !android && !((efect is Bloom || efect is ColorCorrectionCurves || efect is Tonemapping) && !_Loader.enableBloom);        
    }
    static Dictionary<string,Texture2D> textureSkin = new Dictionary<string, Texture2D>();
    public Texture2D GetTexture(string s)
    {
        if (textureSkin.ContainsKey(s))
            return textureSkin[s];
        else
        {
            var path = "skin/" + s;
            Texture2D texture2D = (Texture2D)LoadRes(path);
            if(texture2D==null)
                print("texture not found "+path);
            return textureSkin[s] = texture2D;
        }
    }
    public void ChangeSkin()
    {
        print("Change Skin");
        foreach (var r in _Game.LevelRenderers.Where(a => a != null))
            foreach (var m in Application.isEditor ? r.materials : r.sharedMaterials)
            {
                if (Application.isEditor)
                    m.name = m.name.Replace(" (Instance)", "");
                var mainTexture = m.mainTexture;
                if (mainTexture != null)
                {
                    var t = GetTexture(mainTexture.name);

                    if (t != null)
                    {
                        m.mainTexture = t;
                        //if (t.format == TextureFormat.DXT5 || t.format == TextureFormat.ARGB32)
                        //    m.shader = bs.res.transparmentMat.shader;
                    }
                }
            }
    }
    public void UpdateMaterials()
    {
        //return;
        //var f = _Game.night ? .1f : RenderSettings.skybox.GetColor("_Tint").a * 2;
        if(setting.changeSkin && _Loader.mapLoader)
            ChangeSkin();
        //print("FFFF "+f);
        print("Update Materials " + _Game.LevelRenderers.Count);
        if (!Application.isEditor)
            foreach (var r in _Game.LevelRenderers)
                if (r != null)
                {
                    //var reflect = Shader.Find("ReflectiveSpecular2");
                    //var diffuse = Shader.Find("Diffuse");
                    var name = r.sharedMaterial.name;
                    var material = Application.isEditor ? r.material : r.sharedMaterial;
                    if (Application.isEditor)
                        material.name = name;
                    if ((material.shader.name == res.reflect.name || material.shader.name == res.diffuse.name || material.shader.name == "Diffuse"))
                    {
                        if (highQuality && !res.dirtMaterials.Contains(material.name.Split(' ')[0]))
                        {
                            material.shader = res.reflect;
                            material.SetTexture("_SpecTex", res.noise);

                            material.SetFloat("_Shininess", 1);
                            material.SetColor("_ReflectColor", Color.white * 1 * (_Loader.rain ? .2f : _Loader.night ? .05f : .1f)); //RenderSettings.ambientLight 
                            //if (_Game.rain)   
                            //    r.material.color *= .8f;
                            material.SetColor("_SpecColor", Color.white * .3f);
                            material.SetTexture("_Cube", res.cubeMap);
                            material.SetTexture("_MainTex2", res.noise);
                            material.SetTextureScale("_MainTex2", new Vector2(10, 50));
                        }
                        else
                        {
                            material.shader = res.diffuse;
                        }
                    }
            }
    }
    public void UpdateShadows()
    {
        QualitySettings.shadowDistance = _Loader.shadows ? (android ? 50 : _Loader.levelEditor != null ? 1000 : 150) : 0;
    }
    public void UpdateCull()
    {
        
        //if (_Loader.CullMode == 1)
        //{
        //    foreach (var c in Camera.allCameras)
        //        if (c.tag == Tag.mainCamera)
        //        {
        //            c.useOcclusionCulling = false;
        //            c.layerCullSpherical = false;
        //            c.layerCullDistances = new float[32];
        //            c.farClipPlane = _Loader.drawDistance;
        //        }
        //}
        //else// if (_Loader.CullMode == 0 || _Loader.CullMode == 2)
        //{

        print("UpdateCull()" + _Loader.drawDistance);
        var drawDistance = _Loader.drawDistance;
        foreach (var c in Camera.allCameras)
            if (c.tag == Tag.mainCamera)
            {
                float[] ds = new float[32];
                var cull = _Loader.CullMode;
                if (cull == 0)
                {
                    c.layerCullSpherical = true;
                    c.useOcclusionCulling = false;
                    ds[Layer.cull] = drawDistance;
                    ds[Layer.level] = drawDistance;
                    ds[Layer.model] = drawDistance;
                    ds[Layer.def] = drawDistance;
                    ds[Layer.terrain] = highQuality ? 0 : drawDistance;
                    ds[Layer.fx] = 100;
                    ds[Layer.block] = drawDistance / 2f;
                }
                if (cull == CullEnum.Smooth)
                {
                    c.useOcclusionCulling = false;
                    c.layerCullSpherical = false;
                }
                else
                {
                    c.layerCullSpherical = false;
                    c.useOcclusionCulling = true;
                }
                c.farClipPlane = cull == CullEnum.Smooth ? drawDistance : 10000;
                c.layerCullDistances = ds;
            }
        //}
        
        
    }
    private static bool drawDistanceSet { get { return PlayerPrefsGetBool("drawDistanceSet"); } set { PlayerPrefsSetBool("drawDistanceSet", value); } }
    public void DrawSetQuality()
    {
        //Label("Select Quality:");

        //if (_Loader.autoQuality)
        //    GUI.enabled = false;

        Label("Graphics Quality");
        GUILayout.BeginHorizontal();
        
        if (!androidPlatform)
            _Loader.autoQuality = GlowButton("Auto", _Loader.autoQuality) || _Loader.autoQuality;
        Quality2 q = (Quality2)Toolbar(autoQuality ? -1 : (int)quality, Enum.GetNames(typeof(Quality2)), false, false, android || flash ? 4 : 99, -1, false);
        GUILayout.EndHorizontal();
        if (quality != q && (int)q != -1)
        {
            _Loader.autoQuality = false;
            drawDistanceSet = false;
            StartCoroutine(SetQuality(q));
        }

        //GUI.enabled = true;
        if (Application.platform != RuntimePlatform.Android)
        {
            bool shadows = Toggle(_Loader.shadows, "Enable shadows");
            if (shadows != _Loader.shadows)
            {
                _Loader.shadows = shadows;
                UpdateShadows();
            }

        }
#if blur
        if (highQuality)
        {
            var mb = Toggle(_Loader.enableBlur, "Motion blur");
            if (mb != _Loader.enableBlur)
            {
                _Loader.enableBlur = mb;
                StartCoroutine(SetQuality(quality));
            }
        }
#endif
        if (UltraQuality)
        {
            bool bloom = Toggle(_Loader.enableBloom, "Enable Bloom");
            if (bloom != _Loader.enableBloom)
            {
                _Loader.enableBloom = bloom;
                StartCoroutine(SetQuality(quality));
            }
        }
    }

    public void DrawDistance()
    {
        
        Label(Trs("Draw Distance: ") + _Loader.drawDistance);
        int hs = (int)GUILayout.HorizontalSlider(_Loader.drawDistance, 1, android ? 1000 : 5000);
        if (hs != _Loader.drawDistance)
        {
            drawDistanceSet = true;
            _Loader.drawDistance = hs;
            UpdateCull();
        }
    }
    public static void InitSkybox()
    {
        if (RenderSettings.skybox == null && !lowestQuality)
            RenderSettings.skybox = (Material)LoadRes("Skybox16");
    }    
}

//public void UpdateLod()
//    {
//        var distLod = highQuality && android ? .5f : lowestOrAndorid ? .9f : lowQuality ? .5f : 0;
//        foreach (Renderer r in FindObjectsOfType(typeof(Renderer)))
//        {
//            if (r.gameObject.layer == Layer.level || r.gameObject.layer == Layer.model || r.gameObject.layer == Layer.cull)
//                SetLod(r, distLod);
//        }
//    }
//    public void SetLod(Renderer r, float dist = 0)
//    {
//        //if (!cullLod)
//        //    Destroy(r.GetComponent<LODGroup>());
//        //else
//        {
//            var f = r.bounds.size.magnitude / 100f;
//            f = Mathf.Min(1, f);
//            LODGroup a = null;
//            a = r.gameObject.GetComponent<LODGroup>();
//            if (a == null)
//                a = r.gameObject.AddComponent<LODGroup>();
//            a.SetLODS(new LOD[] { new LOD() { renderers = new Renderer[] { r }, screenRelativeTransitionHeight = dist * f } });
//        }

//    }
//internal bool cullLod = false;
//private float oldd;
public enum Dock { Left, Right, UP, Down, Center }