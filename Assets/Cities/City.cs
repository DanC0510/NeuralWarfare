using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class City : MonoBehaviour
{
    public enum CityAction
    {
        Produce,
        FortifyCity,
        CloseGates,
    }

    [SerializeField] private CityAction action; 
    [SerializeField] private List<CityAction> actionQueue = new List<CityAction>();
    [SerializeField] private Vector2 position;
    [SerializeField] private Player owningPlayer;
    [SerializeField] private int population;
    [SerializeField] private float productionRate;
    [SerializeField] private float productionProgress;
    [SerializeField] private GameObject cityPrefab;
    [SerializeField] private float cityHealth;
    [SerializeField] private float maxHealth;
    [SerializeField] private HashSet<Tile> tilesOwned;
    [SerializeField] private GameObject[] productionItems;
    [SerializeField] private GameObject productionPrefab;
    public HashSet<Tile> TilesOwned => tilesOwned;
    public List<CityAction> ActionQueue => actionQueue;
    public Vector2 Position => position;

    public void CreateCity(Vector2 startingPosition, Player player, float radiusOwned)
    {
        position = startingPosition;
        transform.GetChild(0).position = new Vector3(Helpers.TileToWorldPos(position).x, 0.5f, Helpers.TileToWorldPos(position).z);
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
        
        CityBorderMeshRenderer border = GetComponent<CityBorderMeshRenderer>();
        if (border != null)
        {
            border.Rebuild();
        }
    }

    void Start()
    {
        TurnController.Instance.OnNextTurn += nextTurn;
        TurnController.Instance.RegisterCity(this);
    }

    void nextTurn()
    {
        if (actionQueue.Count == 0) { return; }
        action = actionQueue[0];
        switch (action)
        {
            case CityAction.Produce:
                ProduceUnit();
                break;
            case CityAction.FortifyCity:
                actionQueue.Clear();
                actionQueue.Add(CityAction.FortifyCity);
                cityHealth += 5;
                break;
            case CityAction.CloseGates:
                actionQueue.Clear();
                actionQueue.Add(CityAction.CloseGates);
                cityHealth += 10;
                productionRate -= 2f;
                break;
        }

        if (cityHealth >= maxHealth){ cityHealth = maxHealth; }
        if(action != CityAction.FortifyCity || action != CityAction.CloseGates) { actionQueue.RemoveAt(0); }
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
        
        CityBorderMeshRenderer border = GetComponent<CityBorderMeshRenderer>();
        if (border != null)
        {
            border.Rebuild();
        }
    }

    void RazeCity()
    {
        TurnController.Instance.OnNextTurn -= nextTurn;
        TurnController.Instance.UnregisterCity(this);
        Destroy(gameObject);
    }

    public void StartProduction(int index)
    {
        Debug.Log(index);
        productionPrefab = productionItems[index];
        actionQueue.Add(CityAction.Produce);
    }

    void ProduceUnit()
    {
        productionProgress += productionRate;
        actionQueue.Add(CityAction.Produce);
        if (productionProgress >= productionPrefab.GetComponent<Unit>().ProductionCost)
        {
            GameObject newUnit = Instantiate(productionPrefab, Helpers.TileToWorldPos(GetSpawnPosition()), Quaternion.identity);
            productionProgress = 0f; // Reset progress after producing a unit
            actionQueue.Clear();

            switch (newUnit.GetComponent<Unit>().Type)
            {
                case Unit.UnitType.Ranged:
                    newUnit.GetComponent<Archer>().CreateArcher(GetSpawnPosition(), owningPlayer);
                    break;
                case Unit.UnitType.Melee:
                    newUnit.GetComponent<Melee>().CreateMelee(GetSpawnPosition(), owningPlayer);
                    break;
                case Unit.UnitType.Settler:
                    newUnit.GetComponent<Settler>().CreateSettler(owningPlayer.Cities.IndexOf(this), owningPlayer);
                    break;
                case Unit.UnitType.Builder:
                    newUnit.GetComponent<Builder>().CreateBuilder(GetSpawnPosition(), owningPlayer);
                    break;
            }
        }
    }
}
