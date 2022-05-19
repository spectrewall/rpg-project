using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Entity entity;

    [Header("Regen System")]
    public bool regenHPEnabled = true;
    public float regenHPTime = 5f;
    public int regenHPValue = 5;

    public bool regenMPEnabled = true;
    public float regenMPTime = 10f;
    public int regenMPValue = 5;

    public bool regenSPEnabled = true;
    public float regenSPTime = 1f;
    public int regenSPValue = 1;

    [Header("Game Manager")]
    public GameManager manager;

    [Header("Player Shortcuts")]
    public KeyCode attributesKey = KeyCode.C;

    [Header("Player UI Panels")]
    public GameObject attributesPanel;

    [Header("Player UI")]
    public Slider health;
    public Slider mana;
    public Slider stamina;
    public Slider exp;
    public Text expText;
    public Text levelText;
    public Text strText;
    public Text dexText;
    public Text resText;
    public Text intText;
    public Text wpText;
    public Text agiText;
    public Text points;
    public Button pointsApplyBtn;

    [Header("Positive Buttons")]
    public Button strBtnP;
    public Button dexBtnP;
    public Button resBtnP;
    public Button intBtnP;
    public Button wpBtnP;
    public Button agiBtnP;

    [Header("Negative Buttons")]
    public Button strBtnN;
    public Button dexBtnN;
    public Button resBtnN;
    public Button intBtnN;
    public Button wpBtnN;
    public Button agiBtnN;

    [Header("Stats UI")]
    public Text hpValue;
    public Text defValue;
    public Text pshDmgValue;
    public Text critDmgValue;
    public Text critChanceValue;
    public Text speedValue;

    [Header("Exp")]
    public int currentExp;
    public int expBase;
    public int expLeft;
    public float expMod;
    public GameObject levelUpFX;
    public AudioClip levelUpSound;
    public int givePoints = 5;
    public int LevelsBeforeGivePoints = 3;
    public float levelUpAnimDurantion = 2f;

    [Header("Respawn")]
    public float respawnTime = 5;
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        if (manager == null)
        {
            Debug.LogError("Você precisa anexar o game manager aqui no player");
            return;
        }

        entity.maxHealth = manager.CalculateHealth(entity);
        entity.maxMana = manager.CalculateMana(entity);
        entity.maxStamina = manager.CalculateStamina(entity);

        entity.currentHealth = entity.maxHealth;
        entity.currentMana = entity.maxMana;
        entity.currentStamina = entity.maxStamina;

        health.maxValue = entity.maxHealth;
        health.value = health.maxValue;

        mana.maxValue = entity.maxMana;
        mana.value = mana.maxValue;

        stamina.maxValue = entity.maxStamina;
        stamina.value = stamina.maxValue;

        exp.value = currentExp;
        exp.maxValue = expLeft;

        expText.text = String.Format("Exp: {0}/{1}", currentExp, expLeft);
        levelText.text = entity.level.ToString();

        StartCoroutine(RegenHealth());
        StartCoroutine(RegenMana());
        StartCoroutine(RegenStamina());

        UpdatePoints();
        SetupUIButtons();
    }

    private void Update()
    {
        if (entity.dead)
            return;

        if (entity.currentHealth <= 0)
        {
            Die();
        }

        if (Input.GetKeyDown(attributesKey))
        {
            UpdatePoints();
            attributesPanel.SetActive(!attributesPanel.activeSelf);
        }

        health.value = entity.currentHealth;
        mana.value = entity.currentMana;
        stamina.value = entity.currentStamina;
        exp.value = currentExp;
        exp.maxValue = expLeft;
    }

    IEnumerator RegenHealth()
    {
        while(true)
        {
            if (regenHPEnabled && entity.currentHealth < entity.maxHealth)
            {
                entity.currentHealth += regenHPValue;
                yield return new WaitForSeconds(regenHPTime);
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator RegenMana()
    {
        while (true)
        {
            if (regenMPEnabled && entity.currentMana < entity.maxMana)
            {
                entity.currentMana += regenMPValue;
                yield return new WaitForSeconds(regenMPTime);
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator RegenStamina()
    {
        while (true)
        {
            if (regenSPEnabled && entity.currentStamina < entity.maxStamina)
            {
                entity.currentStamina += regenSPValue;
                yield return new WaitForSeconds(regenSPTime);
            }
            else
            {
                yield return null;
            }
        }
    }

    void Die()
    {
        entity.dead = true;
        entity.currentHealth = 0;
        entity.target = null;

        StopAllCoroutines();
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        GetComponent<PlayerController>().enabled = false;
        yield return new WaitForSeconds(respawnTime);

        GameObject newPlayer = Instantiate(prefab, transform.position, transform.rotation, null);
        newPlayer.name = prefab.name;
        newPlayer.GetComponent<Player>().entity.dead = false;
        newPlayer.GetComponent<Player>().entity.combatCoroutine = false;
        newPlayer.GetComponent<PlayerController>().enabled = true;

        Destroy(this.gameObject);
    }

    public void GainExp(int amount)
    {
        currentExp += amount;
        expText.text = String.Format("Exp: {0}/{1}", currentExp, expLeft);
        StartCoroutine(LevelUpQueue());
    }

    IEnumerator LevelUpQueue()
    {
        while (true)
        {
            if (currentExp >= expLeft)
            {
                LevelUp();
                yield return new WaitForSeconds(levelUpAnimDurantion);
            }
            else
            {
                StopCoroutine(LevelUpQueue());
                yield return null;
            }
        }
    }

    public void LevelUp()
    {
        currentExp -= expLeft;
        entity.level++;
        levelText.text = entity.level.ToString();

        if (entity.level % LevelsBeforeGivePoints == 0)
            entity.points += givePoints;

        UpdatePoints();

        entity.maxHealth = manager.CalculateHealth(entity);
        entity.maxMana = manager.CalculateMana(entity);
        entity.maxStamina = manager.CalculateStamina(entity);

        entity.currentHealth = entity.maxHealth;
        entity.currentMana = entity.maxMana;
        entity.currentStamina = entity.maxStamina;

        float newExp = MathF.Pow((float)expMod, (float)entity.level);
        expLeft = (int)MathF.Floor((float)expBase * newExp);
        expText.text = String.Format("Exp: {0}/{1}", currentExp, expLeft);

        entity.entityAudio.PlayOneShot(levelUpSound);
        Instantiate(levelUpFX, this.gameObject.transform);
    }

    public void UpdatePoints()
    {
        strText.text = entity.strength.ToString();
        dexText.text = entity.dexterity.ToString();
        resText.text = entity.resistence.ToString();
        intText.text = entity.intelligence.ToString();
        wpText.text = entity.willPower.ToString();
        agiText.text = entity.agility.ToString();
        points.text = entity.points.ToString();

        UpdateStats();
    }

    public void UpdateStats()
    {
        // HpValue - Max
        Int32 MaxHp = (int.Parse(resText.text) * 10) + (entity.level * 4) + entity.baseHealth;

        // DefValue - Min/Max
        Int32 defenseValue = (int.Parse(resText.text) * 2) + (entity.level * 3) + entity.defense;

        // PhsDmgValue - Min/Max
        Int32 minFinalDamage = (int)(int.Parse(strText.text) * 3 * (entity.strengthBonus)) + (int.Parse(dexText.text) * 2) + (entity.damage * 2) + (entity.level * 3) + entity.bonusDamage + entity.bonusMinDamage;
        Int32 maxFinalDamage = (int)(int.Parse(strText.text) * 5 * (entity.strengthBonus)) + (int.Parse(dexText.text) * 2) + (entity.damage * 2) + (entity.level * 3) + entity.bonusDamage;
        Int32 finalMinFinalDamage = minFinalDamage <= maxFinalDamage ? minFinalDamage : maxFinalDamage;

        // CritDmgValue - Min/Max
        Int32 minCritDamage = (int.Parse(dexText.text) * 3) + entity.bonusMinCritDamage;
        Int32 maxCritDamage = (int.Parse(strText.text) * 2) + (int.Parse(dexText.text) * 5);
        Int32 MinCritDamageNormalized = minCritDamage <= maxCritDamage ? minCritDamage : maxCritDamage;

        Int32 finalMinCritDamage = MinCritDamageNormalized + finalMinFinalDamage;
        Int32 finalMaxCritDamage = maxCritDamage + maxFinalDamage;

        // CritChance - Percent
        Int32 baseCritchance = (int.Parse(dexText.text) / 5) + (int.Parse(wpText.text) / 10) + (entity.level / 5);
        Int32 normalizedCritChance = baseCritchance <= 50 ? baseCritchance : 50;
        Int32 normalizedCritChanceWithBonus = normalizedCritChance + entity.bonusCritChance;
        Int32 finalCritChance = normalizedCritChanceWithBonus <= 90 ? normalizedCritChanceWithBonus : 90;

        hpValue.text = MaxHp.ToString();
        defValue.text = defenseValue.ToString();
        pshDmgValue.text = String.Format("{0}/{1}", minFinalDamage, maxFinalDamage);
        critDmgValue.text = String.Format("{0}/{1}", finalMinCritDamage, finalMaxCritDamage);
        critChanceValue.text = String.Format("{0}%", finalCritChance);
        speedValue.text = entity.speed.ToString();
    }

    public void SetupUIButtons()
    {
        strBtnP.onClick.AddListener(() => AddPoints(strText));
        dexBtnP.onClick.AddListener(() => AddPoints(dexText));
        resBtnP.onClick.AddListener(() => AddPoints(resText));
        intBtnP.onClick.AddListener(() => AddPoints(intText));
        wpBtnP.onClick.AddListener(() => AddPoints(wpText));
        agiBtnP.onClick.AddListener(() => AddPoints(agiText));

        strBtnN.onClick.AddListener(() => RemovePoints(strText, entity.strength));
        dexBtnN.onClick.AddListener(() => RemovePoints(dexText, entity.dexterity));
        resBtnN.onClick.AddListener(() => RemovePoints(resText, entity.resistence));
        intBtnN.onClick.AddListener(() => RemovePoints(intText, entity.intelligence));
        wpBtnN.onClick.AddListener(() => RemovePoints(wpText, entity.willPower));
        agiBtnN.onClick.AddListener(() => RemovePoints(agiText, entity.agility));

        pointsApplyBtn.onClick.AddListener(() => ApplyPoints());
    }

    public void AddPoints(Text value)
    {
        int attributePoint = Convert.ToInt32(value.text);
        if (entity.points > 0)
        {
            value.text = (attributePoint + 1).ToString();
            entity.points--;
            points.text = entity.points.ToString();
            UpdateStats();
        }
    }

    public void RemovePoints(Text value, int attribute)
    {
        int attributePoint = Convert.ToInt32(value.text);
        if (attributePoint > attribute)
        {
            value.text = (attributePoint - 1).ToString();
            entity.points++;
            points.text = entity.points.ToString();
            UpdateStats();
        }
    }

    public void ApplyPoints()
    {
        entity.strength = Convert.ToInt32(strText.text);
        entity.dexterity = Convert.ToInt32(dexText.text);
        entity.resistence = Convert.ToInt32(resText.text);
        entity.intelligence = Convert.ToInt32(intText.text);
        entity.willPower = Convert.ToInt32(wpText.text);
        entity.agility = Convert.ToInt32(agiText.text);
    }
}
