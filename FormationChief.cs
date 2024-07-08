using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character.Movement;
using Character.Combat;
using Character.Core;
using UnityEngine.AI;
using System.Linq;

public class FormationChief : MonoBehaviour
{
    DestinationPointFinder destinationPointFinder;
    TargetFinder targetFinder;
    NavMeshAgent agent;
    string alliesTag = "Alliance";
    bool[] targetFound;
    bool targetDecision = false;
    int liveAllies = 0;
    private void Start()
    {
        destinationPointFinder = GetComponent<DestinationPointFinder>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 2f;

        agent.Warp(transform.position);

        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }
    private void Update()
    {
        GoToDestinationPoint();
    }

    private void UpdateTarget()
    {
        SearchTarget();
    }

    void GoToDestinationPoint()
    {
        if (destinationPointFinder.targetDestination != null && targetDecision == false && DestinationPointFinder.ReachedEndPoint() != true && liveAllies != 0)
        {
            agent.SetDestination(destinationPointFinder.targetDestination.position);
        }
        else
        {
            agent.SetDestination(transform.position);
        }
    }

    private void SearchTarget()
    {
        GameObject[] allies = GameObject.FindGameObjectsWithTag(alliesTag);
        targetFound = new bool[allies.Length];
        int i = 0;
        liveAllies = 0;
        foreach (GameObject ally in allies)
        {
            targetFinder = ally.GetComponent<TargetFinder>();
            if (targetFinder.target != null)
            {
                targetFound[i] = true;
            }
            if (targetFinder.target == null)
            {
                targetFound[i] = false;
            }

            if (!ally.GetComponent<Health>().IsDead())
            {
                liveAllies++;
            }
            i++;
        }

        if (targetFound.Contains(true))
        {
            targetDecision = true;
        }
        else
        {
            targetDecision = false;
        }
    }
}
