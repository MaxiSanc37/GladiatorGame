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
        originalWidth = healthBar.sizeDelta.x; 
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

        SetHealth(health); // Redraw
    }

    public void SetHealth(float healthParam)
    {
        health = Mathf.Clamp(healthParam, 0, maxHealth);

        float normalizedHealth = health / maxHealth;
        normalizedHealth = Mathf.Clamp01(normalizedHealth);

        float newWidth = normalizedHealth * originalWidth;
        healthBar.sizeDelta = new Vector2(newWidth, healthBar.sizeDelta.y);

        healthText.text = $"{Mathf.RoundToInt(health)} / {Mathf.RoundToInt(maxHealth)}";
    }

}
