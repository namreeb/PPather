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
	public abstract class NPCInteractTask : ParserTask {
		public string NPC;
		GUnit npcUnit;
		bool UseMount;
		Location location = null;

		public NPCInteractTask(PPather pather, NodeTask node)
			: base(pather, node) {
			NPC = node.GetValueOfId("NPC").GetStringValue();
			location = node.GetValueOfId("Location").GetLocationValue();
			UseMount = node.GetBoolValueOfId("UseMount");
		}


		public override void GetParams(List<string> l) {
			l.Add("NPC");
			l.Add("Location");
			l.Add("UseMount");
			base.GetParams(l);
		}

		public GUnit FindNPC() {
			if (npcUnit == null || !npcUnit.IsValid)
				npcUnit = GObjectList.FindUnit(NPC);
			return npcUnit;
		}


		public override Location GetLocation() {
			Location loc = GetLocationOfNPC();
			return loc;
		}

		public bool KnowNPCLocation() {
			return GetLocationOfNPC() != null;
		}
		public bool CanSeeNPC() {

			return FindNPC() != null;
		}

		public bool IsCloseToNPC() {
			GUnit npc = FindNPC();
			if (npc == null) return false;
			Location mel = new Location(GContext.Main.Me.Location);
			Location npcl = new Location(npc.Location);

			float d = mel.GetDistanceTo(npcl);
			if (d < GContext.Main.MeleeDistance && Math.Abs(mel.Z - npcl.Z) < 2.0) return true;
			return false;
		}

		public Location GetLocationOfNPC() {
			GUnit npc = FindNPC();
			if (npc != null)
			{
				return new Location(npc.Location);
			} else if (location != null) {
				return location;
			}
			
			return ppather.FindNPCLocation(NPC);
		}

		protected ActivityApproach approachTask;
		protected ActivityWalkTo walkToTask;

		string prevNPC = "";
		public Activity GetWalkToActivity() {

			GUnit npc = FindNPC();
			if (npc == null) {
				Location l = GetLocationOfNPC();
				if (l == null)
					return null;
				if (walkToTask == null || NPC != prevNPC)
					walkToTask = new ActivityWalkTo(this, l, 2f, UseMount);
				prevNPC = NPC;
				return walkToTask;

			} else {
				if (approachTask == null || NPC != prevNPC)
					approachTask = new ActivityApproach(this, npc, 2f, UseMount);
				prevNPC = NPC;
				return approachTask;
			}

		}
	}
}
