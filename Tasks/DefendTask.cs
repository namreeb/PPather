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
	// Fight back attackers
	public class DefendTask : ParserTask {
		GUnit monster = null;


		public DefendTask(PPather pather, NodeTask node)
			: base(pather, node) {
		}

		public override Location GetLocation() {
			return null; // anywhere
		}

		public override void GetParams(List<string> l) {
			base.GetParams(l);
		}

		public override string ToString() {
			return "Defend";
		}

		public override bool IsFinished() {
			return false;
		}
		public override bool WantToDoSomething() {
			monster = ppather.FindAttacker();
			return monster != null;
		}

		public override Activity GetActivity() {
			Activity attackTask = new ActivityAttack(this, monster);

			return attackTask;
		}

		public override bool ActivityDone(Activity task) {
			task.Stop();

			return false; // never done
		}
	}
}
