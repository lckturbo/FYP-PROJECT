using UnityEngine;
public class PartyFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followDistance = 0.75f;
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private float stopDistance = 0.15f;

    private Animator animator;
    private Vector2 lastMoveDirection;
    private bool isMoving;

    private Vector2 targetPosition;
    private bool hasTargetPosition;

    public void SetTarget(Transform newTarget, Vector2 initialDirection = default)
    {
        target = newTarget;
        if (target != null)
        {
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

                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );

                isMoving = true;
            }
            else
            {
                isMoving = false;
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