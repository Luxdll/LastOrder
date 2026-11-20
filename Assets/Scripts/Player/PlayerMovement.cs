using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private CharacterController controller;
    private Transform cameraTransform;
    private InputAction moveAction;
    private float verticalSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        moveAction = InputSystem.actions.FindAction("Player/Move", throwIfNotFound: true);
    }

    private void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Quaternion cameraYaw = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
        Vector3 movement = cameraYaw * new Vector3(input.x, 0f, input.y) * moveSpeed;

        if (controller.isGrounded && verticalSpeed < 0f)
        {
            verticalSpeed = -2f;
        }

        verticalSpeed += Physics.gravity.y * Time.deltaTime;
        movement.y = verticalSpeed;

        controller.Move(movement * Time.deltaTime);
    }
}