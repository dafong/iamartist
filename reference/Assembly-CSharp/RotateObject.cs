using UnityEngine;

public class RotateObject : MonoBehaviour
{
	public Vector3 RotVelocity;

	private void Update()
	{
		base.transform.Rotate(RotVelocity * Time.deltaTime);
	}
}
