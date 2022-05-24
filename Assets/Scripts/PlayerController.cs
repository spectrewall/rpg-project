using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Player player;
    Entity selfEntity;

    [Header("Player Shortcuts")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode attributesKey = KeyCode.C;
    public KeyCode attackKey = KeyCode.Mouse0;

    [Header("Teleport")]
    public bool canTeleport = false;
    public Region tmpRegion;

    [Header("UI Panels")]
    public AttributesUI attributesPanel;

    [Header("Attack Area")]
    public GameObject attackArea;
    public MeleeRange meleeRange;

    [Header("Game Manager")]
    public GameManager manager;

    float input_x = 0f;
    float input_y = 0f;

    float lastInput_x;
    float lastInput_y = -1f;

    bool isWalking = false;
    Vector2 movement = Vector2.zero;

    void Start()
    {
        isWalking = false;
        player = GetComponent<Player>();
    }

    void Update()
    {
        // Walk Action
        input_x = Input.GetAxisRaw("Horizontal");
        input_y = Input.GetAxisRaw("Vertical");

        if (input_x != 0 && input_y == 0)
        {
            lastInput_x = input_x;
            lastInput_y = 0;
        }
        else if (input_y != 0 && input_x == 0)
        {
            lastInput_y = input_y;
            lastInput_x = 0;
        }
        else if (input_x != 0 && input_y != 0)
        {
            lastInput_y = input_y;
            lastInput_x = input_x;
        }

        isWalking = (input_x != 0 || input_y != 0);
        movement = new Vector2(input_x, input_y);

        if (isWalking)
        {
            player.animator.SetFloat("input_x", input_x);
            player.animator.SetFloat("input_y", input_y);
        }

        player.animator.SetBool("isWalking", isWalking);


        // Attack Action
        if (player.attackTimer < 0)
            player.attackTimer = 0;
        else
            player.attackTimer -= Time.deltaTime;

        if (Input.GetButtonDown("Fire1"))
        {
            if (player.attackTimer <= 0)
            {
                Attack();
            }
        }

        // Other Acations
        if (canTeleport && tmpRegion != null && Input.GetKeyDown(interactKey))
        {
            transform.position = tmpRegion.warpLocation.position;
        }

        if (Input.GetKeyDown(attributesKey))
        {
            attributesPanel.gameObject.SetActive(!attributesPanel.gameObject.activeSelf);
        }
    }

    private void FixedUpdate()
    {
        player.rb2D.MovePosition(player.rb2D.position + (player.speed * Time.fixedDeltaTime * movement));
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.tag == "Enemy" && collider.gameObject.TryGetComponent(out Entity entity))
        {
            player.target = entity;
        }

        if (collider.tag == "Teleport")
        {
            tmpRegion = collider.GetComponent<Teleport>().region;
            canTeleport = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.transform.tag == "Enemy")
        {
            player.target = null;
        }

        if (collider.tag == "Teleport")
        {
            tmpRegion = null;
            canTeleport = false;
        }
    }

    void Attack()
    {
        player.animator.SetTrigger("attack");
        player.attackTimer = player.cooldown;

        Vector3 attackPlace = (Vector3)((new Vector2(lastInput_x, lastInput_y) * player.attackDistance/2f) + (Vector2)transform.position);
        Vector2 direction = (attackPlace - transform.position) * 2;

        float rotation_z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, rotation_z - 180f);

        meleeRange.transform.localScale = new Vector3(player.attackDistance, player.attackDistance, 1);
        meleeRange.transform.rotation = Quaternion.Euler(0f, 0f, rotation_z - 45f);

        float modifier = 1;
        if (lastInput_x > 0) modifier = -1;

        GameObject atkObj = Instantiate(attackArea, attackPlace, rotation, transform);
        atkObj.transform.localScale = new Vector3(1, modifier, 1);

        List<Entity> everyoneInRange = meleeRange.getEveryoneInRange();
        everyoneInRange.ForEach(entity =>
        {
            int totalDmg = manager.CalculateDamage(player, player.damage);
            int targetDef = manager.CalculateDefense(entity, entity.defense);
            int dmgResult = totalDmg - targetDef;

            if (dmgResult < 0)
                dmgResult = 0;

            entity.currentHealth -= dmgResult;
        });
    }
}
