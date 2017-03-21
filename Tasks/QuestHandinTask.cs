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

namespace Pather.Tasks
{
	public class QuestHandinTask : QuestInteractTask
	{

		String Name;
		String ID;
		int Reward;
		bool Repeatable;

		public QuestHandinTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			Name = node.GetValueOfId("Name").GetStringValue();
			ID = node.GetValueOfId("ID").GetStringValue();
			if (ID == null || ID == "") ID = Name;

			Reward = node.GetValueOfId("Reward").GetIntValue();
			if (Reward == 0) Reward = 1;

			Repeatable = node.GetValueOfId("Repeatable").GetBoolValue();
		}

		public override string ToString()
		{
			return "Handin " + Name + " " + ID;
		}

		public override void GetParams(List<string> l)
		{
			l.Add("Name");
			l.Add("ID");
			l.Add("Reward");
			l.Add("Repeatable");
			base.GetParams(l);
		}
		public override bool IsFinished()
		{
			if (ppather.IsQuestDone(ID)) return true;
			return false;
		}

		public override bool WantToDoSomething()
		{
			return ppather.IsQuestGoalDone(ID);
		}

		ActivityHandinQuest gossipActivity;
		public override Activity GetActivity()
		{
			if (!IsCloseToObject())
			{
				return GetWalkToActivity();
			}
			else
			{
				if (gossipActivity == null)
				{
					GObject obj = FindObject();
					if (obj != null)
					{
						gossipActivity = new ActivityHandinQuest(this, new PathObject(obj), Name, ID, Reward, Repeatable);
					}
				}
				return gossipActivity;
			}
			
		}



		public override bool ActivityDone(Activity task)
		{

			if (task == gossipActivity)
			{
				return true;
			}

			return false;
		}
	}
}
