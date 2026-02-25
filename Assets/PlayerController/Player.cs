using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private List<City> cities;
    [SerializeField] private City capital;

    public List<City> Cities
    {
        get { return cities; }
    }
    
    public City Capital
    {
        get { return capital; }
    }
}
