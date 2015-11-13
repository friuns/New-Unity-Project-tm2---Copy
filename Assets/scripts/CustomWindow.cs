#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using gui = UnityEngine.GUILayout;
//using System.Linq;
using System;
using System.Collections.Generic;

using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;

public class CustomWindow : GuiClasses
{
    public Texture2D locked;

    internal float _width;
    internal float _height;
    public GUISkin skinDefault;
    public GUISkin editorSkin;
    public new GUISkin skin;
    internal GUIStyle style;
    public int sizeX;
    public int sizeY;
    private string Window;
    private MonoBehaviour target;
    public string windowTitle;
    public Texture2D windowTexture;
    public Dock dock = Dock.Center;
    internal Action act;
    private Action act2;
    private Dictionary<Action, Action> backs = new Dictionary<Action, Action>();
    public new Texture2D splitScreen;
    public Texture2D[] medals;
    public Texture2D medalsCnt;
    public Texture2D reputation;
    public Texture2D score;
    private float offsetAnim = -1600;
    public float speed = .1f;
    float[] anim = new float[100];
    private int cur;
    private int curAnim;
    public static bool backSpaceDown;
    public override void Awake()
    {

        enabled = false;
        base.Awake();
    }
    public static int buttonId;
    public static int buttonCount;
    public static int curButton = -1;
    public override void OnEditorGui()
    {
#if UNITY_EDITOR
        if (gui.Button("Clean Editor SKin"))
        {
            foreach (GUIStyle a in editorSkin)
            {
                GetValue(a.focused);
                GetValue(a.active);
                GetValue(a.hover);
                GetValue(a.normal);
                GetValue(a.onActive);
                GetValue(a.onFocused);
                GetValue(a.onHover);
                GetValue(a.onNormal);
            }
        }

        //if (GUILayout.Button("Fix Skin"))
        //{
        //    foreach (GUIStyle a in skin)
        //    {
        //        print(a.name);
        //        a.onNormal.background = a.normal.background;
        //    }
        //    UnityEditor.EditorUtility.SetDirty(skin);
        //    foreach (SkinSet a in resEditor.skins)
        //    {
        //        foreach (GUIStyle b in a.skin)
        //            b.onNormal.background = b.normal.background;
        //        UnityEditor.EditorUtility.SetDirty(a.skin);
        //    }

        //}
        if (gui.Button("Save Skin"))
        {
            var builtinSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            var instantiate = (GUISkin)Instantiate(builtinSkin);

            var dps = EditorUtility.CollectDependencies(new Object[] { instantiate });
            foreach (var a in dps)
            {
                print(a);

                if (a is Texture2D)
                {
                    //Texture2D texture2D = (a as Texture2D);
                    //texture2D.Apply(false,false);
                    //texture2D.LoadImage()
                    AssetDatabase.CreateAsset(Instantiate(a), "Assets/EditorSkin/" + a.name + ".asset");
                    //File.WriteAllBytes("Assets/EditorSkin/" + a.name + ".png", texture2D.EncodeToPNG());
                }
            }
            //print(dps.Length);
            //foreach (GUIStyle a in instantiate)
            //{
            //    SaveStyle(a);
            //}
            //AssetDatabase.CreateAsset(instantiate, "Assets/EditorSkin.guiskin");
        }
        base.OnEditorGui();

#endif
    }



#if UNITY_EDITOR
    private static void GetValue(GUIStyleState guiStyleState)
    {
        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(guiStyleState.background)))
            guiStyleState.background = null;
    }
    private void SaveStyle(GUIStyle guiStyle)
    {
        CreateAsset(guiStyle.normal.background);
        //CreateAsset(guiStyle.font);

        //File.write guiStyle.normal.background.EncodeToPNG();
    }
    private static void CreateAsset(Texture2D a)
    {
        AssetDatabase.CreateAsset(Instantiate(a), "Assets/EditorSkin/" + a.name + ".asset");
    }
#endif
    private float WindowScale = 1;
    internal Vector2 offset2;
    internal Vector2 scale;
    internal bool addflexibleSpace;
    internal bool showBackButton = true;
    public void OnGUI()
    {
        if (Event.current.isMouse && (Input.GetMouseButtonUp(0) && mouseDrag > 5f / 1024))
        {
            Event.current.Use();
            //print(mouseDrag);
        }
        buttonCount = buttonId;
        cnt = buttonId = 0;
        GUI.depth = 0;
        scale = GUIMatrix(WindowScale);
        cur = 0;
        float w = _width = Screen.width / scale.x;
        float h = _height = Screen.height / scale.y;
        if (_Loader.mapName == "level" || Application.loadedLevelName == "level")
            GUI.skin = /*resEditor.editorGui ? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector) : */editorSkin;
        else
            GUI.skin = skinDefault;
        skin = GUI.skin;
        if (Window == null && act2 == null)
            enabled = false;
        else
        {
            var c = Vector3.zero;
            var s = new Vector3(Mathf.Min(w, sizeX), Mathf.Min(h, sizeY)) / 2f;
            if (dock == Dock.Right)
                c = new Vector3(w - s.x, h / 2f);
            else if (dock == Dock.Left)
                c = new Vector3(s.x, h / 2f);//+ w * .05f
            else if (dock == Dock.Down)
                c = new Vector3(w / 2f, h - s.y);//+ w * .05f
            else
                c = new Vector3(w, h) / 2f;
            var v1 = c - s;
            var v2 = c + s;
            skin.window.fontSize = 15;
            windowRect = Rect.MinMaxRect(v1.x + offsetAnim * w + offset2.x, v1.y + offset2.y, v2.x + offsetAnim * w, v2.y);
            GUILayout.BeginArea(windowRect, GUIContent(windowTitle, windowTexture), style);
            if (act2 != null)
            {
                act2();
                if (act2 == null) return;
                if (backs.ContainsKey(act2) && backs[act2] != null && showBackButton)
                {
                    if (addflexibleSpace)
                        GUILayout.FlexibleSpace();
                    if (BackButtonLeft())
                        Back();
                }
            }
            else
                target.SendMessage(Window);
            //gui.Button(new GUIContent("Test", "tasd"));
            GUILayout.EndArea();

        }
        if (_Loader.levelEditor == null)
            DrawToolTip();
        if (Event.current.type == EventType.Repaint)
            tooltip = GUI.tooltip;
        //GUI.matrix = mt;
    }

    public void DrawToolTip()
    {
        GUI.Label(new Rect(0, win._height - 15, win._width, 50), tooltip);
    }
    public string tooltip;

    private Rect windowRect;
    public bool WindowHit
    {
        get
        {
            if (!enabled) return false;
            var m = Input.mousePosition;
            return windowRect.Contains(new Vector3(m.x / guiscale.x, m.y / guiscale.y));
        }
    }

    static float originalWidth = 950;
    static float originalHeight = 450;
    public static Vector3 GUIMatrix(float sc = 1)
    {
        var x = ((float)Screen.height / Screen.width) / (originalHeight / originalWidth);
        //var mt = GUI.matrix;

        guiscale = _Loader.scaleButtons && Screen.orientation != ScreenOrientation.AutoRotation
                            ? new Vector3(Screen.width / originalWidth * x, Screen.height / originalHeight, 1) * sc
                            : Vector3.one;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, guiscale);
        return guiscale;
    }
    public static Vector3 guiscale;
    internal float mouseDrag;
    public void Update()
    {

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            mouseDrag = 0;
        else if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
            mouseDrag += new Vector2(Input.GetAxis("Mouse X") / Screen.width, Input.GetAxis("Mouse Y") / Screen.height).magnitude;

        //print(WindowHit);
        KeysForGui();
        if (curAnim < 100)
        {
            if (anim[curAnim] > 0)
                anim[curAnim] -= _Loader.deltaTime * 1500;
            else
            {
                anim[curAnim] = 0;
                curAnim++;
            }
        }
    }
    private void KeysForGui()
    {
        if (setting.useKeysForGui)
        {
            backSpaceDown = input.GetKeyDown(KeyCode.Backspace);
            if (!Input.GetMouseButtonDown(0))
            {
                if (input.GetKeyDown(KeyCode.S))
                    curButton = Mod(curButton + 1, buttonCount);
                if (input.GetKeyDown(KeyCode.W))
                    curButton = Mod(curButton - 1, buttonCount);
            }
        }
    }

    public void CloseWindow()
    {
        print("Close Window");
        Window = null;
        act = act2 = null;
    }
    public void LabelAnim(string s, int fontSize = 16, bool center = false)
    {
        skin.label.wordWrap = false;
        skin.label.fontSize = fontSize;
        skin.label.alignment = center ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
        GUILayout.BeginHorizontal();
        GUILayout.Space(anim[cur]);
        GUILayout.Label(GUIContent(Tr(s)));
        GUILayout.EndHorizontal();
        cur++;
    }
    public void Setup(int x = 400, int y = 300, string tittle = "", Dock dock = Dock.Center, Texture2D txt = null, GUIStyle st = null, float windowscale = 1)
    {
        showBackButton = true;
        addflexibleSpace = true;
        offset2 = Vector2.zero;
        WindowScale = windowscale;
        skin.window.contentOffset = new Vector2(0, -34);
        style = st == null ? skin.window : st;
        sizeX = x;
        sizeY = y;
        windowTitle = Tr(tittle);
        windowTexture = txt;
        this.dock = dock;
    }


    public IEnumerator ShowWindow2(Action func)
    {
        ShowWindow(func, win.act);
        while (act != null && act == func)
            yield return null;
    }
    public void ShowWindow(Action func, Action back = null, bool skipAnim = false)
    {
        if (act == func) return;
        act = func;
        print("Show Window: " + func.Method.Name);
        _Loader.OnWindowShow(func);
        curButton = -1;

        Screen.lockCursor = false;
        scroll = Vector2.zero;
        for (int i = 0; i < anim.Length; i++)
            anim[i] = 500;
        curAnim = 0;
        StopAllCoroutines();
        if (skipAnim || _Loader.levelEditor || isDebug)
        {
            offsetAnim = 0;
            SetWindow(func, back);
        }
        else
            StartCoroutine(cor(func, back));
    }
    public IEnumerator cor(Action func, Action back)
    {
        while (offsetAnim > -2)
        {
            offsetAnim -= _Loader.deltaTime * speed;
            yield return null;
        }
        SetWindow(func, back);
        offsetAnim = 2;
        while (offsetAnim > 0)
        {
            offsetAnim -= _Loader.deltaTime * speed;
            yield return null;
        }
        offsetAnim = 0;
    }
    private void SetWindow(Action func, Action back)
    {
        Setup();
        Screen.lockCursor = false;
        enabled = true;
        act = act2 = func;
        if (back != null)
            backs[func] = back;
#if UNITY_EDITOR
        Window = func.Method.Name;
        target = func.Target as MonoBehaviour;
#endif
    }
    public void Back()
    {
        print("Window Back");
        var action = act2;
        win.CloseWindow();
        if (backs.ContainsKey(action))
            win.ShowWindow(backs[action]);
    }

    //public void OnLevelWasLoaded(int level)
    //{        
    //CloseWindow();
    //}
}
