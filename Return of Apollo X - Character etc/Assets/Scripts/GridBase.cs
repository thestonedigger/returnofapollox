using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GridMaster
{
    public class GridBase : MonoBehaviour
    {
        //Setting up the grid
        public int maxX = 10;
        public int maxY = 3;
        public int maxZ = 10;

        //Offset relates to the world positions
        public float offsetX = 1;
        public float offsetY = 1;
        public float offsetZ = 1;

        public Node[, ,] grid; //Our grid

        public GameObject gridFloorPrefab;

        public Vector3 startNodePosition;
        public Vector3 endNodePosition;

        public int agents;

        void Start()
        {
            grid = new Node[maxX, maxY, maxZ];

            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    for (int z = 0; z < maxZ; z++)
                    {
                        //Apply the offsets and the world object for each node
                        float posX = x * offsetX;
                        float posY = y * offsetY;
                        float posZ = z * offsetZ;
                        GameObject go = Instantiate(gridFloorPrefab, new Vector3(posX, posY, posZ), Quaternion.identity) as GameObject;
                        //Renaming "go"
                        go.transform.name = x.ToString() + " " + y.ToString() + " " + z.ToString();
                        //Parent to this transform, organisation
                        go.transform.parent = transform;                    

                        //Create new node and update its values
                        Node node = new Node();
                        node.x = x;
                        node.y = y;
                        node.z = z;
                        node.worldObject = go;

                        RaycastHit[] hits = Physics.BoxCastAll(new Vector3(posX, posY, posZ), new Vector3 (0,0,1), Vector3.forward);

                        for (int i = 0; i < hits.Length; i++)
                        {
                                node.isWalkable = false;  
                        }

                        grid[x, y, z] = node;
                    }
                }
            }
        }

        //quick way to visualise the path, basically a test
        public bool start;
        private void Update()
        {
            if (start)
            {
                start = false;
                //create new pathfinder class
                //Pathfinding.Pathfinder path = new Pathfinding.Pathfinder();

                //to test avoidance, just makes the node unwalkable
                grid[1, 0, 1].isWalkable = false;

                //pass the target nodes
                Node startNode = GetNodeFromVector3(startNodePosition);
                Node endNode = GetNodeFromVector3(endNodePosition);

                //path.startPosition = startNode;
                //path.endPosition = endNode;

                //find the path
                //List<Node> p = path.FindPath();

                //disable world object for each node we're going to pass from
                startNode.worldObject.SetActive(false);

                for (int i = 0; i < agents; i++)
                {
                    Pathfinding.PathfinderMaster.GetInstance().RequestPathFind(startNode, endNode, ShowPath);
                }
            }
        }

        public void ShowPath(List<Node> path)
        {
            foreach(Node n in path)
            {
                n.worldObject.SetActive(false);
            }

            Debug.Log("agent complete");
        }

        public Node GetNode(int x, int y, int z)
        {
            //Used to get a node from a grid
            //If it's greater than all the maximum values assigned, then it will return null

            Node returnValue = null;

            if(x < maxX && x >= 0 && 
               y >= 0 && y < maxY && 
               z >= 0 && z < maxZ)
            {
                returnValue = grid[x, y, z];
            }

            return returnValue;
        }

        public Node GetNodeFromVector3(Vector3 pos)
        {
            int x = Mathf.RoundToInt(pos.x);
            int y = Mathf.RoundToInt(pos.y);
            int z = Mathf.RoundToInt(pos.z);

            Node returnValue = GetNode(x, y, z);
            return returnValue;
        }

        //Singleton
        public static GridBase instance;
        public static GridBase GetInstance()
        {
            return instance;
        }

        private void Awake()
        {
            instance = this;
        }
    }
}
