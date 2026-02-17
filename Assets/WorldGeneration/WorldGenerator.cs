using System.Collections.Generic;
using System.IO;
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
    
    [Header("Tiles and Chunking")]
    private Tile[] tiles; //making this [SerializeField] slows down the inspector to a crawl.
    [SerializeField] private TerrainType[] regions;
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private float hexSize = 0.5f;
    
    [Header("Rivers")]
    [Range(0, 1000)] [SerializeField] private int riverFrequency;
    [Range(0, 25)] [SerializeField] private int turnFrequency;
    [SerializeField] private int maxRiverLength = 100;
    [SerializeField] private int minRiverLength = 3; 

    
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
        // Convert the noise map to a colour map + texture
        Texture2D colourTexture = new Texture2D(width, height);
        Color[] colourMap = new Color[width * height];
        // Initialise the tile array
        tiles = new Tile[width * height];
        List<Tile> riverStartTiles = new List<Tile>();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        int tileIndex = y * width + x;
                        colourMap[tileIndex] = regions[i].color;
                        
                        Tile currentTile = new Tile( new Vector2(x , y), regions[i].biome);
                        tiles[tileIndex] = currentTile;
                        if (Random.Range(0, riverFrequency) == 0)
                        {
                            if (currentTile.Biome != Tile.TileBiome.Deep_Water && currentTile.Biome != Tile.TileBiome.Water && currentTile.Biome != Tile.TileBiome.Sand)
                            {
                                FindRiverPath(currentTile, currentTile, 0, Random.Range(0, 6));
                                riverStartTiles.Add(currentTile);
                            }
                        }
                        break;
                    }
                }
            }
        }
        
        // After creating all tiles, validate and mark valid rivers
        foreach (Tile startTile in riverStartTiles)
        {
            if (startTile.RiverTiles.Count >= minRiverLength)
            {
                startTile.IsStartingRiver = true;
                // Set all tiles in valid rivers to River biome
                foreach (Tile riverTile in startTile.RiverTiles)
                {
                    if (riverTile != null)
                    {
                        riverTile.Biome = Tile.TileBiome.River;
                    }
                }
            }
            else
            {
                // Clear short rivers
                foreach (Tile riverTile in startTile.RiverTiles)
                {
                    if (riverTile != null)
                    {
                        riverTile.RiverStart = null;
                    }
                }
                startTile.RiverTiles.Clear();
            }
        }
        
        colourTexture.SetPixels(colourMap);
        colourTexture.Apply();
        
        SaveWorld();
        Graphics.Blit(noiseTexture, noiseRT);
        Graphics.Blit(colourTexture, colorRT);
    }
    
    void FindRiverPath(Tile startingTile, Tile current,  int depth, int lastTurnDirection = 0)
    {
        // Prevent infinite recursion and null reference exceptions
        if (current == null) { return; }
        if (depth >= maxRiverLength) { return; }
        if (current.Biome == Tile.TileBiome.Deep_Water || current.Biome == Tile.TileBiome.Water) { return; }
        
        if (startingTile.RiverTiles.Contains(current) && depth > 0) { return; }
        

        if (depth > 0 && current.RiverStart != null && current.RiverStart != startingTile)
        {
            return;
        }
        
        startingTile.RiverTiles.AddLast(current);
        current.RiverStart = startingTile; // Set reference to start tile
        
        int currentDirection = lastTurnDirection;
        bool doTurn = Random.Range(0, Mathf.Max(1, turnFrequency)) == 0;

        if (doTurn)
        {
            int turn = Random.Range(0, 100) < 80
                ? (Random.value < 0.5f ? -1 : 1)
                : (Random.value < 0.5f ? -2 : 2);

            currentDirection = Helpers.mod(currentDirection + turn, 6);
        }
        else
        {
            // Tiny jitter to avoid perfect straight lines
            if (Random.Range(0, 10) == 0)
            {
                currentDirection = Helpers.mod(currentDirection + (Random.value < 0.5f ? -1 : 1), 6);
            }
        }
        
        int back = Helpers.mod(lastTurnDirection + 3, 6);
        if (currentDirection == back)
        {
            currentDirection = Helpers.mod(currentDirection + (Random.value < 0.5f ? 1 : -1), 6);
        }
        
        Tile next = GetNeighbour(current, currentDirection);
        if (next != null)
        {
            FindRiverPath(startingTile, next, depth + 1, currentDirection);
        }
    }
    


    Tile GetNeighbour(Tile currentTile, int direction)
    {
        Vector2 neighbourPos = Vector2.zero;
        int col = (int)currentTile.Position.x;
        int row = (int)currentTile.Position.y;
        
        bool isOddRow = row % 2 == 1;
        
        if (direction == 0) // NE
        {
            neighbourPos = new Vector2(col + (isOddRow ? 1 : 0), row + 1);
        }
        else if (direction == 1) // E
        {
            neighbourPos = new Vector2(col + 1, row);
        }
        else if (direction == 2) // SE
        {
            neighbourPos = new Vector2(col + (isOddRow ? 1 : 0), row - 1);
        }
        else if (direction == 3) // SW
        {
            neighbourPos = new Vector2(col + (isOddRow ? 0 : -1), row - 1);
        }
        else if (direction == 4) // W
        {
            neighbourPos = new Vector2(col - 1, row);
        }
        else if (direction == 5) // NW
        {
            neighbourPos = new Vector2(col + (isOddRow ? 0 : -1), row + 1);
        }
        
        if (neighbourPos.x < 0 || neighbourPos.x >= actualWidth || neighbourPos.y < 0 || neighbourPos.y >= actualHeight)
        {
            return null;
        }
        
        int index = (int)neighbourPos.y * actualWidth + (int)neighbourPos.x;
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
