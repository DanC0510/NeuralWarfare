using UnityEngine;

public class Melee : Unit
{
    public void CreateMelee(Vector2 startingPosition, Player player)
    {
        owningPlayer = player;
        unitType = UnitType.Melee;
        health = maxHealth = 10;
        canDamage = true;
        attackDamage = 3f;
        attackRange = 1.0f;
        moveSpeed = 2.0f;
        stamina = maxStamina = 2f;
        position = startingPosition;
        transform.position = Helpers.TileToWorldPos(position);
        Helpers.GetTileAtTilePos(position).IsOccupied = true;
    }
}
