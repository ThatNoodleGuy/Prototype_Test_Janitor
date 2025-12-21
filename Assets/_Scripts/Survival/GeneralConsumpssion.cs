using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralConsumpssion : MonoBehaviour
{
	public AudioSource playerAudioSource;
	public AudioClip powerDown;
	public AudioClip powerOn;
	public AudioClip oxygenDown;
	public AudioClip oxygenOn;
	public AudioClip refillBaloon;

	public bool usePassiveO2;
	public bool usePassivePower;
	public GameObject[] lights;
	StationManager stationManager;
	public float breatheDrain;
	public float powerDrain;

	bool hasPlayedPowerOff;
	bool hasPlayedPowerOn = true;

	bool hasPlayedOxygenOff;
	bool hasPlayedOxygenOn = true;


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
			if (!hasPlayedPowerOff)
			{
				playerAudioSource.PlayOneShot(powerDown);
				hasPlayedPowerOff = true;
			}
			hasPlayedPowerOn = false;
		}
		else
		{
			LightsOn();
			if (!hasPlayedPowerOn)
			{
				playerAudioSource.PlayOneShot(powerOn);
				hasPlayedPowerOn = true;
			}

			hasPlayedPowerOff = false;
		}

		if (StationManager.Instance.OxygenStorage.amount <= 0)
		{
			if (!hasPlayedOxygenOff)
			{
				playerAudioSource.PlayOneShot(oxygenDown);
				hasPlayedOxygenOff = true;
			}
			hasPlayedOxygenOn = false;
		}
		else
		{
			if (!hasPlayedOxygenOn)
			{
				playerAudioSource.PlayOneShot(oxygenOn);
				playerAudioSource.PlayOneShot(refillBaloon);
				hasPlayedOxygenOn = true;
			}

			hasPlayedOxygenOff = false;
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
