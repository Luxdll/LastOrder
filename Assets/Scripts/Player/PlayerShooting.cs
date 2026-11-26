using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[DefaultExecutionOrder(100)]
public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private OrderBagGun bagPrefab;
    [SerializeField, Min(1f)] private float aimRange = 20f;
    [SerializeField, Min(0.1f)] private float throwInterval = 0.7f;
    [SerializeField, Min(1f)] private float throwSpeed = 14f;

    [SerializeField]
    private Vector3 throwOffset =
        new Vector3(0.45f, 0.35f, 0.65f);

    private Camera aimCamera;
    private PlayerMovement movement;
    private InputAction attackAction;
    private float nextThrowTime;

    private void Awake()
    {
        aimCamera = Camera.main;
        movement = GetComponent<PlayerMovement>();

        attackAction = InputSystem.actions.FindAction(
            "Player/Attack",
            throwIfNotFound: true
        );

        LineRenderer oldTracer = GetComponent<LineRenderer>();

        if (oldTracer != null)
        {
            oldTracer.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (bagPrefab == null ||
            aimCamera == null ||
            !movement.isActiveAndEnabled ||
            Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        bool wantsToThrow =
            attackAction.IsPressed() || attackAction.WasPressedThisFrame();

        if (!wantsToThrow || Time.time < nextThrowTime)
        {
            return;
        }

        nextThrowTime = Time.time + throwInterval;
        ThrowBag();
    }

    private void ThrowBag()
    {
        Physics.SyncTransforms();

        Ray aimRay = aimCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 aimPoint = aimRay.GetPoint(aimRange);

        if (Physics.Raycast(
                aimRay,
                out RaycastHit hit,
                aimRange,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
        }

        Quaternion cameraYaw = Quaternion.Euler(
            0f,
            aimCamera.transform.eulerAngles.y,
            0f
        );

        Vector3 spawnPosition =
            transform.position + cameraYaw * throwOffset;

        Vector3 spawnCenter =
            spawnPosition + Vector3.up * 0.15f;

        Vector3 chestPosition =
            transform.position + Vector3.up * throwOffset.y;

        if (Vector3.Dot(
                aimPoint - spawnPosition,
                aimRay.direction) <= 0f)
        {
            return;
        }

        if (Physics.Linecast(
                chestPosition,
                spawnCenter,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        if (Physics.CheckSphere(
                spawnCenter,
                0.23f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        OrderBagGun bag = Instantiate(
            bagPrefab,
            spawnPosition,
            cameraYaw
        );

        bag.gameObject.layer =
            LayerMask.NameToLayer("Ignore Raycast");

        Vector3 displacement = aimPoint - bag.CenterPosition;

        float flightTime = Mathf.Clamp(
            displacement.magnitude / throwSpeed,
            0.2f,
            1.2f
        );

        Vector3 initialVelocity =
            displacement / flightTime
            - Physics.gravity * flightTime * 0.5f;

        bag.Launch(initialVelocity, movement);
    }
}