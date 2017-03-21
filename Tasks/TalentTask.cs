using System;
using System.Collections.Generic;
using System.Text;

using Glider.Common.Objects;
using Pather;
using Pather.Activities;
using Pather.Graph;
using Pather.Parser;
using Pather.Helpers.UI;

namespace Pather.Tasks
{
	public class TalentTask : ParserTask
	{
		ActivityTalents activity;
		bool isDone = false;

		public TalentTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
		}

		public override Location GetLocation()
		{
			return null;
		}

		public override void GetParams(List<string> l)
		{
			l.Add("Talents");
		}

		public List<string> GetTalents()
		{
			Value tVal = nodetask.GetValueOfId("Talents");
			if (tVal == null)
				return null;
			return tVal.GetStringCollectionValues();
		}

		public override string ToString()
		{
			return "Placing Talents";
		}

		public override bool IsFinished()
		{
			return isDone;
		}

		public override bool WantToDoSomething()
		{
			if (TalentFrame.HasPoints())
				return true;
			return false;
		}
		public override Activity GetActivity()
		{
			TalentFrame.ShowFrame();
			this.activity = new ActivityTalents(this, GetTalents());
			return this.activity;
		}

		public override bool ActivityDone(Activity task)
		{
			if (task == this.activity && task is ActivityTalents)
			{
				if (!((ActivityTalents)task).HasNext())
					this.isDone = true;
				else if (TalentFrame.HasPoints())
					return false;
			}

			TalentFrame.HideFrame();
			return true;
		}


	}
}
