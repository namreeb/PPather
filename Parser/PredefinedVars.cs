using System;
using System.Collections.Generic;
using System.Text;

using Pather.Parser;
using Pather.Helpers.UI;
using Glider.Common.Objects;

namespace Pather.Parser
{
	delegate Value PredefinedVarDelegate();

	/// <summary>
	/// This class contains static methods that directly correspond to
	/// all available pre-defined variables in psc files. The name of the
	/// method will be the name of the variable. All method signatures
	/// must match PredefinedVarDelegate.
	/// </summary>
	static class PredefinedVars
	{
		#region One-Liners
		public static Value MyLevel()
		{
			return new Value(GContext.Main.Me.Level.ToString());
		}

		public static Value MyClass()
		{
			return new Value(GContext.Main.Me.PlayerClass.ToString());
		}

		public static Value MyRace()
		{
			return new Value(GContext.Main.Me.PlayerRace.ToString());
		}

		public static Value MyHealth()
		{
			return new Value(GContext.Main.Me.Health.ToString());
		}

		public static Value MyMana()
		{
			return new Value(GContext.Main.Me.Mana.ToString());
		}

		public static Value MyX()
		{
			return new Value(GPlayerSelf.Me.Location.X.ToString());
		}

		public static Value MyY()
		{
			return new Value(GPlayerSelf.Me.Location.Y.ToString());
		}

		public static Value MyZ()
		{
			return new Value(GPlayerSelf.Me.Location.Z.ToString());
		}

		public static Value MyTarget()
		{
			Dictionary<string, string> TargInfo =
				new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			TargInfo.Add("Health", Helpers.Target.GetTargetAttrValue("Health"));
			TargInfo.Add("HealthPoints", Helpers.Target.GetTargetAttrValue("HealthPoints"));
			TargInfo.Add("HealthMax", Helpers.Target.GetTargetAttrValue("HealthMax"));
			TargInfo.Add("Mana", Helpers.Target.GetTargetAttrValue("Mana"));
			TargInfo.Add("ManaPoints", Helpers.Target.GetTargetAttrValue("ManaPoints"));
			TargInfo.Add("ManaMax", Helpers.Target.GetTargetAttrValue("ManaMax"));
			TargInfo.Add("IsDead", Helpers.Target.GetTargetAttrValue("IsDead"));
			TargInfo.Add("IsPlayer", Helpers.Target.GetTargetAttrValue("IsPlayer"));
			TargInfo.Add("Reaction", Helpers.Target.GetTargetAttrValue("Reaction"));
			TargInfo.Add("Name", Helpers.Target.GetTargetAttrValue("Name"));
			TargInfo.Add("Level", Helpers.Target.GetTargetAttrValue("Level"));
			return new Value(TargInfo);
		}

		public static Value MyCoppers()
		{
			return new Value(GPlayerSelf.Me.Coinage);
		}

		public static Value IsStealthed()
		{
			return new Value((GPlayerSelf.Me.IsStealth) ? 1 : 0);
		}

		public static Value IsInCombat()
		{
			return new Value((GPlayerSelf.Me.IsInCombat) ? 1 : 0);
		}

		public static Value MyEnergy()
		{
			return new Value(GPlayerSelf.Me.Energy.ToString());
		}

		public static Value MyZone()
		{
			return new Value(MacrolessZoneInfo.GetZoneText());
		}

		public static Value MySubZone()
		{
			return new Value(MacrolessZoneInfo.GetSubZoneText());
		}

		public static Value BGQueued()
		{
			return new Value(Tasks.BGQueueTaskManager.IsQueued() ? 1 : 0);
		}

		#endregion

		public static Value ItemCount()
		{
			Dictionary<String, int> items = Pather.Helpers.Inventory.CreateItemCount(false);
			return new Value(items);
		}

		public static Value MyGearType()
		{
			// TODO: should probably allow some way to let the user decide
			// if we should return mail/plate, leather/mail

			String result = "cloth";

			switch (GContext.Main.Me.PlayerClass)
			{
				case GPlayerClass.Hunter:
				case GPlayerClass.Rogue:
				case GPlayerClass.Druid:
					result = "leather";
					break;

				case GPlayerClass.Warrior:
				case GPlayerClass.Paladin:
				case GPlayerClass.Shaman:
					result = "mail";
					break;
			}

			return new Value(result);
		}

		public static Value FreeBagSlots()
		{
			int count = 0;
			int totalslots = 0;
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

				totalslots += SlotCount;
				for (int i = 0; i < SlotCount; i++)
				{
					if (Contents[i] == 0)
						count++;
				}
			}

			return new Value(count);
		}

		public static Value AlreadyTrained()
		{
			bool ret = false;

			int TrainLevel = 0;
			string TrainLevelS = PPather.ToonData.Get("TrainLevel");
			if (TrainLevelS != null && TrainLevelS != "")
				TrainLevel = Int32.Parse(TrainLevelS);
			int mylevel = GContext.Main.Me.Level;

			if (TrainLevel == mylevel) ret = true;

			return new Value(ret ? 1 : 0);
		}

		public static Value MyDurability()
		{
			float worst = 1.0f;

			GItem[] items = GObjectList.GetEquippedItems();

			foreach (GItem item in items)
			{
				if (item.DurabilityMax > 0)
				{
					float dur = (float)item.Durability;
					if (dur < worst) worst = dur;
				}
			}

			return new Value(worst);
		}

		public static Value MyGear()
		{
			Dictionary<String, int> dic = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

			GItem[] items = GObjectList.GetEquippedItems();

			foreach (GItem item in items)
			{
				dic.Add(item.Name, 1);
			}
			return new Value(dic);
		}

		public static Value MyBagNames()
		{
			Dictionary<String, int> dic = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

			long[] AllBags = GPlayerSelf.Me.Bags;
			for (int bag = 1; bag <= 4; bag++)
			{
				GContainer container = (GContainer)GObjectList.FindObject(AllBags[bag - 1]);
				if (container != null)
				{
					string ItemName = container.Name;
					int OldCount = 0;
					dic.TryGetValue(ItemName, out OldCount);
					dic.Remove(ItemName);
					dic.Add(ItemName, OldCount + 1);
				}
			}
			return new Value(dic);
		}

	}
}