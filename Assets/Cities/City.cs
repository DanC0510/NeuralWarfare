using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class City : MonoBehaviour
{
    [SerializeField] private Vector2 position;
    [SerializeField] private Player owningPlayer;
    [SerializeField] private int population;
    [SerializeField] private float productionRate;
    [SerializeField] private float productionProgress;
    [SerializeField] private GameObject cityPrefab;
    [SerializeField] private float cityHealth;
    [SerializeField] private float maxHealth;
    [SerializeField] private HashSet<Tile> tilesOwned;
    public HashSet<Tile> TilesOwned => tilesOwned;

    public void CreateCity(Vector2 startingPosition, Player player, float radiusOwned)
    {
        position = startingPosition;
        owningPlayer = player;
        population = 1;
        productionRate = 1f;
        productionProgress = 0f;
        
        //Giving city control of tiles
        tilesOwned = new HashSet<Tile>();
        tilesOwned.Add(Helpers.GetTileAtTilePos(position));
        for (int i = 0; i < radiusOwned; i++)
        {
            HashSet<Tile> temp = new HashSet<Tile>(tilesOwned);
            foreach (Tile tile in temp)
            {
                for(int j = 0; j < tile.NeighbourTiles.Length; j++)
                {
                    Tile neighbour = tile.NeighbourTiles[j];
                    if (neighbour != null && !tilesOwned.Contains(neighbour))
                    {
                        tilesOwned.Add(neighbour);
                    }
                }
            }
        }

    }
    
    public Player OwningPlayer
    {
        get { return owningPlayer; }
    }

    public Vector2 GetSpawnPosition()
    {
        // Return a position near the city centre for spawning units
        return position + new Vector2(1, 0);
    }
    
    public void TakeDamage(float damage, Unit unitSource)
    {
        cityHealth -= damage;
        if (cityHealth <= 0)
        {
            DoTakeover(unitSource.OwningPlayer);
        }
    }
    
    void DoTakeover(Player playerSource)
    {
        owningPlayer = playerSource;
        cityHealth = maxHealth / 10; //set to 1/10th after takeover
    }

    public void ProduceUnit()
    {
        productionProgress += productionRate * Time.deltaTime;
        if (productionProgress >= 1f)
        {
            productionProgress = 0f; // Reset progress after producing a unit
        }
    }
}
