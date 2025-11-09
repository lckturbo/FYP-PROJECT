using UnityEngine;

public class SettingsManager : MonoBehaviour, IDataPersistence
{
    public static SettingsManager instance;

    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    public void OnBGMVolumeChanged(float value)
    {
        AudioManager.instance.SetBgmVol(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        AudioManager.instance.SetSFXVol(value);
    }
    public void LoadData(GameData data)
    {
        AudioManager.instance.SetBgmVol(data.bgmVolume);
        AudioManager.instance.SetSFXVol(data.sfxVolume);
    }

    public void SaveData(ref GameData data)
    {
        data.bgmVolume = AudioManager.instance.GetBgmVol();
        data.sfxVolume = AudioManager.instance.GetSFXVol();
    }
}
