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

namespace Pather.Helpers
{
	class Inventory
	{
		private static Dictionary<string, int> curItemsCache = null;
		// change this to change the cache time, not sure what would be optimal
		private static GSpellTimer curItemCacheTimer = new GSpellTimer(30000);

		public static void ReadyItemCacheTimer()
		{
			curItemCacheTimer.ForceReady();
		}

		/// <summary>
		/// Retrieves the quantities of all distinct items in all of your bags using
		/// cached data if possible.
		/// </summary>
		/// <returns>
		/// A Dictionary with keys corresponding to the names
		/// of each distinct item in your inventory and values corresponding
		/// to the number of that item across all of your bags.
		/// </returns>
		public static Dictionary<string, int> CreateItemCount()
		{
			return CreateItemCount(true);
		}

		/// <summary>
		/// Retrieves the quantities of all distinct items in all of your bags.
		/// </summary>
		/// <param name="useCache">Whether to use cached data, if possible</param>
		/// <returns>
		/// A Dictionary with keys corresponding to the names
		/// of each distinct item in your inventory and values corresponding
		/// to the number of that item across all of your bags.
		/// </returns>
		public static Dictionary<string, int> CreateItemCount(bool useCache)
		{
			// only check it every 30 seconds to try to reduce
			// overhead
			if (curItemsCache != null && !curItemCacheTimer.IsReady && useCache)
				return curItemsCache;

			Dictionary<string, int> items = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
			long[] AllBags = GPlayerSelf.Me.Bags;

			for (int bagNr = 0; bagNr < 5; bagNr++)
			{
				long[] Contents;
				int SlotCount;
				if (bagNr == 0)
				{
					Contents = GContext.Main.Me.BagContents;
					SlotCount = GContext.Main.Me.SlotCount;
				}
				else
				{
					GContainer bag = (GContainer)GObjectList.FindObject(AllBags[bagNr - 1]);
					if (bag != null)
					{
						Contents = bag.BagContents;
						SlotCount = bag.SlotCount;
					}
					else
					{
						SlotCount = 0;
						Contents = null;
					}
				}
				for (int i = 0; i < SlotCount; i++)
				{
					if (Contents[i] == 0)
						continue;
					GItem CurItem = (GItem)GObjectList.FindObject(Contents[i]);
					if (CurItem != null)
					{
						string ItemName = CurItem.Name;
						int ItemCount = CurItem.StackSize;
						int OldCount = 0;
						items.TryGetValue(ItemName, out OldCount);
						items.Remove(ItemName);
						items.Add(ItemName, OldCount + ItemCount);
					}
				}
			}

			curItemCacheTimer.Reset();
			curItemsCache = items;
			return curItemsCache;
		}

		public static int GetItemCount(string name)
		{
			return GetItemCount(name, true);
		}

		public static int GetItemCount(string name, bool useCache)
		{
			name = name.ToLower();
			int ret = 0;

			CreateItemCount(useCache).TryGetValue(name, out ret);

			return ret;
		}
	}
}
