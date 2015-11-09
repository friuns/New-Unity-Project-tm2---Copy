#define GA

using System.Linq;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Security.Policy;
using System.Threading;
//using Edelweiss.DecalSystem;
//using Vectrosity;
using UnityEngine.Rendering;
#if !UNITY_WP8
using CodeStage.AntiCheat.ObscuredTypes;
#endif
using UnityEngine.SocialPlatforms.Impl;
using gui = UnityEngine.GUILayout;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using System.Net.Sockets;
using UnityEditor;
using Object = UnityEngine.Object;
//using Exception = System.AccessViolationException;

#endif

public class Pursuit:bsNetwork
{
    public override void OnPlConnected()
    {
        base.OnPlConnected();
    }
}