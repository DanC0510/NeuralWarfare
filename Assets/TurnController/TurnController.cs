using System.Collections.Generic;
using UnityEngine;

public class TurnController : MonoBehaviour
{
    private static TurnController _instance;
    public static TurnController Instance => _instance;

    public delegate void NextTurn();
    public event NextTurn OnNextTurn;

    public List<Unit> allUnits;
    public List<City> allCities;
    private void Awake()
    {
        _instance = this;
    }

    public void RegisterCity(City city)
    {
        if (!allCities.Contains(city)) { allCities.Add(city); }
    }
    
    public void UnregisterCity(City city)
    {
        if (allCities.Contains(city)) { allCities.Remove(city); }
    }

    public void RegisterUnit(Unit unit)
    {
        if(!allUnits.Contains(unit)) { allUnits.Add(unit);}
    }
    public void UnregisterUnit(Unit unit)
    {
        if(allUnits.Contains(unit)) { allUnits.Remove(unit);}
    }
    
    public void InvokeNextTurn()
    {
        foreach(Unit unit in allUnits)
        {
            if (unit.TurnActions.Count == 0)
            {
                StartCoroutine(unit.OwningPlayer.gameObject.GetComponent<PlayerController>().MoveCam(Helpers.TileToWorldPos(unit.Position)));
                return; 
            }
        }

        foreach (City city in allCities)
        {
            if(city.ActionQueue.Count == 0)
            {
                StartCoroutine(city.OwningPlayer.gameObject.GetComponent<PlayerController>().MoveCam(Helpers.TileToWorldPos(city.Position)));
                return;
            }
        }
        OnNextTurn.Invoke();
    }
    
}
