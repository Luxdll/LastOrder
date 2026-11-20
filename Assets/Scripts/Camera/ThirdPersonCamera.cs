using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -4f);
    [SerializeField] private float sensitivity = 0.15f;

    private float yaw;
    private float pitch;

    private void LateUpdate()
    {
        Mouse mouse = Mouse.current;

        if (Cursor.lockState == CursorLockMode.Locked && mouse != null)
        {
            Vector2 delta = mouse.delta.ReadValue();
            yaw += delta.x * sensitivity;
            pitch = Mathf.Clamp(pitch - delta.y * sensitivity, -45f, 45f);
        }

        Vector3 focusOffset = Vector3.up * 0.5f;
        Vector3 pivot = target.position + focusOffset;
        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);

        transform.position = pivot + orbit * (offset - focusOffset);
        transform.LookAt(pivot);
    }
}