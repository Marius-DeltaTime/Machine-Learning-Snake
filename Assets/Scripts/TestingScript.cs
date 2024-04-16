using UnityEngine;

public class TestingScript : MonoBehaviour
{
    public GameObject pinkSquarePrefab;
    public GameObject orangeSquarePrefab;

    void Start()
    {
        Vector3 randomPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0f);
        GameObject pinkSquare = Instantiate(pinkSquarePrefab, randomPosition, Quaternion.identity);

        Bounds pinkBounds = GetBounds(pinkSquare);

        Vector3 orangePosition = pinkSquare.transform.position - new Vector3(0f, pinkBounds.extents.y * 2f, 0f);
        GameObject orangeSquare = Instantiate(orangeSquarePrefab, orangePosition, Quaternion.identity);
    }

    Bounds GetBounds(GameObject obj)
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
