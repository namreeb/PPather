using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI
{
	public class TaxiFrame
	{
		public static GInterfaceObject GetFrame()
		{
			return GContext.Main.Interface.GetByName("TaxiFrame");
		}

		public static GInterfaceObject GetButton(int buttonId)
		{
			return GContext.Main.Interface.GetByName("TaxiButton" + buttonId);
		}

		public static bool IsVisible()
		{
			GInterfaceObject obj = GetFrame();
			if (obj != null && obj.IsVisible)
				return true;
			return false;
		}

		public static bool HasTaxiButton(int buttonId)
		{

			GInterfaceObject obj = GetFrame();
			if (obj != null && obj.IsVisible)
			{
				GInterfaceObject subObj = GetButton(buttonId);
				if (subObj != null && subObj.IsVisible)
					return true;
			}

			return false;
		}

		public static void ClickTaxiButton(int buttonId)
		{
			GInterfaceObject obj = GetButton(buttonId);
			if (obj != null && obj.IsVisible)
			{
				Functions.Hover(obj);
				//Thread.Sleep(1000);

				// and off we go!
				Functions.Click(obj, false);
			}
		}

		public static int GetButtonId(String destination)
		{
			// lets hope there's never more than 64 flight paths :)
			for (int i = 1; i < 65; i++)
			{
				if (HasTaxiButton(i) && CheckTaxiButton(i, destination))
				{
					return i;
				}
			}

			return 0;
		}

		public static bool CheckTaxiButton(int buttonId, String destination)
		{
			GInterfaceObject btnObj = GetButton(buttonId);
			if (btnObj != null && btnObj.IsVisible)
			{
				Functions.Hover(btnObj);
				Thread.Sleep(1000);

				String ttText = CheckToolTip();  // jea, why not?

				if (ttText != null && destination != null &&
					destination.Length > 0 && ttText.Length > 0 &&
					ttText.ToLower().Contains(destination.ToLower()))
				{
					return true;
				}
			}

			return false;
		}

		public static String CheckToolTip()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("GameTooltip");
			if (btn != null && btn.IsVisible)
			{
				GInterfaceObject text = btn.GetChildObject("GameTooltipTextLeft1");

				if (text != null)
				{
					return text.LabelText;
				}
			}

			return null;
		}
	}
}
