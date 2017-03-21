using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI
{
	public class MailFrame
	{
		public static GInterfaceObject GetFrame()
		{
			return GContext.Main.Interface.GetByName("MailFrame");
		}

		public static bool IsVisible()
		{
			GInterfaceObject obj = GetFrame();
			if (obj != null && obj.IsVisible)
				return true;
			return false;
		}

		public static bool ClickSendMailTab()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("MailFrameTab2");
			if (btn != null && btn.IsVisible)
			{
				Functions.Click(btn, false);
				return true;
			}
			return false;
		}
	}
}
