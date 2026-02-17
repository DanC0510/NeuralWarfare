using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    [SerializeField] private bool hasRiverAdjecent;
    [SerializeField] private Vector2 position;
    [SerializeField] private TileBiome biome;
    public LinkedList<Tile> riverTiles;
    [SerializeField] private bool isStartingRiver;
    [SerializeField] private Tile riverStart; // Reference to the starting tile of this river
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
    
    public Tile(Vector2 pos, TileBiome bio)
    {
        position = pos;
        biome = bio;
        RiverTiles = new LinkedList<Tile>();
    }

    public bool IsStartingRiver { get => isStartingRiver; set => isStartingRiver = value; } 
    public LinkedList<Tile> RiverTiles { get => riverTiles; set => riverTiles = value; }
    public Tile RiverStart { get => riverStart; set => riverStart = value; }
    public bool HasRiver() { return hasRiverAdjecent; }

}
