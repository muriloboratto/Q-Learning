
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public GridManager grid;
    public QLearning agent;

    [Header("Visualization")]
    public bool showGrid = true;
    public bool showAgentPositions = true;

    private void OnDrawGizmos()
    {
        if (!showGrid || grid == null)
            return;

        for (int y = 0; y < grid.height; y++)
        {
            for (int x = 0; x < grid.width; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);

                bool walkable = grid.IsWalkable(cell);

                Gizmos.color = walkable
                    ? new Color(0f, 1f, 0f, 0.3f)
                    : new Color(1f, 0f, 0f, 0.6f);

                Vector3 position = grid.GridToWorld(cell);

                Gizmos.DrawWireCube(
                    position,
                    new Vector3(
                        grid.cellSize.x * 0.95f,
                        grid.cellSize.y * 0.95f,
                        0.1f
                    )
                );
            }
        }

        if (!showAgentPositions || agent == null)
            return;

        if (agent.zombie != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(
                agent.zombie.position,
                0.25f
            );
        }

        if (agent.hero != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(
                agent.hero.position,
                0.25f
            );
        }
    }
}
