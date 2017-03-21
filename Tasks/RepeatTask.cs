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
	public class RepeatTask : ParserTask
	{
		public const string ParserKeyword = "Rep,Repeatable,Repeat";

		Task child;
		public RepeatTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			child = pather.CreateTaskFromNode(node.subTasks[0], this);
		}

		public override void GetParams(List<string> l)
		{
			base.GetParams(l);
		}

		public override void Restart()
		{
			child.Restart();
		}

		public override bool IsFinished()
		{
			return false; // never finished!
		}

		public override Task[] GetChildren()
		{
			return new Task[] { child };
		}

		public override bool WantToDoSomething()
		{
			if (child != null)
			{
				if (child.IsFinished())
				{
					child.Restart();
				}
				return child.ShouldSchedule();
			}
			return false;
		}

		public override Location GetLocation()
		{
			return child.GetLocation();
		}

		public override Activity GetActivity()
		{
			return child.GetActivity();
		}

		public override bool ActivityDone(Activity task)
		{
			return child.ActivityDone(task);
		}
	}
}