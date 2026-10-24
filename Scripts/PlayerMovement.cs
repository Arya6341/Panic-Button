
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public GridSystem grid;
    public float speed = 8f;
    public Transform[] obstacles;

    private int x = 0;
    private int y = 0;
    private bool moving = false;

    void Start()
    {
        if (grid == null)
            grid = FindFirstObjectByType<GridSystem>();

        if (grid == null)
        {
            Debug.LogError("Grid not found");
            enabled = false;
            return;
        }

        x = 0;
        y = 0;

        transform.position = grid.GetCellPosition(x, y)
            + new Vector3(0, 0, -2);
    }

    void Update()
    {
        if (moving || Keyboard.current == null)
            return;

        int dx = 0;
        int dy = 0;

        if (Keyboard.current.wKey.wasPressedThisFrame)
            dy = 1;
        else if (Keyboard.current.sKey.wasPressedThisFrame)
            dy = -1;
        else if (Keyboard.current.aKey.wasPressedThisFrame)
            dx = -1;
        else if (Keyboard.current.dKey.wasPressedThisFrame)
            dx = 1;

        if (dx == 0 && dy == 0)
            return;

        int nx = x + dx;
        int ny = y + dy;

        if (!grid.IsInsideGrid(nx, ny))
            return;

        Vector3 pos = grid.GetCellPosition(nx, ny);

        // White obstacles
        foreach (Transform obstacle in obstacles)
        {
            if (obstacle == null || !obstacle.gameObject.activeInHierarchy)
                continue;

            BoxCollider2D box = obstacle.GetComponent<BoxCollider2D>();

            if (box == null || !box.enabled)
                continue;

            Bounds b = box.bounds;

            if (pos.x >= b.min.x && pos.x <= b.max.x &&
                pos.y >= b.min.y && pos.y <= b.max.y)
                return;
        }

        // Blue enemies
        BlueEnemy[] blue =
            FindObjectsByType<BlueEnemy>(FindObjectsSortMode.None);

        foreach (BlueEnemy enemy in blue)
        {
            if (!enemy.isActiveAndEnabled)
                continue;

            if (Vector2.Distance(pos, enemy.transform.position)
                < grid.cellSize * 0.75f)
                return;
        }

        // Orange enemies
        OrangeEnemy[] orange =
            FindObjectsByType<OrangeEnemy>(FindObjectsSortMode.None);

        foreach (OrangeEnemy enemy in orange)
        {
            if (enemy.isActiveAndEnabled && enemy.Hit(pos))
            {
                enemy.Die();
                return;
            }
        }

        x = nx;
        y = ny;

        StartCoroutine(Move());
    }

    IEnumerator Move()
    {
        moving = true;

        Vector3 target = grid.GetCellPosition(x, y)
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
