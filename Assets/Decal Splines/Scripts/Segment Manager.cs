using System;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace DecalSplines
{
    public class SegmentManager : MonoBehaviour
    {
        //[HideInInspector]
        [SerializeField] private ISplineSegment firstSegment;

#if UNITY_EDITOR
        //Update the Decal Spline
        public void UpdateDecalSpline()
        {
            ISplineSegment segment = firstSegment;
            while (segment != null)
            {
                segment.UpdateSegment();
                segment = segment.next;
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public void OnFirstRemoved(object sender, EventArgs e)
        {
            firstSegment = firstSegment.next;

            if (firstSegment != null)
            {
                firstSegment.OnRemoved += OnFirstRemoved;
            }
        }

        //Deletes all segments
        public void ClearSegments()
        {
            if (firstSegment != null)
            {
                firstSegment.Cut();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        public void AddSegment(Vector3 position, ISplineStyle style,DecalSpline parentDecalSpline)
        {
            int count = 0;
            if (firstSegment != null)
                count = firstSegment.Count;

            string name = "M" + count.ToString();
            Vector3 pos = transform.InverseTransformPoint(position);

            ISplineSegment newSegment = null;
            if (style.GetType() == typeof(DecalSplineStyle))
                newSegment = DecalSplineSegment.Spawn(name, (DecalSplineStyle)style, pos, transform, parentDecalSpline);
            else if (style.GetType() == typeof(MeshSplineStyle))
                newSegment = MeshSplineSegment.Spawn(name, (MeshSplineStyle)style, pos, transform, parentDecalSpline);
            else if (style.GetType() == typeof(NoneSplineStyle))
                newSegment = NoneSplineSegment.Spawn(name, (NoneSplineStyle)style, pos, transform, parentDecalSpline);

            newSegment.lockHandles = !style.FreeHandles;

            if (firstSegment != null)
                firstSegment.Append(newSegment);
            else
            {
                firstSegment = newSegment;
                firstSegment.OnRemoved += OnFirstRemoved;
            }

            
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public void Snap(Quaternion rotation)
        {
            ISplineSegment segment = firstSegment;
            while (segment != null)
            {
                //Snap PositionHandle
                Vector3 pos = transform.TransformPoint(segment.Position);
                Vector3 newPos = GetSnapPosition(pos, rotation,segment);
                
                if (!segment.IsFirst())
                {
                    if (segment.prev.style.IsFixedLenght())
                    {
                        Vector3 prevWorld = transform.TransformPoint(segment.prev.Position);
                        Vector3 dir = (newPos - prevWorld).normalized;
                        newPos = prevWorld + dir * segment.prev.style.FixedLength;
                    }
                }

                segment.Position = transform.InverseTransformPoint(newPos);

                //Snap H1 and H2
                Vector3 newH1 = transform.TransformPoint(segment.h1);
                segment.h1 = transform.InverseTransformPoint(newH1);
                Vector3 newH2 = transform.TransformPoint(segment.h2);
                segment.h2 = transform.InverseTransformPoint(newH2);

                segment = segment.next;
            }
        }

        private Vector3 GetSnapPosition(Vector3 pos, Quaternion rotation, ISplineSegment segment)
        {
            Vector3 up = rotation * Vector3.up;
            Vector3 down = rotation * Vector3.down;


            Vector3 snapPos = pos;
            float closestDistance = float.MaxValue;

            //Check the down ray first.
            Ray rayDown = new Ray(pos + up * 0.001f, down);
            RaycastHit[] raycastHits = Physics.RaycastAll(rayDown, 2000f);
            if (raycastHits != null && raycastHits.Length > 0)
            {
                foreach (RaycastHit hit in raycastHits)
                {
                    if (hit.distance <= closestDistance)
                    {
                        if (!hit.transform.IsChildOf(segment.transform.parent))
                        {
                            closestDistance = hit.distance;
                            snapPos = hit.point;
                        }
                    }
                }
            }

            //Check the up ray.
            Ray rayUp = new Ray(pos + down * 0.001f, up);
            raycastHits = Physics.RaycastAll(rayUp, 2000f);
            if (raycastHits != null && raycastHits.Length > 0)
            {
                foreach (RaycastHit hit in raycastHits)
                {
                    if (hit.distance <= closestDistance)
                    {
                        if (!hit.transform.IsChildOf(segment.transform.parent))
                        {
                            closestDistance = hit.distance;
                            snapPos = hit.point;
                        }
                    }
                }
            }

            return snapPos;
        }

        //Draw the Editor Gizmos
        public void DrawGizmos()
        {
            if (firstSegment != null)
            {
                ISplineSegment segment = firstSegment;
                while (segment != null)
                {
                    segment.DrawGizmos();
                    segment = segment.next;
                }
            }
        }
#endif
    }
}