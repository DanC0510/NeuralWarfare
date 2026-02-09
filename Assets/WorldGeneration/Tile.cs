using UnityEngine;

[System.Serializable]
public class Tile
{
    [SerializeField] private Vector2 position;
    [SerializeField] private TileBiome biome;
    public enum TileBiome
    {
        Grass,
        Sand,
        Snow,
        Water,
        Deep_Water,
    };
    
    public Vector2 Position { get { return position; } }
    public TileBiome Biome { get { return biome; } }
    
    public Tile(Vector2 pos, TileBiome bio)
    {
        position = pos;
        biome = bio;
    }
}
