using UnityEngine;

public class Builder : Unit
{
    public void CreateBuilder(Vector2 startingPosition, Player player)
    {
        owningPlayer = player;
        unitType = UnitType.Builder;
        health = maxHealth = 1;
        canDamage = false;
        attackDamage = 0f;
        attackRange = 0f;
        moveSpeed = 3f;
        stamina = maxStamina = 3f;
        position = startingPosition;
        transform.position = Helpers.TileToWorldPos(position);
    }
}