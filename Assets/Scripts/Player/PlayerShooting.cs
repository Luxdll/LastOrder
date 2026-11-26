using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.OnScreen;

[RequireComponent(typeof(PlayerMovement))]
[DefaultExecutionOrder(100)]
public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private OrderBagGun bagPrefab;
    [SerializeField] private OnScreenButton mobileFireButton;

    [SerializeField, Min(1f)] private float aimRange = 20f;
    [SerializeField, Min(0.4f)] private float throwInterval = 0.7f;
    [SerializeField, Min(1f)] private float throwSpeed = 14f;
    [SerializeField, Min(0f)] private float releaseDelay = 0.2f;

    [SerializeField]
    private Vector3 throwOffset =
        new Vector3(0.45f, 0.35f, 0.65f);

    private static readonly int ThrowHash =
        Animator.StringToHash("Throw");

    private Camera aimCamera;
    private PlayerMovement movement;
    private Animator animator;
    private InputAction attackAction;

    private float nextThrowTime;
    private float releaseTime;
    private bool isThrowPending;

    private void Awake()
    {
        aimCamera = Camera.main;
        movement = GetComponent<PlayerMovement>();
        animator = GetComponentInChildren<Animator>();

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

    private void OnDisable()
    {
        CancelPendingThrow();
    }

    private void LateUpdate()
    {
        bool inputAvailable = MobileInputMode.IsActive ||
            Cursor.lockState == CursorLockMode.Locked;

        if (bagPrefab == null ||
            aimCamera == null ||
            !movement.isActiveAndEnabled ||
            !inputAvailable)
        {
            CancelPendingThrow();
            return;
        }

        if (isThrowPending)
        {
            if (Time.time >= releaseTime)
            {
                isThrowPending = false;
                ThrowBag();
            }

            return;
        }

        if (!ReadThrowInput() || Time.time < nextThrowTime)
        {
            return;
        }

        isThrowPending = true;
        releaseTime = Time.time + releaseDelay;

        nextThrowTime = Time.time +
            Mathf.Max(throwInterval, releaseDelay + 0.1f);

        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.ResetTrigger(ThrowHash);
            animator.SetTrigger(ThrowHash);
        }
    }

    private bool ReadThrowInput()
    {
        if (MobileInputMode.IsActive)
        {
            if (mobileFireButton != null &&
                mobileFireButton.isActiveAndEnabled &&
                mobileFireButton.control is ButtonControl button)
            {
                return button.isPressed ||
                    button.wasPressedThisFrame;
            }

            return false;
        }

        return attackAction.IsPressed() ||
            attackAction.WasPressedThisFrame();
    }

    private void CancelPendingThrow()
    {
        if (!isThrowPending)
        {
            return;
        }

        isThrowPending = false;

        if (animator != null)
        {
            animator.ResetTrigger(ThrowHash);
        }
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