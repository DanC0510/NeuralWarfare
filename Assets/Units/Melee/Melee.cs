using UnityEngine;

public class Melee : Unit
{
    public void CreateMelee(Vector2 startingPosition, Player player)
    {
        owningPlayer = player;
        unitType = UnitType.Melee;
        health = 10;
        maxHealth = health;
        position = startingPosition;
        canDamage = true;
        attackDamage = 3f;
        moveSpeed = 3.0f;
        attackRange = 1.0f;
        transform.position = Helpers.TileToWorldPos(position);
        stamina = maxStamina = 2f;
    }
}
