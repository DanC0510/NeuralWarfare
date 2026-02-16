using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    [SerializeField] private bool hasRiverAdjecent;
    [SerializeField] private Vector2 position;
    [SerializeField] private TileBiome biome;
    [SerializeField] private List<Tile> riverTiles;
    [SerializeField] private bool isStartingRiver;
    public enum TileBiome
    {
        Water = 0,
        Deep_Water = 1,
        Sand = 2,
        Grass = 3,
        Snow = 4,
        River = 5
    };
    
    public Vector2 Position { get { return position; } }
    public TileBiome Biome { get { return biome; } set { biome = value; } }
    
    public Tile(Vector2 pos, TileBiome bio)
    {
        position = pos;
        biome = bio;
        RiverTiles = new List<Tile>();
    }

    public bool IsStartingRiver { get => isStartingRiver; set => isStartingRiver = value; } 
    public List<Tile> RiverTiles { get => riverTiles; set => riverTiles = value; }
    public bool HasRiver() { return hasRiverAdjecent; }
    
}
