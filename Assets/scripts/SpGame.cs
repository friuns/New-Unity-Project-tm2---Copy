
using System.Collections;
using UnityEngine;

public partial class Game 
{

    internal float spStartTime;
    internal float timeElapsedLevel;

    public void RestartTime()
    {
        if (online)
        {
            if (_MpGame.timeCountRace < 0)
                _MpGame.CallRPC(_MpGame.SetTimeCountRace, (float)_Loader.waitTime);
        }
        else
        {
            _Game.audio.volume = .2f;
            if (setting.DontWait)
            {
                SetStartTrue();
                timeElapsedLevel = 4;
                spStartTime = 4;
            }
            else
                StartCoroutine(CountTo3());
        }
    }

    private IEnumerator CountTo3()
    {
        yield return new WaitForSeconds(1);
        PlayOneShotGui(bs.res.bip);
        SetCenterTexture(bs.res.go123[3]);
        yield return new WaitForSeconds(1);
        PlayOneShotGui(bs.res.bip);
        SetCenterTexture(bs.res.go123[2]);
        yield return new WaitForSeconds(1);
        PlayOneShotGui(bs.res.bip);
        SetCenterTexture(bs.res.go123[1]);
        yield return new WaitForSeconds(1);
        SetStartTrue();
    }
    private static bool sendedWmp;
    

    public virtual void SetStartTrue()
    {
        print("Start!!");
        spStartTime = timeElapsedLevel;
        _Game.SendWmp("goto", _GameSettings.sendWmpOffset + "");        
        SetCenterTexture(bs.res.go123[0]);
        StartCoroutine(AddMethod(1, delegate { _Game.textureCenter.enabled = false; }));
        PlayOneShotGui(bs.res.start);
        gameState = GameState.started;
    }
    protected void SetCenterTexture(Texture2D texture2D)
    {
        var textureCenter = _Game.textureCenter;
        textureCenter.texture = texture2D;
        var w = textureCenter.texture.width;
        var h = textureCenter.texture.height;
        textureCenter.pixelInset = new Rect(-w / 2f, -h / 2f, w, h);
        textureCenter.enabled = true;
    }   
}