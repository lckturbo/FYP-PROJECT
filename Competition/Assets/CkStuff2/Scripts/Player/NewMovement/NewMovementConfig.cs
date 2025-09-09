using UnityEngine;

[CreateAssetMenu(menuName = "Movement/NewConfig", fileName = "NewMovementConfig")]
public class NewMovementConfig : ScriptableObject
{
    [Header("Movement")]
    public float walkSpeed = 5f;
}
