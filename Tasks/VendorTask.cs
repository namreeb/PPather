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
	public class VendorTask : NPCInteractTask {
		public VendorTask(PPather pather, NodeTask node)
			: base(pather, node) {
			if (NPC == null || NPC == "") {
				NPC = node.GetValueOfId("RepairNPC").GetStringValue();
				if (NPC == null || NPC == "") {
					NPC = node.GetValueOfId("SellNPC").GetStringValue();
				}
			}
		}


		public override void GetParams(List<string> l) {
			l.Add("MinDurability");
			l.Add("MinFreeBagSlots");
			l.Add("SellGrey"); l.Add("SellGray");
			l.Add("SellWhite"); l.Add("SellGreen");
			l.Add("Protected");
			l.Add("ForceSell");
			l.Add("BlacklistTime");
			base.GetParams(l);
		}

		#region Gets

		private float GetMinDurabillity() {
			Value v = nodetask.GetValueOfId("MinDurability");
			if (v == null) return -1.0f;
			return v.GetFloatValue();
		}

		private float GetDurabillity() {
			return nodetask.GetFloatValueOfId("MyDurability");
		}

		private int GetMinFreeBagslots() {
			Value v = nodetask.GetValueOfId("MinFreeBagSlots");
			if (v == null) return -1;
			return v.GetIntValue();
		}

		private int GetBlacklistTime() {
			Value v = nodetask.GetValueOfId("BlacklistTime");
			if (v == null) return 300;
			int t = v.GetIntValue();
			if (t == 0) t = 300;
			return t;
		}

		private int GetFreeBagslots() {
			return nodetask.GetIntValueOfId("FreeBagSlots");
		}

		private List<string> GetProtectedItems() {
			Value v = nodetask.GetValueOfId("Protected");
			if (v == null) return null;
			List<string> prot = v.GetStringCollectionValues();
			for (int i = 0; i < prot.Count; i++)
				prot[i] = prot[i].ToLower();
			return prot;
		}

		private List<string> GetForceSellItems() {
			Value v = nodetask.GetValueOfId("ForceSell");
			if (v == null) return null;
			List<string> force = v.GetStringCollectionValues();
			for (int i = 0; i < force.Count; i++)
				force[i] = force[i].ToLower();
			return force;
		}

		private bool GetSellGrey() {
			return nodetask.GetBoolValueOfId("SellGrey") || nodetask.GetBoolValueOfId("SellGray");
		}

		private bool GetSellWhite() {
			return nodetask.GetBoolValueOfId("SellWhite");
		}

		private bool GetSellGreen() {
			return nodetask.GetBoolValueOfId("SellGreen");
		}

		#endregion

		public override bool IsFinished() {
			return false;
		}

		private bool wantSell = false;
		private bool wantRepair = false;

		private void UpdateWants() {
			wantRepair = false;
			wantSell = false;

			Location l = GetLocationOfNPC();

			if (l == null) {
			} else {
				bool close = l.GetDistanceTo(new Location(GContext.Main.Me.Location)) < 50.0;
				if ((GetDurabillity() <= GetMinDurabillity()) || close) 
				{
					wantRepair = true;
				}
				if (GetFreeBagslots() <= GetMinFreeBagslots() || close) 
				{
					wantSell = true;
				}
			}
		}

		public override string ToString()
		{
			return "Selling And Repairing at " + NPC.ToString();
		}

		public override bool WantToDoSomething() 
		{
			if (ppather.IsBlacklisted(NPC)) return false;
			UpdateWants();
			if (wantSell || wantRepair) return true;
			return false;
		}

		ActivitySellAndRepair sellActivity;
		public override Activity GetActivity() {

			// PPather.WriteLine("Pickup::GetActivity()");
			if (!IsCloseToNPC()) {
				return GetWalkToActivity();
			} 
				else 
			{
				if (sellActivity == null) 
				{
					sellActivity =
						new ActivitySellAndRepair(this, FindNPC(),
												  GetSellGrey(), GetSellWhite(),
												  GetSellGreen(), GetProtectedItems(), GetForceSellItems());
				}
				return sellActivity;
			}

		}

		public override bool ActivityDone(Activity task) {
			if (task == sellActivity) 
			{
				ppather.Blacklist(NPC, GetBlacklistTime());
				return true;
			}
			return false;
		}
	}
}
