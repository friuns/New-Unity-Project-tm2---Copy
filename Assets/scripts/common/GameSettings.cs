#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class GameSettings:MonoBehaviour
{
    public float miny = -200;
    public bool InitColliders;
    public float PlayerFriq = 1;
    public float Rotation = 1;
    public float Brake=1;
    public bool ownGravity = true;
    public float speed = 1;
    public bool enableBorderHit=true;
    public float gravitationAntiFly=1;
    public float gravitationFactor = 1;
    public float sendWmpOffset = 0;
    public int laps;
    public void Start()
    {
        if (gravitationFactor == 0)
            gravitationFactor = 1;
        if (gravitationAntiFly == 0)
            gravitationAntiFly = 1;
        if (speed == 0)
            speed = 1;
        if (gravitationAntiFly == 1)
            gravitationAntiFly = 1.5f;
        
    }
//#if UNITY_EDITOR
//    void OnApplicationQuit()
//    {
//        if (bs.resEditor.enableLevelEdit)
//            EditorApplication.delayCall += Delaycall;
//    }

//    private void Delaycall()
//    {
//        EditorApplication.delayCall -= Delaycall;
//        if (this == null) return;
//        var instanceIdToObject = EditorUtility.InstanceIDToObject(this.GetInstanceID());
//        GameSettings a = (GameSettings)instanceIdToObject;
//        Undo.RegisterUndo(a, "rtools");
//        a.sendWmpOffset = sendWmpOffset;
//        a.drag = drag;
//        a.speed = speed;
//        //a.PlayerFriq = PlayerFriq;
//        a.Rotation = Rotation;
//        a.enableBorderHit = enableBorderHit;
//        a.laps = laps;
//        a.disableFlyDir = disableFlyDir;
//        EditorUtility.SetDirty(a);
//    }
//#endif   
    public bool disableFlyDir;
    public float drag = .06f;
    internal Bounds levelBounds;
    internal float levelTime = 180;
    //public bool stuntMap;
}