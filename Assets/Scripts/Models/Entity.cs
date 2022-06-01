using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("System Info")]
    public string entityName;
    public int level;
    public string type; // Possibles: "playeable", "combatent", "passive"
    public string moralAlignment; // Possibles: "good", "evil"

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
    public float knockbackForce = 0f;

    [Header("Combat")]
    public float attackDistance = 1f;
    public float attackTimer = 1f;
    public float cooldown = 2f;
    public bool inCombat = false;
    public Entity target;
    public bool combatCoroutineIsRunning = false;
    public bool dead = false;
    public float knockbackCurrentTime = 0f;
    public bool knockedback;

    [Header("Rewards")]
    public int rewardExperience = 10;
    public int lootGoldMin = 0;
    public int lootGoldMAx = 10;

    [Header("Respawn")]
    public GameObject prefab;
    public bool respawn = true;
    public float respawnTime = 10f;
    public Vector2 respawnPosition;

    [Header("Component")]
    public AudioSource entityAudio;
    public GameObject petPrefab;
    public GameObject pet;
    public Rigidbody2D rb2D;
    public Animator animator;

    [Header("Game Manager")]
    public GameManager manager;

    // TODO implementar um ID unico para cada Entitdade;
    private string UniqueID;

    public struct TotalDamagebyEntity
    {
        public Entity entity;
        public int totalDamage;

        public TotalDamagebyEntity(Entity entity, int totalDamage) : this()
        {
            this.entity = entity;
            this.totalDamage = totalDamage;
        }
    }

    private List<TotalDamagebyEntity> totalDamagebyEntities;

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        return GetHashCode() == obj.GetHashCode();
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(name);
        hash.Add(tag);
        hash.Add(enabled);
        hash.Add(isActiveAndEnabled);
        hash.Add(type);
        hash.Add(moralAlignment);
        hash.Add(UniqueID);
        return hash.ToHashCode();
    }

    protected void Start()
    {
        DateTime now = DateTime.Now;

        UniqueID =
            name +
            now.ToString("yyyyMMddHHmmssfffffff");

        Debug.Log(UniqueID);

        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        entityAudio = GetComponent<AudioSource>();

        gameObject.GetComponent<BoxCollider2D>().isTrigger = false;

        // Initiate values
        dead = false;
        target = null;
        combatCoroutineIsRunning = false;
        maxHealth = manager.CalculateHealth(this);
        maxMana = manager.CalculateMana(this);
        maxStamina = manager.CalculateStamina(this);
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;

        totalDamagebyEntities = new List<TotalDamagebyEntity>();

        // Setup Pet
        if (petPrefab != null)
        {
            if (petPrefab.TryGetComponent(out Pet _))
            {
                pet = Instantiate(petPrefab, transform.position, transform.rotation);
                pet.transform.SetParent(transform);
                pet.name = petPrefab.name;
            }
        }
    }
    protected void Update()
    {
        if (dead)
            return;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        if (knockbackCurrentTime > 0)
        {
            knockbackCurrentTime -= Time.deltaTime;
            knockedback = true;
        }
        else
        {
            knockbackCurrentTime = 0;
            knockedback = false;
        }
    }

    protected void OnTriggerEnter2D(Collider2D collider)
    {
        if (!dead && target == null && !type.Equals("passive"))
        {
            if ((collider.gameObject.TryGetComponent(out Entity entity)) && (entity.level - level <= 5) && (entity.moralAlignment != moralAlignment) && !entity.type.Equals("passive"))
            {
                target = entity;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (!collider.isTrigger && target != null && target.Equals(collider.gameObject.GetComponent<Entity>()))
        {
            target = null;
        }
    }

    public void TakeDamage(Entity entity, int damage)
    {
        bool found = false;
        totalDamagebyEntities.ForEach(totalDamagebyEntity =>
        {
            if (!found && totalDamagebyEntity.entity == entity)
            {
                found = true;
                Debug.Log("Found totaldamage: " + totalDamagebyEntity.totalDamage.ToString() + ", new damage: " + damage);
                totalDamagebyEntity.totalDamage += damage;
            }
        });

        if (!found)
        {
            TotalDamagebyEntity totalDamagebyEntity = new(entity, damage);
            totalDamagebyEntities.Add(totalDamagebyEntity);
            Debug.Log("Created new totaldamage: " + totalDamagebyEntity.totalDamage.ToString());
        }

        currentHealth -= damage;
    }

    public void TakeDamage(Entity entity, int damage, int seconds)
    {
        StartCoroutine(DamageOverTime(entity, damage, seconds));
    }

    IEnumerator DamageOverTime(Entity entity, int damage, int repetition)
    {
        for (int i = 0; i < repetition; i++)
        {
            TakeDamage(entity, damage);
            yield return new WaitForSeconds(1);
        }

        yield break;
    }

    protected void Die()
    {
        dead = true;
        inCombat = false;
        gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        
        animator.SetBool("isWalking", false);

        int totalEnemies = totalDamagebyEntities.Count;
        totalDamagebyEntities.ForEach((totalDamagebyEntitie) =>
        {
            float percentOfDamage = totalDamagebyEntitie.totalDamage / maxHealth;
            float percentOfDamageNormalized = percentOfDamage > 1 ? 1 : percentOfDamage;

            Debug.Log(percentOfDamageNormalized);

            if (totalDamagebyEntitie.entity.gameObject.TryGetComponent(out Player player)) player.GainExp(Mathf.RoundToInt(rewardExperience * percentOfDamageNormalized));
        });

        target = null;

        if (gameObject.TryGetComponent(out PlayerController playerController))
        {
            playerController.enabled = false;
        }

        StopAllCoroutines();
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);

        if (pet != null)
        {
            pet.transform.parent = null;
            pet.SetActive(false);
            Destroy(pet);
        }

        GameObject newEntity = Instantiate(gameObject, respawnPosition, transform.rotation, null);
        newEntity.name = gameObject.name;
        Destroy(gameObject);
    }

    public void Knockback(Entity other)
    {
        float force = other.knockbackForce;
        knockbackCurrentTime = force / 100;
        Vector2 knockbackDirection = transform.position - other.transform.position;
        rb2D.AddForce(knockbackDirection * force, ForceMode2D.Force);
    }
}
