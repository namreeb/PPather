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
using Pather.Tasks;
using System.Collections;

namespace Pather.Helpers
{
	class TaskInfo
	{
		private static Task root;
		private static List<Task> tasklist;

		public static Task Root
		{
			get
			{
				return root;
			}
			set
			{
				root = value;
			}
		}

		public static List<Task> TaskList
		{
			get
			{
				return tasklist;
			}
		}

		private void FindChildren(Task parent)
		{
			tasklist.Add(parent);
			if (parent.GetChildren() != null)
			{
				foreach (Task child in parent.GetChildren())
				{
					FindChildren(child);
				}
			}
		}

		private void Populate()
		{
			tasklist = new List<Task>();
			tasklist.Clear();
			foreach (Task parent in root.GetChildren())
			{
				this.FindChildren(parent);
			}
		}

		public Task[] GetTasks()
		{
			Populate();
			Task[] Tasks = (Task[])tasklist.ToArray();
			return Tasks;
		}
	}
}