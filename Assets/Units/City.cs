using System;
using UnityEngine;

[Serializable]
public class City
{
    [SerializeField] private Vector2 position;
    [SerializeField] private Player owningPlayer;
    [SerializeField] private int population;
    [SerializeField] private float productionRate;
    [SerializeField] private float productionProgress;
    [SerializeField] private GameObject cityPrefab;
    [SerializeField] private float cityHealth;
    [SerializeField] private float maxHealth;

    public City(Vector2 startingPosition, Player player)
    {
        position = startingPosition;
        owningPlayer = player;
        population = 1;
        productionRate = 1f;
        productionProgress = 0f;
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
