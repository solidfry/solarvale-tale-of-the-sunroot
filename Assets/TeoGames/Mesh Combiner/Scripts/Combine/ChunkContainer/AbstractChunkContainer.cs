using System;
using System.Threading.Tasks;
using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using TeoGames.Mesh_Combiner.Scripts.Combine.MeshRendererManager;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.ChunkContainer {
	public abstract partial class AbstractChunkContainer : MonoBehaviour, IAsyncCombiner, IPostBakeAction, IRenderListener {
		public bool SeparateBlendShapes { get; protected set; }

		public bool ClearMaterialCache { get; protected set; }

		public bool BakeMaterials { get; protected set; }

		public int MaxBuildTime { get; protected set; }

		public TargetRendererType RendererTypes { get; protected set; }
		public Action<Renderer[]> OnRenderersUpdated { get; set; }

		public abstract float Compability(AbstractCombinable obj);
		public abstract float Distance(Vector3 position);
		public abstract void Clear();

		public abstract Renderer[] GetRenderers();

		public abstract string GetKey(AbstractCombinable combinable);

		public abstract void Include(AbstractCombinable combinable, string key);

		public abstract void Exclude(AbstractCombinable combinable, string key);

		public abstract void PostBakeAction();

		public abstract Task UpdateTask { get; }

		public virtual void Init(
			TargetRendererType rendererTypes,
			int maxBuildTime,
			bool bakeMaterials,
			bool separateBlendShapes,
			bool clearMaterialCache
		) {
			RendererTypes = rendererTypes;
			MaxBuildTime = maxBuildTime;
			BakeMaterials = bakeMaterials;
			SeparateBlendShapes = separateBlendShapes;
			ClearMaterialCache = clearMaterialCache;
		}

		protected MeshCombiner CreateMeshCombiner(string combinerName, Transform parent = null) {
			var obj = (parent ? parent : transform).gameObject;
			obj.name = $"__SKIP__{obj.name}";
			var combiner = obj.AddComponent<MeshCombiner>();
			combiner.keys = new[] { $"Chunk {combinerName}" };
			combiner.rendererTypes = RendererTypes;
			combiner.maxBuildTime = MaxBuildTime;
			combiner.bakeMaterials = BakeMaterials;
			combiner.separateBlendShapes = SeparateBlendShapes;
			combiner.clearMaterialCache = ClearMaterialCache;
			combiner.OnRenderersUpdated = OnRenderersUpdated;
			combiner.Init();
			obj.name = obj.name.Substring(8);

			return combiner;
		}

		protected Vector3 GetPosition(AbstractCombinable combinable) {
			return combinable.GetCache().renderer.bounds.center;
		}
	}
}