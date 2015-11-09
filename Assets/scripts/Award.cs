using System;
using UnityEngine;

[Serializable]
public class Award
{
    internal string title;
    public string text { get { return GuiClasses.Tr(title); } }
    public Texture2D texture;
        
    public int startLevel = 5000;
    public float factor = 2;
    //public bool precent;
    //public int id;

    public int count { get { return Base2.PlayerPrefsGetInt(bs._Loader.playerName + title + "Award"); } set { Base2.PlayerPrefsSetInt(bs._Loader.playerName + title + "Award", value); } }
    //public int wonTime { get { return Base2.PlayerPrefsGetInt(bs._Loader.playerName + title+ "AwardTime"); } set { Base2.PlayerPrefsSetInt(bs._Loader.playerName + id + "AwardTime", value); } }
    internal int local;
    public void Add(int i = 1)
    {
        //Debug.LogWarning("Award added "+title);
        local+=i;
        count+=i;
    }
    public int total;
    public int level;
    public float upper;

    public void Calculate()
    {
        if (total > 0)
        {
            progress = count/(float) total;
            return;
        }
        var a = this;
        float i1 = 0;

        float i2 = a.startLevel / Mathf.Pow(factor, 7);
        int i;
        for (i = 0; i < bs._Awards.ranks.Length-2; i++)
        {
            if (count< i2)
                break;
            i1 = i2;
            i2 *= factor;
        }
        a.level = i;
        a.upper = i2;
        progress = (float)(count - i1) / (i2 - i1);
    }
    public float progress;
}