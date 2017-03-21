using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI
{
	public class QuestFrame
	{
		/*

Pickup quest (many optioss): 
V QuestLogMicroButton
V QuestFrame
V QuestNpcNameFrame
V QuestFrameCloseButton
V QuestFrameGreetingPanel
V QuestFrameGreetingGoodbyeButton
V QuestGreetingScrollFrame
V QuestGreetingScrollChildFrame
V QuestTitleButton1
V QuestTitleButton2
V QuestTitleButton3
V QuestGreetingScrollFrameScrollBar
V QuestGreetingScrollFrameScrollBarScrollUpButton
V QuestGreetingScrollFrameScrollBarScrollDownButton
V dQuestWatch



Hand in items: 

QuestFrame
QuestNpcNameFrame
QuestFrameCloseButton
QuestFrameProgressPanel
QuestFrameGoodbyeButton
QuestFrameCompleteButton
QuestProgressScrollFrame
QuestProgressScrollChildFrame
QuestProgressItem1
QuestProgressScrollFrameScrollBar
QuestProgressScrollFrameScrollBarScrollUpButton
QuestProgressScrollFrameScrollBarScrollDownButton
QuestWatchFrame

Complete Quest: (no reward selection)

QuestFrame
QuestNpcNameFrame
QuestFrameCloseButton
QuestFrameRewardPanel
QuestFrameCancelButton
QuestFrameCompleteQuestButton
QuestRewardScrollFrame
QuestRewardScrollChildFrame
QuestRewardItem1
QuestRewardMoneyFrame
QuestRewardMoneyFrameCopperButton
QuestRewardScrollFrameScrollBar
QuestRewardScrollFrameScrollBarScrollUpButton
QuestRewardScrollFrameScrollBarScrollDownButton
QuestWatchFrame

Complete Quest: (2 rewards selection)

V QuestFrame
V QuestNpcNameFrame
V QuestFrameCloseButton
V QuestFrameRewardPanel
V QuestFrameCancelButton
V QuestFrameCompleteQuestButton
V QuestRewardScrollFrame
V QuestRewardScrollChildFrame
V QuestRewardItem1
V QuestRewardItem2
V QuestRewardScrollFrameScrollBar
V QuestRewardScrollFrameScrollBarScrollUpButton
V QuestRewardScrollFrameScrollBarScrollDownButton
V QuestWatchFrame

		 */

		public static GInterfaceObject GetFrame()
		{
			return GContext.Main.Interface.GetByName("QuestFrame");
		}

		public static string GetCompleteTitle()
		{
			GInterfaceObject QuestDetailScrollChildFrame = GContext.Main.Interface.GetByName("QuestRewardScrollChildFrame");
			GInterfaceObject QuestTitle = QuestDetailScrollChildFrame.GetChildObject("QuestRewardTitleText");
			PPather.WriteLine("Reward title: " + QuestTitle.LabelText + " v: " + QuestTitle.IsVisible);
			return QuestTitle.LabelText;
		}

		public static string GetAcceptTitle()
		{
			GInterfaceObject QuestDetailScrollChildFrame = GContext.Main.Interface.GetByName("QuestDetailScrollChildFrame");
			GInterfaceObject QuestTitle = QuestDetailScrollChildFrame.GetChildObject("QuestTitleText");
			PPather.WriteLine("Quest title: " + QuestTitle.LabelText + " v: " + QuestTitle.IsVisible);
			return QuestTitle.LabelText;
		}

		public static bool IsVisible()
		{
			GInterfaceObject obj = GetFrame();
			if (obj != null && obj.IsVisible)
				return true;
			return false;
		}

		public static bool IsSelect()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestTitleButton1");
			if (btn != null && btn.IsVisible)
				return true;
			return false;
		}

		public static bool IsAccept()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestFrameAcceptButton");
			if (btn != null && btn.IsVisible)
				return true;
			return false;
		}

		public static bool IsContinue()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestFrameCompleteButton");
			if (btn != null && btn.IsVisible)
				return true;
			return false;
		}

		public static bool IsComplete()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestFrameCompleteQuestButton");
			if (btn != null && btn.IsVisible)
				return true;
			return false;
		}

		public static int[] AvailableRewards()
		{
			List<int> options = new List<int>();
			for (int i = 1; i <= 10; i++)
			{
				GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestRewardItem" + i);
				if (btn != null && btn.IsVisible)
					options.Add(i);
			}
			return options.ToArray();
		}

		public static void SelectReward(int nr)
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestRewardItem" + nr);
			if (btn != null && btn.IsVisible)
				Functions.Click(btn, false);
		}

		public static void Accept()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestFrameAcceptButton");
			if (btn != null && btn.IsVisible)
				Functions.Click(btn, false);
		}

		public static void Continue()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestFrameCompleteButton");
			if (btn != null && btn.IsVisible)
				Functions.Click(btn, false);

		}

		public static void Complete()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestFrameCompleteQuestButton");
			if (btn != null && btn.IsVisible)
				Functions.Click(btn, false);
		}

		public static void Close()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestFrameCloseButton");
			if (btn != null && btn.IsVisible)
				Functions.Click(btn, false);
		}

		public static void Cancel()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestFrameCancelButton");
			if (btn != null && btn.IsVisible)
			{
				PPather.WriteLine("  click QuestFrameCancelButton");
				Functions.Click(btn, false);
			}
			else
				PPather.WriteLine("  can't find cancel button");

		}

		public static void Goodbye()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestFrameGoodbyeButton");
			if (btn != null && btn.IsVisible)
			{
				PPather.WriteLine("  click QuestFrameGoodbyeButton");
				Functions.Click(btn, false);
			}
			else
				PPather.WriteLine("  can't find goodbye button");

		}

		public static GInterfaceObject[] VisibleOptions()
		{
			List<GInterfaceObject> options = new List<GInterfaceObject>();
			for (int i = 1; i <= 32; i++)
			{
				GInterfaceObject btn = GContext.Main.Interface.GetByName("QuestTitleButton" + i);
				if (btn != null && btn.IsVisible)
					options.Add(btn);
			}
			return options.ToArray();
		}

		public static bool ClickOptionText(string text)
		{
			GInterfaceObject[] options = VisibleOptions();
			if (options.Length < 1)
				return false;


			PPather.WriteLine(options.Length + " visible options");

			foreach (GInterfaceObject button in options)
			{
				foreach (GInterfaceObject child in button.Children)
				{
					if (child != null && child.IsVisible && child.LabelText.Contains(text))
					{
						Functions.Click(button, false);
						return true;
					}
				}
			}
			return false;
		}

		public static void ClickOption(GInterfaceObject btn)
		{
			if (btn != null && btn.IsVisible)
				Functions.Click(btn, false);
		}
	}
}
