using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private Sound[] sounds;
    [SerializeField] private AudioMixer audioMixer;

    private Dictionary<string, Sound> soundDictionary;
    private Dictionary<string, float> savedMusicTime = new();
    private Dictionary<GameObject, AudioSource> loopingSFX = new();
    private Dictionary<string, float> activeSFX = new();

    private AudioSource musicSource;
    private AudioSource src;
    private string currentMusic = "";

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        //DontDestroyOnLoad(gameObject);

        soundDictionary = new Dictionary<string, Sound>();
        foreach (Sound s in sounds)
        {
            if (s.clip != null && !soundDictionary.ContainsKey(s.clip.name))
                soundDictionary.Add(s.clip.name, s);
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    public void PlayMusic(string clipName)
    {
        if (!soundDictionary.TryGetValue(clipName, out Sound sound))
        {
            Debug.LogWarning($"Music '{clipName}' not found.");
            return;
        }

        currentMusic = clipName;

        musicSource.clip = sound.clip;
        musicSource.volume = sound.volume;
        musicSource.pitch = sound.pitch;
        musicSource.loop = true;
        musicSource.outputAudioMixerGroup = sound.audioMixer;

        if (savedMusicTime.TryGetValue(clipName, out float savedTime))
            musicSource.time = savedTime;
        else
            musicSource.time = 0f;

        musicSource.Play();
    }

    public void PauseCurrentMusic()
    {
        if (string.IsNullOrEmpty(currentMusic)) return;
        if (!musicSource.isPlaying) return;

        savedMusicTime[currentMusic] = musicSource.time;
        musicSource.Pause();
    }

    public void PauseAllMusic()
    {
        PauseCurrentMusic();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFXAtPoint(string clipName, Vector3 position, float volume = 1f)
    {
        if (!soundDictionary.TryGetValue(clipName, out Sound sound))
        {
            Debug.LogWarning($"SFX '{clipName}' not found.");
            return;
        }

        if (activeSFX.TryGetValue(clipName, out float endTime))
        {
            if (Time.unscaledTime < endTime)
                return;
            else
                activeSFX.Remove(clipName);
        }

        src = AudioPool.instance.GetSource();

        src.transform.position = position;
        src.clip = sound.clip;
        src.volume = Mathf.Pow(10, sound.volume / 20f) * volume;
        src.pitch = sound.pitch;
        src.loop = false;
        src.spatialBlend = 0f;
        src.outputAudioMixerGroup = sound.audioMixer;

        src.Play();

        float length = sound.clip.length / Mathf.Abs(sound.pitch);
        activeSFX[clipName] = Time.unscaledTime + length;

        StartCoroutine(ReturnToPoolAfter(src, sound.clip.length / Mathf.Abs(sound.pitch)));
    }
    public void StopSFX()
    {
        src.Stop();
    }

    private IEnumerator ReturnToPoolAfter(AudioSource src, float time)
    {
        yield return new WaitForSeconds(time);
        AudioPool.instance.ReturnSource(src);
    }


    public void PlayLoopingSFXAtObject(string clipName, GameObject target, float volume = 1f)
    {
        if (!soundDictionary.TryGetValue(clipName, out Sound sound))
        {
            Debug.LogWarning($"Looping SFX '{clipName}' not found.");
            return;
        }

        if (loopingSFX.ContainsKey(target))
            return;

        src = target.AddComponent<AudioSource>();
        src.clip = sound.clip;
        src.volume = sound.volume * volume;
        src.pitch = sound.pitch;
        src.loop = true;
        src.spatialBlend = 0f;
        src.outputAudioMixerGroup = sound.audioMixer;

        src.Play();
        loopingSFX[target] = src;
    }

    public void StopLoopingSFX(GameObject target)
    {
        if (!loopingSFX.TryGetValue(target, out AudioSource src))
            return;

        if (src != null)
        {
            src.Stop();
            Destroy(src);
        }

        loopingSFX.Remove(target);
    }

    public void SetMasterVol(float vol)
    {
        if (vol < 0.0001f) vol = 0.0001f;
        audioMixer.SetFloat("masterVol", Mathf.Log10(vol) * 20);
    }

    public void SetBgmVol(float vol)
    {
        if (vol < 0.0001f) vol = 0.0001f;
        audioMixer.SetFloat("bgmVol", Mathf.Log10(vol) * 20);
    }

    public void SetSFXVol(float vol)
    {
        if (vol < 0.0001f) vol = 0.0001f;
        audioMixer.SetFloat("sfxVol", Mathf.Log10(vol) * 20);
    }

    public float GetMasterVol()
    {
        audioMixer.GetFloat("masterVol", out float val);
        return Mathf.Pow(10, val / 20f);
    }

    public float GetBgmVol()
    {
        audioMixer.GetFloat("bgmVol", out float val);
        return Mathf.Pow(10, val / 20f);
    }

    public float GetSFXVol()
    {
        audioMixer.GetFloat("sfxVol", out float val);
        return Mathf.Pow(10, val / 20f);
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
}
