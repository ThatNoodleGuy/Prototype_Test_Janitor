using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OxygenStorageBar : MonoBehaviour
{
    public float oxygenAmount;
    public float currentOxygen;
    public Slider oxygenSlider;
    public Image OxygenSliderImage;

    StationManager stationManager;

    void Start()
    {
        stationManager = FindObjectOfType<StationManager>();

        //oxygenAmount = stationManager.oxygenStorage.maxAmount;
        //currentOxygen = stationManager.oxygenStorage.amount;

        currentOxygen = oxygenAmount;
        oxygenSlider.value = stationManager.oxygenStorage.amountPerc * 100;
    }

    void Update()
    {
        oxygenSlider.value = stationManager.oxygenStorage.amountPerc * 100;
        currentOxygen = Mathf.Clamp(currentOxygen, 0, oxygenAmount);

        OxygenSliderImage.color = Color.Lerp(Color.grey, Color.blue, oxygenSlider.value / 100);
    }
}
