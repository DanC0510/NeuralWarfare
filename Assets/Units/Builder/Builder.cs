using UnityEngine;

public class Builder : Unit
{
    [SerializeField] private float buildCharges = 2f;
    
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
        Helpers.GetTileAtTilePos(position).IsOccupied = true;
    }

    public void Build()
    {
        Tile currentTile = Helpers.GetTileAtTilePos(position);
        if (!currentTile.HasResources) { return; }
        currentTile.Build();
        removeBuildCharge();
    }

    private void removeBuildCharge()
    {
        if (stamina < 1) { return; }
        stamina = 0;
        buildCharges--;
        if(buildCharges == 0) { DoDeath(); }
    }
}