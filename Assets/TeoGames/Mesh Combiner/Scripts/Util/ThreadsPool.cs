using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeoGames.Mesh_Combiner.Scripts.Extension;

namespace TeoGames.Mesh_Combiner.Scripts.Util {
	public class ThreadsPool {
		private readonly List<(Func<bool> validate, Func<Task> task)> _Queue = new List<(Func<bool>, Func<Task>)>();

		public bool HasTasks => _Queue.Any();

		public async Task<bool> Schedule(Func<bool> validate, Func<Task> action) {
			var promise = new TaskCompletionSource<bool>();
			var hadTasks = HasTasks;

			_Queue.Add(
				(
					validate,
					async () => {
						await action();
						promise.SetResult(true);
					}
				)
			);

			if (!hadTasks) {
				await Task.Yield();
				RunNext().Forget();
			}

			return await promise.Task;
		}

		private async Task RunNext() {
			while (true) {
				var next = _Queue.FirstOrDefault(IsValid);
				if (next.task == null) {
					if (_Queue.Any()) await Task.Yield();
					else return;
				} else {
					try {
						await next.task();
					} finally {
						_Queue.Remove(next);
					}
				}
			}
		}

		private static bool IsValid((Func<bool> validate, Func<Task> task) row) => row.validate();
	}
}