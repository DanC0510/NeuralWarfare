using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class City : MonoBehaviour
{
    public enum CityAction
    {
        No_Action,
        Produce,
        ProduceDistrict,
        FortifyCity,
        CloseGates,
    }

    [SerializeField] private CityAction action; 
    [SerializeField] private List<CityAction> actionQueue;
    [SerializeField] private Vector2 position;
    [SerializeField] private Player owningPlayer;
    [SerializeField] private float population;
    [SerializeField] private float productionRate;
    [SerializeField] private float gold;
    [SerializeField] private float science;
    [SerializeField] private float productionProgress;
    [SerializeField] private GameObject cityPrefab;
    [SerializeField] private float cityHealth;
    [SerializeField] private float maxHealth;
    [SerializeField] private GameObject[] productionItems;
    [SerializeField] private GameObject productionPrefab;
    [SerializeField] private int numOfDistrictsInProductionItems;
    [SerializeField] private List<District> districts;
    private bool hasCampus;
    private bool hasEncampment;
    private HashSet<Tile> tilesOwned;
    List<Tuple<float, float, Tile>> bestTiles;
    public HashSet<Tile> TilesOwned => tilesOwned;
    public List<CityAction> ActionQueue => actionQueue;
    public Vector2 Position => position;
    public float Population { get => population; }
    public float Production { get => productionRate; }
    public float Gold { get => gold; }
    public float Science { get => science; }

    public void CreateCity(Vector2 startingPosition, Player player, float radiusOwned)
    {
        position = startingPosition;
        Helpers.GetTileAtTilePos(position).RemoveResource();
        transform.GetChild(0).position = new Vector3(Helpers.TileToWorldPos(position).x, 0.5f, Helpers.TileToWorldPos(position).z);
        owningPlayer = player;
        population = 1;
        productionRate = 1f;
        productionProgress = 0f;
        cityHealth = maxHealth = 25f;
        
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
    
    public void TakeDamage(float damage, Player source)
    {
        cityHealth -= damage;
        if (cityHealth <= 0)
        {
            DoTakeover(source);
            if (owningPlayer.PlayerIndex == 0) //if player unit dies, then reward AI
            {
                owningPlayer.Opponent.GetComponent<AIController>().OnUnitDestroyed();
            } 
            else if(owningPlayer.PlayerIndex == 1) //if AI unit dies, punish AI
            {
                owningPlayer.Opponent.GetComponent<AIController>().OnUnitLost();
            }//the call is made here so i can implement destroying later down the line and just recycle the DoDeath function.
        }
    }

    void Start()
    {
        TurnController.Instance.OnNextTurn += nextTurn;
        TurnController.Instance.Register(this);
        actionQueue = new List<CityAction>();
        bestTiles = new List<Tuple<float, float, Tile>>();
    }

    void nextTurn()
    {
        //calculate best tiles for use, capped at population
        foreach (Tile tile in tilesOwned)
        {
            bestTiles.Add(new Tuple<float, float, Tile>(tile.Production, tile.Food, tile));
            bestTiles.Sort((a, b) => (b.Item1 + b.Item2).CompareTo(a.Item1 + a.Item2));
        }
        
        for(int i = 0; i < population; i++)
        {
            bestTiles[i].Item3.IsWorked = true;
        }
        
        //calculate new production food and resource values each turn
        float pr = 0; float fd = 0; float gd = 0; float sc = 0;
        
        foreach (Tile tile in tilesOwned)
        {
            if(!tile.IsWorked) { continue; }
            pr += tile.Production;
            fd += tile.Food;
            gd += tile.Gold;
            sc += tile.Science;
        }
        
        productionRate = pr;
        population += (fd * 1.2f) / 20f;
        gold += gd;
        science = sc;
        
        //execute action queue
        if (actionQueue.Count == 0) { return; }
        action = actionQueue[0];
        switch (action)
        {
            case CityAction.Produce:
                ProduceUnit();
                break;
            case CityAction.ProduceDistrict:
                ProduceDistrict();
                break;
            case CityAction.FortifyCity:
                actionQueue.Add(CityAction.FortifyCity);
                cityHealth += 5;
                break;
            case CityAction.CloseGates:
                actionQueue.Add(CityAction.CloseGates);
                cityHealth += 10;
                productionRate -= 2f;
                break;
        }

        if (cityHealth >= maxHealth){ cityHealth = maxHealth; }
        if(action != CityAction.FortifyCity || action != CityAction.CloseGates) { actionQueue.RemoveAt(0); }
        //Finish Turn Event
        TurnController.Instance.FinishTurnEvent(this);
    }
    
    public Player OwningPlayer
    {
        get { return owningPlayer; }
    }

    public Vector2 GetSpawnPosition()
    {
        // Return a position near the city centre for spawning units
        if (hasEncampment)
        {
            foreach(District district in districts)
            {
                if (district.district == District.DistrictType.Encampment && district.tile.IsOccupied == false)
                {
                    return district.tile.Position;
                }
            }
        }
        foreach(Tile tile in bestTiles.Select(t => t.Item3))
        {
            if (!tile.IsOccupied && !tile.DistrictOnThisTile)
            {
                return tile.Position;
            }
        }
        return new Vector2(position.x + 0, position.y + 1);
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
        TurnController.Instance.Unregister(this);
        Destroy(gameObject);
    }

    public void FortifyCity()
    {
        if(actionQueue != null && actionQueue.Count != 0 && actionQueue[0] != CityAction.FortifyCity) { actionQueue.Clear(); }
        actionQueue.Add(CityAction.FortifyCity);
    }
    public void CloseCity()
    {
        if(actionQueue != null && actionQueue.Count != 0 && actionQueue[0] != CityAction.CloseGates) { actionQueue.Clear(); }
        actionQueue.Add(CityAction.CloseGates);
    }

    public void StartProduction(int index)
    {
        if(actionQueue != null && actionQueue.Count != 0 && (actionQueue[0] != CityAction.Produce || actionQueue[0] != CityAction.ProduceDistrict)) { actionQueue.Clear(); }
        int numUnits = productionItems.Length - numOfDistrictsInProductionItems;
        
        if (index >= 0 && index < numUnits)
        {
            QueueUnit(index);
        }
        else if (index >= numUnits && index < productionItems.Length)
        {
            int districtIndex = index - numUnits;
            QueueDistrict(districtIndex);
        }
    }

    public void QueueUnit(int index)
    {
        int numUnits = productionItems.Length - numOfDistrictsInProductionItems;
        int safeIndex = Helpers.mod(index, numUnits);
        productionPrefab = productionItems[safeIndex];
        actionQueue.Add(CityAction.Produce);
    }

    public void QueueDistrict(int index)
    {
        int districtStartIndex = productionItems.Length - numOfDistrictsInProductionItems; 
        int safeIndex = Helpers.mod(index, numOfDistrictsInProductionItems);
        productionPrefab = productionItems[districtStartIndex + safeIndex];
        actionQueue.Add(CityAction.ProduceDistrict);
    }

    void ProduceUnit()
    {
        productionProgress += productionRate;
        actionQueue.Add(CityAction.Produce);
        if (productionProgress >= productionPrefab.GetComponent<Unit>().ProductionCost)
        {
            GameObject newUnit = Instantiate(productionPrefab, Helpers.TileToWorldPos(GetSpawnPosition()), Quaternion.identity);

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
            productionProgress = 0f; // Reset progress after producing a unit
            actionQueue.Clear();
            actionQueue.Insert(0, CityAction.No_Action);
            productionPrefab = null;
        }
    }

    void ProduceDistrict()
    {
        productionProgress += productionRate;
        actionQueue.Add(CityAction.ProduceDistrict);
        
        if (productionProgress >= productionPrefab.GetComponent<District>().ProductionCost)
        {
            Vector2 spawnPos = GetSpawnPosition();
            for (int i = 0; i < population; i++)
            {
                Tile tile = bestTiles[i].Item3;
                if (tile.IsOccupied || tile.DistrictOnThisTile)
                {
                    continue;
                }
                spawnPos = tile.Position;
            }
            
            GameObject newDistrict = Instantiate(productionPrefab, Helpers.TileToWorldPos(spawnPos), Quaternion.identity);

            switch (newDistrict.GetComponent<District>().district)
            {
               case District.DistrictType.Campus:
                   newDistrict.GetComponent<District>().CreateDistrict(spawnPos, District.DistrictType.Campus, this);
                   hasCampus = true;
                   break;
               case District.DistrictType.Encampment:
                   newDistrict.GetComponent<District>().CreateDistrict(spawnPos, District.DistrictType.Encampment, this);
                   hasEncampment = true;
                   break;
            }
            districts.Add(newDistrict.GetComponent<District>());
            productionProgress = 0f; // Reset progress after producing a unit
            actionQueue.Clear();
            actionQueue.Insert(0, CityAction.No_Action);
            productionPrefab = null;
        }
    }
}
