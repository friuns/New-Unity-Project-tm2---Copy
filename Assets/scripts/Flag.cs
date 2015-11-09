using UnityEngine;
using gui = UnityEngine.GUILayout;


public class Flag : bsNetwork
{
    internal Vector3 StartPos;
    public TeamEnum teamEnum;
    public Player pl;
    
    public Flag otherFlag;// { get { return FindObjectsOfType(typeof(Flag)).Cast<Flag>().FirstOrDefault(a => a != this); } }
    //public Texture2D[] textures;
    bool captured { get { return pl != null; } }
    //public int FlagScore = 10;
    
    public void Start()
    {
        transform.position = (teamEnum == TeamEnum.Red ? _Game.redFlagPos : _Game.blueFlagPos).position + _Game.startPosOffset;
        flagIcon.transform.parent = null;
        StartPos = transform.position;
        _MpGame.flags.Add(this);
    }

    public float flagUsedTime;
    bool flagHome;
    public void Update()
    {
        //if (otherFlag != null)
        //{
            //if (rigidbody != null)
        //rigidbody.isKinematic = pl != null;

        

        

        if (PhotonNetwork.isMasterClient)
        {
            var mindist = 10;
            
            flagHome = (StartPos - pos).magnitude < mindist;
            if (captured || flagHome)
                flagUsedTime = Time.time;
            if (Time.time - flagUsedTime > 15)
            {
                flagUsedTime = 99999;
                CallRPC(ResetPos, true);
            }

            if (pl != null && pl.dead)
                CallRPC(DropFlag, pl.FlagPlaceHolder.position);
            if (pl != null && pl.teamEnum == teamEnum) // on team change
                CallRPC(ResetPos, true);

            foreach (var a in _Game.listOfPlayers)
            {
                if (((a.pos - pos).magnitude < mindist || KeyDebug(KeyCode.X)) && !captured && !a.dead && Time.time - a.resetTime > 3)
                {
                    if (a.teamEnum == teamEnum && !flagHome)
                        CallRPC(ResetPos, true);

                    if (a.teamEnum != teamEnum)
                        CallRPC(SetOwner, a.playerId);
                    break;
                    //photonView.RPC("ResetPos", PhotonTargets.All);

                    //photonView.RPC("SetOwner", PhotonTargets.All, _Player.photonView.viewID);
                }
            }

            if ((pos - otherFlag.StartPos).magnitude < mindist && !otherFlag.captured && otherFlag.flagHome && pl != null && !pl.dead && Time.time - pl.resetTime > 3)
            {
                print("score");
                var p = pl;
                pl = null;
                var c = _Game.listOfPlayers.Count ;
                p.CallRPC(p.SetScore2, p.score + (c > 7 ? 60 : c > 3 ? 30 : 10));
                _MpGame.CallRPC(_MpGame.FlagCaptured, p.playerId);
            }

        }

        if (pl != null)
        {
            transform.position = pl.FlagPlaceHolder.position;
            transform.rotation = pl.FlagPlaceHolder.rotation;
        }



        flagIcon.transform.position = _Player.camera.WorldToViewportPoint(pos + Vector3.up) + Vector3.up * .05f - Vector3.forward * 10;
        flagIcon.enabled = flagIcon.transform.position.z > 0;

        flagIcon.color = (teamEnum == TeamEnum.Blue ? Color.blue : Color.red) - new Color(0, 0, 0, Mathf.Min(.9f, (_Player.pos - pos).magnitude / 400f));
        //if (flagIcon.enabled)
        //}
    }
    public GUITexture flagIcon;
    //[RPC]
    //public void Setup(Vector3 p, Team team)
    //{
    //    print("Flag Setup");
    //    StartPos = transform.position = p;
    //    this.team = team;
    //    GetComponentInChildren<Renderer>().material.mainTexture = textures[this.team ? 1 : 0];
    //}
    private bool told;
    [RPC]
    public void ResetPos(bool play)
    {
        told = false;
        print("ResetPos" + pos + "  " + StartPos);
        if (play)
            PlayOneShotGui(teamEnum == TeamEnum.Blue ? res.blueFlagReturn : res.redFlagReturn);
        pl = null;
        pos = StartPos;
        rot = Quaternion.identity;
    }
    [RPC]
    public void DropFlag(Vector3 pos)
    {
        print("Drop Flag");
        transform.position = pos;
        
        pl = null;
    }

    public override void OnPlConnected()
    {
        if (pl != null)
            CallRPC(SetOwner, pl.playerId);
        base.OnPlConnected();
    }

    

    [RPC]
    public void SetOwner(int id)
    {
        print("Set FlagOwner " + id + " ph" + _Game.photonPlayers.Count);
        pl = _Game.photonPlayers[id];
        if (!told)
        {
            PlayOneShotGui(pl == _Player ? res.youHaveFlag : pl.sameTeam ? res.yourTeamHaveFlag : res.enemyHaveYourFlag);
            _Game.centerText(pl == _Player ? "Your have a flag" : pl.sameTeam ? "Your team have flag" : "Enemy have your flag");
        }
        told = true;
    }







}













