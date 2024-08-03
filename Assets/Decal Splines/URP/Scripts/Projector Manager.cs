using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace DecalSplines
{
    //Class used to manage the decal projector attached to the segment.
    public class ProjectorManager : MonoBehaviour
    {
#if UNITY_EDITOR
        [HideInInspector][SerializeField] private List<DecalProjector> projectors;
        [HideInInspector][SerializeField] private DecalSplineSegment segment;

        public static ProjectorManager Spawn(DecalSplineSegment segment)
        {
            GameObject projectorsObject = new GameObject();
            Undo.RegisterCreatedObjectUndo(projectorsObject, "Created projectors");
            projectorsObject.name = segment.name + " Projectors";
            projectorsObject.transform.position = segment.transform.parent.position;
            projectorsObject.transform.rotation = segment.transform.parent.rotation;

            Undo.SetTransformParent(projectorsObject.transform, segment.transform, "Add projector manager to segment transform");
            //projectorsObject.transform.parent = segment.transform;

            ProjectorManager manager = Undo.AddComponent<ProjectorManager>(projectorsObject);  
            //ProjectorManager manager = projectorsObject.AddComponent<ProjectorManager>();
            manager.segment = segment;

            manager.projectors = new List<DecalProjector>();

            return manager;
        }

        public DecalProjector GetFirstProjector()
        {
            if (projectors.Count > 0)
            {
                return projectors[0];
            }
            return null;
        }

        public DecalProjector GetLastProjector()
        {
            if (projectors.Count > 0)
            {
                int lastIndex = projectors.Count - 1;
                return projectors[lastIndex];
            }
            return null;
        }

        private DecalProjector SpawnProjector(Vector3 pos, Vector3 nextPos, DecalSplineStyle style, Transform parent)
        {
            if (segment != null)
            {
                GameObject projectorObject = new GameObject();
                Undo.RegisterCreatedObjectUndo(projectorObject, "Created projector");
                projectorObject.name = "Projector " + parent.childCount.ToString();
                Undo.SetTransformParent(projectorObject.transform, parent, "Added projector object to parent transform");
                //projectorObject.transform.parent = parent;
                projectorObject.transform.localPosition = pos;

                DecalProjector projector = Undo.AddComponent<DecalProjector>(projectorObject);
                //DecalProjector projector = projectorObject.AddComponent<DecalProjector>();
                //Reset and configure the decal projector.
                projector.renderingLayerMask = segment.ParentDecalSpline.RenderLayerMask;
                projector.pivot = Vector3.zero;
                projector.size = new Vector3(style.Width, Vector3.Distance(pos, nextPos), segment.ParentDecalSpline.ProjectionDepth);
                projector.scaleMode = DecalScaleMode.ScaleInvariant;
                projector.material = style.Material;


                return projector;
            }
            return null;
        }

        private void FindProjectors()
        {
            projectors.Clear();
            foreach (Transform child in transform)
            {
                DecalProjector proj;
                if (child.TryGetComponent<DecalProjector>(out proj))
                {
                    projectors.Add(proj);
                }
            }
        }

        private void FillProjectors()
        {
            if (segment != null)
            {
                FindProjectors();
                RemoveUnusedProjectors();
                Vector3[] points = segment.GetCurvePoints();
                if (points.Length > 0 && !segment.IsLast())
                {
                    //Find the required projector count and size the projectors list respectively.
                    int requiredAmount = points.Length - 1;
                    int difference = requiredAmount - projectors.Count;
                    if (difference < 0)
                        TrimProjectors(projectors, difference);
                    else if (difference > 0)
                    {
                        for (int i = 0; i < difference; i++)
                            projectors.Add(SpawnProjector(points[0], points[0], segment.style, transform));
                    }
                }
            }
        }

        private void TrimProjectors(List<DecalProjector> projectorList , int trimAmount)
        {
            List<DecalProjector> accessProjectors = projectorList.GetRange(0, -trimAmount);
            foreach (DecalProjector proj in accessProjectors)
            {
                if (proj != null)
                    DestroyImmediate(proj.gameObject);

            }
            projectorList.RemoveRange(0, -trimAmount);
        }


        private void RemoveUnusedProjectors()
        {
            if (segment.IsLast() && projectors.Count > 0)//Destroy projectors if segment is last.
            {
                foreach (DecalProjector proj in projectors)
                {
                    if (proj != null)
                    {
                        DestroyImmediate(proj.gameObject);
                    }
                }
                projectors = new List<DecalProjector>();
            }
        }

        public void Destroy()
        {
            DestroyImmediate(gameObject);
        }

        //Prepare and update the decal projector
        public void UpdateProjectors()
        {
            if (segment != null)
            {
                transform.position = segment.transform.parent.position;
                FillProjectors();
                UpdateMaterialsAndMasks();
                UpdateProjectorBounds();
                FillGaps();
            }
        }

        private void UpdateMaterialsAndMasks()
        {
            if (segment != null)
            {
                foreach (DecalProjector projector in projectors)
                {
                    projector.material = segment.style.Material;
                    projector.renderingLayerMask = segment.ParentDecalSpline.RenderLayerMask;
                }
            }
        }

        //Size compensation of the projectors used to cover gaps that would otherwise be present.
        private void FillGaps()
        {
            if (segment != null && projectors.Count > 0)
            {
                if (!segment.style.IsFixedLenght())
                {
                    for (int i = 0; i < projectors.Count; i++)
                    {
                        DecalProjector projector1 = null;
                        DecalProjector projector2 = projectors[i];

                        if (i == 0)
                        {
                            if (!segment.IsFirst())
                            {
                                if (segment.prev.GetType() == typeof(DecalSplineSegment))
                                {
                                    DecalSplineSegment prevSegment = (DecalSplineSegment)segment.prev;
                                    prevSegment.DisconnectLastProjector();
                                    DisconnectFirstProjector();
                                    projector1 = prevSegment.GetLastProjector();
                                }
                            }
                        }
                        else
                            projector1 = projectors[i - 1];

                        FillGap(projector1, projector2);

                        //Connect the last projector to the next segment, needed to connect segment if only this segment is updated.
                        if (i == projectors.Count - 1)
                        {
                            if (!segment.IsLast() && !segment.next.IsLast())
                            {
                                if (segment.next.GetType() == typeof(DecalSplineSegment))
                                {
                                    DecalSplineSegment nextSegment = (DecalSplineSegment)segment.next;
                                    projector1 = projectors[i];
                                    projector2 = nextSegment.GetFirstProjector();
                                    nextSegment.DisconnectFirstProjector();
                                    FillGap(projector1, projector2);
                                }
                            }
                        }
                    }
                }
            }
        }

        //Find the gap between the bottom right corner of projector 1 and the bottom left corner of projector 2.
        private void FillGap(DecalProjector projector1, DecalProjector projector2)
        {
            if (projector1 != null && projector2 != null)
            {
                if (projector1.size.x == projector2.size.x)
                {
                    float width = projector1.size.x;
                    float height = projector1.size.y;

                    float gap = FindGapSize(projector1, projector2, width);
                    //Add a tiny overlap onto gap
                    gap += 0.0025f;

                    //Resize projector1 to cover half the gap
                    projector1.size += Vector3.up * gap;
                    Vector3 pivot = new Vector3(0, projector1.size.y * .5f, 0);
                    projector1.pivot = pivot;

                    //Resize and move projector2 to cover half the gap
                    projector2.transform.Translate(new Vector3(0, -gap, 0), Space.Self);
                    projector2.size += Vector3.up * gap;
                    pivot = new Vector3(0, projector2.size.y * .5f, 0);
                    projector2.pivot = pivot;

                    //Offset the texture so it continues
                    Vector2 uvOffset = projector1.uvBias;
                    uvOffset.y += projector1.uvScale.y;
                    uvOffset.y %= 1;
                    projector2.uvBias = uvOffset;
                }
            }
        }

        //find the gap size by rotating 'a' by half the angle between edge1 and edge2. edge1/2 are two intersect at origin.
        private float FindGapSize(DecalProjector projector1, DecalProjector projector2, float projectorWidth)
        {
            //float halveAngle = math.abs((projector2.transform.localRotation.eulerAngles.y - projector1.transform.localRotation.eulerAngles.y) * 0.5f);
            float width = projector1.size.x;
            float height = projector1.size.y;

            Vector3 bottomLeft = new Vector3(width * .5f, 0, 0);

            Vector3 edge1 = projector1.transform.localRotation * bottomLeft;
            Vector3 edge2 = projector2.transform.localRotation * bottomLeft;
            float halveAngle = math.abs(Vector3.Angle(edge1, edge2) * 0.5f);

            Vector3 a = new Vector3(projectorWidth * .5f, 0, 0);
            Vector3 c = Quaternion.Euler(0, 0, halveAngle) * a;
            float scaler = a.x / c.x;
            c *= scaler;

            return c.y;
        }
        

        public void DisconnectLastProjector()
        {
            if (segment != null)
            {
                DecalProjector lastProjector = GetLastProjector();
                Vector3[] points = segment.GetCurvePoints();
                if (points.Length > 1 && lastProjector != null)
                {
                    int lastIndex = points.Length - 1;

                    Vector3 pos = points[lastIndex - 1];
                    Vector3 nextPos = points[lastIndex];
                    Vector3 compPos = lastProjector.transform.localPosition;
                    float leftComp = Vector3.Distance(pos, compPos);//The compensation that needs to remain.

                    Vector3 next2D = new Vector3(nextPos.x, 0, nextPos.z);
                    Vector3 pos2D = new Vector3(pos.x, 0, pos.z);

                    float height = Vector3.Distance(pos2D, next2D);

                    //Resize the projector to remove the size portion that connected the segment to the next.
                    Vector3 size = lastProjector.size;
                    size.y = height + leftComp;
                    lastProjector.size = size;

                    Vector3 pivot = new Vector3(0, lastProjector.size.y * .5f, 0);
                    lastProjector.pivot = pivot;
                }
            }
        }

        public void DisconnectFirstProjector()
        {
            if (segment != null && projectors.Count > 0)
            {
                DecalProjector firstProjector = projectors[0];
                Vector3[] points = segment.GetCurvePoints();
                if (points.Length > 1 && firstProjector != null)
                {
                    Vector3 pos = points[0];
                    Vector3 nextPos = points[1];
                    Vector3 compPos = firstProjector.transform.localPosition;
                    float leftComp = Vector3.Distance(pos, compPos);//the compensation that need to be removed

                    firstProjector.transform.Translate(new Vector3(0, leftComp, 0), Space.Self);

                    //Resize the projector to remove the size portion that connected the segment to the previous one.
                    Vector3 size = firstProjector.size;
                    size.y = size.y - leftComp;
                    firstProjector.size = size;

                    Vector3 pivot = new Vector3(0, firstProjector.size.y * .5f, 0);
                    firstProjector.pivot = pivot;
                }
            }
        }

        //Calculate and set the projector size and pivot based on the segment style.
        private void UpdateProjectorBounds()
        {
            if (segment != null)
            {
                Vector3[] points = segment.GetCurvePoints();
                for (int i = 0; i < points.Length - 1; i++)
                {
                    Vector3 pos = points[i];
                    Vector3 nextPos = points[i + 1];

                    projectors[i].transform.localPosition = pos;

                    float width = segment.style.Width;
                    Vector3 next2D = new Vector3(nextPos.x, 0, nextPos.z);
                    Vector3 pos2D = new Vector3(pos.x, 0, pos.z);

                    float height = Vector3.Distance(pos2D, next2D);

                    Vector3 size = new Vector3(width, height, segment.ParentDecalSpline.ProjectionDepth);
                    projectors[i].size = size;

                    Vector3 pivot = new Vector3(0, height * .5f, 0);
                    projectors[i].pivot = pivot;


                    Vector3 dir = (next2D - pos2D).normalized;

                    if (dir != Vector3.zero)
                    {
                        //Rotate the segment to project parallel to curve section.
                        projectors[i].transform.localRotation = Quaternion.LookRotation(dir);
                        projectors[i].transform.Rotate(90, 0, 0);
                    }

                    if (!segment.style.IsFixedLenght())
                    {
                        float ratioSize = height / width;
                        Vector2 textureSize = segment.style.GetTextureSize();
                        float ratioTex = textureSize.x / textureSize.y;

                        projectors[i].uvScale = new Vector2(1, ratioSize * ratioTex);
                    }
                }
            }
        }
#endif
    }
}
