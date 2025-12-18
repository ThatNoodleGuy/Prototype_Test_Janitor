using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenRoomPoints : MonoBehaviour
{
	public GeneratePickups generatePickups;
	public int points;
	int random;

	public void Awake()
	{
		generatePickups = FindObjectOfType<GeneratePickups>();

		random = Random.Range(3, 6);

		transform.localScale = new Vector3(random, random, random);
		points = random;
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "TrashBox")
		{
			generatePickups.points += points;
			Destroy(gameObject, 0.3f);
		}
	}
}
