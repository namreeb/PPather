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
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Text;
using Glider.Common.Objects;
using System.Windows.Forms;
using System.Drawing;

using Pather.Parser;
using Pather.Tasks;
using Pather.Activities;

namespace Pather
{
	public class Functions
	{
		static PatherForm form = PPather.form;

		public static string GenFormTitle(int size)
		{
			StringBuilder builder = new StringBuilder();
			Random rand = new Random();
			char ch;
			for (int x = 0; x < size; x++)
			{
				ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * rand.NextDouble() + 65)));
				builder.Append(ch);
			}
			return builder.ToString().ToLower();
		}
		#region Cursor Hook Stuff

		#region ClickRepairButton

		public static void ClickRepairButton(GMerchant repr)
		{
			if (repr != null && repr.IsRepairVisible)
			{
				GContext.Main.EnableCursorHook();
				repr.ClickRepairButton();
				GContext.Main.DisableCursorHook();
			}
		}

		#endregion

		#region Click

		public static void Click(GInterfaceObject lbtn)
		{
			Click(lbtn, false);
		}

		public static void Click(GInterfaceObject lbtn, bool right)
		{
			if (lbtn != null && lbtn.IsVisible)
			{
				GContext.Main.EnableCursorHook();
				lbtn.ClickMouse(right);
				GContext.Main.DisableCursorHook();
			}
		}

		#endregion

		#region Hover

		public static void Hover(GMonster m)
		{
			Hover((GUnit)m);
		}

		public static void Hover(GNode u)
		{
			if (u != null && u.IsValid)
			{
				GContext.Main.EnableCursorHook();
				u.Hover();
				GContext.Main.DisableCursorHook();
			}
		}

		public static void Hover(GInterfaceObject u)
		{
			if (u != null && u.IsVisible)
			{
				GContext.Main.EnableCursorHook();
				u.Hover();
				GContext.Main.DisableCursorHook();
			}
		}

		public static void Hover(GUnit u)
		{
			if (u != null && u.IsValid)
			{
				GContext.Main.EnableCursorHook();
				u.Hover();
				GContext.Main.DisableCursorHook();
			}
		}

		#endregion

		#region Interact

		public static void Interact(GUnit u)
		{
			if (u != null && u.IsValid)
			{
				GContext.Main.EnableCursorHook();
				u.Interact();
				GContext.Main.DisableCursorHook();
			}
		}

		public static void Interact(GNode u)
		{
			if (u != null && u.IsValid)
			{
				GContext.Main.EnableCursorHook();
				u.Interact();
				GContext.Main.DisableCursorHook();
			}
		}

		public static void Interact(PathObject u)
		{
			if (u != null)
			{
				GContext.Main.EnableCursorHook();
				u.Interact();
				GContext.Main.DisableCursorHook();
			}
		}

		#endregion


		#endregion
		public static void SaveSettings()
		{

			PPather.PatherSettings.FormTitle =
				form.txt_setting_formtitle.Text;
			PPather.PatherSettings.UseMount =
				form.cb_setting_usemount.SelectedItem.ToString();
			PPather.PatherSettings.MountRange =
				float.Parse(form.txt_setting_mountrange.Text);
			PPather.PatherSettings.MaxResurrection =
				form.chk_setting_maxres.Checked;
			PPather.PatherSettings.MaxResurrectionAmount =
				int.Parse(form.txt_setting_maxres.Text);
			PPather.PatherSettings.StopAtLevel =
				int.Parse(form.txt_setting_stoplevel.Text);
			try
			{
				PPather.PatherSettings.Save();
				PPather.WriteLine("Your settings have been saved");
			}
			catch
			{
				PPather.WriteLine("!Error:Couldn't access PPather.xml. Is it open in another program?");
			}
		}

		#region UI Stuff

		public static void dumpUi()
		{
			form.devTree.Nodes.Clear();
			string check = form.dumpUiFilter.Text.ToLower();

			foreach (string s in GInterfaceHelper.GetAllInterfaceObjectNames())
			{
				if (s == null || !s.ToLower().Contains(check))
					continue;

				GInterfaceObject root = GContext.Main.Interface.GetByName(s);

				if (form.dumpUiVisibleCB.Checked && !root.IsVisible)
					continue;

				TreeNode node = dumpUi_doChildren(ref root);

				form.devTree.Nodes.Add(node);
			}
		}

		private static TreeNode dumpUi_doChildren(ref GInterfaceObject gio)
		{
			// gio.Name is not defined...
			string s = gio.ToString();
			s = s.Substring(s.IndexOf('"') + 1);
			s = s.Substring(0, s.IndexOf('"'));

			//if (s.EndsWith("Text") || s.Contains("Button"))
			string lblText = gio.LabelText;

			if (lblText != "(read failed)")
				s += " \"" + lblText + "\"";

			TreeNode node = new TreeNode(s);

			for (int i = 0; i < gio.Children.Length; i++)
			{
				if (form.dumpUiVisibleCB.Checked && !gio.Children[i].IsVisible)
					continue;

				TreeNode childNode = dumpUi_doChildren(ref gio.Children[i]);
				node.Nodes.Add(childNode);
			}

			return node;
		}

		public static void dumpItemCount()
		{
			form.devTree.Nodes.Clear();

			Dictionary<string, int> inventory = Helpers.Inventory.CreateItemCount(false);

			List<string> keys = new List<string>(inventory.Keys);
			keys.Sort();

			foreach (string key in keys)
			{
				form.devTree.Nodes.Add(key + " x" + inventory[key]);
			}
		}

		private static List<Object> dumpGObjects_seenObjects = new List<Object>();

		public static void dumpGObjects(string what)
		{
			lock (dumpGObjects_seenObjects)
			{
				dumpGObjects_seenObjects.Clear();

				form.devTree.Nodes.Clear();
				string check = form.dumpUiFilter.Text.ToLower();

				try
				{
					MethodInfo mi = typeof(GObjectList).GetMethod(what);
					GObject[] objs = (GObject[])mi.Invoke(null, null);

					Array.Sort<GObject>(objs,
						new Comparison<GObject>(
							delegate(GObject o1, GObject o2)
							{
								return o1.Name.CompareTo(o2.Name);
							})
						);

					foreach (GObject obj in objs)
					{
						if (!obj.Name.ToLower().Contains(check))
							continue;

						// if (obj == GPlayerSelf.Me) continue; // crashing ...

						PPather.WriteLine("Dumping: " + obj.Name);

						TreeNode node = dumpObjectRecursive(obj.Name, obj);
						form.devTree.Nodes.Add(node);
					}
				}
				catch (Exception e)
				{
					string s = e.GetType().FullName + "\n" +
						e.Message + "\n\n";

					if (e.InnerException != null)
					{
						s += e.InnerException.GetType().FullName + "\n" +
							e.InnerException.Message + "\n\n";
					}

					s += e.StackTrace;

					MessageBox.Show(s);
				}
			}
		}

		private static BindingFlags dumpObjectBindingFlags =
			BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

		private static TreeNode dumpObjectRecursive(string name, Object obj)
		{
			TreeNode node = new TreeNode(name + " = ");

			if (null == obj)
			{
				node.Text += "[NULL]";
				return node;
			}

			if (!(obj is System.ValueType) && dumpGObjects_seenObjects.Contains(obj))
			{
				node.Text += "[RECURSION]";
				return node;
			}

			dumpGObjects_seenObjects.Add(obj);

			node.Text += obj.ToString();

			// PPather.WriteLine("   " + node.Text);

			if (obj is Array)
			{
				Array arr = (Array)obj;

				for (int i = 0; i < arr.Length; i++)
				{
					TreeNode child = dumpObjectRecursive(i.ToString(), arr.GetValue(i));

					if (null != child)
						node.Nodes.Add(child);
				}

				return node;
			}

			if (!(obj is System.ValueType))
			{
				PropertyInfo[] pis = obj.GetType().GetProperties(dumpObjectBindingFlags);

				Array.Sort<PropertyInfo>(pis,
					new Comparison<PropertyInfo>(
						delegate(PropertyInfo o1, PropertyInfo o2)
						{
							return o1.Name.CompareTo(o2.Name);
						})
					);

				foreach (PropertyInfo pi in pis)
				{
					// PPather.WriteLine("-- Checking " + name + "." + pi.Name);
					ParameterInfo[] parami = pi.GetIndexParameters();

					if (parami.Length > 0)
						continue;

					try
					{
						TreeNode child = dumpObjectRecursive(pi.Name, pi.GetValue(obj, null));

						if (null != child)
							node.Nodes.Add(child);
					}
					catch
					{
					}
				}
			}

			return node;
		}
		#endregion
	}
}