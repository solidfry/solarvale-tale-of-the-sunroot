using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using TeoGames.Mesh_Combiner.Scripts.Profile;

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	public partial class MeshCombiner {
		private bool _HasRemovals;

		public override void Exclude(AbstractCombinable combinable) {
			_Updates[combinable] = UpdateType.Exclude;
			_HasRemovals = true;

#if DEBUG_BAKING
			Debug.LogError($">>>>> {combinable.name} > Exclude > {name} > {isVisible} > {IsUpdateInProgress} > {_Updates.Count}");
#endif

			if (IsVisible) ScheduleUpdate();
			else CallCombinableExclude(combinable);
		}

		private void CallCombinableExclude(AbstractCombinable comb) {
			if (comb is IVisibilityToglable t) CallCombinableExclude(t);
		}

		private void CallCombinableExclude(IVisibilityToglable togglable) {
			togglable.OnExclude();
			_ToggleUpdates[togglable] = ToggleStatus.Skip;
		}

		protected bool RunExclude(AbstractCombinable combinable) {
			if (!_CombinableToMaterial.TryGetValue(combinable, out var matList)) return false;

			for (var i = 0; i < matList.materials.Length; i++) {
				var material = matList.materials[i];
				var meshKey = matList.cid * 100 + i;
				if (!material.Meshes.ContainsKey(meshKey)) continue;

				material.Meshes.Remove(meshKey);
				material.Updated();
			}

			_CombinableToMaterial.Remove(combinable);
			ProfilerModule.Meshes.Value--;

			return true;
		}
	}
}