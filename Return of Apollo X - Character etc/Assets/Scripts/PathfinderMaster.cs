using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridMaster;
using System.Threading;

namespace Pathfinding
{
    public class PathfinderMaster : MonoBehaviour
    {
        private static PathfinderMaster instance;

        private void Awake()
        {
            instance = this;
        }

        public static PathfinderMaster GetInstance()
        {
            return instance;
        }

        public int maxJobs = 3;

        public delegate void PathFindingJobComplete(List<Node> path);

        private List<Pathfinder> currentJobs;
        private List<Pathfinder> toDoJobs;

        private void Start()
        {
            currentJobs = new List<Pathfinder>();
            toDoJobs = new List<Pathfinder>();
        }

        private void Update()
        {
            int i = 0; //index

            while(i < currentJobs.Count)
            {
                if (currentJobs[i].jobDone)
                {
                    currentJobs[i].NotifyComplete(); //if job is done notify
                    currentJobs.RemoveAt(i); //if job is done remove from index
                }
                else
                {
                    i++;
                }
            }

            if(toDoJobs.Count > 0 && currentJobs.Count < maxJobs)
            {
                Pathfinder job = toDoJobs[0];
                toDoJobs.RemoveAt(0);
                currentJobs.Add(job);
                //start new thread
                Thread jobThread = new Thread(job.FindPath);
                jobThread.Start();
            }
        }

        public void RequestPathFind(Node start, Node target, PathFindingJobComplete completeCallback)
        {
            Pathfinder newJob = new Pathfinder(start, target, completeCallback);
            toDoJobs.Add(newJob);
        }
    }
}