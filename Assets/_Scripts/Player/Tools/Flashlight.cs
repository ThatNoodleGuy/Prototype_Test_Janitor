using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    new Light light;
    public AudioSource playerAudio;
    public AudioClip flashlightClick;
    public float startingLightIntensity = 1;
    public float higherLightIntensity = 10;

    private void Start()
    {
        light = GetComponentInChildren<Light>();
        light.intensity = startingLightIntensity;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerAudio.PlayOneShot(flashlightClick);

            // Flashlight intensity
            if (light.intensity == startingLightIntensity)
            {
                light.intensity = higherLightIntensity;
            }
            else
            {
                light.intensity = startingLightIntensity;
            }

            //ToggleLight();
        }
    }

    /*
    void ToggleLight()
    {
        light.enabled = !light.enabled;
    }
    */
}
