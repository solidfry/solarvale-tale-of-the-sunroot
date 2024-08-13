using System.Collections.Generic;
using System.Linq;
using TeoGames.Mesh_Combiner.Scripts.Combine.Interfaces;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	using UpdatesType = Dictionary<AbstractCombinable, UpdateType>;

	public partial class MeshCombiner : ICombinerVisibilityTogglable {
		[SerializeField, HideInInspector] private bool isVisible = true;

		public bool IsVisible
		{
			get => isVisible;
			set
			{
				if (isVisible == value) return;

				isVisible = value;
				if (Application.isPlaying) {
					if (value) OnBecomeVisible();
					else OnBecomeInvisible();
					Lod.Combiner.chunk.IsVisible = value;
				}
			}
		}

		private void OnBecomeVisible() {
#if DEBUG_BAKING
			Debug.LogError($"----- {name} > OnBecomeVisible > {IsUpdateInProgress} > {_Updates.Count}");
#endif

			if (_Updates.Any() && this) {
				if (IsUpdateInProgress) {
					ToggleRenderers(true);
					ScheduleUpdate(true);
				} else {
					ScheduleUpdate(_Updates.CopyAndClear());
				}

				var cpy = _ToggleUpdates
					.Where(p => p.Value == ToggleStatus.Skip)
					.ToArray();

				UpdateTask.ContinueWith(
					() => {
#if DEBUG_BAKING
						Debug.LogError($"----- {name} > ClearChangeHistory > {IsUpdateInProgress} > {_Updates.Count}");
#endif

						cpy.ForEach(a => _ToggleUpdates.Remove(a.Key));
					}
				);
			} else {
				ToggleRenderers(true);
			}
		}

		private void ToggleVisibility(UpdatesType list) {
			list.ForEach(
				u => {
					if (u.Key is not IVisibilityToglable t) return;

#if DEBUG_BAKING
					Debug.LogError($"----- {u.Key.name} > ToggleVisibility > {name} > {IsUpdateInProgress}");
#endif
					if (u.Value == UpdateType.Include) CallCombinableInclude(t);
					else CallCombinableExclude(t);
				}
			);
		}

		private void ToggleRenderers(bool status) {
			GetRenderers().ForEach(r => r.gameObject.SetActive(status));
		}

		private void OnBecomeInvisible() {
#if DEBUG_BAKING
			Debug.LogError($"----- {name} > OnBecomeInvisible > {IsUpdateInProgress} > {_Updates.Count}");
#endif

			ToggleVisibility(_Updates);
			ToggleRenderers(false);
		}
	}
}