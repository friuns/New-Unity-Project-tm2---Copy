using System.Configuration;
using System.Globalization;
using System.Security.Cryptography;
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

public class Coin : bs
{
    bool gameLoaded;
    public void Update()
    {
        if (!gameLoaded && (_Loader.levelEditor || _Game == null || !_Player))
            return;
        gameLoaded = true;

        if ((_Player.pos - pos).magnitude < 6)
        {
            //_Player.score += 100;

            //_Player.CallRPC(_Player.SetScore2, _Player.score + 100);
            _Player.coins++;
            _Awards.coinsCollected.Add();
            gameObject.SetActive(false);
            _Player.audio.PlayOneShot(res.coinSound);
        }
    }
    public Animation componentInChildren;
    public void Start()
    {
        if (componentInChildren != null)
            foreach (AnimationState a in componentInChildren)
                a.normalizedTime = Random.value;
    }
    public void Reset()
    {
        
        gameObject.SetActive(true);
    }
}