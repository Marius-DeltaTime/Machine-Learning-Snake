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
    private List<Transform> segmentTransforms = new List<Transform>();
    private Transform headTransform, foodTransform;
    private float reward;
    private State thisState, nextState;
    private SnakeAction thisAction, nextAction;
    private uint foodEaten;
    private long timeScore;

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
        float timeElapsed = Time.time - startTime;
        float timeSinceLastReward = Time.time - lastRewardTime;
        long millisecondsAlive = (long)(timeElapsed * 1000);
        timeScore = (long)(millisecondsAlive * 0.01);
    }

    void GetSnakeHeadPosition()
    {
        GameObject snakeHead = GameObject.FindGameObjectWithTag("Snake_Head");
        if (snakeHead != null)
        {
            headTransform = snakeHead.transform;
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

    void GetSegmentPositions()
    {
        GameObject[] snakeBodySegments = GameObject.FindGameObjectsWithTag("Snake_Body");
        foreach (GameObject segment in snakeBodySegments)
        {
            segmentTransforms.Add(segment.transform);
        }
    }

    void OnMovementEnabled()
    {
        Debug.Log(SnakeController.instance.canMove);
        
            GetSegmentPositions();
            GetSnakeHeadPosition();
            GetFoodPosition();

            thisState = QLearningManager.Instance.QLearningInstance.GetState(headTransform, segmentTransforms, foodTransform);

            if (QLearning.qTable.ContainsKey(thisState))
            {
                thisAction = GetAction(thisState);

                ExecuteAction(thisAction);

                nextState = QLearningManager.Instance.QLearningInstance.SimulateActionAndGetNextState(thisState, thisAction);

                CalculateReward(thisState);

                QLearningManager.Instance.QLearningInstance.UpdateQValue(thisState, thisAction, reward, nextState, 1.5f, 1.7f);

                DebugQTable();
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

        QLearningManager.Instance.QLearningInstance.InitializeQTable();

        startTime = Time.time;
        lastRewardTime = startTime;

        GetSegmentPositions();
        GetSnakeHeadPosition();
        GetFoodPosition();

        QLearningManager.Instance.QLearningInstance.GetState(headTransform, segmentTransforms, foodTransform);
    }

    void Update()
    {
        CalculateTimeScore();
    }

    void DebugQTable()
    {
        Debug.Log("Q-Table Contents:");
        foreach (var state in QLearning.qTable)
        {
            string stateInfo = state.Key.ToString();
            foreach (var action in state.Value)
            {
                stateInfo += "\n\t" + action.Key.ToString() + ": " + action.Value.ToString();
            }
            Debug.Log(stateInfo);
        }
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
        float distanceToFood = CalculateDistance(state.HeadTransform.position, state.FoodTransform.position);

        float maxReward = 1f; 
        float minReward = 0.0f;       
        float maxDistance = 10.0f;    

        float currentReward = Mathf.Lerp(maxReward, minReward, distanceToFood / maxDistance);


        if (IsCollidingWithBody(state))
        {
            reward -= collisionPenalty;
        }

        if (IsHeadAtFoodPosition(state))
        {
            reward += foodReward;
        }

        reward = currentReward + timeScore;
    }

    bool IsCollidingWithBody(State state)
    {
        foreach (var bodyPos in state.BodyTransforms)
        {
            if (state.HeadTransform.position == bodyPos.position)
            {
                return true; 
            }
        }
        return false; 
    }

    bool IsHeadAtFoodPosition(State state)
    {
        return state.HeadTransform.position == state.FoodTransform.position;
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