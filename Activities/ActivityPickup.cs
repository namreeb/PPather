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
	public class ActivityPickup : Activity
	{
		GNode node;
		public ActivityPickup(Task t, GNode node)
			: base(t, "Pickup " + node.Name)
		{
			this.node = node;
		}

		public override Location GetLocation()
		{
			return null; // I will not move
		}

		public override bool Do()
		{
			Helpers.Mount.Dismount();

			int n = 3;
			// Todo, special for mines
			if (node.IsMineral)
				n = 6;
			do
			{
				Functions.Interact(node);
				if (GContext.Main.Me.IsInCombat)
					return false;
				bool casted = false;
				Thread.Sleep(500);

				while (GContext.Main.Me.IsCasting)
				{
					casted = true;
					Thread.Sleep(200);
				}

				if (!casted)
				{
					// looks bad
					string message = GContext.Main.RedMessage;
					if (message.StartsWith("Requires "))
					{
						// missing skill or too low
						PPather.WriteLine("Pickup: Missing skill or too low: " + message);
						return true;
					}
				}

				if (GContext.Main.Me.IsInCombat)
					return false;
				GSpellTimer bop = new GSpellTimer(500);
				do
				{
					for (int i = 1; i <= 4; i++)
					{
						if (Popup.IsVisible(i))
						{
							String text = Popup.GetText(i);
							PPather.WriteLine("Pickup: Got a loot popup ('" + text + "')");
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
				if (GContext.Main.Me.IsInCombat)
					return false;
				Thread.Sleep(2000);
				if (GContext.Main.Me.IsInCombat)
					return false;
				n--;
			} while (node.IsValid && n > 0);
			ppather.PickedUp(node);


			return true;
		}
	}
}
