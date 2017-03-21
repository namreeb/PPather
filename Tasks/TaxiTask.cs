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
	public class TaxiTask : NPCInteractTask
	{
		bool flied = false;
		public TaxiTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			if (GetLocationOfNPC() == null)
				PPather.WriteLine("*** NPC " + NPC + " is unknown");
		}

		private int GetCooldown()
		{
			return 60 * 1; // 3 minutes
		}

		public override void GetParams(List<string> l)
		{
			l.Add("Destination");
			base.GetParams(l);
		}


		private String GetTaxiDestination()
		{
			Value v = nodetask.GetValueOfId("Destination");
			if (v == null)
				return null;
			return v.GetStringValue();
		}

		public override bool IsFinished()
		{
			// Fly just once
			return flied;
		}

		public override string ToString()
		{
			return "Flying to " + GetTaxiDestination();
		}

		public override void Restart()
		{
			flied = false;
			// ActualKillCount = 0;
		}

		private bool wantTaxi = false;

		private void UpdateWants()
		{
			// Check queue
			wantTaxi = false;

			if (NPC != null && !ppather.IsBlacklisted(NPC))
			{
				Location l = GetLocationOfNPC();
				//PPather.WriteLine("BG NPC loc: " + l);
				//PPather.WriteLine("My dur: " + GetDurabillity());
				//PPather.WriteLine("min dur: " + GetMinDurabillity());

				if (l == null)
				{
				}
				else if (true) /* Might be a better way to figure out if we should or not.. I use lowest $prio */
				//l.GetDistanceTo(new Location(GContext.Main.Me.Location)) < 50.0)
				{
					//PPather.WriteLine("Want to Queue: " + (wantQueue ? "yes" : "no"));
					wantTaxi = true;
				}
			}

		}

		public override Location GetLocation()
		{
			Location loc = null;
			UpdateWants();
			if (wantTaxi)
			{
				loc = GetLocationOfNPC();
			}
			return loc;
		}

		public override bool WantToDoSomething()
		{
			UpdateWants();
			if (wantTaxi)
				return true;
			return false;
		}

		ActivityTaxi taxiActivity;

		public override Activity GetActivity()
		{

			// PPather.WriteLine("Pickup::GetActivity()");
			if (!IsCloseToNPC())
			{
				return GetWalkToActivity();
			}
			else
			{
				if (taxiActivity == null)
				{
					taxiActivity = new ActivityTaxi(this, FindNPC(), GetTaxiDestination());
				}
				return taxiActivity;
			}

		}

		public override bool ActivityDone(Activity task)
		{
			if (task == taxiActivity)
			{
				//ppather.Blacklist(NPC, 15 * 60);
				flied = true;
				return true;
			}
			return false;
		}

	}
}
