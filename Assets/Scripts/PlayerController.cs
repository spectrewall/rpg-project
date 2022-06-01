using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Player player;

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
    public GameObject attackAnimation;
    public MeleeRange meleeRange;

    [Header("Game Manager")]
    public GameManager manager;

    float input_x = 0f;
    float input_y = 0f;

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

    void Attack()
    {
        player.animator.SetTrigger("attack");
        player.attackTimer = player.cooldown;

        meleeRange.performAttack(attackAnimation);

        List<Entity> everyoneInRange = meleeRange.getEveryoneInRange();
        everyoneInRange.ForEach(entity =>
        {
            int totalDmg = manager.CalculateDamage(player, player.damage);
            int targetDef = manager.CalculateDefense(entity, entity.defense);
            int dmgResult = totalDmg - targetDef;

            if (dmgResult < 0)
                dmgResult = 0;

            entity.Knockback(player);
            entity.TakeDamage(player, dmgResult, 5);
        });

        meleeRange.clearList();
    }
}
