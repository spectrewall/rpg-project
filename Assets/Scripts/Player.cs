using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : Entity
{
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

    [Header("Player UI")]
    public Slider health;
    public Slider mana;
    public Slider stamina;
    public Slider exp;
    public Text expText;
    public Text levelText;

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

    bool levelUpCoroutine = false;

    private new void Start()
    {
        base.Start();

        // Allows player to move
        GetComponent<PlayerController>().enabled = true;
                    
        // Setup UI Sliders
        health.maxValue = maxHealth;
        health.value = health.maxValue;
        mana.maxValue = maxMana;
        mana.value = mana.maxValue;
        stamina.maxValue = maxStamina;
        stamina.value = stamina.maxValue;
        exp.value = currentExp;
        exp.maxValue = expLeft;
        expText.text = String.Format("Exp: {0}/{1}", currentExp, expLeft);
        levelText.text = level.ToString();

        // Start Coroutines
        StartCoroutine(RegenHealth());
        StartCoroutine(RegenMana());
        StartCoroutine(RegenStamina());
    }

    private new void Update()
    {
        base.Update();

        health.value = currentHealth;
        mana.value = currentMana;
        stamina.value = currentStamina;
        exp.value = currentExp;
        exp.maxValue = expLeft;
    }

    IEnumerator RegenHealth()
    {
        while(true)
        {
            if (regenHPEnabled && currentHealth < maxHealth)
            {
                currentHealth += regenHPValue;
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
            if (regenMPEnabled && currentMana < maxMana)
            {
                currentMana += regenMPValue;
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
            if (regenSPEnabled && currentStamina < maxStamina)
            {
                currentStamina += regenSPValue;
                yield return new WaitForSeconds(regenSPTime);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void GainExp(int amount)
    {
        currentExp += amount;
        expText.text = String.Format("Exp: {0}/{1}", currentExp, expLeft);
        if (!levelUpCoroutine) StartCoroutine(LevelUpQueue());
    }

    IEnumerator LevelUpQueue()
    {
        levelUpCoroutine = true;
        while (true)
        {
            if (currentExp >= expLeft)
            {
                LevelUp();
                yield return new WaitForSeconds(levelUpAnimDurantion);
            }
            else
            {
                levelUpCoroutine = false;
                StopCoroutine("LevelUpQueue");
                yield return null;
            }
        }
    }

    public void LevelUp()
    {
        currentExp -= expLeft;
        level++;
        levelText.text = level.ToString();

        if (level % LevelsBeforeGivePoints == 0)
            points += givePoints;

        maxHealth = manager.CalculateHealth(this);
        maxMana = manager.CalculateMana(this);
        maxStamina = manager.CalculateStamina(this);

        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;

        float newExp = MathF.Pow((float)expMod, (float)level);
        expLeft = (int)MathF.Floor((float)expBase * newExp);
        expText.text = String.Format("Exp: {0}/{1}", currentExp, expLeft);

        entityAudio.PlayOneShot(levelUpSound);
        GameObject levelUpFxObj = Instantiate(levelUpFX, this.gameObject.transform);
        Destroy(levelUpFxObj, levelUpAnimDurantion);
    }
}
