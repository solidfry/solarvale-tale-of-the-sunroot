using UnityEngine;

namespace Gaia
{
    public abstract class GaiaTaskBase
    {
        public bool TaskFinished = false;
        public float TaskWaitTime = 0.25f;

        public abstract void SetTaskTimer(float value);
        public abstract void DoTask();
        public abstract void FinishTask();
    }
}