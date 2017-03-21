using System;
using System.Collections.Generic;
using System.Text;

using Glider.Common.Objects;

namespace Pather.Helpers {
	class TimedLogout {
		static double LogoutTime = GContext.Main.GetConfigDouble("AutoStopMinutes");
		static bool AutoLogout = GContext.Main.GetConfigBool("AutoStop");

		static public bool CheckLogoutTime(GSpellTimer startTime) {
			if (LogoutTime <= 0 || AutoLogout == false)
				return false;

			double logoutTime = startTime.TicksSinceLastReset / 60000;
			if (logoutTime >= LogoutTime) {
				//PPather.WriteLine("Limits: Auto Stop Timer Reached. Stopping Glide.");
				return true;
			} else
				return false;
		}
	}

	class StopAtLevel {
		public static bool StopAtLevelEnabled = (PPather.PatherSettings.StopAtLevel != -1);

		public static bool TimeToStop() {
			if (StopAtLevelEnabled) {
				int CurrentLevel = GPlayerSelf.Me.Level;
				if (CurrentLevel <= PPather.PatherSettings.StopAtLevel) {
					//PPather.WriteLine("Limits: Reached wanted level. Stopping Glide.");
					return true;
				}
			}
			return false;
		}
	}
}