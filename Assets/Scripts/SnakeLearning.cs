using UnityEngine;
using System.Collections.Generic;

public class SnakeLearning : MonoBehaviour
{
    public static SnakeLearning instance;

    public enum Action { DoNothing, TurnRight, TurnLeft }

    public float foodReward = 10f;
    public float survivalReward = 0.1f;
    public float collisionPenalty = -5f;

    private float startTime, lastRewardTime;
    private List<Transform> segmentTransforms = new List<Transform>();
    private Transform headTransform, foodTransform;
    private float reward;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
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

    void Start()
    {
        startTime = Time.time;
        lastRewardTime = startTime;
    }

    void Update()
    {
        float timeElapsed = Time.time - startTime;
        float timeSinceLastReward = Time.time - lastRewardTime;

        if (timeSinceLastReward >= 1f)
        {
            OnSurvival();
            lastRewardTime = Time.time;
        }

        if (SnakeController.instance.canMove)
        {
            Action nextAction = GetNextAction();
            ExecuteAction(nextAction);
            GetSegmentPositions();
            GetSnakeHeadPosition();

            QLearning.instance.AddState(headTransform, segmentTransforms, foodTransform);
        }
    }

    Action GetNextAction()
    {
        float randomValue = Random.value;

        if (randomValue < 0.33f)
        {
            return Action.DoNothing;
        }
        else if (randomValue < 0.66f)
        {
            return Action.TurnRight;
        }
        else
        {
            return Action.TurnLeft;
        }
    }

    void ExecuteAction(Action action)
    {
        switch (action)
        {
            case Action.DoNothing:
                break;
            case Action.TurnRight:
                SnakeController.instance.RotateSnakeClockwise();
                break;
            case Action.TurnLeft:
                SnakeController.instance.RotateSnakeCounterClockwise();
                break;
        }
    }

    public void OnFoodEaten()
    {
        GetFoodPosition();
        reward += foodReward;
        ScoreManager.instance.IncreaseScore(reward);
    }

    public void OnSurvival()
    {
        reward += survivalReward;
        ScoreManager.instance.IncreaseScore(reward);
    }

    public void OnCollisionWithBody()
    {
        reward += collisionPenalty;
    }
}