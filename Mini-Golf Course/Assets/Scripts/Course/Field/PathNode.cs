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

        public bool isObstacle;

        public PathNode cameFromNode;

        public PathNode(PathGrid pathGrid, Vector3Int position, bool obstacle = false)
        {
            this.pathGrid = pathGrid;
            this.position = position;
            this.isObstacle = obstacle;
        }

        public void CalculateFCost()
        {
            fCost = gCost + hCost;

            if (isObstacle)
            {
                fCost *= 3;
            }
        }
    }
}
