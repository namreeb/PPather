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
using Pather.Tasks;
using Pather.Helpers;
using Pather.Graph;
using Pather.Helpers.UI;

/*
 * Contributed by Tim
 */

namespace Pather.Activities
{
	public class ActivityBuy : Activity
	{
		GUnit npc;
		List<BuySet> BuySets;

		public ActivityBuy(Task t, GUnit npc, List<BuySet> BuySets)
			: base(t, "Buy")
		{
			this.npc = npc;
			this.BuySets = BuySets;
		}

		public override Location GetLocation()
		{
			return null; // I will not move
		}


		void GetShoppingList(out string[] itemsOut, out int[] quantitiesOut)
		{
			List<string> itemsToBuy = new List<string>();
			List<int> quantitiesToBuy = new List<int>();

			foreach (BuySet set in BuySets)
			{
				if (Inventory.GetItemCount(set.ITEM) <= Convert.ToInt16(set.BUYAMOUNT))
				{
					itemsToBuy.Add(set.ITEM);
					quantitiesToBuy.Add(Convert.ToInt16(set.BUYAMOUNT));
					PPather.WriteLine("Buy: Going to buy [" + set.ITEM + "]x" + (Convert.ToInt16(set.BUYAMOUNT) - Inventory.GetItemCount(set.ITEM)));
				}
			}

			itemsOut = itemsToBuy.ToArray();
			quantitiesOut = quantitiesToBuy.ToArray();
		}


		public override bool Do()
		{
			Helpers.Mount.Dismount();

			while (true)
			{
				Functions.Interact(npc);
				Thread.Sleep(1000);

				if (GossipFrame.IsVisible())
				{
					//PPather.WriteLine("Buy: Got a gossip frame");
					if (GossipFrame.ClickOptionText("browse your") ||
						GossipFrame.ClickOptionText("Sid") ||
                        GossipFrame.ClickOptionText("What do you have"))
						Thread.Sleep(2000); // Lag
				}

				if (MerchantFrame.IsVisible())
				{
					GMerchant Merchant = new GMerchant();

					string[] toBuy;
					int[] toBuyQuantities;
					GetShoppingList(out toBuy, out toBuyQuantities);

					GSpellTimer sanity = new GSpellTimer(5000);

					for (int i = 0; i < toBuy.Length; i++)
					{
						sanity.Reset();

						int lastCount = Inventory.GetItemCount(toBuy[i], false);
						int curCount = lastCount;

						do
						{
							GContext.Main.EnableCursorHook();
							if (!Merchant.BuyOnAnyPage(toBuy[i]))
							{
								PPather.WriteLine("!Info:Buy: Unable to buy [" + toBuy[i] + "], skipping");
								break;
							}
							Thread.Sleep(2000);

							curCount = Inventory.GetItemCount(toBuy[i], false);

							// inventory went up so reset the timer
							if (curCount > lastCount)
							{
								sanity.Reset();
							}

							// PPather.WriteLine(string.Format("  {0} -> {1} of {2}", lastCount, curCount, toBuyQuantities[i]));
							lastCount = curCount;

							if (sanity.IsReady)
							{
								PPather.WriteLine("!Info:Buy: Waiting too long buying [" + toBuy[i] + "], skipping");
								break;
							}
						} while (curCount < toBuyQuantities[i]);
						GContext.Main.DisableCursorHook();
					}

					if (Merchant.IsRepairEnabled)   // Might as well fix it up while we're here.  
					{
						PPather.WriteLine("Buy: Repairing");
						Functions.ClickRepairButton(Merchant);
					}

					Functions.Closeit(Merchant);
				}
				else
				{
					PPather.WriteLine("!Info: Never got merchant frame");
					GContext.Main.SendKey("Common.Escape"); // Close whatever frame popped up
				}
				return true;
			}

		}
	}
}
