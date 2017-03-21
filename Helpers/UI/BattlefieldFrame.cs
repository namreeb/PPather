using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI
{
	public class BattlefieldFrame
	{
		public static GInterfaceObject GetFrame()
		{
			return GContext.Main.Interface.GetByName("BattlefieldFrame");
		}

		public static bool IsVisible()
		{
			GInterfaceObject obj = GetFrame();
			if (obj != null && obj.IsVisible)
				return true;
			return false;
		}

		public static bool IsJoin()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("BattlefieldFrameJoinButton");
			if (btn != null && btn.IsVisible)
				return true;
			return false;
		}

		public static void Join()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("BattlefieldFrameJoinButton");
			if (btn != null && btn.IsVisible)
				Functions.Click(btn, false);
		}

		public static void Close()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("BattlefieldFrameCloseButton");
			if (btn != null && btn.IsVisible)
				Functions.Click(btn, false);
		}
	}
}
