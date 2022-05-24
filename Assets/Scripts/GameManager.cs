using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Int32 CalculateHealth(Entity entity)
    {
        Int32 result = (entity.resistence * 10) + (entity.level * 4) + entity.baseHealth;
        return result;
    }

    public Int32 CalculateMana(Entity entity)
    {
        Int32 result = (entity.intelligence * 5) + (entity.level * 4) + 5;
        return result;
    }
    public Int32 CalculateStamina(Entity entity)
    {
        Int32 result = (entity.resistence + entity.willPower) + (entity.level * 2) + 5;
        return result;
    }

    public Int32 CalculateDamage(Entity entity, int weaponDamage)
    {
        System.Random rnd = new System.Random();
        Int32 baseCritchance = (entity.dexterity / 5) + (entity.willPower / 10) + (entity.level / 5);
        Int32 normalizedCritChance = baseCritchance <= 50 ? baseCritchance : 50;
        Int32 normalizedCritChanceWithBonus = normalizedCritChance + entity.bonusCritChance;
        Int32 finalCritChance = normalizedCritChanceWithBonus <= 90 ? normalizedCritChanceWithBonus : 90;

        Int32 critDamage = 0;

        if (rnd.Next(1,100) <= finalCritChance)
        {
            Int32 minCritDamage = (entity.dexterity * 3) + entity.bonusMinCritDamage;
            Int32 maxCritDamage = (entity.strength * 2) + (entity.dexterity * 5);
            Int32 finalMinCritDamage = minCritDamage <= maxCritDamage ? minCritDamage : maxCritDamage;
            critDamage = rnd.Next(finalMinCritDamage, maxCritDamage);
        }

        Int32 minFinalDamage = (int)(entity.strength * 3 * (entity.strengthBonus)) + (entity.dexterity * 2) + (weaponDamage * 2) + (entity.level * 3) + critDamage + entity.bonusDamage + entity.bonusMinDamage;
        Int32 maxFinalDamage = (int)(entity.strength * 5 * (entity.strengthBonus)) + (entity.dexterity * 2) + (weaponDamage * 2) + (entity.level * 3) + critDamage + entity.bonusDamage;
        Int32 finalMinFinalDamage = minFinalDamage <= maxFinalDamage ? minFinalDamage : maxFinalDamage;

        Int32 FinalDamage = rnd.Next(finalMinFinalDamage, maxFinalDamage);
        return FinalDamage;
    }

    public Int32 CalculateDefense(Entity entity, int armorDefense)
    {
        Int32 result = (entity.resistence * 2) + (entity.level * 3) + armorDefense;
        return result;
    }
}
