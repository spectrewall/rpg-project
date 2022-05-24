using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class AttributesUI : MonoBehaviour
{
    [Header("Attributes Text")]
    Text strText;
    Text dexText;
    Text resText;
    Text intText;
    Text wpText;
    Text agiText;
    Text pointsText;

    [Header("Positive Buttons")]
    Button strBtnP;
    Button dexBtnP;
    Button resBtnP;
    Button intBtnP;
    Button wpBtnP;
    Button agiBtnP;
    Button pointsApplyBtn;


    [Header("Negative Buttons")]
    Button strBtnN;
    Button dexBtnN;
    Button resBtnN;
    Button intBtnN;
    Button wpBtnN;
    Button agiBtnN;

    [Header("Stats UI")]
    Text hpValue;
    Text defValue;
    Text pshDmgValue;
    Text critDmgValue;
    Text critChanceValue;
    Text speedValue;

    Player player;
    int playerLastLevel;
    int totalPoints;
    int spentPoints = 0;

    void Start()
    {
        hpValue = GameObject.Find("HpValue").GetComponent<Text>();
        defValue = GameObject.Find("DefValue").GetComponent<Text>();
        pshDmgValue = GameObject.Find("PshDmgValue").GetComponent<Text>();
        critDmgValue = GameObject.Find("CritDmgValue").GetComponent<Text>();
        critChanceValue = GameObject.Find("CritChanceValue").GetComponent<Text>();
        speedValue = GameObject.Find("SpeedValue").GetComponent<Text>();

        setValues();
        UpdatePoints();
        SetupUIButtons();
    }

    void Update()
    {
        if (player.level > playerLastLevel)
        {
            UpdatePoints();
            playerLastLevel = player.level;
        }
    }

    private void OnEnable()
    {
        setValues();
    }

    private void OnDisable()
    {
        player.points += spentPoints;
        spentPoints = 0;
    }

    void setValues()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        playerLastLevel = player.level;

        if (strText == null) strText = GameObject.Find("StrValue").GetComponent<Text>();
        if (dexText == null) dexText = GameObject.Find("DexValue").GetComponent<Text>();
        if (resText == null) resText = GameObject.Find("ResValue").GetComponent<Text>();
        if (intText == null) intText = GameObject.Find("IntValue").GetComponent<Text>();
        if (wpText == null) wpText = GameObject.Find("WpValue").GetComponent<Text>();
        if (agiText == null) agiText = GameObject.Find("AgiValue").GetComponent<Text>();
        if (pointsText == null) pointsText = GameObject.Find("PointsValue").GetComponent<Text>();

        totalPoints = player.points;
        spentPoints = 0;
        pointsText.text = (totalPoints).ToString();

        strText.text = player.strength.ToString();
        dexText.text = player.dexterity.ToString();
        resText.text = player.resistence.ToString();
        intText.text = player.intelligence.ToString();
        wpText.text = player.willPower.ToString();
        agiText.text = player.agility.ToString();
    }

    public void UpdatePoints()
    {
        totalPoints = player.points + spentPoints;
        pointsText.text = (totalPoints - spentPoints).ToString();
        UpdateStats();
    }

    public void UpdateStats()
    {
        // HpValue - Max
        Int32 MaxHp = (int.Parse(resText.text) * 10) + (player.level * 4) + player.baseHealth;

        // DefValue - Min/Max
        Int32 defenseValue = (int.Parse(resText.text) * 2) + (player.level * 3) + player.defense;

        // PhsDmgValue - Min/Max
        Int32 minFinalDamage = (int)(int.Parse(strText.text) * 3 * (player.strengthBonus)) + (int.Parse(dexText.text) * 2) + (player.damage * 2) + (player.level * 3) + player.bonusDamage + player.bonusMinDamage;
        Int32 maxFinalDamage = (int)(int.Parse(strText.text) * 5 * (player.strengthBonus)) + (int.Parse(dexText.text) * 2) + (player.damage * 2) + (player.level * 3) + player.bonusDamage;
        Int32 finalMinFinalDamage = minFinalDamage <= maxFinalDamage ? minFinalDamage : maxFinalDamage;

        // CritDmgValue - Min/Max
        Int32 minCritDamage = (int.Parse(dexText.text) * 3) + player.bonusMinCritDamage;
        Int32 maxCritDamage = (int.Parse(strText.text) * 2) + (int.Parse(dexText.text) * 5);
        Int32 MinCritDamageNormalized = minCritDamage <= maxCritDamage ? minCritDamage : maxCritDamage;

        Int32 finalMinCritDamage = MinCritDamageNormalized + finalMinFinalDamage;
        Int32 finalMaxCritDamage = maxCritDamage + maxFinalDamage;

        // CritChance - Percent
        Int32 baseCritchance = (int.Parse(dexText.text) / 5) + (int.Parse(wpText.text) / 10) + (player.level / 5);
        Int32 normalizedCritChance = baseCritchance <= 50 ? baseCritchance : 50;
        Int32 normalizedCritChanceWithBonus = normalizedCritChance + player.bonusCritChance;
        Int32 finalCritChance = normalizedCritChanceWithBonus <= 90 ? normalizedCritChanceWithBonus : 90;

        hpValue.text = MaxHp.ToString();
        defValue.text = defenseValue.ToString();
        pshDmgValue.text = String.Format("{0}/{1}", minFinalDamage, maxFinalDamage);
        critDmgValue.text = String.Format("{0}/{1}", finalMinCritDamage, finalMaxCritDamage);
        critChanceValue.text = String.Format("{0}%", finalCritChance);
        speedValue.text = player.speed.ToString();
    }

    void SetupUIButtons()
    {
        // "+" buttons
        strBtnP = GameObject.Find("StrBtnP").GetComponent<Button>();
        dexBtnP = GameObject.Find("DexBtnP").GetComponent<Button>();
        resBtnP = GameObject.Find("ResBtnP").GetComponent<Button>();
        intBtnP = GameObject.Find("IntBtnP").GetComponent<Button>();
        wpBtnP = GameObject.Find("WpBtnP").GetComponent<Button>();
        agiBtnP = GameObject.Find("AgiBtnP").GetComponent<Button>();

        // "-" Buttons
        strBtnN = GameObject.Find("StrBtnN").GetComponent<Button>();
        dexBtnN = GameObject.Find("DexBtnN").GetComponent<Button>();
        resBtnN = GameObject.Find("ResBtnN").GetComponent<Button>();
        intBtnN = GameObject.Find("IntBtnN").GetComponent<Button>();
        wpBtnN = GameObject.Find("WpBtnN").GetComponent<Button>();
        agiBtnN = GameObject.Find("AgiBtnN").GetComponent<Button>();

        // "Apply" Button
        pointsApplyBtn = GameObject.Find("PointsConfirmButton").GetComponent<Button>();

        strBtnP.onClick.AddListener(() => AddPoints(strText));
        dexBtnP.onClick.AddListener(() => AddPoints(dexText));
        resBtnP.onClick.AddListener(() => AddPoints(resText));
        intBtnP.onClick.AddListener(() => AddPoints(intText));
        wpBtnP.onClick.AddListener(() => AddPoints(wpText));
        agiBtnP.onClick.AddListener(() => AddPoints(agiText));

        strBtnN.onClick.AddListener(() => RemovePoints(strText, player.strength));
        dexBtnN.onClick.AddListener(() => RemovePoints(dexText, player.dexterity));
        resBtnN.onClick.AddListener(() => RemovePoints(resText, player.resistence));
        intBtnN.onClick.AddListener(() => RemovePoints(intText, player.intelligence));
        wpBtnN.onClick.AddListener(() => RemovePoints(wpText, player.willPower));
        agiBtnN.onClick.AddListener(() => RemovePoints(agiText, player.agility));

        pointsApplyBtn.onClick.AddListener(() => ApplyPoints());
    }

    void AddPoints(Text value)
    {
        int attributePoint = Convert.ToInt32(value.text);
        if (player.points > 0)
        {
            value.text = (attributePoint + 1).ToString();
            player.points--;
            spentPoints++;
            pointsText.text = player.points.ToString();
            UpdateStats();
        }
    }

    void RemovePoints(Text value, int attribute)
    {
        int attributePoint = Convert.ToInt32(value.text);
        if (attributePoint > attribute)
        {
            value.text = (attributePoint - 1).ToString();
            player.points++;
            spentPoints--;
            pointsText.text = player.points.ToString();
            UpdateStats();
        }
    }

    void ApplyPoints()
    {
        if (spentPoints > player.points)
            gameObject.SetActive(false);

        spentPoints = 0;

        player.strength = Convert.ToInt32(strText.text);
        player.dexterity = Convert.ToInt32(dexText.text);
        player.resistence = Convert.ToInt32(resText.text);
        player.intelligence = Convert.ToInt32(intText.text);
        player.willPower = Convert.ToInt32(wpText.text);
        player.agility = Convert.ToInt32(agiText.text);
    }
}
