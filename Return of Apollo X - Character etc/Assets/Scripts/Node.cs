using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GridMaster
{
    public class Node
    {
        //Node's position on the grid
        public int x;
        public int y;
        public int z;

        //Node's costs for pathfinding purposes
        public float hCost;
        public float gCost;

        public float fCost //the fCost is the sum of the hCost and gCost
        {
            get
            {
                return gCost + hCost;
            }
        }

        public Node parentNode;
        public bool isWalkable = true;

        //Reference to the world object, access the world position of the node
        public GameObject worldObject;

        //Types of Nodes that are available
        public NodeType nodeType;
        public enum NodeType
        {
            ground,
            air
        }
    }
}
