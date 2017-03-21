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
using System.Threading;

using Glider.Common.Objects;
using Pather.Tasks;
using Pather.Helpers;
using Pather.Graph;
using Pather.Helpers.UI;

namespace Pather.Activities {
	public class ActivitySetHome : Activity {
		GUnit npc;

		public ActivitySetHome(Task t, GUnit npc)
			: base(t, "SetHome") {
			this.npc = npc;
		}

		public override Location GetLocation() {
			return null; // I will not move
		}

		public override bool Do() {
			Functions.Interact(npc);
			Thread.Sleep(1000);

			if (GossipFrame.IsVisible()) {
				if (!GossipFrame.ClickOptionText("inn your home"))
					return false;
				Thread.Sleep(2000); // Lag.
				// When I tested it the popup was StaticPopup1. No idea if this changes...
				if (GContext.Main.Interface.GetByName("StaticPopup1").IsVisible) {
					Functions.Click(GContext.Main.Interface.GetByName("StaticPopup1Button1"));
				
				}
				// Should now have hearth set.
			}
			return true;
		}
	}
}
