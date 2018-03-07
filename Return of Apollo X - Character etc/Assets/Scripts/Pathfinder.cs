using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridMaster;

namespace Pathfinding
{
    public class Pathfinder
    {
        GridBase gridBase;
        public Node startPosition;
        public Node endPosition;

        public volatile bool jobDone = false;
        PathfinderMaster.PathFindingJobComplete completeCallback;
        List<Node> foundPath;

        //Constructor
        public Pathfinder(Node start, Node target, PathfinderMaster.PathFindingJobComplete callback)
        {
            startPosition = start;
            endPosition = target;
            completeCallback = callback;
            gridBase = GridBase.GetInstance();
        }

        public void FindPath()
        {
            foundPath = FindPathActual(startPosition, endPosition);

            jobDone = true;
        }

        public void NotifyComplete()
        {
            if(completeCallback != null)
            {
                completeCallback(foundPath);
            }
        }

        private List<Node> FindPathActual(Node start, Node target)
        {
            //A* algorithm

            List<Node> foundPath = new List<Node>();

            //Two lists, one for the nodes that needs to be checked and one for the ones that already are checked
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            //Adding to open set
            openSet.Add(start);

            while(openSet.Count > 0)
            {
                Node currentNode = openSet[0];

                for (int i = 0; i < openSet.Count; i++)
                {
                    //Check cost for current node
                    if (openSet[i].fCost < currentNode.fCost ||
                        (openSet[i].fCost == currentNode.fCost &&
                        openSet[i].hCost < currentNode.hCost))
                    {
                        //assign new current node
                        if (!currentNode.Equals(openSet[i]))
                        {
                            currentNode = openSet[i];
                        }
                    }
                }

                //remove the current node from the open set and add to the closed set
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                //if the current node is the target node
                if (currentNode.Equals(target))
                {
                    //reached destination, retrace path
                    foundPath = RetracePath(start, currentNode);
                    break;
                }

                //if target isn't reached, look at neighbours
                foreach(Node neighbour in GetNeighbours(currentNode, true))
                {
                    if (!closedSet.Contains(neighbour))
                    {
                        //create movement cost for neighbours
                        float newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                        //if lower than neighbour cost
                        if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            //calculate new cost
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, target);
                            //assign the parent node
                            neighbour.parentNode = currentNode;
                            //add neighbour node to open set
                            if (!openSet.Contains(neighbour))
                            {
                                openSet.Add(neighbour);
                            }
                        }
                    }
                }
            }

            //return the path in the end
            return foundPath;

        }

        private List<Node> RetracePath(Node startNode, Node endNode)
        {
            //Retrace the path, from the endNode to the startNode
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                //by taking parent node assigned
                currentNode = currentNode.parentNode;
            }

            //reverse list
            path.Reverse();
            return path;
        }
        
        private List<Node> GetNeighbours(Node node, bool getVerticalNeighbours = false)
        {
            //start taking neighbours
            List<Node> returnList = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int yIndex = -1; yIndex <= 1; yIndex++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        int y = yIndex;

                        //If we don't want a 3D y, set value to zero
                        if (!getVerticalNeighbours)
                        {
                            y = 0;
                        }

                        if(x == 0 && y == 0 && z == 0)
                        {
                            //0, 0, 0 is the current node
                        }
                        else
                        {
                            Node searchPos = new Node();

                            //nodes we want are forward, backwards and on the sides from us
                            searchPos.x = node.x + x;
                            searchPos.y = node.y + y;
                            searchPos.z = node.z + z;

                            Node newNode = GetNeighbourNode(searchPos, true, node);

                            if(newNode != null)
                            {
                                returnList.Add(newNode);
                            }
                        }
                    }
                }
            }

            return returnList;
        }

        private Node GetNeighbourNode(Node adjPos, bool searchTopDown, Node currentNodePos)
        {
            Node returnValue = null;

            //Node from the adjacent positions passed
            Node node = GetNode(adjPos.x, adjPos.y, adjPos.z);

            //If it's not null, we can walk on it
            if(node != null && node.isWalkable)
            {
                //we can use that node
                returnValue = node;
            }
            else if (searchTopDown)
            {
                //look at adjacent node under
                adjPos.y -= 1;
                Node bottomBlock = GetNode(adjPos.x, adjPos.y, adjPos.z);

                //if there is a bottom block we can walk on it
                if(bottomBlock != null && bottomBlock.isWalkable)
                {
                    returnValue = bottomBlock; //return
                }
                else
                {
                    //otherwise look at what's on top
                    adjPos.y += 2;
                    Node topBlock = GetNode(adjPos.x, adjPos.y, adjPos.z);
                    if(topBlock != null && topBlock.isWalkable)
                    {
                        returnValue = topBlock;
                    }
                }
            }

            //If the node is diagonal to the current node, check the neighbouring nodes
            //To move diagonally, we need to have 4 nodes available
            int originalX = adjPos.x - currentNodePos.x;
            int originalZ = adjPos.z - currentNodePos.z;

            if(Mathf.Abs(originalX) == 1 && Mathf.Abs(originalZ) == 1)
            {
                //The first block is originalX, 0 and the second to check is 0, originalZ
                //Need to be pathfinding walkable
                Node neighbour1 = GetNode(currentNodePos.x + originalX, currentNodePos.y, currentNodePos.z);
                if(neighbour1 == null || !neighbour1.isWalkable)
                {
                    returnValue = null;
                }

                Node neighbour2 = GetNode(currentNodePos.x, currentNodePos.y, currentNodePos.z + originalZ);
                if(neighbour2 == null || !neighbour2.isWalkable)
                {
                    returnValue = null;
                }
            }

            //Add additional checks
            if(returnValue != null)
            {
                //example
            }

            return returnValue;
        }

        private Node GetNode(int x, int y, int z)
        {
            Node n = null;

            lock (gridBase)
            {
                n = gridBase.GetNode(x, y, z);
            }
            return n;
        }

        private int GetDistance(Node posA, Node posB)
        {
            //find distance between each node

            int distX = Mathf.Abs(posA.x - posB.x);
            int distY = Mathf.Abs(posA.y - posB.y);
            int distZ = Mathf.Abs(posA.z - posB.z);

            if (distX > distZ)
            {
                return 14 * distZ + 10 * (distX - distZ) + 10 * distY;
            }

            return 14 * distZ + 10 * (distZ - distX) + 10 * distY;
        }
    }
}
