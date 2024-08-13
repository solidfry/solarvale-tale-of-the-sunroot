using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeoGames.Mesh_Combiner.Scripts.Extension;
using TeoGames.Mesh_Combiner.Scripts.Profile;
using UnityEngine;
using UnityEngine.Serialization;

namespace TeoGames.Mesh_Combiner.Scripts.Combine.ChunkContainer {
	[Serializable]
	public class UpdateQueue<T, I> where T : AbstractCombinable where I : UpdateQueueItem {
		[FormerlySerializedAs("maxTimePerFrame")]
		[Tooltip("Maximum time that Update Queue can take in each frame for updating mesh position")]
		public float maxUpdateTimePerFrame = .5f;

		[Tooltip("Maximum time that Update Queue can take in each frame for processing new meshes")]
		public float maxInitTimePerFrame = 5f;

		[Tooltip("Distance between last update position and current position to trigger container check")]
		public float updateDistance = .5f;

		[Tooltip("Max amount of ticks before forcing container check")]
		public int forceUpdateAfter = 100;

		public bool delayedUpdate;

		private readonly Dictionary<T, I> _List = new Dictionary<T, I>();

		private readonly Dictionary<T, UpdateType> _Updates = new Dictionary<T, UpdateType>();

		public bool IsStarted { get; protected set; }
		public int Count => _List.Count;
		public long Ticks { get; protected set; }
		private IChunkContainer<T, I> _Owner;
		private Timer _UpdateTimer = new Timer();
		private Timer _InitTimer = new Timer();

		public void Clear() {
			_Updates.Clear();
			_List.Clear();
			IsStarted = false;
			Ticks = 0;
		}

		public void Start(IChunkContainer<T, I> owner) {
			_UpdateTimer.MaxExecTime = maxUpdateTimePerFrame;
			_InitTimer.MaxExecTime = maxInitTimePerFrame;

			_Owner = owner;
			IsStarted = true;

			UpdateList().Forget();
		}

		public void Stop() {
			IsStarted = false;
		}

		public void Schedule(T combinable) => _Updates[combinable] = UpdateType.Include;

		public void Remove(T combinable) => _Updates[combinable] = UpdateType.Exclude;

		private async Task UpdateList() {
			Ticks = 0;
			var initialState = Application.isPlaying;
			await Task.Yield();

			while (IsStarted && _Owner != null && initialState == Application.isPlaying) {
				try {
					await ApplyUpdates();

					_UpdateTimer.Start();
					var i = 0;
					var cpy = new Dictionary<T, I>(_List);
					foreach (var pair in cpy) {
						if (i++ % 10 == 0 && _UpdateTimer.IsTimeoutRequired) {
							await _UpdateTimer.Wait();
							if (!IsStarted) return;
						}

						ProcessCombinable(pair.Key, pair.Value);
					}

					_UpdateTimer.Stop();
					Ticks++;

					await Task.Yield();
				} catch (Exception ex) {
					Debug.LogException(ex);
				}
			}
		}

		private async Task ApplyUpdates() {
			if (!_Updates.Any()) return;

			_InitTimer.Start();
			var i = 0;
			var cpy = new Dictionary<T, UpdateType>(_Updates);
			_Updates.Clear();

			if (delayedUpdate) {
				await Task.Yield();
				await Task.Yield();
				await Task.Yield();
			}

			foreach (var pair in cpy) {
				if (i++ % 10 == 0 && _InitTimer.IsTimeoutRequired) {
					await _InitTimer.Wait();
					if (!IsStarted) return;
				}

				try {
					var action = pair.Value;
					var comb = pair.Key;
					if (action == UpdateType.Include && comb && comb.gameObject.activeInHierarchy) {
						var item = _Owner.IncludeNew(comb);
						if (comb.IsStatic) continue;

						item.transform = comb.transform;
						item.position = item.transform.position;
						_List[comb] = item;
					} else _List.Remove(comb);
				} catch (Exception ex) {
					Debug.LogException(ex);
				}
			}

			_InitTimer.Stop();
		}

		private void ProcessCombinable(T combinable, I item) {
			if (!combinable || !combinable.gameObject.activeInHierarchy) {
				Remove(combinable);
				return;
			}

			var pos = item.transform.position;
			if (item.ticks < forceUpdateAfter && Vector3.Distance(item.position, pos) < updateDistance) {
				item.ticks++;
				return;
			}

			try {
				_Owner.UpdateDynamic(combinable, item);
				item.ticks = 0;
				item.position = pos;
			} catch (Exception ex) {
				Debug.LogException(ex);
			}
		}
	}
}