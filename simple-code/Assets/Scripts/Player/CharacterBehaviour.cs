using TestGame.Builders;
using TestGame.Camera;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TestGame.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class CharacterBehaviour : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float turnSpeed = 540f;

        [Header("Spawn")]
        [SerializeField] private CorridorBuilder corridor;
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1f, 0f);

        [Header("Camera")]
        [SerializeField] private CameraRigController cameraRig;

        private Rigidbody _rb;
        private Vector3 _moveInput;

        private void Awake()
        {
            gameObject.tag = "Player";
            _rb = GetComponent<Rigidbody>();
            _rb.constraints = RigidbodyConstraints.FreezeRotation;

            if (cameraRig == null)
                cameraRig = FindFirstObjectByType<CameraRigController>();

            if (cameraRig != null)
                cameraRig.SetTarget(transform);
        }

        private void Start()
        {
            if (corridor == null) corridor = FindFirstObjectByType<CorridorBuilder>();
            if (corridor != null)
                transform.position = corridor.EntrancePositionWorld + spawnOffset;
        }

        private void Update()
        {
            Vector2 move = ReadMoveInput();
            _moveInput = new Vector3(move.x, 0f, move.y).normalized;
        }

        private void FixedUpdate()
        {
            if (_moveInput.sqrMagnitude < 0.0001f) return;

            Vector3 camForward = Vector3.forward;
            Vector3 camRight = Vector3.right;
            if (cameraRig != null)
            {
                Transform cam = cameraRig.transform;
                camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
                camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            }

            Vector3 desiredDir = (camForward * _moveInput.z + camRight * _moveInput.x).normalized;
            Vector3 desiredVel = desiredDir * moveSpeed;
            Vector3 vel = _rb.linearVelocity;
            _rb.linearVelocity = new Vector3(desiredVel.x, vel.y, desiredVel.z);

            Quaternion desiredRot = Quaternion.LookRotation(desiredDir, Vector3.up);
            _rb.MoveRotation(Quaternion.RotateTowards(_rb.rotation, desiredRot, turnSpeed * Time.fixedDeltaTime));
        }

        private static Vector2 ReadMoveInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null) return Vector2.zero;

            float x = 0f;
            float y = 0f;

            // Left / Right
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;

            // Forward / Back
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;

            Vector2 v = new Vector2(x, y);
            return v.sqrMagnitude > 1f ? v.normalized : v;
#else
            // Legacy Input Manager: Horizontal/Vertical map to WASD + Arrow keys by default.
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }
    }
}
