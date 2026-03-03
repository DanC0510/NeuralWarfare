using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum UnitType
    {
        Ranged,
        Melee,
        Settler,
    }
    [SerializeField] protected UnitType unitType;
    [SerializeField] protected Vector2 position;
    [SerializeField] protected Player owningPlayer;
    [SerializeField] protected float health;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float attackDamage;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float attackRange;
    [SerializeField] protected bool canDamage;
    [SerializeField] protected bool isSleep;

    public void TakeDamage(float damage, Unit unitSource)
    {
        if (unitSource.canDamage && DistanceTo(this, unitSource) <= unitSource.attackRange)
        {
            health -= damage;
            if (health <= 0)
            {
                DoDeath();
            }
        }
    }
    
    public Vector2 Position
    {
        get { return position; }
    }

    public UnitType Type
    {
        get { return unitType; }
    }

    public Player OwningPlayer
    {
        get { return OwningPlayer; }
    }

    public float DistanceTo(Unit unit1, Unit unit2)
    {
        return Vector2.Distance(unit1.position, unit2.position);
    }

    public void DoDeath()
    {
        //play death anim here
    }

    public void Move()
    {
        StartCoroutine(DoMove(position));
    }

    public IEnumerator DoMove(Vector2 moveTowards)
    {
        Vector3 targetPosition = new Vector3(moveTowards.x, 0.5f, moveTowards.y);
    
        while(Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            Vector2 temp = Vector2.MoveTowards(new Vector2(transform.position.x, transform.position.z), moveTowards, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(temp.x, 0.5f, temp.y);
            yield return null;
        }
    
        // Snap to exact position
        transform.position = targetPosition;
    }
    
    public void Attack(Unit target)
    {
        if (DistanceTo(this, target) <= attackRange)
        {
            target.TakeDamage(attackDamage * (health / maxHealth), this);
        }
    }

    public void Sleep()
    {
        isSleep = true;
    }

    public void Skip()
    {
        // add blank to execution queue
    }
}
