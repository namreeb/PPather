using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI
{
	public class SendMailFrame
	{
		public static GInterfaceObject GetFrame()
		{
			return GContext.Main.Interface.GetByName("SendMailFrame");
		}

		public static bool IsVisible()
		{
			GInterfaceObject obj = GetFrame();
			if (obj != null && obj.IsVisible)
				return true;
			return false;
		}

		public static void TypeTo(string toName)
		{
			GInterfaceObject name = GContext.Main.Interface.GetByName("SendMailNameEditBox");
			Functions.Click(name, false);
			GContext.Main.Interface.SendString(toName);
		}

		public static void ClickSend()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("SendMailMailButton");
			Functions.Click(btn, false);
			Thread.Sleep(200);
		}

		public static bool CanSend()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("SendMailMailButton");
			if (!btn.IsVisible)
				return false;
			return true;
		}

		public static void Close()
		{
			if (!IsVisible())
				return;
			GInterfaceObject btn = GContext.Main.Interface.GetByName("InboxCloseButton");
			if (btn == null)
				return;
			Functions.Click(btn, false);
		}
	}
}
