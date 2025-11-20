using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public void PlaySFX(string name)
    {
        AudioManager.instance.PlaySFXAtPoint(name, new Vector3(0, 0, 0));
    }
}
