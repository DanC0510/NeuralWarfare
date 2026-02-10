using UnityEngine;

[System.Serializable]
public class Tile
{
    [SerializeField] private Vector2 position;
    [SerializeField] private TileBiome biome;
    public enum TileBiome
    {
        Grass = 0,
        Sand = 1,
        Snow = 2,
        Water = 3,
        Deep_Water = 4,
    };
    
    public Vector2 Position { get { return position; } }
    public TileBiome Biome { get { return biome; } }
    
    public Tile(Vector2 pos, TileBiome bio)
    {
        position = pos;
        biome = bio;
    }
}
