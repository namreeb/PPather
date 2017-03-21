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
	// Look for harvestable items and go use them
	public class HarvestTask : ParserTask
	{
		GNode node = null;

		List<String> names;
		bool HarvestFlower = false;
		bool HarvestMineral = false;
		bool HarvestTreasure = false;
		float Distance = 90.0f;
		float HostileDistance;
		bool UseMount;

		int times = 0;
		int harvest_times = 0;

		public override void GetParams(List<string> l)
		{
			l.Add("Names");
			l.Add("Types");
			l.Add("Times");
			l.Add("Distance");
			l.Add("HostileDistance");
			l.Add("UseMount");
			base.GetParams(l);
		}

		public HarvestTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			Value v_names = node.GetValueOfId("Names");

			if (v_names != null)
			{
				names = v_names.GetStringCollectionValues();
			}


			Value v_types = node.GetValueOfId("Types");

			List<Value> types_list = v_types.GetCollectionValue();
			foreach (Value v in types_list)
			{
				string s = v.GetStringValue();
				if (s == "Herb" || s == "Flower")
				{
					HarvestFlower = true;
					PPather.WriteLine("Want to harvest flowers");

				}
				if (s == "Mine" || s == "Mineral")
				{
					HarvestMineral = true;
					PPather.WriteLine("Want to harvest minerals");
				}
				if (s == "Chest" || s == "Treasure")
				{
					HarvestTreasure = true;
					PPather.WriteLine("Want to harvest chests");
				}
			}
			times = node.GetValueOfId("Times").GetIntValue();

			Distance = node.GetValueOfId("Distance").GetFloatValue();
			if (Distance == 0.0f)
				Distance = 90.0f;

			HostileDistance = node.GetValueOfId("HostileDistance").GetFloatValue();

			UseMount = node.GetBoolValueOfId("UseMount");
		}


		public override string ToString()
		{
			String s = "Harvesting ";
			if (node != null)
				s += node.Name;
			return s;
		}

		public override bool IsFinished()
		{
			if (times <= 0)
				return false;
			return harvest_times >= times;
		}

		public override Location GetLocation()
		{
			if (node != null)
				return new Location(node.Location);
			return null;
		}

		private bool IsSafe(GNode node)
		{
			if (HostileDistance == 0.0f)
				return true;
			List<GMonster> monsters = ppather.CheckForMobsAtLoc(node.Location, HostileDistance);
			if (monsters.Count > 0)
				return false;
			return true;
		}

		private bool ShouldHarvest(GNode u)
		{
			if ((u.IsMineral && HarvestMineral ||
				u.IsFlower && HarvestFlower ||
				u.IsTreasure && HarvestTreasure ||
				names != null && names.Contains(u.Name)) && IsSafe(u))
			{
				return !ppather.IsBlacklisted(u.GUID);
			}
			return false;
		}

		private void DidHarvest(GNode u)
		{
			ppather.Blacklist(u.GUID, 5 * 60); // 5 minutes
			harvest_times++;
		}

		GNode FindItemToHarvest()
		{
			// Find stuff to loot
			GNode closest = null;
			GNode[] nodes = GObjectList.GetNodes();
			foreach (GNode node in nodes)
			{
				if (ShouldHarvest(node))
				{
					if (closest == null || node.DistanceToSelf < closest.DistanceToSelf)
					{
						closest = node;
					}
				}
			}
			return closest;
		}

		public override bool WantToDoSomething()
		{
			GNode prevNode = node;
			node = FindItemToHarvest();
			if (prevNode != null && node == null)
			{
				// hmmm, lost a node
				PPather.WriteLine("Lost " + prevNode.Name + ". blacklist");

				ppather.Blacklist(prevNode.GUID, 60); // ban for 1 mins
			}

			if (node != prevNode || prevNode == null || node == null)
			{
				// new target
				lootTask = null;
				walkTask = null;
			}
			if (walkTask != null)
			{
				// check result of walking
				if (walkTask.MoveResult != EasyMover.MoveResult.Moving &&
					walkTask.MoveResult != EasyMover.MoveResult.GotThere)
				{
					PPather.WriteLine("Can't reach " + node.Name + ". blacklist. " + walkTask.MoveResult);
					ppather.Blacklist(node.GUID, 60 * 10);
					return false;
				}
			}

			// at my pos and at target pos
			if (node != null && !ppather.IsItSafeAt(null, node.Location))
			{
				PPather.WriteLine("Unsafe at " + node.Name + ".");
				ppather.Blacklist(node.GUID, 60 * 10); // 10 mins
				node = null;
			}
			if (node != null && !ppather.IsItSafeAt(null, GContext.Main.Me))
			{
				node = null;
			}
			return node != null;
		}

		private Activity lootTask = null;
		private ActivityWalkTo walkTask = null;
		public override Activity GetActivity()
		{
			// check distance
			if (node.DistanceToSelf < 5.0 && Math.Abs(node.Location.Z - GContext.Main.Me.Location.Z) < 5.0)
			{
				if (walkTask != null)
					walkTask.Stop();
				if (lootTask == null)
					lootTask = new ActivityPickup(this, node);
				walkTask = null;
				return lootTask;
			}
			else
			{
				// walk over there
				if (walkTask == null)
				{
					walkTask = new ActivityWalkTo(this, new Location(node.Location), 2f, UseMount);
					PPather.WriteLine("new walk task to node " + node.Name);
				}
				else
				{
					// check status of mover
				}
				lootTask = null;
				return walkTask;
			}
		}

		public override bool ActivityDone(Activity task)
		{
			if (task == lootTask)
			{
				DidHarvest(node);
				node = null;
			}

			task.Stop();
			return false;
		}
	}
}
