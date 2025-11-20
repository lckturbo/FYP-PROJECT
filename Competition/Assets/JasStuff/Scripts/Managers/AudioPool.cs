using UnityEngine;
using System.Collections.Generic;

public class AudioPool : MonoBehaviour
{
    public static AudioPool instance;

    private Queue<AudioSource> pool = new Queue<AudioSource>();
    private const int initialPoolSize = 10;

    void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        //DontDestroyOnLoad(gameObject);

        for (int i = 0; i < initialPoolSize; i++)
            pool.Enqueue(CreateNewSource());
    }

    private AudioSource CreateNewSource()
    {
        GameObject obj = new GameObject("PooledAudioSource");
        obj.transform.parent = transform;
        AudioSource src = obj.AddComponent<AudioSource>();
        src.playOnAwake = false;
        return src;
    }

    public AudioSource GetSource()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        return CreateNewSource();
    }

    public void ReturnSource(AudioSource src)
    {
        src.Stop();
        src.clip = null;
        src.transform.localPosition = Vector3.zero;
        pool.Enqueue(src);
    }
}
