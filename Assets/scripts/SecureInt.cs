
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;




public struct SecureInt
{
    private ObscuredInt holder;
    private int fake;
    public static implicit operator SecureInt(int value)
    {
        var s = new SecureInt();
        s.fake = s.holder = value;
        return s;

    }
    public static implicit operator int(SecureInt value)
    {
        var v = value.holder;
        if (v != value.fake && !bs._Loader.mh)
            bs._Loader.StartCoroutine(bs._Loader.MhSend2("Value changed from " + v + " to " + value.fake));
        return v;
    }
    public override string ToString()
    {

        return holder.ToString();
    }
}


public struct SecureFloat 
{
    private ObscuredFloat holder;
    private float fake;
    public static implicit operator SecureFloat(float value)
    {
        var s = new SecureFloat();
        s.fake = s.holder = value;
        return s;

    }
    public static implicit operator float(SecureFloat value)
    {
        var v = value.holder;
        if (bs.isDebug && (float.IsNaN(value.fake) || float.IsNaN(v)))
            Debug.LogError("Nan Deteced");
        if (v != value.fake && !bs._Loader.mh && !(float.IsNaN(value.fake) || float.IsNaN(v)))
            bs._Loader.StartCoroutine(bs._Loader.MhSend2("Value changed from " + v + " to " + value.fake)); 
        return v;
    }
    public override string ToString()
    {
        return holder.ToString();
    }
}