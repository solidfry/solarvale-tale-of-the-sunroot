using UnityEditor;
using UnityEngine;

namespace DecalSplines
{
    public class MeshSplineSegment : ISplineSegment
    {
#if UNITY_EDITOR
        [SerializeField] private ModelManager modelManager;

        public static MeshSplineSegment Spawn(string name, MeshSplineStyle style, Vector3 position, Transform parent,DecalSpline parentSpline)
        {
            GameObject segmentObject = new GameObject();
            Undo.RegisterCreatedObjectUndo(segmentObject, "Added spline segment.");

            segmentObject.name = name;
            Undo.SetTransformParent(segmentObject.transform, parent, "Set parent");
            //segmentObject.transform.parent = parent;

            MeshSplineSegment newSegment = Undo.AddComponent<MeshSplineSegment>(segmentObject);
            //MeshSplineSegment newSegment = segmentObject.AddComponent<MeshSplineSegment>();
            newSegment.style = style;
            newSegment.Position = position;
            newSegment.transform.rotation = parent.rotation;
            newSegment.lockHandles = true;
            newSegment._parentDecalSpline = parentSpline;

            newSegment.prev = null;
            newSegment.next = null;


            newSegment.modelManager = ModelManager.Spawn(newSegment);

            return newSegment;
        }

        public new MeshSplineStyle style
        {
            get { return (MeshSplineStyle)_style; }
            set { _style = value; }
        }

        public override void UpdateSegment()
        {
            if(modelManager != null)
                modelManager.UpdateModels();
        }

        public override Vector3[] GetCurvePoints()
        {
            Vector3[] points = new Vector3[0];
            if (!IsLast())
            {
                float curveLength = GetCurveLength();
                int pointCount = (int)style.BoneCount;
                points = Handles.MakeBezierPoints(Position, next.Position, h1, h2, pointCount);
            }
            return points;
        }

#endif
    }
}
