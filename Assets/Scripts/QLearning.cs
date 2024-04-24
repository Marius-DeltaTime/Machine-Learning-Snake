using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

public class QLearning
{
    public static Dictionary<State, Dictionary<SnakeLearning.SnakeAction, float>> qTable = new Dictionary<State, Dictionary<SnakeLearning.SnakeAction, float>>();
    public static Dictionary<State, Dictionary<SnakeLearning.SnakeAction, float>> futureQTable = new Dictionary<State, Dictionary<SnakeLearning.SnakeAction, float>>();
    public static List<State> states = new List<State>();
    public static List<State> futureStates = new List<State>();

    public QLearning()
    {
        InitializeQTable(states, qTable);
        InitializeQTable(futureStates, futureQTable);
    }

    public void InitializeQTable(List<State> stateList, Dictionary<State, Dictionary<SnakeLearning.SnakeAction, float>> dictionary)
    {
        foreach (var state in stateList)
        {
            dictionary[state] = new Dictionary<SnakeLearning.SnakeAction, float>();
            foreach (var action in GetAllPossibleActions())
            {
                dictionary[state][action] = 0.0F;
            }
        }
    }

    private List<SnakeLearning.SnakeAction> GetAllPossibleActions()
    {
        List<SnakeLearning.SnakeAction> actions = new List<SnakeLearning.SnakeAction>();
        actions.Add(SnakeLearning.SnakeAction.DoNothing);
        actions.Add(SnakeLearning.SnakeAction.TurnRight);
        actions.Add(SnakeLearning.SnakeAction.TurnLeft);
        return actions;
    }

    public void UpdateQValue(State state, SnakeLearning.SnakeAction action, float reward, State nextState, float alpha, float gamma)
    {
        string currentStateInfo = "Current State:";
        currentStateInfo += "\n\tHead Position: " + state.HeadPosition;
        currentStateInfo += "\n\tHead Rotation: " + state.HeadRotation.eulerAngles;
        currentStateInfo += "\n\tBody Positions:";
        for (int i = 0; i < state.BodyPositions.Count; i++)
        {
            currentStateInfo += "\n\t\tPosition: " + state.BodyPositions[i] + ", Rotation: " + state.BodyRotations[i].eulerAngles;
        }
        currentStateInfo += "\n\tFood Position: " + state.FoodTransform.position;
        currentStateInfo += "\n\tAction: " + action.ToString();
        currentStateInfo += "\n\tReward: " + reward;

        string nextStateInfo = "Next State:";
        nextStateInfo += "\n\tHead Position: " + nextState.HeadPosition;
        nextStateInfo += "\n\tHead Rotation: " + nextState.HeadRotation.eulerAngles;
        nextStateInfo += "\n\tBody Positions:";
        for (int i = 0; i < nextState.BodyPositions.Count; i++)
        {
            nextStateInfo += "\n\t\tPosition: " + nextState.BodyPositions[i] + ", Rotation: " + nextState.BodyRotations[i].eulerAngles;
        }
        nextStateInfo += "\n\tFood Position: " + nextState.FoodTransform.position;
        nextStateInfo += "\n\tReward: " + reward;

        Debug.Log("This: " + currentStateInfo + "\nNext: " + nextStateInfo);

        float currentQValue = qTable[state][action];
        float maxNextQValue = futureQTable[nextState].Values.Max();
        float newQValue = currentQValue + alpha * (reward + gamma * maxNextQValue - currentQValue);
        qTable[state][action] = newQValue;
    }

    public State GetState(Vector3 headPos, Quaternion headRotaton, List<Vector3> bodyPositions, List<Quaternion> bodyRotations, Transform food, Dictionary<State, Dictionary<SnakeLearning.SnakeAction, float>> dictionary, List<State> stateList)
    {
        if (headPos == null || headRotaton == null || bodyPositions == null || bodyRotations == null || food == null)
        {
            Debug.LogError("One or more parameters are null in GetState method.");
            if (headPos == null)
            {
                Debug.LogError("headPos is null.");
            }
            else if (headRotaton == null)
            {
                Debug.LogError("headRotaton is null.");
            }
            else if (bodyPositions == null)
            {
                Debug.LogError("bodyPositions is null.");
            }
            else if (bodyRotations == null)
            {
                Debug.LogError("bodyRotations is null.");
            }
            else
            {
                Debug.LogError("Food is null.");
            }
            return null;
        }

        State newState = new State(headPos, headRotaton, new List<Vector3>(bodyPositions), new List<Quaternion>(bodyRotations), food);

        if (!dictionary.ContainsKey(newState))
        {

            dictionary[newState] = new Dictionary<SnakeLearning.SnakeAction, float>();

            foreach (var action in GetAllPossibleActions())
            {
                dictionary[newState][action] = 0.0f;
            }
        }
        else
        {
            Debug.LogWarning("State already exists in qTable.");
        }
        stateList.Add(newState);

        return newState;
    }

    public State SimulateActionAndGetNextState(State currentState, SnakeLearning.SnakeAction action)
    {
        Vector3 simulatedHeadPosition = currentState.HeadPosition;
        Quaternion simulatedHeadRotation = currentState.HeadRotation;

        Vector2 moveDirection = Vector2.up;

        switch (action)
        {
            case SnakeLearning.SnakeAction.TurnRight:
                moveDirection *= Quaternion.Euler(0, 0, -90) * moveDirection;
                break;
            case SnakeLearning.SnakeAction.TurnLeft:
                moveDirection *= Quaternion.Euler(0, 0, 90) * moveDirection;
                break;
            case SnakeLearning.SnakeAction.DoNothing:
                break;
        }

        Vector3 offset = Vector3.zero;
        if (moveDirection == Vector2.up)
        {
            offset = new Vector3(0, 0.25f, 0);
        }
        else if (moveDirection == Vector2.down)
        {
            offset = new Vector3(0, -0.25f, 0);
        }
        else if (moveDirection == Vector2.right)
        {
            offset = new Vector3(-0.25f, 0, 0);
        }
        else if (moveDirection == Vector2.left)
        {
            offset = new Vector3(0.25f, 0, 0);
        }

        simulatedHeadPosition += offset;

        List<Quaternion> simulatedBodyRotations = new List<Quaternion>();
        foreach (var bodyRotate in currentState.BodyRotations)
        {
            Quaternion simulatedBodyRotation = bodyRotate;
            simulatedBodyRotations.Add(simulatedBodyRotation);
        }

        List<Vector3> simulatedBodyPositions = new List<Vector3>();
        foreach (var bodyPosition in currentState.BodyPositions)
        {
            Vector3 simulatedBodyPosition = bodyPosition;
            simulatedBodyPosition += offset;
            simulatedBodyPositions.Add(simulatedBodyPosition);
        }

        State simulatedState = new State(simulatedHeadPosition, simulatedHeadRotation, simulatedBodyPositions, simulatedBodyRotations, currentState.FoodTransform);

        return simulatedState;
    }
}

public class State
{
    public Vector3 HeadPosition { get; private set; }
    public Quaternion HeadRotation { get; private set; }
    public List<Vector3> BodyPositions { get; private set; }
    public List<Quaternion> BodyRotations { get; private set; }
    public Transform FoodTransform { get; private set; }

    public State(Vector3 headPosition, Quaternion headRotation, List<Vector3> bodyPositions, List<Quaternion> bodyRotations, Transform foodTransform)
    {
        HeadPosition = headPosition;
        HeadRotation = headRotation;
        BodyPositions = bodyPositions;
        BodyRotations = bodyRotations;
        FoodTransform = foodTransform;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is State))
            return false;

        State other = (State)obj;

        if (HeadPosition != other.HeadPosition)
            return false;

        if (HeadRotation != other.HeadRotation)
            return false;

        if (BodyPositions.Count != other.BodyPositions.Count)
            return false;

        for (int i = 0; i < BodyPositions.Count; i++)
        {
            if (BodyPositions[i] != other.BodyPositions[i])
                return false;
        }

        if (BodyRotations.Count != other.BodyRotations.Count)
            return false;

        for (int i = 0; i < BodyRotations.Count; i++)
        {
            if (BodyRotations[i] != other.BodyRotations[i])
                return false;
        }

        if (FoodTransform != other.FoodTransform)
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;

            hash = hash * 23 + HeadPosition.GetHashCode();
            hash = hash * 23 + HeadRotation.GetHashCode();

            foreach (var bodyPos in BodyPositions)
            {
                hash = hash * 23 + bodyPos.GetHashCode();
            }          

            foreach (var bodyRot in BodyRotations)
            {
                hash = hash * 23 + bodyRot.GetHashCode();
            }

            hash = hash * 23 + FoodTransform.GetHashCode();

            return hash;
        }
    }

}

public class QLearningManager
{
    private static QLearningManager instance;
    private QLearning qLearningInstance;

    private QLearningManager()
    {
        qLearningInstance = new QLearning();
    }

    public static QLearningManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new QLearningManager();
            }
            return instance;
        }
    }

    public QLearning QLearningInstance
    {
        get { return qLearningInstance; }
    }
}

