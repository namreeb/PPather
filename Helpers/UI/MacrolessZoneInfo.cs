using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI
{
	public class MacrolessZoneInfo
	{
		public static String GetZoneText()
		{
			return GContext.Main.ZoneText;
			/*GInterfaceObject formZone = GContext.Main.Interface.GetByName("ZoneTextFrame");
			if (formZone != null)
			{
				GInterfaceObject textZone = formZone.GetChildObject("ZoneTextString");

				if (textZone != null)
				{
					return textZone.LabelText;
				}
			}

			return null;*/
		}

		public static String GetSubZoneText()
		{
			//return GContext.Main.SubZoneText;
			return "subzone";
			/*GInterfaceObject formSubZone = GContext.Main.Interface.GetByName("SubZoneTextFrame");
			if (formSubZone != null)
			{
				GInterfaceObject textSubZone = formSubZone.GetChildObject("SubZoneTextString");

				if (textSubZone != null)
				{
					return textSubZone.LabelText;
				}
			}

			return null;*/

		}
	}
}
