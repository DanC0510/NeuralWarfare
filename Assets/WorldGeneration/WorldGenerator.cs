using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("Texture Details")]
    [SerializeField] private float scaleFactor;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float scale;
    [SerializeField] private int octaves;
    [SerializeField] private int lacunarity;
    [SerializeField] private float persistence;
    private int actualWidth;
    private int actualHeight;
    
    [Header("Textures")]
    [SerializeField] private RenderTexture noiseRT;
    [SerializeField] private RenderTexture colorRT;

    [Header("Colour Deatils")]
    [SerializeField] private TerrainType[] regions;

    [Header("Tile Details")] 
    [SerializeField] private Tile[] tiles;
    [SerializeField] private GameObject[] tilePrefabs;
    
    [Header("Chunking")]
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private float hexSize = 0.5f;
    
    [Header("Rivers")]
    [Range(0, 1000)] [SerializeField] private int riverFrequency;
    [Range(0, 25)] [SerializeField] private int turnFrequency;
    [SerializeField] private int maxRiverLength = 100;
    [SerializeField] private Dictionary<int, HashSet<Tile>> edgeToTileMap = new Dictionary<int, HashSet<Tile>>(); 

    
    void Start()
    {
        GenerateTexture(Mathf.FloorToInt(width * scaleFactor), Mathf.FloorToInt(height * scaleFactor));
        CreateChunks();
    }

    
    private void GenerateTexture(int width, int height)
    {
        // Store the actual generated dimensions
        actualWidth = width;
        actualHeight = height;
        
        //Generate the noise map
        Color[] pix = new Color[width * height];
        Texture2D noiseTexture = new Texture2D(width, height);
        if(seed == 0){ seed = Random.Range(int.MinValue, int.MaxValue); }
        if(offset == Vector2.zero) { offset = new Vector2(Random.Range(-100000.0f, 100000.0f), Random.Range(-100000.0f, 100000.0f)); }
        float[,] noiseMap = Noise.CalculateNoiseMap(width, height, seed, scale, octaves, persistence, lacunarity, offset);
        
        // Convert the noise map to a texture
        for (int y = 0; y < noiseTexture.height; y++)
        {
            for (int x = 0; x < noiseTexture.width; x++)
            {
                float currentHeight = noiseMap[x, y];
                pix[y * noiseTexture.width + x] = Color.Lerp(Color.black, Color.white, currentHeight);
            }
        }
        noiseTexture.SetPixels(pix);
        noiseTexture.Apply();
        //Convert the noise map to a colour map + texture
        Texture2D colourTexture = new Texture2D(width, height);
        Color[] colourMap = new Color[width * height];
        tiles = new Tile[width * height];
        int tileIndex = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[y * width + x] = regions[i].color;
                        Tile currentTile = new Tile( new Vector2(x , y), regions[i].biome);
                        tiles[tileIndex] = currentTile;
                        if (Random.Range(0, riverFrequency) == 0)
                        {
                            if (currentTile.Biome != Tile.TileBiome.Deep_Water && currentTile.Biome != Tile.TileBiome.Water && currentTile.Biome != Tile.TileBiome.Sand)
                            {
                                int riverEdge = Random.Range(0, 6);
                                tiles[tileIndex].SetRiverOnEdge(riverEdge, true);
                            }
                        }
                        tileIndex++;
                        break;
                    }
                }
            }
        }

        CreateRivers();
        
        colourTexture.SetPixels(colourMap);
        colourTexture.Apply();
        
        SaveWorld();
        Graphics.Blit(noiseTexture, noiseRT);
        Graphics.Blit(colourTexture, colorRT);
    }

    void CreateRivers()
    {
        HashSet<Tile> tilesToStartFrom = new HashSet<Tile>();
        foreach (Tile currentTile in tiles)
        {
            if (currentTile.HasRiver())
            {
                tilesToStartFrom.Add(currentTile);
            }
        }

        foreach (Tile currentTile in tilesToStartFrom)
        {
            for (int i = 0; i < 6; i++)
            {
                if (currentTile.HasRiverOnEdge(i))
                {
                    FindRiverPath(currentTile, i, 0);
                }
            }
        }
        
        foreach (KeyValuePair<int, HashSet<Tile>> entry in edgeToTileMap)
        {
            int edge = entry.Key;
            HashSet<Tile> tilesWithRiver = entry.Value;
            foreach (Tile tile in tilesWithRiver)
            {
                if(tile == null) { continue; }

                if (edge == -1) edge = 0;
                tile.SetRiverOnEdge(edge, true);
            }
        }
    }

    void AddToEdgeMap(Tile tile, int edge)
    {
        if (!edgeToTileMap.ContainsKey(edge))
        {
            edgeToTileMap[edge] = new HashSet<Tile>();
        }
        edgeToTileMap[edge].Add(tile);
        tile.SetRiverBeenVisited(edge, true);
    }

    //The Idea for this function is to "rotate" the river to be flat when viewed, then go left or right, incorporating turns occasionally until we get to the ocean or max river length.
    void FindRiverPath(Tile current, int riverEdge, int depth)
    {
        // Prevent infinite recursion and null reference exceptions
        if (current == null) { return; }
        if (depth >= maxRiverLength) { return; }
        
        
        int edgeToGetTo = mod(Random.Range(0, 2) + riverEdge, 6);
        int turnDirection = Random.Range(0, 2) == 0 ? -1 : 1; // -1 for left, 1 for right
        
        while(riverEdge != edgeToGetTo)
        {
            AddToEdgeMap(current, riverEdge);
            riverEdge = mod(riverEdge + turnDirection, 6);
            depth++;
        }
        
        Tile next = GetNeighbour(current, riverEdge);
        FindRiverPath(next, riverEdge, depth + 1);
    }
    

    int mod(int x, int m) {
        return (x%m + m)%m;
    }


    Tile GetNeighbour(Tile currentTile, int riverEdge)
    {
        Vector2 neighbourPos = Vector2.zero;
        int col = (int)currentTile.Position.x;
        int row = (int)currentTile.Position.y;
        
        bool isOddRow = row % 2 == 1;
        
        if (riverEdge == 0) // NE
        {
            neighbourPos = new Vector2(row + 1, col + (isOddRow ? 1 : 0));
        }
        else if (riverEdge == 1) // E
        {
            neighbourPos = new Vector2(row, col + 1);
        }
        else if (riverEdge == 2) // SE
        {
            neighbourPos = new Vector2(row - 1, col + (isOddRow ? 1 : 0));
        }
        else if (riverEdge == 3) // SW
        {
            neighbourPos = new Vector2(row - 1, col + (isOddRow ? 0 : -1));
        }
        else if (riverEdge == 4) // W
        {
            neighbourPos = new Vector2(row, col - 1);
        }
        else if (riverEdge == 5) // NW
        {
            neighbourPos = new Vector2(row + 1, col + (isOddRow ? 0 : -1));
        }

        // Check bounds
        if (neighbourPos.x < 0 || neighbourPos.x >= actualWidth || neighbourPos.y < 0 || neighbourPos.y >= actualHeight)
        {
            return null;
        }

        // Use direct array indexing instead of iteration for performance
        int index = (int)neighbourPos.x * actualHeight + (int)neighbourPos.y;
        if (index >= 0 && index < tiles.Length)
        {
            return tiles[index];
        }

        return null;
    }
    
    void CreateChunks()
    {
        int chunksQ = Mathf.CeilToInt(actualWidth / (float)HexChunkRenderer.chunkSize);
        int chunksR = Mathf.CeilToInt(actualHeight / (float)HexChunkRenderer.chunkSize);

        for (int q = 0; q < chunksQ; q++)
        {
            for (int r = 0; r < chunksR; r++)
            {
                GameObject chunkGO = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform);
                HexChunkRenderer chunk = chunkGO.GetComponent<HexChunkRenderer>();

                chunk.hexSize = hexSize;
                chunk.Init(tiles, actualWidth, actualHeight, q, r);
            }
        }
    }
    
    public void SaveWorld()
    {
        string path = Application.persistentDataPath + "/world.dat";
        if (Application.isEditor)
        {
            path = "Assets/WorldGeneration/world.dat";
        }

        if (!File.Exists(path))
        {
            File.Create(path);
        }

        TileArrayWrapper tileArray = new TileArrayWrapper
        {
            tileArr = tiles
        };
        string json = JsonUtility.ToJson(tileArray, true); 
        File.WriteAllText(path, json);
    }

    public void LoadWorld()
    {
        string path = Application.persistentDataPath + "/world.dat";
        if (Application.isEditor)
        {
            path = "Assets/WorldGeneration/world.dat";
        }
        if(!File.Exists(path)) { Debug.LogError("No world data found at " + path); return; }
        string json = File.ReadAllText(path);
        TileArrayWrapper loaded = JsonUtility.FromJson<TileArrayWrapper>(json);
        tiles = loaded.tileArr;
    }
}

[System.Serializable]
public class TileArrayWrapper
{
    public Tile[] tileArr;
}


[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
    public Tile.TileBiome biome;
}
