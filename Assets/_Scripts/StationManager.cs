using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationManager : Singleton<StationManager>
{
    [Header("Resources")]
    [SerializeField] private float powerAmount;
    [SerializeField] private float oxygenAmount;
    [SerializeField] private float points;

    [Header("Storage References")]
    [SerializeField] private Storage powerStorage;
    [SerializeField] private Storage oxygenStorage;

    [Header("Station References")]
    [SerializeField] private WorkStation workStation;
    [SerializeField] private RoomController[] rooms;

    [Header("Audio")]
    [SerializeField] private AudioSource workstationAudioSource;
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioClip workstationOff;
    [SerializeField] private AudioClip upgradeBuilding;
    [SerializeField] private AudioClip upgradeGear;
    [SerializeField] private AudioClip upgradeFail;
    [SerializeField] private AudioClip clickUI;

    [Header("Monitor UI (Main Screen)")]
    [SerializeField] private Button viewBtn;
    [SerializeField] private GameObject homeUI;
    [SerializeField] private GameObject storeUI;
    [SerializeField] private Button StartBtnUI;
    [SerializeField] private GameObject workingIcon;
    [SerializeField] private Text scoreUI;
    [SerializeField] private Text powerTextUI;
    [SerializeField] private Text oxygenTextUI;
    [SerializeField] private Text workstationLvlMain;
    [SerializeField] private Text workstationCurrproductionMain;

    [Header("Store UI: SpaceShip")]
    [SerializeField] private Text powerStorageLvl;
    [SerializeField] private Text powerCurrAmount;
    [SerializeField] private Text powerNextLvlAmount;
    [SerializeField] private Text powerUpgradeCost;
    [SerializeField] private Text oxygenStorageLvl;
    [SerializeField] private Text oxygenCurrAmount;
    [SerializeField] private Text oxygenNextLvlAmount;
    [SerializeField] private Text oxygenUpgradeCost;
    [SerializeField] private Text workstationLvl;
    [SerializeField] private Text workstationCurrproduction;
    [SerializeField] private Text workstationNextLvlproduction;
    [SerializeField] private Text workStationUpgradeCost;

    [Header("Store UI: Player Gear")]
    [SerializeField] private Text healCostText;
    [SerializeField] private Text maskLvl;
    [SerializeField] private Text maskCostText;
    [SerializeField] private Text timeInRooms;
    [SerializeField] private Text oxygenBaloonCost;
    [SerializeField] private Text oxygenLvl;

    // State tracking
    [SerializeField] private bool isHomeScreen = true;
    private bool hasPlayedWorkstationOff;

    // Player references
    private Mask mask;
    private PlayerOxygen playerOxygen;
    private PlayerHealth playerHealth;

    // ===== PUBLIC PROPERTIES (GETTERS/SETTERS) =====
    
    // Resource Properties
    public float PowerAmount 
    { 
        get => powerAmount; 
        set => powerAmount = value; 
    }
    
    public float OxygenAmount 
    { 
        get => oxygenAmount; 
        set => oxygenAmount = value; 
    }
    
    public float Points 
    { 
        get => points; 
        set 
        {
            points = value;
            // Optional: Add validation or events here
            // e.g., points = Mathf.Max(0, value);
        }
    }

    // Storage References (Read-only - only getters)
    public Storage PowerStorage => powerStorage;
    public Storage OxygenStorage => oxygenStorage;

    // Station References
    public WorkStation WorkStation => workStation;
    public RoomController[] Rooms => rooms;

    // Audio References (Read-only)
    public AudioSource WorkstationAudioSource => workstationAudioSource;
    public AudioSource PlayerAudioSource => playerAudioSource;

    // State Properties
    public bool IsHomeScreen 
    { 
        get => isHomeScreen; 
        set => isHomeScreen = value; 
    }

    public bool HasPlayedWorkstationOff 
    { 
        get => hasPlayedWorkstationOff; 
        set => hasPlayedWorkstationOff = value; 
    }

    // Player References (Read-only)
    public Mask Mask => mask;
    public PlayerOxygen PlayerOxygen => playerOxygen;
    public PlayerHealth PlayerHealth => playerHealth;

    // ===== END PROPERTIES =====

    void Start()
    {
        #if UNITY_6000_0_OR_NEWER
            playerOxygen = FindAnyObjectByType<PlayerOxygen>();
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            mask = FindAnyObjectByType<Mask>();
        #else
            playerOxygen = FindObjectOfType<PlayerOxygen>();
            playerHealth = FindObjectOfType<PlayerHealth>();
            mask = FindObjectOfType<Mask>();
        #endif

        // Subscribe to events
        GameEvents.OnResourceChanged += CheckWorkStatus;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
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

    // === HELPER METHODS FOR EXTERNAL ACCESS ===
    
    /// <summary>
    /// Add points to the player's balance
    /// </summary>
    public void AddPoints(float amount)
    {
        points += amount;
    }

    /// <summary>
    /// Remove points from the player's balance. Returns true if successful.
    /// </summary>
    public bool TrySpendPoints(float amount)
    {
        if (points >= amount)
        {
            points -= amount;
            return true;
        }
        return false;
    }
}