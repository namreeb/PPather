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
	public class WalkTask : RunnerTask
	{
		int currentHotSpotIndex = -1;
		string order = "Order";

		public WalkTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			if (node.GetValueOfId("Order").GetStringValue() != null)
				order = node.GetValueOfId("Order").GetStringValue().ToLower();
		}

		public override void GetParams(List<string> l)
		{


			base.GetParams(l);
		}
		public override void Restart()
		{
			currentHotSpotIndex = -1;
		}

		public override bool IsFinished()
		{
			return currentHotSpotIndex >= Locations.Count;
		}


		public override string ToString()
		{
			return "Go to";
		}

		public override Location GetNextLocation()
		{
			currentHotSpotIndex++;
			Location nextLoc = null;
			PPather.WriteLine("Walk " + currentHotSpotIndex + " " + Locations.Count);
			if (currentHotSpotIndex >= Locations.Count || currentHotSpotIndex < 0)
			{
				nextLoc = null;
			}
			else if (order == "reverse")
			{
				nextLoc = Locations[(Locations.Count - 1) - currentHotSpotIndex];
			}
			else
			{
				nextLoc = Locations[currentHotSpotIndex];
			}
			return nextLoc;
		}
	}
}