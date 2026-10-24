
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrangeEnemy : MonoBehaviour
{
    public GridSystem grid;

    void Start()
    {
        if (grid == null)
            grid = FindFirstObjectByType<GridSystem>();
    }

    public bool Hit(Vector3 pos)
    {
        if (grid == null)
            return false;

        Vector3 a = grid.GetCellPosition(0, 0);
        float size = grid.cellSize;

        int x = Mathf.RoundToInt((pos.x - a.x) / size);
        int y = Mathf.RoundToInt((pos.y - a.y) / size);

        int ex = Mathf.RoundToInt(
            (transform.position.x - a.x) / size
        );

        int ey = Mathf.RoundToInt(
            (transform.position.y - a.y) / size
        );

        return x == ex && y == ey;
    }

    public void Die()
    {
        PlayerPrefs.SetString("Level",
            SceneManager.GetActiveScene().name);

        PlayerPrefs.Save();

        SceneManager.LoadScene("Gameover");
    }
}
