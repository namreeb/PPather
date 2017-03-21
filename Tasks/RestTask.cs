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
	public class RestTask : ParserTask {
		double RestHealth;
		double RestMana;


		public RestTask(PPather pather, NodeTask node)
			: base(pather, node) {
			RestHealth = GContext.Main.RestHealth;
			RestMana = GContext.Main.RestMana;

			PPather.WriteLine("Rest  health: " + RestHealth + " mana: " + RestMana);
		}
		public override string ToString() {
			return "Resting";
		}

		public override Location GetLocation() {
			return null; // anywhere
		}

		public override void GetParams(List<string> l) {
			base.GetParams(l);
		}

		public override bool IsFinished() {
			return false;
		}


		public override bool WantToDoSomething() {

			if (ppather.FindAttacker() != null) return false; // can't rest while attacked
			if (GContext.Main.Me.IsInCombat) return false;
			bool wantRest = false;
			double hp = GContext.Main.Me.Health;
			if (hp < RestHealth) {
				wantRest = true;
			}
			if (ppather.CanDrink) {
				double mana = GContext.Main.Me.Mana;
				if (mana < RestMana) {
					wantRest = true;
				}
			}
			// TODO: check for safety
			return wantRest;
		}

		public override Activity GetActivity() {
			Activity restTask = new ActivityRest(this);

			return restTask;
		}

		public override bool ActivityDone(Activity task) {
			task.Stop();

			return false; // never done
		}

	}
}
