using System.Collections.Generic;
using Behaviour.Tree.Nodes;
using UnityEngine;

namespace Creatures.Behaviours
{
    public class TaskFindTargetInSightRange : Node
    {
        private Creature _creature;
        private LayerMask _layerMask;
        private Transform _transform;
        private List<Transform> _targets;
        private float _sightRangeMultiplier = 1f;
        // Animator _animator;
        // the animator will be pulled from the creature 
        
        
        private Collider[] _colliders = new Collider[5];
        
        public TaskFindTargetInSightRange(Creature creature, LayerMask layerMask, ref List<Transform> targets)
        {
            _creature = creature;
            _transform = creature.transform;
            _layerMask = layerMask;
            _targets = targets;
        }
        
        public override NodeState Evaluate()
        {
            Transform t = (Transform)GetData("target");
            
            if (t is null)
            {
                var size = Physics.OverlapSphereNonAlloc(_transform.position, _creature.GetStats.SightRange * _sightRangeMultiplier, _colliders, _layerMask);
                
                if (size > 0) 
                { 
                    Parent.Parent.SetData("target", _colliders[0].transform);
                    _creature.GetBehaviourTree.SetFoodSources(UpdateFoodSources(size));
                    
                    
                    _sightRangeMultiplier = 1f;
                    _creature.CurrentSightRange = _creature.GetStats.SightRange;
                    Debug.Log("Found target in sight range");
                    State = NodeState.Success;
                    return State;
                }
                
                _sightRangeMultiplier += 1;
                _creature.CurrentSightRange = _creature.GetStats.SightRange * _sightRangeMultiplier;
                // Debug.Log("TaskFindTargetInSightRange: Target is " + t + " and sight range multiplier is " + _sightRangeMultiplier + " and the total sight range is " + _creature.GetStats.SightRange * _sightRangeMultiplier);

                State = NodeState.Running;
                return State;
            }
           
            State = NodeState.Failure;
            return State;
        }

        private List<Transform> UpdateFoodSources(int size)
        {
            List<Transform> foodSources = new (5);
            for (int i = 0; i < size; i++)
            {
                foodSources.Add(_colliders[i].transform);
            }

            return foodSources;
        }
    }
}