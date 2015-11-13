using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Music : bs
{

    private new AudioSource audio;

    public override void Awake()
    {
        _music = this;
        audio = GetComponent<AudioSource>();
    }
    public void Update()
    {
        if (audio.isPlaying)
            audio.volume = _Loader.musicVolume;

    }

    public static string music = "http://tmrace.net/cops/tm.mp3";
    public void LoadMusic(string url, bool broadcast = false)
    {
        StartCoroutine(StartLoadMusic(url, broadcast));
    }
    private IEnumerator StartLoadMusic(string url, bool broadcast = false)
    {
        var s = http + "://tmrace.net/cops/music.php?url=" + WWW.EscapeURL(url);
        Debug.LogWarning(s);
        var w = new WWW(s);
        yield return w;
        print(w.text);
        w = new WWW(w.text);
        yield return w;
        var audioClip = w.GetAudioClip(false, true, AudioType.OGGVORBIS);
        if (audioClip.length == 0)
        {
            Debug.LogError(w.error);
            yield break;
        }
        if (broadcast && _Game && audioClip)
        {
            broadCastTime = Time.time;
            _GameGui.CallRPC(_GameGui.Chat, _Loader.playerName + " Set music to " + w.text);
            _GameGui.CallRPCTo(_MpGame.LoadMusic, PhotonTargets.Others, w.text);
        }

        audio.clip = audioClip;
        audio.Play();
    }
    public static float broadCastTime = -10;


}