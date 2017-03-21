using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI
{
	public enum MiniMapBattlefieldFrameState
	{
		Unknown,	// unknown state
		CanQueue,	// can queue (outside, inside)
		CanEnter,	// can enter battleground (outside, inside)
		Queue,		// waiting in queue (outside)
		Inside		// inside battlefield
	}

	public class MiniMapBattlefieldFrame
	{
		// TODO timer should be 15 mins
		//      - clicking dropdown should only be used as a last resort or for initial state
		//      - when more things are toggling SetBattlefieldState() this can be raised
		public static GSpellTimer CheckDropDown = new GSpellTimer(30 * 1000, true);

		public static GInterfaceObject GetFrame()
		{
			return GContext.Main.Interface.GetByName("MiniMapBattlefieldFrame");
		}

		public static bool IsVisible()
		{
			GInterfaceObject obj = GetFrame();
			if (obj != null && obj.IsVisible)
				return true;
			return false;
		}

		public static void RightClick()
		{
			GInterfaceObject obj = GetFrame();
			if (obj != null && obj.IsVisible)
			{
				Functions.Hover(obj);
				Functions.Click(obj, true);
			}
		}

		public static void GetBattlefieldState()
		{
			if (IsVisible() && CheckDropDown.IsReady)
			{
				// get the 1st dropdown frame
				GInterfaceObject obj = DropDownListFrame.GetFrame();

				// check to see if it's already up (possible)
				if (obj == null || obj.IsVisible == false)
				{
					RightClick();
					Thread.Sleep(600);
				}

				// at this point dropdown is visible, else something blocked the click
				if (obj != null && obj.IsVisible)
				{
					string curBattlefield = null;
					int foundAt = 0;	// index match was found at
					int lookAt = 0;		// num elements past index we looked at

					Tasks.BGQueueTaskManager.ResetQueueState();

					// if the dropdown ever has more than 16 lines this has to be increased
					// look at the two entries after the name that matches, if one does
					for (int i = 1; i <= 16; i++)
					{
						MiniMapBattlefieldFrameState bfState = MiniMapBattlefieldFrameState.Unknown;

						string rowText = DropDownListFrame.GetButtonText(i);
						//if (rowText != null) PPather.WriteLine("rowText=" + rowText);

						if (rowText != null && rowText.Length > 0 && foundAt == 0)
						{
							curBattlefield = rowText;
							foundAt = i;
						}
						else if (foundAt > 0)
						{
							if (rowText != null)
							{
								// TODO change this to a GetLocale string
								if (rowText.Equals("Leave Queue", StringComparison.CurrentCultureIgnoreCase))
								{
									bfState = MiniMapBattlefieldFrameState.Queue;
								}
								// TODO change this to a GetLocale string
								else if (rowText.Equals("Enter Battle", StringComparison.CurrentCultureIgnoreCase))
								{
									bfState = MiniMapBattlefieldFrameState.CanEnter;
								}
								// TODO change this to a GetLocale string
								else if (rowText.Equals("Leave Battleground", StringComparison.CurrentCultureIgnoreCase))
								{
									bfState = MiniMapBattlefieldFrameState.Inside;
								}
							}

							lookAt++;
						}

						if (lookAt == 2)
						{
							Tasks.BGQueueTaskManager.SetQueueState(curBattlefield, bfState);
							//PPather.WriteLine("GetBattlefieldState: set " + curBattlefield + " to " + bfState);
							foundAt = 0;
							lookAt = 0;
						}
					}
				}

				// send a second right click to close it, we're done with it
				RightClick();
				Thread.Sleep(300);
				CheckDropDown.Reset();
			}
		}
	}

	public class MiniMapBattlefieldDropDownFrame
	{
		public static GInterfaceObject GetFrame()
		{
			return GContext.Main.Interface.GetByName("MiniMapBattlefieldDropDown");
		}

		public static bool IsVisible()
		{
			GInterfaceObject obj = GetFrame();
			if (obj != null && obj.IsVisible)
				return true;
			return false;
		}
	}
}
