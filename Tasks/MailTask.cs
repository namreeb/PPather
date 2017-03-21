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

using Glider.Common.Objects;
using Pather;
using Pather.Activities;
using Pather.Graph;
using Pather.Parser;
using Pather.Helpers.UI;

namespace Pather.Tasks {
	public class MailTask : ParserTask {
		GNode mailbox;
		Location location;
		bool UseMount;

		public MailTask(PPather pather, NodeTask node)
			: base(pather, node) {
			location = node.GetValueOfId("Location").GetLocationValue();
			UseMount = node.GetBoolValueOfId("UseMount");
		}

		public override void GetParams(List<string> l) {
			l.Add("To");
			l.Add("Items");
			l.Add("FullStacksOnly");
			l.Add("Location");
			l.Add("MinFreeBagSlots");
			l.Add("UseMount");
			base.GetParams(l);
		}

		#region Gets

		private int GetMinFreeBagslots() {
			Value v = nodetask.GetValueOfId("MinFreeBagSlots");
			if (v == null) return -1;
			return v.GetIntValue();
		}

		private int GetFreeBagslots() {
			return nodetask.GetIntValueOfId("FreeBagSlots");
		}

		private string GetRecipient() {
			return nodetask.GetValueOfId("To").GetStringValue();
		}

		private List<string> GetItems() {
			Value v = nodetask.GetValueOfId("Items");
			if (v == null) return null;
			return v.GetStringCollectionValues();
		}

		private bool GetFullStacksOnly() {
			Value v = nodetask.GetValueOfId("FullStacksOnly");
			if (v == null) return false;
			return v.GetBoolValue();
		}

		private List<string> GetProtectedItems() {
			Value v = nodetask.GetValueOfId("Protected");
			if (v == null) return null;
			return v.GetStringCollectionValues();
		}

		private bool GetMailGreens() {
			return nodetask.GetBoolValueOfId("MailGreens");
		}

		private bool GetMailBlues() {
			return nodetask.GetBoolValueOfId("MailBlues");
		}

		private bool GetMailEpics() {
			return nodetask.GetBoolValueOfId("MailEpics");
		}
		#endregion

		public override bool IsFinished() {
			if (!NeedMail()) return true;
			return false;
		}

		public override string ToString() {
			return "Mailing items";
		}

		private bool ValidItem(GItem item) {
			List<string> prots = GetProtectedItems();
			if (item.IsSoulbound ||
				((prots != null) && prots.Contains(item.Name)))
				return false;
			return true;
		}

		private bool NeedMail() {
			BagManager bMan = new BagManager();
			bMan.UpdateItems();
			GItem[] bitems = bMan.GetAllItems();
			List<string> items = GetItems();
			foreach (string item in items) {
				foreach (GItem bitem in bitems) {
					if (bitem.Name.Contains(item) && ValidItem(bitem)) {
						if (!GetFullStacksOnly()) {
							return true;
						} else {
							PPather.WriteLine(string.Format("[{0}]x{1} ==? {2}", bitem.Name, bitem.StackSize, bitem.Definition.StackSize));
							if (bitem.StackSize == bitem.Definition.StackSize)
								return true;
						}
					}
				}
			}
			return false;
		}

		public override Location GetLocation() {
			if (location != null) return location;
			return null;
		}

		GNode FindMailbox() {
			// Find mailbox
			GNode[] nodes = GObjectList.GetNodes();
			foreach (GNode node in nodes) {
				if (node.IsMailBox) {
					if (mailbox == null || node.DistanceToSelf < mailbox.DistanceToSelf) {
						mailbox = node;
					}
				}
			}
			return mailbox;
		}

		public override bool WantToDoSomething() {
			if (location == null) return false;
			//bool blacklisted = mailbox != null && ppather.IsBlacklisted(mailbox.GUID);
			return (NeedMail() && GetFreeBagslots() < GetMinFreeBagslots());
		}

		private ActivityWalkTo approachMailbox;
		private ActivityWalkTo walkToLocation;
		private ActivityMailItems mailItems;

		public override Activity GetActivity() {
			mailbox = FindMailbox();
			if (mailbox == null) {
				if (walkToLocation == null)
					walkToLocation = new ActivityWalkTo(this, location, 2f, UseMount);
				return walkToLocation;
			} else {
				if (mailbox.DistanceToSelf < 5.0) {
					if (mailItems == null) {
						mailItems = new ActivityMailItems(this,
														  mailbox,
														  GetRecipient(),
														  GetItems(),
														  GetProtectedItems(),
														  GetFullStacksOnly(),
														  GetMailGreens(),
														  GetMailBlues(),
														  GetMailEpics()
														  );
					}
					return mailItems;
				} else if (approachMailbox == null)
					approachMailbox = new ActivityWalkTo(this, new Location(mailbox.Location), 2f, UseMount);
				return approachMailbox;
			}
		}

		public override bool ActivityDone(Activity task) {
			if (task == mailItems) return true;
			return false;
		}
	}
}
