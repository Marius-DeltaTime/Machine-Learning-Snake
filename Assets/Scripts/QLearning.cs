using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

public class QLearning
{
    public static Dictionary<State, Dictionary<SnakeLearning.SnakeAction, float>> qTable = new Dictionary<State, Dictionary<SnakeLearning.SnakeAction, float>>();
    List<State> states = new List<State>();

    public QLearning()
    {
        InitializeQTable();
    }

    public void InitializeQTable()
    {
        foreach (var state in states)
        {
            qTable[state] = new Dictionary<SnakeLearning.SnakeAction, float>();
            foreach (var action in GetAllPossibleActions())
            {
                qTable[state][action] = 0.0F;
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
        float currentQValue = qTable[state][action];
        float maxNextQValue = qTable[nextState].Values.Max();
        float newQValue = currentQValue + alpha * (reward + gamma * maxNextQValue - currentQValue);
        qTable[state][action] = newQValue;
    }

    public State GetState(Transform head, List<Transform> body, Transform food)
    {
        if (head == null || body == null || food == null)
        {
            Debug.LogError("One or more parameters are null in GetState method.");
            if (head == null)
            {
                Debug.LogError("Head is null.");
            }
            else if (body == null)
            {
                Debug.LogError("Body is null.");
            }
            else
            {
                Debug.LogError("Food is null.");
            }
            return null;
        }

        State newState = new State(head, body, food);

        if (!qTable.ContainsKey(newState))
        {
            qTable[newState] = new Dictionary<SnakeLearning.SnakeAction, float>();

            foreach (var action in GetAllPossibleActions())
            {
                qTable[newState][action] = 0.0f;
            }
        }
        states.Add(newState);

        return newState;
    }

    public State SimulateActionAndGetNextState(State currentState, SnakeLearning.SnakeAction action)
    {
        List<Transform> copiedBodyPositions = new List<Transform>();
        foreach (Transform bodyTransform in currentState.BodyPositions)
        {
            copiedBodyPositions.Clear();
            copiedBodyPositions.Add(bodyTransform);
        }
        Transform simulatedHeadTransform = currentState.HeadPosition;

        Quaternion headRotation = simulatedHeadTransform.rotation;
        float rotationAngle = 90f;
        Vector2 moveDirection = Vector2.up;

        switch (action)
        {
            case SnakeLearning.SnakeAction.TurnRight:
                headRotation *= Quaternion.Euler(0, 0, -rotationAngle);
                moveDirection = Quaternion.Euler(0, 0, -rotationAngle) * moveDirection;
                break;
            case SnakeLearning.SnakeAction.TurnLeft:
                headRotation *= Quaternion.Euler(0, 0, rotationAngle);
                moveDirection = Quaternion.Euler(0, 0, rotationAngle) * moveDirection;
                break;
            case SnakeLearning.SnakeAction.DoNothing:
                break;
        }

        Vector3 newHeadPosition = simulatedHeadTransform.position + (Vector3)moveDirection * 0.25f;

        Vector3 offset = Vector3.zero;
        if (moveDirection == Vector2.up)
        {
            offset = new Vector3(0, -0.25f, 0);
        }
        else if (moveDirection == Vector2.down)
        {
            offset = new Vector3(0, 0.25f, 0);
        }
        else if (moveDirection == Vector2.right)
        {
            offset = new Vector3(-0.25f, 0, 0);
        }
        else if (moveDirection == Vector2.left)
        {
            offset = new Vector3(0.25f, 0, 0);
        }

        for (int i = 0; i < copiedBodyPositions.Count; i++)
        {
            copiedBodyPositions[i].position = copiedBodyPositions[i].position + offset;

        }
        State simulatedState = new State(simulatedHeadTransform, copiedBodyPositions, currentState.FoodPosition);

        return simulatedState;
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

        if (HeadPosition.position != other.HeadPosition.position || HeadPosition.rotation != other.HeadPosition.rotation)
            return false;

        if (BodyPositions.Count != other.BodyPositions.Count)
            return false;

        for (int i = 0; i < BodyPositions.Count; i++)
        {
            if (BodyPositions[i].position != other.BodyPositions[i].position || BodyPositions[i].rotation != other.BodyPositions[i].rotation)
                return false;
        }

        if (FoodPosition.position != other.FoodPosition.position || FoodPosition.rotation != other.FoodPosition.rotation)
            return false;

        return true;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;

            hash = hash * 23 + HeadPosition.position.GetHashCode();
            hash = hash * 23 + HeadPosition.rotation.GetHashCode();

            foreach (var bodyPos in BodyPositions)
            {
                hash = hash * 23 + bodyPos.position.GetHashCode();
                hash = hash * 23 + bodyPos.rotation.GetHashCode();
            }

            hash = hash * 23 + FoodPosition.position.GetHashCode();
            hash = hash * 23 + FoodPosition.rotation.GetHashCode();

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

