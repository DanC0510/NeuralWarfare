using UnityEngine;

public class Settler : Unit
{ 
    public Settler(int startingCityIndex, Player player)
    {
        owningPlayer = player;
        unitType = UnitType.Settler;
        health = 1;
        if (owningPlayer.Capital != null)
        {
            position = owningPlayer.Cities[startingCityIndex].GetSpawnPosition();
        }
        canDamage = false;
        moveSpeed = 2.0f;
    }
}
