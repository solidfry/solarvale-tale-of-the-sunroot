using Behaviour.Tree.Nodes;
using UnityEngine;

namespace Behaviour.Tree
{
    public abstract class BehaviourTreeBase : MonoBehaviour
    {
        private Node _root = null;
        
        protected virtual void Start()
        {
            _root = SetupTree();
        }

        protected abstract Node SetupTree();
        
        protected void Update()
        {
            if (_root is not null)
                _root.Evaluate();
        }
    }
}