using TeoGames.Mesh_Combiner.Scripts.Combine.MeshRendererManager;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.ChunkContainer {
	[AddComponentMenu("Mesh Combiner/Chunk/MC Bounds Chunk Container")]
	public class BoundsChunkContainer : SingleCombinerChunkContainer {
		[SerializeField]
		[Tooltip(
			"If combinable isn't inside collider, then it will be combined if it's closest one compared to other containers"
		)]
		private bool acceptClosest = true;

		public Bounds bounds;

		private Vector3 _Position;

		public override void Init(
			TargetRendererType rendererTypes,
			int maxBuildTime,
			bool bakeMaterials,
			bool separateBlendShapes,
			bool clearMaterialCache
		) {
			base.Init(rendererTypes, maxBuildTime, bakeMaterials, separateBlendShapes, clearMaterialCache);

			_Position = transform.position;
		}

		public override float Distance(Vector3 position) {
			var dif = position - _Position;
			var closest = bounds.ClosestPoint(dif) + _Position;

			return Vector3.Distance(closest, position);
		}

		public override float Compability(AbstractCombinable obj) {
			var dist = Distance(GetPosition(obj));

			return dist < .1f ? 1 : (acceptClosest ? -dist : 0);
		}

		public void RecalculateBounds() => bounds = gameObject.GetBounds();

		private void Reset() => RecalculateBounds();
	}
}