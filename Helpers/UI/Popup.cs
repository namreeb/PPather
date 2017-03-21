using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI
{
	// static methods to handle popups
	public class Popup
	{
		// 1-4
		public static bool IsVisible(int popNr)
		{
			String name = "StaticPopup" + popNr;
			GInterfaceObject obj = GContext.Main.Interface.GetByName(name);
			if (obj != null && obj.IsVisible)
				return true;
			return false;
		}


		public static bool IsCloseable(int popNr)
		{
			String name = "StaticPopup" + popNr + "CloseButton";
			GInterfaceObject obj = GContext.Main.Interface.GetByName(name);
			if (obj != null && obj.IsVisible)
				return true;
			return false;
		}


		public static string GetText(int popNr)
		{
			String name = "StaticPopup" + popNr;
			GInterfaceObject obj = GContext.Main.Interface.GetByName(name);
			if (obj == null || !obj.IsVisible)
				return null;
			GInterfaceObject text = obj.GetChildObject(name + "Text");
			if (text == null)
				return null;
			return text.LabelText;

		}

		public static bool ClickButton(int popNr, int buttonNr)
		{
			String name = "StaticPopup" + popNr + "Button" + buttonNr;
			GInterfaceObject obj = GContext.Main.Interface.GetByName(name);
			if (obj != null && obj.IsVisible)
			{
				Functions.Click(obj, false);
				return true;
			}
			return false;
		}

		public static bool ClickClose(int popNr)
		{
			String name = "StaticPopup" + popNr + "CloseButton";
			GInterfaceObject obj = GContext.Main.Interface.GetByName(name);
			if (obj != null && obj.IsVisible)
			{
				Functions.Click(obj, false);
				return true;
			}
			return false;
		}
	}
}
