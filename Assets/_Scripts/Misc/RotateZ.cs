using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateZ : MonoBehaviour
{
	//StationManager stationManager;

	float rotateZ;
	public float rotationSpeed;
	// Start is called before the first frame update
	void Start()
	{
		//stationManager = FindObjectOfType<StationManager>();
	}

	// Update is called once per frame
	void Update()
	{
		//if (!stationManager.CanWork())
		//	return;

		rotateZ -= Time.deltaTime * rotationSpeed;
		transform.Rotate(new Vector3(0, 0, -1.5f));

	}
}
