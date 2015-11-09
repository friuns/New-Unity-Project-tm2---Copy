using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using gui = UnityEngine.GUILayout;

public class GameGui : bsNetwork
{    
    private Vector3 mp;
    public void Tutorial(){}
    public void ShowHelpScreen(float time)
    {
        lastHelpTime = Time.time + time;
    }
    
    internal float lastHelpTime = 0;
    internal bool chatEnabled;
    public string chat="";
    private bool firstTimeChat;
    [RPC]
    public void Chat(string Obj)
    {
        if (!_Loader.enableChat) return;
        if (!firstTimeChat)
        {
            firstTimeChat = true;
            _Game.centerText(Tr("press enter to reply"), 2);
        }
        string output = Obj + "\r\n";
        if (!chatOutput.EndsWith(output))
        {
            chatOutput += output;
            ClearChat();
        }
        _Game.audio.PlayOneShot(res.chat, _Loader.soundVolume);
    }

    private void ClearChat()
    {

        if (SplitString(chatOutput).Length > 5)
            chatOutput= RemoveFirstLine(chatOutput);
        else
            StartCoroutine(AddMethod(20, delegate { chatOutput = RemoveFirstLine(chatOutput); }));
    }
    public string RemoveFirstLine(string s)
    {
        int i = s.IndexOf("\r\n", System.StringComparison.Ordinal) + 2;
        //if (i == s.Length) return "";
        return s.Substring(i, s.Length - i);
    }
    Vector2 smoothAxis;
    public void OnGUI()
    {
        CustomWindow.GUIMatrix(.7f);
        GUI.depth = 3;

        var minMaxRect = Rect.MinMaxRect(.003f* Screen.width, .13f * Screen.height, Screen.width, Screen.height);

        //if (isDebug)
        //{
        //    smoothAxis += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        //    var value = Random.value;
        //    smoothAxis = Vector2.Lerp(smoothAxis, Vector2.zero, Time.deltaTime * value * value * value * 5);
        //    smoothAxis += Random.insideUnitCircle * smoothAxis.magnitude*Time.deltaTime;
        //    minMaxRect.x -= smoothAxis.x;
        //    minMaxRect.y += smoothAxis.y;
        //}
        GUILayout.BeginArea(minMaxRect);
        GUI.skin.label.wordWrap = false;
        GUI.skin.label.fontSize = 20;
        if (online)
            gui.Label(chatOutput);
        if (chatEnabled)
        {
            GUI.SetNextControlName("Chat");
            chat = gui.TextField(chat);
            GUI.FocusControl("Chat");
        }
        //print(Event.current.keyCode);

        if (win.act == null && Event.current.keyCode == KeyCode.Return && Event.current.isKey && (Event.current.type == EventType.keyUp || chat.Length > 0) && (online || isDebug))
        {
            chatEnabled = !chatEnabled;
            
            if (!chatEnabled && chat.Length > 0 && !_Loader.banned)
                CallRPC(Chat, "<color=" + (_Player.replay.modType >= ModType.mod ? "blue" : "") + ">" + _Player.playerNameClan + ": " + chat + "</color>");
            chat = "";
        }

        var b = _Loader.PlayersCount < 1 && !online;
        for (int i = 0; i < (b ? _Loader.replays.Count : _Game.listOfPlayers.Count); i++)
        {
            Replay r = b ? _Loader.replays[i] : _Game.listOfPlayers[i].replay;
            
            if (cache[i] == null || FramesElapsed(10))
            {
                r.place = i + 1;
                string text = r.getText();
                cache[i] = GUIContent("<color=" + colorstr[r.textColor] + ">" + "#" + "</color>" + text, res.GetAvatar(r.avatarId, r.avatarUrl));
            }
            GUI.skin.label.fontSize = (int)((r.ghost ? 20 : 30) * (Screen.width < 1000 ? .7f : 1f));
            
            GUILayout.Label(cache[i], GUILayout.Height(r.ghost ? 40 : 60), GUILayout.ExpandWidth(false));
            var last = GUILayoutUtility.GetLastRect();
            GUIStyle guiStyle = r.ghost ? _Loader.guiSkins.ramka2 : _Loader.guiSkins.ramka3;
            //guiStyle.contentOffset = new Vector2(r.ghost ? 15: 30, 0);
            GUI.Label(last, _Awards.ranks[r.rank], guiStyle);
            
        }
        
        
        GUILayout.EndArea();
    }
    string[] colorstr = new[] { "black", "blue", "cyan", "lime", "red", "white", "yellow", "magenta", "brown" ,"grey"};
    public static Color[] colors = new[] { Color.black, Color.blue, Color.cyan, Color.green, Color.red, Color.white, Color.yellow, Color.magenta, new Color(0xa5 / 255f, 0x2a / 255f, 0x2a / 255f, 1), new Color(0x80 / 255f, 0x80 / 255f, 0x80 / 255f, 1) };

    public string chatOutput = "";
    GUIContent[] cache = new GUIContent[30];


    internal IEnumerator OpenAndroidChat()
    {
#if UNITY_ANDROID
        var t = TouchScreenKeyboard.Open("");
        yield return t;
        while (!t.done)
            yield return null;
        CallRPC(Chat, _Player.playerNameClan + ": " + t.text);
#endif
        yield return null;
    }
}
