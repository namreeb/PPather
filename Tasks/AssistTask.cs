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
	class AssistTask : ParserTask {
		int MinLevel;
		int MaxLevel;
		GUnit monster = null;
		float Distance = 1E30f;
		bool UseMount;

		public AssistTask(PPather pather, NodeTask node)
			: base(pather, node) {
			MinLevel = node.GetValueOfId("MinLevel").GetIntValue();
			MaxLevel = node.GetValueOfId("MaxLevel").GetIntValue();
			Distance = node.GetValueOfId("Distance").GetFloatValue();
			if (Distance == 0.0f) Distance = 1E30f;
			UseMount = node.GetBoolValueOfId("UseMount");
		}

		public override void GetParams(List<string> l) {
			l.Add("MinLevel");
			l.Add("MaxLevel");
			l.Add("Distance");
			base.GetParams(l);
		}

		public override Location GetLocation() {
			if (monster != null)
				return new Location(monster.Location);
			return null;
		}

		GUnit FindMobToPull() {
			// Find stuff to pull
			GUnit closest = null;
			// only assist players
			//GMonster[] monsters = GObjectList.GetMonsters();
			GPlayer[] players = GObjectList.GetPlayers();

			List<GUnit> units = new List<GUnit>();
			//units.AddRange(monsters);
			units.AddRange(players);

			float me_z = GContext.Main.Me.Location.Z;
			foreach (GUnit cur in units) {
				GUnit unit = cur.Target;

				if (cur.Reaction != GReaction.Friendly ||
					!cur.IsInCombat ||
					unit == null)
					continue;

				if (!unit.IsDead && unit.Reaction != GReaction.Friendly &&
					unit.IsInCombat &&
					unit.DistanceToSelf < Distance &&
					!ppather.IsBlacklisted(unit) &&
					!PPather.IsStupidItem(unit)) {
					Location ml = new Location(unit.Location);
					float dz = (float)Math.Abs(ml.Z - me_z);
					if (dz < 30.0f) {
						if (PPather.world.IsUnderwaterOrInAir(ml)) {
							PPather.WriteLine(unit.Name + " is underwater or flying");
							ppather.Blacklist(unit);
						} else {
							if (closest == null || unit.DistanceToSelf < closest.DistanceToSelf) {
								closest = unit;
							}
						}
					}
				}
			}

			//PPather.WriteLine("Returning unit: " + (closest == null ? "null" : closest.Name));
			return closest;
		}

		public override bool IsFinished() {
			return false;
		}

		public override bool WantToDoSomething() {
			MinLevel = nodetask.GetValueOfId("MinLevel").GetIntValue();
			MaxLevel = nodetask.GetValueOfId("MaxLevel").GetIntValue();

			GUnit prevMonster = monster;
			monster = FindMobToPull();
			if (monster != prevMonster) {
				if (prevMonster != null && prevMonster.IsValid && !prevMonster.IsDead) {
					PPather.WriteLine("new monster to attack. ban old one:  " + prevMonster.Name + "");
					ppather.Blacklist(prevMonster.GUID, 45); // ban for 45 seconds
				}
				attackTask = null;
				walkTask = null;
			}
			if (monster == null) {
				attackTask = null;
				walkTask = null;
			}
			return monster != null;
		}

		private Activity attackTask = null;
		private ActivityApproach walkTask = null;
		public override Activity GetActivity() {

			if (walkTask != null) {
				// check result of walking
				if (walkTask.MoveResult != EasyMover.MoveResult.Moving &&
					walkTask.MoveResult != EasyMover.MoveResult.GotThere) {
					PPather.WriteLine("Can't reach " + monster.Name + ". blacklist. " + walkTask.MoveResult);
					ppather.Blacklist(monster);
					return null;
				}

			}
			// check distance
			if (monster.DistanceToSelf < ppather.PullDistance) {


				PPather.mover.Stop();
				if (attackTask == null)
					attackTask = new ActivityAttack(this, monster);
				walkTask = null;
				return attackTask;
			} else {
				// walk over there
				if (walkTask == null)
					walkTask = new ActivityApproach(this, monster, ppather.PullDistance, UseMount);
				attackTask = null;
				return walkTask;
			}
		}

		public override bool ActivityDone(Activity task) {

			if (task == attackTask) {
				monster = null;
				attackTask = null;
				walkTask = null;

			}

			task.Stop();

			return false;
		}
	}
}
