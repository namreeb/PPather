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
	public class ActivityHandinQuest : Activity
	{
		PathObject pathObject;
		String QuestName;
		String QuestID;
		Task t;
		int Reward;
		bool Repeatable;
		EasyMover em;

		public ActivityHandinQuest(Task t, PathObject obj, String QuestName, String QuestID, int Reward, bool Repeatable)
			: base(t, "HandinQuest " + QuestName)
		{
			this.pathObject = obj;
			this.QuestName = QuestName;
			this.QuestID = QuestID;
			this.Reward = Reward;
			this.t = t;
			this.Repeatable = Repeatable;
		}

		public override Location GetLocation()
		{
			return null; // I will not move
		}

		public override bool Do()
		{
			Helpers.Mount.Dismount();

			if (!ppather.IsQuestGoalDone(QuestID))
				return true; // Quest isn't ready to handin

			ppather.Face(pathObject);
			Functions.Interact(pathObject);

			while (GPlayerSelf.Me.IsCasting)
				Thread.Sleep(100);

			Thread.Sleep(1000);

			if (GossipFrame.IsVisible())
			{
				if (!GossipFrame.ClickOptionText(QuestName))
					return false;
				Thread.Sleep(2000); // Lag
			}
			if (QuestFrame.IsVisible() && QuestFrame.IsSelect())
			{
				if (!QuestFrame.ClickOptionText(QuestName))
					return false;
				Thread.Sleep(2000); // Lag
			}
			if (QuestFrame.IsVisible() && QuestFrame.IsContinue())
			{
				QuestFrame.Continue();
				Thread.Sleep(2000);
			}

			if (QuestFrame.IsVisible() && QuestFrame.IsComplete())
			{

				PPather.WriteLine("HandinQuest: Great! A quest complete frame");
				string quest = QuestFrame.GetCompleteTitle();
				if (quest.ToLower().Contains(QuestName.ToLower()))
				{
					PPather.WriteLine("HandinQuest: Complete quest name match for " + quest);
					QuestFrame.SelectReward(Reward);
					Thread.Sleep(300);
					QuestFrame.Complete();
					ppather.QuestCompleted(QuestID, Repeatable);
					Thread.Sleep(3000);
					return true;
				}
				else
				{
					PPather.WriteLine("HandinQuest: Not the right quest");
					GContext.Main.SendKey("Common.Escape");
					Thread.Sleep(1000);
					ppather.QuestFailed(QuestID);
					return true; // cry                        
				}

			}

			// If we're here...that means we failed to pick up the quest.
			// Maybe we could try a different angle if it's not an item

			if (!pathObject.isItem())
			{
				Location newLoc = new Location((pathObject.getLocation().X + (GContext.Main.Me.Location.X - pathObject.getLocation().X) * 2), pathObject.getLocation().Y + ((GContext.Main.Me.Location.Y - pathObject.getLocation().Y) * 2), pathObject.getLocation().Z + ((GContext.Main.Me.Location.Z - pathObject.getLocation().Z) * 2));
				em = new EasyMover(ppather, newLoc, false, true);
				em.move(2.5f);
				return false; // Does this loop through again?
			}
			return true;
		}
	}
}
