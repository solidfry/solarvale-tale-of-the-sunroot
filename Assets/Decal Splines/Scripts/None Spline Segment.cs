using UnityEditor;
using UnityEngine;

namespace DecalSplines
{
    public class NoneSplineSegment : ISplineSegment
    {
#if UNITY_EDITOR
        public static NoneSplineSegment Spawn(string name, NoneSplineStyle style, Vector3 position, Transform parent, DecalSpline parentSpline)
        {
            GameObject segmentObject = new GameObject();
            Undo.RegisterCreatedObjectUndo(segmentObject, "Added spline segment.");

            segmentObject.name = name;
            segmentObject.transform.parent = parent;

            NoneSplineSegment newSegment = segmentObject.AddComponent<NoneSplineSegment>();
            newSegment.style = style;
            newSegment.Position = position;
            newSegment.transform.rotation = parent.rotation;
            newSegment.lockHandles = true;
            newSegment._parentDecalSpline = parentSpline;

            newSegment.prev = null;
            newSegment.next = null;

            return newSegment;
        }

        public override void UpdateSegment()
        {

        }
#endif
    }
}
