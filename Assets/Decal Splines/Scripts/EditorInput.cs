using System;
using UnityEditor;
using UnityEngine;

namespace DecalSplines
{
    [Serializable]
    public static class EditorInput
    {
#if UNITY_EDITOR
        public static bool LeftMouseDown;
        public static bool ShiftDown;
        public static bool CTRLDown;
        public static bool SpaceDown;

        //Find the mouse position in 3D.
        public static bool MousePosition3D(Vector2 mousePos, Transform parent, out Vector3 position3D)
        {
            Camera sceneCam = SceneView.currentDrawingSceneView.camera;
            Ray ray = sceneCam.ScreenPointToRay(mousePos);

            Vector3 snapPos = Vector3.zero;
            bool result = false;
            RaycastHit[] raycastHits = Physics.RaycastAll(ray, 2000f);
            if (raycastHits != null && raycastHits.Length > 0)
            {
                float closestDistance = float.MaxValue;
                foreach (RaycastHit hit in raycastHits)
                {
                    if (hit.distance <= closestDistance)
                    {
                        if (!hit.transform.IsChildOf(parent))
                        {
                            closestDistance = hit.distance;
                            snapPos = hit.point;
                            result = true;
                        }
                    }
                }
            }

            position3D = snapPos;
            return result;
        }
#endif
    }
}
