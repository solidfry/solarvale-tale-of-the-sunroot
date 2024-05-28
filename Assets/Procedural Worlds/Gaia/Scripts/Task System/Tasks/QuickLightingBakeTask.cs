#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    public class QuickLightingBakeTask : GaiaTaskBase
    {
        public override void SetTaskTimer(float value)
        {
            TaskWaitTime = value;
        }
        public override void DoTask()
        {
#if UNITY_EDITOR
            Lightmapping.bakedGI = false;
            Lightmapping.realtimeGI = false;
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
            Lightmapping.BakeAsync();
#endif
            FinishTask();
        }
        public override void FinishTask()
        {
            TaskFinished = true;
        }
    }
}