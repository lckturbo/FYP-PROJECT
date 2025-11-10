using UnityEngine;

public class PartyFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followDistance = 0.9f;
    [SerializeField] private float moveSpeed = 3.5f; // fallback if target has no movement script
    [SerializeField] private float stopDistance = 0.15f;

    private Animator animator;
    private Vector2 lastMoveDirection;
    private bool isMoving;

    private Vector2 targetPosition;
    private bool hasTargetPosition;

    private NewPlayerMovement playerMovement; // reference to player's movement

    public void SetTarget(Transform newTarget, Vector2 initialDirection = default)
    {
        target = newTarget;

        if (target != null)
        {
            playerMovement = target.GetComponent<NewPlayerMovement>(); // get player movement speed source
            targetPosition = target.position;
            hasTargetPosition = true;
        }

        if (initialDirection != Vector2.zero)
            lastMoveDirection = initialDirection;
    }

    public void FaceSameDirectionAs(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        lastMoveDirection = direction;
        isMoving = false;

        if (animator)
        {
            animator.SetFloat("moveX", direction.x);
            animator.SetFloat("moveY", direction.y);
            animator.SetBool("moving", false);
            animator.speed = 1f;
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!target) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget > followDistance)
        {
            Vector2 dir = ((Vector2)transform.position - (Vector2)target.position).normalized;
            targetPosition = (Vector2)target.position + dir * followDistance;
            hasTargetPosition = true;
        }

        if (hasTargetPosition)
        {
            float distanceToDestination = Vector2.Distance(transform.position, targetPosition);

            if (distanceToDestination > stopDistance)
            {
                Vector2 moveDir = (targetPosition - (Vector2)transform.position).normalized;
                lastMoveDirection = moveDir;

                float usedSpeed = moveSpeed;
                if (playerMovement != null)
                    usedSpeed = playerMovement.GetWalkSpeed(); 

                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPosition,
                    usedSpeed * Time.deltaTime
                );

                isMoving = true;

                if (animator && playerMovement != null && playerMovement.GetStats() != null)
                    animator.speed = usedSpeed / playerMovement.GetStats().Speed;
            }
            else
            {
                isMoving = false;
                if (animator) animator.speed = 1f;
            }
        }

        if (animator)
        {
            animator.SetFloat("moveX", lastMoveDirection.x);
            animator.SetFloat("moveY", lastMoveDirection.y);
            animator.SetBool("moving", isMoving);
        }

        var ySorter = GetComponent<YSorter>();
        if (ySorter)
        {
            ySorter.SetMoveDirection(lastMoveDirection);
            ySorter.SetLeader(target == null);
        }
    }
}
