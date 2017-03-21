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
	// similar to when but the cond is inverted and this one is considered done when the condition is true
	public class UntilTask : ParserTask {
		Task childTask;
		public UntilTask(PPather pather, NodeTask node)
			: base(pather, node) {
			if (node.subTasks.Count == 1)
				childTask = pather.CreateTaskFromNode(node.subTasks[0], this);
		}


		public override void GetParams(List<string> l) {
			l.Add("cond");
			base.GetParams(l);
		}

		public override Location GetLocation() {
			if (childTask == null) return null;
			return childTask.GetLocation();
		}

		public override Task[] GetChildren() {
			if (childTask == null) return null;
			return new Task[] { childTask };
		}


		public override bool IsFinished() {
			bool done = nodetask.GetBoolValueOfId("cond");
			if (!done) {
				if (childTask != null && childTask.IsFinished())
					childTask.Restart(); // Try to restart it
			}
			return done || (childTask != null && childTask.IsFinished());
		}

		public override void Restart() {
			childTask.Restart(); // restart my baby
		}

		public override bool WantToDoSomething() {

			bool done = nodetask.GetBoolValueOfId("cond");
			//PPather.WriteLine("Until  cond = " + done);
			if (done)
				return false;

			if (childTask == null) return false;

			bool child = childTask.WantToDoSomething();
			//PPather.WriteLine("Until child is " + child);

			return child;
		}

		public override Activity GetActivity() {
			if (childTask == null) return null;

			return childTask.GetActivity();
		}

		public override bool ActivityDone(Activity task) {

			// cool
			return false;
		}
	}
}
