using UnityEngine;

public class Archer : Unit
{
    public void CreateArcher(Vector2 startingPosition, Player player)
    {
        owningPlayer = player;
        unitType = UnitType.Ranged;
        health = maxHealth = 8;
        canDamage = true;
        attackDamage = 2f;
        attackRange = 2.5f;
        moveSpeed = 2.5f;
        stamina = maxStamina = 2f;
        position = startingPosition;
        transform.position = Helpers.TileToWorldPos(position);
        Helpers.GetTileAtTilePos(position).IsOccupied = true;
    }
}
