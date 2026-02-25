using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private List<City> cities;
    private City capital;

    public List<City> Cities
    {
        get { return cities; }
    }
    
    public City Capital
    {
        get { return capital; }
    }
}
