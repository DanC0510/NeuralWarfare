using UnityEngine;

public class City
{
    private Vector2 position;
    private Player owningPlayer;
    private int population;
    private float productionRate;
    private float productionProgress;
    private GameObject cityPrefab;
    private float cityHealth;
    private float maxHealth;

    public City(Vector2 startingPosition, Player player)
    {
        position = startingPosition;
        owningPlayer = player;
        population = 1;
        productionRate = 1f;
        productionProgress = 0f;
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
