using UnityEngine;

public class RotateToVelocity : MonoBehaviour
{
	private Vector3 lastPosition;

	private void Start()
	{
		lastPosition = base.transform.position;
	}

	private void Update()
	{
		Vector3 vector = base.transform.position - lastPosition;
		if (vector.sqrMagnitude > 0.0001f)
		{
			float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			base.transform.rotation = Quaternion.Euler(0f, 0f, z);
		}
		lastPosition = base.transform.position;
	}
}
