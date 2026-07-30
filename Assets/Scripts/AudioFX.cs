using UnityEngine;

/* ================= 程序化环境音（雨声白噪 + 低频嗡鸣，零音频资产）
   注意：OnAudioFilterRead 在音频线程执行，不使用 UnityEngine.Random / Time ================= */
[RequireComponent(typeof(AudioListener))]
public class AudioFX : MonoBehaviour
{
    public static AudioFX I;
    volatile bool muted;
    volatile bool humTrigger;
    uint noiseState = 123456789u;
    float lp, lp2;            // 雨声滤波状态
    float humPhase, humT = -1f; // 嗡鸣相位与包络时间（秒）
    const float HumFreq = 52f;

    void Awake()
    {
        I = this;
        if (GetComponent<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
    }

    public void ToggleMute() => muted = !muted;
    public void Hum() { humT = 0f; }

    float NextNoise()
    {
        noiseState = noiseState * 1664525u + 1013904223u;
        return ((noiseState >> 8) & 0xFFFF) / 32767.5f - 1f;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (muted) { humT = -1f; return; }
        float sr = 1f / AudioSettings.outputSampleRate;
        for (int i = 0; i < data.Length; i += channels)
        {
            // 雨声：白噪 → 两级一阶低通
            lp += 0.09f * (NextNoise() - lp);
            lp2 += 0.35f * (lp - lp2);
            float sample = lp2 * 0.05f;
            // 低频嗡鸣：52Hz 正弦，包络 0.4s 起 / 2.4s 落
            if (humT >= 0f)
            {
                float env = humT < 0.4f ? humT / 0.4f : Mathf.Max(0f, 1f - (humT - 0.4f) / 2.4f);
                sample += Mathf.Sin(humPhase * Mathf.PI * 2f) * env * 0.14f;
                humPhase += HumFreq * sr;
                if (humPhase > 1f) humPhase -= 1f;
                humT += sr;
                if (humT > 2.8f) humT = -1f;
            }
            for (int c = 0; c < channels; c++) data[i + c] += sample;
        }
    }
}
