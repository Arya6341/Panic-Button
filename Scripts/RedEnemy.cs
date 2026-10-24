
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class RedEnemy : MonoBehaviour
{
    public GridSystem grid;
    public Transform player;
    public LayerMask obstacles;

    public float delay = 3f;
    public float time = 0.8f;

    private Vector2Int cell;
    private bool chasing;

    private readonly Vector2Int[] dirs =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    IEnumerator Start()
    {
        if (grid == null)
            grid = FindFirstObjectByType<GridSystem>();

        if (player == null)
        {
            PlayerMovement p = FindFirstObjectByType<PlayerMovement>();

            if (p != null)
                player = p.transform;
        }

        if (grid == null || player == null)
        {
            Debug.LogError("Red enemy is missing Grid or Player");
            yield break;
        }

        cell = GetCell(transform.position);
        transform.position = Position(cell);

        yield return new WaitForSeconds(delay);

        chasing = true;

        while (chasing)
        {
            Vector2Int next = NextCell();

            if (next != cell)
            {
                cell = next;
                transform.position = Position(cell);
            }

            CheckPlayer();

            yield return new WaitForSeconds(time);
        }
    }

    Vector3 Position(Vector2Int c)
    {
        return grid.GetCellPosition(c.x, c.y) + Vector3.back * 2f;
    }

    Vector2Int GetCell(Vector3 pos)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;

        for (int x = 0; x < grid.columns; x++)
        {
            for (int y = 0; y < grid.rows; y++)
            {
                Vector3 p = grid.GetCellPosition(x, y);

                float d = Vector2.Distance(pos, p);

                if (d < distance)
                {
                    distance = d;
                    closest = new Vector2Int(x, y);
                }
            }
        }

        return closest;
    }

    bool Free(Vector2Int c)
    {
        if (!grid.IsInsideGrid(c.x, c.y))
            return false;

        Vector3 pos = Position(c);

        BoxCollider2D[] boxes =
            FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None);

        foreach (BoxCollider2D box in boxes)
        {
            if (!box.enabled || !box.gameObject.activeInHierarchy)
                continue;

            if ((obstacles.value & (1 << box.gameObject.layer)) == 0)
                continue;

            Bounds b = box.bounds;

            if (pos.x >= b.min.x && pos.x <= b.max.x &&
                pos.y >= b.min.y && pos.y <= b.max.y)
                return false;
        }

        BlueEnemy[] blue =
            FindObjectsByType<BlueEnemy>(FindObjectsSortMode.None);

        foreach (BlueEnemy enemy in blue)
        {
            if (enemy.isActiveAndEnabled &&
                c == GetCell(enemy.transform.position))
                return false;
        }

        OrangeEnemy[] orange =
            FindObjectsByType<OrangeEnemy>(FindObjectsSortMode.None);

        foreach (OrangeEnemy enemy in orange)
        {
            if (enemy.isActiveAndEnabled &&
                c == GetCell(enemy.transform.position))
                return false;
        }

        return true;
    }

    Vector2Int NextCell()
    {
        Vector2Int goal = GetCell(player.position);

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> previous =
            new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(cell);
        previous[cell] = cell;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == goal)
                break;

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int next = current + dir;

                if (previous.ContainsKey(next) || !Free(next))
                    continue;

                previous[next] = current;
                queue.Enqueue(next);
            }
        }

        if (!previous.ContainsKey(goal))
            return cell;

        Vector2Int step = goal;

        while (previous[step] != cell && step != cell)
            step = previous[step];

        return step;
    }

    void CheckPlayer()
    {
        if (cell == GetCell(player.position))
        {
            chasing = false;

            PlayerPrefs.SetString(
                "Level",
                SceneManager.GetActiveScene().name
            );

            PlayerPrefs.Save();

            SceneManager.LoadScene("Gameover");
        }
    }
}
