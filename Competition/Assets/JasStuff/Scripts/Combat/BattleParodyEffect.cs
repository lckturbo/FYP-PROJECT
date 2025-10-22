using UnityEngine;

public class BattleParodyEffect : MonoBehaviour
{
    public enum ParodyType
    {
        None,
        ZoomCrit
    }

    public ParodyType type;
    private Camera cam;
    private Vector3 originalPos;
    private float originalSize;

    private void Awake()
    {
        cam = Camera.main;
        if(cam!= null)
        {
            originalPos = cam.transform.position;
            originalSize = cam.orthographicSize;
        }
    }

    public void PlayParody(ParodyType type, Combatant attacker, Combatant target)
    {
        switch (type)
        {
            case ParodyType.ZoomCrit:
                break;
        }
    }
}
