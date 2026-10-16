using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private List<City> cities;
    [SerializeField] private City capital;
    [SerializeField] private GameObject settlerPrefab;
    [SerializeField] private GameObject meleePrefab;
    [SerializeField] private GameObject archerPrefab;
    [SerializeField] private GameObject builderPrefab;
    [SerializeField] private float gold;
    [SerializeField] private float production;
    [SerializeField] private float science;
    [SerializeField] private float population;
    [SerializeField] private int playerIndex; //0 for player, 1 for AI
    [SerializeField] private Player opponent;
    [SerializeField] private bool isAI;
    public float Gold { get { return gold; } }
    public float Production { get { return production; } }
    public float Science { get { return science; } }
    public float Population { get { return population; } }
    public List<City> Cities { get { return cities; } } 
    public City Capital { get { return capital; } set { capital = value; } }
    public int PlayerIndex { get { return playerIndex; } }
    public Player Opponent { get { return opponent; } }
    public bool  IsAI { get { return isAI; } }

    void Start()
    {
        TurnController.Instance.RegisterPlayer(this);
        TurnController.Instance.OnNextTurn += NextTurn;
        
        GameObject startingSettler = Instantiate(settlerPrefab, Vector3.zero, Quaternion.identity);
        startingSettler.GetComponent<Settler>().CreateSettler(-1, this);
        
        //GameObject startingMelee = Instantiate(meleePrefab, Vector3.zero, Quaternion.identity);
        //startingMelee.GetComponent<Melee>().CreateMelee(startingSettler.GetComponent<Settler>().Position + new Vector2(1, 0), this);
    }

    void NextTurn()
    {
        float cumulativeGold = 0;
        float cumulativePopulation = 0;
        float cumulativeProduction = 0;
        float cumulativeScience = 0;
        foreach (City city in cities)
        {
            cumulativeGold += city.Gold;
            cumulativePopulation += city.Population;
            cumulativeProduction += city.Production;
            cumulativeScience += city.Science;
        }
    }

    void Die()
    {
        TurnController.Instance.UnregisterPlayer(this);
        SeriesController.Instance.RecordGameResult(playerIndex == 1);
    }
}
