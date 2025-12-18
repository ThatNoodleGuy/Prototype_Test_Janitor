using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricFuse : MonoBehaviour
{
	AudioSource audioSource;
	public AudioClip zap;
	public float shockDmg;

	GameObject playerHealth;

	public bool onOff;

	public bool correctState = true;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
	}
	public void ToggleSwitch()
	{
		onOff = !onOff;
		correctState = !correctState;

		if (!correctState)
		{
			playerHealth.GetComponent<PlayerHealth>().takeDamage(shockDmg);
			audioSource.PlayOneShot(zap);
		}
	}

	private void Update()
	{
		playerHealth = GameObject.Find("Player");//.GetComponent<PlayerHealth>();

		if (onOff)
		{
			transform.rotation = Quaternion.Euler(new Vector3(45, 0, 0));
		}
		else
		{
			transform.rotation = Quaternion.Euler(new Vector3(-45, 0, 0));
		}
	}
}
