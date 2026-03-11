using UnityEngine;

public class Settler : Unit
{
    public void CreateSettler(int startingCityIndex, Player player)
    {
        owningPlayer = player;
        unitType = UnitType.Settler;
        health = maxHealth = 1;
        if (startingCityIndex != 0)
        {
            position = owningPlayer.Cities[startingCityIndex].GetSpawnPosition();
        }
        else
        {
            position = Helpers.GetTileAtWorldPos(new Vector2(Random.Range(80, 460), Random.Range(80, 290))).Position;  // Random position if no cities exist
            Vector3 temp = Helpers.TileToWorldPos(position);
            owningPlayer.transform.position = new Vector3(temp.x, 10f, temp.z);
        }
        canDamage = false;
        moveSpeed = 2.0f;
        transform.position = Helpers.TileToWorldPos(position);
        stamina = maxStamina = 2f;
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
