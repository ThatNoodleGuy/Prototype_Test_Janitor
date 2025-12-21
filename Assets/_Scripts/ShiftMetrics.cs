using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks player performance metrics during a single shift.
/// Core to the evaluation-based gameplay loop.
/// </summary>
[System.Serializable]
public class ShiftMetrics
{
    [Header("Task Performance")]
    public int tasksAttempted = 0;      // Rooms entered
    public int tasksCompleted = 0;      // Storages filled
    public int tasksAbandoned = 0;      // Left without completing
    
    [Header("Resource Management")]
    public float resourcesConsumed = 0; // Total resources used
    public float moneyEarned = 0;       // Total money made
    
    [Header("Time Tracking")]
    public float shiftStartTime = 0;
    public float shiftEndTime = 0;
    
    [Header("Risk & Safety")]
    public int contaminationEvents = 0; // Timer hit 0
    public float healthLost = 0;        // Damage taken
    
    // Private tracking
    private HashSet<RoomController.RoomType> attemptedRooms = new HashSet<RoomController.RoomType>();
    private HashSet<RoomController.RoomType> completedRooms = new HashSet<RoomController.RoomType>();

    /// <summary>
    /// Initialize a new shift
    /// </summary>
    public void StartShift()
    {
        shiftStartTime = Time.time;
        shiftEndTime = 0;
        
        tasksAttempted = 0;
        tasksCompleted = 0;
        tasksAbandoned = 0;
        
        resourcesConsumed = 0;
        moneyEarned = 0;
        
        contaminationEvents = 0;
        healthLost = 0;
        
        attemptedRooms.Clear();
        completedRooms.Clear();
        
        Debug.Log("[ShiftMetrics] Shift started at " + shiftStartTime);
    }
    
    /// <summary>
    /// Finalize shift duration
    /// </summary>
    public void EndShift()
    {
        shiftEndTime = Time.time;
        Debug.Log("[ShiftMetrics] Shift ended. Duration: " + GetShiftDuration() + "s");
    }
    
    /// <summary>
    /// Record when player enters a room
    /// </summary>
    public void RecordRoomEntered(RoomController.RoomType roomType)
    {
        if (!attemptedRooms.Contains(roomType))
        {
            attemptedRooms.Add(roomType);
            tasksAttempted++;
            Debug.Log($"[ShiftMetrics] Task attempted: {roomType}");
        }
    }
    
    /// <summary>
    /// Record when player completes a room (fills storage)
    /// </summary>
    public void RecordRoomCompleted(RoomController.RoomType roomType)
    {
        if (!completedRooms.Contains(roomType))
        {
            completedRooms.Add(roomType);
            tasksCompleted++;
            Debug.Log($"[ShiftMetrics] Task completed: {roomType}");
        }
    }
    
    /// <summary>
    /// Record when player abandons a room
    /// </summary>
    public void RecordRoomAbandoned(RoomController.RoomType roomType)
    {
        // Only count as abandoned if it was attempted but not completed
        if (attemptedRooms.Contains(roomType) && !completedRooms.Contains(roomType))
        {
            tasksAbandoned++;
            Debug.Log($"[ShiftMetrics] Task abandoned: {roomType}");
        }
    }
    
    /// <summary>
    /// Record contamination event (timer hit 0 while player in room)
    /// </summary>
    public void RecordContaminationEvent()
    {
        contaminationEvents++;
        Debug.Log($"[ShiftMetrics] Contamination event! Total: {contaminationEvents}");
    }
    
    /// <summary>
    /// Record health damage
    /// </summary>
    public void RecordHealthLoss(float damage)
    {
        healthLost += damage;
    }
    
    /// <summary>
    /// Record resources consumed
    /// </summary>
    public void RecordResourcesUsed(float amount)
    {
        resourcesConsumed += amount;
    }
    
    /// <summary>
    /// Record money earned
    /// </summary>
    public void RecordMoneyEarned(float amount)
    {
        moneyEarned += amount;
    }
    
    // ===== CALCULATIONS =====
    
    /// <summary>
    /// Get total shift duration in seconds
    /// </summary>
    public float GetShiftDuration()
    {
        if (shiftEndTime > 0)
            return shiftEndTime - shiftStartTime;
        else
            return Time.time - shiftStartTime;  // Ongoing shift
    }
    
    /// <summary>
    /// Get completion rate (0-1)
    /// </summary>
    public float GetCompletionRate()
    {
        if (tasksAttempted <= 0) return 0;
        return (float)tasksCompleted / tasksAttempted;
    }
    
    /// <summary>
    /// Get abandonment rate (0-1)
    /// </summary>
    public float GetAbandonmentRate()
    {
        if (tasksAttempted <= 0) return 0;
        return (float)tasksAbandoned / tasksAttempted;
    }
    
    /// <summary>
    /// Get resource efficiency (money earned per resource consumed)
    /// </summary>
    public float GetEfficiency()
    {
        if (resourcesConsumed <= 0) return 0;
        return (moneyEarned / resourcesConsumed) * 100f;
    }
    
    /// <summary>
    /// Get performance classification label
    /// This is the AI's judgment of the player's work
    /// </summary>
    public string GetClassification()
    {
        float completionRate = GetCompletionRate();
        float efficiency = GetEfficiency();
        
        // Perfect performance
        if (completionRate >= 1.0f && efficiency >= 300f && contaminationEvents == 0)
            return "EXEMPLARY OPERATOR";
        
        // High performance
        if (completionRate >= 0.9f && efficiency >= 250f)
            return "EFFICIENT OPERATOR";
        
        // Good performance
        if (completionRate >= 0.7f && efficiency >= 200f)
            return "ADEQUATE ASSET";
        
        // Acceptable performance
        if (completionRate >= 0.5f && efficiency >= 150f)
            return "ACCEPTABLE PERFORMANCE";
        
        // Below standard
        if (completionRate >= 0.3f)
            return "SUBOPTIMAL BEHAVIOR";
        
        // Very poor
        if (tasksCompleted > 0)
            return "INEFFICIENT PROCESS";
        
        // Did nothing
        return "UNPRODUCTIVE SHIFT";
    }
    
    /// <summary>
    /// Get a detailed evaluation message from the AI
    /// </summary>
    public string GetEvaluationMessage()
    {
        string classification = GetClassification();
        float completionRate = GetCompletionRate() * 100f;
        
        // AI speaks in corporate, emotionless language
        string message = $"Performance classification: {classification}\n\n";
        message += $"Task completion rate: {completionRate:0}%\n";
        message += $"Resource efficiency: {GetEfficiency():0}%\n";
        
        if (contaminationEvents > 0)
            message += $"\nWarning: {contaminationEvents} safety protocols violated.";
        
        if (tasksAbandoned > 0)
            message += $"\nNote: {tasksAbandoned} tasks initiated but not completed.";
        
        return message;
    }
}