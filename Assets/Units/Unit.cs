using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    [SerializeField] protected Queue<Tile> moveQueue;

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            DoDeath();
        }
    }
    
    public void Attack(Unit target)
    {
        if (DistanceTo(this, target) <= attackRange && canDamage)
        {
            target.TakeDamage(attackDamage * (health / maxHealth));
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
        return Vector2.Distance(unit1.position, unit2.position); //change this after I have the graph set up to calculate pathfinding distance instead of straight line distance
    }

    public void DoDeath()
    {
        //play death anim here
        Destroy(this.gameObject);
    }

    public void Move(Vector2 newPosition)
    {
        moveQueue = new Queue<Tile>(PathfindingGraph.Instance.GetPath(position, newPosition));
        if (moveQueue.Count > 0)
        {
            StartCoroutine(DoMove(moveQueue.Dequeue().Position));
        }
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

    public void Sleep()
    {
        isSleep = true;
    }

    public void Skip()
    {
        // add blank to execution queue
    }
}
