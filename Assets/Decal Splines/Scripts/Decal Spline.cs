
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DecalSplines
{
    public class DecalSpline : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private SplineTheme activeTheme;
        [SerializeField] private float projectionDepth = 5f;
        [SerializeField] private uint renderLayerMask = uint.MaxValue;
        [SerializeField] private bool autoSnap = true;
        [SerializeField] private bool liveUpdate = true;

        public float ProjectionDepth {get{return projectionDepth;} }
        public uint RenderLayerMask { get { return renderLayerMask; } }
        public bool AutoSnap { get { return autoSnap; } }
        public bool LiveUpdate { get { return liveUpdate; } }

        private SegmentManager _segmentManager;
        private SegmentManager segmentManager
        {
            get
            {
                if (_segmentManager != null)
                    return _segmentManager;
                FindSegmentsManager();
                return _segmentManager;
            }
        }

        //Draw the Editor Gizmos
        public void DrawGizmos()
        {
            segmentManager.DrawGizmos();
        }

        //Find the Segments Transform or Create one if it doesn't excist yet.
        private void FindSegmentsManager()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                //if the child's name is "Segments" then it is the segment container and _segments will be assigned.
                Transform child = transform.GetChild(i);
                if (child.name == "Segments")
                {
                    child.TryGetComponent<SegmentManager>(out _segmentManager);
                    return;
                }
            }
            //if no matching container was found an object named "Segments" will be created.
            GameObject segmentContainer = new GameObject();
            segmentContainer.name = "Segments";
            segmentContainer.transform.parent = transform;
            segmentContainer.transform.position = transform.position;
            _segmentManager = segmentContainer.AddComponent<SegmentManager>();

        }

        //Snaps the Decal Spline in place
        public void Snap()
        {
            segmentManager.Snap(transform.rotation);
        }

        //Update the Decal Spline
        public void UpdateDecalSpline()
        {
            segmentManager.UpdateDecalSpline();
        }

        //Deletes all segments
        public void ClearDecalSpline()
        {
            segmentManager.ClearSegments();
        }

        public void AddSegment(Vector3 position, ISplineStyle style)
        {
            if (style != null)
                segmentManager.AddSegment(position, style, this);  
        }
#endif
    }
}
