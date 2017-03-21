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
	public class ActivityAttack : Activity
	{
		GUnit monster;
		public ActivityAttack(Task t, GUnit monster)
			: base(t, "Attack " + monster.Name)
		{
			this.monster = monster;
		}

		public override Location GetLocation()
		{
			return new Location(monster.Location);
		}

		public override bool Do()
		{

			GPlayerSelf Me = GContext.Main.Me;
			Helpers.Mount.Dismount();
			ppather.Face(monster);

			if (monster.SetAsTarget(false))
			{
				GUnit target = monster;
				GCombatResult res;
				do
				{
					ppather.UnBlacklist(target);
					ppather.TargetIs(target);
					target.TouchHealthDrop();
					ppather.StartCombat();
					res = ppather.KillTarget(target, Me.IsInCombat);
					PPather.WriteLine("Kill result is: " + res);
					if (res == GCombatResult.Bugged || res == GCombatResult.OtherPlayerTag)
					{
						// TODO make sure to wait out evaders that are attackign us, they usually stop after a few seconds
						if (res == GCombatResult.Bugged)
						{
							GSpellTimer t = new GSpellTimer(3000);
							while (Me.IsInCombat && !t.IsReadySlow)
								;
						}
						ppather.Blacklist(target);
					}
					if (res == GCombatResult.Died)
					{
						return true; // sigh
					}
					if (res == GCombatResult.Success ||
						res == GCombatResult.SuccessWithAdd)
					{
						ppather.Killed(target);
					}

					if (res == GCombatResult.SuccessWithAdd)
					{
						target = Me.Target;
					}
					else
					{
						target.Refresh(true);
						Thread.Sleep(100);

						// wait for combat flag to expire unless we have attackers
						{
							GSpellTimer t = new GSpellTimer(2000);
							while (Me.IsInCombat && GObjectList.GetNearestAttacker(0) == null && !t.IsReadySlow)
								;
							//PPather.WriteLine("t: " + (2000 - t.TicksLeft));
						}
						if (ppather.IsItSafeAt(null, GContext.Main.Me) && !Me.IsInCombat)
						{
							if (GContext.Main.Me.Target != null)
								GContext.Main.ClearTarget();
							ppather.Rest();
						}
					}
				} while (res == GCombatResult.SuccessWithAdd && target != null);
			}
			else
			{
				PPather.WriteLine("!Warning:Can not target monster " + monster.Name);
				ppather.Blacklist(monster);
			}

			return true; // !?!?
		}

	}
}
