using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnController : MonoBehaviour
{
    private static TurnController _instance;
    public static TurnController Instance => _instance;

    [SerializeField] private Button nextTurnButton;
    public delegate void NextTurn();
    public event NextTurn OnNextTurn;

    public List<Player> allPlayers;
    public List<Unit> allUnits;
    public List<City> allCities;
    public List<object> takingTurn; //allows me to put any object i want into this list so i dont need individual lists for cities and units
    private void Awake()
    {
        _instance = this;
        takingTurn = new List<object>();
    }

    public void RegisterPlayer(Player player)
    {
        if (!allPlayers.Contains(player)) { allPlayers.Add(player); }
    }

    public void UnregisterPlayer(Player player)
    {
        if (allPlayers.Contains(player)) { allPlayers.Remove(player); }
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

    public void FinishTurnEvent<T>(T finishEvent)
    {
        if (takingTurn.Contains(finishEvent))
        {
            takingTurn.Remove(finishEvent);
        }
    }
    
    public void InvokeNextTurn()
    {
        takingTurn.Clear();
        foreach(Unit unit in allUnits)
        {
            if (unit.Action == Unit.TurnAction.Move)
            {
                if (unit.MoveQueue.Count == 0)
                {
                    unit.OwningPlayer.gameObject.GetComponent<PlayerController>().MoveTo = new Vector3(Helpers.TileToWorldPos(unit.Position).x, 0, Helpers.TileToWorldPos(unit.Position).z - 10);
                    StartCoroutine(unit.OwningPlayer.gameObject.GetComponent<PlayerController>().MoveCam(Helpers.TileToWorldPos(unit.Position)));
                    return;
                }
            }
            if (unit.TurnActions.Count > 0)
            {
                takingTurn.Add(unit);
            }
        }

        foreach (City city in allCities)
        {
            if (city.ActionQueue.Count == 0)
            {
                city.OwningPlayer.gameObject.GetComponent<PlayerController>().MoveTo = new Vector3(Helpers.TileToWorldPos(city.Position).x, 0, Helpers.TileToWorldPos(city.Position).z - 10);
                StartCoroutine(city.OwningPlayer.gameObject.GetComponent<PlayerController>().MoveCam(Helpers.TileToWorldPos(city.Position)));
                return;
            }
            if(city.ActionQueue.Count > 0)
            {
                takingTurn.Add(city);
            }
        }

        foreach (Player player in allPlayers)
        {
            player.GetComponent<PlayerController>().MoveTo = new Vector3(Helpers.TileToWorldPos(player.Capital.Position).x, 0, Helpers.TileToWorldPos(player.Capital.Position).z - 10);
            StartCoroutine(player.GetComponent<PlayerController>().MoveCam(Helpers.TileToWorldPos(player.Capital.Position)));
        }
        OnNextTurn.Invoke();
        StartCoroutine(WaitForTurnFinish());
    }

    IEnumerator WaitForTurnFinish()
    {
        nextTurnButton.interactable = false;
        yield return new WaitUntil(() => takingTurn.Count == 0);
        nextTurnButton.interactable = true;
        
    }
    
}
