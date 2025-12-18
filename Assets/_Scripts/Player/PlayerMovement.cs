using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public AudioSource musicBG;
    AudioSource audioManager;
    
    // UI
    public Slider playerSpeed;
    public Slider mouseSensitivity;
    public Slider gameVolume;
    public Slider musicBGVolume;
    public GameObject pauseMenu;

    // Look around
    float mouseX;
    public float cameraSpeed = 225f; // Default value
    float mouseY;
    float cameraX;
    Transform cameraTrn;

    // Movement
    float horizontal;
    float vertical;
    public float movementSpeed = 4f; // Default value
    Vector3 moveDirection;
    Rigidbody rb;

    // Jump
    public LayerMask groundLayer;
    public bool isGrounded;
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] float groundCheckDistance = 0.3f;
    [SerializeField] float groundCheckRadius = 0.3f;

    void Start()
    {
        audioManager = GetComponent<AudioSource>();
        
        // Get or add Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configure Rigidbody
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        // Camera init
        cameraX = 0;
        cameraTrn = GameObject.FindGameObjectWithTag("PlayerCamera").transform;

        // Initialize values from sliders if they exist
        if (mouseSensitivity != null)
            cameraSpeed = mouseSensitivity.value;
        if (playerSpeed != null)
            movementSpeed = playerSpeed.value;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Handle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }

        if (pauseMenu.activeInHierarchy)
        {
            return;
        }

        // Handle sprint
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? movementSpeed * 2 : movementSpeed;

        // Camera rotation (left & right) - horizontal rotation on player body
        mouseX = Input.GetAxis("Mouse X") * cameraSpeed;
        transform.Rotate(0, mouseX * Time.deltaTime, 0);

        // Camera rotation (up & down) - vertical rotation on camera
        mouseY = Input.GetAxis("Mouse Y") * cameraSpeed;
        cameraX -= mouseY * Time.deltaTime;
        cameraX = Mathf.Clamp(cameraX, -65, 48);
        cameraTrn.localRotation = Quaternion.Euler(cameraX, 0, 0);

        // Get input
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // Calculate movement direction relative to where player is looking
        moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized * currentSpeed;

        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Sqrt(jumpHeight * 2f * Mathf.Abs(Physics.gravity.y)), rb.linearVelocity.z);
        }
    }

    void FixedUpdate()
    {
        // Apply movement in FixedUpdate for physics
        Vector3 newVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.z);
        rb.linearVelocity = newVelocity;
    }

    private void PauseGame()
    {
        pauseMenu.SetActive(!pauseMenu.activeInHierarchy);
        if (pauseMenu.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void SetPlayerSpeed()
    {
        movementSpeed = playerSpeed.value;
    }

    public void SetMouseSensitivity()
    {
        cameraSpeed = mouseSensitivity.value;
    }

    public void SetGameVoluve()
    {
        AudioListener.volume = gameVolume.value;
    }

    public void SetMusicVolume()
    {
        musicBG.volume = musicBGVolume.value;
    }

    public void SetDefaults()
    {
        musicBGVolume.value = 0.5f;
        gameVolume.value = 1;
        mouseSensitivity.value = 225;
        playerSpeed.value = 4;
        
        // Apply the defaults immediately
        SetMouseSensitivity();
        SetPlayerSpeed();
        SetGameVoluve();
        SetMusicVolume();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
}