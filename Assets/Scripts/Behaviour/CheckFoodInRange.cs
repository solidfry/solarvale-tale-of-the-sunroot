using Behaviour.Tree.Nodes;
using Creatures;
using UnityEngine;

namespace Behaviour
{
    public class CheckFoodInRange : Node
    {
        private Creature _creature;
        private LayerMask _layerMask;
        private Transform _transform;
        // Animator _animator;
        // the animator will be pulled from the creature 
        
        
        private Collider[] _colliders = new Collider[5];
        
        public CheckFoodInRange(Creature creature, LayerMask layerMask)
        {
            _creature = creature;
            Debug.Log("CHECKINFOOD: Creature is " + _creature);
            _transform = creature.transform;
            _layerMask = layerMask;
            Debug.Log($"{creature.GetStats}  {_creature.GetEntityData} {_creature.GetStats.SightRange}" , creature);
            Debug.Log("CHECKINFOOD: Creature is " + _layerMask);
        }
        
        public override NodeState Evaluate()
        {
            if (_creature.GetBehaviourTree.GetAllNonNullTargetSources( out var nonNullTargets) > 0)
            {
                Debug.Log(_transform + "CHECKINFOOD: Target is " + nonNullTargets[0]);
                GetRootNode().SetData("target", nonNullTargets[0]);
                State = NodeState.Success;
                return State;
            }
            Transform t = (Transform)GetData("target");
            Debug.Log("CHECKINFOOD: Target is " + t);
            if (t == null)
            {
                var size = Physics.OverlapSphereNonAlloc(_transform.position, _creature.GetStats.SightRange, _colliders, _layerMask);
                
                if (size > 0) 
                {
                    Parent.Parent.SetData("target", _colliders[0].transform);
                    _creature.GetBehaviourTree.SetFoodSources(UpdateFoodSources(size));
                    State = NodeState.Success;
                    return State;
                }
                
                State = NodeState.Failure;
                return State;
                
            }
            
            State = NodeState.Success;
            return State;
        }

        private Transform[] UpdateFoodSources(int size)
        {
            Transform[] foodSources = new Transform[size];
            for (int i = 0; i < size; i++)
            {
                foodSources[i] = _colliders[i].transform;
            }

            return foodSources;
        }
    }
}