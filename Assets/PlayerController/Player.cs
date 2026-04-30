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
    public float Gold { get { return gold; } }
    public float Production { get { return production; } }
    public float Science { get { return science; } }
    public float Population { get { return population; } }
    
    public List<City> Cities { get { return cities; } set { cities = value; } } 
    public City Capital { get { return capital; } set { capital = value; } }

    void Start()
    {
        TurnController.Instance.RegisterPlayer(this);
        TurnController.Instance.OnNextTurn += NextTurn;
        
        GameObject startingSettler = Instantiate(settlerPrefab, Vector3.zero, Quaternion.identity);
        startingSettler.GetComponent<Settler>().CreateSettler(-1, this);
        
        GameObject startingMelee = Instantiate(meleePrefab, Vector3.zero, Quaternion.identity);
        startingMelee.GetComponent<Melee>().CreateMelee(startingSettler.GetComponent<Settler>().Position + new Vector2(1, 0), this);
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
    }
}
