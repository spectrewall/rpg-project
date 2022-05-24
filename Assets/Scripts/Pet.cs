using System.Collections.Generic;
using UnityEngine;

public class Pet : MonoBehaviour
{
    public float petMoveSpeed = 5f;
    public float keepDistance = 1f;
    public float maxDistanceToOwner = 5f;

    bool isWalking;

    float input_x;
    float input_y;

    Animator petAnimator;
    Rigidbody2D rb2D;

    GameObject owner;

    Vector2 ownerBack;
    Vector2 ownerLastPosition;
    Vector2 ownerMoveDirection;

    float heightDifferenceBottom;

    private void Start()
    {
        // Get pet's components
        petAnimator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();

        // Get pet's owner
        owner = transform.parent.gameObject;

        // Calculete height difference between pet and owner
        float petHeight = GetComponent<SpriteRenderer>().bounds.size.y;
        float ownerHeight = owner.GetComponent<SpriteRenderer>().bounds.size.y;

        float heightDifference = ownerHeight - petHeight;
        heightDifferenceBottom = heightDifference / 2;

        // Makes Pet ignore collision with owner and enemys
        List<GameObject> entities = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        entities.Add(owner);
        entities.ForEach(entity => {
            Physics2D.IgnoreCollision(entity.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        });

        // Defines start position
        ownerBack = getNewOwnerBack(0, -1, owner.transform.position);
        ownerLastPosition = owner.transform.position;
    }

    private void Update()
    {
        // Calculates the direction the owner is moving
        if (!ownerLastPosition.Equals((Vector2)owner.transform.position))
            ownerMoveDirection = ((Vector2)owner.transform.position - ownerLastPosition).normalized;
        else
        {
            if (ownerMoveDirection.x != 0 && ownerMoveDirection.y != 0)
                ownerMoveDirection.Set(0, ownerMoveDirection.y);
        }

        input_x = ownerMoveDirection.x;
        input_y = ownerMoveDirection.y;

        // Defines if it isWalking
        float distanceToOwner = Vector2.Distance(rb2D.position, ownerBack);
        int distanceToOwner10x = Mathf.RoundToInt(distanceToOwner * 10f);
        float distanceToOwnerOneDecimal = distanceToOwner10x / 10f;

        isWalking = distanceToOwnerOneDecimal >= 0.2f;

        if (isWalking)
        {
            petAnimator.SetFloat("input_x", input_x);
            petAnimator.SetFloat("input_y", input_y);
        }

        petAnimator.SetBool("isWalking", isWalking);

        // Get the new position the pet most go
        ownerBack = getNewOwnerBack(input_x, input_y, ownerBack);

        // Teleports the pet if it's too far from owner
        if (distanceToOwner >= maxDistanceToOwner)
        {
            transform.position = ownerBack;
        }
    }

    private void FixedUpdate()
    {
        rb2D.MovePosition(rb2D.position + (petMoveSpeed * Time.fixedDeltaTime * (ownerBack - rb2D.position)));
        ownerLastPosition = owner.transform.position;
    }

    /**
     * Calculates the position right behind the owner of the pet
     * according to the distance the pet must keep from owner and
     * the pet height, also based on the direction the owner is moving
     */
    Vector2 getNewOwnerBack(float input_x, float input_y, Vector2 currentOwnerBack)
    {
        float ownerPositionX = owner.transform.position.x;
        float ownerPositionY = owner.transform.position.y;

        if (input_x < 0)
        {
            currentOwnerBack.x = ownerPositionX + keepDistance;
            ownerPositionY -= heightDifferenceBottom;
        }
        else if (input_x > 0)
        {
            currentOwnerBack.x = ownerPositionX - keepDistance;
            ownerPositionY -= heightDifferenceBottom;
        }
        else if (input_x == 0 && input_y != 0)
        {
            currentOwnerBack.x = ownerPositionX;
        }

        if (input_y < 0)
        {
            currentOwnerBack.y = ownerPositionY + keepDistance;
            currentOwnerBack.y -= heightDifferenceBottom;
        }
        else if (input_y > 0)
        {
            currentOwnerBack.y = ownerPositionY - keepDistance;
        }
        else if (input_y == 0 && input_x != 0)
        {
            currentOwnerBack.y = ownerPositionY;
        }

        return currentOwnerBack;
    }
}
