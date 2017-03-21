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
using System.Threading;

using Glider.Common.Objects;
using Pather.Tasks;
using Pather.Helpers;
using Pather.Graph;
using Pather.Helpers.UI;

// Started adding a bit more documentation to my scripts
// for people who like to look through them ^_^ - Flix

namespace Pather.Activities
{
	public class ActivityInteract : Activity
	{
		GUnit npc;

		public ActivityInteract(Task t, GUnit npc)
			: base(t, "Interact")
		{
			this.npc = npc;
		}

		public override Location GetLocation()
		{
			return null; // I will not move
		}

		public override bool Do()
		{
			Functions.Interact(npc);
			PPather.WriteLine("Interact: Clicking NPC - " + npc.Name);
			Thread.Sleep(1000); // Give it a sec in case of lag.
			return true;
		}
	}
}
