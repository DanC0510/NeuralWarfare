using Unity.VisualScripting;
using UnityEngine;

public class Unit : MonoBehaviour
{
    protected enum UnitType
    {
        Ranged,
        Melee,
        Settler,
    }
    protected UnitType unitType;
    protected GameObject unitPrefab;
    protected Vector2 position;
    protected float health;
    protected float attackDamage;
    protected float moveSpeed;
    protected float attackRange;
    protected bool canDamage;

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
    
    public float DistanceTo(Unit unit1, Unit unit2)
    {
        return Vector2.Distance(unit1.position, unit2.position);
    }

    public void DoDeath()
    {
        //play death anim here
    }
}
