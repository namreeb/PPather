using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI
{
	// static methods to handle Gossip frames
	public class GossipFrame
	{
		/*
		 V GossipFrame
V GossipNpcNameFrame
V GossipFrameCloseButton
V GossipFrameGreetingPanel
V GossipFrameGreetingGoodbyeButton
V GossipGreetingScrollFrame
V GossipGreetingScrollChildFrame
V GossipTitleButton1
GossipTitleButton2
...
GossipTitleButton32
V GossipSpacerFrame
V GossipGreetingScrollFrameScrollBar
V GossipGreetingScrollFrameScrollBarScrollUpButton
V GossipGreetingScrollFrameScrollBarScrollDownButton
*/
		public static GInterfaceObject GetFrame()
		{
			return GContext.Main.Interface.GetByName("GossipFrame");
		}

		public static bool IsVisible()
		{
			GInterfaceObject obj = GetFrame();
			if (obj != null && obj.IsVisible)
				return true;
			return false;
		}

		public static GInterfaceObject[] VisibleOptions()
		{
			List<GInterfaceObject> options = new List<GInterfaceObject>();
			for (int i = 1; i <= 32; i++)
			{
				GInterfaceObject btn = GContext.Main.Interface.GetByName("GossipTitleButton" + i);
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
						Functions.Click(button);
						return true;
					}
				}
			}
			return false;
		}

		public static void ClickOption(GInterfaceObject btn)
		{
			if (btn != null && btn.IsVisible)
				Functions.Click(btn);
		}

		public static void Close()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("GossipFrameCloseButton");
			if (btn != null && btn.IsVisible)
				Functions.Click(btn);
		}
	}
}
