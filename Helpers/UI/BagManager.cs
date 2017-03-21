using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI {
	public class BagManager {
		/*
		  This is designed to just keep one bag open at a time. 
		  Will make it more robust. 

		  Bag buttons: 
		  MainMenuBarBackpackButton
		  CharacterBag0Slot
		  CharacterBag1Slot
		  CharacterBag2Slot
		  CharacterBag3Slot

		 */
		private string[] BagButtonNames = new string[]{
				"MainMenuBarBackpackButton",
				"CharacterBag0Slot",
				"CharacterBag1Slot",
				"CharacterBag2Slot",
				"CharacterBag3Slot"
			};

		private int CurrentOpenBag = -1;
		private class BagItem {
			public int bag;
			public int slot;
			public GItem item;
			public BagItem(int b, int s, GItem i) {
				bag = b; slot = s; item = i;
			}
		}

		private GItem[] Items;
		private BagItem[] BagItems;

		/*public BagManager() {
			// make sure all bags are closed

			CloseAllBags();
			UpdateItems();

			// check all contents
		}*/

		public void UpdateItems() {
			List<GItem> ItemList = new List<GItem>();
			List<BagItem> BagItemList = new List<BagItem>();
			long[] AllBags = GPlayerSelf.Me.Bags;

			long[] Contents;
			int SlotCount;

			for (int bag = 0; bag <= 4; bag++) {
				SlotCount = 0;
				if (bag == 0) {
					Contents = GContext.Main.Me.BagContents;
					SlotCount = GContext.Main.Me.SlotCount;
				} else {
					GContainer container = (GContainer)GObjectList.FindObject(AllBags[bag - 1]);
					if (container != null) {
						Contents = container.BagContents;
						SlotCount = container.SlotCount;
					} else
						Contents = null;
				}
				if (Contents != null) {
					for (int i = 0; i < Contents.Length; i++) {
						GItem CurItem = (GItem)GObjectList.FindObject(Contents[i]);
						if (CurItem != null) {
							ItemList.Add(CurItem);
							BagItem bi = new BagItem(bag, SlotCount - i, CurItem);
							BagItemList.Add(bi);
						}
					}
				}
			}


			Items = ItemList.ToArray();
			BagItems = BagItemList.ToArray();
		}

		public void CloseAllBags() {
			foreach (string BagKey in BagButtonNames) {
				GInterfaceObject CurBag = GContext.Main.Interface.GetByName(BagKey);
				if (CurBag != null) {
					if (CurBag.IsFiring) {
						Functions.Click(CurBag);
						//Thread.Sleep(300);
						//PPather.WriteLine("BagManager: Close bag " + BagKey);
					}
				}
			}
			CurrentOpenBag = -1;
		}

		public void OpenBag(int nr) {

			if (nr == CurrentOpenBag) {
				// verify it is open
				GInterfaceObject CurBag = GContext.Main.Interface.GetByName(BagButtonNames[nr]);
				if (CurBag == null || !CurBag.IsFiring) {
					PPather.WriteLine("BagManager: Something is very fishy with the bags. Close them all and try again");
					CloseAllBags();
				}
			}

			if (nr != CurrentOpenBag) {
				CloseAllBags();

				GInterfaceObject CurBag = GContext.Main.Interface.GetByName(BagButtonNames[nr]);
				if (CurBag != null) {
					Functions.Click(CurBag);
					//Thread.Sleep(300);
					CurrentOpenBag = nr;
					//PPather.WriteLine("BagManager: Open bag " + BagButtonNames[nr]);
				}
			}
		}

		//BagNr is 0-4, ItemNr is 1-BagSlots
		public void ClickItem(int BagNr, int ItemNr, bool RightMouse) {
			OpenBag(BagNr);
			//PPather.WriteLine("BagManager: Click item " + BagNr + " " + ItemNr);
			String itemStr = "ContainerFrame1Item" + ItemNr;
			GInterfaceObject ItemObj = GContext.Main.Interface.GetByName(itemStr);
			if (ItemObj != null) {
				Functions.Click(ItemObj, RightMouse);
			}
		}

		public void ClickItem(GItem item, bool RightMouse) {
			UpdateItems();

			// serach for it
			for (int i = 0; i < BagItems.Length; i++) {
				BagItem it = BagItems[i];
				if (it.item == item)
				{
					ClickItem(it.bag, it.slot, RightMouse);
					break;
				}
			}
		}

		public void CastSpellItem(GItem item) {
			ClickItem(item, true);
			Thread.Sleep(500);
			while (GContext.Main.Me.IsCasting) {
			Thread.Sleep(100);
			}
		}

		public GItem[] GetAllItems() {
			UpdateItems();
			return Items;
		}
	}
}
