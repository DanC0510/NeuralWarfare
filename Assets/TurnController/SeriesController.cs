using UnityEngine;

public class SeriesController : MonoBehaviour
{
    private static SeriesController _instance;
    public static SeriesController Instance => _instance;

    [SerializeField] private AIController aiController;
    private int currentGameNumber = 1;
    public int CurrentGameNumber => currentGameNumber;
    private int aiWins = 0;
    
    public void StartNewGame()
    {
        currentGameNumber++;
        if (currentGameNumber > 7)
        {
            DetermineSeries();
            return;
        }
        // Update AI difficulty
        aiController.SetGameNumber(currentGameNumber);
        
        Debug.Log($"Starting Game {currentGameNumber} of 7. AI Wins: {aiWins}");
    }
    
    public void RecordGameResult(bool aiWon)
    {
        if (aiWon)
        {
            aiWins++;
            aiController.OnWinGame();
        }
        else
        {
            aiController.OnLoseGame();
        }
        
        StartNewGame();
    }
    
    private void DetermineSeries()
    {
        bool playerWins = 4 - aiWins > aiWins;
        Debug.Log(playerWins ? "Player wins the series!" : "AI wins the series!");
    }
}