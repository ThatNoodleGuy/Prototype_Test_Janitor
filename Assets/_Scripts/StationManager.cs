using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationManager : MonoBehaviour
{
    public static StationManager Instance;

    [Header("Resources")]
    public float powerAmount;
    public float oxygenAmount;
    public float points;

    [Header("Storage References")]
    public Storage powerStorage;
    public Storage oxygenStorage;

    [Header("Station References")]
    public WorkStation workStation;
    public RoomController[] rooms;

    [Header("Audio")]
    public AudioSource workstationAudioSource;
    public AudioSource playerAudioSource;
    public AudioClip workstationOff;
    public AudioClip upgradeBuilding;
    public AudioClip upgradeGear;
    public AudioClip upgradeFail;
    public AudioClip clickUI;

    [Header("Monitor UI (Main Screen)")]
    public Button viewBtn;
    public GameObject homeUI;
    public GameObject storeUI;
    public Button StartBtnUI;
    public GameObject workingIcon;
    public Text scoreUI;
    public Text powerTextUI;
    public Text oxygenTextUI;
    public Text workstationLvlMain;
    public Text workstationCurrproductionMain;

    [Header("Store UI: SpaceShip")]
    public Text powerStorageLvl;
    public Text powerCurrAmount;
    public Text powerNextLvlAmount;
    public Text powerUpgradeCost;
    public Text oxygenStorageLvl;
    public Text oxygenCurrAmount;
    public Text oxygenNextLvlAmount;
    public Text oxygenUpgradeCost;
    public Text workstationLvl;
    public Text workstationCurrproduction;
    public Text workstationNextLvlproduction;
    public Text workStationUpgradeCost;

    [Header("Store UI: Player Gear")]
    public Text healCostText;
    public Text maskLvl;
    public Text maskCostText;
    public Text timeInRooms;
    public Text oxygenBaloonCost;
    public Text oxygenLvl;

    // State tracking
    public bool isHomeScreen = true;
    private bool hasPlayedWorkstationOff;

    // Player references
    private Mask mask;
    private PlayerOxygen playerOxygen;
    private PlayerHealth playerHealth;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        playerOxygen = FindObjectOfType<PlayerOxygen>();
        playerHealth = FindObjectOfType<PlayerHealth>();
        mask = FindObjectOfType<Mask>();

        // Subscribe to events
        GameEvents.OnResourceChanged += CheckWorkStatus;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        GameEvents.OnResourceChanged -= CheckWorkStatus;
    }

    void Update()
    {
        // Update resources from storage
        if (powerStorage != null)
            powerAmount = powerStorage.amount;
        
        if (oxygenStorage != null)
            oxygenAmount = oxygenStorage.amount;

        // Update UI based on current view
        if (scoreUI != null)
            scoreUI.text = "Balance: " + points.ToString("#,###") + "$";

        if (isHomeScreen)
        {
            HomeUI();
        }
        else
        {
            StoreUI();
        }

        // Stop machines if can't work
        if (!CanWork())
        {
            CancelInvoke(nameof(StartMining));
            if (StartBtnUI != null)
                StartBtnUI.interactable = true;
            if (workingIcon != null)
                workingIcon.SetActive(false);

            if (!hasPlayedWorkstationOff && workstationOff != null && playerAudioSource != null)
                playerAudioSource.PlayOneShot(workstationOff);
            hasPlayedWorkstationOff = true;
            
            if (workstationAudioSource != null)
                workstationAudioSource.enabled = false;
        }
    }

    // === UI METHODS ===

    public void SwitchViews()
    {
        isHomeScreen = !isHomeScreen;
        if (playerAudioSource != null && clickUI != null)
            playerAudioSource.PlayOneShot(clickUI);
    }

    public void HomeUI()
    {
        if (homeUI != null)
            homeUI.SetActive(true);
        if (storeUI != null)
            storeUI.SetActive(false);

        // Update home screen UI
        if (powerTextUI != null && powerStorage != null)
            powerTextUI.text = (powerStorage.amountPerc * 100).ToString("0") + "%";
        
        if (oxygenTextUI != null && oxygenStorage != null)
            oxygenTextUI.text = (oxygenStorage.amountPerc * 100).ToString("0") + "%";
        
        if (workstationLvlMain != null && workStation != null)
            workstationLvlMain.text = "Work Station Lvl: " + workStation.level;
        
        if (workstationCurrproductionMain != null && workStation != null)
            workstationCurrproductionMain.text = "Current Production Rate: " + workStation.addPoints.ToString("0") + "$/sec";
    }

    public void StoreUI()
    {
        if (homeUI != null)
            homeUI.SetActive(false);
        if (storeUI != null)
            storeUI.SetActive(true);

        // Update storage info
        if (powerStorage != null)
        {
            if (powerStorageLvl != null)
                powerStorageLvl.text = "Power\nLvl: " + powerStorage.level;
            if (powerUpgradeCost != null)
                powerUpgradeCost.text = "Cost: " + powerStorage.upgradeCost.ToString("0") + "$";
            if (powerCurrAmount != null)
                powerCurrAmount.text = "Storage capacity: " + powerStorage.maxAmount.ToString("0");
            if (powerNextLvlAmount != null)
                powerNextLvlAmount.text = "Upgrade capacity: " + (powerStorage.maxAmount * powerStorage.upgradePerc * powerStorage.level).ToString("0");
        }

        if (oxygenStorage != null)
        {
            if (oxygenStorageLvl != null)
                oxygenStorageLvl.text = "O2\nLvl: " + oxygenStorage.level;
            if (oxygenUpgradeCost != null)
                oxygenUpgradeCost.text = "Cost: " + oxygenStorage.upgradeCost.ToString("0") + "$";
            if (oxygenCurrAmount != null)
                oxygenCurrAmount.text = "Storage capacity: " + oxygenStorage.maxAmount.ToString("0");
            if (oxygenNextLvlAmount != null)
                oxygenNextLvlAmount.text = "Upgrade capacity: " + (oxygenStorage.maxAmount * oxygenStorage.upgradePerc * oxygenStorage.level).ToString("0");
        }

        if (workStation != null)
        {
            if (workstationLvl != null)
                workstationLvl.text = "Work Station\nLvl: " + workStation.level;
            if (workStationUpgradeCost != null)
                workStationUpgradeCost.text = "Cost: " + workStation.upgradeCost.ToString("0") + "$";
            if (workstationCurrproduction != null)
                workstationCurrproduction.text = "Production: " + workStation.addPoints.ToString("0") + "$/sec";
            if (workstationNextLvlproduction != null)
                workstationNextLvlproduction.text = "Upgrade Production: " + (workStation.addPoints * workStation.upgradePerc).ToString("0") + "$/sec";
        }

        // Update player gear info
        if (playerHealth != null && healCostText != null)
            healCostText.text = "Cost: " + playerHealth.missingHealth.ToString("0") + "$";

        if (mask != null)
        {
            if (maskCostText != null)
                maskCostText.text = "Cost: " + mask.upgradeCost.ToString("0");
            if (timeInRooms != null)
                timeInRooms.text = "Room time: " + mask.roomTimer.ToString() + "s";
            if (maskLvl != null)
                maskLvl.text = "Gas Mask\nLvl" + mask.level;
        }

        if (playerOxygen != null)
        {
            if (oxygenBaloonCost != null)
                oxygenBaloonCost.text = "Cost: " + playerOxygen.upgradeCost + "$";
            if (oxygenLvl != null)
                oxygenLvl.text = "Oxygen Baloon\nLvl" + playerOxygen.level;
        }
    }

    // === WORK MANAGEMENT ===

    public bool CanWork()
    {
        if (workStation == null) return false;

        // Check if all rooms have enough resources
        if (rooms != null)
        {
            foreach (RoomController room in rooms)
            {
                if (room != null && !room.HasRecource())
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void CheckWorkStatus()
    {
        // This gets called by events when resources change
        if (!CanWork())
        {
            StopWorking();
        }
    }

    public void PowerBtn()
    {
        if (CanWork())
        {
            if (workstationAudioSource != null)
                workstationAudioSource.enabled = true;
            hasPlayedWorkstationOff = false;
            
            InvokeRepeating(nameof(StartMining), 1, 1);
            
            if (StartBtnUI != null)
                StartBtnUI.interactable = false;
            if (workingIcon != null)
                workingIcon.SetActive(true);
        }
        if (playerAudioSource != null && clickUI != null)
            playerAudioSource.PlayOneShot(clickUI);
    }

    public void StartMining()
    {
        if (workStation != null)
            workStation.Work();
    }

    public void StopWorking()
    {
        CancelInvoke(nameof(StartMining));
        if (StartBtnUI != null)
            StartBtnUI.interactable = true;
        if (workingIcon != null)
            workingIcon.SetActive(false);
    }

    // === UPGRADE METHODS ===

    public void UpgradePowerStorage()
    {
        if (powerStorage != null && points >= powerStorage.upgradeCost)
        {
            powerStorage.UpgradeAndFillStorage();
            points -= powerStorage.upgradeCost;
            if (playerAudioSource != null && upgradeBuilding != null)
                playerAudioSource.PlayOneShot(upgradeBuilding);
            GameEvents.TriggerResourceChanged();
        }
        else
        {
            if (playerAudioSource != null && upgradeFail != null)
                playerAudioSource.PlayOneShot(upgradeFail);
        }
    }

    public void UpgradeOxygenStorage()
    {
        if (oxygenStorage != null && points >= oxygenStorage.upgradeCost)
        {
            oxygenStorage.UpgradeAndFillStorage();
            points -= oxygenStorage.upgradeCost;
            if (playerAudioSource != null && upgradeBuilding != null)
                playerAudioSource.PlayOneShot(upgradeBuilding);
            GameEvents.TriggerResourceChanged();
        }
        else
        {
            if (playerAudioSource != null && upgradeFail != null)
                playerAudioSource.PlayOneShot(upgradeFail);
        }
    }

    public void UpgradeWorkStation()
    {
        if (workStation != null && points >= workStation.upgradeCost)
        {
            workStation.UpgradeWorkStation();
            points -= workStation.upgradeCost;
            if (playerAudioSource != null && upgradeBuilding != null)
                playerAudioSource.PlayOneShot(upgradeBuilding);
        }
        else
        {
            if (playerAudioSource != null && upgradeFail != null)
                playerAudioSource.PlayOneShot(upgradeFail);
        }
    }

    public void Heal()
    {
        if (playerHealth != null && points >= playerHealth.missingHealth)
        {
            playerHealth.currentHealth = playerHealth.healthAmount;
            points -= playerHealth.missingHealth;
            if (playerAudioSource != null && upgradeGear != null)
                playerAudioSource.PlayOneShot(upgradeGear);
        }
        else
        {
            if (playerAudioSource != null && upgradeFail != null)
                playerAudioSource.PlayOneShot(upgradeFail);
        }
    }

    public void UpgradeBaloon()
    {
        if (playerOxygen != null && points >= playerOxygen.upgradeCost)
        {
            playerOxygen.UpgardeMaxCapacity();
            points -= playerOxygen.upgradeCost;
            if (playerAudioSource != null && upgradeGear != null)
                playerAudioSource.PlayOneShot(upgradeGear);
        }
        else
        {
            if (playerAudioSource != null && upgradeFail != null)
                playerAudioSource.PlayOneShot(upgradeFail);
        }
    }

    public void UpgradeMask()
    {
        if (mask != null && points >= mask.upgradeCost)
        {
            mask.UpgradeMask();
            points -= mask.upgradeCost;
            if (playerAudioSource != null && upgradeGear != null)
                playerAudioSource.PlayOneShot(upgradeGear);
        }
        else
        {
            if (playerAudioSource != null && upgradeFail != null)
                playerAudioSource.PlayOneShot(upgradeFail);
        }
    }
}
