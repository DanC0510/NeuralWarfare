using System.Collections.Generic;
using UnityEngine;

public class TurnController : MonoBehaviour
{
    private static TurnController _instance;
    public static TurnController Instance => _instance;

    public delegate void NextTurn();
    public event NextTurn OnNextTurn;

    public List<Unit> allUnits;
    private void Awake()
    {
        _instance = this;
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
                //move cam to unit without turn action
                return; 
            }
        }
        OnNextTurn.Invoke();
    }
    
}
