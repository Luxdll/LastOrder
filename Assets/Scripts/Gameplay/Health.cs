using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 100;

    private int currentHealth;

    public event Action Died;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth == 0)
        {
            Died?.Invoke();
        }
    }
}