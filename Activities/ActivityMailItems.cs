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
using System.Threading;

using Glider.Common.Objects;
using Pather.Tasks;
using Pather.Helpers;
using Pather.Helpers.UI;
using Pather.Graph;

namespace Pather.Activities {
	public class ActivityMailItems : Activity {
		GNode mailbox;
		string to;
		bool MailGreens, MailBlues, MailEpics;
		List<string> items;
		bool FullStacksOnly;
		List<string> protitems;
		BagManager bMan = new BagManager();

		public ActivityMailItems(Task t, GNode mailbox,
								string to, List<string> items, List<string> protitems,
								bool FullStacksOnly,
								bool MailGreens, bool MailBlues, bool MailEpics)
			: base(t, "Mail") {
			this.mailbox = mailbox;
			this.to = to;
			this.items = items;
			this.FullStacksOnly = FullStacksOnly;
			this.protitems = protitems;
			this.MailGreens = MailGreens;
			this.MailBlues = MailBlues;
			this.MailEpics = MailEpics;
		}

		public override Location GetLocation() {
			return null;
		}

		/// <summary>
		/// Goes through your inventory sending up to 12 items.
		/// </summary>
		/// <returns>The number of sent items. 0 implies no items or an error.</returns>
		private int SendMail() 
		{
		
			bMan.CloseAllBags(); // Make sure for sanity
			Functions.Interact(mailbox);
			Thread.Sleep(1000); // Accomodate for lag

			if (!MailFrame.IsVisible()) return 0;
			if (!MailFrame.ClickSendMailTab()) return 0;
			Thread.Sleep(500); // Give the frame time to show
			if (!SendMailFrame.IsVisible()) return 0;

			GItem[] bagitems = bMan.GetAllItems();
			int citems = 0; // Mail frame only allows 12 items to be mailed, so we need to count

			foreach (GItem bagitem in bagitems) {
				if (ShouldMail(bagitem) && citems < 12) {
					citems++;
					PPather.WriteLine(string.Format("Mail: Adding {0} to mail, #{1}", bagitem.Name, citems));
					bMan.ClickItem(bagitem, true);
					Thread.Sleep(200); // Accomodate for lag
				}
			}

			if (citems > 0) {
				int coppers = GPlayerSelf.Me.Coinage;
				int total = citems * 30;
				if (total > coppers) {
					// Not enough money
					PPather.WriteLine("!Warning:Mail: Not enough money to mail items");
					return 0;
				}
				SendMailFrame.TypeTo(to);
				Thread.Sleep(200);

				if (SendMailFrame.CanSend()) {
					SendMailFrame.ClickSend();
					Thread.Sleep(2000); // Make sure if finishes sending.
					PPather.WriteLine("Mail: Items have been mailed");
				} else {
					PPather.WriteLine("Mail: Unable to send. Button not accessible");
					citems = 0;
				}
			}

			SendMailFrame.Close();
			bMan.CloseAllBags();
			return citems;
		}

		public override bool Do() {
			// keep mailing as long as there is something to mail in
			// our inventory

			int mailed = 0;

			while (true) {
				int i = SendMail();

				if (i == 0) break;

				mailed += i;
				PPather.WriteLine("Mail: Sent mail, checking for more...");
			}

			PPather.WriteLine("Mail: Done mailing, sent " + mailed + " items");
			//ppather.Blacklist(mailbox.GUID, 15 * 60); // Don't use for another 15 minutes.

			return true;
		}

		bool IsProtected(string item) // To allow for partial matches
		{
			foreach (string pitem in protitems) {
				if (item.Contains(pitem)) return true;
			}
			return false;
		}

		private bool ShouldMail(GItem item) {
			if (IsProtected(item.Name) ||
				(FullStacksOnly && item.StackSize != item.Definition.StackSize) ||
				item.IsSoulbound) return false;

			foreach (string litem in items) {
				if (item.Name.Contains(litem)) return true;
			}
			if (item.Definition.Quality == GItemQuality.Uncommon && MailGreens == true)
				return true;
			if (item.Definition.Quality == GItemQuality.Rare && MailBlues == true)
				return true;
			if (item.Definition.Quality == GItemQuality.Epic && MailEpics == true)
				return true;
			return false;
		}
	}
}
