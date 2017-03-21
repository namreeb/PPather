using System;
using System.Collections.Generic;
using System.Text;

using Glider.Common.Objects;

namespace Pather {
	public class NPCDatabase {
		public class NPC {
			public String name;
			public GLocation location;
			public int faction;
			public GReaction reaction;

			public override string ToString() {
				return String.Format(PPather.numberFormat, "{0}|{1},{2},{3}|{4}|{5}",
									 name, location.X, location.Y, location.Z, faction, reaction);
			}

			public bool FromString(string s) {
				char[] splitter = { '|' };
				string[] sp = s.Split(splitter);
				if (sp.Length < 4) return false;
				name = sp[0];
				string locs = sp[1];
				char[] splitter2 = { ',' };
				string[] coords = locs.Split(splitter2);
				if (coords.Length == 3) {

					float x = float.Parse(coords[0], PPather.numberFormat);
					float y = float.Parse(coords[1], PPather.numberFormat);
					float z = float.Parse(coords[2], PPather.numberFormat);
					location = new GLocation(x, y, z);
				} else return false;
				faction = Int32.Parse(sp[2], PPather.numberFormat);
				string rs = sp[3];
				if (rs == "Friendly")
					reaction = GReaction.Friendly;
				if (rs == "Hostile")
					reaction = GReaction.Hostile;
				if (rs == "Neutral")
					reaction = GReaction.Neutral;
				if (rs == "Unknown")
					reaction = GReaction.Unknown;

				return true;
			}
		}

		Dictionary<string, NPC> NPCs;
		string continent = null;
		string myFaction = "";
		bool changed = false;

		public void SetContinent(string continent, string myFaction) {

			lock (this) {
				// Continent change
				Save();

				this.continent = continent;
				this.myFaction = myFaction;

				Load();
			}


		}


		public void Update() {

			lock (this) {
				if (continent == null) return;
				if (NPCs == null) return;
				GUnit[] units = GObjectList.GetUnits();
				foreach (GUnit unit in units) {
					if ((unit.Reaction == GReaction.Friendly || unit.Reaction == GReaction.Neutral)
						&& !unit.IsPlayer &&
						  unit.CreatureType != GCreatureType.Critter &&
						  unit.CreatureType != GCreatureType.Totem &&
						  !PPather.IsPlayerFaction(unit)) {
						string name = unit.Name;
						NPC n = null;
						if (!NPCs.TryGetValue(name, out n)) {
							n = new NPC();
							n.name = name;
							n.faction = unit.FactionID;
							n.location = unit.Location;
							n.reaction = unit.Reaction;
							PPather.WriteLine("New NPC found: " + name);
							NPCs.Add(name, n);
							changed = true;
						}
					}
				}
			}
		}

		public void Save() {
			if (!changed) return;
			lock (this) {
				if (continent == null) return;
				string filename = "PPather\\NPCInfo\\" + continent + "_" + myFaction + ".txt";
				try {
					System.IO.TextWriter s = System.IO.File.CreateText(filename);
					foreach (string name in NPCs.Keys) {
						NPC n = NPCs[name];
						s.WriteLine(n.ToString());
					}
					s.Close();
				} catch (Exception e) {
					PPather.WriteLine("!Error:Exception writing NPC data: " + e);
				}
				changed = false;

			}
		}

		public void Load() {
			lock (this) {
				if (continent == null) return;
				NPCs = new Dictionary<string, NPC>(StringComparer.InvariantCultureIgnoreCase);
				// Load from file
				try {
					string filename = "PPather\\NPCInfo\\" + continent + "_" + myFaction + ".txt";
					System.IO.TextReader s = System.IO.File.OpenText(filename);

					int nr = 0;
					string line;
					while ((line = s.ReadLine()) != null) {
						NPC n = new NPC();
						if (n.FromString(line)) {
							if (!NPCs.ContainsKey(n.name)) {
								NPCs.Add(n.name, n);
								nr++;
							}
						}

					}
					PPather.WriteLine("Loaded " + nr + " NPCs");

					s.Close();
				} catch (Exception) {
					//PPather.WriteLine("Exception reading NPC data: " + e);
					PPather.WriteLine("!Warning:Failed to load NPC data");
				}
				changed = false;
			}
		}

		public NPC Find(string name) {
			NPC n;
			if (NPCs.TryGetValue(name, out n)) {
				return n;
			}
			return null;
		}
	}
}
