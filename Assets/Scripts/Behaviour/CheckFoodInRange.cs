using Behaviour.Tree.Nodes;
using UnityEngine;

namespace Behaviour
{
    public class CheckFoodInRange : Node
    {
        private LayerMask _foodLayer;
        private Transform _transform;
        
        private Collider[] _colliders = new Collider[5];
        
        public CheckFoodInRange(Transform transform)
        {
            _transform = transform;
        }
        
        public override NodeState Evaluate()
        {
            object t = GetData("food");
            if (t == null)
            {
                var size = Physics.OverlapSphereNonAlloc(_transform.position, 5, _colliders, _foodLayer);
                if (size == 0)
                {
                    State = NodeState.Failure;
                    return State;
                }
            }
            
            State = NodeState.Success;
            return State;
        }
    }
}