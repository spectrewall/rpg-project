using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Entity
{
    [Header("Name")]
    public string name;
    public int level;

    [Header("Health")]
    public int currentHealth;
    public int maxHealth;
    public int baseHealth = 26;

    [Header("Mana")]
    public int currentMana;
    public int maxMana;

    [Header("Stamina")]
    public int currentStamina;
    public int maxStamina;

    [Header("Stats")]
    public int strength = 5;
    public int dexterity = 5;
    public int resistence = 5;
    public int intelligence = 5;
    public int willPower = 5;
    public int agility = 5;
    public int damage = 1;
    public int defense = 1;
    public float speed = 2f;
    public int points = 0;

    [Header("Banuses")]
    public int bonusCritChance = 0;
    public int bonusSpeed = 0;
    public int bonusDamage = 0;
    public int bonusMinDamage = 0;
    public int bonusMinCritDamage = 0;
    public float strengthBonus = 1f;

    [Header("Combat")]
    public float attackDistance = 0.5f;
    public float attackTimer = 1f;
    public float cooldown = 2f;
    public bool inCombat = false;
    public GameObject target;
    public bool combatCoroutine = false;
    public bool dead = false;

    [Header("Component")]
    public AudioSource entityAudio;
    public GameObject petPrefab;
    public GameObject pet;
}
