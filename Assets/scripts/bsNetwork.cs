using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using Debug = UnityEngine.Debug;
//using Exception = System.AccessViolationException;
using TargetParameterCountException = System.AppDomainUnloadedException;
using Curvy.Utils;
#endif
public delegate void Action<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);

public class bsNetwork : GuiClasses
{

    public void CallRPCTo<T>(Action<T> n, PhotonTargets target, T p)
    {
        photonView.RPC(n.Method.Name, target, p);
    }

    public static bool? m_IsMaster;

    public static bool isMaster
    {
        get
        {
            return PhotonNetwork.isMasterClient;
            //if (!m_IsMaster.HasValue)
            //    m_IsMaster = PhotonNetwork.isMasterClient;
            //return m_IsMaster.Value;
        }
    }

    public void CallRPC(Action<PhotonMessageInfo> n)
    {
        CallRPC(n.Method.Name);
    }

    public void CallRPC<T>(Action<T, PhotonMessageInfo> n, T p)
    {
        CallRPC(n.Method.Name, p);
    }
    public void CallRPC<T, T2>(Action<T, T2, PhotonMessageInfo> n, T p, T2 p2)
    {
        CallRPC(n.Method.Name, p, p2);
    }

    public void CallRPC(Action n)
    {
        CallRPC(n.Method.Name);
    }
    public void CallRPC<T>(Action<T> n, T p)
    {
        CallRPC(n.Method.Name, p);
    }
    public void CallRPC<T, T2>(Action<T, T2> n, T p, T2 p2)
    {
        CallRPC(n.Method.Name, p, p2);
    }
    public void CallRPC<T, T2, T3>(Action<T, T2, T3> n, T p, T2 p2, T3 p3)
    {
        CallRPC(n.Method.Name, p, p2, p3);
    }
    public void CallRPC<T, T2, T3, T4>(Action<T, T2, T3, T4> n, T p, T2 p2, T3 p3, T4 p4)
    {
        CallRPC(n.Method.Name, p, p2, p3, p4);
    }
    public void CallRPC<T, T2, T3, T4, T5>(Action<T, T2, T3, T4, T5> n, T p, T2 p2, T3 p3, T4 p4, T5 p5)
    {
        CallRPC(n.Method.Name, p, p2, p3, p4, p5);
    }
    public void CallRPC<T, T2, T3, T4, T5, T6>(Action<T, T2, T3, T4, T5, T6> n, T p, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
    {
        CallRPC(n.Method.Name, p, p2, p3, p4, p5, p6);
    }
    public void CallRPCTo(Action n)
    {
        CallRPCTo(n.Method.Name);
    }
    public void CallRPCTo<T>(Action<T> n, T p)
    {
        CallRPCTo(n.Method.Name, p);
    }
    public void CallRPCTo<T, T2>(Action<T, T2> n, T p, T2 p2)
    {
        CallRPCTo(n.Method.Name, p, p2);
    }
    public void CallRPCTo<T, T2, T3>(Action<T, T2, T3> n, T p, T2 p2, T3 p3)
    {
        CallRPCTo(n.Method.Name, p, p2, p3);
    }
    public void CallRPCTo<T, T2, T3, T4>(Action<T, T2, T3, T4> n, T p, T2 p2, T3 p3, T4 p4)
    {
        CallRPCTo(n.Method.Name, p, p2, p3, p4);
    }
    public void CallRPCTo<T, T2, T3, T4, T5>(Action<T, T2, T3, T4, T5> n, T p, T2 p2, T3 p3, T4 p4, T5 p5)
    {
        CallRPCTo(n.Method.Name, p, p2, p3, p4, p5);
    }
    public void CallRPCTo<T, T2, T3, T4, T5, T6>(Action<T, T2, T3, T4, T5, T6> n, T p, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
    {
        CallRPCTo(n.Method.Name, p, p2, p3, p4, p5, p6);
    }
    private void CallRPC(string mn, params object[] p)
    {
        try
        {
            if (offlineMode)
            {
                MethodInfo methodInfo = GetType().GetMethod(mn, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (p.Length < methodInfo.GetParameters().Length)
                    p = p.Concat(new[] { new PhotonMessageInfo() }).ToArray();
                methodInfo.Invoke(this, p);
            }
            else 
                if (ToPhotonPlayer != null)
            {
                print(LogLevel.temp, mn + " Sending to " + ToPhotonPlayer.ID);
                photonView.RPC(mn, ToPhotonPlayer, p);
            }
            else
                photonView.RPC(mn, PhotonTargets.All, p);

        } 
        catch (TargetParameterCountException) { }
#if !UNITY_EDITOR
        catch (Exception e)
        {
            Debug.LogError(e);
        } 
#endif
    }
    private PhotonPlayer ToPhotonPlayer;
    public void CallRPCTo(string mn, params object[] p)
    {
        photonView.RPC(mn, ToPhotonPlayer, p);
    }
    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        ToPhotonPlayer = player;
        if (PhotonNetwork.isMasterClient)
            try
            {
                OnPlConnected();
            } catch (Exception e)
            {
                Debug.LogError(e);
            }
        ToPhotonPlayer = null;
    }
    public virtual void OnPlConnected()
    {

    }
}