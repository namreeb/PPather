using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI {
	// static methods to handle Merchant frames
	public class MerchantFrame {
		public static GInterfaceObject GetFrame() {
			return GContext.Main.Interface.GetByName("MerchantFrame");
		}

		public static bool IsVisible() {
			GInterfaceObject obj = GetFrame();
			if (obj != null && obj.IsVisible) return true;
			return false;
		}
	}
}
