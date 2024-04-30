using UnityEngine;
using System.Collections.Generic;
using static SnakeLearning;
using System;
using Sirenix.OdinInspector;


public class SnakeLearning : MonoBehaviour
{
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
    private State thisState, nextState;
    private SnakeAction thisAction, nextAction;
    private uint foodEaten;
    private uint aliveScore;

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
        nextState = QLearningManager.Instance.QLearningInstance.SimulateActionAndGetNextState(thisState, nextAction);

        if (QLearning.qTable.ContainsKey(thisState))
        {
            CalculateReward(thisState, QLearning.qTable);
            CalculateReward(nextState, QLearning.futureQTable);
            thisAction = GetBestAction(thisState, nextState);

            ExecuteAction(thisAction);
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

        //startTime = Time.time;
        //lastRewardTime = startTime;

        GetBodyPositions();
        GetSnakeHeadPosition();
        GetFoodPosition();

        thisState = QLearningManager.Instance.QLearningInstance.GetState(headPosition, headRotation, bodyPositions, bodyRotations, foodTransform, QLearning.qTable, QLearning.states);
        nextState = QLearningManager.Instance.QLearningInstance.SimulateActionAndGetNextState(thisState, nextAction);
        //nextState = QLearningManager.Instance.QLearningInstance.GetState(headPosition, headRotation, bodyPositions, bodyRotations, foodTransform, QLearning.futureQTable, QLearning.futureStates);
    }

    SnakeAction GetBestAction(State state, State nextState)
    {
        SnakeAction bestAction = SnakeAction.DoNothing;
        float bestQValue = float.MinValue;
        float epsilon = 1f; // Exploration rate (adjust as needed)

        if (QLearning.qTable.ContainsKey(state) && QLearning.futureQTable.ContainsKey(nextState))
        {
            Dictionary<SnakeAction, float> currentQValues = QLearning.qTable[state];
            Dictionary<SnakeAction, float> futureQValues = QLearning.futureQTable[nextState];

            foreach (var action in Enum.GetValues(typeof(SnakeAction)))
            {
                SnakeAction currentAction = (SnakeAction)action;
                float currentQValue = currentQValues[currentAction];
                float futureQValue = futureQValues[currentAction];

                // Calculate action Q-value as a combination of current and future Q-values
                float actionQValue = (1 - epsilon) * currentQValue + epsilon * futureQValue;

                // Choose the action with the highest Q-value
                if (actionQValue > bestQValue)
                {
                    bestQValue = actionQValue;
                    bestAction = currentAction;
                }

                // Debugging information
                Debug.Log($"Action: {currentAction}, Current Q-value: {currentQValue}, Future Q-value: {futureQValue}, Action Q-value: {actionQValue}, Best Action: {bestAction}, Best Q-value: {bestQValue}");
            }
        }
        else
        {
            Debug.LogWarning($"Either current state ({state}) or future state ({nextState}) is not found in qTable or futureQTable.");
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

    void CalculateReward(State state, Dictionary<State, Dictionary<SnakeLearning.SnakeAction, float>> dictionary)
    {
        float distanceToFood = CalculateDistance(state.HeadPosition, state.FoodTransform.position);
        //Debug.Log(state.HeadPosition + " " + state.FoodTransform.position + " " + distanceToFood);
        float maxReward = 1f;
        float minReward = 0.0f;
        float maxDistance = 10.0f;

        float currentReward = Mathf.Lerp(maxReward, minReward, distanceToFood / maxDistance);

        // Calculate the reward based on conditions
        //float totalReward = currentReward + aliveScore;
        float totalReward = currentReward;

        // Update the Q-values for all possible actions in the next state
        foreach (SnakeAction action in Enum.GetValues(typeof(SnakeAction)))
        {
            QLearningManager.Instance.QLearningInstance.UpdateQValue(state, action, totalReward, nextState, 0.2f, 0.5f, dictionary);
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
        //reward += collisionPenalty;
    }
}