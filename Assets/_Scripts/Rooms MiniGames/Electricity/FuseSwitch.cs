using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Individual 3D fuse switch - Simple, clean, no transform dependencies
/// </summary>
[RequireComponent(typeof(Collider))]
public class FuseSwitch : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer switchRenderer;
    [SerializeField] private Transform switchLever;  // Optional: physical lever to rotate
    
    [Header("Rotation Settings")]
    [SerializeField] private float onRotation = 45f;
    [SerializeField] private float offRotation = -45f;
    [SerializeField] private float rotationSpeed = 10f;
    
    private int switchIndex;
    private bool isOn = false;
    private bool shouldBeOn = false;  // The correct target state
    
    private Material correctMaterial;
    private Material incorrectMaterial;
    private Material currentMaterial;
    
    private Quaternion targetRotation;

    void Update()
    {
        // Smoothly rotate lever if it exists
        if (switchLever != null)
        {
            switchLever.localRotation = Quaternion.Lerp(
                switchLever.localRotation, 
                targetRotation, 
                Time.deltaTime * rotationSpeed
            );
        }
    }
    
    /// <summary>
    /// Initialize the switch
    /// </summary>
    public void Initialize(int index, bool startState, bool isCorrect)
    {
        switchIndex = index;
        isOn = startState;
        shouldBeOn = isCorrect ? startState : !startState;
        
        UpdateVisuals();
    }
    
    /// <summary>
    /// Set the materials for visual feedback
    /// </summary>
    public void SetMaterials(Material correct, Material incorrect)
    {
        correctMaterial = correct;
        incorrectMaterial = incorrect;
        UpdateVisuals();
    }
    
    /// <summary>
    /// Set what the correct state should be
    /// </summary>
    public void SetCorrectState(bool correct)
    {
        // If correct = true, current state IS correct
        // If correct = false, current state is WRONG
        shouldBeOn = correct ? isOn : !isOn;
        UpdateVisuals();
    }
    
    /// <summary>
    /// Toggle the switch state
    /// </summary>
    public void Toggle()
    {
        isOn = !isOn;
        UpdateVisuals();
    }
    
    /// <summary>
    /// Check if switch is in correct state
    /// </summary>
    public bool IsInCorrectState()
    {
        return isOn == shouldBeOn;
    }
    
    /// <summary>
    /// Set solved material (puzzle complete)
    /// </summary>
    public void SetSolvedMaterial(Material solvedMat)
    {
        if (switchRenderer != null && solvedMat != null)
        {
            switchRenderer.material = solvedMat;
        }
    }
    
    /// <summary>
    /// Update visual appearance based on state
    /// </summary>
    void UpdateVisuals()
    {
        // Update material color
        if (switchRenderer != null)
        {
            if (IsInCorrectState() && correctMaterial != null)
            {
                switchRenderer.material = correctMaterial;
            }
            else if (!IsInCorrectState() && incorrectMaterial != null)
            {
                switchRenderer.material = incorrectMaterial;
            }
        }
        
        // Update rotation
        if (switchLever != null)
        {
            float angle = isOn ? onRotation : offRotation;
            targetRotation = Quaternion.Euler(angle, 0, 0);
        }
        else
        {
            // If no lever, rotate the whole object
            float angle = isOn ? onRotation : offRotation;
            targetRotation = Quaternion.Euler(angle, 0, 0);
            transform.localRotation = targetRotation;
        }
    }
}