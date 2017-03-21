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
	public class TrainTask : NPCInteractTask
	{
		public TrainTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
		}

		public override void GetParams(List<string> l)
		{
			l.Add("Type");
			base.GetParams(l);
		}

		private string GetTrainType()
		{
			Value v = nodetask.GetValueOfId("Type");
			if (v == null)
				return "";
			return v.GetStringValue();
		}

		private bool NeedTrain()
		{
			int TrainLevel = 0;
			string TrainLevelS = PPather.ToonData.Get("TrainLevel" + GetTrainType());
			if (TrainLevelS != null && TrainLevelS != "")
				TrainLevel = Int32.Parse(TrainLevelS);
			int mylevel = GContext.Main.Me.Level;
			if (TrainLevel != mylevel)
			{
				/*if (mylevel >= 60) return true;
				if (mylevel % 2 == 0)*/
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			return "Training " + GetType() + " at " + NPC.ToString();
		}

		public override bool WantToDoSomething()
		{
			if (!KnowNPCLocation() || ppather.IsBlacklisted(NPC))
				return false;
			if (NeedTrain())
				return true;
			return true;
		}

		ActivityTrain gossipActivity;
		public override Activity GetActivity()
		{
			if (!IsCloseToNPC())
			{
				return GetWalkToActivity();
			}
			else
			{
				if (gossipActivity == null)
					gossipActivity = new ActivityTrain(this, FindNPC(), GetTrainType());
				return gossipActivity;
			}
		}

		public override bool ActivityDone(Activity task)
		{
			if (task == gossipActivity)
			{
				ppather.Blacklist(NPC);
				return true;
			}
			return false;
		}

		public override bool IsFinished()
		{
			if (!NeedTrain())
				return true;
			return false;
		}
	}
}
