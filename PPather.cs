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
using System.IO;
using Glider.Common.Objects;
using System.Windows.Forms;
using System.Drawing;

using Pather;

using Pather.Tasks;
using Pather.Graph;
using Pather.Activities;
using Pather.Parser;
using Pather.Helpers.UI;
using WowTriangles;

namespace Pather
{
	public abstract class PPather : GGameClass
	{
		public const double PI = Math.PI;

		public const string VERSION = "1.0.4";

		public static Random random = new Random();
		public static Mover mover;
		public static UnitRadar radar;
		public static CultureInfo numberFormat = CultureInfo.InvariantCulture;

		// presumably PPather acts as a singleton so we shouldn't have
		// any issues making fields static if necessary

		bool BGMode = false;
		bool Stopped = true;
		public PullTask CurrentPullTask = null;
		public string CurrentContinent = null;
		public static PathGraph world = null;
		Spot WasAt = null;

		public enum RunState_e
		{
			Stopped,
			Paused,
			Running
		};
		public RunState_e RunState = RunState_e.Stopped;
		public RunState_e WantedState = RunState_e.Stopped;
		public NPCDatabase NPCs = new NPCDatabase();

		public static ToonState ToonData = new ToonState();

		Thread glideThread = null;

		GSpellTimer SaveTimer = new GSpellTimer(60 * 1000);
		int XPInitial;
		int XPCurrent;
		GSpellTimer GliderStart;
		GSpellTimer LogoutTimer;
		int Kills = 0;
		int Deaths = 0;
		int Loots = 0;
		int Harvests = 0;
		int TTL = 0; // time to level in minutes
		int XPh = 0; // XP/h since last level/start
		int KPh = 0;


		// GUI

		public static PatherForm form;
		public static Settings PatherSettings;

		// subclass constructors must call this constructor via base()
		public PPather()
			: base()
		{
			RegisterTasks();
			RootNode.Init();
			PatherSettings = Settings.Load();
		}

		/// <summary>
		/// This will use reflection to read the ParserKeyword field
		/// for all subclasses of ParserTask (both built-in and user defined)
		/// and register that keyword. You can register multiple keywords by
		/// delimiting that field with commas, i.e.
		///    public const string ParserKeyword = "Par,Parallel";
		///
		/// If the field does not exist, the name of the class will
		/// be used instead without "Task" at the end of the name, which
		/// is the desired behavior for most of the parser tasks.
		/// </summary>
		private void RegisterTasks()
		{
			ParserTask.registeredTasks.Clear();

			Type taskType = typeof(ParserTask);
			Assembly cur = Assembly.GetExecutingAssembly();
			Assembly[] allAsms = System.AppDomain.CurrentDomain.GetAssemblies();

			// this is only sorted for debugging
			SortedList<string, Type> internalTypes = new SortedList<string, Type>();
			SortedList<string, Type> userTypes = new SortedList<string, Type>();

			foreach (Type t in cur.GetTypes())
			{
				if (t.IsSubclassOf(taskType) && !t.IsAbstract)
				{
					internalTypes[t.FullName] = t;
				}
			}

			foreach (Assembly a in allAsms)
			{
				if (a == cur)
					continue; // already did them

				foreach (Type t in a.GetTypes())
				{
					if (t.IsSubclassOf(taskType) && !t.IsAbstract)
					{
						userTypes[t.FullName] = t;
					}
				}
			}

			List<Type> allTypes = new List<Type>();
			allTypes.AddRange(internalTypes.Values);
			allTypes.AddRange(userTypes.Values);

			foreach (Type t in allTypes)
			{
				try
				{
					FieldInfo f = t.GetField("ParserKeyword");
					string s = "";

					if (null != f)
					{
						s = f.GetValue(null).ToString();
					}
					else
					{
						// if the field doesn't exist, use the class name
						int index = t.Name.LastIndexOf("Task");
						s = index >= 0 ? t.Name.Substring(0, index) : t.Name;
					}

					foreach (string ss in s.Split(','))
					{
						String sss = ss.Trim(); /// :rolleyes: @ C#

						if (sss != "")
							ParserTask.RegisterTask(sss, t);
					}
				}
				catch (Exception)
				{
				}
			}
			//string str = "Built-in Tasks: ";

			//foreach (string s in internalTypes.Keys) {
			//    str += s + ", ";
			//}

			//str += "\n\nUser Tasks: ";

			//foreach (string s in userTypes.Keys) {
			//    str += s + ", ";
			//}


			//MessageBox.Show("Asm: " + cur.GetName().ToString() + "\n\n" + str);


			//str = "";
			//foreach (string key in ParserTask.registeredTasks.Keys) {
			//    str += key + " -> " + ParserTask.registeredTasks[key].FullName + "\n";
			//}


			//MessageBox.Show("Registered Types:\n\n" + str);
		}



		public override string DisplayName
		{
			get
			{
				return VERSION;
			}
		}


		#region Blacklist Stuff

		private Dictionary<string, GSpellTimer> blacklisted = new Dictionary<string, GSpellTimer>();

		public void Blacklist(string name, int howlong_seconds)
		{
			GSpellTimer t = null;
			if (blacklisted.TryGetValue(name, out t))
			{
				blacklisted.Remove(name);
			}
			t = new GSpellTimer(howlong_seconds * 1000);
			blacklisted.Add(name, t);
			PPather.WriteLine("Blacklisted " + name + " for " + howlong_seconds + "s");
		}
		public void Blacklist(long GUID, int howlong_seconds)
		{
			Blacklist("GUID" + GUID, howlong_seconds);
		}

		public void Blacklist(GUnit unit)
		{
			Blacklist(unit.GUID, 15 * 60); // 15 minutes
		}
		public void Blacklist(GUnit unit, int howlong_seconds)
		{
			Blacklist(unit.GUID, howlong_seconds);
		}
		public void Blacklist(String name)
		{
			Blacklist(name, 15 * 60); // 15 minutes
		}

		public void UnBlacklist(string name)
		{
			blacklisted.Remove(name);
			PPather.WriteLine("Un-Blacklisted " + name);
		}
		public void UnBlacklist(long GUID)
		{
			UnBlacklist("GUID" + GUID);
		}

		public void UnBlacklist(GUnit u)
		{
			UnBlacklist(u.GUID);
		}

		public bool IsBlacklisted(string name)
		{
			GSpellTimer t = null;
			if (!blacklisted.TryGetValue(name, out t))
				return false;

			return !t.IsReady;
		}

		public bool IsBlacklisted(long GUID)
		{
			return IsBlacklisted("GUID" + GUID);
		}

		public bool IsBlacklisted(GUnit unit)
		{
			return IsBlacklisted(unit.GUID);
		}

		#endregion


		public void Killed(GUnit unit)
		{
			//PPather.WriteLine("Killed unit " + unit.Name);
			Kills++;
		}

		public void TargetIs(GUnit unit)
		{
			form.SetTarget(unit);
		}


		public void Looted(GUnit unit)
		{
			//PPather.WriteLine("Looted unit " + unit.Name);
			Loots++;
		}

		public void PickedUp(GNode node)
		{
			//PPather.WriteLine("Picked up node " + node.Name);
			Harvests++;
		}

		public override void LoadConfig()
		{
		}

		public override void CreateDefaultConfig()
		{
		}

		void Pather_ChatLog(string RawText, string ParsedText)
		{

			if (ParsedText.Contains("The Horde wins!") ||
					  ParsedText.Contains("The battle has ended") ||
					  ParsedText.Contains("The Alliance wins!"))
			{
				PPather.WriteLine("BG ended: " + ParsedText);
				//                Context.KillAction("BG ended", false);
			}
		}

		public string CombatLogCleaner(string raw)
		{
			StringBuilder sb = new StringBuilder();

			/*
			 * Syntax: 
			 * |Hunit:0xXXXXXXXXXXXXXXXX:Name|hName|h
			 * |cXXXXXXXXstring|r
			 * */

			int len = raw.Length;
			for (int i = 0; i < len; i++)
			{
				char c = raw[i];
				if (c == '|')
				{
					c = raw[++i];
					if (c == 'H')
					{
						while (raw[i++] != '|')
							;
						i++; // skip the 'h'
						while (raw[i] != '|')
							sb.Append(raw[i++]);
						i++; // skip the 'r'

					}
					else if (c == 'c')
					{
						i += 9;
						while (raw[i] != '|')
							sb.Append(raw[i++]);
						i++; // skip the 'r'
					}
				}
				else
					sb.Append(c);
			}

			return sb.ToString();
		}
		/*
You perform Herb Gathering on Peacebloom.
Toon begins casting Lightning Bolt.
Toon has slain Venomtail Scorpid!
Toon's Lightning Bolt hits Venomtail Scorpid for 1141 Nature.
Venomtail Scorpid died.
		 * */
		void Pather_CombatLog(string rawText)
		{
			if (rawText == null)
				return;
			string text = CombatLogCleaner(rawText);
			string myname = GContext.Main.Me.Name;
			if (text.StartsWith(myname) && text.Contains(" has slain "))
			{
				int start = text.IndexOf(" has slain ") + 11;
				int end = text.IndexOf("!");
				String mob = text.Substring(start, end - start);
				PPather.WriteLine("Killed mob: " + mob);
				if (CurrentPullTask != null)
				{
					CurrentPullTask.KilledMob(mob);
					PPather.WriteLine(CurrentPullTask.ToString());
				}
				else
				{
					PPather.WriteLine("No Pull Task");
				}

			}

		}

		public string GetCurrentLocation()
		{
            try
            {
                GLocation loc = GContext.Main.Me.Location;
                if (loc == null)
                    return null;
                return (string.Format(PPather.numberFormat, "[{0,2:#0.0}, {1,2:#0.0}, {2,2:#0.0}]", loc.X, loc.Y, loc.Z));
            }
            catch (Exception)
            {
                return null;
            }
		}

		public override void OnStartGlide()
		{
			Stopped = false;
			Helpers.Inventory.ReadyItemCacheTimer();
			//Helpers.Equip.ItemCache.Load();

			glideThread = Thread.CurrentThread;
			glideThread.CurrentCulture = CultureInfo.InvariantCulture;

			RunState = RunState_e.Paused;
			WantedState = RunState_e.Running;
			CurrentPullTask = null;

			base.OnStartGlide();
			PPather.WriteLine("Memory Usage: " + System.GC.GetTotalMemory(true) / (1024 * 1024) + " MB");

			WasAt = null;
			CurrentPullTask = null;

			string zone = MacrolessZoneInfo.GetZoneText();
			string subzone = MacrolessZoneInfo.GetSubZoneText();

			MPQTriangleSupplier mpq = new MPQTriangleSupplier();
			//CurrentContinent = mpq.SetZone(subzone + ":" + zone);            
			CurrentContinent = GContext.Main.WorldMap;
			mpq.SetContinent(CurrentContinent);

			BGMode = false;
			PPather.WriteLine("Continent is : " + ((CurrentContinent != "") ? CurrentContinent : "*** unknown"));
			PPather.WriteLine("Zone is : " + ((zone != "") ? zone : "*** unknown"));
			PPather.WriteLine("Subzone is : " + ((subzone != "") ? subzone : "*** unknown"));
			if (Helpers.StopAtLevel.StopAtLevelEnabled)
				PPather.WriteLine("!Info:Will auto stop at level " +
							PPather.PatherSettings.StopAtLevel.ToString());

			if (CurrentContinent.StartsWith("PVPZone") ||
				CurrentContinent == "NetherstormBG")
			{
				BGMode = true;
			}

			if (!System.IO.File.Exists("PPather\\ccode.dll"))
				throw new Exception("ccode.dll wasn't found! Make sure it's in the PPather folder!");
			if (!System.IO.File.Exists("PPather\\StormLib.dll"))
				throw new Exception("StormLib.dll wasn't found! Make sure it's in the PPather folder!");

			string myFaction = "Unknown";
			if (IsHordePlayerFaction(Me))
				myFaction = "Horde";
			if (IsAlliancePlayerFaction(Me))
				myFaction = "Alliance";
			NPCs.SetContinent(CurrentContinent, myFaction);
			ToonData.SetToonName(Me.Name);

			ChunkedTriangleCollection triangleWorld = new ChunkedTriangleCollection(512);
			triangleWorld.SetMaxCached(9);
			triangleWorld.AddSupplier(mpq);

			world = new PathGraph(CurrentContinent, triangleWorld, null);
		}

		private void SaveAllState()
		{
			NPCs.Save();
			ToonData.Save();
			if (world != null)
			{
				world.Save();
			}

		}

		public override void OnStopGlide()
		{
			if (Stopped)
				return;
			Stopped = true;

			WantedState = RunState_e.Stopped;
			RunState = RunState_e.Stopped;

			SaveAllState();

			if (ToonData != null)
				ToonData.SetToonName(null);
			if (NPCs != null)
				NPCs.SetContinent(null, null); // stop tracking

			if (world != null)
				world.Close();
			world = null;

			CurrentPullTask = null;

			// forget what we know about battlefield state
			BGQueueTaskManager.ResetQueueState();

			PPather.WriteLine("Memory Usage Before: " + GC.GetTotalMemory(false) / (1024 * 1024) + " MB");
			world = null; // release RAM
			GC.Collect();

			// passing true to GetTotalMemory isn't the same as calling Collect()
			PPather.WriteLine("Memory Usage After: " + GC.GetTotalMemory(true) / (1024 * 1024) + " MB");

            GContext.Main.DisableCursorHook();

			base.OnStopGlide();
		}

		public override void Startup()
		{
			mover = new Mover(Context);
			radar = new UnitRadar();
			form = new PatherForm(this);
			PPather.WriteLine("!Good:PPather Startup - Version " + VERSION);
			form.ShowInTaskbar = false;
			form.Show();
			base.Startup();
			GContext.Main.ChatLog += new GContext.GChatLogHandler(Pather_ChatLog);
			GContext.Main.CombatLog += new GContext.GCombatLogHandler(Pather_CombatLog);
		}

		public override void Shutdown()
		{
			GContext.Main.CombatLog -= new GContext.GCombatLogHandler(Pather_CombatLog);
			GContext.Main.ChatLog -= new GContext.GChatLogHandler(Pather_ChatLog);
			form.Dispose();
			base.Shutdown();
		}

		public override void Patrol()
		{
			OnStartGlide();

			Kills = 0;
			Loots = 0;
			Harvests = 0;

			XPInitial = Me.Experience;
			GliderStart = new GSpellTimer(0);
			LogoutTimer = new GSpellTimer(0);

			if (!Me.IsInCombat && !Me.IsDead)
				Rest();

			while (true)
			{
				MyPather();
				Thread.Sleep(1000);
			}
		}

		private void UpdateXP()
		{
			XPCurrent = Me.Experience;
			if (XPCurrent < XPInitial)
			{
				PPather.WriteLine("!Good:Ding! Congratulations.");
				XPInitial = Me.Experience;
				XPCurrent = XPInitial;
				GliderStart = new GSpellTimer(0);
			}
			else
			{
				double XPGained = (double)(XPCurrent - XPInitial);
				double time = (double)(-GliderStart.TicksLeft) / 3600000.0; // hours
				if (time != 0.0)
				{
					int XpNeeded = Me.NextLevelExperience - Me.Experience;
					int XPPerHour = (int)(XPGained / time);
					KPh = (int)((double)Kills / time);

					int minsToLvl = XPPerHour == 0 ? 0 : (60 * XpNeeded) / XPPerHour;
					//PPather.WriteLine("Kills: " + Kills + " Kills/h: " + KillsPerHour + " XP/h: " + XPPerHour + " TTL: " + minsToLvl + " min");
					TTL = minsToLvl;
					XPh = XPPerHour;
				}
			}
		}

		private GSpellTimer ChunkLoadT = new GSpellTimer(5000, true);

		public void ResetMyPos()
		{
			WasAt = null;
		}

		public void UpdateMyPos()
		{
			radar.Update();
			if (world != null)
			{
				GLocation loc = GContext.Main.Me.Location;
				Location isAt = new Location(loc.X, loc.Y, loc.Z);
				//if(WasAt != null)  PPather.WriteLine("was " + WasAt.location);
				//PPather.WriteLine("isAt " + isAt); 
				if (WasAt != null)
				{
					if (WasAt.GetLocation().GetDistanceTo(isAt) > 20)
						WasAt = null;
				}
				WasAt = world.TryAddSpot(WasAt, isAt);
			}
		}

		public static string GetQuestStatus(string quest)
		{
			string val = ToonData.Get("Quest:" + quest);
			return val;
		}

		// Quest have 4 states:  
		//  accepted   - picked up
		//  failed     - failed for some reason
		//  goaldone   - goal done, need handin
		//  completed  - completed and handed in
		//  completedr - completed but repeatable

		public void QuestAccepted(string name)
		{
			PPather.WriteLine("!Info:Quest accepted: '" + name + "'");
			ToonData.Set("Quest:" + name, "accepted");
		}

		public void QuestFailed(string name)
		{
			PPather.WriteLine("!Info:Quest failed: '" + name + "'");
			ToonData.Set("Quest:" + name, "failed");
		}

		public void QuestGoalDone(string name)
		{
			PPather.WriteLine("!Info:Quest goal done: '" + name + "'");
			ToonData.Set("Quest:" + name, "goaldone");
		}

		public void QuestCompleted(string name, bool repeat)
		{
			PPather.WriteLine("!Info:Quest completed: '" + name + "'");
			ToonData.Set("Quest:" + name, "completed" + ((repeat) ? "r" : ""));
		}

        public void SetToonState(string key, string value) 
        { 
                PPather.WriteLine("!Info:Saved toon state variable: \"" + key + "\" with state: " + value); 
                ToonData.Set(key, value); 
        } 

        public static string GetToonState(string key) 
        { 
                string val = ToonData.Get(key); 
                return val; 
        } 

		// completed or failed
		public bool IsQuestDone(string name)
		{
			string val = ToonData.Get("Quest:" + name);
			if (val == null)
				return false;
			if (val == "completedr")
				return false;
			if (val == "failed" || val == "completed")
				return true;
			return false;
		}

		public bool IsQuestFailed(string name)
		{
			string val = ToonData.Get("Quest:" + name);
			if (val == null)
				return false;
			if (val == "failed")
				return true;
			return false;
		}

		public bool IsQuestAccepted(string name)
		{
			string val = ToonData.Get("Quest:" + name);
			if (val == null)
				return false;
			if (val == "accepted")
				return true;
			return false;
		}

		public bool IsQuestGoalDone(string name)
		{
			string val = ToonData.Get("Quest:" + name);
			if (val == null)
				return false;
			if (val == "goaldone")
				return true;
			return false;
		}

		// Called by gui thread
		public void UpdateNPCs()
		{
			if (NPCs == null)
				return;

			NPCs.Update();
		}

		public List<GMonster> CheckForMobsAtLoc(GLocation l, float radius)
		{
			List<GMonster> returns = new List<GMonster>();
			GMonster[] mobs = GObjectList.GetMonsters();
			if (mobs.Length > 0)
			{
				foreach (GMonster mob in mobs)
				{
					float mdt = mob.GetDistanceTo(l);
					if (mdt <= radius && !mob.IsDead && !mob.IsTagged)
						returns.Add(mob);
				}
			}
			return returns;
		}

		public Location FindNPCLocation(string name)
		{
			if (NPCs == null)
				return null;
			NPCDatabase.NPC npc = NPCs.Find(name);
			//PPather.WriteLine("found '" + name + "' or? " + npc);
			if (npc == null)
				return null;
			return new Location(npc.location);
		}

		public GLocation PredictedLocation(GUnit mob)
		{
			GLocation currentLocation = mob.Location;
			double x = currentLocation.X;
			double y = currentLocation.Y;
			double z = currentLocation.Z;
			double heading = mob.Heading;
			double dist = 4;

			x += Math.Cos(heading) * dist;
			y += Math.Sin(heading) * dist;

			GLocation predictedLocation = new GLocation((float)x, (float)y, (float)z);

			GLocation closestLocatition = currentLocation;
			if (predictedLocation.DistanceToSelf < closestLocatition.DistanceToSelf)
				closestLocatition = predictedLocation;
			return closestLocatition;
		}

		public static bool IsStupidItem(GUnit unit)
		{
			if (unit.CreatureType == GCreatureType.Totem)
				return true;
			// Filter out all stupid sting found in outland
			string name = unit.Name.ToLower();
			if (name.Contains("target") || name.Contains("trigger") ||
				name.Contains("flak cannon") || name.Contains("trip wire") ||
				name.Contains("infernal rain") || name.Contains("anilia") ||
				name.Contains("teleporter credit") || name.Contains("door fel cannon") ||
				name.Contains("ethereum glaive") || name.Contains("orb flight"))
				return true;
			return false;
		}

		public bool IsItSafeAt(GUnit ignore, GUnit u)
		{
			return IsItSafeAt(ignore, u.Location);
		}

		public bool IsItSafeAt(GUnit ignore, Location l)
		{
			return IsItSafeAt(ignore, new GLocation(l.X, l.Y, l.Z));
		}

		public bool IsItSafeAt(GUnit ignore, GLocation l)
		{
			/*List<GMonster> mobs = CheckForMobsAtLoc(l, 30.0f); // Setting for radius?
			foreach (GMonster mob in mobs) {
				if (mob != (GMonster)ignore &&
					!IsStupidItem(mob)) {
					if (!mob.IsDead && mob.Reaction == GReaction.Hostile &&
						!mob.IsTagged)
						return false;
				}
			}*/
			return true;
		}

		public double DistanceToClosestHostileFrom(GUnit target)
		{

			GMonster m = GObjectList.GetNearestHostile(target.Location, target.GUID, false);
			if (m != null)
				return m.GetDistanceTo(target);
			else
				return 1E100;
		}

		public GPlayer GetClosestPvPPlayer()
		{
			GPlayer[] plys = GObjectList.GetPlayers();
			GPlayer ClosestPlayer = null;

			foreach (GPlayer p in plys)
			{
				if (!p.IsSameFaction && !p.IsDead && p.Location.Z > Me.Location.Z - 5 && p.Location.Z < Me.Location.Z + 5)
				{
					double d = p.GetDistanceTo(Me);
					if ((ClosestPlayer == null || d < ClosestPlayer.GetDistanceTo(Me)))
						ClosestPlayer = p;
				}
			}
			return ClosestPlayer;
		}

		public GPlayer GetClosestPvPPlayerAttackingMe()
		{
			GPlayer[] plys = GObjectList.GetPlayers();
			GPlayer ClosestPlayer = null;

			foreach (GPlayer p in plys)
			{
				if (!p.IsSameFaction && p.Target == Me)
				{
					if (ClosestPlayer == null || p.GetDistanceTo(Me) < ClosestPlayer.GetDistanceTo(Me))
						ClosestPlayer = p;
				}
			}
			return ClosestPlayer;
		}

		public GPlayer GetClosestFriendlyPlayer()
		{
			GPlayer[] plys = GObjectList.GetPlayers();
			GPlayer ClosestPlayer = null;

			foreach (GPlayer p in plys)
			{
				if (p.IsSameFaction && p != Me)
				{
					if (ClosestPlayer == null || p.GetDistanceTo(Me) < ClosestPlayer.GetDistanceTo(Me))
						ClosestPlayer = p;
				}
			}
			return ClosestPlayer;
		}

		public GUnit FindAttacker()
		{
			// Find attackers
			GUnit attacker = GObjectList.GetNearestAttacker(0);
			if (attacker != null)
			{
				if (attacker.IsPlayer)
				{
					// hmmm
					if (attacker.IsInCombat &&
						attacker.Target != null &&
						attacker.Target == GContext.Main.Me)
					{
						// looks like this sucker is attacking me!
						return attacker;
					}
				}
				else
				{
					return attacker; // a monster
				}
			}
			return null;
		}

		public bool Face(GUnit monster)
		{
			return Face(monster, PI / 8);
		}

		public bool Face(GNode node)
		{
			return Face(node, PI / 8);
		}

		public bool Face(GUnit monster, double tolerance)
		{
			int timeout = 3000;
			if (monster == null)
				return false;
			GSpellTimer approachTimeout = new GSpellTimer(timeout, false);
			if (Math.Abs(monster.Bearing) < tolerance)
				return true;
			bool wasDead = monster.IsDead;
			do
			{
				if (Me.IsDead || wasDead != monster.IsDead)
				{
					mover.Stop();
					return false;
				}

				double b = monster.Bearing;
				if (b < -tolerance)
				{
					// to the left
					mover.RotateLeft(true);
				}
				else if (b > tolerance)
				{
					// to the rigth
					mover.RotateRight(true);
				}
				else
				{
					// ahead
					mover.Stop();
					return true;
				}

				UpdateMyPos();
			} while (!approachTimeout.IsReadySlow);

			mover.Stop();
			PPather.WriteLine("!Error:Couldn't face unit");
			return false;

		}

		public bool Face(GNode monster, double tolerance)
		{

			if (monster == null)
				return false;
			int timeout = 3000;

			if (mover == null)
				return false;
			GSpellTimer approachTimeout = new GSpellTimer(timeout, false);
			if (Math.Abs(monster.Location.Bearing) < tolerance)
				return true;
			do
			{
				if (Me.IsDead)
				{
					mover.Stop();
					return false;
				}

				double b = monster.Location.Bearing;
				if (b < -tolerance)
				{
					// to the left
					mover.RotateLeft(true);
				}
				else if (b > tolerance)
				{
					// to the rigth
					mover.RotateRight(true);
				}
				else
				{
					// ahead
					mover.Stop();
					return true;
				}

				UpdateMyPos();
			} while (!approachTimeout.IsReadySlow);

			mover.Stop();
			PPather.WriteLine("!Error:Couldn't face unit");
			return false;

		}

		public bool Face(PathObject obj)
		{
			return Face(obj, PI / 8);
		}

		public bool Face(PathObject obj, double tolerance)
		{
			if (obj.isUnit())
			{
				return Face(obj.getUnit(), PI / 8);
			}
			else if (obj.isNode())
			{
				return Face(obj.getNode(), PI / 8);
			}
			else
			{
				throw new Exception("!Error:Couldn't assign type to : " + obj);
			}
		}

		public static bool IsHordePlayerFaction(GUnit u)
		{
			int f = u.FactionID;
			if (f == 2 ||
				f == 5 ||
				f == 6 ||
				f == 116 ||
				f == 1610)
				return true;
			return false;
		}

		public static bool IsAlliancePlayerFaction(GUnit u)
		{
			int f = u.FactionID;
			if (f == 1 ||
				f == 3 ||
				f == 4 ||
				f == 115 ||
				f == 1629)
				return true;

			return false;
		}

		public static bool IsPlayerFaction(GUnit u)
		{
			return IsHordePlayerFaction(u) || IsAlliancePlayerFaction(u);
		}

		public bool MoveToGetThemInFront(GUnit Target, GUnit Add)
		{
			double bearing = Add.Bearing;
			if (!IsInFrontOfMe(Add))
			{
				PPather.WriteLine("Got add " + Add.Name + " behind me");
				/*
				  hmm, just back up? or turn a bit too?
				*/

				mover.Backwards(true);
				if (bearing < 0)
				{
					//PPather.WriteLine("  back up left");
					mover.RotateLeft(true);
				}
				else
				{
					//PPather.WriteLine("  back up right");
					mover.RotateRight(true);
				}
				Thread.Sleep(300);
				mover.RotateLeft(false);
				mover.RotateRight(false);
				Thread.Sleep(300);
				mover.Backwards(false);
				return true;
			}
			return false;
		}

		public void TweakMelee(GUnit Monster)
		{
			double Distance = Monster.DistanceToSelf;
			double sensitivity = 2.5; // default melee distance is 4.8 - 2.5 = 2.3, no monster will chase us at 2.3
			double min = Context.MeleeDistance - sensitivity;
			if (min < 1.0)
				min = 1.0;

			if (Distance > Context.MeleeDistance)
			{
				// Too far
				//Spam("Tweak forwards. "+ Distance + " > " + Context.MeleeDistance);
				mover.Forwards(true);
				Thread.Sleep(100);
				mover.Forwards(false);
			}
			else if (Distance < min)
			{
				// Too close
				//Spam("Tweak backwards. "+ Distance + " < " + min);
				mover.Backwards(true);
				Thread.Sleep(200);
				mover.Backwards(false);
			}
		}

		double BearingToMe(GUnit unit)
		{
			// return value from -PI to PI
			GLocation MyLocation = Me.Location;
			float bearing = (float)unit.GetHeadingDelta(MyLocation);
			return bearing;
		}

		public bool IsInFrontOfMe(GUnit unit)
		{
			double bearing = unit.Bearing;
			return bearing < PI / 2.0 && bearing > -PI / 2.0;
		}

		public bool Approach(GUnit monster, bool AbortIfUnsafe)
		{
			return Approach(monster, AbortIfUnsafe, 10000);
		}

		public bool Approach(GUnit monster, bool AbortIfUnsafe, int timeout)
		{
			GLocation loc = monster.Location;
			if (loc.DistanceToSelf < Context.MeleeDistance &&
				Math.Abs(loc.Bearing) < PI / 8)
			{
				mover.Stop();
				return true;
			}

			GSpellTimer approachTimeout = new GSpellTimer(timeout, false);
			StuckDetecter sd = new StuckDetecter(this, 1, 2);
			GSpellTimer t = new GSpellTimer(0);
			bool doJump = random.Next(4) == 0;
			EasyMover em = null;
			GSpellTimer NewTargetUpdate = new GSpellTimer(1000);
			bool WasInCombat = GContext.Main.Me.IsInCombat;
			do
			{
				UpdateMyPos();

				// Check for stuck
				if (sd.checkStuck())
				{
					PPather.WriteLine("!Error:Major stuck on approach. Giving up");
					mover.Stop();
					return false;
				}
				if (WasInCombat != GContext.Main.Me.IsInCombat)
				{
					//PPather.WriteLine("Combat status changed");
					mover.Stop();
					return false;
				}

				double distance = monster.DistanceToSelf;
				bool moved;
				if (distance < 8)
				{
					loc = monster.Location;
					moved = mover.moveTowardsFacing(Me, loc, Context.MeleeDistance, loc);
				}
				else
				{
					if (em == null)
					{
						loc = monster.Location;
						em = new EasyMover(this, new Location(loc), false, AbortIfUnsafe);
					}
					if (NewTargetUpdate.IsReady)
					{
						loc = monster.Location;
						em.SetNewTarget(new Location(loc));
						NewTargetUpdate.Reset();
					}
					EasyMover.MoveResult mr = em.move();

					moved = true;
					if (mr != EasyMover.MoveResult.Moving)
					{
						moved = false;
					}
				}

				if (!moved)
				{
					mover.Stop();
					return true;
				}

			} while (!approachTimeout.IsReadySlow && !Me.IsDead);
			mover.Stop();
			PPather.WriteLine("!Error:Approach timed out");
			return false;
		}

		public bool WalkTo(GLocation loc, bool AbortIfUnsafe, int timeout, bool AllowDead)
		{
			if (loc.DistanceToSelf < Context.MeleeDistance &&
				Math.Abs(loc.Bearing) < PI / 8)
			{
				mover.Stop();
				return true;
			}

			GSpellTimer approachTimeout = new GSpellTimer(timeout, false);
			StuckDetecter sd = new StuckDetecter(this, 1, 2);
			GSpellTimer t = new GSpellTimer(0);
			bool doJump = random.Next(4) == 0;
			EasyMover em = null;
			do
			{
				UpdateMyPos();

				// Check for stuck
				if (sd.checkStuck())
				{
					PPather.WriteLine("!Error:Major stuck on approach. Giving up");
					mover.Stop();
					return false;
				}


				double distance = loc.DistanceToSelf;
				bool moved;
				if (distance < 8)
				{
					moved = mover.moveTowardsFacing(Me, loc, Context.MeleeDistance, loc);
				}
				else
				{
					if (em == null)
						em = new EasyMover(this, new Location(loc), false, AbortIfUnsafe);
					EasyMover.MoveResult mr = em.move();

					moved = true;
					if (mr != EasyMover.MoveResult.Moving)
					{
						moved = false;
					}
				}

				if (!moved)
				{
					mover.Stop();
					//PPather.WriteLine("did not move");
					return true;
				}

			} while (!approachTimeout.IsReadySlow && (!Me.IsDead || AllowDead));
			mover.Stop();
			PPather.WriteLine("!Error:Approach timed out");
			return false;
		}

		public GLocation InFrontOf(GUnit unit, double d)
		{
			double x = unit.Location.X;
			double y = unit.Location.Y;
			double z = unit.Location.Z;
			double heading = unit.Heading;
			x += Math.Cos(heading) * d;
			y += Math.Sin(heading) * d;

			return new GLocation((float)x, (float)y, (float)z);
		}

		public GLocation InFrontOf(GLocation loc, double heading, double d)
		{
			double x = loc.X;
			double y = loc.Y;
			double z = loc.Z;

			x += Math.Cos(heading) * d;
			y += Math.Sin(heading) * d;

			return new GLocation((float)x, (float)y, (float)z);
		}

		/*
		 * Doesn't seem to do anything...
		private void DoPatrolerLearnTravelMode() {
			while (true) {
				Thread.Sleep(1000);
				GUnit target = Me.Target;

				if (target != null) {
					PPather.WriteLine("Target GUID: " + target.GUID);
				}
			}
		}
		 */

		private void GhostRun()
		{
			PPather.WriteLine("!Info:I died. Let's resurrect");
			GLocation CorpseLocation = null;
			#region 1. Release
			while (Me.IsDead && !Me.IsGhost)
			{
				if (CorpseLocation == null)
					CorpseLocation = Me.Location;
				for (int n = 1; n <= 4; n++)
				{
					if (Popup.IsVisible(n))
					{
						String text = Popup.GetText(n);
						if (text.Contains("until release") || text.Contains("You have died") || text.Contains("Releasing"))
						{
							PPather.WriteLine("Found the release dialog, #" + n);
							Popup.ClickButton(n, 1);
						}
						else
							PPather.WriteLine("!Error:Couldn't find release dialog");
					}
				}
				Thread.Sleep(1000);
			}
			Thread.Sleep(3000);
			if (BGMode)
			{
				GSpellTimer s = new GSpellTimer(1000 * 60); // 1 minute
				PPather.WriteLine("BGMode. Waiting for res");

				// make sure we stop moving, some ccs try to run back to
				// where we were fighting...
				mover.Stop();
				// since glider's default movement doesn't have stop
				Movement.MoveToLocation(Me.Location);

				while (Me.IsDead)
				{
					Thread.Sleep(500);
				}
				//PPather.WriteLine("Alive!");
			}
			else
			{
				Thread.Sleep(5000);
				GLocation gloc = Me.CorpseLocation;
				if (CorpseLocation != null)
					gloc = CorpseLocation;

				Location target = null;
				GLocation gtarget;
				//PPather.WriteLine("Corpse is at " + gloc);

				if (gloc.Z == 0)
				{
					PPather.WriteLine("!Warning:Corpse Z is 0");
					target = new Location(gloc);
					for (int q = 0; q < 50; q += 5)
					{
						float stand_z = 0;
						int flags = 0;
						float x = gloc.X + random.Next(20) - 10;
						float y = gloc.Y + random.Next(20) - 10;
						bool ok = world.triangleWorld.FindStandableAt(x, y,
																	  -5000,
																	  5000,
																	  out stand_z, out flags, 0, 0);
						if (ok)
						{
							target = new Location(x, y, stand_z);
							break;
						}
					}
				}
				else
				{
					target = new Location(gloc);

				}
				gtarget = new GLocation(target.X, target.Y, target.Z);

				PPather.WriteLine("Corpse is at " + target);
				PPather.WriteLine("I am at " + new Location(Me.Location));
				EasyMover em = new EasyMover(this, target, false, false);
			#endregion

				#region 2. Run to corpse
				while (Me.IsDead && Me.GetDistanceTo(gloc) > 20)
				{
					EasyMover.MoveResult mr = em.move();
					if (mr != EasyMover.MoveResult.Moving)
						return; // buhu
					UpdateMyPos();
					Thread.Sleep(50);

				}
				mover.Stop();
				#endregion

				#region 3. Find safe place to res
				float SafeDistance = 25.0f;
				while (true)
				{
					// some brute force :p
					GMonster[] monsters = GObjectList.GetMonsters();
					PPather.WriteLine("Looking for a safe spot to resurrect");
					float best_score = 1E30f;
					float best_distance = 1E30f;
					Location best_loc = null;
					for (float x = -35; x <= 35; x += 5)
					{
						for (float y = -35; y <= 35; y += 5)
						{
							float rx = target.X + x;
							float ry = target.Y + y;
							GLocation xxx = new GLocation(rx, ry, 0);
							if (xxx.GetDistanceTo(gtarget) < 35)
							{
								float stand_z = 0;
								int flags = 0;
								bool ok = world.triangleWorld.FindStandableAt(rx, ry,
																			  target.Z - 20,
																			  target.Z + 20,
																			  out stand_z, out flags, 0, 0);
								if (ok)
								{
									float score = 0.0f;
									GLocation l = new GLocation(rx, ry, stand_z);
									foreach (GMonster monster in monsters)
									{
										if (monster != null && monster.Reaction == GReaction.Hostile && !monster.IsDead && !PPather.IsStupidItem(monster))
										{
											float d = l.GetDistanceTo(monster.Location);
											if (d < 35)
											{
												// one point per yard 
												score += 35 - d;
											}
										}
									}
									float this_d = Me.GetDistanceTo(l);
									if (score <= best_score && this_d < best_distance)
									{
										best_score = score;
										best_distance = this_d;
										best_loc = new Location(l);
									}
								}
							}
						}
					}
					if (best_loc != null)
					{
						GLocation best_gloc = new GLocation(best_loc.X, best_loc.Y, best_loc.Z);
						PPather.WriteLine("Looks safe at " + best_gloc + " score " + best_score + " i am at " + Me.Location);
						// walk over there
						WalkTo(best_gloc, false, 10000, true);

						// Check if I am safe
						bool safe = true;
						GMonster unsafe_monster = null;
						foreach (GMonster monster in monsters)
						{
							if (monster.Reaction == GReaction.Hostile && !monster.IsDead && !PPather.IsStupidItem(monster))
							{
								float d = Me.GetDistanceTo(monster);
								if (d < SafeDistance)
									if (Math.Abs(monster.Location.Z - Me.Location.Z) < 15)
									{
										safe = false;
										unsafe_monster = monster;
									}

							}
						}
						if (safe)
						{
							PPather.WriteLine("Spot is safe. Resurrecting");
							break; // yeah
						}
						else
						{
							if (unsafe_monster != null)
								PPather.WriteLine("Spot is unsafe. Mob: " + unsafe_monster.Name);
							else
								PPather.WriteLine("Spot is unsafe");
						}
					}

					// hmm, look again
					PPather.WriteLine("It was not safe, wait a little and lower standards. safe distance now is: " + SafeDistance);
					Thread.Sleep(2000);
					SafeDistance -= 0.5f;
				}
				#endregion

				#region 4. Resurrect

				// dialog should be up now
				while (Me.IsDead)
				{
					for (int n = 1; n <= 4; n++)
					{
						if (Popup.IsVisible(n))
						{
							String text = Popup.GetText(n);
							if (text == "Resurrect now?")
							{
								PPather.WriteLine("Found the accept dialog, #" + n);
								Popup.ClickButton(n, 1);
							}
							else
								PPather.WriteLine("!Error:Accept dialog wasn't found");
						}
					}
					Thread.Sleep(1000);
				}
				#endregion
			}
		}

		public Task CreateTaskFromNode(NodeTask node, Task parent)
		{
			Task n = ParserTask.GetTask(this, node);
			if (n != null)
				n.parent = parent;
			else
			{
				PPather.WriteLine("!Warning:Unknown task type: " + node.type);
				Context.KillAction("PPather, script corrupt", false);
			}
			return n;
		}

		private void MyPather()
		{
			System.IO.TextReader reader = null;
			NodeTask astRoot = null;

			if (System.IO.File.Exists(PatherSettings.TaskFile))
			{
				reader = System.IO.File.OpenText(PatherSettings.TaskFile);
			}
			else if (System.IO.File.Exists(PatherSettings.TaskFile + ".txt"))
			{
				// Let's check if they saved it as a .txt by accident
				PPather.WriteLine("!Warning:Your task file has a .txt extension.");
				PPather.WriteLine("!Warning:PPather will continue but you should save as .psc");
				reader = System.IO.File.OpenText(PatherSettings.TaskFile + ".txt");
			}
			else
			{
				MessageBox.Show("Your selected task file wasn't found:\n" + PatherSettings.TaskFile);
			}

			if (reader != null)
			{
				Preprocessor pproc = new Preprocessor(reader);
				TaskParser t = new TaskParser(new StreamReader(pproc.ProcessedStream));
				astRoot = t.ParseTask(null);
				reader.Close();
			}
			else
				GContext.Main.KillAction("!Error:TextReader was null. File not found?", false);


			RootNode root = new RootNode();
			root.AddTask(astRoot);
			root.BindSymbols(); // Just to make it a tad faster


			Helpers.DetectFollower CheckFollow = new Helpers.DetectFollower();

			Stack<Task> taskQueue = new Stack<Task>();
			Stack<Activity> activityQueue = new Stack<Activity>();

			Task rootTask = CreateTaskFromNode(root, null);
			Helpers.TaskInfo.Root = rootTask;
			form.CreateTreeFromTasks(rootTask);
			Activity activity = null;
			GSpellTimer taskTimer = new GSpellTimer(300);
			GSpellTimer updateStatusTimer = new GSpellTimer(2000);
			GSpellTimer nothingToDoTimer = new GSpellTimer(3 * 1000);
			bool exit = false;
			GSpellTimer Tick = new GSpellTimer(100);
			do
			{
				if (RunState != WantedState)
				{
					if (RunState == RunState_e.Running)
					{
						if (activity != null)
							activity.Stop();
					}
					RunState = WantedState;
				}

				if (updateStatusTimer.IsReady)
				{
					UpdateXP();
					form.SetStatus(Kills, KPh, Loots, XPh, Harvests, TTL, Deaths);
					updateStatusTimer.Reset();
				}
				if (RunState == RunState_e.Stopped)
				{
					PPather.WriteLine("!Info:Stop requested. Stopping glide");
					Context.KillAction("PPather wants to stop", false);
					Thread.Sleep(500); // pause
				}
				else if (RunState == RunState_e.Paused)
				{
					UpdateMyPos();
					Thread.Sleep(100); // pause
				}
				else if (RunState == RunState_e.Running)
				{
					UpdateMyPos();
					if (Me.IsDead)
					{
						Deaths++;
						if (PatherSettings.MaxResurrection != true ||
							(PatherSettings.MaxResurrection == true) &&
							(Deaths < PatherSettings.MaxResurrectionAmount))
						{
							Thread.Sleep(1000);
							activity = null;
							GhostRun();
							Thread.Sleep(1500);
							if (Me.IsDead)
							{
								//!!!
							}
							else
							{
								Rest();
							}
						}
						else if ((PatherSettings.MaxResurrection == true) &&
								(Deaths >= PatherSettings.MaxResurrectionAmount))
						{
							Thread.Sleep(300);
							Context.HearthAndExit();
							PPather.WriteLine("!Info:Died too many times. Stopping Glide");
							Context.KillAction("Died too many times, stopping glide", false);
						}
					}

					if (CheckFollow.CheckFollowers())
					{
						Context.HearthAndExit();
						PPather.WriteLine("!Info:Followed for too long. Stopping Glide");
						Context.KillAction("Followed for too long, stopping glide", false);
					}

					if (Helpers.TimedLogout.CheckLogoutTime(LogoutTimer))
					{
						Context.HearthAndExit();
						PPather.WriteLine("!Info:Logout timer reached. Stopping Glide");
						Context.KillAction("Logout timer reached. Stopping glide", false);
					}

					if (Helpers.StopAtLevel.TimeToStop())
						WantedState = RunState_e.Stopped;

					if (activity == null || taskTimer.IsReady)
					{
						// Reevaluate what to do
						Location l = new Location(Me.Location);
						taskTimer.Reset();

						Activity newActivity = null;
                        if (rootTask.WantToDoSomething())
						{
							newActivity = rootTask.GetActivity();
							nothingToDoTimer.Reset();
						}

						if (newActivity.task.GetType().ToString() == "Pather.Tasks.LoadTask")
						{
							PPather.WriteLine("Queueing the old task tree");
							taskQueue.Push(rootTask);
							activityQueue.Push(activity);
							//PPather.WriteLine("Still alive!");
							if (activity != null)
							{
								bool done = false;
								int wait = 0;

								do
								{
									done = activity.Do();
									wait++;
									Thread.Sleep(10);
								} while (!done && (wait > 100));

								if (!done)
									activity.Stop();

								Task tr = activity.task;
								while (tr != null)
								{
									tr.isActive = false;
									tr = tr.parent;
								}
								activity = null;
							}
							reader = null;
							astRoot = null;

							string loadfile = Functions.GetTaskFilePath() + ((LoadTask)newActivity.task).File;
							PPather.WriteLine("Loading file - " + loadfile);
							if (File.Exists(loadfile))
								reader = File.OpenText(loadfile);
							else
								PPather.WriteLine("!Warning:File could not be loaded - " + loadfile);
							if (reader != null)
							{
								Preprocessor pproc = new Preprocessor(reader);
								TaskParser t = new TaskParser(new StreamReader(pproc.ProcessedStream));
								astRoot = t.ParseTask(null);
								reader.Close();
								root = null;
								root = new RootNode();
								root.AddTask(astRoot);
								root.BindSymbols(); // Just to make it a tad faster

								rootTask = null;
								rootTask = CreateTaskFromNode(root, null);
								Helpers.TaskInfo.Root = rootTask; // Desired?
								form.CreateTreeFromTasks(rootTask);
								activity = null;
								newActivity.task.Restart();
								newActivity = null;

								if (rootTask == null)
									PPather.WriteLine("!Error:Load: No root task!");
								else
								{
									PPather.WriteLine("Load: Load has been successful");
									rootTask.Restart();
                                    if (rootTask.WantToDoSomething())
									{
										newActivity = rootTask.GetActivity();
										nothingToDoTimer.Reset();
									}
								}
							}
						}

						if (newActivity.task.GetType().ToString() == "Pather.Tasks.UnloadTask")
						{
							if (activity != null)
							{
								activity.Stop();
								activity = null;
							}
							rootTask = taskQueue.Pop();
							activity = activityQueue.Pop();
							newActivity.task.Restart();
							newActivity = null;
                            if (rootTask.WantToDoSomething())
							{
								newActivity = rootTask.GetActivity();
								nothingToDoTimer.Reset();
							}
						}

						if (newActivity != activity)
						{
							if (activity != null)
							{
								// change activity before it was finished
								activity.Stop();
								Task tr = activity.task;
								while (tr != null)
								{
									tr.isActive = false;
									tr = tr.parent;
								}
							}
							activity = newActivity;
							if (activity != null)
							{
								Task tr = activity.task;
								while (tr != null)
								{
									tr.isActive = true;
									tr = tr.parent;
								}
							}
							else
							{
								PPather.WriteLine("!Error:Got a null activity");
							}

							form.SetActivity(activity);
							form.SetZone(MacrolessZoneInfo.GetZoneText(), MacrolessZoneInfo.GetSubZoneText());
							form.SetLocation(GetCurrentLocation());

							if (activity != null)
							{
								PPather.WriteLine("Got a new activity: " + activity);
								if (SaveTimer.IsReady)
								{
									SaveAllState();
									SaveTimer.Reset();
								}
								activity.Start();
							}
						}
						if (newActivity == null)
							activity = null;

					}

					if (activity == null)
					{
						if (nothingToDoTimer.IsReady)
						{
							PPather.WriteLine("!Info:Script ended. Stopping Glide");
							Context.KillAction("PPather, nothing more to do", false);
							return;
						}
					}
					else
						nothingToDoTimer.Reset();
					//form.SetTask(task); 

					if (activity != null)
					{
						bool done = activity.Do();
						nothingToDoTimer.Reset(); // did something
						if (done)
						{
							//PPather.WriteLine("Finished activity " + activity);
							activity.task.ActivityDone(activity);
							Task tr = activity.task;
							while (tr != null)
							{
								tr.isActive = false;
								tr = tr.parent;
							}
							activity = null;
						}
					}


					Tick.Wait();
					Thread.Sleep(10); // min sleep time
					Tick.Reset();
					RunningAction();
				}
			} while (!exit);
		}

		public static void WriteLine(string msg)
		{
			PPather.form.WriteLine(msg);
		}

		public static void Write(string msg)
		{
			PPather.form.Write(msg);
		}


	}
}
