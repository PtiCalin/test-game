using UnityEngine;

public class GameCamera : MonoBehaviour
{
	private const float YMin = -50f;
	private const float YMax = 50f;

	[SerializeField] private Transform lookAt;
	[SerializeField] private float distance = 10f;
	[SerializeField] private float sensitivity = 4f;

	private float currentX;
	private float currentY;
	private InputSystem_Actions inputActions;

	public void SetTarget(Transform target) => lookAt = target;

	private void Awake()
	{
		transform.rotation = Quaternion.Normalize(transform.rotation);
		currentX = transform.eulerAngles.y;
		currentY = transform.eulerAngles.x;
		inputActions = new InputSystem_Actions();
		inputActions.Enable();
	}

	private void OnValidate()
	{
		transform.rotation = Quaternion.Normalize(transform.rotation);
	}

	private void LateUpdate()
	{
		if (lookAt == null)
			return;

		float dt = Time.deltaTime;
		Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();
		currentX += lookInput.x * sensitivity * dt;
		currentY = Mathf.Clamp(currentY + lookInput.y * sensitivity * dt, YMin, YMax);

		Quaternion rotation = Quaternion.Normalize(Quaternion.Euler(currentY, currentX, 0f));
		Vector3 offset = rotation * Vector3.back * distance;
		transform.SetPositionAndRotation(lookAt.position + offset, rotation);
	}
}
