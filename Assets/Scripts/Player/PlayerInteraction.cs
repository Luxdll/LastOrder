using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.OnScreen;

[RequireComponent(typeof(Health), typeof(PlayerMovement))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float pickupRadius = 2f;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private OnScreenButton mobilePickupButton;

    private Health health;
    private PlayerMovement movement;

    private void Awake()
    {
        health = GetComponent<Health>();
        movement = GetComponent<PlayerMovement>();

        SetPrompt(string.Empty);
    }

    private void OnDisable()
    {
        SetPrompt(string.Empty);
    }

    private void Update()
    {
        bool inputAvailable = MobileInputMode.IsActive ||
            Cursor.lockState == CursorLockMode.Locked;

        if (health.IsDead ||
            !movement.isActiveAndEnabled ||
            !inputAvailable)
        {
            SetPrompt(string.Empty);
            return;
        }

        HealthPickup pickup = FindClosestPickup();

        if (pickup == null)
        {
            SetPrompt(string.Empty);
            return;
        }

        if (health.CurrentHealth >= health.MaxHealth)
        {
            SetPrompt("Здоровье полное");
            return;
        }

        SetPrompt(
            MobileInputMode.IsActive
                ? "Нажмите «ПОДОБРАТЬ»"
                : "E — подобрать аптечку"
        );

        if (ReadPickupInput() && pickup.TryPickUp(health))
        {
            SetPrompt(string.Empty);
        }
    }

    private bool ReadPickupInput()
    {
        if (MobileInputMode.IsActive)
        {
            return mobilePickupButton != null &&
                mobilePickupButton.isActiveAndEnabled &&
                mobilePickupButton.control is ButtonControl button &&
                button.wasPressedThisFrame;
        }

        Keyboard keyboard = Keyboard.current;

        return keyboard != null &&
            keyboard.eKey.wasPressedThisFrame;
    }

    private HealthPickup FindClosestPickup()
    {
        Vector3 origin = transform.position;

        Collider[] nearbyColliders = Physics.OverlapSphere(
            origin,
            pickupRadius,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide
        );

        HealthPickup closest = null;
        float closestDistanceSquared = float.PositiveInfinity;

        foreach (Collider nearbyCollider in nearbyColliders)
        {
            HealthPickup pickup =
                nearbyCollider.GetComponentInParent<HealthPickup>();

            if (pickup == null || !pickup.isActiveAndEnabled)
            {
                continue;
            }

            Vector3 closestPoint =
                nearbyCollider.ClosestPoint(origin);

            float distanceSquared =
                (closestPoint - origin).sqrMagnitude;

            if (distanceSquared >= closestDistanceSquared)
            {
                continue;
            }

            Vector3 direction =
                nearbyCollider.bounds.center - origin;

            float distance = direction.magnitude;

            if (distance > 0.001f &&
                Physics.Raycast(
                    origin,
                    direction / distance,
                    out RaycastHit hit,
                    distance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                HealthPickup hitPickup =
                    hit.collider.GetComponentInParent<HealthPickup>();

                if (hitPickup != pickup)
                {
                    continue;
                }
            }

            closest = pickup;
            closestDistanceSquared = distanceSquared;
        }

        return closest;
    }

    private void SetPrompt(string message)
    {
        if (promptText == null)
        {
            return;
        }

        promptText.text = message;
        promptText.enabled = !string.IsNullOrEmpty(message);
    }
}