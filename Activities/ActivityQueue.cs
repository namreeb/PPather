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
	public class ActivityQueue : Activity
	{
		GUnit npc;
		String battlefield;

		public ActivityQueue(Task t, GUnit npc, String battlefield)
			: base(t, "Queue for " + battlefield)
		{
			this.npc = npc;
			this.battlefield = battlefield;
		}

		public override Location GetLocation()
		{
			return null; // I will not move
		}

		public override bool Do()
		{
			int option = 0;
			int maxtries = 3;

			// ToDo: Have this use GossipFrame.ClickOptionText().

			while (maxtries > 0)
			{
				Functions.Interact(npc);
				Thread.Sleep(5000);

				if (GossipFrame.IsVisible())
				{
					//PPather.WriteLine(this.name + ": attempt to skip gossip");

					GInterfaceObject[] options = GossipFrame.VisibleOptions();
					if (option >= options.Length)
					{
						PPather.WriteLine("Queue: no battleground option in gossip frame");
						return true;
					}
					GossipFrame.ClickOption(options[option]);
					option++;
					Thread.Sleep(1000);
				}

				if (BattlefieldFrame.IsVisible())
				{
					PPather.WriteLine("Queue: got battlefield frame");

					if (BattlefieldFrame.IsJoin())
					{
						BattlefieldFrame.Join();
						BGQueueTaskManager.SetQueueState(battlefield, MiniMapBattlefieldFrameState.Queue);
						Thread.Sleep(3000);
					}
					else
					{
						PPather.WriteLine("Queue: can't join, no button?");
					}

					BattlefieldFrame.Close();
					Thread.Sleep(1000);

					return true;
				}
				else
					GContext.Main.SendKey("Common.Escape"); // Close whatever frame popped up

				maxtries--;
			}

			return true;
		}
	}
}
