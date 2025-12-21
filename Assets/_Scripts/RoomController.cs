using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomController : MonoBehaviour
{
    public enum RoomType { Power, Oxygen }
    
    [Header("Room Settings")]
    public RoomType roomType;
    public Door door;
    public float contaminationTimer = 30f;
    
    [Header("Audio")]
    public AudioSource playerAudioSource;
    public AudioClip lowResouces;
    public AudioClip fillStorageSFX;
    public AudioClip a10;
    public AudioClip a5;
    public AudioClip a3;
    public AudioClip a2;
    public AudioClip a1;
    public AudioClip a0;
    
    [Header("Resources")]
    public Storage myTank;
    
    [Header("UI")]
    public Text timerText;
    public Text ammountPerc;
    public Text storageLvlText;
    public Button fillStorage;
    public Text alertMsg;
    public string baseMsg = "No Errors, can fill storage.";
    
    [Header("Visual Effects")]
    public Light myAlertLight;
    public float spinningSpeed = 180f;
    
    // Private variables
    private float currentTimer;
    private float spintLight = 180f;
    public float dir;
    private bool isInRoom;
    private bool wasInRoom; // Track previous state
    public bool isGameOverCon = false;
    
    // Voice alert flags
    private bool tenSecAlert;
    private bool fiveSecAlert;
    private bool threeSecAlert;
    private bool twoSecAlert;
    private bool oneSecAlert;
    private bool zeroSecAlert;
    private bool hasPlayedVoiceAlert;
    
    // References
    [SerializeField] WorkStation workStation;
    [SerializeField] StationManager stationManager;

    void Start()
    {
        workStation = GetComponent<WorkStation>();
        stationManager = StationManager.Instance;
        
        // Ensure we have a trigger collider
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null)
        {
            Debug.LogWarning($"{gameObject.name} needs a Collider for player detection!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name} collider should be set to IsTrigger!");
        }
    }

    void Update()
    {
        VoiceAlert();
        SetLock();
        UpdateAlertLights();
        UpdateUI();
        UpdateTimer();
        
        // Update wasInRoom for next frame
        wasInRoom = isInRoom;
    }

    // TRIGGER DETECTION (replaces CheckSpace.cs)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRoom = true;
            currentTimer = contaminationTimer;
            ResetVoiceAlerts();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRoom = false;
            currentTimer = 0;
            ResetVoiceAlerts();
        }
    }

    void UpdateTimer()
    {
        if (isInRoom)
        {
            currentTimer -= Time.deltaTime;
            currentTimer = Mathf.Max(0, currentTimer);
        }
        else
        {
            currentTimer = 0;
        }
        
        if (timerText != null)
            timerText.text = "Time:" + Mathf.Floor(currentTimer);
    }

    void SetLock()
    {
        if (door == null) return;

        if (isInRoom)
        {
            // When player is in room, lock if time runs out
            door.isLocked = currentTimer <= 0;
        }
        else
        {
            // When player is outside, lock if resources are full
            door.isLocked = HasRecource();
        }
    }

    void VoiceAlert()
    {
        if (!isInRoom || playerAudioSource == null) return;

        float time = Mathf.Floor(currentTimer);

        if (time == 16 && !tenSecAlert)
        {
            if (a10 != null) playerAudioSource.PlayOneShot(a10);
            tenSecAlert = true;
        }
        else if (time == 5 && !fiveSecAlert)
        {
            if (a5 != null) playerAudioSource.PlayOneShot(a5);
            fiveSecAlert = true;
        }
        else if (time == 3 && !threeSecAlert)
        {
            if (a3 != null) playerAudioSource.PlayOneShot(a3);
            threeSecAlert = true;
        }
        else if (time == 2 && !twoSecAlert)
        {
            if (a2 != null) playerAudioSource.PlayOneShot(a2);
            twoSecAlert = true;
        }
        else if (time == 1 && !oneSecAlert)
        {
            if (a1 != null) playerAudioSource.PlayOneShot(a1);
            oneSecAlert = true;
        }
        else if (time == 0 && !zeroSecAlert && oneSecAlert)
        {
            if (a0 != null)
            {
                playerAudioSource.PlayOneShot(a0);
                StartCoroutine(WaitForGameOver(a0.length + 0.5f));
            }
            zeroSecAlert = true;
        }
    }

    void ResetVoiceAlerts()
    {
        tenSecAlert = false;
        fiveSecAlert = false;
        threeSecAlert = false;
        twoSecAlert = false;
        oneSecAlert = false;
        zeroSecAlert = false;
    }

    IEnumerator WaitForGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isInRoom && zeroSecAlert)
        {
            isGameOverCon = true;
        }
    }

    void UpdateAlertLights()
    {
        if (myAlertLight == null) return;

        if (HasRecource())
        {
            // Resources full - alert on
            myAlertLight.gameObject.SetActive(true);
            spintLight = 0;
        }
        else
        {
            // Resources low - spin light
            myAlertLight.gameObject.SetActive(true);
            spintLight += spinningSpeed * Time.deltaTime;
            myAlertLight.transform.localRotation = Quaternion.Euler(spintLight, dir, 0);
        }
    }

    void UpdateUI()
    {
        if (myTank == null) return;

        if (ammountPerc != null)
            ammountPerc.text = (myTank.amountPerc * 100).ToString("0") + "%";
        
        if (storageLvlText != null)
            storageLvlText.text = "(lvl" + myTank.level + ")";
        
        if (fillStorage != null && alertMsg != null)
        {
            fillStorage.interactable = (alertMsg.text == baseMsg);
        }
    }

    public bool HasRecource()
    {
        if (myTank == null || workStation == null) return false;

        bool hasEnough = myTank.amount >= myTank.reqAmount * workStation.level;

        if (hasEnough)
        {
            hasPlayedVoiceAlert = false;
        }
        else if (!hasPlayedVoiceAlert && lowResouces != null && playerAudioSource != null)
        {
            StartCoroutine(PlayLowResourceAlert());
            hasPlayedVoiceAlert = true;
        }

        return hasEnough;
    }

    IEnumerator PlayLowResourceAlert()
    {
        yield return new WaitForSeconds(2.5f);
        if (playerAudioSource != null && lowResouces != null)
            playerAudioSource.PlayOneShot(lowResouces);
    }

    public void FillStorage()
    {
        if (myTank == null) return;

        myTank.amount = myTank.maxAmount;
        
        if (playerAudioSource != null && fillStorageSFX != null)
            playerAudioSource.PlayOneShot(fillStorageSFX);

        // Trigger event to notify StationManager (using trigger methods)
        GameEvents.TriggerResourceFilled(roomType);
        GameEvents.TriggerResourceChanged();
    }

    // === COMPATIBILITY METHODS FOR OLD SCRIPTS ===
    
    // Old method name (note the typo "Massage" instead of "Message")
    // Kept for compatibility with OxygenSpawner and FuseBoard
    public void AlertMassage(List<string> errors)
    {
        SetAlertMessage(errors);
    }
    
    public void SetAlertMessage(List<string> errors)
    {
        if (alertMsg == null) return;

        if (errors != null && errors.Count > 0)
        {
            alertMsg.text = string.Join("; ", errors);
        }
        else
        {
            alertMsg.text = baseMsg;
        }
    }

    // === PUBLIC ACCESSORS ===
    
    // For compatibility with old RoomController
    public bool IsInRoom => isInRoom;
    public float Timer => currentTimer;
    public float timer => currentTimer; // lowercase version for old scripts
    
    // isGoingOut - true when player just exited the room
    public bool isGoingOut => !isInRoom && wasInRoom;
}