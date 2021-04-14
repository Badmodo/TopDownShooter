using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameGUI : MonoBehaviour
{
    public Image expBar;
    public Image healthBar;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI ammoText;
     
    public void SetAmmoInfo(int totalAmmo, int ammoInMag)
    {
        ammoText.text = "Ammo - " + ammoInMag + "/" + totalAmmo;
    }

    public void SetPlayerExperience(float percentToLevel, int playerLevel)
    {
        expText.text = "Level - " + playerLevel;
        expBar.fillAmount = percentToLevel;
    }

    //public void SetPlayerHealth(float currentHealth, float health)
    //{
    //    healthBar.fillAmount = health;
    //}
}
