using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public float health;
    public float maxHealth;
    public float width;
    public float height;

    public TMP_Text healthText;

    public GameObject player;

    [SerializeField] private RectTransform healthBar;

    public void Start()
    {
        //provide the initial health
        healthText.text = player.GetComponent<PlayerController>().health.ToString();
    }
    public void SetMaxHealth(float maxHealthParam)
    {
        //sets the maxHealth
        maxHealth = maxHealthParam;
    }
    public void SetHealth(float healthParam)
    {
        health = healthParam;
        //changes the width of the health
        float newWidth = (health / maxHealth) * width;

        //resizes the health bar based on the newWidth value
        healthBar.sizeDelta = new Vector2 (newWidth, healthBar.sizeDelta.y);
        //add the hp number
        healthText.text = Mathf.RoundToInt(health).ToString();
    }
}
