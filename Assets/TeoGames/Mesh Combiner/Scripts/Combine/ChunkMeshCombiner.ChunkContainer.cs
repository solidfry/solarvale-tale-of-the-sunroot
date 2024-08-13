using TeoGames.Mesh_Combiner.Scripts.Combine.ChunkContainer;
using UnityEngine;

namespace TeoGames.Mesh_Combiner.Scripts.Combine {
	using QueueType = UpdateQueue<AbstractCombinable, UpdateQueueItem>;

	public partial class ChunkMeshCombiner : IChunkContainer<AbstractCombinable, UpdateQueueItem> {
		[SerializeField] public QueueType updateQueue = new QueueType();

		public UpdateQueueItem IncludeNew(AbstractCombinable combinable) {
			var cell = FindNewCell(combinable);
			var key = string.Empty;

			if (cell) {
				key = cell.GetKey(combinable);
				cell.Include(combinable, key);
			}

			return Instances[combinable] = new UpdateQueueItem { container = cell, containerKey = key };
		}

		public UpdateQueueItem UpdateDynamic(AbstractCombinable combinable, UpdateQueueItem item) {
			var cell = item.container;
			if (cell) {
				if (Mathf.Abs(cell.Compability(combinable) - 1) < .01f) {
					item.UpdateKey(combinable);
					return null;
				}

				var newCell = FindNewCell(combinable);
				if (newCell == cell) return null;

				cell.Exclude(combinable, item.containerKey);
				cell = newCell;
			} else {
				cell = FindNewCell(combinable);
			}

			if (cell) {
				item.container = cell;
				item.containerKey = cell.GetKey(combinable);
				cell.Include(combinable, item.containerKey);
			}

			return Instances[combinable] = item;
		}
	}
}