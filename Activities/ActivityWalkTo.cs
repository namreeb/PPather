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

namespace Pather.Activities
{
	public class ActivityWalkTo : Activity
	{
		private Location to;
		private EasyMover em;
		private float howClose;
		private bool UseMount;
		public EasyMover.MoveResult MoveResult = EasyMover.MoveResult.Moving;

		public ActivityWalkTo(Task t, Location to, float howClose, bool UseMount)
			: base(t, "Walk to " + to)
		{
			this.to = to;
			this.howClose = howClose;
			this.UseMount = UseMount;
		}

		public override Location GetLocation()
		{
			return to;
		}

		public override void Start()
		{
			// Create a path
			em = new EasyMover(ppather, to, false, true);
		}

		public override bool Do()
		{
			if (em == null)
				return true; // WTF!

			Location meLocation = new Location(GContext.Main.Me.Location);
			float distanceToDestination = meLocation.GetDistanceTo(to);
			float mountRange = PPather.PatherSettings.MountRange;

			// mount if it's far enough away
			if (distanceToDestination >= mountRange)
			{
				if (PPather.PatherSettings.UseMount != "Never Mount")
				{
					if (PPather.PatherSettings.UseMount == "Always Mount" ||
						(PPather.PatherSettings.UseMount == "Let Task Decide" &&
						UseMount == true))
					{
						Helpers.Mount.MountUp();
					}
				}
			}
			MoveResult = em.move(howClose);
			if (MoveResult != EasyMover.MoveResult.Moving)
				return true; // done, can't do more

			if (distanceToDestination < howClose && Math.Abs(meLocation.Z - to.Z) < howClose)
			{
				// we're here so dismount, if we weren't mounted it doesn't matter
				//Helpers.Mount.Dismount();

				PPather.mover.Stop();
				return true;
			}

			return false;
		}

		public override void Stop()
		{
			PPather.mover.Stop();
			em = null;

		}
	}
}
