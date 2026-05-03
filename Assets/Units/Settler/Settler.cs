using UnityEngine;

public class Settler : Unit
{
    [SerializeField] private GameObject cityPrefab;
    public void CreateSettler(int startingCityIndex, Player player)
    {
        owningPlayer = player;
        unitType = UnitType.Settler;
        health = maxHealth = 1;
        if (startingCityIndex != -1)
        {
            position = owningPlayer.Cities[startingCityIndex].GetSpawnPosition();
        }
        else
        {
            Tile spawnTile = Helpers.GetTileAtWorldPos(new Vector2(Random.Range(80, 230), Random.Range(80, 145)));
            while (spawnTile.Biome == Tile.TileBiome.Deep_Water || spawnTile.Biome == Tile.TileBiome.Water)
            {
                spawnTile = Helpers.GetTileAtWorldPos(new Vector2(Random.Range(80, 230), Random.Range(80, 145)));
            }
            position = spawnTile.Position;  // Random position if no cities exist
            Vector3 temp = Helpers.TileToWorldPos(position);
            owningPlayer.transform.position = new Vector3(temp.x, 10f, temp.z - 10f);
            if (!owningPlayer.IsAI)
            {
                owningPlayer.GetComponent<PlayerController>().MoveTo = new Vector3(temp.x, 10f, temp.z - 10f);
            }
        }
        canDamage = false;
        moveSpeed = 2.0f;
        transform.position = Helpers.TileToWorldPos(position);
        stamina = maxStamina = 2f;
        Helpers.GetTileAtTilePos(position).IsOccupied = true;
    }

    public void PlaceCity()
    {
        Vector2 pos = Helpers.TileToWorldPos(position);
        if (owningPlayer.Capital == null)
        {
            GameObject city = Instantiate(cityPrefab, new Vector3(pos.x, 0f, pos.y), Quaternion.identity);
            city.GetComponent<City>().CreateCity(position, owningPlayer, 3);
            owningPlayer.Capital = city.GetComponent<City>();
            owningPlayer.Cities.Add(city.GetComponent<City>());
        }
        else
        {
            GameObject city = Instantiate(cityPrefab, new Vector3(pos.x, 0f, pos.y), Quaternion.identity);
            city.GetComponent<City>().CreateCity(position, owningPlayer, 2);
            owningPlayer.Cities.Add(city.GetComponent<City>());
        }
        DoDeath();
    }
}
