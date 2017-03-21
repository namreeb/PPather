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


namespace Pather.Tasks {
	public abstract class RunnerTask : ParserTask {
		public List<Location> Locations;
		Location currentHotSpot = null;
		ActivityWalkTo currentWalker = null;
		bool UseMount;

		public override void GetParams(List<string> l) {
			l.Add("Locations");
			l.Add("UseMount");
			base.GetParams(l);
		}

		public RunnerTask(PPather pather, NodeTask node)
			: base(pather, node) {
			Locations = new List<Location>();
			Value hs = node.GetValueOfId("Locations");
			List<Value> hs_list = hs.GetCollectionValue();
			foreach (Value v in hs_list) {
				Location l = v.GetLocationValue();
				Locations.Add(l);
			}
			UseMount = node.GetBoolValueOfId("UseMount");
		}

		public abstract Location GetNextLocation();

		public override Location GetLocation() {
			if (currentHotSpot == null) {
				currentHotSpot = GetNextLocation();
				PPather.WriteLine("GetLoc need next . got " + currentHotSpot);
			}
			return currentHotSpot;
		}

		public override bool WantToDoSomething() {
			if (GetLocation() == null) return false;
			return true; // always want to run
		}

		public override Activity GetActivity() {
			if (currentHotSpot == null) {
				PPather.WriteLine("GetAct need next");
				currentHotSpot = GetNextLocation();
			}
			if (currentWalker == null)
				currentWalker = new ActivityWalkTo(this, currentHotSpot, 10.0f, UseMount);
			return currentWalker;
		}

		public override bool ActivityDone(Activity task) {
			PPather.WriteLine("ActDone need next");
			currentHotSpot = GetNextLocation();
			currentWalker = null;
			task.Stop();
			return false;
		}
	}
}