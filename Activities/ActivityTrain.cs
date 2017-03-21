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
using Pather.Helpers.UI;

namespace Pather.Activities
{
	public class ActivityTrain : Activity
	{
		GUnit unit;
		string trainType;

		public ActivityTrain(Task t, GUnit unit, string trainType)
			: base(t, "Train")
		{
			this.unit = unit;
			this.trainType = trainType;
		}

		public override Location GetLocation()
		{
			return null;
		}

		public override bool Do()
		{
			ppather.Face(unit);

			Functions.Interact(unit);
			Thread.Sleep(2000);

			if (GossipFrame.IsVisible())
			{
				if (!GossipFrame.ClickOptionText("training"))
					return false;
				Thread.Sleep(2000);
			}
			if (TrainerFrame.IsVisible())
			{
				TrainerFrame.LearnAllSkills();
				PPather.WriteLine("Train: Learned new skills");
				GContext.Main.SendKey("Common.Escape"); // Clear up
				// Stop it from trying to execute again on the same level
				PPather.ToonData.Set("TrainLevel" + trainType,
									GPlayerSelf.Me.Level.ToString());
			}
			else
			{
				GContext.Main.SendKey("Common.Escape");
				return false;
			} // ? Try again?
			return true;
		}
	}
}
