using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Random = System.Random;

#if !UNITY_WP8
using CodeStage.AntiCheat.ObscuredTypes;
#else
using ObscuredInt = System.Int32;
using ObscuredFloat = System.Single;
#endif


public enum TeamEnum { Red, Blue }
[Serializable]
public class Replay:IComparer<Replay>
{

    
    public bool blue { get { return teamEnum == TeamEnum.Blue; } }
    public bool red { get { return teamEnum == TeamEnum.Red; } }
    public bool cop { get { return blue && bs._Loader.pursuit; } }
    internal TeamEnum teamEnum =  TeamEnum.Blue;
    
    public Color? color;
    public Difficulty dif = Difficulty.Normal;
    public string Name;
    public string playerName="guest999";
    public bool finnished { get { return pl == null || pl.finnished; } }
    public int rank;
    public Player pl;
    public bool ghost { get { return pl == null || pl.ghost; } }
    public bool selected;
    public int avatarId;
    public int carSkin = 0;
    public int backupRetries = 0;
    public CountryCodes contry;
    internal float m_finnishTime;
    internal SecureFloat m_finnishTimeSec;
    internal float finnishTime
    {
        get { return bs.online ? (float)m_finnishTimeSec : m_finnishTime; }
        set
        {
            if (bs.online) m_finnishTimeSec = value;
            else m_finnishTime = value;
        }
    }
    //public string levelName;
    internal List<PosVel> posVels = new List<PosVel>();
    public List<KeyDown> keyDowns = new List<KeyDown>();
    internal bool[] OnKey = new bool[500];
    internal bool[] OnKeyDown = new bool[500];
    internal bool[] OnKeyUp = new bool[500];
    internal string clanTag = "";
    internal string playerNameClan { get { return clanTag + playerName; } }
    internal bool ally;
    internal float voiceChatTime = bs.MinValue;
    internal int textColor;
    public static int id;
    public string avatarUrl;
    public ModType modType;
    internal bool ignored;
    public Replay()
    {
        teamEnum = bs.isDebug ? TeamEnum.Red: TeamEnum.Blue;
        textColor = id % (GameGui.colors.Length - 1);
        id++;
    }
    public int place;
    public string getText(bool drawScore = true)
    {
        Replay r = this;
        //float f = bs._Loader.dmNoRace && r.pl != null ? (int)r.pl.score : ;
        StringBuilder sb = new StringBuilder();
        if (drawScore)
        sb.Append(place + ". ");
        sb.Append("<color=");
        sb.Append(modType >= ModType.mod && !bs.isDebug ? "purple" : bs._Loader.race ? "#15FF00" : teamEnum == TeamEnum.Blue ? "blue" : "red");
        sb.Append(">");
        sb.Append(playerNameClan);
        sb.Append("</color>");

        
        if (drawScore)
        {
            sb.Append("<color=blue>");
            //if (bs._Loader.pursuit)
            //    sb.Append(GuiClasses.Tr(pl.dead ? "(dead)" : cop ? "(cop)" : "(bandit)"));    
            if (bs._Loader.race)
            {
                float f = r.pl != null && !bs.online ? r.pl.finnishTime : r.finnishTime;
                if (f > 0)
                    sb.Append("[" + bs.TimeToStr(f) + "]");
            }
            if (bs._Loader.dmOrPursuit && r.pl != null)
                sb.Append("[" + r.pl.scoreInt + "]");
            sb.Append("</color>");
        }

        //string score;
        //if (bs._Loader.pursuit) score = pl.dead ? "(busted)" : cop ? "(cop)" : "(bandit)";
        //else
        //{
        //    if (f != 0) score = " [" + (bs._Loader.dmNoRace ? f.ToString() : bs.TimeToStr(f)) + "]";
        //    else score = "";
        //}
        //string text = string.Format("<color={3}>{0}. {1}</color><color=blue>{2}</color>", place, playerNameClan, score, modType >= ModType.mod && !bs.isDebug ? "purple" : bs._Loader.race ? "#15FF00" : teamEnum == TeamEnum.Blue ? "blue" : "red");
        
        return sb.ToString();
    }
    public int Compare(Replay x, Replay y)
    {
        //if (y.dif > x.dif)
        //    return 1;
        //if (y.dif < x.dif)
        //    return -1;
        
        return x.finnishTime.CompareTo(y.finnishTime);
    }
    public Replay CloneAndClear()
    {
        Replay cloneAndClear = (Replay)MemberwiseClone();
        cloneAndClear.posVels = new List<PosVel>();
        return cloneAndClear;
    }
}