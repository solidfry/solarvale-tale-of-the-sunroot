using Behaviour.Tree.Nodes;
using UnityEngine;

namespace Behaviour.Tree
{
    public abstract class Tree : MonoBehaviour
    {
        private Node _root = null;
        
        protected void Start()
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