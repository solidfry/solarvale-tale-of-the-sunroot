using UnityEngine;

namespace QuestSystem.InitialisationActions
{
    public abstract class InitialisationAction : ScriptableObject, IInitialisationAction
    {
        public virtual void Execute()
        {
            return;
        }

        public virtual void Clear()
        {
            return;
        }
    }
}