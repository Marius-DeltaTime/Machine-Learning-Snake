using System;
using System.Collections.Generic;
using UnityEngine;

public class QLearning
{
    public static QLearning instance;

    private Dictionary<State, Dictionary<Action, float>> qTable = new Dictionary<State, Dictionary<Action, float>>();
    List<State> states = new List<State>();

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public QLearning()
    {
        InitializeQTable();
    }

    private void InitializeQTable()
    {
        foreach (var state in GetAllPossibleStates())
        {
            qTable[state] = new Dictionary<Action, float>();
            foreach (var action in GetAllPossibleActions())
            {
                qTable[state][action] = 0.0;
            }
        }
    }

    private List<State> GetAllPossibleStates()
    {
        List<State> states = new List<State>();
        return states;
    }

    private List<Action> GetAllPossibleActions()
    {
        List<Action> actions = new List<Action>();
        return actions;
    }

    public void UpdateQValue(State state, Action action, float reward, State nextState, float alpha, float gamma)
    {
        float currentQValue = qTable[state][action];
        float maxNextQValue = qTable[nextState].Values.Max();
        float newQValue = currentQValue + alpha * (reward + gamma * maxNextQValue - currentQValue);
        qTable[state][action] = newQValue;
    }

    public void AddState(Transform head, List<Transform> body, Transform food)
    {
        State newState = new State
        (
            head,
            body,
            food
        );

        states.Add(newState);
    }
}

public class State
{
    public Transform HeadPosition { get; private set; }
    public List<Transform> BodyPositions { get; private set; }
    public Transform FoodPosition { get; private set; }

    public State(Transform headPosition, List<Transform> bodyPositions, Transform foodPosition)
    {
        HeadPosition = headPosition;
        BodyPositions = bodyPositions;
        FoodPosition = foodPosition;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is State))
            return false;

        State other = (State)obj;
        return HeadPosition == other.HeadPosition &&
               BodyPositions.SequenceEqual(other.BodyPositions) &&
               FoodPosition == other.FoodPosition;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + HeadPosition.GetHashCode();
            foreach (var pos in BodyPositions)
            {
                hash = hash * 23 + pos.GetHashCode();
            }
            hash = hash * 23 + FoodPosition.GetHashCode();
            return hash;
        }
    }
}
