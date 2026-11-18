using UnityEngine;

public static class PickupSoundGenerator
{
    public static AudioClip Create(string clipName = "PickupChime")
    {
        const int sampleRate = 44100;
        const float duration = 0.22f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        float[] notes = { 880f, 1174.66f, 1567.98f };
        float[] starts = { 0f, 0.045f, 0.09f };
        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            float value = 0f;
            for (int note = 0; note < notes.Length; note++)
            {
                float localTime = time - starts[note];
                if (localTime < 0f) continue;
                float envelope = Mathf.Exp(-localTime * 18f) * Mathf.Min(1f, localTime * 180f);
                value += Mathf.Sin(2f * Mathf.PI * notes[note] * localTime) * envelope / notes.Length;
            }
            samples[i] = value * 0.65f;
        }
        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
