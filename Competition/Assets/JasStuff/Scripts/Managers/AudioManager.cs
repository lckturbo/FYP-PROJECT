using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private Sound[] sounds;
    private Dictionary<string, Sound> soundDictionary;
    [SerializeField] private AudioMixer audioMixer;

    private Dictionary<string, float> savedMusicTime = new Dictionary<string, float>();
    private string currentMusic = "";

    private Dictionary<GameObject, AudioSource> loopingSFX = new();


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

    public void PlayMusic(string clipName)
    {
        if (!soundDictionary.ContainsKey(clipName))
        {
            Debug.LogWarning($"Music '{clipName}' not found.");
            return;
        }

        Sound s = soundDictionary[clipName];

        // Save which music is playing
        currentMusic = clipName;

        // Restore saved time if exists
        if (savedMusicTime.TryGetValue(clipName, out float time))
            s.audioSource.time = time;
        else
            s.audioSource.time = 0f;

        s.audioSource.Play();
    }

    public void PauseCurrentMusic()
    {
        if (string.IsNullOrEmpty(currentMusic)) return;

        Sound s = soundDictionary[currentMusic];
        if (s.audioSource.isPlaying)
        {
            savedMusicTime[currentMusic] = s.audioSource.time;
            s.audioSource.Pause();
        }
    }
    public void PauseAllMusic()
    {
        foreach (var pair in soundDictionary)
        {
            Sound s = pair.Value;
            if (s.audioSource.isPlaying)
            {
                savedMusicTime[pair.Key] = s.audioSource.time;
                s.audioSource.Pause();
            }
        }
    }

    public void PlaySFXAtPoint(string clipName, Vector3 position, float volume = 1f)
    {
        if (!soundDictionary.ContainsKey(clipName))
        {
            Debug.LogWarning($"SFX '{clipName}' not found.");
            return;
        }

        Sound sfx = soundDictionary[clipName];

        // Create a temporary AudioSource at the position
        GameObject tempObj = new GameObject($"SFX_{clipName}");
        tempObj.transform.position = position;

        AudioSource tempSource = tempObj.AddComponent<AudioSource>();
        tempSource.clip = sfx.clip;
        tempSource.volume = sfx.volume * volume;
        tempSource.pitch = sfx.pitch;
        tempSource.spatialBlend = 1f;               // <-- Enables 3D sound
        tempSource.outputAudioMixerGroup = sfx.audioMixer;

        tempSource.Play();

        // Destroy object after the sound finishes
        Destroy(tempObj, sfx.clip.length / tempSource.pitch);
    }

    public void PlayLoopingSFXAtObject(string clipName, GameObject target, float volume = 1f)
    {
        if (!soundDictionary.ContainsKey(clipName))
        {
            Debug.LogWarning($"Looping SFX '{clipName}' not found.");
            return;
        }

        if (loopingSFX.ContainsKey(target))
            return; // already playing

        Sound sfx = soundDictionary[clipName];

        AudioSource src = target.AddComponent<AudioSource>();
        src.clip = sfx.clip;
        src.volume = sfx.volume * volume;
        src.pitch = sfx.pitch;
        src.loop = true;
        src.spatialBlend = 1f;
        src.outputAudioMixerGroup = sfx.audioMixer;

        src.Play();
        loopingSFX[target] = src;
    }

    public void StopLoopingSFX(GameObject target)
    {
        if (!loopingSFX.ContainsKey(target))
            return;

        AudioSource src = loopingSFX[target];
        if (src != null)
        {
            src.Stop();
            Destroy(src);
        }

        loopingSFX.Remove(target);
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
    public void SetMasterVol(float vol)
    {
        if (vol <= 0.0001f) vol = 0.0001f;
        audioMixer.SetFloat("masterVol", Mathf.Log10(vol) * 20);
    }

    public float GetMasterVol()
    {
        audioMixer.GetFloat("masterVol", out float value);
        return Mathf.Pow(10, value / 20f);
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
