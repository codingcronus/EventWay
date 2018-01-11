using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventWay.Infrastructure.CosmosDb
{
	public class DocumentDbParallelHelper
	{
		public static async Task RunParallel(int taskCount, List<Task> actions)
		{
			//var taskCount = 0;
			//taskCount = Math.Max(noOfParallelTasks, 1);
			//taskCount = Math.Min(taskCount, 50);
			var tasks = new List<Task>();
			foreach (var action in actions)
			{
				tasks.Add(action);
				if (tasks.Count >= taskCount)
				{
					await Task.WhenAll(tasks);
					tasks.Clear();
				}
			}
			if (tasks.Count > 0)
			{
				await Task.WhenAll(tasks);
				tasks.Clear();
			}
		}
	}
}
