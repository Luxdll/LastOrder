using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int healAmount = 3;

    private bool isCollected;

    public bool TryPickUp(Health playerHealth)
    {
        if (isCollected || playerHealth == null)
        {
            return false;
        }

        if (!playerHealth.Heal(healAmount))
        {
            return false;
        }

        isCollected = true;

        gameObject.SetActive(false);
        Destroy(gameObject);

        return true;
    }
}