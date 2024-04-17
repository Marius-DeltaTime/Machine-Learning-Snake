using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject snakeHeadPrefab; public GameObject snakeBodyPrefab; public GameObject foodPrefab;
    private GameObject snakeHead; private GameObject snakeBody; private Transform[] snakeSegments;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        Vector3 spawnPosition = Vector3.zero;
        snakeHead = Instantiate(snakeHeadPrefab, spawnPosition, Quaternion.identity);

        SpawnFood();
    }

    public void SpawnFood()
    {
        float screenWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float screenHeight = Camera.main.orthographicSize;

        float x = Random.Range(-screenWidth + 0.5f, screenWidth - 0.5f); float y = Random.Range(-screenHeight + 0.5f, screenHeight - 0.5f);
        x = Mathf.Round(x * 4) / 4; y = Mathf.Round(y * 4) / 4;
        Vector3 spawnPosition = new Vector3(x, y, 0);

        if (IsSpawnPositionValid(spawnPosition))
        {
            Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            SpawnFood();
        }
    }

    bool IsSpawnPositionValid(Vector3 spawnPosition)
    {
        if (Vector3.Distance(spawnPosition, snakeHead.transform.position) < 0.25f)
        {
            return false;
        }

        if (snakeSegments != null)
        {
            foreach (Transform segment in snakeSegments)
            {
                if (Vector3.Distance(spawnPosition, segment.position) < 0.25f)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void UpdateSnakeSegments(Transform[] segments)
    {
        snakeSegments = segments;
    }

    public void EndGame()
    {

        Debug.Log("Game Over!");


        Time.timeScale = 0f;
        Invoke("RestartGame", 2f);
        Time.timeScale = 1f;
    }

    private void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }


}
