using UnityEngine;

/// <summary>
/// Singleton to store the latest buff value across scenes.
/// </summary>
public class BuffData : MonoBehaviour
{
    public static BuffData instance;

    public int latestAttackBuff = 0;       // Latest applied attack buff
    public bool isBuffActive = false;      // Whether the buff is active

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetBuff(int amount)
    {
        latestAttackBuff = amount;
        isBuffActive = true;
    }

    public void ClearBuff()
    {
        latestAttackBuff = 0;
        isBuffActive = false;
    }
}
