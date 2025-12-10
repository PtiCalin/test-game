using UnityEngine;
using UnityEngine.InputSystem;
using UCamera = UnityEngine.Camera;

/// <summary>
/// Rigidbody-based character controller with acceleration, speed cap, air control and jump stack.
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float maxSpeed = 6f;
    [SerializeField, Min(0f)] private float acceleration = 18f;
    [SerializeField, Range(0f, 1f)] private float airControlMultiplier = 0.5f;
    [SerializeField, Min(0f)] private float groundedDrag = 5f;
    [SerializeField, Min(0f)] private float airDrag = 0.5f;

    [Header("Rotation")]
    [SerializeField, Min(0f)] private float rotationSpeed = 12f;
    [SerializeField] private Transform character;

    [Header("Jump")]
    [SerializeField, Min(0f)] private float jumpForce = 5f;
    [SerializeField, Min(1)] private int maxJumps = 2; // 2 = double jump
    [SerializeField] private bool resetVerticalVelocityOnJump = true;

    [Header("Ground Check")]
    [SerializeField, Min(0f)] private float groundCheckDistance = 1.1f;
    [SerializeField, Min(0f)] private float groundCheckRadius = 0.35f;
    [SerializeField] private float groundCheckOffset = 0.05f;
    [SerializeField] private LayerMask groundLayers = Physics.DefaultRaycastLayers;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private Rigidbody rb;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool jumpRequested;
    private bool isGrounded;
    private int jumpsUsed;

    public float MaxSpeed { get => maxSpeed; set => maxSpeed = Mathf.Max(0f, value); }
    public float JumpForce { get => jumpForce; set => jumpForce = Mathf.Max(0f, value); }
    public void SetAllowDoubleJump(bool enabled) => maxJumps = enabled ? Mathf.Max(maxJumps, 2) : 1;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (cameraTransform == null)
        {
            cameraTransform = UCamera.main != null ? UCamera.main.transform : null;
        }
    }

    private void Update()
    {
        if (cameraTransform == null)
        {
            cameraTransform = UCamera.main != null ? UCamera.main.transform : null;
        }

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        if (inputActions.Player.Jump.WasPressedThisFrame())
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        EvaluateGrounded();
        ApplyDrag();

        Vector3 desiredMove = CalculateMoveDirection(moveInput);
        ApplyMovement(desiredMove);
        HandleJump();
        ApplyRotation(desiredMove);
    }

    private void EvaluateGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * groundCheckOffset;
        isGrounded = Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out _, groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore);
        if (isGrounded)
        {
            jumpsUsed = 0;
        }
    }

    private void ApplyDrag()
    {
        rb.linearDamping = isGrounded ? groundedDrag : airDrag;
    }

    private Vector3 CalculateMoveDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        if (cameraTransform != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            return (right * input.x + forward * input.y).normalized;
        }

        return new Vector3(input.x, 0f, input.y).normalized;
    }

    private void ApplyMovement(Vector3 desiredDirection)
    {
        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 targetHorizontal = desiredDirection * maxSpeed;

        float accel = acceleration * (isGrounded ? 1f : airControlMultiplier);
        horizontal = Vector3.MoveTowards(horizontal, targetHorizontal, accel * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
    }

    private void HandleJump()
    {
        if (!jumpRequested)
        {
            return;
        }

        bool canJump = isGrounded || jumpsUsed < maxJumps - 1;
        if (!canJump)
        {
            jumpRequested = false;
            return;
        }

        Vector3 velocity = rb.linearVelocity;
        if (resetVerticalVelocityOnJump)
        {
            velocity.y = 0f;
        }
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpsUsed++;
        isGrounded = false;
        jumpRequested = false;
    }

    private void ApplyRotation(Vector3 desiredDirection)
    {
        if (desiredDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRot = Quaternion.LookRotation(desiredDirection, Vector3.up);
        Quaternion smoothRot = Quaternion.Slerp(rb.rotation, targetRot, 1f - Mathf.Exp(-rotationSpeed * Time.fixedDeltaTime));
        rb.MoveRotation(smoothRot);

        if (character != null)
        {
            character.rotation = smoothRot;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 origin = transform.position + Vector3.up * groundCheckOffset;
        Gizmos.DrawWireSphere(origin - Vector3.up * groundCheckDistance, groundCheckRadius);
    }
}
