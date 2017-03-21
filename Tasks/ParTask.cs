/*
  This file is part of PPather.

    PPather is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PPather is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with PPather.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.Text;

using Glider.Common.Objects;
using Pather;
using Pather.Activities;
using Pather.Graph;
using Pather.Parser;

namespace Pather.Tasks
{
	public class ParTask : ParserTask
	{
		public const string ParserKeyword = "Par,Parallel";

		SortedList<int, List<Task>> orderedChildTasks = new SortedList<int, List<Task>>();
		bool done = false;


		public ParTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			foreach (NodeTask nt in node.subTasks)
			{
				Task t = pather.CreateTaskFromNode(nt, this);
				if (t != null)
				{
					int prio = nt.GetPrio();
					List<Task> l;
					if (orderedChildTasks.TryGetValue(prio, out l))
					{
						l.Add(t);
					}
					else
					{
						l = new List<Task>();
						l.Add(t);
						orderedChildTasks.Add(prio, l);
					}
					//PPather.WriteLine("Par add prio " + prio + " task " + t);

				}
			}
		}


		public override bool IsFinished()
		{
			if (done)
				return true;
			foreach (List<Task> l in orderedChildTasks.Values)
			{
				foreach (Task t in l)
				{
					if (!t.IsFinished())
						return false;
				}
			}
			this.Unload();
			done = true;
			return true;
		}

		public override void GetParams(List<string> l)
		{
			base.GetParams(l);
		}


		public override Task[] GetChildren()
		{
			List<Task> ts = new List<Task>();
			foreach (int prio in orderedChildTasks.Keys)
			{
				List<Task> l;
				orderedChildTasks.TryGetValue(prio, out l);
				foreach (Task t in l)
				{
					ts.Add(t);
				}
			}
			return ts.ToArray();
		}


		public override void Restart()
		{
			foreach (List<Task> l in orderedChildTasks.Values)
			{
				foreach (Task t in l)
				{
					t.Restart();
				}
			}
			done = false;

		}

		private Task GetBestTask()
		{
			Location meLoc = new Location(GContext.Main.Me.Location);
			Task bestFound = null;
			float bestFoundDistance = 1E30f;
			foreach (int prio in orderedChildTasks.Keys)
			{
				List<Task> l;
				orderedChildTasks.TryGetValue(prio, out l);
				foreach (Task t in l)
				{
					//PPather.WriteLine("Consider " + t);
					// PPather.WriteLine("  f: "  + t.IsFinished() + " wtd " + t.WantToDoSomething());
					if (!t.IsFinished() && t.WantToDoSomething())
					{
						float d = 0;
						Location loc = t.GetLocation();
						if (loc != null)
							d = loc.GetDistanceTo(meLoc);
						if (d < bestFoundDistance)
						{
							bestFound = t;
							bestFoundDistance = d;
						}
					}
				}
				if (bestFound != null)
				{
					return bestFound; // Found one
				}
			}
			return bestFound;
		}

		public override Location GetLocation()
		{
			return GetBestTask().GetLocation();
		}

		public override bool WantToDoSomething()
		{
			Task bestFound = GetBestTask();
			return bestFound != null;
		}

		public override Activity GetActivity()
		{
			Task bestFound = GetBestTask();
			//PPather.WriteLine("par beast task: " + bestFound);
			if (bestFound == null)
				return null; // huh?
			Activity a = bestFound.GetActivity();
			//PPather.WriteLine("par beast a: " + a);
			return a;
		}

		public override bool ActivityDone(Activity task)
		{
			Task bestFound = GetBestTask();
			if (bestFound == null)
				return false; // huh?
			bestFound.ActivityDone(task);
			return false; // I am never done
		}
	}
}
