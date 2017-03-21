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
	// Fight back attackers
	public class DangerTask : ParserTask
	{
		GUnit monster = null;
		float DangerDistance;

		public DangerTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			DangerDistance = node.GetValueOfId("Distance").GetFloatValue();
			if (DangerDistance == 0)
			{
				// Check if using $DangerDistance
				DangerDistance = node.GetValueOfId("DangerDistance").GetFloatValue();
				if (DangerDistance == 0)
					DangerDistance = 20;
			}
		}

		public override Location GetLocation()
		{
			return null; // anywhere
		}
		public override void GetParams(List<string> l)
		{
			l.Add("Distance");
			base.GetParams(l);
		}

		public override string ToString()
		{
			return "Danger";
		}

		public override bool IsFinished()
		{
			return false;
		}

		GUnit FindMobToPull()
		{
			// TODO better elite/max level decision, this at least prevents
			// attacking flight masters
			int minLevel = nodetask.GetValueOfId("MinLevel").GetIntValue();
			int maxLevel = nodetask.GetValueOfId("MaxLevel").GetIntValue();

			// TODO add active pvp option
			// int doPvp = ...;

			// Find stuff to pull
			GUnit closest = null;
			GMonster[] monsters = GObjectList.GetMonsters();
			foreach (GMonster monster in monsters)
			{
				if (!monster.IsDead &&
					(!monster.IsTagged || monster.IsTargetingMe || monster.IsTargetingMyPet) &&
					!ppather.IsBlacklisted(monster) && !PPather.IsPlayerFaction(monster) &&
					(!monster.IsPlayer /*|| (doPvp && (GPlayer)monster).IsPVP)*/) &&
					!PPather.IsStupidItem(monster))
				{
					double dangerd = (double)DangerDistance + ((monster.IsElite ? 1.25 : 1.0) * (monster.Level - GContext.Main.Me.Level));
					if (monster.Reaction == GReaction.Hostile &&
						!monster.IsElite &&
						minLevel <= monster.Level &&
						monster.Level <= maxLevel &&
						monster.DistanceToSelf < dangerd &&
						Math.Abs(monster.Location.Z - GContext.Main.Me.Location.Z) < 15.0)
					{
						if (closest == null || monster.DistanceToSelf < closest.DistanceToSelf)
						{
							closest = monster;
						}
					}
				}
			}
			return closest;
		}

		public override bool WantToDoSomething()
		{
			GUnit prevMonster = monster;
			monster = FindMobToPull();
			if (monster != prevMonster)
			{
				attackTask = null;

			}
			return monster != null;
		}

		private Activity attackTask = null;
		public override Activity GetActivity()
		{
			if (attackTask == null)
				attackTask = new ActivityAttack(this, monster);

			return attackTask;
		}

		public override bool ActivityDone(Activity task)
		{
			task.Stop();
			return false;
		}
	}
}