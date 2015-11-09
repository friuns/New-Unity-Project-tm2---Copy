using UnityEngine;

public class ChatPhp
{
    private Vector2 chatScroll = new Vector2(0, 1000);
    public string mapChat;
    private string mapChatInput = "";
    public  string def = "What you think about this map?";
    public string room = "none";
    public void DrawChat()
    {
        var skin = bs._Loader.skin;
        var l = bs._Loader;
        if (mapChat == null)
        {
            mapChat = GuiClasses.Tr(def) + "\n";
            bs.Download(bs.mainSite + "chat/" + room + ".txt", delegate(string s, bool b) { if (b)mapChat += s; chatScroll = new Vector2(0, 10000); }, false);
        }
        
        skin.label.alignment = TextAnchor.UpperLeft;
        chatScroll = GUILayout.BeginScrollView(chatScroll, bs.win.editorSkin.box);
        skin.textField.wordWrap = true;
        
        skin.label.wordWrap = true;
        GUILayout.Label(mapChat, GUILayout.ExpandHeight(true));
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        if (Event.current.keyCode == KeyCode.Return && Event.current.isKey)
            Event.current.Use();
        mapChatInput = GUILayout.TextField(mapChatInput, 200);
        if ((l.Button("Send", false)) && Time.realtimeSinceStartup - sendTime > 5 &&
            !string.IsNullOrEmpty(mapChatInput.Trim()))
        {
            string prms = l.playerNamePrefixed + ": " + mapChatInput.Trim().Replace(":", "-");
            //if(bs.online)
                //bs._GameGui.Chat(bs._Player.playerNameClan + ":" + mapChatInput);
            bs.Download(bs.mainSite + "scripts/chatSend.php", null, true, "map", room, "send", prms);
            mapChat += "\n" + prms;
            sendTime = Time.realtimeSinceStartup;
            mapChatInput = "";
            chatScroll = new Vector2(0, 10000);
        }
        GUILayout.EndHorizontal();
        
    }
    private float sendTime = bs.MinValue;
}