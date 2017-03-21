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
	public class ActivityApproach : Activity
	{
		private GUnit monster;
		private EasyMover em;
		private Location to;
		private float howClose;
		private bool UseMount;
		public EasyMover.MoveResult MoveResult = EasyMover.MoveResult.Moving;

		public ActivityApproach(Task t, GUnit monster, float howClose, bool UseMount)
			: base(t, "Approach " + monster.Name)
		{
			this.monster = monster;
			this.howClose = howClose;
			this.UseMount = UseMount;
		}

		public override Location GetLocation()
		{
			return new Location(monster.Location);
		}

		public override void Start()
		{
			// Create a path
			to = GetLocation();
			em = new EasyMover(ppather, to, false, true);
		}

		GSpellTimer tabSpam = new GSpellTimer(600);
		GSpellTimer updateTimer = new GSpellTimer(5000);
		float mountRange = PPather.PatherSettings.MountRange;
		public override bool Do()
		{
			if (em == null)
				return true; // WTF!

			// mount if we're really far away
			if (monster.DistanceToSelf >= mountRange)
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

			if (GContext.Main.Me.Target != monster &&
				tabSpam.IsReady &&
				monster.DistanceToSelf < 50f &&
				monster.Reaction != GReaction.Friendly)
			{
				GContext.Main.SendKey("Common.Target");
				tabSpam.Reset();
			}

			if (GetLocation().GetDistanceTo(to) > GContext.Main.MeleeDistance &&
				monster.DistanceToSelf < 30f || updateTimer.IsReady)
			{
				// need a new path, monster moved
				to = GetLocation();
				em = new EasyMover(ppather, to, false, true);
				updateTimer.Reset();
			}

			MoveResult = em.move(howClose);
			if (MoveResult != EasyMover.MoveResult.Moving)
				return true; // done, can't do more

			Location meLocation = new Location(GContext.Main.Me.Location);
			if (meLocation.GetDistanceTo(to) < howClose)
			{
				PPather.mover.Stop();
				Helpers.Mount.Dismount();
				monster.Face(PPather.PI / 8);
				return true;
			}
			return false;
		}

		public EasyMover.MoveResult GetMoveResult()
		{
			return MoveResult;
		}

		public override void Stop()
		{
			PPather.mover.Stop();
			em = null;

		}
	}
}
