using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private float range = 50f;
    [SerializeField, Min(0.01f)] private float fireInterval = 0.25f;
    [SerializeField, Min(0.01f)] private float tracerDuration = 0.22f;

    private const float TracerLength = 0.8f;

    private Camera aimCamera;
    private InputAction attackAction;
    private LineRenderer tracer;
    private float nextShotTime;

    private Vector3 tracerStart;
    private Vector3 tracerEnd;
    private float tracerDistance;
    private float tracerStartTime;

    private void Awake()
    {
        aimCamera = Camera.main;
        attackAction = InputSystem.actions.FindAction("Player/Attack", throwIfNotFound: true);

        tracer = GetComponent<LineRenderer>();
        tracer.useWorldSpace = true;
        tracer.positionCount = 2;
        tracer.startWidth = 0.03f;
        tracer.endWidth = 0.08f;
        tracer.enabled = false;
    }

    private void Update()
    {
        bool wantsToShoot = attackAction.IsPressed() || attackAction.WasPressedThisFrame();

        if (Cursor.lockState == CursorLockMode.Locked && wantsToShoot && Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + fireInterval;
        }

        if (tracer.enabled)
        {
            UpdateTracer();
        }
    }

    private void Shoot()
    {
        Ray aimRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        tracerEnd = aimRay.GetPoint(range);

        if (Physics.Raycast(aimRay, out RaycastHit hit, range))
        {
            tracerEnd = hit.point;
            Debug.Log($"Hit: {hit.collider.name}", hit.collider);
        }

        tracerStart = transform.position
            + Vector3.up * 0.25f
            + aimCamera.transform.right * 0.65f;
        tracerDistance = Vector3.Distance(tracerStart, tracerEnd);
        tracerStartTime = Time.unscaledTime;
        tracer.enabled = true;
    }

    private void UpdateTracer()
    {
        float progress = (Time.unscaledTime - tracerStartTime) / tracerDuration;

        if (progress >= 1f)
        {
            tracer.enabled = false;
            return;
        }

        float headDistance = progress * (tracerDistance + TracerLength);
        float tailDistance = Mathf.Max(0f, headDistance - TracerLength);

        tracer.SetPosition(0, Vector3.MoveTowards(tracerStart, tracerEnd, tailDistance));
        tracer.SetPosition(1, Vector3.MoveTowards(tracerStart, tracerEnd, headDistance));
    }
}