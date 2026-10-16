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
    [SerializeField] private bool isTraining;
    private int episodeRewards = 0;
    private float explorationRate = 1f; // Start at 100% exploration
    [SerializeField] private bool shouldDecide;
    
    public override void Initialize()
    {
        if (aiPlayer == null)
        {
            aiPlayer = GetComponent<Player>();
        }
    }

    void Start()
    {
        TurnController.Instance.OnAIInferenceControl += OnAITurn;
    }

    private void OnDestroy()
    {
        if (TurnController.Instance != null)
        {
            TurnController.Instance.OnAIInferenceControl -= OnAITurn;
        }
    }
    
    private void OnAITurn()
    {
        shouldDecide = true;
    }
    
    public override void OnEpisodeBegin()
    {
        episodeRewards = 0;
        // Adjust exploration rate based on series progress
        if(!isTraining)
        {
            explorationRate = Mathf.Lerp(1f, 0f, (seriesGameNumber - 1) / 6f); // 1 => 0 across 7 games
        }
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
        
        sensor.AddObservation(aiPlayer.Gold / 1000f);
        sensor.AddObservation(aiPlayer.Science / 1000f);
        sensor.AddObservation(aiPlayer.Population / 100f);
    
        List<Unit> ownedUnits = GetOwnedUnits();
        List<Unit> enemies = GetEnemyUnits();
        
        sensor.AddObservation(ownedUnits.Count / 10f);
        sensor.AddObservation(enemies.Count / 10f);
        
        for (int i = 0; i < 20; i++)
        {
            if (i < ownedUnits.Count)
            {
                Unit unit = ownedUnits[i];
                City capital = aiPlayer.Capital;
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
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }
        
        for (int i = 0; i < 20; i++)
        {
            if (i < enemies.Count)
            {
                Unit enemy = enemies[i];
                if (aiPlayer.Capital != null)
                {
                    Vector2 relativePos = enemy.Position - aiPlayer.Capital.Position;
                    sensor.AddObservation(relativePos.x / 100f);
                    sensor.AddObservation(relativePos.y / 100f);
                }
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!shouldDecide)
            return;
        
        ActionSegment<int> discreteActions = actions.DiscreteActions;

        ExecuteAction(discreteActions);
        shouldDecide = false;
    }
    
    private void ExecuteAction(ActionSegment<int> discreteActions)
    {
        List<Unit> ownedUnits = GetOwnedUnits();
        if (ownedUnits.Count == 0)
        {
            AddReward(-0.01f); // Small penalty for having no units
        }

        List<City> ownedCities = GetOwnedCities();
        if (ownedCities.Count == 0)
        {
            AddReward(-0.15f); // "Big" penalty for having no cities
        }
        
        int actionIndex = 0;
        foreach (Unit selectedUnit in ownedUnits)
        {
            int action = discreteActions[Helpers.mod(actionIndex, discreteActions.Length)];
            if (Random.value < explorationRate)
            {
                // Random action (early games seem unpredictable)
                action = Random.Range(0, 5);
                
                if ((selectedUnit.GetUnitType != Unit.UnitType.Settler && selectedUnit.GetUnitType != Unit.UnitType.Builder) && action == 4)
                {
                    action = Random.Range(0, 4); //re roll if the unit cannot do Build or Settle
                }
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
                    break;
            }

            actionIndex++;
        }
        foreach (City city in ownedCities)
        {
            int action = discreteActions[Helpers.mod(actionIndex, discreteActions.Length)];

            if (Random.value < explorationRate)
            {
                action = Random.Range(0, 8);
            }
        
            switch (action)
            {
                case 0: // Produce Ranged unit
                    city.StartProduction(0);
                    AddReward(0.05f);
                    break;
                case 1: // Produce Melee unit
                    city.StartProduction(1);
                    AddReward(0.05f);
                    break;
                case 2: // Produce Settler unit
                    city.StartProduction(2);
                    AddReward(0.05f);
                    break;
                case 3: // Produce Builder unit
                    city.StartProduction(3);
                    AddReward(0.05f);
                    break;
                case 4: // Produce Campus district
                    city.StartProduction(4);
                    AddReward(0.08f);
                    break;
                case 5: // Produce Encampment district
                    city.StartProduction(5);
                    AddReward(0.08f);
                    break;
                case 6: // Fortify City
                    city.FortifyCity();
                    AddReward(-0.03f);
                    break;
                case 7: // Close City Gates
                    city.CloseCity();
                    AddReward(-0.03f);
                    break;
            }

            actionIndex++;
        }
        TurnController.Instance.SetPlayerReady(aiPlayer.PlayerIndex, true);
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
        List<Unit> units = new List<Unit>();
        foreach (object obj in TurnController.Instance.unitsAndCities)
        {
            if (obj is Unit unit && unit.OwningPlayer == aiPlayer)
                units.Add(unit);
        }
        return units;
    }
    
    private List<Unit> GetEnemyUnits()
    {
        List<Unit> enemies = new List<Unit>();
        foreach (object obj in TurnController.Instance.unitsAndCities)
        {
            if (obj is Unit unit && unit.OwningPlayer != aiPlayer)
                enemies.Add(unit);
        }
        return enemies;
    }
    
    private List<City> GetOwnedCities()
    {
        List<City> cities = new List<City>();
        foreach (object obj in TurnController.Instance.unitsAndCities)
        {
            if (obj is City city && city.OwningPlayer == aiPlayer)
                cities.Add(city);
        }
        return cities;
    }
    
    private List<City> GetEnemyCities()
    {
        List<City> cities = new List<City>();
        foreach (object obj in TurnController.Instance.unitsAndCities)
        {
            if (obj is City city && city.OwningPlayer != aiPlayer)
                cities.Add(city);
        }
        return cities;
    }
}