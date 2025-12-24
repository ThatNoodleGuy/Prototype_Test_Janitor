using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FuseBoard : MonoBehaviour
{
    [Header("Room Reference")]
    [SerializeField] private RoomController powerRoomController;
    
    [Header("Puzzle Setup")]
    [SerializeField] private GameObject switchPrefab;
    [SerializeField] private Transform switchContainer;  // Parent for organization
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;  // Relative to this GameObject
    [SerializeField] private Vector2 switchSpacing = new Vector2(0.3f, 0.3f);
    
    [Header("Puzzle Settings")]
    [SerializeField] private int baseGridSize = 3;  // 3x3 at level 1
    [SerializeField] private int maxGridSize = 5;   // 5x5 max
    
    [Header("Visual Feedback")]
    [SerializeField] private Material correctMaterial;
    [SerializeField] private Material incorrectMaterial;
    [SerializeField] private Material solvedMaterial;
    
    private List<FuseSwitch> switches = new List<FuseSwitch>();
    private bool puzzleActive = false;
    private bool puzzleSolved = false;

    void Update()
    {
        // Show puzzle when room needs resources
        if (powerRoomController != null && !powerRoomController.HasRecource() && !puzzleActive)
        {
            GeneratePuzzle();
            puzzleActive = true;
            puzzleSolved = false;
        }
        
        // Clear puzzle when player leaves
        if (powerRoomController != null && powerRoomController.isGoingOut && puzzleActive)
        {
            ClearPuzzle();
            puzzleActive = false;
        }
        
        // Check for player interaction
        if (puzzleActive && !puzzleSolved)
        {
            HandlePlayerInput();
        }
    }
    
    /// <summary>
    /// Generate the puzzle based on room level
    /// </summary>
    void GeneratePuzzle()
    {
        ClearPuzzle();
        
        // Calculate grid size
        int level = powerRoomController?.myTank?.level ?? 1;
        int gridSize = Mathf.Clamp(baseGridSize + (level - 1) / 2, baseGridSize, maxGridSize);
        
        // Create switch grid
        int index = 0;
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                // Calculate world position (relative to this transform)
                Vector3 localPos = gridOrigin + new Vector3(
                    col * switchSpacing.x - (gridSize - 1) * switchSpacing.x / 2f,  // Center horizontally
                    row * switchSpacing.y - (gridSize - 1) * switchSpacing.y / 2f,  // Center vertically
                    0
                );
                
                Vector3 worldPos = transform.TransformPoint(localPos);
                
                // Instantiate switch
                GameObject switchObj = Instantiate(switchPrefab, worldPos, transform.rotation, switchContainer != null ? switchContainer : transform);
                switchObj.name = $"Switch_R{row}_C{col}";
                
                FuseSwitch fuseSwitch = switchObj.GetComponent<FuseSwitch>();
                if (fuseSwitch != null)
                {
                    // Random initial state
                    bool startOn = Random.value > 0.5f;
                    fuseSwitch.Initialize(index, startOn, true);  // All start as correct
                    fuseSwitch.SetMaterials(correctMaterial, incorrectMaterial);
                    
                    switches.Add(fuseSwitch);
                }
                
                index++;
            }
        }
        
        // Set some switches to incorrect (puzzle goal)
        int errorCount = Mathf.Clamp(level, 1, switches.Count / 2);
        List<int> incorrectIndices = new List<int>();
        
        while (incorrectIndices.Count < errorCount)
        {
            int randomIndex = Random.Range(0, switches.Count);
            if (!incorrectIndices.Contains(randomIndex))
            {
                incorrectIndices.Add(randomIndex);
                switches[randomIndex].SetCorrectState(false);
            }
        }
        
        Debug.Log($"[FuseBoard] Generated {gridSize}x{gridSize} puzzle with {errorCount} errors");
    }
    
    /// <summary>
    /// Clear all switches
    /// </summary>
    void ClearPuzzle()
    {
        foreach (var sw in switches)
        {
            if (sw != null)
                Destroy(sw.gameObject);
        }
        
        switches.Clear();
    }
    
    /// <summary>
    /// Handle player looking at and clicking switches
    /// </summary>
    void HandlePlayerInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        
        // Raycast from camera
        #if UNITY_6000_0_OR_NEWER
            Camera playerCamera = FindAnyObjectByType<PlayerCamera>().GetComponent<Camera>();
        #else
            Camera playerCamera = FindObjectOfType<PlayerCamera>().GetComponent<Camera>();
        #endif
        
        if (playerCamera == null) return;
        
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 5f))  // 5 meter reach
        {
            FuseSwitch fuseSwitch = hit.collider.GetComponent<FuseSwitch>();
            
            if (fuseSwitch != null && switches.Contains(fuseSwitch))
            {
                // Toggle the switch
                fuseSwitch.Toggle();

                // Check if solved
                CheckSolution();
            }
        }
    }
    
    /// <summary>
    /// Check if puzzle is solved
    /// </summary>
    void CheckSolution()
    {
        bool allCorrect = true;
        
        foreach (var sw in switches)
        {
            if (sw != null && !sw.IsInCorrectState())
            {
                allCorrect = false;
                break;
            }
        }
        
        if (allCorrect)
        {
            OnPuzzleSolved();
        }
    }
    
    /// <summary>
    /// Called when puzzle is solved
    /// </summary>
    void OnPuzzleSolved()
    {
        puzzleSolved = true;
        
        // Visual feedback - all switches turn solved color
        foreach (var sw in switches)
        {
            if (sw != null && solvedMaterial != null)
            {
                sw.SetSolvedMaterial(solvedMaterial);
            }
        }

        // Fill the room storage
        if (powerRoomController != null)
        {
            powerRoomController.FillStorage();
        }
        
        Debug.Log("[FuseBoard] Puzzle solved!");
    }
    
    /// <summary>
    /// Optional: Draw grid in editor for setup
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            int gridSize = baseGridSize;
            Gizmos.color = Color.yellow;
            
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    Vector3 localPos = gridOrigin + new Vector3(
                        col * switchSpacing.x - (gridSize - 1) * switchSpacing.x / 2f,
                        row * switchSpacing.y - (gridSize - 1) * switchSpacing.y / 2f,
                        0
                    );
                    
                    Vector3 worldPos = transform.TransformPoint(localPos);
                    Gizmos.DrawWireCube(worldPos, Vector3.one * 0.15f);
                }
            }
        }
    }
}










