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
using Pather.Helpers.UI;

namespace Pather.Tasks
{
	public class BGQueueTaskStatus
	{
		public string Name = null;
		public MiniMapBattlefieldFrameState State = MiniMapBattlefieldFrameState.Unknown;
	}

	public static class BGQueueTaskManager
	{
		public const int MAX_BATTLEGROUNDS = 3;
		public static List<BGQueueTaskStatus> Status = new List<BGQueueTaskStatus>(MAX_BATTLEGROUNDS);

		// are we in a battleground?
		public static bool IsInBG()
		{
			int statusCount = Status.Count;

			for (int i = 0; i < statusCount; i++)
			{
				if (GetQueueState(Status[i].Name) == MiniMapBattlefieldFrameState.Inside)
					return true;
			}

			return false;
		}

		// are we queued (or inside) a battleground?
		public static bool IsQueued()
		{
			int statusCount = Status.Count;

			for (int i = 0; i < statusCount; i++)
			{
				MiniMapBattlefieldFrameState bfState = GetQueueState(Status[i].Name);
				if (bfState == MiniMapBattlefieldFrameState.Queue ||
					bfState == MiniMapBattlefieldFrameState.CanEnter ||
					bfState == MiniMapBattlefieldFrameState.Inside)
					return true;
			}

			return false;
		}

		// get a queue info by name
		public static MiniMapBattlefieldFrameState GetQueueState(string sBattlefield)
		{
			MiniMapBattlefieldFrameState bfState = MiniMapBattlefieldFrameState.Unknown;

			if (sBattlefield != null && sBattlefield.Length > 0)
			{
				MiniMapBattlefieldFrame.GetBattlefieldState();

				int statusCount = Status.Count;

				for (int i = 0; i < statusCount; i++)
				{
					if (Status[i] != null && Status[i].Name.Equals(sBattlefield.ToLower()))
					{
						//PPather.WriteLine("BGQueueTaskManager.GetQueueState(" + sBattlefield + ") found entry");
						return Status[i].State;
					}
				}

				if (statusCount < MAX_BATTLEGROUNDS)
				{
					bfState = MiniMapBattlefieldFrameState.CanQueue;
					//PPather.WriteLine("BGQueueTaskManager.GetQueueState(" + sBattlefield + ") adding entry: " + bfState);
					SetQueueState(sBattlefield, bfState);
				}
			}

			return bfState;
		}

		public static void ResetQueueState()
		{
			//PPather.WriteLine("BGQueueTaskManager.ResetQueueState()");
			Status.Clear();
			MiniMapBattlefieldFrame.CheckDropDown.ForceReady();
		}

		public static bool SetQueueState(string sBattlefield, MiniMapBattlefieldFrameState bfState)
		{
			bool setStatus = false;

			if (sBattlefield != null && sBattlefield.Length > 0)
			{
				int statusCount = Status.Count;

				for (int i = 0; i < statusCount && !setStatus; i++)
				{
					if (Status[i] != null && Status[i].Name.Equals(sBattlefield.ToLower()))
					{
						Status[i].State = bfState;
						setStatus = true;
					}
				}

				if (!setStatus && Status.Count < MAX_BATTLEGROUNDS)
				{
					BGQueueTaskStatus bfStatus = new BGQueueTaskStatus();
					bfStatus.Name = sBattlefield.ToLower();
					bfStatus.State = bfState;
					Status.Add(bfStatus);
					setStatus = true;
				}

				//PPather.WriteLine("BGQueueTaskManager.SetQueueState(" + sBattlefield + ", " + bfState + "), Status.Count=" + Status.Count);
				statusCount = Status.Count;
				for (int i = 0; i < statusCount; i++)
				{
					if (Status[i] != null)
					{
						//PPather.WriteLine("BGQueueTaskManager.SetQueueState: Status[" + i + "] is " + Status[i]);
					}
					else
					{
						//PPather.WriteLine("BGQueueTaskManager.SetQueueState: Status[" + i + "] is null");
					}
				}
			}

			return setStatus;
		}
	}

	public class BGQueueTask : NPCInteractTask
	{
		string Battlefield;

		public BGQueueTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			Battlefield = node.GetValueOfId("Battlefield").GetStringValue();

			if (Battlefield == null || Battlefield.Length == 0)
				PPather.WriteLine("*** BGQueue: Battlefield is missing");

			if (GetLocationOfNPC() == null)
				PPather.WriteLine("*** BGQueue: NPC '" + NPC + "' is unknown");
		}

		private int GetCooldown()
		{
			return 60 * 3; // 3 minutes
		}

		public override void GetParams(List<string> l)
		{
			l.Add("Battlefield");
			base.GetParams(l);
		}

		public override bool IsFinished()
		{
			return false;
		}

		private bool wantQueue = false;

		private void UpdateWants()
		{
			// Check queue
			wantQueue = false;

			MiniMapBattlefieldFrameState bfState = BGQueueTaskManager.GetQueueState(Battlefield);

			if (NPC != null && !ppather.IsBlacklisted(NPC))
			{
				Location l = GetLocationOfNPC();
				//PPather.WriteLine("BG NPC loc: " + l);

				if (l != null && bfState == MiniMapBattlefieldFrameState.CanQueue)
				{
					wantQueue = true;
				}
				/*
				else
				{
					PPather.WriteLine("Queue State (" + Battlefield + "): " + bfState);
				}
				 */
			}

			//PPather.WriteLine("Want to Queue (" + Battlefield + "): " + (wantQueue ? "yes" : "no"));
		}

		public override Location GetLocation()
		{
			Location loc = null;
			UpdateWants();
			if (wantQueue)
			{
				loc = GetLocationOfNPC();
			}
			//PPather.WriteLine("NPC location is: " + loc);
			return loc;
		}

		public override string ToString()
		{
			return "Queueing for BG";
		}

		public override bool WantToDoSomething()
		{
			UpdateWants();
			if (wantQueue)
				return true;
			return false;
		}

		ActivityQueue queueActivity;

		public override Activity GetActivity()
		{

			// PPather.WriteLine("Pickup::GetActivity()");
			if (!IsCloseToNPC())
			{
				return GetWalkToActivity();
			}
			else
			{
				if (queueActivity == null)
				{
					queueActivity = new ActivityQueue(this, FindNPC(), Battlefield);
				}
				return queueActivity;
			}

		}

		public override bool ActivityDone(Activity task)
		{
			if (task == queueActivity)
			{
				//ppather.Blacklist(NPC, 15 * 60);
				return true;
			}
			return false;
		}

	}
}
