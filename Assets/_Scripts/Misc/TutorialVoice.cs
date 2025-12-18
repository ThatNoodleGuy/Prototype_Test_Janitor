using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialVoice : MonoBehaviour
{
	public Coroutine voice;

	public GameObject tutorailPanel;

	public Text subtitlesText;
	public AudioClip[] tutorial;

	public AudioSource playerAudioSource;
	public AudioSource musicBG;

	public float timeBetweenLines;

	string intro = "Hello.\nWelcome to the research station.";
	string mainObjective = "In this room is the main computer from which you can run the research machines and make money.\nYou can return home only after earning $ 100,000.";
	string job = "Our sophisticated machines consume much electricity to operate.\nIn addition the machines use oxygen as fuel.\nYour job is to make sure there is always enough electricity and oxygen in the storages for the station to continue working.";
	string solvingPreoblems = "If a particular resource runs out you can go into the room where it is stored and see on the computer of that room what the problem is.\nAfter you solve the problem in the room the storages can be filled through the room's computer.";
	string infaction = "Another thing, the other rooms at the station are full of pollution.\nYou can only be inside them for a limited time until the infection becomes fatal and will kill you.\nYou can upgrade your mask to extend the time allowed in these rooms.";
	string store = "Upgrades of personal equipment and of the various parts of the station are available in the store.\nThe store is on the computer in the main room.";
	string lastTip = "Keep the storages full.\nKeep the machines working at all times.\nAnd do not forget that you also need oxygen yorself.";
	string goodluck = "Good luck and godspeed.";

	string[] subtitles;
	public bool playIntro = true;
	string loading;
	private void Start()
	{
		loading = subtitlesText.text;
		subtitles = new string[8] { intro, mainObjective, job, solvingPreoblems, infaction, store, lastTip, goodluck };

		if (playIntro)
			StartTutorial();
	}

	public void StartTutorial()
	{
		if (voice == null)
			voice = StartCoroutine(Voice(tutorial, subtitles));
	}

	public void StopAll()
	{
		if (voice != null)
			StopCoroutine(voice);

		subtitlesText.text = "";
		voice = null;
		tutorailPanel.SetActive(false);
		playerAudioSource.Stop();
		subtitlesText.text = loading;
		musicBG.volume = tempVol;
	}

	public void StartPartTutorial(int i)
	{
		if (voice == null)
			voice = StartCoroutine(PartVoice(i));
	}

	float tempVol;
	IEnumerator PartVoice(int i)
	{
		char[] charArr = subtitles[i].ToCharArray();
		tutorailPanel.SetActive(true);
		tempVol = musicBG.volume;
		musicBG.volume = 0.1f;

		yield return new WaitForSeconds(timeBetweenLines);
		subtitlesText.text = "";
		playerAudioSource.PlayOneShot(tutorial[i]);
		for (int j = 0; j < charArr.Length; j++)
		{
			subtitlesText.text += charArr[j];
			yield return new WaitForSeconds(0.04f);
		}
		yield return new WaitForSeconds(4);

		tutorailPanel.SetActive(false);
		musicBG.volume = tempVol;

		StopAll();
	}

	IEnumerator Voice(AudioClip[] lines, string[] subs)
	{
		tutorailPanel.SetActive(true);
		tempVol = musicBG.volume;
		musicBG.volume = tempVol / 3;
		yield return new WaitForSeconds(timeBetweenLines);
		subtitlesText.text = "";

		for (int i = 0; i < lines.Length; i++)
		{
			yield return new WaitForSeconds(timeBetweenLines);
			playerAudioSource.PlayOneShot(lines[i]);
			subtitlesText.text = subs[i];

			yield return new WaitForSeconds(lines[i].length);
		}

		tutorailPanel.SetActive(false);
		musicBG.volume = tempVol;

		StopAll();
	}
}
