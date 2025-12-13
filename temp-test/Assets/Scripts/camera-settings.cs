using UnityEngine;
using UnityEngine.InputSystem;
using CameraComponent = UnityEngine.Camera;

namespace IFT2720
{
	/// <summary>
	/// Handles third-person orbit and bird's-eye orthographic camera modes (self-contained input).
	/// </summary>
	public class GameCamera : MonoBehaviour
	{
		public enum CameraMode
		{
			ThirdPerson,
			BirdsEye
		}

		[Header("Mode Settings")]
		[SerializeField] private CameraMode startMode = CameraMode.ThirdPerson;
		[SerializeField] private bool lockCursorInThirdPerson = true;
		[SerializeField] private bool unlockCursorInBirdsEye = true;

		[Header("Third-Person Orbit")]
		[SerializeField] private Transform target;
		[SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);
		[SerializeField, Min(0.1f)] private float distance = 6f;
		[SerializeField] private float rotationSpeed = 120f;
		[SerializeField] private float verticalSensitivity = 0.8f;
		[SerializeField] private float minPitch = -30f;
		[SerializeField] private float maxPitch = 70f;

		[Header("Bird's-Eye View")]
		[SerializeField] private float birdsEyeHeight = 35f;
		[SerializeField] private float birdsEyeFollowSmoothing = 6f;
		[SerializeField] private float birdsEyeOrthoLerpSpeed = 6f;
		[SerializeField] private float birdsEyeMinOrthographicSize = 15f;

		private CameraComponent cachedCamera;
		private CameraMode currentMode;
		private float yaw;
		private float pitch;
		private Vector3 birdsEyeCenter;
		private float birdsEyeTargetOrthographicSize;
		private float boundsHalfX;
		private float boundsHalfZ;
		private bool hasBounds;
		private InputAction toggleAction;
		private InputAction lookAction;

		public CameraMode CurrentMode => currentMode;

		private void Awake()
		{
			cachedCamera = GetComponent<CameraComponent>();
			birdsEyeCenter = Vector3.zero;
			birdsEyeTargetOrthographicSize = (cachedCamera != null && cachedCamera.orthographic)
				? Mathf.Max(cachedCamera.orthographicSize, birdsEyeMinOrthographicSize)
				: birdsEyeMinOrthographicSize;

			toggleAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/tab");
			lookAction = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta");
			toggleAction.performed += _ => ToggleCameraMode();
		}

		private void OnEnable()
		{
			toggleAction?.Enable();
			lookAction?.Enable();
		}

		private void Start()
		{
			EnsureTarget();

			Vector3 euler = transform.rotation.eulerAngles;
			yaw = euler.y;
			pitch = ClampPitch(euler.x);

			SetMode(startMode, true);
		}

		private void LateUpdate()
		{
			switch (currentMode)
			{
				case CameraMode.ThirdPerson:
					UpdateThirdPerson();
					break;
				case CameraMode.BirdsEye:
					UpdateBirdsEye();
					break;
			}
		}

		public void SetMode(CameraMode mode, bool instant = false)
		{
			currentMode = mode;
			ApplyCameraMode(instant);
		}

		public void SetTarget(Transform newTarget)
		{
			target = newTarget;
			if (currentMode == CameraMode.BirdsEye && newTarget != null)
			{
				SetCenter(new Vector3(newTarget.position.x, 0f, newTarget.position.z));
			}
		}

		public void SnapToTarget()
		{
			if (target == null)
			{
				return;
			}

			Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0f);
			transform.rotation = desiredRotation;
			transform.position = target.position + targetOffset - desiredRotation * Vector3.forward * distance;
		}

		public void SetCenter(Vector3 center)
		{
			Vector3 clamped = new Vector3(center.x, 0f, center.z);
			if (hasBounds)
			{
				clamped.x = Mathf.Clamp(clamped.x, -boundsHalfX, boundsHalfX);
				clamped.z = Mathf.Clamp(clamped.z, -boundsHalfZ, boundsHalfZ);
			}
			birdsEyeCenter = clamped;
		}

		public void ConfigureBounds(float width, float depth)
		{
			boundsHalfX = width * 0.5f;
			boundsHalfZ = depth * 0.5f;
			hasBounds = true;

			float halfExtent = Mathf.Max(width, depth) * 0.5f;
			birdsEyeTargetOrthographicSize = Mathf.Max(halfExtent, birdsEyeMinOrthographicSize);
			if (cachedCamera != null && currentMode == CameraMode.BirdsEye)
			{
				cachedCamera.orthographicSize = birdsEyeTargetOrthographicSize;
			}
		}

		public void SnapToCenter()
		{
			Vector3 desiredPosition = new Vector3(birdsEyeCenter.x, birdsEyeHeight, birdsEyeCenter.z);
			transform.position = desiredPosition;
			transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			if (cachedCamera != null)
			{
				cachedCamera.orthographicSize = birdsEyeTargetOrthographicSize;
			}
		}

		private void ApplyCameraMode(bool instant)
		{
			if (cachedCamera == null)
			{
				cachedCamera = GetComponent<CameraComponent>();
			}

			if (currentMode == CameraMode.ThirdPerson)
			{
				if (cachedCamera != null)
				{
					cachedCamera.orthographic = false;
				}
				if (lockCursorInThirdPerson)
				{
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
				}
				if (instant)
				{
					SnapToTarget();
				}
			}
			else
			{
				if (cachedCamera != null)
				{
					cachedCamera.orthographic = true;
					cachedCamera.orthographicSize = birdsEyeTargetOrthographicSize;
				}
				if (unlockCursorInBirdsEye)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
				if (instant)
				{
					SnapToCenter();
				}
			}
		}

		private void UpdateThirdPerson()
		{
			EnsureTarget();
			if (target == null)
			{
				return;
			}

			if (!lookAction.enabled)
			{
				lookAction.Enable();
			}

			Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
			if (mouseDelta.sqrMagnitude < 0.000001f)
			{
				// Fallback to legacy input axes when the new Input System delta is unavailable (e.g., old Input Manager only).
				mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
			}
			yaw += mouseDelta.x * rotationSpeed * Time.deltaTime;
			pitch -= mouseDelta.y * rotationSpeed * verticalSensitivity * Time.deltaTime;
			pitch = ClampPitch(pitch);

			Quaternion desiredRotation = Quaternion.Normalize(Quaternion.Euler(pitch, yaw, 0f));
			Vector3 desiredPosition = target.position + targetOffset - desiredRotation * Vector3.forward * distance;

			// Pin the camera to the character (no smoothing) for tight follow.
			transform.position = desiredPosition;
			transform.rotation = desiredRotation;
		}

		private void UpdateBirdsEye()
		{
			if (target != null)
			{
				SetCenter(target.position);
			}
			Vector3 desiredPosition = new Vector3(birdsEyeCenter.x, birdsEyeHeight, birdsEyeCenter.z);
			float followLerp = 1f - Mathf.Exp(-birdsEyeFollowSmoothing * Time.deltaTime);
			transform.position = Vector3.Lerp(transform.position, desiredPosition, followLerp);
			transform.rotation = Quaternion.Euler(90f, 0f, 0f);

			if (cachedCamera != null)
			{
				float orthoLerp = 1f - Mathf.Exp(-birdsEyeOrthoLerpSpeed * Time.deltaTime);
				cachedCamera.orthographicSize = Mathf.Lerp(cachedCamera.orthographicSize, birdsEyeTargetOrthographicSize, orthoLerp);
			}
		}

		private float ClampPitch(float rawPitch)
		{
			rawPitch = Mathf.Repeat(rawPitch + 180f, 360f) - 180f;
			return Mathf.Clamp(rawPitch, minPitch, maxPitch);
		}

		private void EnsureTarget()
		{
			if (target != null)
			{
				return;
			}

			GameObject candidate = GameObject.FindWithTag("Player");
			if (candidate != null)
			{
				target = candidate.transform;
			}
		}

		private void OnDisable()
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			toggleAction.Disable();
			lookAction.Disable();
		}

		private void ToggleCameraMode()
		{
			CameraMode next = currentMode == CameraMode.ThirdPerson ? CameraMode.BirdsEye : CameraMode.ThirdPerson;
			SetMode(next, false);
		}
	}
}
