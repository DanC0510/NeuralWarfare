using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum UnitType
    {
        Ranged,
        Melee,
        Settler,
        Builder,
    }
    public enum TurnAction
    {
        No_Action,
        Move,
        Skip,
        Sleep,
        Attack,
        Build,
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
    [SerializeField] protected Tile currentTile;
    [SerializeField] protected List<Tile> moveQueue;
    [SerializeField] protected List<TurnAction> turnActions;
    [SerializeField] protected float productionCost;
    public List<TurnAction> TurnActions { get { return turnActions; } }
    public TurnAction Action { get { return turnAction; } }
    public Vector2 Position { get { return position; } }
    public UnitType Type { get { return unitType; } }
    public Player OwningPlayer { get { return owningPlayer; } }
    public float Health { get { return health; } }
    public float MaxHealth { get { return maxHealth; } }
    public float AttackRange { get { return attackRange; } }
    public float ProductionCost { get { return productionCost; } }
    public UnitType GetUnitType { get { return unitType; } }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            DoDeath();
            if (owningPlayer.PlayerIndex == 0) //if player unit dies, then reward AI
            {
                owningPlayer.Opponent.GetComponent<AIController>().OnUnitDestroyed();
            } 
            else if(owningPlayer.PlayerIndex == 1) //if AI unit dies, punish AI
            {
                owningPlayer.Opponent.GetComponent<AIController>().OnUnitLost();
            }//the call is made here so i can implement destroying later down the line and just recycle the DoDeath function.
        }
    }

    protected void Start()
    {
        TurnController.Instance.OnNextTurn += nextTurn;
        TurnController.Instance.Register(this);
    }

    protected void nextTurn()
    {
        if (turnActions.Count == 0) { return; }
        turnAction = turnActions[0]; // Default to Skip if no actions queued
        turnActions.RemoveAt(0);
        stamina = maxStamina;
        switch (turnAction)
        {
            case TurnAction.Move:
                Move();
                break;
            case TurnAction.Skip:
                moveQueue.Clear();
                turnActions.Clear();
                turnAction = TurnAction.No_Action;
                TurnController.Instance.FinishTurnEvent(this);
                break;
            case TurnAction.Sleep:
                Sleep();
                TurnController.Instance.FinishTurnEvent(this);
                break;
            case TurnAction.Attack:
                TurnController.Instance.FinishTurnEvent(this);
                break;
        }
        currentTile = Helpers.GetTileAtTilePos(position);
    }
    
    public void Attack(object target)
    {
        Debug.Log("Starting Attack");
        if (turnAction != TurnAction.Attack) { turnActions.Clear(); }
        if(target.GetType().BaseType == typeof(Unit))
        {
            Unit targetUnit = (Unit)target;
            if (DistanceTo(this, targetUnit) <= attackRange && canDamage)
            {
                Debug.Log("Attacking Unit");
                targetUnit.TakeDamage(attackDamage * (health / maxHealth));
                turnActions.Insert(0, TurnAction.Attack);
            }
        }
        else if(target.GetType() == typeof(City))
        {
            City targetCity = (City)target;
            if (DistanceTo(this, targetCity) <= attackRange && canDamage)
            {
                Debug.Log("Attacking City");
                targetCity.TakeDamage(attackDamage * (health / maxHealth), OwningPlayer);
                turnActions.Insert(0, TurnAction.Attack);
            }
        }


        stamina = 0;
        TurnController.Instance.FinishTurnEvent(this);
    }

    public float DistanceTo(Unit unit, object obj)
    {
        float distance = -10;
        if (obj.GetType().BaseType == typeof(Unit))
        {
            Unit targetUnit = (Unit)obj;
            distance = PathfindingGraph.Instance.GetPath(unit.position, targetUnit.position).Count - 1;
            
        }
        else if(obj.GetType() == typeof(City))
        {
            City targetCity = (City)obj;
            distance = PathfindingGraph.Instance.GetPath(unit.position, targetCity.Position).Count - 1;
        }
        return distance;
        
    }

    public void DoDeath()
    {
        TurnController.Instance.OnNextTurn -= nextTurn;
        TurnController.Instance.Unregister(this);
        //play death anim here
        Destroy(this.gameObject);
    }

    void Move()
    {
        if (moveQueue.Count == 0)
        {
            turnActions.Clear();
            turnAction = TurnAction.No_Action;
            TurnController.Instance.FinishTurnEvent(this);
            return;
        }
        currentTile.IsOccupied = false;
        if (moveQueue.Count > 0)
        {
            if (moveQueue[0].IsOccupied)
            {
                //repath if next tile is occupied
                Tile last = moveQueue.Last();
                moveQueue.Clear();
                Vector2 worldPos = new Vector2(Helpers.TileToWorldPos(position).x, Helpers.TileToWorldPos(position).z);
                Vector2 targetWorldPos = new Vector2(Helpers.TileToWorldPos(last.Position).x, Helpers.TileToWorldPos(last.Position).z);
                List<Tile> path = PathfindingGraph.Instance.GetPath(worldPos, targetWorldPos);
                if (path == null)
                {
                    // Can't find path, skip turn
                    TurnController.Instance.FinishTurnEvent(this);
                    return;
                }
                moveQueue = new List<Tile>(path);
            }
            
            if (moveQueue.Count == 0) //if moveQueue is still empty after trying to repath
            {
                TurnController.Instance.FinishTurnEvent(this);
                return;
            }
            
            Tile next = moveQueue[0];
            position = next.Position;
            if(stamina - next.MovementCost < 0)
            {
                //not enough stamina to move, skip turn instead
                TurnController.Instance.FinishTurnEvent(this);
                return;
            }
            StartCoroutine(DoMove(next.Position));
            next.IsOccupied = true;
            if (moveQueue.Count > 0)
            {
                moveQueue.RemoveAt(0);
            }

            turnActions.Insert(0, TurnAction.Move);
        }  
    }

    public void GetPath(Vector2 newPosition)
    {
        Vector2 worldPos = new Vector2(Helpers.TileToWorldPos(position).x, Helpers.TileToWorldPos(position).z);
        Vector2 targetWorldPos = new Vector2(Helpers.TileToWorldPos(newPosition).x, Helpers.TileToWorldPos(newPosition).z);
        
        moveQueue = new List<Tile>(PathfindingGraph.Instance.GetPath(worldPos, targetWorldPos));
        if(moveQueue == null || moveQueue.Count == 0)
        {
            // No path found
            TurnController.Instance.FinishTurnEvent(this);
            return;
        }
        if (moveQueue.Count > 0 && moveQueue[0].Position == position) { moveQueue.RemoveAt(0); }
        if (turnAction != TurnAction.Move) { turnActions.Clear(); }
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

        if (moveQueue.Count > 0 && stamina >= moveQueue[0].MovementCost)
        {
            Move(); // Continue moving without finishing turn
        }
        else
        {
            TurnController.Instance.FinishTurnEvent(this); // End turn when no stamina or no more moves
        }
    }

    public void Sleep()
    {
        moveQueue.Clear();
        turnActions.Clear();
        turnActions.Insert(0, TurnAction.Sleep);
    }

    public void Skip()
    {
        if (turnAction != TurnAction.Skip) { turnActions.Clear(); }
        turnActions.Insert(0, TurnAction.Skip);
    }
}
