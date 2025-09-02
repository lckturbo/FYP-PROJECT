using UnityEngine;

[CreateAssetMenu(menuName = "Movement/Config", fileName = "MovementConfig")]
public class MovementConfig : ScriptableObject
{
    [Header("Run")]
    public float moveSpeed = 7f;
    [Tooltip("0..1. Lerp factor for air control vs ground. 1 = full control.")]
    public float airControlMultiplier = 0.9f;

    [Header("Jump")]
    public float jumpForce = 14f;
    [Tooltip("0 = no double jump, 1 = double jump, etc.")]
    public int maxAirJumps = 1;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    [Header("Better Jump (variable height)")]
    public float fallGravityMultiplier = 2.0f;
    public float lowJumpMultiplier = 2.5f;

    [Header("Dash")]
    public bool enableDash = true;
    public float dashSpeed = 18f;
    public float dashTime = 0.18f;
    public float dashCooldown = 0.5f;

    [Header("Wall Slide/Jump")]
    public bool enableWallMoves = true;
    public float wallSlideSpeed = 2.5f;
    public Vector2 wallJumpPower = new Vector2(12f, 14f);
    public float wallStickTime = 0.1f;

    [Header("Visual")]
    public bool flipSprite = true;
}
