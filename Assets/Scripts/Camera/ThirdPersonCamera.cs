using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.OnScreen;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField]
    private Vector3 offset =
        new Vector3(0.85f, 0.25f, -3f);

    [SerializeField] private float sensitivity = 0.15f;
    [SerializeField] private OnScreenStick lookStick;
    [SerializeField, Min(1f)] private float mobileSensitivity = 120f;

    private const float ShoulderHeight = 0.55f;
    private const float CameraRadius = 0.2f;
    private const float WallPadding = 0.05f;

    private float yaw;
    private float pitch = 20f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector2 delta = ReadLookDelta();

        yaw += delta.x;
        pitch = Mathf.Clamp(pitch - delta.y, -45f, 45f);

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
            distance = Mathf.Max(
                0f,
                hit.distance - WallPadding
            );
        }

        transform.SetPositionAndRotation(
            pivot + direction * distance,
            rotation
        );
    }

    private Vector2 ReadLookDelta()
    {
        if (MobileInputMode.IsActive)
        {
            if (lookStick != null &&
                lookStick.isActiveAndEnabled &&
                lookStick.control is Vector2Control stick)
            {
                return stick.ReadValue()
                    * mobileSensitivity
                    * Time.deltaTime;
            }

            return Vector2.zero;
        }

        Mouse mouse = Mouse.current;

        if (mouse != null &&
            Cursor.lockState == CursorLockMode.Locked)
        {
            return mouse.delta.ReadValue() * sensitivity;
        }

        return Vector2.zero;
    }
}