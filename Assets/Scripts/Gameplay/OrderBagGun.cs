using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class OrderBagGun : MonoBehaviour
{
    [SerializeField, Min(1)] private int damage = 25;
    [SerializeField, Min(0.1f)] private float lifetime = 5f;
    [SerializeField, Min(0f)] private float spinSpeed = 6f;

    private Rigidbody body;
    private BoxCollider bodyCollider;
    private PlayerMovement owner;

    private bool isLaunched;
    private bool hasHit;

    public Vector3 CenterPosition => body.worldCenterOfMass;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<BoxCollider>();
    }

    public void Launch(Vector3 initialVelocity, PlayerMovement thrower)
    {
        if (isLaunched)
        {
            return;
        }

        if (thrower == null || !thrower.isActiveAndEnabled)
        {
            Destroy(gameObject);
            return;
        }

        owner = thrower;

        Collider[] ownerColliders =
            owner.GetComponentsInChildren<Collider>();

        foreach (Collider ownerCollider in ownerColliders)
        {
            if (ownerCollider != null)
            {
                Physics.IgnoreCollision(
                    bodyCollider,
                    ownerCollider,
                    true
                );
            }
        }

        isLaunched = true;

        body.linearVelocity = initialVelocity;
        body.angularVelocity = Random.onUnitSphere * spinSpeed;

        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isLaunched || hasHit)
        {
            return;
        }

        hasHit = true;

        if (owner != null && owner.isActiveAndEnabled)
        {
            Health health =
                collision.collider.GetComponentInParent<Health>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}