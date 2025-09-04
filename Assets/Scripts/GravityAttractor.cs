using UnityEngine;
using System.Collections;

public class GravityAttractor : MonoBehaviour {
	
	public float gravity = -9.8f;
	public Vector3 gravityUp = new Vector3(0, 1, 0);
	
	
	public void Attract(Rigidbody body)
	{
		Vector3 localUp = body.transform.up;

		// Apply downwards gravity to body
		body.AddForce(gravityUp * gravity);
		// Allign bodies up axis with the centre of planet
		body.rotation = Quaternion.FromToRotation(localUp, gravityUp) * body.rotation;
	}  
}
