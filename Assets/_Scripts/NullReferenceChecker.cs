using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TEMPORARY DEBUG SCRIPT
/// Add this to MainRoomPC GameObject to find all null references
/// Remove it once everything is working!
/// </summary>
public class NullReferenceChecker : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== STARTING NULL REFERENCE CHECK ===");
        
        StationManager sm = GetComponent<StationManager>();
        if (sm == null)
        {
            Debug.LogError("StationManager component not found!");
            return;
        }

        Debug.Log("StationManager found. Checking references...");
        
        // Critical references
        CheckReference(sm.powerStorage, "powerStorage", true);
        CheckReference(sm.oxygenStorage, "oxygenStorage", true);
        CheckReference(sm.workStation, "workStation", true);
        CheckReference(sm.playerAudioSource, "playerAudioSource", true);
        
        if (sm.rooms == null || sm.rooms.Length == 0)
            Debug.LogError("❌ CRITICAL: rooms array is empty or null!");
        else
        {
            Debug.Log($"✓ rooms array has {sm.rooms.Length} elements");
            for (int i = 0; i < sm.rooms.Length; i++)
            {
                if (sm.rooms[i] == null)
                    Debug.LogError($"❌ CRITICAL: rooms[{i}] is NULL!");
                else
                    Debug.Log($"✓ rooms[{i}] = {sm.rooms[i].gameObject.name} ({sm.rooms[i].roomType})");
            }
        }

        // Audio references
        CheckReference(sm.workstationAudioSource, "workstationAudioSource", false);
        CheckReference(sm.workstationOff, "workstationOff clip", false);
        CheckReference(sm.upgradeBuilding, "upgradeBuilding clip", false);
        CheckReference(sm.upgradeGear, "upgradeGear clip", false);
        CheckReference(sm.upgradeFail, "upgradeFail clip", false);
        CheckReference(sm.clickUI, "clickUI clip", false);

        // UI references
        CheckReference(sm.viewBtn, "viewBtn", false);
        CheckReference(sm.homeUI, "homeUI", false);
        CheckReference(sm.storeUI, "storeUI", false);
        CheckReference(sm.StartBtnUI, "StartBtnUI", true);
        CheckReference(sm.workingIcon, "workingIcon", false);
        CheckReference(sm.scoreUI, "scoreUI", true);
        CheckReference(sm.powerTextUI, "powerTextUI", false);
        CheckReference(sm.oxygenTextUI, "oxygenTextUI", false);
        CheckReference(sm.workstationLvlMain, "workstationLvlMain", false);
        CheckReference(sm.workstationCurrproductionMain, "workstationCurrproductionMain", false);

        // Store UI - SpaceShip
        Debug.Log("--- Store UI: SpaceShip ---");
        CheckReference(sm.powerStorageLvl, "powerStorageLvl", false);
        CheckReference(sm.powerCurrAmount, "powerCurrAmount", false);
        CheckReference(sm.powerNextLvlAmount, "powerNextLvlAmount", false);
        CheckReference(sm.powerUpgradeCost, "powerUpgradeCost", false);
        CheckReference(sm.oxygenStorageLvl, "oxygenStorageLvl", false);
        CheckReference(sm.oxygenCurrAmount, "oxygenCurrAmount", false);
        CheckReference(sm.oxygenNextLvlAmount, "oxygenNextLvlAmount", false);
        CheckReference(sm.oxygenUpgradeCost, "oxygenUpgradeCost", false);
        CheckReference(sm.workstationLvl, "workstationLvl", false);
        CheckReference(sm.workstationCurrproduction, "workstationCurrproduction", false);
        CheckReference(sm.workstationNextLvlproduction, "workstationNextLvlproduction", false);
        CheckReference(sm.workStationUpgradeCost, "workStationUpgradeCost", false);

        // Store UI - Player Gear
        Debug.Log("--- Store UI: Player Gear ---");
        CheckReference(sm.healCostText, "healCostText", false);
        CheckReference(sm.maskLvl, "maskLvl", false);
        CheckReference(sm.maskCostText, "maskCostText", false);
        CheckReference(sm.timeInRooms, "timeInRooms", false);
        CheckReference(sm.oxygenBaloonCost, "oxygenBaloonCost", false);
        CheckReference(sm.oxygenLvl, "oxygenLvl", false);

        Debug.Log("=== NULL REFERENCE CHECK COMPLETE ===");
        Debug.Log("Check above for any ❌ marks - those need to be assigned!");
        Debug.Log("Red ❌ CRITICAL = will cause crashes");
        Debug.Log("Yellow ⚠ Optional = UI won't update but won't crash");
    }

    void CheckReference(UnityEngine.Object obj, string name, bool critical)
    {
        if (obj == null)
        {
            if (critical)
                Debug.LogError($"❌ CRITICAL: {name} is NULL!");
            else
                Debug.LogWarning($"⚠ Optional: {name} is NULL (UI won't update)");
        }
        else
        {
            Debug.Log($"✓ {name} is assigned");
        }
    }
}