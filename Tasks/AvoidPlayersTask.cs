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

/*
 * Contributed by Surray
 */

namespace Pather.Tasks
{
	class AvoidPlayersTask : ParserTask
	{
		Activity WaitActivity = null;
		GPlayer Me = GPlayerSelf.Me;

		public AvoidPlayersTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
		}

		public override Location GetLocation()
		{
			return null;
		}

		public override void GetParams(List<string> l)
		{
			l.Add("AvoidRange");
			l.Add("WaitUntilClear");
			l.Add("TimeUntilExit");
			l.Add("StealthWhileHiding");
			l.Add("StealthKey");
			l.Add("PlaySound");
			l.Add("CatFormKey");
			base.GetParams(l);
		}

		public override string ToString()
		{
			return "Waiting for other players to leave...";
		}

		private float GetAvoidRange()
		{
			return nodetask.GetFloatValueOfId("AvoidRange");
		}

		private bool GetWaitUntilClear()
		{
			return nodetask.GetBoolValueOfId("WaitUntilClear");
		}

		private float GetTimeUntilExit()
		{
			return nodetask.GetFloatValueOfId("TimeUntilExit");
		}

		private bool GetStealthWhileHiding()
		{
			return nodetask.GetBoolValueOfId("StealthWhileHiding");
		}

		private string GetStealthKey()
		{
			return nodetask.GetValueOfId("StealthKey").ToString();
		}

		private string GetCatFormKey()
		{
			return nodetask.GetValueOfId("CatFormKey").ToString();
		}

		private bool GetPlaySound()
		{
			return nodetask.GetBoolValueOfId("PlaySound");
		}

		public override bool IsFinished()
		{
			return false;
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

		public override bool WantToDoSomething()
		{
			GUnit Player = null;
			Player = GetClosestPlayer();

			if (Player != null && Player.DistanceToSelf < GetAvoidRange())
			{
				return true;
			}
			return false;
		}

		public override Activity GetActivity()
		{
			if (WaitActivity == null)
				WaitActivity = new ActivityAvoidPlayers(this, GetAvoidRange(), GetWaitUntilClear(), GetTimeUntilExit(), GetStealthWhileHiding(), GetStealthKey(), GetPlaySound(), GetCatFormKey());
			return WaitActivity;
		}

		public override bool ActivityDone(Activity task)
		{
			if (GPlayerSelf.Me.IsStealth && GetStealthWhileHiding())
				GContext.Main.CastSpell(GetStealthKey());
			return true;
		}
	}
}