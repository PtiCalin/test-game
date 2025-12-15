using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private Transform cam;

    private CharacterController controller;
    private Vector3 velocity;
    private InputSystem_Actions inputActions;

    public float Speed { get => speed; set => speed = value; }
    public void SetCamera(Transform cameraTransform) => cam = cameraTransform;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        transform.rotation = Quaternion.Normalize(transform.rotation);
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
    }

    private void OnValidate()
    {
        transform.rotation = Quaternion.Normalize(transform.rotation);
    }

    private void Update()
    {
        if (cam == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
                return;
            cam = mainCam.transform;
        }

        float dt = Time.deltaTime;
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        float horiz = moveInput.x;
        float vert = moveInput.y;

        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 move = camRight * horiz + camForward * vert;

        if (move.sqrMagnitude > 0.0001f)
        {
            move = move.normalized * speed;
            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.1f);
        }

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += Physics.gravity.y * dt;

        Vector3 motion = move * dt + velocity * dt;
        controller.Move(motion);
    }
}
