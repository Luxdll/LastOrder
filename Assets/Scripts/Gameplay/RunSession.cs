using System;
using UnityEngine;

[RequireComponent(typeof(Health), typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerShooting))]
[DefaultExecutionOrder(200)]
public class RunSession : MonoBehaviour
{
    [SerializeField] private ZombieSpawner spawner;
    [SerializeField] private AudioSource music;

    private Health health;
    private PlayerMovement movement;
    private PlayerShooting shooting;
    private Animator animator;

    private float startTime;
    private float finalTime;

    public bool HasStarted { get; private set; }
    public bool IsFinished { get; private set; }
    public bool IsDelivered { get; private set; }

    public float ElapsedTime =>
        !HasStarted ? 0f :
        IsFinished ? finalTime :
        Time.time - startTime;

    public int KilledCount =>
        spawner != null ? spawner.KilledCount : 0;

    public int RemainingHealth => health.CurrentHealth;

    public event Action Finished;

    private void Awake()
    {
        health = GetComponent<Health>();
        movement = GetComponent<PlayerMovement>();
        shooting = GetComponent<PlayerShooting>();
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

    private void Start()
    {
        if (spawner == null)
        {
            Debug.LogError("Назначь Spawner в RunSession.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (!HasStarted &&
            !IsFinished &&
            !health.IsDead &&
            movement.isActiveAndEnabled &&
            Cursor.lockState == CursorLockMode.Locked)
        {
            startTime = Time.time;
            HasStarted = true;
        }
    }

    public void CompleteDelivery()
    {
        if (!HasStarted ||
            IsFinished ||
            health.IsDead ||
            spawner == null ||
            !spawner.IsCleared)
        {
            return;
        }

        Finish(true);
    }

    private void HandleDeath()
    {
        if (HasStarted)
        {
            Finish(false);
        }
    }

    private void Finish(bool delivered)
    {
        if (IsFinished)
        {
            return;
        }

        finalTime = Time.time - startTime;
        IsDelivered = delivered;
        IsFinished = true;

        movement.enabled = false;
        shooting.enabled = false;

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }

        if (music != null)
        {
            music.Stop();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Finished?.Invoke();

        Debug.Log(
            delivered ? "Заказ доставлен." : "Заказ не доставлен.",
            this
        );
    }
}