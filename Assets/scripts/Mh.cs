using System;
using System.Collections;
using System.Text;
using UnityEngine;

public partial class Loader
{
    
    public bool mh;
    //public VarCheck medalsCheck = new VarCheck();
    //public VarCheck carsCheck= new VarCheck();
    public void MhSend(string msg)
    {
        if (!_Loader.mh)
            StartCoroutine(MhSend2(msg));
    }
    public IEnumerator MhSend2(string msg)
    {
        if (mh) yield break;
        if (msg != null)
        {
            if (isDebug)
                print(msg);
            mh = true;
            msg += " player:" + _Loader.playerName + " password:" + _Loader.password + " deviceId:" + SystemInfo.deviceUniqueIdentifier + " version:" + setting.version;
        }
        WWWForm f = new WWWForm();
        if (msg != null)
            f.AddField("msg", Convert.ToBase64String(Encoding.UTF8.GetBytes(msg)));
        f.AddField("id", SystemInfo.deviceUniqueIdentifier);
        f.AddField("version", setting.version);
        if(!guest)
            f.AddField("name", _Loader.playerName);
        //for (int i = 0; i < 10; i++)
        //{
            var w = new WWW("https://tmrace.net/tm/scripts/stats2.php", f);
            print(w.url + Encoding.UTF8.GetString(f.data));
            yield return w;
            //Debug.LogWarning(w.url + w.text);
            if (string.IsNullOrEmpty(w.error))
            {
                if (w.text.StartsWith("1"))
                    mh = true;
                //break;
            }
            //yield return new WaitForSeconds(60);
        //}
    }
    public void UpdateMh()
    {
        if (online && _Loader.mh)
            PhotonNetwork.Disconnect();
        //if (medalsCheck.IsMh())
        //    StartCoroutine(MhSend("medals changed to " + medalsCheck.Value));
        //if (carsCheck.IsMh())
        //    StartCoroutine(MhSend("car changed to " + carsCheck.Value));

        //medalsCheck.Value = medals;

    }
}
//[Serializable]
//public class VarCheck
//{

//    int check;
//    public int m_Value;
//    public bool IsMh()
//    {
//        return check != 0 && m_Value != (check ^ 2424);
//    }
//    public int Value
//    {
//        get
//        {
//            return m_Value;
//        }
//        set
//        {
//            check = value ^ 2424;
//            m_Value = value;
//        }
//    }
//}