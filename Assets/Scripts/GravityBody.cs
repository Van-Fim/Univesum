using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{

	GravityAttractor planet;
	Rigidbody rigidbody;
	public float maxDistance = 5000;

	void Awake()
	{
		// planet = GameObject.FindGameObjectWithTag("Planet").GetComponent<GravityAttractor>();
		// rigidbody = GetComponent<Rigidbody>();

		// // Disable rigidbody gravity and rotation as this is simulated in GravityAttractor script
		// rigidbody.useGravity = false;
		// rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	}

	void FixedUpdate()
	{
		// float dist = Vector3.Distance(planet.transform.position, rigidbody.transform.position);
		// if (dist <= maxDistance)
		// {
		// 	planet.Attract(rigidbody);
		// 	SuitController sc = rigidbody.gameObject.GetComponent<SuitController>();
		// }
	}
}