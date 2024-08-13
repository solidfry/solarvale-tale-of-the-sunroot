using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.ChunkContainer {
	[AddComponentMenu("Mesh Combiner/Chunk/MC Quad Chunk Container")]
	public class QuadChunkContainer : GridChunkContainer {
		public override string GetKey(AbstractCombinable combinable) {
			return (GetPosition(combinable) / size).Round().ToString();
		}
	}
}