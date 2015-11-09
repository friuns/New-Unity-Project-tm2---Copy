using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;
public class AndroidHud : bs
{
    public Player pl;
    public Archor pad;
    public Archor flashBack;
    public Archor brake;
    public Archor forward;
    public Archor padTouch;
    public Archor left;
    public Archor right;
    public Archor nitro;
    private Dictionary<KeyCode, KeyHudBool> m_dict;
    public Dictionary<KeyCode, KeyHudBool> dict { get { if (m_dict == null)InitDict(); return m_dict; } }
    internal Vector2 mouse;
    private KeyHudBool last;
    private void keyhudAdd(KeyHudBool keyHudBool)
    {
        keyHudBool.LoadPos();
        dict.Add(keyHudBool.key, keyHudBool);
    }

    public void InitDict()
    {
        m_dict = new Dictionary<KeyCode, KeyHudBool>();
        keyhudAdd(new KeyHudBool() { key = KeyCode.W, guitext = forward });
        keyhudAdd(new KeyHudBool() { key = KeyCode.S, guitext = brake });
        keyhudAdd(new KeyHudBool() { key = KeyCode.A, guitext = left });
        keyhudAdd(new KeyHudBool() { key = KeyCode.D, guitext = right });
        keyhudAdd(new KeyHudBool() { key = KeyCode.Backspace, guitext = flashBack });
        keyhudAdd(new KeyHudBool() { key = KeyCode.Clear, guitext = pad });
        keyhudAdd(new KeyHudBool() { key = KeyCode.LeftShift, guitext = nitro });

        keyhudAdd(new KeyHudBool() { key = KeyCode.UpArrow, guitext = forward, secondPlayer = true });
        keyhudAdd(new KeyHudBool() { key = KeyCode.DownArrow, guitext = brake, secondPlayer = true });
        keyhudAdd(new KeyHudBool() { key = KeyCode.LeftArrow, guitext = left, secondPlayer = true });
        keyhudAdd(new KeyHudBool() { key = KeyCode.RightArrow, guitext = right, secondPlayer = true });
        keyhudAdd(new KeyHudBool() { key = KeyCode.Insert, guitext = flashBack, secondPlayer = true });
        keyhudAdd(new KeyHudBool() { key = KeyCode.Caret, guitext = pad, secondPlayer = true });
        keyhudAdd(new KeyHudBool() { key = KeyCode.RightShift, guitext = nitro, secondPlayer = true });

    }
    public void Update()
    {   
#if !UNITY_FLASH


        
        padTouch.enabled = pad.enabled = _Loader.controls == Contr.mouse;
        bool keys = _Loader.controls == Contr.keys;

        left.enabled = right.enabled = keys;
        nitro.enabled = pl.nitro > 0;
        
        //Log("Android Mouse " + mouse + " Taps" + Input.touchCount);
        mouse = Vector3.Lerp(mouse, Vector3.zero, Time.deltaTime * 10);
        bool padDown = false;
        if (_Loader.accelometer)
            mouse = new Vector2(Input.acceleration.x, 0) * 2;
        else if (_Loader.enableMouse)
        {
            
            foreach (var touch in Input.touches)
            {
                Vector3 mpos = touch.position;
                Vector3 pp = pad.inversePos;
                pp.x *= Screen.width;
                pp.y *= Screen.height;
                if (mpos.x < Screen.width / 2f && pp.x < Screen.width / 2f || mpos.x > Screen.width / 2f && pp.x > Screen.width / 2f)
                {
                    Vector3 mouseTmp = (pp - mpos) / (pad.size.x / 2f) * (pl.secondPlayer ? 1 : -1);
                    mouseTmp.x = Mathf.Clamp(mouseTmp.x, -1, 1);
                    mouseTmp.y = Mathf.Clamp(mouseTmp.y, -1, 1);
                    if (!splitScreen || mpos.y > Screen.height / 2f && pl.secondPlayer || mpos.y < Screen.height / 2f && !pl.secondPlayer)
                    {
                        mouse = mouseTmp;
                        padTouch.screenPos = mpos;
                        padDown = true;
                    }
                }
                
            }
#if UNITY_ANDROID 
            if (AndroidInput.secondaryTouchEnabled && AndroidInput.touchCountSecondary > 0)
            {
                Vector2 p = AndroidInput.GetSecondaryTouch(0).position;
                p.x /= AndroidInput.secondaryTouchWidth; 
                p.x -= .5f;
                p.x = Mathf.Clamp(p.x, -1, 1) * 2;
                mouse = p;
            }
#endif
            if (!padDown)
                padTouch.transform.position = pad.transform.position;
        }
        if (_Game.editControls)
        {
            if (Input.touchCount == 1)
            {
                foreach (KeyHudBool a in dict.Values)
                {
                    Touch? touch = HitTest(a);
                    if (touch != null)
                    {
                        last = a;
                        Vector2 mpos = touch.Value.position;
                        mpos = pl.hud.camera.ScreenToViewportPoint(mpos);
                        a.guitext.inversePos = mpos;
                        a.posx = mpos.x;
                        a.posy = mpos.y;
                    }
                }
            }
            else if (last != null)
            {
                last.scale += GetDoubleTouch() * 0.01f;
                last.scale = Mathf.Max(.7f, last.scale);
                last.UpdateScale();
            }
        }
        if (!_Game.editControls)
        {

            

            if (keys && !splitScreen)
            {
                foreach (KeyHudBool a in dict.Values)
                    a.hitTest = null;
                foreach (Touch b in touches)
                {
                    Vector2 pos = b.position;
                    pos.x /= Screen.width;
                    pos.y /= Screen.height;
                    KeyHudBool d = dict.Values.Where(a => a.guitext.enabled).OrderBy(a => Vector2.Distance(pos, a.guitext.pos)).FirstOrDefault();
                    //print(d.guitext.name);
                    d.hitTest = b;
                }
            }
            else
            {
                foreach (KeyHudBool a in dict.Values)
                    a.hitTest = HitTest(a);
            }

            foreach (KeyHudBool a in dict.Values)
            {
                TouchPhase ph = a.hitTest.HasValue ? a.hitTest.Value.phase : ((TouchPhase)221);
                a.hold = ph == TouchPhase.Stationary || ph == TouchPhase.Moved;
                a.up = ph == TouchPhase.Canceled || ph == TouchPhase.Ended;
                a.down = ph == TouchPhase.Began;
                if (!splitScreen)
                    a.hudColor = padDown && a.guitext == pad || a.hold ? new Color(.5f, .5f, .5f, .5f) : new Color(0, 0, 0, 0);
            }
        }
#endif
    }
    private Touch? HitTest(KeyHudBool hud)
    {
        foreach (Touch b in Input.touches)
        {
            var pos = b.position;
            if (hud.guitext.HitTest(pos)) return b;
        }
        return null;
    }

    public static float GetDoubleTouch()
    {
        if (touches.Length > 1)
        {
            Vector2 curDist = touches[0].position - touches[1].position;
            Vector2 prevDist = (touches[0].position - touches[0].deltaPosition) - (touches[1].position - touches[1].deltaPosition);
            return curDist.magnitude - prevDist.magnitude;
        }
        return 0;
    }
    
    public class KeyHudBool
    {
        public Touch? hitTest;
        public KeyHudBool LoadPos()
        {
            if (posx != 0 && posy != 0)
                guitext.pos = new Vector3(posx, posy, 0);
            if (scale != 1)
                UpdateScale();
            return this;
        }
        public bool hold;
        public Archor guitext;
        //public Color? m_hudColor;
        //public Color hudColor { get { return m_hudColor ?? (m_hudColor = guitext.color).Value; } set { if (value != m_hudColor)  m_hudColor = guitext.color = value; } }
        public Color hudColor { get { return guitext.color; } set { guitext.color = value; } }
        public KeyCode key;
        public bool down;
        public bool up;
        public bool secondPlayer;
        public string prefix { get { return _Loader.playerName + guitext.name + secondPlayer; } }
        public float scale { get { return PlayerPrefsGetFloat(prefix + "scale2", 1); } set { PlayerPrefsSetFloat(prefix + "scale2", value); } }
        public float posx { get { return PlayerPrefsGetFloat(prefix + "posx2", 0); } set { PlayerPrefsSetFloat(prefix + "posx2", value); } }
        public float posy { get { return PlayerPrefsGetFloat(prefix + "posy2", 0); } set { PlayerPrefsSetFloat(prefix + "posy2", value); } }
        public void UpdateScale()
        {
            var last = this;
            last.guitext.scale = last.scale;
        }
    }
    public static Touch[] touches { get { return Input.touches; } }
}