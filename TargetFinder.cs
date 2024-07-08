using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character.Core;


namespace Character.Combat
{
    public class TargetFinder : MonoBehaviour
    {
        [SerializeField] string targetTag = null;
        [SerializeField] string friendTag = null;
        CharacterStats characterStats = null;
        public Transform target;
        GameObject nearestEnemy;
        float shortestDistance;
        Health health;
        TargetFinder friendTargetFinder;

        private void Start()
        {
            characterStats = GetComponent<CharacterStats>();
            InvokeRepeating("UpdateTarget", 0f, 0.5f);
        }

        private void UpdateTarget()
        {
            if (GetComponent<Health>().IsDead())
            {
                target = null;
                return;
            }
            SearchTarget();
        }

        private void SearchTarget()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(targetTag);
            shortestDistance = Mathf.Infinity;
            nearestEnemy = null;

            foreach (GameObject enemy in enemies)
            {
                health = enemy.GetComponent<Health>();
                if (health == null || health.IsDead()) continue;
                FindingNearestenemy(enemy);
            }

            if (nearestEnemy != null && shortestDistance <= characterStats.awarenessRange)
            {
                target = nearestEnemy.transform;
            }
            else
            {
                FindingOtherAlliesTarget();
            }
        }

        private GameObject FindingNearestenemy(GameObject enemy)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance && !health.IsDead())
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }

            return nearestEnemy;
        }

        private void FindingOtherAlliesTarget()
        {
            if (characterStats.characterType == 2800)
            {
                target = null;
                return;
            }

            if (GameObject.FindGameObjectsWithTag(friendTag) == null)
            {
                target = null;
                return;
            }
            GameObject[] friends = GameObject.FindGameObjectsWithTag(friendTag);
            foreach (GameObject friend in friends)
            {
                float distanceToFriend = Vector3.Distance(transform.position, friend.transform.position);
                friendTargetFinder = friend.GetComponent<TargetFinder>();

                if (distanceToFriend <= characterStats.friendAwarenessRange && friendTargetFinder.target != null && !friendTargetFinder.target.gameObject.GetComponent<Health>().IsDead())
                {
                    target = friendTargetFinder.target;
                    break;
                }
                else
                {
                    target = null;
                }
            }
        }
    }
}
