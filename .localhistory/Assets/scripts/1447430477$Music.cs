using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Music:bs
{
    public List<WWW> aus = new List<WWW>();
    private WWW w;
    public IEnumerator Start()
    {
        _music = this;
        enabled = false;
        if (android) yield break;
        audio.priority = 0;
        audio.ignoreListenerVolume = true;
        audio.loop = true;
        if (_Loader.musicVolume == 0 || Application.isEditor) yield break;
        while (assetBundle.Count < 2)
            yield return null;
        w = new WWW(mainSite + "scripts/music.php");
        yield return w;
        if(!string.IsNullOrEmpty(w.error))yield break;
        var splitString = SplitString(w.text);
        foreach (string a in splitString.OrderBy(a => Random.value))
        {
            if (_Loader.musicVolume == 0) yield break;
            w = new WWW(mainSite + Uri.EscapeUriString(a));
            
            yield return w;
            print("loading "+w.url +"\n"+w.error);
            
            if(string.IsNullOrEmpty(w.error))
                aus.Add(w);
            if (!audio.isPlaying)
                PlayRandom();
        }
        //PlayRandom();
        
        //www = new WWW("http://ec-media.soundcloud.com/OazcRmCFM8vu?ff61182e3c2ecefa438cd0210ad0e38569b9775ddc9e06b3c362a4853c955ce470ed17f12749aaec389235f59746f98e45c8eac0b9399f4aea41d5d3a6&AWSAccessKeyId=AKIAJ4IAZE5EOI7PA7VQ&Expires=1371465674&Signature=OIut%2BOW2FGd5llv7zHRRwwxelJ4%3D");
        //yield return www;
        //print("Music Loaded");
        //_Loader.audio.clip = www.GetAudioClip(false, true, AudioType.MPEG);

    }
    public void Update()
    {
        if(w!=null)
        LogRight(w.progress);
        if (KeyDebug(KeyCode.E))
            PlayRandom();
        if (audio.isPlaying)
            audio.volume = _Loader.musicVolume;
        //else
        //    PlayRandom();
        
        //Log("music is playing " + _Loader.audio.isPlaying);
        //Log("music loaded " + www.isDone);
        //if (!_Loader.audio.isPlaying)
        //    _Loader.audio.Play();
    }
    
    private int id;
    public void PlayRandom()
    {
        if (canPlay)
        {
            audio.clip = aus[id % aus.Count].GetAudioClip(false,true);
            audio.Play();
            id++;
        }
    }
    private bool canPlay
    {
        get { return aus.Count > 0 && _Loader.musicVolume != 0; }
    }
}