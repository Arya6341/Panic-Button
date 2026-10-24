
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class BlueEnemy : MonoBehaviour
{
    public GridSystem grid;
    public Transform player;
    public Transform finish;
    public LayerMask obstacles;
    public Transform[] dangers;
    public float speed = 5f;

    private Moves moves;
    private bool moving;
    private Collider2D col;

    private readonly Vector2Int[] dirs =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

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

        if (finish == null)
        {
            GameObject f = GameObject.Find("Finish");
            if (f != null)
                finish = f.transform;
        }

        moves = FindFirstObjectByType<Moves>();
        col = GetComponent<Collider2D>();

        if (grid == null || player == null || finish == null ||
            col == null || moves == null)
        {
            Debug.LogError("Blue enemy is missing a reference.");
            enabled = false;
            return;
        }

        Vector2Int cell = GetCell(transform.position);
        transform.position = grid.GetCellPosition(cell.x, cell.y)
            + Vector3.back * 2f;
    }

    void Update()
    {
        if (moving || Mouse.current == null || Camera.main == null)
            return;

        if (!moves.Available())
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mouse = Mouse.current.position.ReadValue();

        Vector3 point = Camera.main.ScreenToWorldPoint(
            new Vector3(mouse.x, mouse.y,
                -Camera.main.transform.position.z)
        );

        if (col.OverlapPoint(new Vector2(point.x, point.y)))
            Move();
    }

    Vector2Int GetCell(Vector3 pos)
    {
        Vector2Int cell = Vector2Int.zero;
        float closest = float.MaxValue;

        for (int x = 0; x < grid.columns; x++)
        {
            for (int y = 0; y < grid.rows; y++)
            {
                Vector3 p = grid.GetCellPosition(x, y);

                float d = (
                    new Vector2(p.x, p.y) -
                    new Vector2(pos.x, pos.y)
                ).sqrMagnitude;

                if (d < closest)
                {
                    closest = d;
                    cell = new Vector2Int(x, y);
                }
            }
        }

        return cell;
    }

    bool Blocked(Vector2Int cell, Vector2Int blue)
    {
        if (!grid.IsInsideGrid(cell.x, cell.y))
            return true;

        if (cell == blue)
            return true;

        Vector3 pos = grid.GetCellPosition(cell.x, cell.y);

        BoxCollider2D[] boxes =
            FindObjectsByType<BoxCollider2D>(
                FindObjectsSortMode.None);

        foreach (BoxCollider2D box in boxes)
        {
            if (!box.enabled || !box.gameObject.activeInHierarchy)
                continue;

            if ((obstacles.value & (1 << box.gameObject.layer)) == 0)
                continue;

            Bounds b = box.bounds;

            if (pos.x >= b.min.x && pos.x <= b.max.x &&
                pos.y >= b.min.y && pos.y <= b.max.y)
                return true;
        }

        foreach (Transform danger in dangers)
        {
            if (danger == null || !danger.gameObject.activeInHierarchy)
                continue;

            if (cell == GetCell(danger.position))
                return true;
        }

        BlueEnemy[] enemies =
            FindObjectsByType<BlueEnemy>(
                FindObjectsSortMode.None);

        foreach (BlueEnemy enemy in enemies)
        {
            if (enemy == this || !enemy.isActiveAndEnabled)
                continue;

            if (cell == GetCell(enemy.transform.position))
                return true;
        }

        return false;
    }

    int Path(Vector2Int start, Vector2Int goal, Vector2Int blue)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> steps =
            new Dictionary<Vector2Int, int>();

        if (Blocked(start, blue) || Blocked(goal, blue))
            return -1;

        queue.Enqueue(start);
        steps[start] = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == goal)
                return steps[current];

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int next = current + dir;

                if (steps.ContainsKey(next) || Blocked(next, blue))
                    continue;

                steps[next] = steps[current] + 1;
                queue.Enqueue(next);
            }
        }

        return -1;
    }

    void Move()
    {
        if (!moves.Available())
            return;

        Vector2Int current = GetCell(transform.position);
        Vector2Int p = GetCell(player.position);
        Vector2Int goal = GetCell(finish.position);

        Vector2Int best = current;

        int bestPath = int.MaxValue;
        int bestDistance = int.MinValue;
        bool foundPath = false;

        foreach (Vector2Int dir in dirs)
        {
            Vector2Int next = current + dir;

            if (Blocked(next, current) || next == p || next == goal)
                continue;

            int path = Path(p, goal, next);

            if (path < 0)
                continue;

            int distance = Mathf.Abs(next.x - p.x) +
                           Mathf.Abs(next.y - p.y);

            if (!foundPath ||
                path < bestPath ||
                (path == bestPath && distance > bestDistance))
            {
                best = next;
                bestPath = path;
                bestDistance = distance;
                foundPath = true;
            }
        }

        if (!foundPath)
        {
            bestDistance = int.MinValue;

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int next = current + dir;

                if (Blocked(next, current) || next == p || next == goal)
                    continue;

                int distance = Mathf.Abs(next.x - p.x) +
                               Mathf.Abs(next.y - p.y);

                if (distance > bestDistance)
                {
                    best = next;
                    bestDistance = distance;
                }
            }
        }

        if (best == current)
            return;

        moves.Use();
        StartCoroutine(Jump(best));
    }

    IEnumerator Jump(Vector2Int cell)
    {
        moving = true;

        Vector3 target = grid.GetCellPosition(cell.x, cell.y)
            + Vector3.back * 2f;

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = target;
        moving = false;
    }
}
