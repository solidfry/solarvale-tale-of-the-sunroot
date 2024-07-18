using UnityEngine;

namespace OccaSoftware.Buto.Runtime
{
    public static class WindOffsetHandler
    {
        public static Vector3 position = Vector3.zero;
        public static int lastUpdateFrame = 0;
        public static WindZone windZone = null;

        public static void UpdateWindOffset(Vector3 wind)
        {
            if (lastUpdateFrame == Time.frameCount)
                return;

            if (windZone != null)
                wind = windZone.windMain * windZone.transform.forward;

            lastUpdateFrame = Time.frameCount;
            position += wind * Time.deltaTime;
        }

        public static void SetWindOffset(Vector3 position)
        {
            WindOffsetHandler.position = position;
        }

        public static void SetWindZone(WindZone windZone)
        {
            WindOffsetHandler.windZone = windZone;
        }

        public static void ClearWindZone()
        {
            WindOffsetHandler.windZone = null;
        }
    }
}
