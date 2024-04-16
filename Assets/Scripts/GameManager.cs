using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject snakeHeadPrefab;
    public GameObject snakeBodyPrefab;
    public GameObject foodPrefab;

    private GameObject snakeHead;
    private GameObject snakeBody;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        Vector3 spawnPosition = Vector3.zero;
        snakeHead = Instantiate(snakeHeadPrefab, spawnPosition, Quaternion.identity);

        SpawnFood();
    }

    public void SpawnFood()
    {
        float screenWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float screenHeight = Camera.main.orthographicSize;

        float x = Random.Range(-screenWidth + 0.5f, screenWidth - 0.5f);
        float y = Random.Range(-screenHeight + 0.5f, screenHeight - 0.5f);

        x = Mathf.Round(x * 4) / 4;
        y = Mathf.Round(y * 4) / 4;

        Vector3 spawnPosition = new Vector3(x, y, 0);

        Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
    }
}
