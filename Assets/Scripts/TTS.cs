using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TTS : MonoBehaviour
{
    private const string keyAPI = ""; // Please enter API-key

    [SerializeField] private AudioSource _audioSource;

    private string _text;

    public void Speak(string text)
    {
        _text = text;
        StartCoroutine(SendRequest());
    }

    private IEnumerator SendRequest()
    {
        var form = new WWWForm();
        form.AddField("text", _text);
        form.AddField("lang", "ru-RU");
        form.AddField("format", "lpcm");
        form.AddField("sampleRateHertz", "48000");

        var request = UnityWebRequest.Post("https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize", form);

        request.SetRequestHeader("Authorization", "Api-Key " + keyAPI);

        yield return request.SendWebRequest();

        var data = request.downloadHandler.data;
        CreateAudioClip(data);

    }

    private void CreateAudioClip(byte[] audioData)
    {
        var audioFloat = ConvertBytes2Float(audioData);

        _audioSource.clip = AudioClip.Create("Speaker", audioData.Length / 2, 1, 48000, false);
        _audioSource.clip.SetData(audioFloat, 0);
        _audioSource.Play();
    }

    private float[] ConvertBytes2Float(byte[] bytes)
    {
        var max = -(float)System.Int16.MinValue;
        var samples = new float[bytes.Length / 2];

        for (int i = 0; i < samples.Length; i++)
        {
            var int16sample = System.BitConverter.ToInt16(bytes, i * 2);
            samples[i] = (float)int16sample / max;
        }

        return samples;
    }
}
