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
	public class ActivitySellAndRepair : Activity
	{
		GUnit npc;
		bool SellGrey;
		bool SellWhite;
		bool SellGreen;
		List<string> Protected = null;
		List<string> Forced = null;
		public ActivitySellAndRepair(Task t, GUnit npc,
									 bool SellGrey, bool SellWhite, bool SellGreen,
									 List<string> Protected, List<string> Forced)
			: base(t, "SellRepair")
		{
			this.npc = npc;
			this.SellGrey = SellGrey;
			this.SellWhite = SellWhite;
			this.SellGreen = SellGreen;
			this.Protected = Protected;
			this.Forced = Forced;
		}

		public override Location GetLocation()
		{
			return null; // I will not move
		}

		private bool ShouldSell(GItem CurItem)
		{
			string ItemName = CurItem.Name.ToLower();
			if (Forced != null)
			{
				foreach (string ForceSell in Forced)
				{
					if (ItemName.Equals(ForceSell))
					{
						return true; // sell
					}
				}
			}
			if (CurItem.Definition.Quality == GItemQuality.Rare ||
				CurItem.Definition.Quality == GItemQuality.Epic ||
				CurItem.Definition.Quality == GItemQuality.Legendary) // Yeah right
				return false;
			if (Protected != null)
			{
				foreach (string ProtItem in Protected)
				{
					if (ItemName.Contains(ProtItem.ToLower()))
					{
						return false; // do not sell
					}
				}
			}
			// Can't sell soulbound items
			if (CurItem.IsSoulbound)
				return false;

			if ((CurItem.Definition.Quality == GItemQuality.Poor && SellGrey) ||
				(CurItem.Definition.Quality == GItemQuality.Common && SellWhite) ||
				(CurItem.Definition.Quality == GItemQuality.Uncommon && SellGreen))
				return true; // Sell

			return false; // don't sell it
		}

		public override bool Do()
		{
			Helpers.Mount.Dismount();

			ppather.Face(npc);

			while (true)
			{
				Functions.Interact(npc);
				Thread.Sleep(2000);

				if (GossipFrame.IsVisible())
				{
					PPather.WriteLine("Vendor: Got a gossip frame");
					if (!GossipFrame.ClickOptionText("browse your"))
						return false;
					Thread.Sleep(2000);
				}

				if (MerchantFrame.IsVisible())
				{
					GMerchant Merchant = new GMerchant();

					BagManager bm = new BagManager();

					GItem[] items = bm.GetAllItems();
					foreach (GItem item in items)
					{
						if (ShouldSell(item))
						{
							bm.ClickItem(item, true);
							Thread.Sleep(500); // extra delay
						}
					}

					if (Merchant.IsRepairEnabled)   // Might as well fix it up while we're here.  
					{
						PPather.WriteLine("Vendor: Repairing");
						Functions.ClickRepairButton(Merchant);
					}

					Functions.Closeit(Merchant);
				}
				else
					GContext.Main.SendKey("Common.Escape"); // Close whatever frame popped up
				return true;
			}

		}
	}
}
