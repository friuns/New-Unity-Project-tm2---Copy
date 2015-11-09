using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
public class Hud : GuiClasses
{
    public GameObject root; 
    public TextMesh time;
    public TextMesh distance;
    public TextMesh speed;
    public TextMesh checkPoint;
    public TextMesh checkPointRed;
    public TextMesh backup;
    public Renderer backupIcon;
    public Player pl;
    public Transform damage;
    public Animation damageAnim;
    public TextMesh zombieScore;
    public override void Awake()
    {
        base.Awake();
    }
    public void Start()
    {
        checkPointRed.text = checkPoint.text = "";
        //if (_Loader.dm)
        
    }
    public void Update()
    {
        backupIcon.renderer.enabled = backup.renderer.enabled = !lowestQuality;

        if (pl == null) return;
        if (!pl.finnished)
        {

            if (_Loader.pursuit)
                time.color = !_Player.cop && _MpGame.blueTeam.players.Any(a => a.Seeing(_Player)) ? Color.red : Color.white;

            if (_Loader.enableMatchTime)
                time.text = TimeToStr(Mathf.Max(0, _MpGame.timeCountMatch), false, false, false);//((int)pl.rigidbody.velocity.magnitude).ToString() : 
            else
                time.text = TimeToStr(_Game.started ? _Game.timeElapsed : 0);//((int)pl.rigidbody.velocity.magnitude).ToString() : 
            speed.text = ((int)(pl.rigidbody.velocity.magnitude * 3.6f)).ToString();
            distance.text = (int)pl.totalMeters + "m";
        }
    }
    public void PlayScore(string s,Color c)
    {
        //var zombieScore = _Player.hud.zombieScore;
        zombieScore.text = s;
        zombieScore.renderer.material.color = c;
        zombieScore.animation.Rewind();
        zombieScore.animation.Play();
    }
}
