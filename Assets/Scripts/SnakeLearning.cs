using UnityEngine;
using System.Collections.Generic;

public class SnakeLearning : MonoBehaviour
{
    private float foodReward = 10f;
    private float survivalReward = 0.1f;
    private float collisionPenalty = -10f;
    private SnakeController snakeController;
    private QLearning qLearning;
    private State currentState;
    private bool isLearningActive = true;

    void Start()
    {
        snakeController = GetComponent<SnakeController>();
        qLearning = new QLearning();
        qLearning.InitializeQTable();
        currentState = qLearning.GetState(snakeController.snakeHead.position, GetBodyPositions(), snakeController.food.position);
    }

    void Update()
    {
        if (isLearningActive)
        {
            SnakeAction action = GetBestAction(currentState);
            ExecuteAction(action);
            State nextState = qLearning.SimulateActionAndGetNextState(currentState, action);
            float reward = CalculateReward(nextState);
            qLearning.UpdateQValue(currentState, action, reward, nextState);
            currentState = nextState;
        }
    }

    SnakeAction GetBestAction(State state)
    {
        return qLearning.GetBestAction(state);
    }

    void ExecuteAction(SnakeAction action)
    {
        switch (action)
        {
            case SnakeAction.TurnRight:
                snakeController.RotateSnakeClockwise();
                break;
            case SnakeAction.TurnLeft:
                snakeController.RotateSnakeCounterClockwise();
                break;
            default:
                break;
        }

        snakeController.TriggerMove();
    }

    float CalculateReward(State nextState)
    {
        //if (snakeController.DidEatFood()) return foodReward;
        //if (snakeController.DidCollide()) return collisionPenalty;
        return survivalReward;
    }

    List<Vector3> GetBodyPositions()
    {
        List<Vector3> bodyPositions = new List<Vector3>();
        foreach (var segment in snakeController.snakeSegments)
        {
            bodyPositions.Add(segment.position);
        }
        return bodyPositions;
    }

    public void StopLearning()
    {
        isLearningActive = false;
    }

    public void StartLearning()
    {
        isLearningActive = true;
    }
}
