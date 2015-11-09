using System;
using UnityEngine;

public class AntiSpeedHack : bs
{
    private DateTime olddt;
    private long oldTick;
    private int errorCount;


    public void Start()
    {
        if (offlineMode)
        {
            Destroy(gameObject);
            return;
        }
        olddt = DateTime.Now;
        oldTick = Environment.TickCount;
        InvokeRepeating("invSH", 5, 2);
    }

    public void invSH()
    {
        TimeSpan span = DateTime.Now - olddt;
        olddt = DateTime.Now;

        long dTick = Environment.TickCount - oldTick;
        oldTick = Environment.TickCount;

        if (span.TotalMilliseconds * 1.3f < dTick)
        {
            errorCount++;
        }

        if (errorCount > 5)
        {
            errorCount = 0;
            Debug.LogWarning("SH detected");
            if (PhotonNetwork.room != null)
                PhotonNetwork.LeaveRoom();
        }
    }
}