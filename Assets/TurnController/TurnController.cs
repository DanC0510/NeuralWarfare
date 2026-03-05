using UnityEngine;

public class TurnController : MonoBehaviour
{
    private static TurnController _instance;
    public static TurnController Instance => _instance;

    public delegate void NextTurn();
    public event NextTurn OnNextTurn;
    
    [SerializeField] private bool nextTurn;
    private void Awake()
    {
        _instance = this;
    }
    
    void Update()
    {
        if (nextTurn)
        {
            nextTurn = false;
            OnNextTurn.Invoke();
        }
    }
    
}
