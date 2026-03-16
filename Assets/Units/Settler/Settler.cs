using UnityEngine;

public class Settler : Unit
{
    [SerializeField] private GameObject cityPrefab;
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
            Tile spawnTile = Helpers.GetTileAtWorldPos(new Vector2(Random.Range(80, 460), Random.Range(80, 290)));
            while (spawnTile.Biome == Tile.TileBiome.Deep_Water || spawnTile.Biome == Tile.TileBiome.Water)
            {
                spawnTile = Helpers.GetTileAtWorldPos(new Vector2(Random.Range(80, 460), Random.Range(80, 290)));
            }
            position = spawnTile.Position;  // Random position if no cities exist
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

        if (cityPrefab != null)
        {
            Instantiate(cityPrefab, Helpers.TileToWorldPos(position), Quaternion.identity);
        }
        DoDeath();
    }
}
