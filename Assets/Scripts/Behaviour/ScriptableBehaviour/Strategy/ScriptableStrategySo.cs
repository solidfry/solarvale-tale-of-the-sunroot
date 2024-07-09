using Behaviour.Pathfinding;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour.Strategy
{
    public abstract class ScriptableStrategySo : ScriptableObject, IScriptableStrategy
    {
        public abstract NodeState Process(BehaviourTreeContext context);

        public virtual void Reset()
        {
            // Noop
        }
    }

    public class MoveToTarget : ScriptableStrategySo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            return NodeState.Running;
        }
    }
    

    public interface IScriptableStrategy
    {
        NodeState Process(BehaviourTreeContext context);

        void Reset()
        {
            // Noop
        }
    }
}