using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public float rotationAngle = 90f;
    public GameObject foodPrefab;
    public GameObject segmentPrefab;
    public float distanceFromHead;

    private Vector2 moveDirection = Vector2.up;
    private List<Transform> segments = new List<Transform>();
    private Quaternion headRotation = Quaternion.identity;
    private bool canMove = true;

    public float moveDelay = 0.1f;

    private void Start()
    {
        AddSegment();
        AddSegment();
        AddSegment();
    }

    private void Update()
    {
        if (canMove)
        {
            MoveSnake();
        }

        HandleInput();
        RotateSnakeBody();
    }

    private void MoveSnake()
    {
        Vector3 newPosition = transform.position + (Vector3)moveDirection * 0.25f;
        transform.position = newPosition;

        Vector3 position = transform.position;
        float screenWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float screenHeight = Camera.main.orthographicSize;

        if (position.x > screenWidth)
        {
            position.x = -screenWidth;
        }
        else if (position.x < -screenWidth)
        {
            position.x = screenWidth;
        }

        if (position.y > screenHeight)
        {
            position.y = -screenHeight;
        }
        else if (position.y < -screenHeight)
        {
            position.y = screenHeight;
        }

        transform.position = position;

        for (int i = segments.Count - 1; i > 0; i--)
        {
            segments[i].position = segments[i - 1].position;
        }

        if (segments.Count > 0)
        {
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

            segments[0].position = transform.position + offset;
        }

        canMove = false;
        Invoke("EnableMovement", moveDelay);
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            RotateSnakeClockwise();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RotateSnakeCounterClockwise();
        }
    }

    private void EnableMovement()
    {
        canMove = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Snake_Body"))
        {
            Debug.Log("Game Over: Head collided with body segment!");
        }
        else if (other.CompareTag("Food"))
        {
            Destroy(other.gameObject);
            GameManager.instance.SpawnFood();
            ScoreManager.instance.IncreaseScore(10);
            AddSegment();
        }
    }

    private void RotateSnakeClockwise()
    {
        headRotation *= Quaternion.Euler(0, 0, -rotationAngle);
        transform.Rotate(Vector3.forward, -rotationAngle);
        moveDirection = Quaternion.Euler(0, 0, -rotationAngle) * moveDirection;
    }

    private void RotateSnakeCounterClockwise()
    {
        headRotation *= Quaternion.Euler(0, 0, rotationAngle);
        transform.Rotate(Vector3.forward, rotationAngle);
        moveDirection = Quaternion.Euler(0, 0, rotationAngle) * moveDirection;
    }

    private void RotateSnakeBody()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].rotation = headRotation;
        }
    }

    private void AddSegment()
    {
        GameObject newSegment = Instantiate(segmentPrefab);

        if (segments.Count > 0)
        {
            Vector3 newPosition = segments[segments.Count - 1].position;
            Quaternion newRotation = segments[segments.Count - 1].rotation;
            newSegment.transform.position = newPosition;
            newSegment.transform.rotation = newRotation;
        }
        else
        {
            newSegment.transform.position = transform.position;
            newSegment.transform.rotation = transform.rotation;
        }

        newSegment.transform.up = -transform.up;
        segments.Add(newSegment.transform);
    }

    private Bounds GetBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }
        else
        {
            Collider collider = obj.GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds;
            }
        }
        return new Bounds();
    }
}
