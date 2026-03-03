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
        set { capital = value; }
    }

    void Start()
    {
        GameObject go = Instantiate(settlerPrefab, Vector3.zero, Quaternion.identity);
        go.GetComponent<Settler>().CreateSettler(0, this);
    }
}
