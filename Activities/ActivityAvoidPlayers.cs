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

/*
 * Contributed by Surray
 */

namespace Pather.Activities
{
	public class ActivityAvoidPlayers : Activity
	{
		float AvoidRange = 40;
		bool WaitUntilClear = true;
		float TimeUntilExit = 3;
		bool StealthWhileHiding = false;
		string StealthKey = "Rogue.Stealth";
		string CatFormKey = "Druid.CatForm";
		bool PlaySound = true;

		GPlayer Me = GPlayerSelf.Me;

		public ActivityAvoidPlayers(Task t, float AvoidRange, bool WaitUntilClear, float TimeUntilExit, bool StealthWhileHiding, string StealthKey, bool PlaySound, string CatFormKey)
			: base(t, "AvoidPlayers")
		{
			this.AvoidRange = AvoidRange;
			this.WaitUntilClear = WaitUntilClear;
			this.TimeUntilExit = TimeUntilExit;
			this.StealthWhileHiding = StealthWhileHiding;
			this.StealthKey = StealthKey;
			this.PlaySound = PlaySound;
			this.CatFormKey = CatFormKey;
		}

		public override Location GetLocation()
		{
			return null;
		}

		public GPlayer GetClosestPlayer()
		{
			GPlayer[] plys = GObjectList.GetPlayers();
			GPlayer ClosestPlayer = null;

			foreach (GPlayer p in plys)
			{
				if (p != Me)
				{
					if (ClosestPlayer == null || p.GetDistanceTo(Me) < ClosestPlayer.GetDistanceTo(Me))
						ClosestPlayer = p;
				}
			}
			return ClosestPlayer;
		}

		public override bool Do()
		{
			bool hide;
			int minutes = 0;
			GUnit Player = null;
			Player = GetClosestPlayer();

			if (Player != null && Player.DistanceToSelf < AvoidRange)
			{
				GSpellTimer WaitAfterLeave = new GSpellTimer(10 * 1000); // wait an exta 10 s
				GSpellTimer SpamTimer = new GSpellTimer(60 * 1000); // Spam every 1 min
				System.Media.SoundPlayer sp = new System.Media.SoundPlayer("PlayerNear.wav");
				WaitAfterLeave.Reset();
				SpamTimer.Reset();
				try {
					if (PlaySound) {
						sp.Load();
						sp.Play();
					}
				} catch { }

				do
				{
					if (Me.IsInCombat)
					{
						PPather.WriteLine("AvoidPlayers: I was attacked while hiding");
						return false;
					}

					if (Me.IsDead)
					{
						PPather.WriteLine("AvoidPlayers: I died while hiding");
						return false;
					}

					hide = false;
					if (WaitUntilClear == true && GetClosestPlayer() != null) { hide = true; }
					else if (WaitUntilClear != true && GetClosestPlayer().DistanceToSelf < AvoidRange) { hide = true; }
					if (hide) WaitAfterLeave.Reset();

					if (Me.PlayerClass.ToString() == "Rogue" && StealthWhileHiding == true && !GPlayerSelf.Me.IsStealth)
					{
						GContext.Main.CastSpell(StealthKey);
					}

					if (Me.PlayerClass.ToString() == "Druid" && StealthWhileHiding == true && !GPlayerSelf.Me.IsStealth)
					{
						if (!Me.HasWellKnownBuff("CatForm")) GContext.Main.CastSpell(CatFormKey, true, false); // if we aren't in cat form, then switch before trying to stealth
						GContext.Main.CastSpell(StealthKey, true, false);
					}

					if (SpamTimer.IsReady)
					{
						SpamTimer.Reset();
						minutes++;
						PPather.WriteLine("AvoidPlayers: Waited " + minutes.ToString() + "minutes for players to leave."); if (PlaySound) sp.Play();
						if (minutes >= TimeUntilExit)
						{
							PPather.WriteLine("AvoidPlayers: Time to log out");
							GContext.Main.HearthAndExit();
							return false;
						}
					}
					Thread.Sleep(1000);
				} while (!WaitAfterLeave.IsReady);
				return true;
			}
			return false;
		}

	}
}