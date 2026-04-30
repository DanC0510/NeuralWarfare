using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class AIController : Agent
{
    [SerializeField] private Player aiPlayer;
    [SerializeField] private int seriesGameNumber = 1; // Track which game in the series (1-7)
    
    private int episodeRewards = 0;
    private float explorationRate = 1f; // Start at 100% exploration
    private bool shouldDecide;
    
    public override void Initialize()
    {
        // Initialize AI player reference
        if (aiPlayer == null)
            aiPlayer = GetComponent<Player>();
    }

    void Start()
    {
        TurnController.Instance.OnAIInferenceControl += OnAITurn;
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid errors
        if (TurnController.Instance != null)
            TurnController.Instance.OnAIInferenceControl -= OnAITurn;
    }
    
    private void OnAITurn()
    {
        // Only request decision when it's this AI's turn
        shouldDecide = true;
    }
    
    public override void OnEpisodeBegin()
    {
        episodeRewards = 0;
        // Adjust exploration rate based on series progress
        explorationRate = Mathf.Lerp(1f, 0f, (seriesGameNumber - 1) / 6f); // 1 => 0 across 7 games
        shouldDecide = false;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        if (!shouldDecide)
        {
            for (int i = 0; i < 125; i++)
                sensor.AddObservation(0f);
            return;
        }
        
        // Base observations
        sensor.AddObservation(aiPlayer.Gold / 1000f);
        sensor.AddObservation(aiPlayer.Science / 1000f);
        sensor.AddObservation(aiPlayer.Population / 100f);
    
        var ownedUnits = GetOwnedUnits();
        var enemies = GetEnemyUnits();
    
        // Observe counts
        sensor.AddObservation(ownedUnits.Count / 10f); // Normalize to max 10
        sensor.AddObservation(enemies.Count / 10f);
    
        // Pad units to max 20
        for (int i = 0; i < 20; i++)
        {
            if (i < ownedUnits.Count)
            {
                var unit = ownedUnits[i];
                var capital = aiPlayer.Capital;
                if (capital != null)
                {
                    Vector2 relativePos = unit.Position - capital.Position;
                    sensor.AddObservation(relativePos.x / 100f);
                    sensor.AddObservation(relativePos.y / 100f);
                    sensor.AddObservation(unit.Health / unit.MaxHealth);
                }
            }
            else
            {
                sensor.AddObservation(0f); // Padding
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }
    
        // Pad enemies to max 20
        for (int i = 0; i < 20; i++)
        {
            if (i < enemies.Count)
            {
                var enemy = enemies[i];
                if (aiPlayer.Capital != null)
                {
                    Vector2 relativePos = enemy.Position - aiPlayer.Capital.Position;
                    sensor.AddObservation(relativePos.x / 100f);
                    sensor.AddObservation(relativePos.y / 100f);
                }
            }
            else
            {
                sensor.AddObservation(0f); // Padding
                sensor.AddObservation(0f);
            }
        }
    }
    
    private float lastInferenceTime = 0f;
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!shouldDecide)
            return;
        
        float startTime = Time.realtimeSinceStartup;
        ActionSegment<int> discreteActions = actions.DiscreteActions;
        
        // Action space:
        // 0: Move unit (with sub-actions for direction)
        // 1: Attack
        // 2: Build/Settle (if applicable)
        // 3: Skip turn
        // 4: Sleep
        

        ExecuteAction(discreteActions);
        shouldDecide = false;
        
        lastInferenceTime = (Time.realtimeSinceStartup - startTime) * 1000f;
        Debug.Log($"Inference time: {lastInferenceTime:F2}ms");
    }
    
    private void ExecuteAction(ActionSegment<int> discreteActions)
    {
        List<Unit> ownedUnits = GetOwnedUnits();
        if (ownedUnits.Count == 0)
        {
            AddReward(-0.01f); // Small penalty for having no units
            return;
        }

        int action = discreteActions[0];
        foreach (Unit selectedUnit in ownedUnits)
        {
            if (Random.value < explorationRate)
            {
                // Random action (early games seem unpredictable)
                action = Random.Range(0, 5);
            }

            switch (action)
            {
                case 0: // Move
                    MoveUnitTowardEnemy(selectedUnit);
                    break;
                case 1: // Attack
                    AttackNearbyEnemy(selectedUnit);
                    break;
                case 2: // Skip
                    selectedUnit.Skip();
                    break;
                case 3: // Sleep
                    selectedUnit.Sleep();
                    break;
                case 4: // Build (if applicable for settler/builder)
                    if (selectedUnit.GetUnitType == Unit.UnitType.Settler)
                    {
                        Settler settler = (Settler)selectedUnit;
                        settler.PlaceCity();
                    }
                    else if (selectedUnit.GetUnitType == Unit.UnitType.Builder)
                    {
                        Builder builder = (Builder)selectedUnit;
                        builder.Build();
                    }
                    else
                    {
                        action = Random.Range(0, 4); // If not a builder or settler, choose a different action
                    }
                    break;
            }
        }
    }
    
    private void MoveUnitTowardEnemy(Unit unit)
    {
        List<Unit> enemies = GetEnemyUnits();
        if (enemies.Count == 0) return;
        
        Unit closestEnemy = enemies[0];
        float minDistance = float.MaxValue;
        
        foreach (Unit enemy in enemies)
        {
            float dist = unit.DistanceTo(unit, enemy);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestEnemy = enemy;
            }
        }
        
        unit.GetPath(closestEnemy.Position);
        AddReward(0.01f); // Small reward for moving toward enemy
    }
    
    private void AttackNearbyEnemy(Unit unit)
    {
        List<Unit> enemies = GetEnemyUnits();
        foreach (Unit enemy in enemies)
        {
            if (unit.DistanceTo(unit, enemy) <= unit.AttackRange)
            {
                unit.Attack(enemy);
                AddReward(0.5f); // Significant reward for attacking
                return;
            }
        }
        AddReward(-0.1f); // Penalty for attacking when no enemy in range
    }
    
    // Reward functions
    public void OnUnitDestroyed()
    {
        AddReward(1f); // Big reward for destroying enemy unit
        episodeRewards += 1;
    }
    
    public void OnUnitLost()
    {
        AddReward(-0.5f); // Penalty for losing own unit
        episodeRewards -= 1;
    }
    
    public void OnWinGame()
    {
        AddReward(10f);
        episodeRewards += 10;
        SetReward(episodeRewards);
    }
    
    public void OnLoseGame()
    {
        AddReward(-10f);
        episodeRewards -= 10;
        SetReward(episodeRewards);
    }
    
    public void SetGameNumber(int gameNumber)
    {
        seriesGameNumber = Mathf.Clamp(gameNumber, 1, 7);
        explorationRate = Mathf.Lerp(1f, 0.1f, (seriesGameNumber - 1) / 6f);
    }
    
    private List<Unit> GetOwnedUnits()
    {
        var units = new List<Unit>();
        foreach (var obj in TurnController.Instance.unitsAndCities)
        {
            if (obj is Unit unit && unit.OwningPlayer == aiPlayer)
                units.Add(unit);
        }
        return units;
    }
    
    private List<Unit> GetEnemyUnits()
    {
        var enemies = new List<Unit>();
        foreach (var obj in TurnController.Instance.unitsAndCities)
        {
            if (obj is Unit unit && unit.OwningPlayer != aiPlayer)
                enemies.Add(unit);
        }
        return enemies;
    }
}