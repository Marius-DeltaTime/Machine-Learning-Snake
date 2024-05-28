using UnityEngine;
using System.Collections.Generic;

public class SnakeController : MonoBehaviour
{
    public Transform snakeHead;
    public GameObject snakeSegmentPrefab;
    public Transform food;
    public List<Transform> snakeSegments = new List<Transform>();
    private Vector2 direction = Vector2.right;
    private bool moveFlag = false;

    void Start()
    {
        snakeSegments.Add (snakeHead);
        AddSegment();
        AddSegment();
        AddSegment();
    }

    void Update()
    {
        HandleInput();
        if (moveFlag) MoveSnake();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && direction != Vector2.down) direction = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow) && direction != Vector2.up) direction = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && direction != Vector2.right) direction = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow) && direction != Vector2.left) direction = Vector2.right;
    }

    public void RotateSnakeClockwise()
    {
        RotateSnake(-90f);
    }

    public void RotateSnakeCounterClockwise()
    {
        RotateSnake(90f);
    }

    void RotateSnake(float angle)
    {
        direction = Quaternion.Euler(0, 0, angle) * direction;
        transform.Rotate(Vector3.forward, angle);
    }

    void MoveSnake()
    {
        Vector3 prevPosition = snakeHead.position;
        snakeHead.Translate(direction);

        for (int i = 1; i < snakeSegments.Count; i++)
        {
            Vector3 tempPosition = snakeSegments[i].position;
            snakeSegments[i].position = prevPosition;
            prevPosition = tempPosition;
        }

        moveFlag = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Food")
        {
            Destroy(other.gameObject);
            AddSegment();
            UpdateFoodPosition();       
            GameManager.instance.SpawnFood();
            ScoreManager.instance.IncreaseScore(10);
        }
    }

    void AddSegment()
    {
        Transform newSegment = Instantiate(snakeSegmentPrefab).transform;
        newSegment.position = snakeSegments[snakeSegments.Count - 1].position;
        snakeSegments.Add(newSegment);
    }

    void UpdateFoodPosition()
    {
        food.position = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 0);
    }

    public void TriggerMove() => moveFlag = true;
}
