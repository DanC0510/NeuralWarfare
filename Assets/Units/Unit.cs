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
        No_Action,
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
    [SerializeField] protected float maxStamina;
    [SerializeField] protected float attackRange;
    [SerializeField] protected bool canDamage;
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

    protected void nextTurn()
    {
        if (turnActions.Count == 0) { return; }
        turnAction = turnActions.Count > 0 ? turnActions[0] : TurnAction.No_Action; // Default to Skip if no actions queued
        turnActions.RemoveAt(0);
        switch (turnAction)
        {
            case TurnAction.Move:
                Move();
                break;
            case TurnAction.Skip:
                moveQueue.Clear();
                turnActions.Clear();
                turnAction = TurnAction.No_Action;
                break;
            case TurnAction.Sleep:
                moveQueue.Clear();
                turnActions.Clear();
                turnActions.Add(TurnAction.Sleep);
                break;
            case TurnAction.Attack:
                stamina = 0;
                break;
            case TurnAction.Settle:
                
                break;
        }
        stamina = maxStamina;
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
        float distance = PathfindingGraph.Instance.GetPath(unit1.position, unit2.position).Count - 1;
        //Debug.Log(distance);
        return distance; //unit1.position and unit2.position are tile positions, not world positions#
    }

    public void DoDeath()
    {
        TurnController.Instance.OnNextTurn -= nextTurn;
        //play death anim here
        Destroy(this.gameObject);
    }

    void Move()
    {
        if (moveQueue.Count > 0)
        {
            position = moveQueue[0].Position; 
            StartCoroutine(DoMove(moveQueue[0].Position));
            moveQueue.RemoveAt(0);
            turnActions.Add(TurnAction.Move);
        }  
    }

    public void GetPath(Vector2 newPosition)
    {
        Vector2 worldPos = new Vector2(Helpers.TileToWorldPos(position).x, Helpers.TileToWorldPos(position).z);
        Vector2 targetWorldPos = new Vector2(Helpers.TileToWorldPos(newPosition).x, Helpers.TileToWorldPos(newPosition).z);
        
        moveQueue = new List<Tile>(PathfindingGraph.Instance.GetPath(worldPos, targetWorldPos)); 
        turnActions.Add(TurnAction.Move);
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
        stamina -= Helpers.GetTileAtTilePos(tilePos).MovementCost;
        if(moveQueue.Count != 0 && stamina - moveQueue[0].MovementCost >= 0)
        {
            StartCoroutine(DoMove(moveQueue[0].Position));
            stamina -= moveQueue[0].MovementCost;
            moveQueue.RemoveAt(0);
        }
    }

    public void Sleep()
    {
        turnActions.Add(TurnAction.Sleep);
    }

    public void Skip()
    {
        // add blank to execution queue
        turnActions.Insert(0, TurnAction.Skip);
    }
}
