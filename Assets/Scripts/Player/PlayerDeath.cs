using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerShooting))]
public class PlayerDeath : MonoBehaviour
{
    private Health health;
    private PlayerMovement movement;
    private PlayerShooting shooting;
    private LineRenderer tracer;
    private Animator animator;

    private void Awake()
    {
        health = GetComponent<Health>();
        movement = GetComponent<PlayerMovement>();
        shooting = GetComponent<PlayerShooting>();
        tracer = GetComponent<LineRenderer>();
        animator = GetComponentInChildren<Animator>();
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
        movement.enabled = false;
        shooting.enabled = false;

        if (tracer != null)
        {
            tracer.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Курьер погиб.", gameObject);
    }
}