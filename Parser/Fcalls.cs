using System;
using System.Collections.Generic;
using System.Text;

using Pather.Graph;
using Pather.Helpers.UI;
using Glider.Common.Objects;
using System.Xml.XPath;

namespace Pather.Parser {
	delegate Value FcallDelegate(params Value[] args);

	/// <summary>
	/// This class contains static methods that directly correspond to
	/// the available functions in a psc file. The name of the
	/// method will be the name of the available function. All method 
	/// signatures must match FcallDelegate.
	/// </summary>
	static class Fcalls {
		public static Value QuestStatus(params Value[] args) {
			String q = args[0].GetStringValue();
			String status = PPather.GetQuestStatus(q);
			if (status == null)
				return Pather.Parser.Value.NilValue;
			return new Value(status);
		}

		public static Value BGQueued(params Value[] args) {
			String bf = args[0].GetStringValue();
			if (bf.Length > 0) {
				MiniMapBattlefieldFrameState bfState = Tasks.BGQueueTaskManager.GetQueueState(bf);
				bool isQueued = false;
				if (bfState == MiniMapBattlefieldFrameState.Queue ||
					bfState == MiniMapBattlefieldFrameState.CanEnter ||
					bfState == MiniMapBattlefieldFrameState.Inside) {
					isQueued = true;
				}
				return new Value(isQueued ? 1 : 0);
			} else {
				PPather.WriteLine("*** Warning - BGQueued() called without a battleground name!");
				PPather.WriteLine("*** Did you mean to use $BGQueued?");
			}
			return null;
		}

		public static Value NearTo(params Value[] args) {
			Location cloc = args[0].GetLocationValue();
			if (cloc == null) return new Value(0);
			float howClose = args[1].GetFloatValue();
			Location mloc = new Location(GContext.Main.Me.Location);
			float d = mloc.GetDistanceTo(cloc);
			return new Value((d <= howClose) ? 1 : 0);
		}

		public static Value HaveBuff(params Value[] args) {
			string cbuff = args[0].GetStringValue();
			if (cbuff != "") {
				Helpers.Buff Buffs = new Helpers.Buff();
				if (Buffs.HaveBuff(cbuff)) return new Value(1);
			}
			return new Value(0);
		}
	}
}
