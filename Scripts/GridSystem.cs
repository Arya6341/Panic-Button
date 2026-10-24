
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public int columns = 12;
    public int rows = 8;
    public float cellSize = 1f;
    public float gap = 0.04f;

    public Material tileMaterial;

    void Start()
    {
        CreateGrid();
    }

    [ContextMenu("Create Grid")]
    public void CreateGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name.StartsWith("Tile_"))
            {
                child.SetActive(false);

                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }

        if (tileMaterial == null)
        {
            Debug.LogError("Assign GridMaterial in the Inspector!");
            return;
        }

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject tile = GameObject.CreatePrimitive(
                    PrimitiveType.Quad
                );

                tile.name = "Tile_" + x + "_" + y;
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = GetLocalPosition(x, y);

                float size = cellSize - gap;
                tile.transform.localScale =
                    new Vector3(size, size, 1f);

                Collider box = tile.GetComponent<Collider>();

                if (box != null)
                {
                    if (Application.isPlaying)
                        Destroy(box);
                    else
                        DestroyImmediate(box);
                }

                MeshRenderer sr = tile.GetComponent<MeshRenderer>();
                sr.sharedMaterial = tileMaterial;
            }
        }
    }

    private Vector3 GetLocalPosition(int x, int y)
    {
        return new Vector3(
            (x - (columns - 1) / 2f) * cellSize,
            (y - (rows - 1) / 2f) * cellSize,
            0
        );
    }

    public Vector3 GetCellPosition(int x, int y)
    {
        return transform.TransformPoint(GetLocalPosition(x, y));
    }

    public bool IsInsideGrid(int x, int y)
    {
        return x >= 0 && x < columns &&
               y >= 0 && y < rows;
    }
}
