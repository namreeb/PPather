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
	public class ActivityLoot : Activity
	{
		GUnit monster;
		bool Skin;
		public ActivityLoot(Task t, GUnit monster, bool Skin)
			: base(t, "Loot " + monster.Name)
		{
			this.monster = monster;
			this.Skin = Skin;
		}

		public override Location GetLocation()
		{
			return null; // I will not move
		}

		public override bool Do()
		{
			if (monster.IsLootable)
			{
				Functions.Interact(monster);

				GSpellTimer bop = new GSpellTimer(500);
				do
				{
					for (int i = 1; i <= 4; i++)
					{
						if (Popup.IsVisible(i))
						{
							String text = Popup.GetText(i);
							PPather.WriteLine("Loot: Got a loot popup ('" + text + "')");
							if (text == "Looting this item will bind it to you.")
							{
								Popup.ClickButton(i, 1);
							}
							else
							{
								Popup.ClickButton(i, 2);
							}
						}
					}
				} while (!bop.IsReadySlow);
				Thread.Sleep(500);
			}



			/*Skinning start*/
			if (Skin)
			{
				int attempt = 1;
				GSpellTimer futile = new GSpellTimer(3000);
				futile.Reset(); // is 3s enough for skinning to be done?!
				PPather.WriteLine("Want to skin");
				while (monster.IsLootable && !futile.IsReadySlow)
					; //Waiting for looting to finish
				Thread.Sleep(1000);

				// Check the type of the cursor
				if (!monster.IsCursorOnUnit)
					Functions.Hover(monster);

				PPather.WriteLine("Loot: Cursor Type is: " + GContext.Main.Interface.CursorType);
				PPather.WriteLine("Loot: Skinnable: " + monster.IsSkinnable);

				// TODO what cursot types are good? 12=skin, ??=mine,  ??=herb
				while (monster.IsValid && monster.IsSkinnable && attempt <= 3)
				{
					PPather.WriteLine("Loot: Skin. Attempt #" + attempt);
					Functions.Interact(monster);
					Thread.Sleep(500);
					if (GContext.Main.Me.IsInCombat)
						return false;
					bool casted = false;
					while (GContext.Main.Me.IsCasting)
					{
						casted = true;
						Thread.Sleep(1500);
					}
					if (!casted)
					{
						// looks bad
						string message = GContext.Main.RedMessage;
						if (message.StartsWith("Requires "))
						{
							// missing skill or too low
							PPather.WriteLine("Loot: Skill too low to skin: " + message);
							break;
						}
						PPather.WriteLine("Loot: Never entered casting after skin attempt");
						break;

					}
					if (GContext.Main.Me.IsInCombat)
						return false;

					attempt++;
					Thread.Sleep(800);
					if (GPlayerSelf.Me.Target == null)
						break; // corpse gone, done
				}
			}
			/*Skinning end*/
			ppather.Looted(monster);

			// Use default postloot key. If undefined does nothing so don't need to check
			// whether it actually contains a value
			GContext.Main.SendKey("Common.PostLoot");

			return true;
		}
	}
}
