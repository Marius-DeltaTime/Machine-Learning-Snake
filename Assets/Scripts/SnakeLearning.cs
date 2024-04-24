using UnityEngine;
using System.Collections.Generic;
using static SnakeLearning;
using System;
using Sirenix.OdinInspector;


public class SnakeLearning : MonoBehaviour
{
    [ShowInInspector]
    [ReadOnly]
    [FoldoutGroup("Q-Table")]
    public Dictionary<State, Dictionary<SnakeLearning.SnakeAction, float>> qTableRef = new Dictionary<State, Dictionary<SnakeLearning.SnakeAction, float>>();

    public static SnakeLearning instance;

    public enum SnakeAction { DoNothing, TurnRight, TurnLeft }

    public float foodReward = 10f;
    public float survivalReward = 0.1f;
    public float collisionPenalty = -5f;

    private float startTime, lastRewardTime;
    private List<Vector3> bodyPositions = new List<Vector3>();
    private List<Quaternion> bodyRotations = new List<Quaternion>();
    private Transform foodTransform;
    private Vector3 headPosition;
    private Quaternion headRotation;
    private float reward;
    private State thisState, nextState;
    private SnakeAction thisAction, nextAction;
    private uint foodEaten;
    private uint aliveScore;

    void Awake()
    {
        qTableRef = QLearning.qTable;
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void CalculateTimeScore()
    {
        /*
        float timeElapsed = Time.time - startTime;
        float timeSinceLastReward = Time.time - lastRewardTime;
        long millisecondsAlive = (long)(timeElapsed * 1000);
        timeScore = (long)(millisecondsAlive * 0.00001);
        // I can change this to just increment a score everytime the invoke movement event triggers.
        */
    }

    void GetSnakeHeadPosition()
    {
        GameObject snakeHead = GameObject.FindGameObjectWithTag("Snake_Head");
        if (snakeHead != null)
        {
            headPosition = snakeHead.transform.position;
            headRotation = snakeHead.transform.rotation;
        }
    }

    void GetFoodPosition()
    {
        GameObject food = GameObject.FindGameObjectWithTag("Food");
        if (food != null)
        {
            foodTransform = food.transform;
        }
    }

    void GetBodyPositions()
    {
        bodyPositions.Clear();
        bodyRotations.Clear();

        GameObject[] bodyObjects = GameObject.FindGameObjectsWithTag("Snake_Body");

        foreach (GameObject body in bodyObjects)
        {
            bodyPositions.Add(body.transform.position);
            bodyRotations.Add(body.transform.rotation);
        }
    }

    void OnMovementEnabled()
    {
        aliveScore++;
        GetBodyPositions();
        GetSnakeHeadPosition();
        GetFoodPosition();

        thisState = QLearningManager.Instance.QLearningInstance.GetState(headPosition, headRotation, bodyPositions, bodyRotations, foodTransform, QLearning.qTable, QLearning.states);

        if (QLearning.qTable.ContainsKey(thisState))
        {
            thisAction = GetAction(thisState);

            ExecuteAction(thisAction);

            nextState = QLearningManager.Instance.QLearningInstance.GetState(headPosition, headRotation, bodyPositions, bodyRotations, foodTransform, QLearning.futureQTable, QLearning.futureStates);
            nextState = QLearningManager.Instance.QLearningInstance.SimulateActionAndGetNextState(thisState, thisAction);

            CalculateReward(thisState);
            CalculateReward(nextState);

            QLearningManager.Instance.QLearningInstance.UpdateQValue(thisState, thisAction, reward, nextState, 1.5f, 1.7f);
        }
        else
        {
            Debug.LogWarning("Current state not found in Q-table.");
        }  
    }

    void OnDestroy()
    {
        SnakeController.MovementEnabled -= OnMovementEnabled;
    }

    void Start()
    {
        SnakeController.MovementEnabled += OnMovementEnabled;

        QLearningManager.Instance.QLearningInstance.InitializeQTable(QLearning.states, QLearning.qTable);
        QLearningManager.Instance.QLearningInstance.InitializeQTable(QLearning.futureStates, QLearning.futureQTable);

        startTime = Time.time;
        lastRewardTime = startTime;

        GetBodyPositions();
        GetSnakeHeadPosition();
        GetFoodPosition();

        thisState = QLearningManager.Instance.QLearningInstance.GetState(headPosition, headRotation, bodyPositions, bodyRotations, foodTransform, QLearning.qTable, QLearning.states);
        nextState = QLearningManager.Instance.QLearningInstance.GetState(headPosition, headRotation, bodyPositions, bodyRotations, foodTransform, QLearning.futureQTable, QLearning.futureStates);
    }

    void Update()
    {
        CalculateTimeScore();
    }

    SnakeAction GetAction(State state)
    {
        SnakeAction bestAction = SnakeAction.DoNothing;
        float bestQValue = float.MinValue;

        if (QLearning.qTable.ContainsKey(state))
        {
            foreach (var action in Enum.GetValues(typeof(SnakeAction)))
            {
                float qValue = QLearning.qTable[state][(SnakeAction)action];
                if (qValue > bestQValue)
                {
                    bestQValue = qValue;
                    bestAction = (SnakeAction)action;
                }
            }
        }

        return bestAction;
    }

    void ExecuteAction(SnakeAction action)
    {
        switch (action)
        {
            case SnakeAction.DoNothing:
                break;
            case SnakeAction.TurnRight:
                SnakeController.instance.RotateSnakeClockwise();
                break;
            case SnakeAction.TurnLeft:
                SnakeController.instance.RotateSnakeCounterClockwise();
                break;
        }
    }

    float CalculateDistance(Vector3 position1, Vector3 position2)
    {
        return Vector3.Distance(position1, position2);
    }

    void CalculateReward(State state)
    {
        float distanceToFood = CalculateDistance(state.HeadPosition, state.FoodTransform.position);

        float maxReward = 1f;
        float minReward = 0.0f;
        float maxDistance = 10.0f;

        float currentReward = Mathf.Lerp(maxReward, minReward, distanceToFood / maxDistance);

        // Calculate the reward based on conditions
        reward = currentReward + aliveScore;

        // Apply the reward to the Q-value for the current state-action pair
        if (IsCollidingWithBody(state))
        {
            QLearningManager.Instance.QLearningInstance.UpdateQValue(state, thisAction, reward - collisionPenalty, nextState, 1.5f, 1.7f);
        }
        else if (IsHeadAtFoodPosition(state))
        {
            QLearningManager.Instance.QLearningInstance.UpdateQValue(state, thisAction, reward + foodReward, nextState, 1.5f, 1.7f);
        }
        else
        {
            QLearningManager.Instance.QLearningInstance.UpdateQValue(state, thisAction, reward, nextState, 1.5f, 1.7f);
        }
    }

    bool IsCollidingWithBody(State state)
    {
        /*
        foreach (var bodyPos in state.BodyTransforms)
        {
            if (state.HeadTransform.position == bodyPos.position)
            {
                return true; 
            }
        }
        return false; 
        */
        return false;
    }

    bool IsHeadAtFoodPosition(State state)
    {
        //return state.HeadTransform.position == state.FoodTransform.position;
        return false;
    }

    public void OnFoodEaten()
    {
        GetFoodPosition();
        foodEaten++;
    }

    public void OnCollisionWithBody()
    {
        reward += collisionPenalty;
    }
}