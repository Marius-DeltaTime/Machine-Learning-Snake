using UnityEngine;

public class SnakeLearning : MonoBehaviour
{
    public enum Action { DoNothing, TurnRight, TurnLeft }

    public float foodReward = 10f;
    public float survivalReward = 0.1f;
    public float collisionPenalty = -5f;

    private float startTime, lastRewardTime;
    private List<Transform> segmentTransforms = new List<Transform>();
    private Transform headTransform, foodTransform;

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
            ScoreManager.instance.IncreaseScore(survivalReward);
            lastRewardTime = Time.time;
        }

        if (SnakeController.instance.canMove)
        {
            Action nextAction = GetNextAction();
            ExecuteAction(nextAction);         
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
    /*
    Action SelectAction(State currentState)
    {
        // Choose action with highest Q-value for the current state
        Action bestAction = null;
        float maxQValue = float.MinValue;

        foreach (var action in GetPossibleActions())
        {
            float qValue = QTable[currentState][action];
            if (qValue > maxQValue)
            {
                maxQValue = qValue;
                bestAction = action;
            }
        }

        return bestAction;
    }
    */

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
        Reward(foodReward);
    }

    public void OnSurvival()
    {
        Reward(survivalReward);
    }

    public void OnCollisionWithBody()
    {
        Reward(collisionPenalty);
    }

    void Reward(float rewardValue)
    {

    }
}
