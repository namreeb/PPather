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

namespace Pather.Activities {
	public class ActivityRest : Activity {
		public ActivityRest(Task t)
			: base(t, "Rest") {
		}
		public override Location GetLocation() {
			return null;
		}


		public override bool Do() {
            if (GContext.Main.Me.Target != null) GContext.Main.ClearTarget();
			ppather.Rest();
			return true; // !?!?
		}

	}
}
