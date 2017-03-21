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

namespace Pather.Tasks {
	public class SeqTask : ParserTask {
		public const string ParserKeyword = "Seq,Sequence";

		List<Task> sequence;
		int currentSequence = 0;
		public SeqTask(PPather pather, NodeTask node)
			: base(pather, node) {
			sequence = new List<Task>();
			foreach (NodeTask nt in node.subTasks) {
				Task t = pather.CreateTaskFromNode(nt, this);
				if (t != null) {
					sequence.Add(t);
				}
			}
			currentSequence = 0;
		}

		public override void GetParams(List<string> l) {
			base.GetParams(l);
		}

		public override Task[] GetChildren() {
			return sequence.ToArray();
		}

		private void PickChild() {
			if (currentSequence < sequence.Count) {
				do {
					Task child = sequence[currentSequence];
					if (child.IsFinished()) {
						//PPather.WriteLine("Seq " + currentSequence + " is done. " + " " + child);
						currentSequence++;
					} else
						return;
				} while (currentSequence < sequence.Count);
			}
		}
		public override void Restart() {
			currentSequence = 0;
			foreach (Task t in sequence) {
				t.Restart();
			}
		}
		public override bool IsFinished() {
			PickChild();
			if (currentSequence >= sequence.Count) 
			{
				this.Unload();
				return true;
			}
			return false;
		}
		public override bool WantToDoSomething() {
			PickChild();
			if (currentSequence >= sequence.Count) return false; // all done
			return sequence[currentSequence].WantToDoSomething();
		}

		public override Location GetLocation() {
			PickChild();
			return sequence[currentSequence].GetLocation();
		}

		public override Activity GetActivity() {
			PickChild();
			if (currentSequence >= sequence.Count) {
				PPather.WriteLine("all seqeuences done. no activity");
				return null; // doh!
			}
			Activity a = sequence[currentSequence].GetActivity();
			//PPather.WriteLine("seq a " + currentSequence + " : " + a);
			return a;
		}

		public override bool ActivityDone(Activity task) {
			bool childDone = sequence[currentSequence].ActivityDone(task);

			PickChild();

			return currentSequence >= sequence.Count;
		}
	}
}
