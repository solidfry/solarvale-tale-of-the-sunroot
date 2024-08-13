using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using TeoGames.Mesh_Combiner.Scripts.Extension;

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	public partial class ChunkMeshCombiner : IPostBakeAction {
		public void PostBakeAction() {
			updateQueue.Clear();
			if (IsLodReady) Lod.Clear();

			chunks.ForEach(
				c => {
					if (c is IPostBakeAction pb) pb.PostBakeAction();
				}
			);
		}
	}
}