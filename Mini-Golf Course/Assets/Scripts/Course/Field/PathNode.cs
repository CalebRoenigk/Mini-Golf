using UnityEngine;

namespace Course.Field
{
    public class PathNode
    {
        public PathGrid pathGrid;
        public Vector3Int position;

        public int gCost;
        public int hCost;
        public int fCost;

        public int obstacleModifier;

        public PathNode cameFromNode;

        public PathNode(PathGrid pathGrid, Vector3Int position, int obstacleModifier = 1)
        {
            this.pathGrid = pathGrid;
            this.position = position;
            this.obstacleModifier = obstacleModifier;
        }

        public void SetModifierCost(int cost)
        {
            this.obstacleModifier = cost;
        }

        public void CalculateFCost()
        {
            fCost = (gCost + hCost) * obstacleModifier;
        }
    }
}
