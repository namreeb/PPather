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
	
	public abstract class ActivityFreeTask : ParserTask 
	{
		public ActivityFreeTask(PPather pather, NodeTask node)
			: base(pather, node) 
		{
			
		}

		public override Location GetLocation()
		{
			return null;
		}

		public abstract bool DoActivity(); 

		public override Activity GetActivity()
		{
			return new SimpleActivity(this, this.ToString());
		}

		private class SimpleActivity : Activity
		{
			public SimpleActivity(Task task, String name) : base(task, name)
			{
			}

			public override Location GetLocation()
			{
				return null; 
			}

			public override bool Do()
			{
				ActivityFreeTask aft = (ActivityFreeTask)task;
				return aft.DoActivity(); 
			}

		}
	}
}
