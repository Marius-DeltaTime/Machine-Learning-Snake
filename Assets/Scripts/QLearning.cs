using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QLearning
{
    private Dictionary<State, float[]> qTable;
    private float learningRate = 0.1f;
    private float discountFactor = 0.9f;
    private float initialQValue = 0f;  // Initial Q-value for all state-action pairs

    public QLearning()
    {
        qTable = new Dictionary<State, float[]>();
    }

    public void InitializeQTable()
    {
        // Note: In a real scenario, we would dynamically populate this based on observed states.
        // Here, we assume a fixed set of initial states for demonstration purposes.

        List<Vector3> possibleHeadPositions = GetPossiblePositions();
        List<Vector3> possibleFoodPositions = GetPossiblePositions();
        List<List<Vector3>> possibleBodyConfigurations = GetPossibleBodyConfigurations();

        foreach (var headPos in possibleHeadPositions)
        {
            foreach (var foodPos in possibleFoodPositions)
            {
                foreach (var bodyConfig in possibleBodyConfigurations)
                {
                    State state = new State(headPos, bodyConfig, foodPos);
                    if (!qTable.ContainsKey(state))
                    {
                        qTable[state] = new float[3] { initialQValue, initialQValue, initialQValue };
                    }
                }
            }
        }
    }

    private List<Vector3> GetPossiblePositions()
    {
        // Dummy implementation: Populate with all grid positions within a certain range.
        List<Vector3> positions = new List<Vector3>();
        for (int x = -10; x <= 10; x++)
        {
            for (int y = -10; y <= 10; y++)
            {
                positions.Add(new Vector3(x, y, 0));
            }
        }
        return positions;
    }

    private List<List<Vector3>> GetPossibleBodyConfigurations()
    {
        // Dummy implementation: Return a small set of body configurations for simplicity.
        return new List<List<Vector3>>()
        {
            new List<Vector3> { new Vector3(0, 0, 0), new Vector3(0, -1, 0) },
            new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0) },
            new List<Vector3> { new Vector3(0, 0, 0), new Vector3(0, 1, 0) },
            new List<Vector3> { new Vector3(0, 0, 0), new Vector3(-1, 0, 0) }
        };
    }

    public void UpdateQValue(State state, SnakeAction action, float reward, State nextState)
    {
        if (!qTable.ContainsKey(state))
        {
            qTable[state] = new float[3];
        }

        int actionIndex = (int)action;
        float oldQValue = qTable[state][actionIndex];
        float bestNextQValue = GetBestQValue(nextState);
        float newQValue = oldQValue + learningRate * (reward + discountFactor * bestNextQValue - oldQValue);
        qTable[state][actionIndex] = newQValue;
    }

    private float GetBestQValue(State state)
    {
        if (!qTable.ContainsKey(state))
        {
            qTable[state] = new float[3];
        }

        return Mathf.Max(qTable[state]);
    }

    public State GetState(Vector3 snakeHeadPosition, List<Vector3> snakeBodyPositions, Vector3 foodPosition)
    {
        return new State(snakeHeadPosition, snakeBodyPositions, foodPosition);
    }

    public State SimulateActionAndGetNextState(State currentState, SnakeAction action)
    {
        Vector3 newHeadPosition = SimulateMove(currentState.HeadPosition, action);
        List<Vector3> newBodyPositions = new List<Vector3>(currentState.BodyPositions);
        newBodyPositions.Insert(0, currentState.HeadPosition);
        newBodyPositions.RemoveAt(newBodyPositions.Count - 1);

        return new State(newHeadPosition, newBodyPositions, currentState.FoodPosition);
    }

    private Vector3 SimulateMove(Vector3 currentPosition, SnakeAction action)
    {
        switch (action)
        {
            case SnakeAction.TurnRight:
                return currentPosition + Vector3.right;
            case SnakeAction.TurnLeft:
                return currentPosition + Vector3.left;
            default:
                return currentPosition + Vector3.up;
        }
    }

    public SnakeAction GetBestAction(State state)
    {
        if (!qTable.ContainsKey(state))
        {
            qTable[state] = new float[3];
        }

        int bestActionIndex = 0;
        float bestQValue = qTable[state][0];

        for (int i = 1; i < qTable[state].Length; i++)
        {
            if (qTable[state][i] > bestQValue)
            {
                bestQValue = qTable[state][i];
                bestActionIndex = i;
            }
        }

        return (SnakeAction)bestActionIndex;
    }
}

public class State
{
    public Vector3 HeadPosition { get; }
    public List<Vector3> BodyPositions { get; }
    public Vector3 FoodPosition { get; }

    public State(Vector3 headPosition, List<Vector3> bodyPositions, Vector3 foodPosition)
    {
        HeadPosition = headPosition;
        BodyPositions = bodyPositions;
        FoodPosition = foodPosition;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        State other = (State)obj;
        return HeadPosition.Equals(other.HeadPosition) &&
               FoodPosition.Equals(other.FoodPosition) &&
               BodyPositions.SequenceEqual(other.BodyPositions);
    }

    public override int GetHashCode()
    {
        int hash = HeadPosition.GetHashCode() ^ FoodPosition.GetHashCode();
        foreach (var pos in BodyPositions)
        {
            hash ^= pos.GetHashCode();
        }
        return hash;
    }
}

public enum SnakeAction
{
    DoNothing,
    TurnRight,
    TurnLeft
}
