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
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Text;
using Glider.Common.Objects;

using Pather;
using Pather.Graph;
using Pather.Parser;
using WowTriangles;

/*
 * UnitRadar keeps track of all units we can see
 * 
 * It keeps track of their movement speed and implements
 * the ILocationHeuristict for the PathGraph to avoid 
 * running into hostile monsters
 * 
 */

namespace Pather
{
	public class UnitRadar : ILocationHeuristics
	{
		class UnitData
		{
			public long guid;
			public GUnit unit;
			public GLocation oldLocation;
			public double movementSpeed;
			public GSpellTimer lastSeen;

			public UnitData(GUnit u)
			{
				unit = u;
				guid = u.GUID;
				movementSpeed = 0.0;
				oldLocation = u.Location;
				lastSeen = new GSpellTimer(120 * 1000);
				lastSeen.Reset();

			}
			public void Update(int dt)  // dt is milliseconds
			{
				if (dt == 0)
					return;
				double ds = (double)dt / 1000.0;
				double d = oldLocation.GetDistanceTo(unit.Location);
				movementSpeed = d / ds;
				oldLocation = unit.Location;
				lastSeen.Reset();
			}
		}

		Dictionary<long, UnitData> dic = new Dictionary<long, UnitData>();
		GSpellTimer updateTimer = new GSpellTimer(0);

		public UnitRadar()
		{
		}


		public float Score(float x, float y, float z)
		{
			GLocation l = new GLocation(x, y, z);
			GLocation me = GContext.Main.Me.Location;
			if (l.GetDistanceTo(me) > 200.0)
				return 0;
			float s = 0;
			foreach (UnitData ud in dic.Values)
			{
				GUnit unit = ud.unit;
				if (!unit.IsPlayer)
				{

					int ld = unit.Level - GContext.Main.Me.Level;
					float distance = 30 + ld;

					float d = unit.Location.GetDistanceTo(l);

					if (((GMonster)unit).IsElite)
					{
						distance += 10;
					}
					if (d < distance)
					{
						float n = distance - d;

						//if(ld < 0)
						//    n /= -ld+2;
						s += n;
					}
				}
			}
			//if(s>0)
			//    PPather.WriteLine("  " + l + " score " + s);
			return s;
		}

		public void Update()
		{
			int dt = -updateTimer.TicksLeft;
			if (dt >= 1000)
			{
				GUnit[] units = GObjectList.GetUnits();
				Update(units);
			}
		}

		public void Update(GUnit[] units)
		{
			int dt = -updateTimer.TicksLeft;
			foreach (GUnit u in units)
			{
				if (u.Reaction == GReaction.Hostile)
				{
					if (!u.IsDead && !PPather.IsStupidItem(u))
					{
						UnitData ud;
						if (dic.TryGetValue(u.GUID, out ud))
						{
							ud.Update(dt);
						}
						else
						{
							// new one
							ud = new UnitData(u);
							dic.Add(u.GUID, ud);
						}
					}
				}
			}

			List<long> rem = new List<long>();
			foreach (UnitData ud in dic.Values)
			{
				if (!ud.unit.IsValid && ud.lastSeen.IsReady)
				{
					rem.Add(ud.guid);
				}
				else if (ud.unit.IsDead)
				{
					rem.Add(ud.guid);
				}
			}
			foreach (long guid in rem)
			{
				dic.Remove(guid);
			}
			updateTimer.Reset();
		}

		public double GetSpeed(GUnit u)
		{
			UnitData ud;
			if (dic.TryGetValue(u.GUID, out ud))
			{
				return ud.movementSpeed;
			}
			return 0.0;
		}
	}

}