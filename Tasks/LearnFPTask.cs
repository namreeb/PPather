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
	public class LearnFPTask : NPCInteractTask
	{
		public const string ParserKeyword = "LearnFP, GetFP";
		private bool done = false;

		public LearnFPTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
		}

		public override bool IsFinished()
		{
			return done;
		}

		public override string ToString()
		{
			return "Learning flightpath";
		}

		public override Location GetLocation()
		{
			return null;
		}

		public override bool WantToDoSomething()
		{
			return (KnowNPCLocation() && !done);
		}

		ActivityLearnFP activity;
		public override Activity GetActivity()
		{
			if (!IsCloseToNPC())
				return GetWalkToActivity();
			else
			{
				if (activity == null)
					activity = new ActivityLearnFP(this, FindNPC());
				return activity;
			}
		}

		public override bool ActivityDone(Activity task)
		{
			if (activity == task)
			{
				done = true;
				return true;
			}
			return false;
		}

	}
}
