using UnityEngine;
using Character.Movement;
using System;
using Character.Combat;
using Character.Core;
using GameMechanics.GlobalSystem;

namespace Character.Control
{
    public class PlayerAI : MonoBehaviour
    {
        Mover mover = null;
        Wander wander = null;
        CombatController combatController = null;
        TargetFinder targetFinder;
        AllyFinder allyFinder;
        Health health;
        CharacterStats characterStats;
        GameObject formationChief;
        DestinationPointFinder formationChiefDPF;
        Vector3 formationOffSet;
        Quaternion formationChiefDeltaRotation;
        Quaternion formationChiefOriginalRotation;

        Vector3 dest;
        float distance;
        FormationNode connectedNode;

        Transform fastPhasedDestinaton;
        bool isFastPhased = false;

        private void Start()
        {
            mover = GetComponent<Mover>();
            wander = GetComponent<Wander>();
            combatController = GetComponent<CombatController>();
            targetFinder = GetComponent<TargetFinder>();
            allyFinder = GetComponent<AllyFinder>();
            characterStats = GetComponent<CharacterStats>();
             
            health = GetComponent<Health>();
            if (!isFastPhased)
            {
                formationChief = GameObject.Find("FormationChief");
                formationChiefDPF = formationChief.GetComponent<DestinationPointFinder>();
            }
        }

        private void Update()
        {
            if (health.IsDead()) return;
            if (InteractWithCombat()) return;
            if (InteractWithMovement()) return;

            if (GlobalVariables.readyForPreparation && DestinationPointFinder.ReachedEndPoint() == false) return;
            if (GlobalVariables.readyForAttack && DestinationPointFinder.ReachedEndPoint() == false) return;
            if (isFastPhased) return;
            if (InteractWithWandering()) return;
        }

        private bool InteractWithCombat()
        {
            if (characterStats.characterType == 2800 && allyFinder.target != null)
            {
                combatController.StartAttackAction(allyFinder.target);
                return true;
            }

            if (characterStats.characterType != 2800 && targetFinder.target != null)
            {
                combatController.StartAttackAction(targetFinder.target);
                return true;
            }

            if (targetFinder.target == null && allyFinder.target == null)
            {
                combatController.StartAttackAction(null);
                return false;
            }
            return false;
        }

        private bool InteractWithMovement()
        {
            //if (destinationPointFinder.targetDestination != null && targetFinder.target == null && destinationPointFinder.endPoint != true)
            //{
            //    mover.StartMoveAction(destinationPointFinder.targetDestination.position, true);
            //    return true;
            //}

            if (formationChief != null && formationChiefDPF.targetDestination != null && targetFinder.target == null && DestinationPointFinder.ReachedEndPoint() != true)
            {
                formationChiefDeltaRotation = formationChief.transform.rotation* Quaternion.Inverse(formationChiefOriginalRotation);
                dest = formationChief.transform.position + formationChiefDeltaRotation * formationOffSet;
                distance = Vector3.Distance(transform.position , dest);
                if (distance >= 2.5f)
                {
                    mover.StartMoveAction(dest, false);
                }
                else
                {
                    mover.StartMoveAction(dest, true);
                }
                
                return true;
            }
            if (fastPhasedDestinaton != null && targetFinder.target == null)
            {
                mover.StartMoveAction(fastPhasedDestinaton.position, false);
                return true;
            }
            if (fastPhasedDestinaton != null && targetFinder.target != null)
            {
                mover.StartMoveAction(transform.position, false);
                return true;
            }

            return false;

            #region MouseClickMove
            //RaycastHit hit;

            //bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
            //if (hasHit)
            //{
            //    if (Input.GetMouseButton(0))
            //    {
            //        GetComponent<Mover>().StartMoveAction(hit.point);
            //    }
            //    return true;
            //}
            //return false;
            #endregion
        }

        private bool InteractWithWandering()
        {
            if (formationChiefDPF == null)
            {
                wander.StartWanderAction();
                return true;
            }
            if (formationChiefDPF.targetDestination == null && targetFinder.target == null)
            {
                wander.StartWanderAction();
                return true;
            }
            if (formationChiefDPF.targetDestination != null && DestinationPointFinder.ReachedEndPoint() == true)
            {
                wander.StartWanderAction();
                return true;
            }
            return false;
        }

        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        public void SetFormationValues(GameObject formationChief, Vector3 formationOffSet)
        {
            this.formationChief = formationChief;
            this.formationOffSet = formationOffSet;
            formationChiefOriginalRotation = formationChief.transform.rotation;
            formationChiefDPF = formationChief.GetComponent<DestinationPointFinder>();
        }

        public void SetNodeConnection(FormationNode connectedNode)
        {
            this.connectedNode = connectedNode;
        }
        public FormationNode GetNodeConnection()
        {
            return connectedNode;
        }
        public void SendUnitToNode()
        {
            mover.WarpTo(connectedNode.transform.position);
        }
        public void BreakNodeConnection()
        {
            if (connectedNode != null)
            {
                connectedNode.BreakCharacterConnection();
                connectedNode = null;
            }
        }
        public void SetFastPhasedDestination(Transform destination, bool isFastPhased)
        {
            fastPhasedDestinaton = destination;
            this.isFastPhased = isFastPhased;
        }
    }
}

