using UnityEngine;

public class Settler : Unit
{
    public void CreateSettler(int startingCityIndex, Player player)
    {
        owningPlayer = player;
        unitType = UnitType.Settler;
        health = 1;
        maxHealth = health;
        if (startingCityIndex != 0)
        {
            position = owningPlayer.Cities[startingCityIndex].GetSpawnPosition();
        }
        else
        {
            position = Helpers.GetTileAtWorldPos(new Vector2(Random.Range(80, 460), Random.Range(80, 290))).Position;  // Random position if no cities exist
            owningPlayer.transform.position = new Vector3(position.x, 10f, position.y);
        }
        canDamage = false;
        moveSpeed = 2.0f;
        transform.position = new Vector3(position.x, 0.5f, position.y);
        
    }

    public void PlaceCity()
    {
        if (owningPlayer.Capital.OwningPlayer != owningPlayer)
        {
            owningPlayer.Capital = new City(position, owningPlayer);
            owningPlayer.Cities.Add(owningPlayer.Capital);
        }
        else
        {
            owningPlayer.Cities.Add(new City(position, owningPlayer));
        }
        Destroy(this.gameObject);
    }
}
