using System;
using System.Linq;
using UnityEngine;

public class VoiceChatBase:MonoBehaviour
{
    float middle;
    public void NormalizeSample(float[] sample)
    {
        var max = Math.Max(middle, Mathf.Lerp(middle, sample.Max() * 10, .1f));
        if (max > middle) print("Set Max" + middle);
        middle = max;
        for (int i = 0; i < sample.Length; i++)
            sample[i] *= 20f / middle;
    }   
}