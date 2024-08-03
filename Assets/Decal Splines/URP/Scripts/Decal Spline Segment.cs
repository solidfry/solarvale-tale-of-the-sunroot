using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace DecalSplines
{
    public class DecalSplineSegment : ISplineSegment
    {
#if UNITY_EDITOR
        [SerializeField] private ProjectorManager projectorManager;
     
        public static DecalSplineSegment Spawn(string name, DecalSplineStyle style, Vector3 position, Transform parentTransform, DecalSpline parentSpline)
        {
            GameObject segmentObject = new GameObject();
            Undo.RegisterCreatedObjectUndo(segmentObject, "Added spline segment.");

            segmentObject.name = name;
            Undo.SetTransformParent(segmentObject.transform, parentTransform,"Set parent for new segment");
            //segmentObject.transform.parent = parentTransform;

            DecalSplineSegment newSegment = Undo.AddComponent<DecalSplineSegment>(segmentObject);
            //DecalSplineSegment newSegment = segmentObject.AddComponent<DecalSplineSegment>();
            newSegment.style = style;
            newSegment.Position = position;
            newSegment.transform.rotation = parentTransform.rotation;
            newSegment._h1 = position;
            newSegment._h2 = position;
            newSegment.lockHandles = true;
            newSegment._parentDecalSpline = parentSpline;

            newSegment.prev = null;
            newSegment.next = null;

            newSegment.projectorManager = ProjectorManager.Spawn(newSegment);

            return newSegment;

        }

        public new DecalSplineStyle style
        {
            get { return (DecalSplineStyle)_style; }
            set { _style = value; }
        }

        public override void UpdateSegment()
        {
            if(projectorManager != null)
                projectorManager.UpdateProjectors();
        }

        public override Vector3[] GetCurvePoints()
        {
            Vector3[] points = new Vector3[0];
            if (!IsLast())
            {
                float resolution = style.Resolution;
                float curveLength = GetCurveLength();
                int pointCount = (int)(curveLength * resolution) + 2;
                points = Handles.MakeBezierPoints(Position, next.Position, h1, h2, pointCount);
            }
            return points;
        }

        public DecalProjector GetFirstProjector()
        {
            return projectorManager.GetFirstProjector();
        }

        public DecalProjector GetLastProjector()
        {
            return projectorManager.GetLastProjector();
        }

        public void DisconnectLastProjector()
        {
            projectorManager.DisconnectLastProjector();
        }

        public void DisconnectFirstProjector()
        {
            projectorManager.DisconnectFirstProjector();
        }
#endif
    }
}