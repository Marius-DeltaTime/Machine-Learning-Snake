using System.Collections.Generic;
using UnityEngine;

public class SnakeBot : MonoBehaviour
{
    public static SnakeBot instance;
    private List<Transform> segmentTransforms = new List<Transform>();
    private Transform headTransform, foodTransform;
    private bool hasDecided = false;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        GameObject snakeHead = GameObject.FindGameObjectWithTag("Snake_Head");
        if (snakeHead != null)
        {
            headTransform = snakeHead.transform;
        }

        GameObject food = GameObject.FindGameObjectWithTag("Food");
        if (food != null)
        {
            foodTransform = food.transform;
        }

        GameObject[] snakeBodySegments = GameObject.FindGameObjectsWithTag("Snake_Body");
        foreach (GameObject segment in snakeBodySegments)
        {
            segmentTransforms.Add(segment.transform);
        }

        UpdateSnakeHeadAndFood();
    }

    public void UpdateSnakeHeadAndFood()
    {
        GameObject snakeHead = GameObject.FindGameObjectWithTag("Snake_Head");
        if (snakeHead != null)
        {
            headTransform = snakeHead.transform;
        }

        GameObject food = GameObject.FindGameObjectWithTag("Food");
        if (food != null)
        {
            foodTransform = food.transform;
        }

    }

    void DecideNextMove()
    {
        GameObject snakeHead = GameObject.FindGameObjectWithTag("Snake_Head");
        if (snakeHead != null)
        {
            headTransform = snakeHead.transform;
        }

        foreach (Transform segment in segmentTransforms)
        {
            Vector3 segmentDirection = segment.position - headTransform.position;
            float distance = segmentDirection.magnitude;

            float angle = Vector3.Angle(headTransform.up, segmentDirection.normalized);

            if (angle <= 135f)
            {
                if (distance < 0.25f)
                {
                    float dotRight = Vector3.Dot(segmentDirection.normalized, headTransform.right);
                    if (dotRight > 0)
                    {
                        SnakeController.instance.RotateSnakeClockwise();
                    }
                    else
                    {
                        SnakeController.instance.RotateSnakeCounterClockwise();
                    }

                    return;
                }

                if (Vector3.Cross(headTransform.up, segmentDirection.normalized).z < 0)
                {
                    SnakeController.instance.RotateSnakeCounterClockwise();
                    return;
                }

                if (Vector3.Cross(headTransform.up, segmentDirection.normalized).z > 0)
                {
                    SnakeController.instance.RotateSnakeClockwise();
                    return;
                }
            }
        }

        if (foodTransform != null)
        {
            Vector3 foodDirection = foodTransform.position - headTransform.position;
            float dotFoodForward = Vector3.Dot(foodDirection.normalized, headTransform.up);

            if (dotFoodForward > 0.9f)
            {
                return;
            }
            else
            {
                float dotRight = Vector3.Dot(foodDirection.normalized, headTransform.right);
                if (dotRight > 0)
                {
                    SnakeController.instance.RotateSnakeClockwise();
                }
                else
                {
                    SnakeController.instance.RotateSnakeCounterClockwise();
                }
            }
        }
    }

    void Update()
    {
        Invoke("DoThis", 0.001f);
    }

    void DoThis()
    {
        if (SnakeController.instance.canMove)
        {
            if (!hasDecided)
            {
                DecideNextMove();
                hasDecided = true;
            }
        }
        else
        {
            hasDecided = false;
        }
    }
}
