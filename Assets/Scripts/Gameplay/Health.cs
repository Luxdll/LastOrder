using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 100;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    public event Action Died;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (IsDead)
        {
            Died?.Invoke();
        }
    }

    public bool Heal(int amount)
    {
        if (amount <= 0 || IsDead || currentHealth >= maxHealth)
        {
            return false;
        }

        currentHealth = (int)Math.Min(
            (long)currentHealth + amount,
            maxHealth
        );

        return true;
    }
}