using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class TurnController : MonoBehaviour
{
    private static TurnController _instance;
    public static TurnController Instance => _instance;

    [SerializeField] private Button nextTurnButton;
    public delegate void NextTurn();
    public event NextTurn OnNextTurn;
    public int turnNumber;
    public delegate void AIInferenceControl();
    public event AIInferenceControl OnAIInferenceControl;

    public List<bool> playersReady;
    public List<Player> allPlayers;
    public List<object> unitsAndCities;
    public List<object> takingTurn; //allows me to put any object i want into this list so i dont need individual lists for cities and units
    private void Awake()
    {
        _instance = this;
        playersReady = new List<bool>();
        allPlayers = new List<Player>();
        takingTurn = new List<object>();
        unitsAndCities = new List<object>();
    }

    private void Start()
    {
        StartCoroutine(CallNextTurn());
    }

    public void RegisterPlayer(Player player)
    {
        if (!allPlayers.Contains(player)) { allPlayers.Add(player); playersReady.Add(false); }
    }

    public void UnregisterPlayer(Player player)
    {
        if (allPlayers.Contains(player)) { allPlayers.Remove(player); playersReady.RemoveAt(player.PlayerIndex); }
    }

    public void Register<T>(T obj)
    {
        if (!unitsAndCities.Contains(obj)) { unitsAndCities.Add(obj); }
    }
    
    public void Unregister<T>(T obj)
    {
        if (unitsAndCities.Contains(obj)) { unitsAndCities.Remove(obj); }
    }

    public void FinishTurnEvent<T>(T finishEvent)
    {
        if (takingTurn.Contains(finishEvent))
        {
            takingTurn.Remove(finishEvent);
        }
    }

    public void SetPlayerReady(int playerIndex, bool isReady)
    {
        playersReady[playerIndex] = isReady;
    }

    public IEnumerator CallNextTurn()
    {
        yield return new WaitUntil(() => playersReady.Count >= 2);
        OnAIInferenceControl.Invoke();
        yield return new WaitUntil(() => playersReady.TrueForAll(x => x));
        InvokeNextTurn();
    }
    
    void InvokeNextTurn()
    {
        takingTurn.Clear();
        foreach (object obj in unitsAndCities)
        {
            if (obj.GetType().BaseType == typeof(Unit)) //Weirdly .BaseType just goes 1 layer lower, not to the actual base type which in this case would be MonoBehaviour
            {
                Unit unit = (Unit)obj;
                if (unit.TurnActions.Count == 0)
                {
                    if (!unit.OwningPlayer.IsAI)
                    {
                        unit.OwningPlayer.gameObject.GetComponent<PlayerController>().MoveTo = new Vector3(Helpers.TileToWorldPos(unit.Position).x, 0, Helpers.TileToWorldPos(unit.Position).z - 10);
                        StartCoroutine(unit.OwningPlayer.gameObject.GetComponent<PlayerController>().MoveCam(Helpers.TileToWorldPos(unit.Position)));
                        unit.OwningPlayer.GetComponent<PlayerController>().BringUpUI(obj);
                    }
                    ResetTurn();
                    return;
                }
                takingTurn.Add(unit);
            }
            else if (obj.GetType() == typeof(City)) //doesn't need .BaseType as they are put in as just a city
            {
                City city = (City)obj;
                if (city.ActionQueue.Count == 0)
                {
                    if (!city.OwningPlayer.IsAI)
                    {
                        city.OwningPlayer.gameObject.GetComponent<PlayerController>().MoveTo = new Vector3(Helpers.TileToWorldPos(city.Position).x, 0, Helpers.TileToWorldPos(city.Position).z - 10);
                        StartCoroutine(city.OwningPlayer.gameObject.GetComponent<PlayerController>().MoveCam(Helpers.TileToWorldPos(city.Position)));
                        city.OwningPlayer.GetComponent<PlayerController>().BringUpUI(obj);
                    }
                    ResetTurn();
                    return;
                }
                takingTurn.Add(city);
            }
        }

        foreach (Player player in allPlayers)
        {
            if(player.Capital == null || player.IsAI) { continue; }
            player.GetComponent<PlayerController>().MoveTo = new Vector3(Helpers.TileToWorldPos(player.Capital.Position).x, 0, Helpers.TileToWorldPos(player.Capital.Position).z - 10);
            StartCoroutine(player.GetComponent<PlayerController>().MoveCam(Helpers.TileToWorldPos(player.Capital.Position)));
        }
        OnNextTurn.Invoke();
        StartCoroutine(WaitForTurnFinish());
    }

    IEnumerator WaitForTurnFinish()
    {
        yield return new WaitUntil(() => takingTurn.Count == 0);
        nextTurnButton.interactable = true;
        for (int i = 0; i < playersReady.Count; i++)
        {
            SetPlayerReady(i, false);
        }
        turnNumber++;
        StartCoroutine(CallNextTurn());
    }

    void ResetTurn()
    {        
        nextTurnButton.interactable = true;
        for (int i = 0; i < playersReady.Count; i++)
        {
            SetPlayerReady(i, false);
        }
        StartCoroutine(CallNextTurn());
    }
    
}
