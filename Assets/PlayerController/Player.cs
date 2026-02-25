using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private List<City> cities;
    [SerializeField] private City capital;
    [SerializeField] private GameObject settlerPrefab;

    public List<City> Cities
    {
        get { return cities; }
    }
    
    public City Capital
    {
        get { return capital; }
    }

    void Start()
    {
        Instantiate(settlerPrefab, new Vector3(Random.Range(80, 460), 0.5f, Random.Range(80, 290)), Quaternion.identity);
    }
}
