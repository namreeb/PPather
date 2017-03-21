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
using Pather;
using Pather.Activities;
using Pather.Graph;
using Pather.Parser;
using Pather.Helpers;

/*
 * Contributed by Tim
 */
namespace Pather.Tasks
{

	public class BuySet
	{
		private string item;
		private string minAmount;
		private string buyAmount;


		public string ITEM
		{
			get
			{
				return item;
			}
		}
		public string MINAMOUNT
		{
			get
			{
				return minAmount;
			}
		}
		public string BUYAMOUNT
		{
			get
			{
				return buyAmount;
			}
		}

		public BuySet(string item, string minAmount, string buyAmount)
		{
			this.item = item;
			this.minAmount = minAmount;
			this.buyAmount = buyAmount;
		}
	}

	public class BuyTask : NPCInteractTask
	{

		public List<BuySet> BuySets;

		public BuyTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			if (NPC == null || NPC == "")
			{
				NPC = node.GetValueOfId("BuyNPC").GetStringValue();
			}

			BuySets = new List<BuySet>();
			Value sets = node.GetValueOfId("Items");
			List<Value> sets_list = sets.GetCollectionValue();
			foreach (Value v in sets_list)
			{
				BuySet set = v.GetBuySetValue();
				BuySets.Add(set);
			}
		}

		public override void GetParams(List<string> l)
		{
			l.Add("Items");
			l.Add("BlacklistTime");
			base.GetParams(l);
		}

		private int GetBlacklistTime()
		{
			Value v = nodetask.GetValueOfId("BlacklistTime");
			if (v == null)
				return 300;
			int t = v.GetIntValue();
			if (t == 0)
				t = 300;
			return t;
		}

		public override bool IsFinished()
		{
			return false;
		}

		public override string ToString()
		{
			return "Buying From " + NPC.ToString();
		}

		private bool NeedToBuy()
		{
            Location l = GetLocationOfNPC();
			foreach (BuySet set in BuySets)
			{
				if (Inventory.GetItemCount(set.ITEM) <= Convert.ToInt16(set.MINAMOUNT))
				{
					return true;
				}
                if (l != null)
                {
                    bool close = l.GetDistanceTo(new Location(GContext.Main.Me.Location)) < 50.0;
                    if (close && (Inventory.GetItemCount(set.ITEM) < Convert.ToInt16(set.BUYAMOUNT)))
                    {
                        return true;
                    }
                }
			}

			return false;
		}

		private bool wantBuy = false;

		private void UpdateWants()
		{
			wantBuy = false;
			if (NeedToBuy())
			{
				wantBuy = true;
			}
		}

		public override bool WantToDoSomething()
		{
			if (ppather.IsBlacklisted(NPC))
				return false;
			UpdateWants();
			if (wantBuy)
				return true;
			return false;
		}

		ActivityBuy buyActivity;
		public override Activity GetActivity()
		{
			if (!IsCloseToNPC())
			{
				return GetWalkToActivity();
			}
			else
			{
				if (buyActivity == null)
				{
					buyActivity =
						new ActivityBuy(this, FindNPC(), BuySets);
				}
				return buyActivity;
			}

		}

		public override bool ActivityDone(Activity task)
		{
			if (task == buyActivity)
			{
				ppather.Blacklist(NPC, GetBlacklistTime());
				return true;
			}
			return false;
		}
	}
}
