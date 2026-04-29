using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Tile
{
    [SerializeField] private bool hasRiverAdjecent;
    [SerializeField] private Vector2 position;
    [SerializeField] private TileBiome biome;
    private LinkedList<Tile> riverTiles;
    private Tile[] neighbourTiles;
    [SerializeField] private bool isStartingRiver;
    [SerializeField] private Tile riverStart;
    [SerializeField] private Resource resourceOnThisTile;
    [SerializeField] private bool hasResources;
    [SerializeField] private float movementCost = 1f;
    [SerializeField] private bool isOccupied;
    [SerializeField] private District districtOnThisTile;
    [Header("Tile Attributes")]
    [SerializeField] private float food;
    [SerializeField] private float production;
    [SerializeField] private float gold;
    [SerializeField] private float science;
    [SerializeField] private bool isWorked;
    
    public enum TileBiome
    {
        Water = 0,
        Deep_Water = 1,
        Sand = 2,
        Grass = 3,
        Snow = 4,
        River = 5,
        //river 60 deg = 6
        //river 120 deg = 7
        //river start = 8
    };
    
    public Vector2 Position { get { return position; } }
    public TileBiome Biome { get { return biome; } set { biome = value; } }
    public Tile[] NeighbourTiles { get; set; } = new Tile[6];
    public float MovementCost { get => movementCost; }
    public Resource ResourceOnThisTile { get => resourceOnThisTile; /*set => resourcesOnThisTile = value; */ }
    public bool HasResources { get => hasResources; set =>  hasResources = value; }
    public bool IsStartingRiver { get => isStartingRiver; set => isStartingRiver = value; } 
    public LinkedList<Tile> RiverTiles { get => riverTiles; set => riverTiles = value; }
    public Tile RiverStart { get => riverStart; set => riverStart = value; }
    public bool HasRiver() { return hasRiverAdjecent; }
    public float Food { get => food; set => food = value; }
    public float Production { get => production; set => production = value;  }
    public float Gold { get => gold; set => gold = value; }
    public float Science { get => science; set => science = value; }
    public bool IsWorked { get => isWorked; set => isWorked = value; }
    public bool IsOccupied { get => isOccupied; set => isOccupied = value; }
    public District DistrictOnThisTile { get => districtOnThisTile; set => districtOnThisTile = value; }

    public Tile(Vector2 pos, TileBiome bio, Resource resource)
    {
        position = pos;
        biome = bio;
        switch (biome)
        {
            case TileBiome.Deep_Water:
                food = 0f;
                production = 0f;
                gold = 0f;
                break;
            case TileBiome.Water:
                food = 0f;
                production = 0f;
                gold = 0f;
                break;
            case TileBiome.Sand:
                food = 0f;
                production = 1f;
                gold = 1f;
                break;
            case TileBiome.Grass:
                food = 1.5f;
                production = 1.5f;
                gold = 0.5f;
                break;
            case TileBiome.Snow:
                food = 0.5f;
                production = 0.5f;
                gold = 1.5f;
                break;
            case TileBiome.River:
                food = 2f;
                production = 2f;
                gold = 0.5f;
                break;
        }
        RiverTiles = new LinkedList<Tile>();
        resourceOnThisTile = resource;
        if (resource != null)
        {
            hasResources = true;
            switch (resource.resourceType)
            {
                case Resource.ResourceType.Wheat:
                    food += 3f;
                    production += 1.5f;
                    gold += 1f;
                    break;
                case Resource.ResourceType.Fish:
                    food += 1.5f;
                    production += 0.5f;
                    gold += 1f;
                    break;
                case Resource.ResourceType.Cows:
                    food += 2f;
                    production += 1.5f;
                    gold += 1.5f;
                    break;
                case Resource.ResourceType.Horses:
                    food += 0f;
                    production += 2.5f;
                    gold += 2f;
                    break;
                case Resource.ResourceType.Stone:
                    food += 0f;
                    production += 3f;
                    gold += 1f;
                    break;
                case Resource.ResourceType.Iron:
                    food += 0f;
                    production += 4f;
                    gold += 2f;
                    break;
            }
        }

        switch (biome)
        {
            case TileBiome.Deep_Water:
                movementCost = 9999f; //change when player unlocks ocean travel
                break;
            case TileBiome.Water:
                movementCost = 9999f; //change when player unlocks water travel
                break;
            case TileBiome.Sand:
                movementCost = 1.25f;
                break;
            case TileBiome.River:
                movementCost = 2f;
                break;
            case TileBiome.Snow:
                movementCost = 1.5f;
                break;
            case TileBiome.Grass:
                movementCost = 1f;
                break;
        }
    }

    public void NotRiver()
    {
        bool isStartingRiver = false;
        bool hasRiverAdjecent = false;
        RiverTiles.Clear();
        riverStart = null;
    }
    
    public void AddResource(Resource resource)
    {
        resourceOnThisTile = resource;
        hasResources = true;
        ResourceIconManager.Instance.RefreshTileIcon(this);
    }

    public void RemoveResource()
    {
        resourceOnThisTile = null;
        hasResources = false;
        ResourceIconManager.Instance.RefreshTileIcon(this);
    }
    
    
    public void Build()
    {
        if (resourceOnThisTile == null) return; // Guard against null
        switch (resourceOnThisTile.resourceType)
        {
            case Resource.ResourceType.Wheat:
                food += 2f;
                break;
            case Resource.ResourceType.Fish:
                food += 2f;
                break;
            case Resource.ResourceType.Cows:
                food += 2f;
                break;
            case Resource.ResourceType.Horses:
                production += 2f;
                break;
            case Resource.ResourceType.Stone:
                production += 2f;
                break;
            case Resource.ResourceType.Iron:
                production += 2f;
                break;
        }
        
        RemoveResource();
        ResourceIconManager.Instance.RefreshTileIcon(this);
    }
}
