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
	class GroupFollowTask : ParserTask
	{
		GUnit target = null;
		float Distance = 10f;
		bool UseMount;
		GSpellTimer UpdateTimer = new GSpellTimer(5000);

		public GroupFollowTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			Distance = node.GetValueOfId("Distance").GetFloatValue();
			if (Distance == 0.0f)
				Distance = 10f;
			UseMount = node.GetBoolValueOfId("UseMount");
		}

		public override void GetParams(List<string> l)
		{
			l.Add("Distance");
			l.Add("UseMount");
			base.GetParams(l);
		}

		GUnit FindBestTarget()
		{
			if (target == null || ppather.IsBlacklisted(target))
				UpdateTimer.ForceReady();
			else
			{
				if (!target.IsValid || target.IsDead)
					UpdateTimer.ForceReady();
			}

			if (UpdateTimer.IsReady)
			{
				GPlayer[] players = GObjectList.GetPlayers();

				float[] playerScore = new float[players.Length];

				float best_score = 0;
				GPlayer best_player = null;
				for (int i = 0; i < players.Length; i++)
				{
					GPlayer player = players[i];
					if (!player.IsSameFaction || ppather.IsBlacklisted(player) ||
						player == GContext.Main.Me || player.Health < 0.05)
					{
						playerScore[i] = -100f;
					}
					else
					{
						for (int j = 0; j < players.Length; j++)
						{
							if (players[j].IsSameFaction && players[j] != GContext.Main.Me &&
								players[j] != player && players[j].Health > 0.05)
							{
								double d = players[j].GetDistanceTo(player);
								if (d < 30)
								{
									playerScore[i] += 30f - (float)d;
								}
							}
						}
					}
					if (playerScore[i] > best_score)
					{
						best_player = player;
						best_score = playerScore[i];
					}
				}
				if (best_score > 50)
				{
					target = best_player;
					if (target != null)
						PPather.WriteLine("follow player " + target.Name + " score " + best_score);
				}
				else
					target = null;
				UpdateTimer.Reset();
			}

			return target;
		}

		public override Location GetLocation()
		{
			target = FindBestTarget();
			if (target != null)
				return new Location(target.Location);
			return null;
		}

		public override bool IsFinished()
		{
			return false;
		}

		public override bool WantToDoSomething()
		{
			GUnit prevTarget = target;
			target = FindBestTarget();
			if (target != prevTarget)
			{
				if (prevTarget != null && prevTarget.IsValid && !prevTarget.IsDead)
				{
					PPather.WriteLine("New player to follow. ban old one:  " + prevTarget.Name);
					ppather.Blacklist(prevTarget.GUID, 30); // ban for 30 seconds
				}
				waitTask = null;
				walkTask = null;
			}
			if (target == null)
			{
				waitTask = null;
				walkTask = null;
			}
			return target != null;
		}

		private Activity waitTask = null;
		private ActivityApproach walkTask = null;

		public override Activity GetActivity()
		{
			if (walkTask != null)
			{
				// check result of walking
				if (walkTask.MoveResult != EasyMover.MoveResult.Moving &&
					walkTask.MoveResult != EasyMover.MoveResult.GotThere)
				{
					PPather.WriteLine("Can't reach " + target.Name + ". blacklist. " + walkTask.MoveResult);
					ppather.Blacklist(target.GUID, 120);
					return null;
				}
			}

			// check distance
			if (target.DistanceToSelf < Distance)
			{
				PPather.mover.Stop();
				if (waitTask == null)
					waitTask = new ActivityWait(this, "GroupFollow");
				walkTask = null;
				return waitTask;
			}
			else
			{
				// walk over there
				if (walkTask == null)
					walkTask = new ActivityApproach(this, target, Distance, UseMount);
				waitTask = null;
				return walkTask;
			}
		}

		public override bool ActivityDone(Activity task)
		{
			return false;
		}
	}
}
