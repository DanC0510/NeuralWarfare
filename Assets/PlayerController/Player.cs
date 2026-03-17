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
    public List<City> Cities { get { return cities; } set { cities = value; } } 
    public City Capital { get { return capital; } set { capital = value; } }

    void Start()
    {
        GameObject startingSettler = Instantiate(settlerPrefab, Vector3.zero, Quaternion.identity);
        startingSettler.GetComponent<Settler>().CreateSettler(0, this);
        
        GameObject startingMelee = Instantiate(meleePrefab, Vector3.zero, Quaternion.identity);
        startingMelee.GetComponent<Melee>().CreateMelee(startingSettler.GetComponent<Settler>().Position + new Vector2(1, 0), this);

        /*GameObject testingArcher = Instantiate(archerPrefab, Vector3.zero, Quaternion.identity);
        testingArcher.GetComponent<Archer>().CreateArcher(startingSettler.GetComponent<Settler>().Position + new Vector2(-1, 0), this);
        
        GameObject testingBuilder = Instantiate(builderPrefab, Vector3.zero, Quaternion.identity);
        testingBuilder.GetComponent<Builder>().CreateBuilder(startingSettler.GetComponent<Settler>().Position + new Vector2(0, 1), this);
        */
    }
}
