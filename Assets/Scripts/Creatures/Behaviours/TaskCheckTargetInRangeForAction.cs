using System;
using Behaviour.Tree.Nodes;
using UnityEngine;

namespace Creatures.Behaviours
{
    public class TaskCheckTargetInRangeForAction : Node
    {
        private Creature _creature;
        private LayerMask _layerMask;
        private Transform _transform; 
        // private Animator _animator;
        // the animator will be pulled from the creature
        
        public TaskCheckTargetInRangeForAction (Creature creature, LayerMask layerMask)
        {
            _creature = creature;
            _transform = creature.transform;
            _layerMask = layerMask;
        }

        public override NodeState Evaluate()
        {
            Transform target = (Transform)GetData("target");
            
            if (target is null)
            {
                State = NodeState.Failure;
                return State;
            }

            if (IsInRangeForAction())
            {
                Debug.Log("CheckTargetInRangeForAction: Arrived at target");
                State = NodeState.Success;
                return State;
            }

            
            Debug.Log("CheckTargetInRangeForAction: Moving to target to perform action");
            
            State = NodeState.Running;
            return State;
            
        }

        private bool IsInRangeForAction()
        {
            return _creature.GetAgent().remainingDistance <= _creature.GetAgent().stoppingDistance && !_creature.GetAgent().pathPending;
        }
    }
}