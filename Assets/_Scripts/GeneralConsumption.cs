using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralConsumption : MonoBehaviour
{
	public bool usePassiveO2;
	public bool usePassivePower;
	public GameObject[] lights;
	StationManager stationManager;
	public float breatheDrain;
	public float powerDrain;



	private void Start()
	{
		lights = GameObject.FindGameObjectsWithTag("RoomLight");
		stationManager = StationManager.Instance;
	}

	private void Update()
	{
		if (usePassiveO2)
		{
			Breath();
		}

		if (usePassivePower)
		{
			UsePower();
		}

		if (StationManager.Instance.PowerStorage.amount <= 0)
		{
			LightsOff();
		}
		else
		{
			LightsOn();
		}

	}
	public void Breath()
	{
		StationManager.Instance.OxygenStorage.amount -= Time.deltaTime * breatheDrain;
	}

	public void UsePower()
	{
		StationManager.Instance.PowerStorage.amount -= Time.deltaTime * powerDrain * lights.Length;
	}

	public void LightsOn()
	{
		foreach (var item in lights)
		{
			item.SetActive(true);
		}
	}
	public void LightsOff()
	{
		foreach (var item in lights)
		{
			item.SetActive(false);
		}
	}
}
