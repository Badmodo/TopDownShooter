using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    private int level;
    private float currentLevelExp;
    private float experienceToLevel;

    private GameGUI gUI;

    private void Start()
    {
        LevelUp();
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
        gUI.SetPlayerHealth(health -= currentHealth, health);
        Debug.Log("Exp: " + currentLevelExp + "Level:" + level);
    }

    public override void Die()
    {
        base.Die();
    }

    private void LevelUp()
    {
        level++;
        experienceToLevel = level * 50 + Mathf.Pow(level * 2, 2);

        AddExperience(0);
    }
}
