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
	// Run along hotspots
	public class HotspotTask : RunnerTask
	{
		public const string ParserKeyword = "Hotspots";
		int currentHotSpotIndex = -1;
		String order;

		public HotspotTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			// save us the hassle of case sensitive checking
			order = node.GetValueOfId("Order").GetStringValue();
			currentHotSpotIndex = -1;
			if (order == "reverse")
				currentHotSpotIndex = Locations.Count;
		}

		public override void GetParams(List<string> l)
		{
			l.Add("Order");
			base.GetParams(l);
		}

		public override bool IsFinished()
		{
			return false;
		}

		public override string ToString()
		{
			return "Moving to hotspot";
		}

		public override Location GetNextLocation()
		{
			if (Locations.Count == 1)
				return Locations[0];
			if (order.Equals("order", StringComparison.InvariantCultureIgnoreCase))
			{
				currentHotSpotIndex++;
				if (currentHotSpotIndex >= Locations.Count)
					currentHotSpotIndex = 0;
			}
			else if (order.Equals("reverse", StringComparison.InvariantCultureIgnoreCase))
			{
				currentHotSpotIndex--;
				if (currentHotSpotIndex < 0)
					currentHotSpotIndex = Locations.Count - 1;
			}
			else
			{
				currentHotSpotIndex = PPather.random.Next(Locations.Count);
			}

			return Locations[currentHotSpotIndex];
		}
	}
}