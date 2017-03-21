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
using Pather.Helpers;
using Pather.Helpers.UI;

namespace Pather.Activities
{
	/*
	  hmm dialog navigation: 
   
		If there are many options you will get either a
			GossipFrame or a QuestFrame.IsSelect
 
		After that was selected you want to get a
		   QuestFrame.IsAccept

	 */

	public class ActivityPickupQuest : Activity
	{
		private PathObject pathObject;
		private EasyMover em;
		String QuestName;
		String QuestID;

		public ActivityPickupQuest(Task t, PathObject obj, String QuestName, String QuestID)
			: base(t, "PickupQuest " + QuestName)
		{
			this.pathObject = obj;
			this.QuestName = QuestName;
			this.QuestID = QuestID;
		}

		public override Location GetLocation()
		{
			return null; // I will not move
		}

		public override bool Do()
		{
			Helpers.Mount.Dismount();

			if (ppather.IsQuestAccepted(QuestID))
				return true;

			if (pathObject != null)
			{
				if (pathObject.isNode() || pathObject.isUnit())
					ppather.Face(pathObject);
				Functions.Interact(pathObject);
			}

			// For quest pickup on some items
			while (GPlayerSelf.Me.IsCasting)
				Thread.Sleep(250);
			Thread.Sleep(1000); // In case of lag

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
			if (QuestFrame.IsVisible() && QuestFrame.IsAccept())
			{
				string quest = QuestFrame.GetAcceptTitle();
				if (quest.Contains(QuestName))
				{
					PPather.WriteLine("PickupQuest: Woot! We have the right quest. Picking up.");
					ppather.QuestAccepted(QuestID);
					QuestFrame.Accept();
					Thread.Sleep(1000);
					return true;
				}
				else
				{
					// Got the wrong quest somehow
					GContext.Main.SendKey("Common.Escape");
					ppather.QuestFailed(QuestID);
					return true;
				}
			}
			/*
			 * Try Continue button if it didn't have the Accept button.
			 * This is the case with many repeatable quests.
			 */
			if (QuestFrame.IsVisible() && QuestFrame.IsContinue())
			{
				PPather.WriteLine("Yiha! Continuing with quest...");
				ppather.QuestAccepted(QuestID);
				QuestFrame.Continue();
				Thread.Sleep(1000);
				return true;
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
