using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer;
    
    [Header("Held Object")]
    [SerializeField] private Transform holdPosition;  // Optional: specific hold point
    
    [Header("UI Feedback")]
    [SerializeField] private UnityEngine.UI.Text interactionPrompt;

    
    private Camera playerCamera;
    private OxygenTank heldTank;
    private bool isHoldingObject => heldTank != null;

    void Start()
    {
        #if UNITY_6000_0_OR_NEWER
            playerCamera = FindAnyObjectByType<PlayerCamera>().GetComponent<Camera>();
        #else
            playerCamera = FindObjectOfType<PlayerCamera>().GetComponent<Camera>();
        #endif
        
        if (interactionPrompt != null)
            interactionPrompt.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isHoldingObject)
        {
            CheckForInteractable();
        }
        
        HandleInput();
    }
    
    /// <summary>
    /// Check if player is looking at an interactable object
    /// </summary>
    void CheckForInteractable()
    {
        if (playerCamera == null) return;
        
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            OxygenTank tank = hit.collider.GetComponent<OxygenTank>();
            
            if (tank != null && tank.CanInteract(transform.position))
            {
                ShowInteractionPrompt("Press E to Pick Up");
                return;
            }
        }
        
        HideInteractionPrompt();
    }
    
    /// <summary>
    /// Handle player input for interaction
    /// </summary>
    void HandleInput()
    {
        // Pick up / interact
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isHoldingObject)
            {
                TryPickUp();
            }
            else
            {
                DropHeldObject();
            }
        }
        
        // Alternative: Left click also works
        if (Input.GetMouseButtonDown(0) && !isHoldingObject)
        {
            TryPickUp();
        }
        
        // Drop with Q or right click
        if (isHoldingObject)
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(1))
            {
                ThrowHeldObject();
            }
        }
    }
    
    /// <summary>
    /// Try to pick up an object
    /// </summary>
    void TryPickUp()
    {
        if (playerCamera == null) return;
        
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            OxygenTank tank = hit.collider.GetComponent<OxygenTank>();
            
            if (tank != null && tank.CanInteract(transform.position))
            {
                PickUpObject(tank);
            }
        }
    }
    
    /// <summary>
    /// Pick up a specific tank
    /// </summary>
    void PickUpObject(OxygenTank tank)
    {
        heldTank = tank;
        
        Transform holdPoint = holdPosition != null ? holdPosition : playerCamera.transform;
        heldTank.PickUp(holdPoint);

        // Update UI
        ShowInteractionPrompt("E/Q to Drop | Right Click to Throw");
    }
    
    /// <summary>
    /// Drop the held object
    /// </summary>
    void DropHeldObject()
    {
        if (heldTank == null) return;
        
        heldTank.Drop();
        heldTank = null;
        
        HideInteractionPrompt();
    }
    
    /// <summary>
    /// Throw the held object
    /// </summary>
    void ThrowHeldObject()
    {
        if (heldTank == null) return;
        
        heldTank.Throw();
        heldTank = null;
        
        HideInteractionPrompt();
    }
    
    /// <summary>
    /// Show interaction UI prompt
    /// </summary>
    void ShowInteractionPrompt(string message)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.text = message;
            interactionPrompt.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide interaction UI prompt
    /// </summary>
    void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Draw interaction range in editor
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionRange);
        }
    }
}










