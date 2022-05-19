using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]

public class Monster : MonoBehaviour
{
    [Header("Controller")]
    public Entity entity;
    public GameManager manager;

    [Header("Patrol")]
    public List<Transform> waypointList;
    public float arrivalDistance = 0.5f;
    public float waitTime = 5;
    public int waypointID;

    [Header("Rewards")]
    public int rewardExperience = 10;
    public int lootGoldMin = 0;
    public int lootGoldMAx = 10;

    [Header("Respawn")]
    public GameObject prefab;
    public bool respawn = true;
    public float respawnTime = 10f;

    [Header("UI")]
    public Slider healthSlider;

    // Private
    Transform targetWaypoint;
    int currentWaypoint = 0;
    float lastDistanceToTarget = 0f;
    float currentWaitTime = 0f;
    Vector2 direction;
    Rigidbody2D rb2D;
    Animator animator;
    Vector2 initialPosition;
    Coroutine combatCoroutine;

    private void Start()
    {
        initialPosition = transform.position;

        // Get Components
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // Entity Setup
        entity.dead = false;
        entity.combatCoroutine = false;
        entity.maxHealth = manager.CalculateHealth(entity);
        entity.maxMana = manager.CalculateMana(entity);
        entity.maxStamina = manager.CalculateMana(entity);
        entity.currentHealth = entity.maxHealth;
        entity.currentMana = entity.maxMana;
        entity.currentStamina = entity.maxStamina;

        // Slider Setup
        healthSlider.maxValue = entity.maxHealth;
        healthSlider.value = healthSlider.maxValue;

        // Waypoints Setup
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Waypoint"))
        {
            int ID = obj.GetComponent<WaypointID>().ID;
            if (ID == waypointID)
            {
                waypointList.Add(obj.transform);
            }
        }

        currentWaitTime = waitTime;
        if (waypointList.Count > 0)
        {
            targetWaypoint = waypointList[currentWaypoint];
            lastDistanceToTarget = Vector2.Distance(transform.position, targetWaypoint.position);
        }
    }

    private void Update()
    {
        if (entity.dead) return;

        if (entity.currentHealth <= 0)
        {
            entity.currentHealth = 0;
            Die();
        }

        healthSlider.value = entity.currentHealth;

        if (entity.target == null)
        {
            if (waypointList.Count > 0) Patrol();
            else animator.SetBool("isWalking", false);
        }
        else
        {
            FollowTarget();
        }
    }

    private void FixedUpdate()
    {
        if (entity.dead) return;
        rb2D.MovePosition(rb2D.position + direction * (entity.speed * Time.fixedDeltaTime));
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (!entity.dead && entity.target == null)
        {
            if (collider.tag == "Player" && !collider.gameObject.GetComponent<Player>().entity.dead)
            {
                entity.target = collider.gameObject;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (entity.target.Equals(collider.gameObject))
        {
            entity.target = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collider)
    {
        
    }

    void Patrol()
    {
        if (entity.dead) return;

        float distanceToTarget = Vector2.Distance(transform.position, targetWaypoint.position);

        if (distanceToTarget <= arrivalDistance || distanceToTarget >= lastDistanceToTarget)
        {
            if (currentWaitTime <= 0)
            {
                currentWaypoint++;

                if (currentWaypoint >= waypointList.Count) currentWaypoint = 0;

                targetWaypoint = waypointList[currentWaypoint];
                lastDistanceToTarget = Vector2.Distance(transform.position, targetWaypoint.position);

                currentWaitTime = waitTime;
            } 
            else
            {
                animator.SetBool("isWalking", false);
                currentWaitTime -= Time.deltaTime;
            }
        } 
        else
        {
            animator.SetBool("isWalking", true);
            lastDistanceToTarget = distanceToTarget;
        }

        direction = (targetWaypoint.position - transform.position).normalized;
        animator.SetFloat("input_x", direction.x);
        animator.SetFloat("input_y", direction.y);
    }

    void FollowTarget()
    {
        if (entity.dead) return;
        if (entity.target.GetComponent<Player>().entity.dead)
        {
            entity.target = null;
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, entity.target.transform.position);

        if (distanceToTarget <= entity.attackDistance)
        {
            animator.SetBool("isWalking", false);
            direction = Vector2.zero;
            if (!entity.combatCoroutine) combatCoroutine = StartCoroutine(Attack());
        }
        else
        {
            direction = (entity.target.transform.position - transform.position).normalized;
            animator.SetBool("isWalking", true);
            animator.SetFloat("input_x", direction.x);
            animator.SetFloat("input_y", direction.y);
        }
    }

    IEnumerator Attack()
    {
        entity.combatCoroutine = true;
        while (true)
        {
            if(entity.target != null && !entity.target.GetComponent<Player>().entity.dead)
            {
                float distance = Vector2.Distance(transform.position, entity.target.transform.position);

                if (distance <= entity.attackDistance)
                {
                    animator.SetBool("attack", true);
                    int monsterDmg = manager.CalculateDamage(entity, entity.damage);
                    int targetDef = manager.CalculateDefense(entity.target.GetComponent<Player>().entity, entity.target.GetComponent<Player>().entity.defense);
                    int dmgResult = monsterDmg - targetDef;

                    if (dmgResult < 0)
                        dmgResult = 0;

                    entity.target.GetComponent<Player>().entity.currentHealth -= dmgResult;
                    Debug.Log("Damage given: " + dmgResult);
                    yield return new WaitForSeconds(entity.cooldown);
                }
            }
            else
            {
                entity.currentHealth = entity.maxHealth;
                entity.combatCoroutine = false;
                StopCoroutine(combatCoroutine);
            }

            yield return null;
        }
    }

    void Die()
    {
        entity.dead = true;
        entity.inCombat = false;
        entity.target = null;

        animator.SetBool("isWalking", false);
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.GainExp(rewardExperience);

        StopAllCoroutines();
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        GameObject newMonster = Instantiate(gameObject, initialPosition, transform.rotation, null);
        newMonster.name = gameObject.name;
        Destroy(gameObject);
    }
}
