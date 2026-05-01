using System.Collections.Generic;
using UnityEngine;

public class District : MonoBehaviour
{
    public enum DistrictType
    {
        Encampment,
        Campus,
    }

    public DistrictType district;
    //private List<Building> buildingsInDistrict = new List<Building>();
    public float ProductionCost;
    public Tile tile;
    private City owningCity;
    public void CreateDistrict(Vector2 startingPosition, DistrictType type, City city)
    {
        tile = Helpers.GetTileAtTilePos(startingPosition);
        tile.RemoveResource();
        
        district = type;
        owningCity = city;
        switch (type)
        {
            case DistrictType.Campus:
                tile.Science += 3f;
                break;
            case DistrictType.Encampment:
                tile.Production += 3f;
                tile.Food += 1f;
                tile.Gold += 1f;
                break;
        }
    }
}
