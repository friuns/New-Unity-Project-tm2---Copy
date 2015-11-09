#if oldStunts
using System.Globalization;
#if !UNITY_WP8
using CodeStage.AntiCheat.ObscuredTypes;
#else
using ObscuredInt = System.Int32;
using ObscuredFloat = System.Single;
#endif
#if !UNITY_FLASH || UNITY_EDITOR
using System.Linq;
#endif
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public partial class Player
{

    Vector3 oldRot;
    private void UpdateStunts()
    {

        //score = score;
        if (ghost) return;
        if (!online)
            score = Mathf.Lerp(score, 0, Time.deltaTime * 0.01f);
        var euler = transform.eulerAngles;
        var grounded = Time.time - groundedTime2 < .1f;
        //if (grounded)
        //{
        //    if (skid2 > 1 )
        //        tempScore += Time.deltaTime * 20;
        //    //tempScore += Math.Abs(Mathf.DeltaAngle(oldRot.x, euler.x)) + Mathf.Abs(Mathf.DeltaAngle(oldRot.z, euler.z));
        //    //print(Mathf.Abs(Mathf.DeltaAngle(oldRot.x, euler.x)) + Math.Abs(Mathf.DeltaAngle(oldRot.y, euler.y)) + Mathf.Abs(Mathf.DeltaAngle(oldRot.z, euler.z)));
        //    //print("local euler:"+transform.localEulerAngles);
        //    //print("euler:"+transform.eulerAngles);
        //}
        if (!grounded)
        {
            tempScore += Time.deltaTime * 50;
            tempScore += Mathf.Abs(Mathf.DeltaAngle(oldRot.x, euler.x)) + Math.Abs(Mathf.DeltaAngle(oldRot.y, euler.y)) + Mathf.Abs(Mathf.DeltaAngle(oldRot.z, euler.z)) / 30f;
        }
        //if (_Game.backTime)
            //oldTempScore=tempScore = 0;
        var changed = Math.Abs(oldTempScore - tempScore) > .01f;
        if (changed&&tempScore>100)
        {
            var t = ((int)tempScore).ToString();
            //_Loader.CenterText.text = t;
            //_Loader.CenterTextBackground.enabled = _Loader.CenterText.enabled = true;

            _Player.hud.PlayScore(t, Color.white);
        }
        if (grounded && tempScore > 100 && !changed)
        {
            //_Game.centerText("Got " + ((int) tempScore) + "!", .1f);
            //_Player.hud.PlayScore(((int)tempScore).ToString(), Color.blue);
            //score += tempScore;
            AddScore(tempScore,Color.blue);
            
        }
        //else if (!changed)
        //    _Player.hud.PlayScore(((int)tempScore).ToString(), Color.red);

        if (!changed && tempScore>0)
        {
            //print(Math.Abs(oldTempScore - tempScore));
            tempScore = 0;
        }
        //if (!online)
        //if (online)
        //{
        //    _Game.scoreText.enabled = true;
        //    _Game.scoreText.text = ((int)score).ToString();
        //}
        oldRot = euler;
        oldTempScore = tempScore;
    }
    float tempScore;
    float oldTempScore;

    public void AddScore(float i, Color c)
    {
        if (_Loader.stunts)
        {
            _Player.hud.PlayScore(((int)i).ToString(), c);
            _Player.score += i;
            _Player.CallRPC(SetScore2, (float)_Player.score);
        }
    }
}

#endif