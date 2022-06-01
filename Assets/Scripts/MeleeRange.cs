using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeRange : MonoBehaviour
{
    CircleCollider2D circle;
    BoxCollider2D box;
    Entity selfEntity;
    List<Entity> everyoneInRange;
    Vector3 difference;
    GameObject animSpawn;

    void Start()
    {
        circle = GetComponent<CircleCollider2D>();
        box = GetComponent<BoxCollider2D>();
        selfEntity = gameObject.GetComponentInParent<Entity>();
        everyoneInRange = new List<Entity>();
        animSpawn = transform.GetChild(0).gameObject;

        transform.localScale = new Vector3(selfEntity.attackDistance, selfEntity.attackDistance, 1);
    }

    void Update()
    {
        Vector3 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        difference = diff.normalized;
        float rotation_z = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotation_z - 45f);
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
        if (!other.isTrigger && notMeCondition(other) && other.TryGetComponent(out Entity entity) && everyoneInRange.Contains(entity) && (!inRangeCondition(other) || entity.dead))
            everyoneInRange.Remove(entity);
    }

    public List<Entity> getEveryoneInRange()
    {
        return everyoneInRange;
    }

    public void performAttack(GameObject atkAnim)
    {
        Vector3 diff = difference;
        float rotation_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, rotation_z - 180f);

        Instantiate(atkAnim, transform.position, rotation);
    }

    public Vector2 getDirection()
    {
        return (Vector2)(animSpawn.transform.position - transform.position).normalized;
    }

    public void clearList()
    {
        everyoneInRange.Clear();
    }
}
