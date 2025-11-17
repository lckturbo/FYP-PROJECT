using UnityEngine;
using System.Collections.Generic;

public class PartyFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followDistance = 0.8f;
    [SerializeField] private float moveSpeed = 3.5f; // fallback if target has no movement script
    [SerializeField] private float catchUpSpeed = 5f;
    [SerializeField] private float stopDistance = 0.05f;

    private Animator animator;
    private Vector2 lastMoveDirection;
    private bool isMoving;

    private Queue<Vector3> pathQueue = new Queue<Vector3>();
    private Vector3 targetPathPosition;
    private float recordDistance = 0.1f;

    private NewPlayerMovement playerMovement; // reference to player's movement

    public void SetTarget(Transform newTarget, Vector2 initialDirection = default)
    {
        target = newTarget;

        if (target != null)
        {
            playerMovement = target.GetComponent<NewPlayerMovement>(); // get player movement speed source
            targetPathPosition = target.position;
            pathQueue.Enqueue(target.position);
        }

        if (initialDirection != Vector2.zero)
            lastMoveDirection = SnapToCardinal(initialDirection);
        else
            lastMoveDirection = Vector2.down;
    }

    public void FaceSameDirectionAs(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        lastMoveDirection = SnapToCardinal(direction);
        isMoving = false;

        if (animator)
        {
            animator.SetFloat("moveX", lastMoveDirection.x);
            animator.SetFloat("moveY", lastMoveDirection.y);
            animator.SetBool("moving", false);
            animator.speed = 1f;
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        lastMoveDirection = Vector2.down;
    }

    private Vector2 SnapToCardinal(Vector2 dir)
    {
        if (dir == Vector2.zero) return lastMoveDirection;

        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);

        if (absX > absY)
            return new Vector2(dir.x > 0 ? 1 : -1, 0);
        else
            return new Vector2(0, dir.y > 0 ? 1 : -1);
    }

    private void Update()
    {
        if (!target) return;

        if (pathQueue.Count == 0 || Vector3.Distance(target.position, pathQueue.ToArray()[pathQueue.Count - 1]) >= recordDistance)
        {
            pathQueue.Enqueue(target.position);
        }

        float totalPathLength = 0f;
        Vector3 lastPos = transform.position;
        foreach (var pos in pathQueue)
        {
            totalPathLength += Vector3.Distance(lastPos, pos);
            lastPos = pos;
        }

        while (pathQueue.Count > 1 && totalPathLength > followDistance)
        {
            Vector3 firstWaypoint = pathQueue.Peek();
            float distToFirst = Vector3.Distance(transform.position, firstWaypoint);

            if (distToFirst < stopDistance)
            {
                pathQueue.Dequeue();
                totalPathLength -= Vector3.Distance(transform.position, firstWaypoint);
            }
            else
            {
                break;
            }
        }

        if (pathQueue.Count > 0)
        {
            targetPathPosition = pathQueue.Peek();
        }

        float distanceToWaypoint = Vector2.Distance(transform.position, targetPathPosition);

        if (distanceToWaypoint > stopDistance)
        {
            Vector2 rawDir = (targetPathPosition - transform.position).normalized;

            Vector2 cardinalDir = SnapToCardinal(rawDir);

            if (distanceToWaypoint <= 0.3f && Vector2.Dot(cardinalDir, lastMoveDirection) < 0)
            {
            }
            else
            {
                lastMoveDirection = cardinalDir;
            }

            float usedSpeed = moveSpeed;

            var targetMover = target.GetComponent<NewPlayerMovement>();
            if (targetMover != null)
                usedSpeed = targetMover.GetWalkSpeed();

            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            if (distanceToTarget > followDistance * 2f)
                usedSpeed = catchUpSpeed;

            //usedSpeed *= 0.95f;

            transform.position = Vector2.MoveTowards(
                transform.position,
                (Vector2)transform.position + lastMoveDirection,
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