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
	// Look for lootables and go loot them
	public class LootTask : ParserTask
	{
		GUnit monster = null;
		List<string> ignoreSkin;
		bool Skin = false;
		float Distance = 30f;
		bool UseMount;

		public LootTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			Value or = node.GetValueOfId("Skin");
			Skin = or.GetBoolValue();

			Distance = node.GetValueOfId("Distance").GetFloatValue();
			if (Distance == 0.0f)
				Distance = 30.0f;
			UseMount = node.GetBoolValueOfId("UseMount");

			this.ignoreSkin = node.GetValueOfId("IgnoreSkin").GetStringCollectionValues();

		}

		public override void GetParams(List<string> l)
		{
			l.Add("Skin");
			l.Add("Distance");
			l.Add("IgnoreSkin");
			base.GetParams(l);
		}

		public override string ToString()
		{
			String s = "Looting ";
			if (monster != null)
				s += monster.Name;
			return s;
		}

		public override bool IsFinished()
		{
			return false;
		}

		public override Location GetLocation()
		{
			if (monster != null)
				return new Location(monster.Location);
			return null;
		}

		private bool ShouldLoot(GUnit u)
		{
			float d = u.DistanceToSelf;
			if (d > Distance)
				return false;
			bool looted = ppather.IsBlacklisted(u.GUID);  // hmm, blacklisted mob
			// Check for safety
			return !looted;
		}

		private void DidLoot(GUnit u)
		{
			ppather.Blacklist(u.GUID, 5 * 60);
		}

		GUnit FindMobToLoot()
		{
			// Find stuff to loot
			GUnit closest = null;
			GMonster[] monsters = GObjectList.GetMonsters();
			foreach (GMonster monster in monsters)
			{
				if ((monster.IsLootable || (monster.IsSkinnable && Skin)) &&
					 ShouldLoot(monster))
				{
					if (closest == null || monster.DistanceToSelf < closest.DistanceToSelf)
					{
						closest = monster;
					}
				}
			}
			return closest;
		}

		public override bool WantToDoSomething()
		{
			if (GObjectList.GetNearestAttacker(0) != null)
				return false;

			GUnit prevMonster = monster;
			monster = FindMobToLoot();
			if (monster != prevMonster)
			{
				lootTask = null;
				walkTask = null;
			}

			if (walkTask != null)
			{
				// check result of walking
				if (walkTask.MoveResult != EasyMover.MoveResult.Moving &&
					walkTask.MoveResult != EasyMover.MoveResult.GotThere)
				{
					PPather.WriteLine("Can't reach " + monster.Name + ". blacklist. " + walkTask.MoveResult);
					ppather.Blacklist(monster);
					return false;
				}
			}


			// at my pos and at target pos
			if (monster != null && !ppather.IsItSafeAt(null, monster))
			{
				PPather.WriteLine("It is not safe at: " + monster.Name);
				DidLoot(monster); // ignore that              
				monster = null;
			}
			if (monster != null && !ppather.IsItSafeAt(null, GContext.Main.Me))
			{
				monster = null;
			}

			return monster != null;
		}

		private Activity lootTask = null;
		private ActivityApproach walkTask = null;
		public override Activity GetActivity()
		{

			// override a true to skin with false if it's in the ignorelist
			bool shouldSkin = (this.ignoreSkin.Contains(monster.Name)) ? false : Skin;
			// check distance
			if (monster.DistanceToSelf < 5.0)
			{
				if (walkTask != null)
					walkTask.Stop();
				if (lootTask == null)
					lootTask = new ActivityLoot(this, monster, shouldSkin);
				walkTask = null;
				return lootTask;
			}
			else
			{
				// walk over there
				if (walkTask == null)
					walkTask = new ActivityApproach(this, monster, 2f, UseMount);
				lootTask = null;
				return walkTask;
			}
		}

		public override bool ActivityDone(Activity task)
		{
			if (task == lootTask)
			{
				DidLoot(monster);
				monster = null;
			}

			task.Stop();
			return false; // never done
		}

	}
}
