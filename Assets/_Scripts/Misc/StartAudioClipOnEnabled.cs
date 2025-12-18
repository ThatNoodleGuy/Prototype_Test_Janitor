using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAudioClipOnEnabled : MonoBehaviour
{
	public AudioSource audioSource;
	private void OnEnable()
	{
		audioSource.Play();
	}
}
