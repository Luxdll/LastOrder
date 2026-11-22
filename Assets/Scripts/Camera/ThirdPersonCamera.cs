using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0.85f, 0.25f, -3f);
    [SerializeField] private float sensitivity = 0.15f;

    private const float ShoulderHeight = 0.55f;
    private const float CameraRadius = 0.2f;
    private const float WallPadding = 0.05f;

    private float yaw;
    private float pitch = 20f;

    private void LateUpdate()
    {
        Mouse mouse = Mouse.current;

        if (Cursor.lockState == CursorLockMode.Locked && mouse != null)
        {
            Vector2 delta = mouse.delta.ReadValue();
            yaw += delta.x * sensitivity;
            pitch = Mathf.Clamp(pitch - delta.y * sensitivity, -45f, 45f);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = target.position + Vector3.up * ShoulderHeight;
        Vector3 cameraOffset = rotation * offset;
        float distance = cameraOffset.magnitude;
        Vector3 direction = cameraOffset.normalized;

        if (Physics.SphereCast(
                pivot,
                CameraRadius,
                direction,
                out RaycastHit hit,
                distance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            distance = Mathf.Max(0f, hit.distance - WallPadding);
        }

        transform.SetPositionAndRotation(pivot + direction * distance, rotation);
    }
}