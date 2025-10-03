using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private Sound[] sounds;
    private Dictionary<string, Sound> soundDictionary;
    [SerializeField] private AudioMixer audioMixer;

    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        soundDictionary = new Dictionary<string, Sound>();
        
        foreach(Sound sound in sounds)
        {
            if (sound.clip == null) return;

            sound.audioSource = gameObject.AddComponent<AudioSource>();
            sound.audioSource.clip = sound.clip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.pitch;
            sound.audioSource.loop = sound.loop;
            sound.audioSource.outputAudioMixerGroup = sound.audioMixer;

            soundDictionary[sound.clip.name] = sound;
        }
    }

    public void PlaySound(string clipName)
    {
        if (soundDictionary.ContainsKey(clipName))
            soundDictionary[clipName].audioSource.Play();
    }
    public void StopSound(string clipName)
    {
        if (soundDictionary.ContainsKey(clipName))
            soundDictionary[clipName].audioSource.Stop();
    }
    public void StopAllSounds()
    {
        foreach (var sound in soundDictionary.Values)
        {
            if (sound.audioSource.isPlaying)
                sound.audioSource.Stop();
        }
    }
    public void SetBgmVol(float vol)
    {
        if (vol <= 0.0001f) vol = 0.0001f; 
        audioMixer.SetFloat("bgmVol", Mathf.Log10(vol) * 20);
    }

    public void SetSFXVol(float vol)
    {
        if (vol <= 0.0001f) vol = 0.0001f;
        audioMixer.SetFloat("sfxVol", Mathf.Log10(vol) * 20);
    }

    public float GetBgmVol()
    {
        audioMixer.GetFloat("bgmVol", out float value);
        return Mathf.Pow(10, value / 20f); 
    }

    public float GetSFXVol()
    {
        audioMixer.GetFloat("sfxVol", out float value);
        return Mathf.Pow(10, value / 20f);
    }
}

[System.Serializable]
public class Sound
{
    public AudioClip clip;
    [Range(0, 1f)] public float volume = 1f;
    [Range(0, 3f)] public float pitch = 1f;
    public bool loop = false;

    public AudioMixerGroup audioMixer;

    [HideInInspector] public AudioSource audioSource;
}
