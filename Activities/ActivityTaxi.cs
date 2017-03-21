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
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;
using Pather.Tasks;
using Pather.Graph;
using Pather.Parser;
using Pather.Helpers.UI;

namespace Pather.Activities
{
	public class ActivityTaxi : Activity
	{
		GUnit npc;
		String destination;

		public ActivityTaxi(Task t, GUnit npc, String destination)
			: base(t, "Taxi")
		{
			this.npc = npc;
			this.destination = destination;
		}

		public override Location GetLocation()
		{
			return null; // I will not move
		}

		public void EnjoyFlight()
		{
			bool isFlying = true;

			while (isFlying)
			{
				GLocation loc1 = GContext.Main.Me.Location, loc2 = GContext.Main.Me.Location;
				Thread.Sleep(5000);
				GContext.Main.Me.Refresh();
				Thread.Sleep(1500);
				loc2 = GContext.Main.Me.Location;

				// if we aren't moving.. we aren't in flight
				if (((loc1.X - loc2.X) == 0) && ((loc1.Y - loc2.Y) == 0) && ((loc1.Z - loc2.Z) == 0)) isFlying = false;
			}

			PPather.WriteLine("Taxi: Taxi should have landed, EnjoyFlight() done!");
		}

		public override bool Do()
		{

			ppather.Face(npc);
			Functions.Interact(npc);

			Thread.Sleep(3000);
			PPather.WriteLine("UI.TaxiFrame.IsVisible:" + (TaxiFrame.IsVisible() ? "yes" : "no"));

			if (TaxiFrame.IsVisible())
			{
				PPather.WriteLine("Taxi: Got Taxi Frame - Time to Fly");
				int buttonId = TaxiFrame.GetButtonId(destination);

				if (buttonId > 0)
				{
					ppather.ResetMyPos();
					TaxiFrame.ClickTaxiButton(buttonId);
					Thread.Sleep(1000);

					EnjoyFlight();
				}
			}

			return true;
		}
	}
}
