using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum UnitType
    {
        Ranged,
        Melee,
        Settler,
    }
    public enum TurnAction
    {
        Move,
        Skip,
        Sleep,
        Attack,
        Settle,
    }
    
    [SerializeField] protected UnitType unitType;
    [SerializeField] protected TurnAction turnAction;
    [SerializeField] protected Vector2 position;
    [SerializeField] protected Player owningPlayer;
    [SerializeField] protected float health;
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float attackDamage;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float stamina;
    [SerializeField] protected float attackRange;
    [SerializeField] protected bool canDamage;
    [SerializeField] protected bool isSleep;
    [SerializeField] protected List<Tile> moveQueue;
    [SerializeField] protected List<TurnAction> turnActions;

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            DoDeath();
        }
    }

    protected void Start()
    {
        TurnController.Instance.OnNextTurn += nextTurn;
    }

    void nextTurn()
    {
        turnAction = turnActions.Count > 0 ? turnActions[0] : TurnAction.Skip; // Default to Skip if no actions queued
        switch (turnAction)
        {
            case TurnAction.Move:
                Move();
                isSleep = false;
                break;
            case TurnAction.Skip:
                isSleep = false;
                break;
            case TurnAction.Sleep:
                break;
            case TurnAction.Attack:
                isSleep = false;
                // Attack logic should be handled externally since it requires a target
                break;
            case TurnAction.Settle:
                isSleep = false;
                // Settling logic should be handled externally since it requires a location
                break;
        }
        turnActions.RemoveAt(0);
    }
    
    public void Attack(Unit target)
    {
        if (DistanceTo(this, target) <= attackRange && canDamage)
        {
            target.TakeDamage(attackDamage * (health / maxHealth));
            turnActions.Add(TurnAction.Attack);
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

    void Move()
    {
        if (moveQueue.Count > 0 && !isSleep)
        {
            position = moveQueue[0].Position; 
            StartCoroutine(DoMove(moveQueue[0].Position));
            moveQueue.RemoveAt(0);
            turnActions.Add(TurnAction.Move);
        }  
    }

    public void Move(Vector2 newPosition)
    {
        Vector2 worldPos = new Vector2(Helpers.TileToWorldPos(position).x, Helpers.TileToWorldPos(position).z);
        Vector2 targetWorldPos = new Vector2(Helpers.TileToWorldPos(newPosition).x, Helpers.TileToWorldPos(newPosition).z);
        
        moveQueue = new List<Tile>(PathfindingGraph.Instance.GetPath(worldPos, targetWorldPos));
        if (moveQueue.Count > 0)
        {
            position = moveQueue[0].Position; 
            StartCoroutine(DoMove(moveQueue[0].Position));
            moveQueue.RemoveAt(0);
            turnActions.Add(TurnAction.Move);
        }
    }

    public IEnumerator DoMove(Vector2 tilePos)
    {
        Vector3 targetPosition = Helpers.TileToWorldPos(tilePos);
    
        while(Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
    
        // Snap to exact position
        transform.position = targetPosition;
    }

    public void Sleep()
    {
        isSleep = true;
        turnActions.Add(TurnAction.Sleep);
    }

    public void Skip()
    {
        // add blank to execution queue
        turnActions.Add(TurnAction.Skip);
    }
}
