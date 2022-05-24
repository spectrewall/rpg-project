using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeRange : MonoBehaviour
{
    CircleCollider2D circle;
    BoxCollider2D box;
    Entity selfEntity;
    List<Entity> everyoneInRange;

    void Start()
    {
        circle = GetComponent<CircleCollider2D>();
        box = GetComponent<BoxCollider2D>();
        selfEntity = gameObject.GetComponentInParent<Entity>();
        everyoneInRange = new List<Entity>();
    }

    void Update()
    {
        
    }

    bool notMeCondition(Collider2D other)
    {
        if (other.gameObject != gameObject && other.gameObject != transform.parent.gameObject)
            return true;

        return false;
    }

    bool validEntityCondition(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Entity entity) && !entity.dead)
        {
            if (entity.moralAlignment != selfEntity.moralAlignment && !entity.type.Equals("passive"))
            {
                if (!everyoneInRange.Contains(entity))
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool inRangeCondition(Collider2D other)
    {
        if (box.IsTouching(other) && circle.IsTouching(other))
            return true;

        return false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (
                !other.isTrigger &&
                notMeCondition(other) &&
                validEntityCondition(other) &&
                inRangeCondition(other)
            )
        {
            everyoneInRange.Add(other.GetComponent<Entity>());
        }
        else
        {
            removeFromListIfNotInRange(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        removeFromListIfNotInRange(other);
    }

    void removeFromListIfNotInRange(Collider2D other)
    {
        if (!other.isTrigger && notMeCondition(other) && other.TryGetComponent(out Entity entity) && everyoneInRange.Contains(entity) && !inRangeCondition(other))
            everyoneInRange.Remove(entity);
    }

    public List<Entity> getEveryoneInRange()
    {
        return everyoneInRange;
    }
}
