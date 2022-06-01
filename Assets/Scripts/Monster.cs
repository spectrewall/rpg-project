using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Monster : Entity
{
    [Header("Patrol")]
    public List<Transform> waypointList;
    public float arrivalDistance = 0.5f;
    public float waitTime = 5;
    public int waypointID;

    [Header("UI")]
    public Slider healthSlider;
    public Text nameTag;

    Transform targetWaypoint;
    int currentWaypoint = 0;
    float lastDistanceToTarget = 0f;
    float currentWaitTime = 0f;
    Vector2 direction;
    Coroutine combatCoroutine;

    private new void Start()
    {
        base.Start();
        respawnPosition = transform.position;        

        // UI Setup
        healthSlider.maxValue = maxHealth;
        healthSlider.value = healthSlider.maxValue;
        nameTag.text = entityName ?? gameObject.name;

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

    private new void Update()
    {
        base.Update();

        healthSlider.value = currentHealth;

        if (target == null)
        {
            if (waypointList.Count > 0) Patrol();
            else animator.SetBool("isWalking", false);
        }
        else
            FollowTarget();
    }

    private void FixedUpdate()
    {
        if (dead) return;
        if (!knockedback) rb2D.MovePosition(rb2D.position + direction * (speed * Time.fixedDeltaTime));
    }

    void Patrol()
    {
        if (dead) return;

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
        if (dead) return;
        if (target.GetComponent<Entity>().dead)
        {
            target = null;
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

        if (distanceToTarget <= attackDistance)
        {
            animator.SetBool("isWalking", false);
            direction = Vector2.zero;
            if (!combatCoroutineIsRunning) combatCoroutine = StartCoroutine(Attack());
        }
        else
        {
            direction = (target.transform.position - transform.position).normalized;
            animator.SetBool("isWalking", true);
            animator.SetFloat("input_x", direction.x);
            animator.SetFloat("input_y", direction.y);
        }
    }

    IEnumerator Attack()
    {
        combatCoroutineIsRunning = true;
        while (true)
        {
            if(target != null && !target.GetComponent<Entity>().dead)
            {
                float distance = Vector2.Distance(transform.position, target.transform.position);

                if (distance <= attackDistance)
                {
                    animator.SetBool("attack", true);
                    int monsterDmg = manager.CalculateDamage(this, damage);
                    int targetDef = manager.CalculateDefense(target.GetComponent<Entity>(), target.GetComponent<Entity>().defense);
                    int dmgResult = monsterDmg - targetDef;

                    if (dmgResult < 0)
                        dmgResult = 0;

                    target.GetComponent<Entity>().currentHealth -= dmgResult;
                    yield return new WaitForSeconds(cooldown);
                }
            }
            else
            {
                currentHealth = maxHealth;
                combatCoroutineIsRunning = false;
                StopCoroutine(combatCoroutine);
            }

            yield return null;
        }
    }
}
