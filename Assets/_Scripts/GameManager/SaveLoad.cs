using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveLoad : MonoBehaviour
{
	public static SaveLoad Instance;

	StationManager stationManager;
	GameObject player;

	private void Start()
	{
		stationManager = StationManager.Instance;
	}

	private void Update()
	{
		player = GameObject.Find("Player");
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	[System.Serializable]
	class SaveData
	{
		public int powerLvlSaved;
		public float powerAmmountSaved;
		public int oxygenLvlSaved;
		public float oxygenAmmountSaved;
		public int workstationLvlSaved;
		public float balanceSaved;
		public int maskLvlSaved;
		public int oxygenBaloonLvlSaved;
		public float playerHealthSaved;
		public float playerMovementSaved;
		public float playerMouseSpeedSaved;
		public float musicVolume;
		public float gameVolume;
	}

	public void SaveGame()
	{
		SaveData data = new SaveData();
		data.powerLvlSaved = stationManager.powerStorage.level;
		data.powerAmmountSaved = stationManager.powerStorage.amount;
		data.oxygenLvlSaved = stationManager.oxygenStorage.level;
		data.oxygenAmmountSaved = stationManager.oxygenStorage.amount;
		data.workstationLvlSaved = stationManager.workStation.level;
		data.balanceSaved = stationManager.points;
		data.maskLvlSaved = player.GetComponent<Mask>().level;
		data.oxygenBaloonLvlSaved = player.GetComponent<PlayerOxygen>().level;
		data.playerHealthSaved = player.GetComponent<PlayerHealth>().currentHealth;
		data.playerMovementSaved = player.GetComponent<PlayerMovement>().movementSpeed;
		data.playerMouseSpeedSaved = player.GetComponent<PlayerMovement>().cameraSpeed;
		data.musicVolume = player.GetComponent<PlayerMovement>().musicBGVolume.value;
		data.gameVolume = player.GetComponent<PlayerMovement>().gameVolume.value;

		string json = JsonUtility.ToJson(data);

		File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
	}

	public void LoadGame()
	{
		string path = Application.persistentDataPath + "/savefile.json";
		if (File.Exists(path))
		{
			string json = File.ReadAllText(path);
			SaveData data = JsonUtility.FromJson<SaveData>(json);

			StartCoroutine(LoadData(data));
		}
	}

	IEnumerator LoadData(SaveData _data)
	{
		yield return new WaitForSeconds(0.6f);
		stationManager.powerStorage.level = _data.powerLvlSaved;
		stationManager.powerStorage.amount = _data.powerAmmountSaved;
		stationManager.oxygenStorage.level = _data.oxygenLvlSaved;
		stationManager.oxygenStorage.amount = _data.oxygenAmmountSaved;
		stationManager.workStation.level = _data.workstationLvlSaved;
		stationManager.points = _data.balanceSaved;
		player.GetComponent<Mask>().level = _data.maskLvlSaved;
		player.GetComponent<PlayerOxygen>().level = _data.oxygenBaloonLvlSaved;
		player.GetComponent<PlayerHealth>().currentHealth = _data.playerHealthSaved;
		//player settings
		player.GetComponent<PlayerMovement>().movementSpeed = _data.playerMovementSaved;
		player.GetComponent<PlayerMovement>().playerSpeed.value = _data.playerMovementSaved;
		player.GetComponent<PlayerMovement>().cameraSpeed = _data.playerMouseSpeedSaved;
		player.GetComponent<PlayerMovement>().mouseSensitivity.value = _data.playerMouseSpeedSaved;
		player.GetComponent<PlayerMovement>().musicBG.volume = _data.musicVolume;
		player.GetComponent<PlayerMovement>().musicBGVolume.value = _data.musicVolume;
		player.GetComponent<PlayerMovement>().gameVolume.value = _data.gameVolume;
		player.GetComponent<PlayerMovement>().SetGameVoluve();
	}

}
