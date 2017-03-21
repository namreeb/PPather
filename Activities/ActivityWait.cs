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

namespace Pather.Activities
{
	public class ActivityWait : Activity
	{
		string waitfor;
		GSpellTimer ActionTimer = new GSpellTimer(5000);
		Helpers.Buff Buffs = new Helpers.Buff();

		public ActivityWait(Task t, string waitfor)
			: base(t, "Wait")
		{
			this.waitfor = waitfor;
		}

		public override Location GetLocation()
		{
			return null;
		}

		private bool KeepOnWaiting() {
			return true;
		}

		public override bool Do()
		{
			if (waitfor.Equals("BG", StringComparison.InvariantCultureIgnoreCase) &&
				!Buffs.HaveBuff("Preperation")) {
				PPather.WriteLine("Wait: BG Starting. Stopping Wait");
				return true;
			}

			bool DoWait = KeepOnWaiting();
			if (DoWait && ActionTimer.IsReady)
			{
				int action = GContext.Main.RNG.Next(3);
				if (action == 0)
				{
					GContext.Main.SendKey("Common.RotateRight");
				} 
				else if (action == 1)
				{
					GContext.Main.SendKey("Common.RotateLeft");
				} 
				else if (action == 2)
				{
					GContext.Main.SendKey("Common.Jump");
				}
				ActionTimer = new GSpellTimer(30000 + GContext.Main.RNG.Next(10000)); 
			}
			return !DoWait; 
		}

	}
}
