using UnityEngine;
using System.Collections.Generic;

public class ResourceIconManager : MonoBehaviour
{
    private static ResourceIconManager _instance;
    public static ResourceIconManager Instance => _instance;

    [Header("Icon Settings")]
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private Sprite[] resourceSprites = new Sprite[6]; // Wheat, Fish, Cows, Horses, Stone, Iron
    [SerializeField] private float iconYOffset = 0.5f;
    [SerializeField] private float iconScale = 0.3f;
    [SerializeField] private bool billboardIcons = true;

    private Camera mainCamera;
    private Dictionary<Tile, GameObject> activeIcons = new Dictionary<Tile, GameObject>();

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (billboardIcons)
        {
            BillboardAllIcons();
        }
    }
    
    public void InitializeResourceIcons()
    {
        Tile[] tiles = WorldGenerator.Instance.Tiles;
        
        foreach (Tile tile in tiles)
        {
            if (tile != null && tile.HasResources && tile.ResourceOnThisTile != null)
            {
                SpawnIconForTile(tile);
            }
        }
    }

    public void SpawnIconForTile(Tile tile)
    {
        if (tile == null || !tile.HasResources || tile.ResourceOnThisTile == null)
            return;
        
        if (activeIcons.ContainsKey(tile))
            return;

        Vector3 worldPos = Helpers.TileToWorldPos(tile.Position);
        worldPos.y += iconYOffset;

        GameObject iconGO = Instantiate(iconPrefab, worldPos, Quaternion.Euler(90, 0, 0), transform);
        iconGO.name = "ResourceIcon_" + tile.Position.x + "_" + tile.Position.y;
        
        SpriteRenderer spriteRenderer = iconGO.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            int spriteIndex = (int)tile.ResourceOnThisTile.resourceType;
            if (spriteIndex >= 0 && spriteIndex < resourceSprites.Length)
            {
                spriteRenderer.sprite = resourceSprites[spriteIndex];
            }
        }

        iconGO.transform.localScale = Vector3.one * iconScale;
        activeIcons[tile] = iconGO;
    }
    
    public void RemoveIconForTile(Tile tile)
    {
        if (activeIcons.ContainsKey(tile))
        {
            GameObject iconGO = activeIcons[tile];
            activeIcons.Remove(tile);
            Destroy(iconGO);
        }
    }
    
    public void RefreshTileIcon(Tile tile)
    {
        if (tile == null)
            return;

        bool hasIcon = activeIcons.ContainsKey(tile);
        bool shouldHaveIcon = tile.HasResources && tile.ResourceOnThisTile != null;

        if (shouldHaveIcon && !hasIcon)
        {
            SpawnIconForTile(tile);
        }
        else if (!shouldHaveIcon && hasIcon)
        {
            RemoveIconForTile(tile);
        }
    }
    
    private void BillboardAllIcons()
    {
        if (mainCamera == null)
            return;

        Vector3 cameraPos = mainCamera.transform.position;

        foreach (var icon in activeIcons.Values)
        {
            if (icon != null)
            {
                Vector3 dirToCamera = (cameraPos - icon.transform.position).normalized;
                icon.transform.rotation = Quaternion.LookRotation(dirToCamera);
            }
        }
    }
}
