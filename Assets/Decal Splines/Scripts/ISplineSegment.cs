using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DecalSplines
{
    public class ISplineSegment : MonoBehaviour
    {
        #if UNITY_EDITOR
        [HideInInspector][SerializeField] protected DecalSpline _parentDecalSpline;
        [SerializeField] protected ISplineStyle _style;
        [SerializeField] protected Vector3 _h1;//Bezier handle1
        [SerializeField] protected Vector3 _h2;//Bezier handle2
        [SerializeField] public bool lockHandles;
        [SerializeField] public ISplineSegment prev;
        [SerializeField] public ISplineSegment next;


        public EventHandler OnRemoved;


        public Vector3 Position
        {
            get { return transform.localPosition; }
            set
            {
                Vector3 displacement = value - transform.localPosition;
                _h1 = _h1 + displacement;
                transform.localPosition = value;
                if (!IsFirst())
                {
                    prev._h2 += displacement;
                    //prev.UpdateSegment();
                }
                //UpdateSegment();
            }
        }

        public Vector3 h1
        {
            get { return _h1; }
            set { SetHandles(value, _h2); }
        }
        public Vector3 h2
        {
            get { return _h2; }
            set { SetHandles(_h1, value); }
        }

        public ISplineStyle style
        {
            get { return _style; }
            set { _style = value; }
        }

        public DecalSpline ParentDecalSpline
        {
            get { return _parentDecalSpline; }
        }

        public virtual void UpdateSegment()
        { }

        public virtual Vector3[] GetCurvePoints()
        { return null; }

        public void Straighten()
        {
            if (!IsLast())
            {
                h1 = Vector3.Lerp(Position, next.Position, 0.1f);
                h2 = Vector3.Lerp(Position, next.Position, 0.9f);
            }
        }

        public void AutoSetHandles()
        {
            if (!IsFirst())
            {
                if (!IsLast())
                {
                    Vector3 tangent = next.Position - prev.Position;
                    tangent.Normalize();

                    float prevLenght = prev.Length;

                    _h1 = Position + tangent * Length * style.Curviness*0.05f;
                    prev.h2 = Position - tangent * prevLenght * style.Curviness * 0.05f;
                }
            }
        }

        private void SetHandles(Vector3 h1, Vector3 h2)
        {
            if (h1 != _h1)
            {
                _h1 = h1;
                if (lockHandles)
                {
                    if (!IsFirst())
                    {
                        float prevHandleLength = Vector3.Distance(prev.h2, Position);
                        Vector3 dir = Position - h1;
                        dir.Normalize();

                        prev._h2 = Position + dir * prevHandleLength;
                    }
                }
            }

            if (h2 != _h2)
            {
                _h2 = h2;
                if (!IsLast())
                {
                    if (next.lockHandles)
                    {
                        float nextHandleLength = Vector3.Distance(next.h1, next.Position);
                        Vector3 dir = next.Position - h2;
                        dir.Normalize();

                        next._h1 = next.Position + dir * nextHandleLength;
                    }
                }
            }
            //UpdateSegment();
        }

        public void DrawGizmos()
        {
            SplineUtility.DrawGizmos(this);
        }

        public ISplineSegment Last
        {
            get
            {
                if (!IsLast())
                {
                    return next.Last;
                }

                return this;
            }
        }

        public ISplineSegment First
        {
            get
            {
                if (!IsFirst())
                {
                    return prev.First;
                }

                return this;
            }
        }

        public void Cut()
        {
            while (!IsLast())
                Last.Remove();

            Remove();
        }

        public void Insert(ISplineSegment segment, bool undoable = false)
        {
            if (segment != null)
            {  
                if (!IsLast())
                {
                    ISplineSegment newSegmentLast = segment.Last;
                    if (undoable)
                        Undo.RecordObject(next, "Inserted Segment");
                    next.prev = newSegmentLast;
                    if (undoable)
                        Undo.RecordObject(newSegmentLast, "Inserted Segment");
                    newSegmentLast.next = next;
                    //newSegmentLast.UpdateSegment();
                    EditorUtility.SetDirty(next);
                    EditorUtility.SetDirty(newSegmentLast);
                }
                if (undoable)
                    Undo.RecordObject(this, "Inserted Segment");
                next = segment;
                if (undoable)
                    Undo.RecordObject(segment, "Inserted Segment");
                segment.prev = this;

                Straighten();
                AutoSetHandles();
               // UpdateSegment();
                EditorUtility.SetDirty(this);
                EditorUtility.SetDirty(segment);
            }
        }

        public void InsertNew(string name ,Vector3 pos,ISplineStyle style)
        {
            ISplineSegment newSegment = null;
            if (style.GetType() == typeof(DecalSplineStyle))
                newSegment = DecalSplineSegment.Spawn(name, (DecalSplineStyle)style, pos, transform.parent,ParentDecalSpline);
            else if (style.GetType() == typeof(MeshSplineStyle))
                newSegment = MeshSplineSegment.Spawn(name, (MeshSplineStyle)style, pos, transform.parent, ParentDecalSpline);
            else if (style.GetType() == typeof(NoneSplineStyle))
                newSegment = NoneSplineSegment.Spawn(name, (NoneSplineStyle)style, pos, transform.parent, ParentDecalSpline);

            Vector3 originalH1 = h1;
            Vector3 originalH2 = h2;
            Insert(newSegment, true);
            newSegment.Straighten();
            newSegment.AutoSetHandles();
            h1 = originalH1;
            newSegment.h2 = originalH2;
        }

        public void Remove(bool undoable = false)
        {
            if (!IsFirst())
            {
                if (undoable)
                    Undo.RecordObject(prev, "prev");
                prev.next = next;
                //prev.UpdateSegment();
                EditorUtility.SetDirty(prev);
            }
            if (!IsLast())
            {
                if (undoable)
                    Undo.RecordObject(next, "next");
                next.prev = prev;
                EditorUtility.SetDirty(next);
            }

            OnRemoved?.Invoke(this, EventArgs.Empty);

            if (undoable)
                Undo.DestroyObjectImmediate(gameObject);
            else
                DestroyImmediate(gameObject);
        }

        public void Replace(ISplineSegment segment)
        {
            if (segment != null)
            {
                if (!IsLast())
                {
                    ISplineSegment newSegmentLast = segment.Last;
                    next.prev = newSegmentLast;
                    newSegmentLast.next = next;
                    //newSegmentLast.UpdateSegment();
                    EditorUtility.SetDirty(next);
                    EditorUtility.SetDirty(newSegmentLast);
                }
                segment._h1 = _h1;
                segment._h2 = _h2;
                segment.lockHandles = lockHandles;
                next = segment;
                segment.prev = this;
                //segment.UpdateSegment();
                EditorUtility.SetDirty(segment);
            }
            Remove();
        }

        public void Append(ISplineSegment segment)
        {
            if (segment != null)
            {
                ISplineSegment last = Last;

                //place the segment fixedlenght away from the previous one if a fixedLength is set.
                if (segment.style.IsFixedLenght())
                {
                    Vector3 dir = (segment.Position - last.Position).normalized;
                    Vector3 newPos = last.Position + dir * segment.style.FixedLength;
                    segment.Position = newPos;
                }


                last.Insert(segment);
                //convert the connecting segment to match the inserted style
                last = Convert(last, segment.style);
                last.style = segment.style;
                //last.UpdateSegment();
            }
        }

        public bool IsLast() 
        {
            return next == null;
        }

        public bool IsFirst() { return prev == null; }

        public int Count
        {
            get
            {
                if (next != null)
                    return next.Count + 1;

                return 1;
            }
        }

        public float Length
        {
            get
            {
                if (!IsLast())
                    return Vector3.Distance(Position, next.Position);
                else
                    return 0;
            }
        }

        public float GetCurveLength()
        {
            float result = 0;
            if (!IsLast())
            {
                Vector3[] points = Handles.MakeBezierPoints(Position, next.Position, h1, h2, 1000);
                for (int i = 0; i < points.Length - 1; i++)
                {
                    result += Vector3.Distance(points[i], points[i + 1]);
                }
            }
            return result;
        }

        public bool IsEnd()
        {
            bool result = false;
            if (IsLast())
                result = true;
            else
            {
                if (next.GetType() != this.GetType())
                    result = true;
            }
            return result;
        }

        public static ISplineSegment Convert(ISplineSegment segment, ISplineStyle convertToStyle)
        {
            ISplineSegment newSegment = null;
            if (convertToStyle.GetType() == typeof(DecalSplineStyle) && segment.GetType() != typeof(DecalSplineSegment))
            {
                newSegment = DecalSplineSegment.Spawn(segment.name, (DecalSplineStyle)convertToStyle, segment.Position, segment.transform.parent, segment.ParentDecalSpline);
            }
            else if (convertToStyle.GetType() == typeof(MeshSplineStyle) && (segment.GetType() != typeof(MeshSplineSegment) || convertToStyle != segment.style))
            {
                newSegment = MeshSplineSegment.Spawn(segment.name, (MeshSplineStyle)convertToStyle, segment.Position, segment.transform.parent, segment.ParentDecalSpline);
            }
            else if (convertToStyle.GetType() == typeof(NoneSplineStyle) && segment.GetType() != typeof(NoneSplineSegment))
            {
                newSegment = NoneSplineSegment.Spawn(segment.name, (NoneSplineStyle)convertToStyle, segment.Position, segment.transform.parent, segment.ParentDecalSpline);
            }

            if (newSegment != null)
            {
                newSegment.lockHandles = segment.lockHandles;

                segment.Replace(newSegment);
                return newSegment;
            }
            return segment;
        }
#endif
    }
}
