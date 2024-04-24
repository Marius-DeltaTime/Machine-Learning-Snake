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
        string currentStateInfo = "Current State:";
        currentStateInfo += "\n\tHead Position: " + state.HeadTransform.position;
        currentStateInfo += "\n\tHead Rotation: " + state.HeadTransform.rotation.eulerAngles;
        currentStateInfo += "\n\tBody Positions:";
        foreach (var bodyPos in state.BodyTransforms)
        {
            currentStateInfo += "\n\t\tPosition: " + bodyPos.position + ", Rotation: " + bodyPos.rotation.eulerAngles;
        }
        currentStateInfo += "\n\tFood Position: " + state.FoodTransform.position;

        string nextStateInfo = "Next State:";
        nextStateInfo += "\n\tHead Position: " + nextState.HeadTransform.position;
        nextStateInfo += "\n\tHead Rotation: " + nextState.HeadTransform.rotation.eulerAngles;
        nextStateInfo += "\n\tBody Positions:";
        foreach (var bodyPos in nextState.BodyTransforms)
        {
            nextStateInfo += "\n\t\tPosition: " + bodyPos.position + ", Rotation: " + bodyPos.rotation.eulerAngles;
        }
        nextStateInfo += "\n\tFood Position: " + nextState.FoodTransform.position;

        Debug.Log("This: " + currentStateInfo + "\nNext: " + nextStateInfo);

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

        State newState = new State(head, new List<Transform>(body), food);

        if (!qTable.ContainsKey(newState))
        {

            qTable[newState] = new Dictionary<SnakeLearning.SnakeAction, float>();

            foreach (var action in GetAllPossibleActions())
            {
                qTable[newState][action] = 0.0f;
            }
        }
        else
        {
            Debug.LogWarning("State already exists in qTable.");
        }
        states.Add(newState);

        return newState;
    }



    public State SimulateActionAndGetNextState(State currentState, SnakeLearning.SnakeAction action)
    {
        Transform currentHeadTransform = currentState.HeadTransform;

        GameObject newHeadGameObject = new GameObject("SimulatedHead");
        Transform simulatedHeadTransform = newHeadGameObject.transform;
        simulatedHeadTransform.position = currentHeadTransform.position;
        simulatedHeadTransform.rotation = currentHeadTransform.rotation;
        simulatedHeadTransform.localScale = currentHeadTransform.localScale;

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

        // Apply the offset to the head position to simulate movement
        simulatedHeadTransform.position += offset;
        //Debug.Log("Current Head: " + currentState.HeadTransform.position + " Simulated Head: " + simulatedHeadTransform.position);

        // Copy the current body positions
        List<GameObject> simulatedBodyGameObject = new List<GameObject>();
        List<Transform> simulatedBodyTransforms = new List<Transform>();
        foreach (var bodyTransform in currentState.BodyTransforms)
        {
            Transform currentBodyTransform = bodyTransform;
            GameObject newBodyGameObject = new GameObject("SimulatedBody");
            Transform simulatedBodyTransform = newBodyGameObject.transform;
            simulatedBodyTransform.position = currentBodyTransform.position;
            simulatedBodyTransform.position += offset;
            simulatedBodyTransform.rotation = currentBodyTransform.rotation;
            simulatedBodyTransform.localScale = currentBodyTransform.localScale;
            simulatedBodyGameObject.Add(newBodyGameObject);
            simulatedBodyTransforms.Add(simulatedBodyTransform);
        }

        // Create a new state representing the simulated state
        // Remember to change the first parameter to simulatedHeadTransform
        State simulatedState = new State(simulatedHeadTransform, simulatedBodyTransforms, currentState.FoodTransform);

        foreach (var item in simulatedBodyGameObject)
        {
            UnityEngine.Object.Destroy(item);
        }
        UnityEngine.Object.Destroy(newHeadGameObject);
        return simulatedState;
    }
}

public class State
{
    public Transform HeadTransform { get; private set; }
    public List<Transform> BodyTransforms { get; private set; }
    public Transform FoodTransform { get; private set; }

    public State(Transform headTransform, List<Transform> bodyTransforms, Transform foodTransform)
    {
        HeadTransform = headTransform;
        BodyTransforms = bodyTransforms;
        FoodTransform = foodTransform;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is State))
            return false;

        State other = (State)obj;

        if (HeadTransform != other.HeadTransform)
            return false;

        if (BodyTransforms.Count != other.BodyTransforms.Count)
            return false;

        for (int i = 0; i < BodyTransforms.Count; i++)
        {
            if (BodyTransforms[i] != other.BodyTransforms[i])
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

            hash = hash * 23 + HeadTransform.GetHashCode();

            foreach (var bodyPos in BodyTransforms)
            {
                hash = hash * 23 + bodyPos.GetHashCode();
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

