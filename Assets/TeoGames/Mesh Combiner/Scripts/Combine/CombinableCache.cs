using System;
using System.Linq;
using TeoGames.Mesh_Combiner.Scripts.BlendShape;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	public enum MeshCorrection {
		None,
		Stat,
		Anim,
	}

	public enum CacheStatus {
		None = 0,
		MeshUpdated = 1,
		Cached = 2,
	}

	[Serializable]
	public class CombinableCache {
		[SerializeField] public Transform transform;

		[SerializeField] public Material[] materials;
		[SerializeField] public Renderer renderer;
		[SerializeField] public Mesh mesh;
		[NonSerialized] public Transform[] Bones;

		[SerializeField] public bool isSkinnedMesh;
		[SerializeField] public MeshRenderer meshRenderer;
		[SerializeField] public MeshFilter meshFilter;
		[SerializeField] public SkinnedMeshRenderer skinnedMeshRenderer;

		[SerializeField] public CacheStatus status = CacheStatus.None;
		[SerializeField] public MeshCorrection isCorrectionRequired = MeshCorrection.None;
		[NonSerialized] public bool IsOptimized = false;

		[SerializeField, HideInInspector] public BlendShapeConfiguration blendShape = new BlendShapeConfiguration();

		public bool IsDifferent(CombinableCache other) =>
			isSkinnedMesh != other.isSkinnedMesh ||
			renderer != other.renderer ||
			mesh != other.mesh ||
			!materials.SequenceEqual(other.materials);
	}
}