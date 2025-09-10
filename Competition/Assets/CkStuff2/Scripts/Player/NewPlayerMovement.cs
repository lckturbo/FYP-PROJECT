using UnityEngine;
using UnityEngine.InputSystem;

public class NewPlayerMovement : MonoBehaviour
{
    public float speed;
    private Rigidbody2D myRigidbody;
    private Vector3 change;
    private Animator animator;

    private PlayerInput playerInput;
    private InputAction moveAction;

    void Start()
    {
        animator = GetComponent<Animator>();
        myRigidbody = GetComponent<Rigidbody2D>();

        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
    }

    void Update()
    {
        change = Vector3.zero;

        Vector2 input = moveAction.ReadValue<Vector2>();
        change.x = input.x;
        change.y = input.y;

        UpdateAnimationAndMove();
    }

    void UpdateAnimationAndMove()
    {
        if (change != Vector3.zero)
        {
            MoveCharacter();
            animator.SetFloat("moveX", change.x);
            animator.SetFloat("moveY", change.y);
            animator.SetBool("moving", true);
        }
        else
        {
            animator.SetBool("moving", false);
        }
    }

    void MoveCharacter()
    {
        myRigidbody.MovePosition(
            transform.position + change * speed * Time.fixedDeltaTime
        );
    }
}
