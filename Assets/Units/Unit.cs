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
    [SerializeField] protected UnitType unitType;
    [SerializeField] protected GameObject unitPrefab;
    [SerializeField] protected Vector2 position;
    [SerializeField] protected Player owningPlayer;
    [SerializeField] protected float health;
    [SerializeField] protected float attackDamage;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float attackRange;
    [SerializeField] protected bool canDamage;

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
}
