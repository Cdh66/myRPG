using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="New Attack",menuName ="Attack/AttackData")]
public class AttackData_SO : ScriptableObject
{
    public float attackRange;
    public float skillRange;
    public float coolDown;
    public int minDamage;
    public int maxDamage;
    [Header("暴击加成")]
    public float criticalMultiplier;
    [Header("暴击率")]
    public float criticalChance;
}
