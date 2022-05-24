using System;
using System.Collections;
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

    [Header("Combat")]
    public float attackDistance = 1f;
    public float attackTimer = 1f;
    public float cooldown = 2f;
    public bool inCombat = false;
    public Entity target;
    public bool combatCoroutineIsRunning = false;
    public bool dead = false;

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

    protected Entity()
    {
    }

    protected void Start()
    {
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
        if (target != null && target.Equals(collider.gameObject.GetComponent<Entity>()))
        {
            target = null;
        }
    }

    protected void Die()
    {
        dead = true;
        inCombat = false;
        gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        
        animator.SetBool("isWalking", false);

        if (target.TryGetComponent(out Player player)) player.GainExp(rewardExperience);
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
}
