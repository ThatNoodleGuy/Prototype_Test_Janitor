using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Individual oxygen tank - Can be picked up and moved
/// Uses physics for natural interaction
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class OxygenTank : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer tankRenderer;
    
    [Header("Physics")]
    [SerializeField] private float pickupDistance = 2f;
    [SerializeField] private float throwForce = 5f;
    
    private int tankIndex;
    private Rigidbody rb;
    private bool isHeld = false;
    private Transform playerCamera;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Configure rigidbody
        if (rb != null)
        {
            rb.mass = 2f;
            rb.linearDamping = 1f;
            rb.angularDamping = 0.5f;
        }
    }
    
    void Update()
    {
        if (isHeld)
        {
            UpdateHeldPosition();
            
            // Drop if click again
            if (Input.GetMouseButtonDown(0))
            {
                Drop();
            }
            
            // Throw if right click
            if (Input.GetMouseButtonDown(1))
            {
                Throw();
            }
        }
    }
    
    /// <summary>
    /// Initialize the tank
    /// </summary>
    public void Initialize(int index, Material material)
    {
        tankIndex = index;
        
        if (tankRenderer != null && material != null)
        {
            tankRenderer.material = material;
        }
    }
    
    /// <summary>
    /// Pick up the tank
    /// </summary>
    public void PickUp(Transform camera)
    {
        if (isHeld) return;
        
        isHeld = true;
        playerCamera = camera;
        
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    
    /// <summary>
    /// Drop the tank
    /// </summary>
    public void Drop()
    {
        if (!isHeld) return;
        
        isHeld = false;
        playerCamera = null;
        
        if (rb != null)
        {
            rb.useGravity = true;
        }
    }
    
    /// <summary>
    /// Throw the tank forward
    /// </summary>
    public void Throw()
    {
        if (!isHeld || playerCamera == null) return;
        
        isHeld = false;
        
        if (rb != null)
        {
            rb.useGravity = true;
            rb.AddForce(playerCamera.forward * throwForce, ForceMode.VelocityChange);
        }
        
        playerCamera = null;
    }
    
    /// <summary>
    /// Update position when held
    /// </summary>
    void UpdateHeldPosition()
    {
        if (playerCamera == null) return;
        
        // Position in front of camera
        Vector3 targetPos = playerCamera.position + playerCamera.forward * pickupDistance;
        
        if (rb != null)
        {
            // Smooth movement
            rb.MovePosition(Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f));
            
            // Face camera
            Quaternion targetRot = Quaternion.LookRotation(playerCamera.forward);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f));
        }
        else
        {
            transform.position = targetPos;
        }
    }
    
    /// <summary>
    /// Check if player can interact
    /// </summary>
    public bool CanInteract(Vector3 playerPos)
    {
        float distance = Vector3.Distance(transform.position, playerPos);
        return distance <= pickupDistance && !isHeld;
    }
}