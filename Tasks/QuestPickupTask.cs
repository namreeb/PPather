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
using Pather;
using Pather.Activities;
using Pather.Graph;
using Pather.Parser;

namespace Pather.Tasks {
	public class QuestPickupTask : QuestInteractTask {
		String Name;
		String ID;


		public QuestPickupTask(PPather pather, NodeTask node)
			: base(pather, node) {
			Name = node.GetValueOfId("Name").GetStringValue();
			ID = node.GetValueOfId("ID").GetStringValue();
			if (ID == null || ID == "") ID = Name;
		}

		public override string ToString() {
			return "Pickup " + Name + " " + ID;
		}

		public override void GetParams(List<string> l) {
			l.Add("Name");
			l.Add("ID");
			base.GetParams(l);
		}

		public override bool IsFinished() {
			if (ppather.IsQuestDone(ID)) return true;
			if (ppather.IsQuestAccepted(ID)) return true;
			if (ppather.IsQuestGoalDone(ID)) return true;
			if (ppather.IsQuestFailed(ID)) return true;
			return false;
		}

		public override bool WantToDoSomething() {
			if (IsFinished()) return false;
			return true;
		}

		ActivityPickupQuest gossipActivity;
		public override Activity GetActivity() {
			 if(IsCloseToObject())  {
				if (gossipActivity == null)
					gossipActivity = new ActivityPickupQuest(this, new PathObject(FindObject()), Name, ID);
				return gossipActivity;
			}
			else {
				return GetWalkToActivity();
			}
		}

		public override bool ActivityDone(Activity task) {
			if (task == gossipActivity)
			{
				return true;
			}

			return false;
		}
	}
}
