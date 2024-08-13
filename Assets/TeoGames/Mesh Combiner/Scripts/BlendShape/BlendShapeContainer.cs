using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TeoGames.Mesh_Combiner.Scripts.BlendShape {
	[Serializable]
	public class BlendShapeContainer {
		[FormerlySerializedAs("BlendShape")] public Dictionary<string, Dictionary<int, BlendShapeFrame>> blendShape =
			new Dictionary<string, Dictionary<int, BlendShapeFrame>>();

		[FormerlySerializedAs("LiveSync")] public Dictionary<string, (int, Transform)> liveSync = new Dictionary<string, (int, Transform)>();

		public int length;

		public void Clear() {
			length = 0;
			blendShape.Clear();
			liveSync.Clear();
		}

		public void Extend(BlendShapeContainer container) {
			foreach (var pair in container.blendShape) {
				if (!blendShape.TryGetValue(pair.Key, out var existing)) {
					existing = new Dictionary<int, BlendShapeFrame>();
					blendShape[pair.Key] = existing;
				}

				foreach (var frame in pair.Value) existing[frame.Key + length] = frame.Value;
			}

			foreach (var sync in container.liveSync) liveSync[sync.Key] = sync.Value;

			length += container.length;
		}

		public BlendShapeContainer Export(BlendShapeConfiguration conf, Transform renTransform, int id) {
			if (!conf.enabled) return this;

			var res = new BlendShapeContainer() { length = length };
			var syncCnt = conf.liveSync.Count;

			if (conf.merge) {
				res.blendShape = blendShape;
				for (var i = 0; i < syncCnt; i++) res.liveSync[conf.liveSync[i]] = (i, renTransform);
			} else {
				var prefix = $"[{id}] ";
				foreach (var pair in blendShape) res.blendShape[prefix + pair.Key] = pair.Value;
				for (var i = 0; i < syncCnt; i++) res.liveSync[prefix + conf.liveSync[i]] = (i, renTransform);
			}

			return res;
		}
	}
}