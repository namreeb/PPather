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
	// Look for monsters and go kill them
	public class PullTask : ParserTask {
		GUnit monster = null;
		int MinLevel = 0;
		int MaxLevel = 1000;
		public float Distance = 1E30f;
		List<String> names;
		List<String> ignore;
		List<int> factions;
		Dictionary<string, int> killedMobs = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
		bool UseMount;
		bool skipMobsWithAdds;
		float addsDistance;
		int addsCount;

		public override void GetParams(List<string> l) {
			l.Add("Names");
			l.Add("Factions");
			l.Add("Ignore");
			l.Add("MinLevel");
			l.Add("MaxLevel");
			l.Add("Distance");
			l.Add("SkipMobsWithAdds");
			l.Add("AddsDistance");
			l.Add("AddsCount");
			base.GetParams(l);
		}

		public PullTask(PPather pather, NodeTask node)
			: base(pather, node) {


			Value v_names = node.GetValueOfId("Names");
			if (v_names != null) {
				names = v_names.GetStringCollectionValues();
				if (names.Count == 0) names = null;
			}

			Value v_ignore = node.GetValueOfId("Ignore");
			if (v_ignore != null) {
				ignore = v_ignore.GetStringCollectionValues();
				if (ignore.Count == 0) ignore = null;
			}

			Value v_factions = node.GetValueOfId("Factions");
			if (v_factions != null) {
				factions = v_factions.GetIntCollectionValues();
				if (factions.Count == 0) factions = null;
				if (factions != null) foreach (int faction in factions) {
						PPather.WriteLine("  faction '" + faction + "'");
					}
			}

			MinLevel = node.GetValueOfId("MinLevel").GetIntValue();
			MaxLevel = node.GetValueOfId("MaxLevel").GetIntValue();

			Distance = node.GetValueOfId("Distance").GetFloatValue();
			if (Distance == 0.0f) Distance = 1E30f;

			if (MaxLevel == 0) MaxLevel = 10000;
			//PPather.WriteLine("  Max level " + MaxLevel);
			//PPather.WriteLine("  Min level " + MinLevel);
			SetKillCount();
			UseMount = node.GetBoolValueOfId("UseMount");

			skipMobsWithAdds = node.GetBoolValueOfId("SkipMobsWithAdds");
			if (skipMobsWithAdds)
			{
				addsDistance = node.GetFloatValueOfId("AddsDistance");
				addsCount = node.GetIntValueOfId("AddsCount");
				if (addsDistance == 0)
				{
					addsDistance = 15;
				}
				if (addsCount == 0)
				{
					addsCount = 2;
				}
			}
		}

		private void SetKillCount() {
			Value v = new Value(killedMobs);
			nodetask.SetValueOfId("KillCount", v);
		}

		public void KilledMob(String name) {
			// called my combat log parser
			if (IsValidTargetName(name)) {
				// one of mine


				int count = 0;
				killedMobs.TryGetValue(name, out count);
				count++;
				killedMobs.Remove(name);
				killedMobs.Add(name, count);

				PPather.WriteLine("Killed " + count + " " + name);
				SetKillCount();
			}

		}

		public override void Restart() {
			// ActualKillCount = 0;
		}

		public override string ToString() {
			String s = "Pulling ";
			if (monster != null)
				s += monster.Name;
			return s;
		}

		public override Location GetLocation() {
			if (monster != null)
				return new Location(monster.Location);
			return null;
		}

		private bool IsValidTargetName(String name) {
			if (names == null) return true;
			foreach (string tst_name in names) {
				if (tst_name == name) return true;
			}
			if (ignore != null) {
				foreach (string tst_ignore in ignore) {
					if (tst_ignore == name) {
						PPather.WriteLine("Skipping (ignore): " + name);
						return false; // Ignore the mob.
					}
				}
			}
			return false;
		}

		private bool IsValidTarget(GUnit monster) {
			bool ok = true;
			if (monster.Level < MinLevel || monster.Level > MaxLevel) return false;
			if (factions != null) {
				ok = false;
				// Faction must match
				foreach (int faction in factions) {
					if (monster.FactionID == faction) ok = true;
				}
			}

			if (ok == false) return false;

			ok = IsValidTargetName(monster.Name);
			if (ok == false) return false;


			return ok;
		}

		bool IsPet(GPlayer[] players, GUnit unit) {
			if (unit.IsPlayer) return false;
			
			foreach (GPlayer cur in players) {
				if (!cur.HasLivePet) continue;
				if (cur.PetGUID == unit.GUID) return true;
			}

			return false;
		}

		public virtual GUnit FindMobToPull()
		{

			// Find stuff to pull
			GUnit closest = null;
			GMonster[] monsters = GObjectList.GetMonsters();
			GPlayer[] players = /*doPvp ? */GObjectList.GetPlayers() /* new GPlayer[] { }*/;
			// maybe use GObjectList.GetUnits() instead

			List<GUnit> units = new List<GUnit>();
			units.AddRange(monsters);
			units.AddRange(players);

			float me_z = GContext.Main.Me.Location.Z;
			foreach (GUnit unit in units) {
				//PPather.WriteLine(
				//    string.Format("Considering {0}, D={1}, P={2}, R={3}, TM={4}, TP={5}, BL={6}, V={7}",
				//    unit.Name,
				//    unit.IsDead.ToString()[0],
				//    unit.IsPlayer.ToString()[0],
				//    unit.Reaction.ToString()[0],
				//    unit.IsTargetingMe.ToString()[0],
				//    unit.IsTargetingMyPet.ToString()[0],
				//    ppather.IsBlacklisted(unit).ToString()[0],
				//    IsValidTarget(unit).ToString()[0]));


				if (!unit.IsDead && unit.Reaction != GReaction.Friendly &&
					((unit.IsPlayer || !((GMonster)unit).IsTagged) || unit.IsTargetingMe || unit.IsTargetingMyPet) &&
					unit.DistanceToSelf < Distance &&
					!ppather.IsBlacklisted(unit) && IsValidTarget(unit) &&
					!IsPet(players, unit) &&
					!PPather.IsStupidItem(unit))
				{


					if (skipMobsWithAdds)
					{
						List<GMonster> closeUnits = ppather.CheckForMobsAtLoc(unit.Location, addsDistance);
						if (closeUnits.Count >= addsCount)
						{
							continue;
						}
					}

					Location ml = new Location(unit.Location);
					float dz = (float)Math.Abs(ml.Z - me_z);
					if (dz < 30.0f)
					{
						if (PPather.world.IsUnderwaterOrInAir(ml))
						{
							PPather.WriteLine(unit.Name + " is underwater or flying");
							ppather.Blacklist(unit);
						}
						else
						{
							// replace closest if unit is closer but do not replace closest if
							// closest is a player and unit is not a player
							if (closest == null || (unit.DistanceToSelf < closest.DistanceToSelf && (!closest.IsPlayer || unit.IsPlayer)))
							{
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
				if (prevMonster != null && prevMonster != monster && prevMonster.IsValid && !prevMonster.IsDead) {
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
				ppather.CurrentPullTask = this;
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
