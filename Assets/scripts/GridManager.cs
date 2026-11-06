using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 14;
    public int height = 9;
    public Vector2 cellSize = new Vector2(1.28f, 1.1f);
    public Vector2 gridOrigin = new Vector2(-8.35f, -4.39f);

    private bool[,] blocked;

    void Awake()
    {
        InitializeGrid();
    }

    void InitializeGrid()
    {
        blocked = new bool[width, height];

        // 1. Bordas marrons bloqueadas
        for (int x = 0; x < width; x++)
        {
            blocked[x, 0] = true;          
            blocked[x, height - 1] = true; 
        }

        for (int y = 0; y < height; y++)
        {
            blocked[0, y] = true;         
            blocked[width - 1, y] = true; 
        }

        //(X = coluna, Y = linha)

        blocked[6, 1] = true;
        blocked[11, 1] = true; 
        blocked[4, 2] = true;
        blocked[6, 3] = true;  
        blocked[8, 3] = true;
        blocked[2, 4] = true;  
        blocked[11, 5] = true;
        blocked[6, 6] = true; 
        blocked[9, 7] = true;          
        blocked[11, 2] = true;
    }

    public bool IsWalkable(Vector2Int position)
    {
        if (blocked == null)
        {
            InitializeGrid();
        }

        if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
        {
            return false;
        }

        return !blocked[position.x, position.y];
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt((worldPos.x - gridOrigin.x) / cellSize.x),
            Mathf.RoundToInt((worldPos.y - gridOrigin.y) / cellSize.y)
        );
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            (gridPos.x * cellSize.x) + gridOrigin.x,
            (gridPos.y * cellSize.y) + gridOrigin.y,
            0f
        );
    }

    public int GridToState(Vector2Int position)
    {
        return position.y * width + position.x;
    }

    public Vector2Int StateToGrid(int state)
    {
        return new Vector2Int(state % width, state / width);
    }

    void OnDrawGizmos()
    {
        if (blocked == null)
        {
            InitializeGrid();
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 center = GridToWorld(new Vector2Int(x, y));
                Gizmos.color = blocked[x, y] ? new Color(1f, 0f, 0f, 0.6f) : new Color(0f, 1f, 0f, 0.3f);
                Gizmos.DrawWireCube(center, new Vector3(cellSize.x * 0.95f, cellSize.y * 0.95f, 0.1f));
            }
        }
    }
}