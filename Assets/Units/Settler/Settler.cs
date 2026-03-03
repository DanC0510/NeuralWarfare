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
        transform.position = new Vector3(position.x, 0.5f, position.y);
        owningPlayer.transform.position = new Vector3(position.x, 10f, position.y);
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
