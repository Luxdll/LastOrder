using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieMovement : MonoBehaviour
{
    [SerializeField] private PlayerMovement target;

    private NavMeshAgent agent;
    private Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!agent.isOnNavMesh)
        {
            return;
        }

        bool canFollow = target != null && target.isActiveAndEnabled;

        agent.isStopped = !canFollow;

        if (canFollow)
        {
            agent.SetDestination(target.transform.position);
        }
    }

    private void LateUpdate()
    {
        if (animator == null)
        {
            return;
        }

        bool isMoving = false;

        if (agent.isActiveAndEnabled &&
            agent.isOnNavMesh &&
            !agent.isStopped)
        {
            Vector3 velocity = agent.velocity;
            velocity.y = 0f;

            isMoving = velocity.sqrMagnitude > 0.01f;
        }

        animator.SetBool("IsMoving", isMoving);
    }
}