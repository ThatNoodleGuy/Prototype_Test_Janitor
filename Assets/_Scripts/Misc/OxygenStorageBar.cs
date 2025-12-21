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
        stationManager = StationManager.Instance;

        //oxygenAmount = StationManager.Instance.OxygenStorage.maxAmount;
        //currentOxygen = StationManager.Instance.OxygenStorage.amount;

        currentOxygen = oxygenAmount;
        oxygenSlider.value = StationManager.Instance.OxygenStorage.amountPerc * 100;
    }

    void Update()
    {
        oxygenSlider.value = StationManager.Instance.OxygenStorage.amountPerc * 100;
        currentOxygen = Mathf.Clamp(currentOxygen, 0, oxygenAmount);

        OxygenSliderImage.color = Color.Lerp(Color.grey, Color.blue, oxygenSlider.value / 100);
    }
}
