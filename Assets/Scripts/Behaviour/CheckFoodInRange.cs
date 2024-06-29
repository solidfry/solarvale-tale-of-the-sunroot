using Behaviour.Tree.Nodes;
using Creatures;
using UnityEngine;

namespace Behaviour
{
    public class CheckFoodInRange : Node
    {
        private Creature _creature;
        private LayerMask _foodLayer;
        private Transform _transform;
        private Transform[] _foodSources;
        // Animator _animator;
        // the animator will be pulled from the creature 
        
        
        private Collider[] _colliders = new Collider[5];
        
        public CheckFoodInRange(Creature creature, LayerMask foodLayer)
        {
            _creature = creature;
            _transform = creature.transform;
            _foodLayer = foodLayer;
        }
        
        public override NodeState Evaluate()
        {
            object t = GetData("target");
            if (t == null)
            {
                var size = Physics.OverlapSphereNonAlloc(_transform.position, _creature.GetStats.SightRange, _colliders, _foodLayer);
               
                
                if (size == 0)
                {
                    State = NodeState.Failure;
                    return State;
                }
                
                var foodSources = UpdateFoodSources(size);

                _creature.GetBehaviourTree.SetFoodSources(foodSources);
                
                Parent.Parent.SetData("target", _colliders[0].transform);
                // _animator.SetBool("isWalking", true);
                State = NodeState.Success;
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