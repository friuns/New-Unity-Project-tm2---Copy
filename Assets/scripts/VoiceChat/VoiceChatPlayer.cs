using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VoiceChatPlayer : VoiceChatBase
{
#if VOICECHAT
    float lastTime = 0;
    double played = 0;
    double received = 0;
    int index = 0;
    float[] data;
    float playDelay = 0;
    bool shouldPlay = false;
    float lastRecvTime = 0;
    NSpeex.SpeexDecoder speexDec = new NSpeex.SpeexDecoder(NSpeex.BandMode.Narrow);

    [SerializeField]
    int playbackDelay = 2;

    public float LastRecvTime
    {
        get { return lastRecvTime; }
    }

    public new AudioSource audio;
    void Start()
    {
        if (bs.offlineMode)
        {
            enabled = false; 
            return;
        }
        int size = VoiceChatSettings.Instance.Frequency * 10;

        audio = gameObject.AddComponent<AudioSource>();
        audio.ignoreListenerVolume = true;
        audio.volume = 3;
        audio.bypassEffects = audio.bypassReverbZones = audio.bypassListenerEffects = true;
        audio.loop = true;
        audio.clip = AudioClip.Create("VoiceChat", size, 1, VoiceChatSettings.Instance.Frequency, false, false);
        audio.priority = 0;
        audio.enabled = false;
        data = new float[size];

        if (VoiceChatSettings.Instance.LocalDebug)
        {
            VoiceChatRecorder.Instance.NewSample += OnNewSample;
        }
    }
    
    void Update()
    {
        if (audio.isPlaying)
        {
            // Wrapped around
            if (lastTime > audio.time)
            {
                played += audio.clip.length;
            }

            lastTime = audio.time;

            // Check if we've played to far
            if (played + audio.time >= received)
            {
                Stop();
                shouldPlay = false;
            }
        }
        else
        {
            if (shouldPlay)
            {

                playDelay -= bs._Loader.deltaTime;

                if (playDelay <= 0)
                {
                    audio.Play();
                }
            }
        }
    }

    void Stop()
    {
        audio.Stop();
        audio.time = 0;
        index = 0;
        played = 0;
        received = 0;
        lastTime = 0;
    }
    public void OnNewSample(VoiceChatPacket packet)
    {
        // Store last packet

        // Set last time we got something
        lastRecvTime = Time.realtimeSinceStartup;

        // Decompress
        float[] sample = null;
        int length = VoiceChatUtils.Decompress(speexDec, packet, out sample);
        
        //NormalizeSample(sample);

        // Add more time to received
        received += VoiceChatSettings.Instance.SampleTime;

        // Push data to buffer
        Array.Copy(sample, 0, data, index, length);

        // Increase index
        index += length;

        // Handle wrap-around
        if (index >= audio.clip.samples)
        {
            index = 0;
        }

        // Set data
        audio.enabled = true;
        audio.clip.SetData(data, 0);

        // If we're not playing
        if (!audio.isPlaying)
        {
            // Set that we should be playing
            shouldPlay = true;

            // And if we have no delay set, set it.
            if (playDelay <= 0)
            {
                playDelay = (float)VoiceChatSettings.Instance.SampleTime * playbackDelay;
            }
        }

        VoiceChatFloatPool.Instance.Return(sample);
    }

    
#endif
}
