using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthBar;
    public int healthAmount = 100;
    public float currentHealth;
    public float decreaseHealthBy = 1f;
    PlayerOxygen playerOxygen;
    public float missingHealth;
    public GameManager gameManager;

    void Start()
    {
        currentHealth = healthAmount;
        playerOxygen = GetComponent<PlayerOxygen>();
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        missingHealth = healthAmount - currentHealth;

        if (playerOxygen.currentOxygen <= 0)
        {
            LooseHP();
        }

        healthBar.value = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, healthAmount);
    }
    public void takeDamage(float ammount)
    {
        currentHealth -= ammount;
    }

    public void LooseHP()
    {
        currentHealth -= decreaseHealthBy * Time.deltaTime;
    }
}
