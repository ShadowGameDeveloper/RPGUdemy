using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using UnityEngine;

namespace RPG.Control
{
    public class AIConroller : MonoBehaviour
    {
        [SerializeField] float distanceTOChase = 5f;
        [SerializeField] float suspicionTime = 3f;
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float wayPointTolarance = 3f;
        [SerializeField] float wayPointDwellTime = 3f;
        
        Fighter fighter;
        Health health;
        GameObject player;
        Mover moves;

        float timeSinceLastSawPlayer = Mathf.Infinity;
        float timeSinceArriveAt = Mathf.Infinity;
        Vector3 guardPos;

        int wayPointIndex = 0;

        private void Start()
        {
            moves = GetComponent<Mover>();
            fighter = GetComponent<Fighter>();
            health = GetComponent<Health>();

            guardPos = transform.position;
            player = GameObject.FindWithTag("Player");
        }
        // Update is called once per frame
        void Update()
        {
            if (health.IsDead()) return;
            if (InAttackRangeOfPlayer() && fighter.CanAttack(player))
            {
                timeSinceLastSawPlayer = 0;
                AttackBehavior();
            }

            else if (timeSinceLastSawPlayer < suspicionTime)
            {
                SuspicionBehavior();
            }

            else
            {
                PatrolBehavior();
            }
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            timeSinceArriveAt += Time.deltaTime;
            timeSinceLastSawPlayer += Time.deltaTime;
        }

        private void PatrolBehavior()
        {
            Vector3 nextPosition = guardPos;

            if(patrolPath != null)
            {
                if(AtMyWayPoint())
                {
                    timeSinceArriveAt = 0;
                    CycleWaypoint();
                }
                nextPosition = GetCurrentWayPoint();
            }
            if(timeSinceArriveAt > wayPointDwellTime)
            {
                moves.StartMoveAction(nextPosition);
            }
        }

        private bool AtMyWayPoint()
        {
            float distanceToWayPoint = Vector3.Distance(transform.position, GetCurrentWayPoint());
            return distanceToWayPoint < wayPointTolarance;
        }

        private void CycleWaypoint()
        {
            wayPointIndex = patrolPath.GetNextIndex(wayPointIndex);
        }

        private Vector3 GetCurrentWayPoint()
        {
            return patrolPath.GetMyWaPoint(wayPointIndex);
        }
        private void SuspicionBehavior()
        {
            GetComponent<ActionScheduler>().CancelCurrentAcion();
        }

        private void AttackBehavior()
        {
            timeSinceLastSawPlayer = 0;
            fighter.Attack(player);
        }

        private bool InAttackRangeOfPlayer()
        {
            float disatnceToPlay = Vector3.Distance(player.transform.position, transform.position);
            return disatnceToPlay < distanceTOChase;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, distanceTOChase);
        }
    }

}