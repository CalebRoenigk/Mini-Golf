using System.Collections.Generic;
using UnityEngine;

namespace Course.Field
{
    public class PathGrid
    {
        public List<PathNode> gridNodes;
        public BoundsInt bounds;

        public PathGrid(BoundsInt bounds, Dictionary<int, List<Vector3Int>> obstacles)
        {
            this.gridNodes = new List<PathNode>();
            this.bounds = bounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0);
                    gridNodes.Add(new PathNode(this, position));
                }
            }
            
            // Iterate over each modifier cost in the obstacles dictionary
            foreach (KeyValuePair<int, List<Vector3Int>> obstacleList in obstacles)
            {
                int modifierValue = obstacleList.Key;
                // For each point in this modifier group, set the modifier cost of this pathnode
                foreach (Vector3Int obstacle in obstacleList.Value)
                {
                    PathNode obstacleNode = GetNode(obstacle);
                    obstacleNode.SetModifierCost(modifierValue);
                }
            }
        }

        // Gets a node from the grid
        public PathNode GetNode(Vector3Int node)
        {
            return gridNodes.Find(p => p.position == node);
        }

        // Returns the size of the grid
        public Vector3Int GetSize()
        {
            return bounds.size;
        }

        // Returns a bool if the point is within the grid
        public bool Contains(Vector3Int point)
        {
            return bounds.Contains(point);
        }
    }
}
