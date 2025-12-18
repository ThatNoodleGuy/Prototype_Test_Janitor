using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoTool : MonoBehaviour
{
	public GameObject heldObject;
	public AudioClip flick;
	public AudioClip pickup;
	public AudioClip drop;
	public AudioSource playerAudioSource;

	RaycastHit hit;
	public Camera playerCam;
	public Transform player;
	public float reach = 5;

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (heldObject != null)
			{
				DropObject();
				playerAudioSource.PlayOneShot(drop);
			}
			UseHand();
		}

		if (heldObject != null)
		{
			MoveObject();
		}

	}

	public void UseHand()
	{
		if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, reach))
		{
			print(hit.transform.name);
			ElectricFuse fuse = hit.transform.GetComponent<ElectricFuse>();
			if (fuse != null)
			{
				fuse.ToggleSwitch();
				playerAudioSource.PlayOneShot(flick);
			}
			else//pickup
			{
				if (heldObject == null)
				{
					OxygenBaloon baloon = hit.transform.GetComponent<OxygenBaloon>();

					if (baloon != null)
					{
						playerAudioSource.PlayOneShot(pickup);
						heldObject = baloon.gameObject;
					}
				}
			}
		}
	}

	void MoveObject()
	{
		heldObject.transform.position = transform.position;
	}

	void DropObject()
	{
		heldObject = null;
	}

}
