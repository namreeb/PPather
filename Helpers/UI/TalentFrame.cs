using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI
{
	public class TalentFrame
	{
		private static string fTAB = "PlayerTalentFrameTab";
		private static string fRANK = "PlayerTalentFrameTalent";
		private static string fPOINTS = "PlayerTalentFrameTalentPointsText";
		private static string fDOWN = "PlayerTalentFrameScrollFrameScrollBarScrollDownButton";
		private static string fUP = "PlayerTalentFrameScrollFrameScrollBarScrollUpButton";

		// True is scrolled down / false is scrolled up
		private static bool scrolled = false;

		// All trees 14th talent is visible when fully scrolled down
		// 13 is the safest limit to scroll down on until more interface
		// information becomes available through the API
		private static int middleTalent = 13;

		// Opening the frame is your own damn job!
		public static void SpendPoint(int tab, int spot)
		{
			Functions.Click(GContext.Main.Interface.GetByName(fTAB + tab), false);
			Thread.Sleep(250);

			if (spot > middleTalent && !scrolled)
			{
				ScrollDown();
			}
			else if (spot <= middleTalent && scrolled)
			{
				ScrollUp();
			}

			Functions.Click(GContext.Main.Interface.GetByName(fRANK + spot), false);

			// Sometimes this takes a while. The task should loop until it counts
			// but lets try and play safe
			Thread.Sleep(1000);
		}

		// Opening the frame is your own damn job!
		public static string[] GetTalentString()
		{
			ScrollUp();

			string[] talents = new string[3];

			for (int i = 0; i < 3; i++)
			{
				Functions.Click(GContext.Main.Interface.GetByName(fTAB + (i + 1)), false);
				Thread.Sleep(10); // wait a moment for wow to load the new data
				GInterfaceObject talent;
				GInterfaceObject rank;

				for (int t = 1; t <= 40; t++)
				{
					talent = GContext.Main.Interface.GetByName(fRANK + t);

					if (!talent.IsVisible)
					{
						break;
					}

					rank = talent.GetChildObject(fRANK + t + "Rank");
					talents[i] += rank.LabelText;
				}
			}

			return talents;
		}

		// Frame doesn't need to be open, but it needs to be loaded
		public static bool HasPoints()
		{
			// Talent frame hasn't been loaded
			if (!FrameExists())
			{
				ToggleFrame();
				ToggleFrame();
			}


			GInterfaceObject points = GetFrame().GetChildObject(fPOINTS);
			if (points == null)
			{
				//PPather.WriteLine("Talent Frame interaction failed!");
				return false;
			}

			// That little label text that has available points 
			// updates even if the talent frame isn't shown
			if (Int32.Parse(points.LabelText) > 0)
			{
				return true;
			}
			return false;
		}

		// The talent frame doesn't actually exist in memory
		// until it's been opened at least once
		private static bool FrameExists()
		{
			if (GetFrame() == null)
				return false;
			return true;
		}

		// external frame opening
		public static void ShowFrame()
		{
			if (!GetFrame().IsVisible)
				ToggleFrame();
		}

		// external frame closing
		public static void HideFrame()
		{
			if (GetFrame().IsVisible)
				ToggleFrame();
		}

		// Internal scroll handling
		private static void ScrollDown()
		{
			GInterfaceObject down = GContext.Main.Interface.GetByName(fDOWN);
			Functions.Click(down, false);
			Thread.Sleep(50);
			Functions.Click(down, false);
			scrolled = true;
		}

		// Internal scroll handling
		private static void ScrollUp()
		{
			GInterfaceObject up = GContext.Main.Interface.GetByName(fUP);
			Functions.Click(up, false);
			Thread.Sleep(50);
			Functions.Click(up, false);
			scrolled = false;
		}

		public static bool IsVisible()
		{
			if (TalentFrame.FrameExists())
				return TalentFrame.GetFrame().IsVisible;
			return false;
		}

		// Internal frame handling
		private static GInterfaceObject GetFrame()
		{
			return GContext.Main.Interface.GetByName("PlayerTalentFrame");
		}

		// Internal frame handling
		private static void ToggleFrame()
		{
			GInterfaceObject micro = GContext.Main.Interface.GetByName("TalentMicroButton");

			if (micro.IsVisible)
			{
				Functions.Click(micro, false);
				Thread.Sleep(500);
			}
		}
	}
}
