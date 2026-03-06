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
    [SerializeField] private List<Resource> resourcesOnThisTile;
    [SerializeField] private float movementCost = 1f;
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
    
    public Tile(Vector2 pos, TileBiome bio, List<Resource> resources)
    {
        position = pos;
        biome = bio;
        RiverTiles = new LinkedList<Tile>();
        resourcesOnThisTile = resources;

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
        resourcesOnThisTile.Add(resource);
    }

    public void RemoveResource(Resource resource)
    {
        resourcesOnThisTile.Remove(resource);
    }

    public bool IsStartingRiver { get => isStartingRiver; set => isStartingRiver = value; } 
    public LinkedList<Tile> RiverTiles { get => riverTiles; set => riverTiles = value; }
    public Tile RiverStart { get => riverStart; set => riverStart = value; }
    public bool HasRiver() { return hasRiverAdjecent; }

}
