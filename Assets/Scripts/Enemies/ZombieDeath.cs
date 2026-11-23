using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
public class ZombieDeath : MonoBehaviour
{
    [SerializeField, Min(0f)] private float destroyDelay = 3.5f;

    private Health health;
    private ZombieMovement movement;
    private NavMeshAgent agent;
    private Animator animator;
    private Collider[] colliders;

    private void Awake()
    {
        health = GetComponent<Health>();
        movement = GetComponent<ZombieMovement>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        colliders = GetComponentsInChildren<Collider>();
    }

    private void OnEnable()
    {
        health.Died += HandleDeath;
    }

    private void OnDisable()
    {
        health.Died -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (movement != null)
        {
            movement.enabled = false;
        }

        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
            }

            agent.enabled = false;
        }

        foreach (Collider bodyCollider in colliders)
        {
            bodyCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("Die");
        }

        Destroy(gameObject, destroyDelay);
    }
}