using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Health))]
public class ZombieMovement : MonoBehaviour
{
    [SerializeField] private PlayerMovement target;

    [SerializeField, Min(0.1f)] private float attackRange = 1.4f;
    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField, Min(0f)] private float hitDelay = 0.8f;
    [SerializeField, Min(0.1f)] private float attackDuration = 2.9f;

    private NavMeshAgent agent;
    private Animator animator;
    private Health health;
    private Health targetHealth;
    private bool isAttacking;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        health = GetComponent<Health>();

        Initialize(target);
    }

    public void Initialize(PlayerMovement newTarget)
    {
        target = newTarget;
        targetHealth = target != null
            ? target.GetComponent<Health>()
            : null;
    }

    private void Update()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            return;
        }

        if (!CanAct())
        {
            agent.isStopped = true;
            return;
        }

        if (isAttacking)
        {
            agent.isStopped = true;
            return;
        }

        if (animator != null && IsTargetInRange())
        {
            StartCoroutine(Attack());
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(target.transform.position);
    }

    private void LateUpdate()
    {
        if (animator == null)
        {
            return;
        }

        bool isMoving = false;

        if (!isAttacking &&
            agent.isActiveAndEnabled &&
            agent.isOnNavMesh &&
            !agent.isStopped)
        {
            Vector3 velocity = agent.velocity;
            velocity.y = 0f;

            isMoving = velocity.sqrMagnitude > 0.01f;
        }

        animator.SetBool("IsMoving", isMoving);
    }

    private bool CanAct()
    {
        return !health.IsDead &&
            target != null &&
            target.isActiveAndEnabled &&
            targetHealth != null &&
            !targetHealth.IsDead;
    }

    private bool IsTargetInRange()
    {
        Vector3 difference = target.transform.position - transform.position;
        difference.y = 0f;

        return difference.sqrMagnitude <= attackRange * attackRange;
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        agent.isStopped = true;
        agent.ResetPath();

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        animator.SetBool("IsMoving", false);
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(hitDelay);

        if (CanAct() && IsTargetInRange())
        {
            targetHealth.TakeDamage(damage);

            Debug.Log(
                $"Здоровье курьера: {targetHealth.CurrentHealth}/{targetHealth.MaxHealth}",
                targetHealth
            );
        }

        yield return new WaitForSeconds(
            Mathf.Max(0f, attackDuration - hitDelay)
        );

        isAttacking = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
        }
    }
}