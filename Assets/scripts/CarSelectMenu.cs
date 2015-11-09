using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using CodeStage.AntiCheat.ObscuredTypes;
using gui = UnityEngine.GUILayout;
using System.Text;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class CarSelectMenu : GuiClasses
{

    public SecureInt car=-1;
    public GUITexture leftButton;
    public GUITexture rightButton;
    public Texture2D lockedIcon;
    public Texture2D selectIcon;
    public GUITexture selectButton;
    public GUITexture cancelButton;
    public GUITexture selectFlagButton;
    public Transform CarPlaceHolder;
    public GUIText carName;
    public GUIText lockedTxt;
    public GUITexture textbacground;
    public GUITexture lockedButton;
    public void Start()
    {
        AutoQuality.InitSkybox();
        foreach (GUITexture a in FindObjectsOfType(typeof(GUITexture)))
            if (a != _Loader.fullScreen)
            {
                var rr = a.pixelInset;
                rr.width = (int)rr.width * Screen.width / 800f;
                rr.height = (int)rr.height * Screen.width / 800f;
                a.pixelInset = rr;
            }
        LogEvent("!CarSelect");
        win.ShowWindow(Window, null, true);
        car = _Loader.carSkin == -1 ? 0 : _Loader.carSkin;
        sPos = car;
        LoadSkin();
        camRot = cam.eulerAngles;
        //camera = Camera.main;

        StartCoroutine(_AutoQuality.OnLevelWasLoaded2(0));
        //_Loader.ShowWindow(CarSelectWindow);
    }
    //public void CarSelectWindow()
    //{

    //    win.Setup(2000, 150, "Select car", Dock.Down);
    //    gui.BeginHorizontal();
    //    gui.Button(leftButton.texture, gui.Width(64), gui.Height(64));
    //    gui.FlexibleSpace();
    //    gui.Button(cancelButton.texture, gui.Width(64), gui.Height(64));
    //    gui.FlexibleSpace();
    //    gui.Button(selectButton.texture, gui.Width(64), gui.Height(64));
    //    gui.FlexibleSpace();
    //    gui.BeginVertical();
    //    gui.Label(new GUIContent("Speed",res.rating[0]));
    //    gui.Label(new GUIContent("Rotation", res.rating[0]));
    //    gui.Label(new GUIContent("Drift", res.rating[0]));
    //    gui.EndVertical();
    //    gui.FlexibleSpace();
    //    gui.Button(rightButton.texture, gui.Width(64), gui.Height(64));
    //    gui.EndHorizontal();


    //}

    public Color huefrag(Vector2 o, Color te)
    {
        float p = Mathf.Floor(o.x * 6);
        float i = o.x * 6 - p;
        Color c = p == 0 ? new Color(1, i, 0, 1) :
                  p == 1 ? new Color(1 - i, 1, 0, 1) :
                  p == 2 ? new Color(0, 1, i, 1) :
                  p == 3 ? new Color(0, 1 - i, 1, 1) :
                  p == 4 ? new Color(i, 0, 1, 1) :
                  p == 5 ? new Color(1, 0, 1 - i, 1) :
                           new Color(1, 0, 0, 1);
        return c;
    }
    public Color frag(Vector2 o, Color _Color)
    {
        Color c = o.y * Color.white + (_Color - Color.white) * o.x * Color.white;
        c.a = 1;
        return c;
    }

    private Texture2D pickTexture;
    //private Texture2D selectedColorTexture;
    private Texture2D hueTexture;
    private Color hueColor;
    
    private void MaterialPick()
    {
        Label("Pick Color");
        win.dock = Dock.Right;
        bool mouseButton0 = Input.GetMouseButtonDown(0);
        if (hueTexture == null)
            hueTexture = Create(new Texture2D(250, 20), huefrag, Color.white);
        gui.Label(hueTexture, GUIStyle.none, gui.Width(250));
        var r = GUILayoutUtility.GetLastRect();
        var mp = Event.current.mousePosition;
        if (r.Contains(mp) && mouseButton0)
        {
            hueColor = hueTexture.GetPixel((int)((mp.x - r.x) / r.width * hueTexture.width), 1);
            pickTexture = null;
        }
        //gui.Label(selectedColorTexture, GUIStyle.none);
        if (pickTexture == null)
            pickTexture = Create(new Texture2D(100, 100), frag, hueColor);
        gui.Label(pickTexture, GUIStyle.none, gui.ExpandWidth(false));
        r = GUILayoutUtility.GetLastRect(); 
        if (r.Contains(mp))
        {
            if (mouseButton0)
            {
                var pixel = pickTexture.GetPixel((int)Mathf.Clamp((mp.x - r.x), 0, r.width), (int)(Mathf.Clamp(r.height - (mp.y - r.y), 0, r.height - 1)));
                carSkin.color = pixel;
                carSkin.SetColor(renderers);
                //selectedColorTexture = null;
            }
        }
        //if (selectedColorTexture == null)
            //selectedColorTexture = Create(new Texture2D(50, 10), delegate { return color; }, Color.white);
    }
    
    private Texture2D Create(Texture2D hue, Func<Vector2, Color, Color> act, Color c)
    {
        for (int i = 0; i < hue.width; i++)
            for (int j = 0; j < hue.height; j++)
                hue.SetPixel(i, j, act(new Vector2((float)i / hue.width, (float)j / hue.height), c));
        hue.Apply(true);
        return hue;
    }
    private void LoadSkin()
    {
        foreach (Transform b in CarPlaceHolder)
            Destroy(b.gameObject);
        CarSkin carSkin = _Loader.GetCarSkin(CarId,true);
        //carSkin.SetCountry(_Loader.contry);
        var original = carSkin.model;
        carModel = (GameObject)Instantiate(original, CarPlaceHolder.position + original.transform.position.y * Vector3.up, Quaternion.identity);
        renderers = carModel.GetComponentsInChildren<Renderer>();
        if (carSkin.haveColor)
            carSkin.SetColor(renderers);
        Player.SetFlag(carModel, _Loader.Country);
        carModel.transform.parent = CarPlaceHolder;
    }
    
    Renderer[] renderers;
    private GameObject carModel;
    private int CarId
    {
        get { return Mod(car, _Loader.CarSkins.Count); }
    }


    //private bool paintCar;
    public class Profile
    {
        public string prefs = "";

    }
    public void ProfileWindow()
    {
        gui.Label(_Loader.Avatar);
        Label("Medals:"+_Loader.medals);
        Label("Reputation:" + _Loader.reputation);
        Label("Friends:" + _Loader.friendCount);
        Label("Clan:" + _Loader.clanName);

        foreach (var a in _Loader.friends)
        {
            Label(a);
        }
    }
    public void Window()
    {
        win.Setup(350, 600, " ", Dock.Right);
        BeginScrollView();
        //if (gui.Button("Paint Car"))
        //{
        //    paintTexture = new Texture2D(1024, 1024);
        //    paintCar = true;
        //    var renderers = carModel.GetComponentsInChildren<MeshRenderer>();
        //    foreach (MeshRenderer r in renderers)
        //    {
        //        var m = r.material.mainTexture;
        //        var mesh = r.GetComponent<MeshFilter>().mesh;
        //        print("UVS");
        //        print(mesh.uv.Length);
        //        print(mesh.uv2.Length);
        //        mesh.uv = mesh.uv2.ToArray();
        //        r.gameObject.AddComponent<MeshCollider>();
        //        r.material.mainTexture = paintTexture;
        //    }
        //    //Texture2D txt = new Texture2D();
        //}
        //gui.Label(_Loader.Avatar);
        gui.BeginHorizontal();
        if (gui.Button(_Loader.Avatar, skin.label))
            ShowWindow(_Loader.SelectAvatar, Window);
        gui.BeginVertical();
        if (isDebug && Button("Profile"))
        {
            ShowWindowNoBack(ProfileWindow);
        }
        gui.EndVertical();
        gui.EndHorizontal();
        gui.Label(_Loader.playerName.ToUpper());

        gui.BeginHorizontal();
        //if(Math.Abs(old - posx) < .005f)
        var prev = Button("<Prev");
        if (prev || Button("Next>"))
        {
            var i = prev ? -1 : 1;
            while (true)
            {
                car += i;
                if (!_Loader.GetCarSkin(CarId, true).hidden || _Loader.tester)
                    break;
            }
        }
        gui.EndHorizontal();

        if (isDebug && Button("Select"))
        {
            _Loader.carSkin = CarId;
            LoadLevel(Levels.menu);
        }
        if (!locked)
        {
            if (!carSkin.repUnlocked && !_Loader.disableRep)
            {
                skin.label.wordWrap = true;
                if (_Loader.reputation < carSkin.repNeeded) 
                {
                    gui.Label(new GUIContent(string.Format(Tr("You need {0} reputation points to unlock this car"), carSkin.repNeeded), win.reputation)); 
                }
                else
                {
                    gui.Label(new GUIContent(string.Format(Tr("Price: {0} reputation points"), carSkin.repNeeded), win.reputation));
                    if (Button("Buy this car"))
                    {
                        BuyWindow(delegate
                        {
                            carSkin.repUnlocked = true;
                        }, carSkin.repNeeded, Tr("car"));

                    }
                }
            }
            else
            {
                if (ButtonTexture(Tr(locked ? "Locked" : "Select"), locked ? lockedIcon : selectIcon, 60) && !locked)
                {
                    _Loader.carSkin = CarId;
                    LoadLevel(Levels.menu);
                }
                if (car == 0 && Button("Change Skin"))
                {
                    _Loader.Country++;
                    _Loader.Country =
                        (CountryCodes) ((int) _Loader.Country%Enum.GetValues(typeof (CountryCodes)).Length);
                    Player.SetFlag(carModel, _Loader.Country);
                }
                if (carSkin.canPickColor && medium && Button("Pick Color"))
                    BuyWindow(delegate { carSkin.paintUnlocked = true; ShowWindow(MaterialPick, Window); }, 15, Tr("Pick Color"), carSkin.paintUnlocked || _Loader.disableRep);
            }
        }
        else
        {
            skin.label.wordWrap = true;
            gui.Label(new GUIContent(lockedTxt.text,lockedIcon));
        }
        if (!locked && carSkin != null)
        {
            skin.label.alignment = TextAnchor.UpperLeft;
            //skin.label.wordWrap = false;
            gui.Label(new GUIContent(Tr("Top Speed"), res.rating[(carSkin.TopSpeed + 1) * 2]));
            gui.Label(new GUIContent(Tr("Acceleration"), res.rating[(carSkin.Speed + 1) * 2]));
            gui.Label(new GUIContent(Tr("Handling"), res.rating[(carSkin.Rotation + 1) * 2]));
            gui.Label(new GUIContent(Tr("Drift"), res.rating[(carSkin.Drift + 1) * 2]));
            //gui.Label(new GUIContent(Tr("Mass"), res.rating[(carSkin.mass+ 1) * 2]));
        }
        if (input.GetKeyDown(KeyCode.Escape) || Button("Exit")) //
            Cancel();
        if (Button(_Loader.medals + Trs(" medals"), false, 20, false, win.medalsCnt))
            Popup("medaltext", win.medalsCnt);
        if (!_Loader.disableRep)
            if (Button(_Loader.reputation + Trs(" Reputation"), false, 20, false, win.reputation))
                Popup("reptext", win.reputation);
        
        gui.EndScrollView();
    }
    private void BuyWindow(Action a, int rep, string s, bool unlocked = false)
    {
        print(win.act);
        Action oldwin = win.act;
        if (!unlocked)
        {
            ShowWindowNoBack(delegate
            {
                skin.label.wordWrap = true;
                if (_Loader.reputation < rep)
                {
                    gui.Label(new GUIContent(string.Format(Tr("You must have at least {0} reputation points"), rep),win.reputation));
                    if (Button("Back"))
                        ShowWindowNoBack(oldwin);
                }
                else
                {
                    gui.Label(new GUIContent(string.Format(Tr("Unlock '{0}' for {1} reputation points?"), s, rep),
                        win.reputation));
                    gui.BeginHorizontal();
                    if (Button("Yes"))
                    {
                        ShowWindowNoBack(oldwin);
                        _Loader.reputation -= rep;
                        a();
                    }
                    if (Button("No"))
                        ShowWindowNoBack(oldwin);
                    gui.EndHorizontal();
                }
            });
        }
        else
        {
            a();
        }
    }
    //Texture2D paintTexture;
    public Transform cam;
    private Vector3 camRot;
    private float sPos;
    //private float oldDt;
    //internal new Camera camera;
    private Vector2 prevCord;
    private bool locked;
    private CarSkin carSkin;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Cancel();
   
        sPos = Mathf.Lerp(sPos, car, Time.deltaTime * 2);
        sPos = Mathf.MoveTowards(sPos, car, Time.deltaTime * .3f);
        carName.text = (CarId + 1) + "/" + _Loader.CarSkins.Count;
        if (Input.GetMouseButton(0))
        {
            var mouseDelta = getMouseDelta();
            //camRot.x -= mouseDelta.y * 3;
            camRot.y += mouseDelta.x * 3;            
        }
        else
            camRot.y += 10 * Time.deltaTime;
        camRot.x = Mathf.Clamp(camRot.x, 0, 45);
        cam.eulerAngles = camRot;

        GUITexture[] txts = new[] { leftButton, rightButton, selectButton, cancelButton, selectFlagButton };

        carSkin = _Loader.GetCarSkin(CarId, true);
        locked = false;
        if (!carSkin.medalsUnlocked && !_Loader.carsCheat)
        {
            locked = true;
            lockedTxt.text = string.Format(Tr("You need {0} medals to unlock this car, you have {1}"), carSkin.medalsNeeded, _Loader.medals);
        }
        else if (carSkin.friendsNeeded > _Integration.FbFriendsInGame && !_Loader.tankCheat)
        {
            locked = true;
            lockedTxt.text = string.Format(Tr("You need {0} invited friends on facebook to unlock this tank, you have {1}"), carSkin.friendsNeeded, _Integration.FbFriendsInGame);
        }
        lockedButton.enabled = locked;
        selectButton.texture = locked ? lockedIcon : selectIcon;
        lockedTxt.enabled = textbacground.enabled = locked;
        selectFlagButton.enabled = car == 0;
        var posx = Mod(sPos + .5f, 1) - .5f;
        foreach (GUITexture a in txts)
        {
            //a.enabled = animation.isPlaying;
            var hitTest = a.HitTest(Input.mousePosition) && (a != selectButton || !locked);
            a.color = hitTest ? new Color(1, 1, 1, 1) : new Color(.5f, .5f, .5f, 1);

            if (hitTest && Input.GetMouseButtonDown(0))
            {
                //var leftButtonDown = a == leftButton ;
                //var rightButtonDown = a == rightButton ;
                ////if(Math.Abs(old - posx) < .005f)
                //if (leftButtonDown || rightButtonDown)
                //{

                //    var i = leftButtonDown ? -1 : 1;
                //    while (true)
                //    {
                //        car += i;
                //        if (_Loader.GetCarSkin(CarId).friendsNeeded == 0 || _Integration.fbLoggedIn || _Loader.tankCheat)
                //            break;
                //    }
                //    break;
                //}
                //if (selectFlagButton.enabled && selectFlagButton == a)
                //{
                //    _Loader.Country++;
                //    _Loader.Country = (CountryCodes)((int)_Loader.Country % Enum.GetValues(typeof(CountryCodes)).Length);
                //    SetFlag(carModel, _Loader.Country);
                //}
                //if (input.GetKeyDown(KeyCode.Escape) || cancelButton == a)
                //{
                //    Cancel();
                //}
                //else if (!locked && selectButton == a)
                //{
                //    _Loader.carSkin = CarId;
                //    LoadLevel(Levels.menu);
                //}
            }
        }
        if (Math.Abs(old - posx) > .9f)
        {
            LoadSkin();
        }
        old = posx;

        CarPlaceHolder.parent.position = -Camera.main.transform.right * posx * Scale;
    }
    private void Cancel()
    {
        if (_Loader.carSkin == -1)
            _Loader.carSkin = 0;
        LoadLevel(Levels.menu);
    }

    private float old;
    public float Scale = 30;
    
    
    
}