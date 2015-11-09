
using System.Collections;
using UnityEngine;

public class FreeCamera:bs
{
    public Player pl;
    internal GUILayer guiLayer;
    
    private float smooth = 3;
    private float speed = 1;
    private bool oldFreeCamera;
    public void LateUpdate()
    {
        if (oldFreeCamera != pl.freeCamera && pl.freeCamera)
            ChangePl();
        oldFreeCamera = pl.freeCamera;
        if (oldFreeCamera)
        {
            //guiLayer.enabled = Time.time - changeTime < 2;
            //if (!Screen.lockCursor) return;

            Vector3 ms = getMouseDelta() * (_Loader.sensivity * _Loader.sensivity) * .3f;
            //if (!Screen.lockCursor)
            //    ms = Vector3.zero;
            camrot += new Vector3(-ms.y, ms.x * 2)*10;
            camrot.x = Mathf.Clamp(camrot.x, -70, 85);

            transform.localEulerAngles = camrot;
            if (Input.GetMouseButtonDown(0))
                ChangePl();
             
            if (obs != null)
            {
                Vector3 vector3 = obs.pos - transform.forward * speed * 20;
                bool b = Vector3.Distance(pos, vector3) > 10;
                if (b)
                {
                    transform.LookAt(obs.transform);
                    camrot = transform.localEulerAngles;
                }
                pos = Vector3.Lerp(pos, vector3, b ? Time.deltaTime * 5 : 1);
                RaycastHit h;
                if (Physics.Linecast(obs.pos,pos, out h, Layer.levelMask))
                    pos = h.point;

                //obs.pos-
            }
            else
            {
                Vector3 vector3 = new Vector3(Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0,
                    Input.GetKey(KeyCode.Space) ? 1 : Input.GetKey(KeyCode.X) ? -1 : 0,
                    Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0);
                vector3.y *= .5f;
                
                //LogRight("(hold shift)");
                //LogRight("smooth:" + smooth);
                //LogRight("speed:" + speed);
                v = Vector3.Lerp(v, rot * vector3, _Loader.deltaTime * smooth * smooth);
                transform.position += (v * speed);
            }
            if (Input.GetKey(KeyCode.LeftShift))
                smooth += Input.GetAxis("Mouse ScrollWheel");
            else
                speed += Input.GetAxis("Mouse ScrollWheel");
            speed = Mathf.Max(.1f, speed);
            smooth = Mathf.Max(.1f, smooth);
            if (Input.GetMouseButtonDown(1))
                obs = null;
        }
    }
    public float changeTime;
    public void ChangePl()
    {
        obs = _Game.listOfPlayers[i%_Game.listOfPlayers.Count];
        transform.position = pl.pos;
        _Loader.centerText("Spectating " + obs.playerNameClan,3,true);
        changeTime = Time.time;
        i++;
    }
    public int i;
    private Player obs;
    private Vector3 camrot;
    private Vector3 v;
    private Vector3 vel;
}
