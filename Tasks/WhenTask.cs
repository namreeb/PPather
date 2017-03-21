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

using Pather;
using Pather.Activities;
using Pather.Graph;
using Pather.Parser;

namespace Pather.Tasks {
	// A task that is never done
	public class WhenTask : ParserTask {
		Task childTask;
		bool inside = false;
		public WhenTask(PPather pather, NodeTask node)
			: base(pather, node) {
			childTask = pather.CreateTaskFromNode(node.subTasks[0], this);
		}

		public override void GetParams(List<string> l) {
			l.Add("cond");
			base.GetParams(l);
		}

		public override bool IsFinished() {
			return childTask.IsFinished();
		}

		public override Location GetLocation() {
			return childTask.GetLocation();
		}

		public override Task[] GetChildren() {
			return new Task[] { childTask };
		}

		public override void Restart() {
			childTask.Restart(); // restart my baby
		}

		public override bool WantToDoSomething() {
			if(!inside && nodetask.GetBoolValueOfId("cond"))
				inside = true;
			if(inside && !childTask.WantToDoSomething())
				inside = false;
			if(inside)
				return true;
			return false;
		}

		public override Activity GetActivity() {
			return childTask.GetActivity();
		}

		public override bool ActivityDone(Activity task) {
			bool childDone = childTask.ActivityDone(task);
			if (childDone) {
				// TODO hmm, my child is done... cool
			}
			return childDone;
		}


	}
}
