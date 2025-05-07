using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public float health;
    public float maxHealth;
    public float width;
    public float height;

    [SerializeField] private RectTransform healthBar;

    public void SetMaxHealth(float maxHealthParam)
    {
        maxHealth = maxHealthParam;
    }
    public void SetHealth(float healthParam)
    {
        health = healthParam;
        //changes the width of the health
        float newWidth = (health / maxHealth) * width;

        //resizes the health bar based on the newWidth value
        healthBar.sizeDelta = new Vector2 (newWidth, healthBar.sizeDelta.y);
    }
}
