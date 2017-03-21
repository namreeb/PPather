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

// Ripped from Pogue :p

namespace Pather.Helpers
{
	// this class must be instantiated in the event that 2 independent things
	// want to check out the buffs you wouldn't want to use the same
	// BuffSnap object

	class Buff
	{
		private GBuff[] BuffSnap = null;

		public GBuff[] Snapshot()
		{
			BuffSnap = GPlayerSelf.Me.GetBuffSnapshot();
			return BuffSnap;
		}

		// Find a buff not present in last BuffSnapshot
		public GBuff FindNew()
		{
			if (BuffSnap == null)
				return null;

			GBuff[] buffs = GPlayerSelf.Me.GetBuffSnapshot();

			for (int i = 0; i < buffs.Length; i++)
			{
				GBuff b = buffs[i];
				// O(n^2) FTW
				GBuff old = null;

				for (int j = 0; j < BuffSnap.Length && old == null; j++)
				{
					GBuff b2 = BuffSnap[j];
					if (b2.SpellID == b.SpellID)
					{
						old = b;
					}
				}

				if (old == null)
				{
					PPather.WriteLine("BuffHelper: New buff: " + b.SpellName);
					return b;
				}
			}

			return null;
		}

		public bool HaveBuff(string buffname)
		{
			GBuff[] buffs = Snapshot();
			foreach (GBuff buff in buffs)
			{
				if (buff.SpellName.Equals(buffname, StringComparison.InvariantCultureIgnoreCase))
					return true;
			}
			return false;
		}
	}
}
