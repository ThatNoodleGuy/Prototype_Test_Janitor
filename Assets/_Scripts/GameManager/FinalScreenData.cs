using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalScreenData : MonoBehaviour
{
    [Header("TextMeshPro Objects")]
    public TextMeshProUGUI powerStorageLvl = null;
    public TextMeshProUGUI oxygenStorageLvl = null;
    public TextMeshProUGUI workstationLvl = null;
    public TextMeshProUGUI maskLvl = null;
    public TextMeshProUGUI oxygenBalloonLvl = null;
    public TextMeshProUGUI balanceText = null;

    [Header("GameObjects")]
    public StationManager stationManager;
    public Storage powerStorage;
    public Storage oxygenStorage;
    public WorkStation workStation;
    public Mask mask;
    public PlayerOxygen playerOxygen;


    void Start()
    {
        CollectDataFromLevel();
    }

    public void CollectDataFromLevel()
    {
        powerStorageLvl.text = "Power Storage Lvl:........" + PlayerPrefs.GetInt("powerStorageLvl");

        oxygenStorageLvl.text = "Oxygen Storage Lvl:........" + PlayerPrefs.GetInt("oxygenStorageLvl");

        workstationLvl.text = "Work Station Lvl:........" + PlayerPrefs.GetInt("workStationLvl");

        maskLvl.text = "Gas Mask Lvl:........" + PlayerPrefs.GetInt("maskLvl", mask.level);

        oxygenBalloonLvl.text = "Oxygen Balloon Lvl:........" + PlayerPrefs.GetInt("oxygenBalloonLvl");

        balanceText.text = "Balance:........" + PlayerPrefs.GetInt("balanceText") + '$';
    }
}
