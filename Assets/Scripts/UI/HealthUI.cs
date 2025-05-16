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

    private float originalWidth;

    public void Start()
    {
        width = healthBar.sizeDelta.x;
        height = healthBar.sizeDelta.y;

        if (player.TryGetComponent<PlayerController>(out var pc))
        {
            maxHealth = pc.maxHealth;
            health = pc.health;

            SetMaxHealth(maxHealth);
            SetHealth(health);
        }
    }

    public void SetMaxHealth(float newValue)
    {
        maxHealth = newValue;

        if (health > maxHealth)
            health = maxHealth;

        // Recalculate width
        width = GetComponent<RectTransform>().sizeDelta.x;

        SetHealth(health); // Redraw
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
