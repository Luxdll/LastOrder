using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    private CharacterController controller;
    private Animator animator;
    private Transform visual;
    private Transform cameraTransform;
    private InputAction moveAction;
    private GyroMovementInput gyroInput;
    private float verticalSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        gyroInput = GetComponent<GyroMovementInput>();

        if (animator != null)
        {
            visual = animator.transform;
        }

        cameraTransform = Camera.main.transform;

        moveAction = InputSystem.actions.FindAction(
            "Player/Move",
            throwIfNotFound: true
        );
    }

    private void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector2 tiltInput =
            gyroInput != null && gyroInput.isActiveAndEnabled
                ? gyroInput.ReadInput()
                : Vector2.zero;

        if (input.sqrMagnitude <= 0.01f)
        {
            input = tiltInput;
        }

        Quaternion cameraYaw = Quaternion.Euler(
            0f,
            cameraTransform.eulerAngles.y,
            0f
        );

        Vector3 movement =
            cameraYaw * new Vector3(input.x, 0f, input.y) * moveSpeed;

        if (controller.isGrounded && verticalSpeed < 0f)
        {
            verticalSpeed = -2f;
        }

        verticalSpeed += Physics.gravity.y * Time.deltaTime;
        movement.y = verticalSpeed;

        controller.Move(movement * Time.deltaTime);

        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;

        bool isMoving = horizontalVelocity.sqrMagnitude > 0.01f;

        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }

        if (visual != null && isMoving)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(horizontalVelocity);

            visual.rotation = Quaternion.RotateTowards(
                visual.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}