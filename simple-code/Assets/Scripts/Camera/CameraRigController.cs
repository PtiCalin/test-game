using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TestGame.Camera
{
    public sealed class CameraRigController : MonoBehaviour
    {
        public enum ViewMode
        {
            ThirdPerson,
            BirdsEye
        }

        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Input")]
        [SerializeField] private float mouseSensitivity = 3f;
        [SerializeField] private bool invertY;
    #if ENABLE_INPUT_SYSTEM
        [Tooltip("Scale applied to Mouse.delta (pixels per frame) when using the new Input System.")]
        [SerializeField] private float inputSystemMouseScale = 0.1f;
    #endif

        [Header("Third Person")]
        [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0f, 2.5f, -5.5f);
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 70f;

        [Header("Birds Eye")]
        [SerializeField] private float birdsEyeHeight = 18f;
        [SerializeField] private float birdsEyeDistance = 0.01f;

        [Header("Mode")]
        [SerializeField] private ViewMode mode = ViewMode.ThirdPerson;

        private float _yaw;
        private float _pitch;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void Start()
        {
            // Typical mouse-look setup.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            if (target == null) return;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
                mode = mode == ViewMode.ThirdPerson ? ViewMode.BirdsEye : ViewMode.ThirdPerson;
#else
            if (Input.GetKeyDown(KeyCode.Tab))
                mode = mode == ViewMode.ThirdPerson ? ViewMode.BirdsEye : ViewMode.ThirdPerson;
#endif

            switch (mode)
            {
                case ViewMode.ThirdPerson:
                    UpdateThirdPerson();
                    break;
                case ViewMode.BirdsEye:
                    UpdateBirdsEye();
                    break;
            }
        }

        private void UpdateThirdPerson()
        {
            Vector2 look = ReadLookDelta();
            float mx = look.x;
            float my = look.y;

            _yaw += mx * mouseSensitivity;
            _pitch += (invertY ? my : -my) * mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 desired = target.position + rot * thirdPersonOffset;

            transform.position = desired;
            transform.rotation = rot;
        }

        private void UpdateBirdsEye()
        {
            Vector3 pos = target.position + Vector3.up * birdsEyeHeight + Vector3.back * birdsEyeDistance;
            transform.position = pos;
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private Vector2 ReadLookDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                Vector2 d = Mouse.current.delta.ReadValue();
                return d * inputSystemMouseScale;
            }
            return Vector2.zero;
#else
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
        }
    }
}
