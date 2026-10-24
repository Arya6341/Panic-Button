
using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    public GridSystem grid;
    public Transform player;

    private bool completed;

    void Start()
    {
        if (grid == null)
            grid = FindFirstObjectByType<GridSystem>();

        if (player == null)
        {
            PlayerMovement p = FindFirstObjectByType<PlayerMovement>();

            if (p != null)
                player = p.transform;
        }
    }

    void Update()
    {
        if (completed || grid == null || player == null)
            return;

        Vector2 a = transform.position;
        Vector2 b = player.position;

        if (Vector2.Distance(a, b) < grid.cellSize * 0.3f)
        {
            completed = true;

            PlayerPrefs.SetString(
                "Level",
                SceneManager.GetActiveScene().name
            );

            PlayerPrefs.Save();

            SceneManager.LoadScene("LevelComplete");
        }
    }
}
