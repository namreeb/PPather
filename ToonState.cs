using System;
using System.Collections.Generic;
using System.Text;

using Glider.Common.Objects;

namespace Pather {
	public class ToonState {
		// Simple saved storage or key=>value pairs

		Dictionary<string, string> dic = new Dictionary<string, string>();
		string toonName = null;
		bool changed = false;

		public void Save() {
			lock (this) {
				if (!changed) return;
				if (toonName == null) return;
				string filename = "PPather\\ToonInfo\\" + toonName + ".txt";
				try {
					System.IO.TextWriter s = System.IO.File.CreateText(filename);
					foreach (string key in dic.Keys) {
						string val = dic[key];
						s.WriteLine(key + "|" + val);
					}
					s.Close();
				} catch (Exception e) {
					PPather.WriteLine("!Error:Exception writing toon state data: " + e);
				}
				changed = false;
			}

		}

		public void Load() {
			lock (this) {
				if (toonName == null) return;
				dic = new Dictionary<string, string>();

				// Load from file
				PPather.WriteLine("Loading toon data...");
				try {
					string filename = "PPather\\ToonInfo\\" + toonName + ".txt";
					System.IO.TextReader s = System.IO.File.OpenText(filename);

					int nr = 0;
					string line;
					while ((line = s.ReadLine()) != null) {
						char[] splitter = { '|' };
						string[] st = line.Split(splitter);
						if (st.Length == 2) {
							string key = st[0];
							string val = st[1];
							Set(key, val);
							nr++;
						}
					}
					PPather.WriteLine("Loaded " + nr + " toon state data");

					s.Close();
				} catch (Exception) {
					PPather.WriteLine("!Warning:Failed reading toon state data");
				}
				changed = false;
			}
		}

		public void SetToonName(string name) {
			Save();
			toonName = name;
			Load();
		}

		public string Get(string key) {
			string val;
			if (dic.TryGetValue(key, out val)) {
				return val;
			}
			return null;
		}

		public void Set(string key, string name) {
			if (dic.ContainsKey(key))
				dic.Remove(key);
			dic.Add(key, name);
			changed = true;
		}
	}
}