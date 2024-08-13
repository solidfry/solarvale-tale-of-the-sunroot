using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.ChunkContainer {
	[AddComponentMenu("Mesh Combiner/Chunk/MC Collider Chunk Container")]
	public class ColliderChunkContainer : SingleCombinerChunkContainer {
		[SerializeField]
		[Tooltip(
			"If combinable isn't inside collider, then it will be combined if it's closest one compared to other containers"
		)]
		private bool acceptClosest = true;

		[SerializeField] private UnityEngine.Collider col;

		public override float Distance(Vector3 position) {
			var closest = col.ClosestPoint(position);
			return Vector3.Distance(closest, position);
		}

		public override float Compability(AbstractCombinable obj) {
			var dist = Distance(GetPosition(obj));

			return dist < .1f ? 1 : (acceptClosest ? -dist : 0);
		}
	}
}