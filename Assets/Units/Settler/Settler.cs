using UnityEngine;

public class Settler : Unit
{
    public void CreateSettler(int startingCityIndex, Player player)
    {
        owningPlayer = player;
        unitType = UnitType.Settler;
        health = 1;
        if (startingCityIndex != 0)
        {
            position = owningPlayer.Cities[startingCityIndex].GetSpawnPosition();
        }
        else
        {
            position = new Vector2(Random.Range(80, 460), Random.Range(80, 290));  // Random position if no cities exist
        }
        canDamage = false;
        moveSpeed = 2.0f;
    }

    public void PlaceCity()
    {
        if (owningPlayer.Capital != null)
        {
            owningPlayer.Capital = new City(position, owningPlayer);
            owningPlayer.Cities.Add(owningPlayer.Capital);
            return;
        }
        owningPlayer.Cities.Add(new City(position, owningPlayer));
    }
}
