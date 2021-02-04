using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Data",menuName ="Caracter Stats/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]
    public int maxHealth;
    public int CurrentHealth;
    public int baseDefence;
    public int currentDefence;

    public int killPoint;

    [Header("Level")]
    public int currentLevel;
    public int maxLevel;
    public int baseExp;
    public int currentExp;
    public float levelBuff;

    public float LevelMultipliter
    {
        get { return 1 + (currentLevel - 1) * levelBuff; }
    }

    public void UpdateExp(int point)
    {
        currentExp += point;
        if (currentExp >= baseExp)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        //升级属性
        currentLevel = Mathf.Clamp(currentLevel + 1,0,maxLevel);

        baseExp += (int)(baseExp * LevelMultipliter);
        maxHealth = (int)(maxHealth * LevelMultipliter);
        CurrentHealth = maxHealth;
        Debug.Log("Level Up!");
    }
}
