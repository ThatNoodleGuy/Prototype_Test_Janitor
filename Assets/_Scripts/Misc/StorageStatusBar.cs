using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageStatusBar : MonoBehaviour
{
    RoomController roomController;
    StationManager stationManager;

    public float powerAmount = 100f;
    public float currentPower;
    public Slider powerSlider;
    public Image powerSliderImage;

    void Start()
    {
		stationManager = StationManager.Instance;

        //powerAmount = StationManager.Instance.PowerStorage.maxAmount;
        //currentPower = StationManager.Instance.PowerStorage.amount;

        currentPower = powerAmount;
        powerSlider.value = StationManager.Instance.PowerStorage.amountPerc * 100;
    }

    void Update()
    {
        powerSlider.value = StationManager.Instance.PowerStorage.amountPerc * 100;
        currentPower = Mathf.Clamp(currentPower, 0, powerAmount);

        powerSliderImage.color = Color.Lerp(Color.red, Color.green, powerSlider.value / 100);
    }
}
