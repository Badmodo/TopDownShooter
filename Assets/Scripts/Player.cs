using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Character
{
    private int level;
    private float currentLevelExp;
    private float experienceToLevel;
    public float maxHealth = 10f;

    private GameGUI gUI;
    public Image healthBar;
    public GameObject deadScreen;

    private void Start()
    {
        LevelUp();
        health = maxHealth;
    }

    private void Update()
    {
        healthBar.fillAmount = health / maxHealth;
    }

    private void Awake()
    {
        gUI = GameObject.FindGameObjectWithTag("GUI").GetComponent<GameGUI>();
    }

    public void AddExperience(float exp)
    {
        currentLevelExp += exp;
        if(currentLevelExp >= experienceToLevel)
        {
            //allows for experience to be carried over
            currentLevelExp -= experienceToLevel;
            LevelUp();
        }

        gUI.SetPlayerExperience(currentLevelExp / experienceToLevel, level);
        //gUI.SetPlayerHealth(health -= currentHealth, health);
        Debug.Log("Exp: " + currentLevelExp + "Level:" + level);
    }

    public override void Die()
    {
        ////test
        //Debug.Log("You Died");

        StartCoroutine(Dead());
    }

    IEnumerator Dead()
    {
        deadScreen.SetActive(true);
        Time.timeScale = 0;
        yield return null;
    }

    private void LevelUp()
    {
        level++;
        experienceToLevel = level * 50 + Mathf.Pow(level * 2, 2);

        AddExperience(0);
    }
}
