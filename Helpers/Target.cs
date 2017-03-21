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

namespace Pather.Helpers
{
	class Target
	{
		public static string GetTargetAttrValue(string attr)
		{
			if (attr != "" && GPlayerSelf.Me.Target != null)
			{
				switch (attr)
				{
					case "Health":
						return GPlayerSelf.Me.Target.Health.ToString();
					case "HealthPoints":
						return GPlayerSelf.Me.Target.HealthPoints.ToString();
					case "HealthMax":
						return GPlayerSelf.Me.Target.HealthPoints.ToString();
					case "Mana":
						return GPlayerSelf.Me.Target.Mana.ToString();
					case "ManaPoints":
						return GPlayerSelf.Me.Target.ManaPoints.ToString();
					case "ManaMax":
						return GPlayerSelf.Me.Target.ManaMax.ToString();
					case "IsDead":
						return (GPlayerSelf.Me.Target.IsDead) ? "1" : "0";
					case "IsPlayer":
						return (GPlayerSelf.Me.Target.IsPlayer) ? "1" : "0";
					case "Reaction":
						return GPlayerSelf.Me.Target.Reaction.ToString();
					case "Name":
						return GPlayerSelf.Me.Target.Name;
					case "Level":
						return GPlayerSelf.Me.Target.Level.ToString();
				}
			}
			return "e_Error";
		}
	}
}