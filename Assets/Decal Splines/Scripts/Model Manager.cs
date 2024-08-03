using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DecalSplines
{
    public class ModelManager : MonoBehaviour
    {
#if UNITY_EDITOR
        [HideInInspector][SerializeField] private List<Transform> bones;
        [HideInInspector][SerializeField] private Mesh mesh;
        [HideInInspector][SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
        [HideInInspector][SerializeField] private MeshCollider meshcollider;
        [HideInInspector][SerializeField] private MeshSplineSegment segment;

        public static ModelManager Spawn(MeshSplineSegment segment)
        {
            GameObject curveModelObject = Instantiate(segment.style.Prefab.gameObject);
            Undo.RegisterCreatedObjectUndo(curveModelObject, "Created Model Object");
            curveModelObject.name = segment.name + " Model";
            curveModelObject.transform.position = segment.transform.position;
            curveModelObject.transform.rotation = segment.transform.parent.rotation;
            Undo.SetTransformParent(curveModelObject.transform, segment.transform, "Set parent of model");
            //curveModelObject.transform.parent = segment.transform;

            ModelManager manager = Undo.AddComponent<ModelManager>(curveModelObject);
            //ModelManager manager = curveModelObject.AddComponent<ModelManager>();
            manager.segment = segment;
            manager.skinnedMeshRenderer = curveModelObject.GetComponentInChildren<SkinnedMeshRenderer>();
            manager.skinnedMeshRenderer.updateWhenOffscreen = true;
            manager.mesh = new Mesh();
            manager.meshcollider = curveModelObject.GetComponentInChildren<MeshCollider>();

            //Reset and configure the decal curveModel.
            manager.bones = new List<Transform>();
            manager.FindBones();
            manager.UpdateModels();

            return manager;
        }

        private void FindBones()
        {
            bones.Clear();
            foreach (Transform child in transform)
            {
                int i = 0;
                Transform potentialBone = child.Find("C" + i.ToString());
                while (potentialBone != null)
                {
                    bones.Add(potentialBone);
                    i++;
                    potentialBone = child.Find("C" + i.ToString());
                }
                if (bones.Count > 0)
                    break;
            }
        }


        //Prepare and update the model
        public void UpdateModels()
        {
            if (segment != null)
            {
                if (segment.IsLast())
                    gameObject.SetActive(false);
                else
                    gameObject.SetActive(true);
                transform.position = segment.transform.position;
                UpdateModelCurves();
                UpdateCollider();
            }
        }

        private void UpdateCollider()
        {
            if (meshcollider != null)
            {
                skinnedMeshRenderer.BakeMesh(mesh);
                meshcollider.sharedMesh = mesh;
            }
        }

        private void UpdateModelCurves()
        {
            if (segment != null)
            {
                Vector3[] points = segment.GetCurvePoints();
                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 pos = points[i] - transform.parent.localPosition;
                    bones[i].localPosition = pos;

                    Vector3 d1 = Vector3.zero;
                    Vector3 d2 = Vector3.zero;
                    if (i == 0)
                    {
                        Vector3 h1 = segment.h1 - transform.parent.localPosition;
                        d1 = new Vector3(pos.x, 0, pos.z);
                        d2 = new Vector3(h1.x, 0, h1.z);

                        if (d1 == d2 && i + 1 < points.Length)
                        {
                            Vector3 nextPos = points[i + 1] - transform.parent.localPosition;
                            d2 = new Vector3(nextPos.x, 0, nextPos.z);
                        }
                    }
                    else if (i + 1 < points.Length)
                    {
                        Vector3 nextPos = points[i + 1] - transform.parent.localPosition;
                        d1 = new Vector3(pos.x, 0, pos.z);
                        d2 = new Vector3(nextPos.x, 0, nextPos.z);
                    }
                    else
                    {
                        Vector3 h2 = segment.h2 - transform.parent.localPosition;
                        d2 = new Vector3(pos.x, 0, pos.z);
                        d1 = new Vector3(h2.x, 0, h2.z);
                    }

                    Vector3 dir = (d2 - d1).normalized;

                    if (dir != Vector3.zero)
                    {
                        bones[i].localRotation = Quaternion.LookRotation(dir);
                        bones[i].transform.Rotate(90, 0, 0);
                    }
                }

            }
        }
#endif
    }
}