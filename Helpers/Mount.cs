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
using Pather.Graph;

namespace Pather.Helpers {
	class Mount {
		public const int MIN_MOUNT_LEVEL = 40;

		// from oober*
		public static bool IsMounted() {
			GUnit Unit = GPlayerSelf.Me;
			Unit.SetBuffsDirty();
			GBuff[] buffs = Unit.GetBuffSnapshot();

			foreach (GBuff b in buffs) {
				string s = b.SpellName;

				if (
				   // s.Contains(MOUNTNAME) || // TODO use config var
				   s.Contains("Horse") ||
				   s.Contains("Stallion") ||
				   s.Contains("Warhorse") ||
				   s.Contains("Palomino") ||
				   s.Contains("Raptor") ||
				   s.Contains("Kodo") ||
				   s.Contains(" Wolf") ||
				   s.Contains("Saber") ||
				   s.Contains("saber") ||
				   s.Contains("Ram") ||
				   s.Contains("Mechanostrider") ||
				   s.Contains("Hawkstrider") ||
				   s.Contains("Elekk") ||
				   s.Contains("Steed") ||
				   s.Contains("steed") ||
				   s.Contains("Tiger") ||
				   s.Contains("Talbuk") ||
				   s.Contains("Battle Tank") ||
				   s.Contains("Dreadsteed") ||
				   s.Contains("Felsteed") ||
				   s.Contains("Frostwolf Howler") ||
				   s.Contains("Cheetah") ||
				   s.Contains("Travel Form") ||
				   s.Contains(" War") ||
				   s.Contains("Ravager") ||
				   s.Contains("Riding") ||
				   s.Contains("charger") ||
				   s.Contains("Charger") ||
				   s.Contains("Angry Programmer") ||
				   s.Contains("Reins") || // yeah right
				   s.Contains("Turtle")  // lol
				   
				) {
					if (s != "Ghost Wolf") return true;
				}
			}

			return false;
		}

		public static bool HaveMount()
		{
			return new GInterfaceHelper().IsKeyPopulated("Common.Mount");
		}

		static GSpellTimer mountTimer = new GSpellTimer(15000, true);

		public static bool MountUp() {
			if (GPlayerSelf.Me.Level < MIN_MOUNT_LEVEL) return false;
			if (IsMounted()) return true;
			if (GPlayerSelf.Me.IsInCombat) return false;
			if (!HaveMount()) return false;

			// only try to mount every so often
			if (!mountTimer.IsReady) {
				return false;
			}

			// check that we're not inside
			Spot mySpot = null;

			try {
				mySpot = PPather.world.GetSpot(
					new Location(GPlayerSelf.Me.Location));

				if (mySpot.GetFlag(Spot.FLAG_INDOORS)) {
					//PPather.WriteLine("Not mounting, we're inside");
					mountTimer.Reset();
					return false;
				}
			} catch {
				// if we got an exception something must be up
				return false;
			}


			PPather.mover.Stop();

			//PPather.WriteLine("Mount up");
			//buff.Snapshot();

			//mountBuffID = 0;

			GContext.Main.CastSpell("Common.Mount");

			Thread.Sleep(100);

			string badMount = null;

			if (GContext.Main.RedMessage.Contains("while swimming")) {
				badMount = "Trying to mount while swimming";
			} else if (GContext.Main.RedMessage.Contains("can't mount here")) {
				badMount = "Trying to mount inside";
				mySpot.SetFlag(Spot.FLAG_INDOORS, true);
			}

			if (null != badMount) {
				PPather.WriteLine(badMount);
				mountTimer.Reset();
				return false;
			}

			while (GPlayerSelf.Me.IsCasting) {
				Thread.Sleep(100);
			}

			if (!IsMounted()) {
				mountTimer.Reset();
				return false;
			}

			return true;
		}

		public static void Dismount() {
			if (IsMounted()) {
				PPather.WriteLine("Dismounting");
				GContext.Main.SendKey("Common.Mount");
			}
		}
	}
}
