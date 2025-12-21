using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenSpawner : MonoBehaviour
{
	public RoomController oxygenRoomPC;

	public GameObject oxygenPrefab;
	public Collider spawnArea;

	public int maxSpawn;
	public int pointsTarget;
	public OxygenBaloon[] baloons;

	private void Update()
	{


		if (!oxygenRoomPC.HasRecource())
		{
			if (baloons.Length == 0)
			{
				SpawnOxygen();
			}

		}

		oxygenRoomPC.AlertMassage(ErrorList());

		if (oxygenRoomPC.isGoingOut && oxygenRoomPC.alertMsg.text == oxygenRoomPC.baseMsg)
		{
			DespawnOxygen();
		}
	}

	public void DespawnOxygen()
	{
		foreach (Transform item in transform)
		{
			Destroy(item.gameObject);
		}
		baloons = new OxygenBaloon[0];
	}

	public void SpawnOxygen()
	{
		maxSpawn = oxygenRoomPC.myTank.level + 3;
		pointsTarget = oxygenRoomPC.myTank.level * 3;
		baloons = new OxygenBaloon[maxSpawn];

		for (int i = 0; i < maxSpawn; i++)
		{
			Vector3 randomPos = new Vector3(Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
											Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y),
											Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z));

			baloons[i] = Instantiate(oxygenPrefab, transform).GetComponent<OxygenBaloon>();
			baloons[i].gameObject.transform.position = randomPos;
		}
	}

	public List<string> ErrorList()
	{
		List<string> names = new List<string>();
		string title = "Alert!! Can't fill storage!\nPlease Add Oxygen to the container...\n.....\n";
		names.Add(title);

		if (pointsTarget > 0)//more score to reach
		{

			names.Add("still error...");
			names.Add("missing more " + pointsTarget + "Kg");

		}
		else
		{
			if (names.Count > 1)
			{
				for (int i = 1; i < name.Length; i++)
				{
					names.RemoveAt(i);
				}

			}
		}



		return names;
	}


}
