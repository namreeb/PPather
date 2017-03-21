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
	public class PatherForm : Form
	{
		#region Declarations

		private PPather pather;
		private NotifyIcon nicon;
		private Button btn_pause;
		private Button btn_go;
		private Button btn_getloc;
		private Button btn_stop;
		private Button btn_load;
		private Button btn_tray;
		private TabPage tabPage7;
		public ComboBox dumpCombo;
		public CheckBox dumpUiVisibleCB;
		public TreeView devTree;
		public TextBox dumpUiFilter;
		private Button dumpUiBtn;
		private TabPage tabPage3;
		private TabPage tabPage2;
		private TreeView treeView1;
		private ListBox lb_params;
		private TabPage tabPage1;
		private Label lblPatherVersion;
		private GroupBox groupBox2;
		private Label lbl_deaths;
		private Label label16;
		private Label lbl_ttl;
		private Label label18;
		private Label lbl_harvest;
		private Label label14;
		private Label lbl_XPh;
		private Label label11;
		private Label lbl_loots;
		private Label label8;
		private Label lbl_kills;
		private Label label6;
		private GroupBox groupBox1;
		private Label label7;
		private Label lbl_loc;
		private Label label4;
		private Label lbl_subzone;
		private Label label17;
		private Label lbl_zone;
		private Label label13;
		private Label label15;
		private Label label2;
		private Label lbl_activity;
		private Label label5;
		private Label lbl_state;
		private GroupBox grpBox;
		private Label label1;
		private Label lbl_name;
		private Label lbl_reaction;
		private Label lbl_level;
		private Label lbl_faction;
		private Label label12;
		private Label label3;
		private Label label9;
		private TabControl tabControl1;
		private Label label24;
		private TextBox textBox1;
		private TextBox textBox2;
		private TextBox textBox3;
		private TextBox textBox4;
		private Label label25;
		private CheckBox checkBox1;
		private Label label26;
		private Label label27;
		private Label label28;
		private ComboBox comboBox1;
		private Button button1;
		private Label label29;
		private Label label31;
		private Label label32;
		private TextBox textBox5;
		private TextBox textBox6;
		private TextBox textBox7;
		private TextBox textBox8;
		private Label label33;
		private CheckBox checkBox2;
		private Label label34;
		private Label label35;
		private Label label36;
		private ComboBox comboBox2;
		private Button button2;
		private Label label37;
		private Label label38;
		private Button btn_save_settings;
		private Panel settings_scroll_pannel;
		public TextBox txt_setting_stoplevel;
		public TextBox txt_setting_maxres;
		public TextBox txt_setting_mountrange;
		public TextBox txt_setting_formtitle;
		private Label label30;
		public CheckBox chk_setting_maxres;
		private Label label23;
		private Label label22;
		private Label label21;
		public ComboBox cb_setting_usemount;
		private Button btn_random_formtitle;
		private Label label20;
		private Label label19;
		private Label label40;
		private Button btn_default;
		private PictureBox pictureBox1;
		private Label label10;
		private TextBox textBox9;
		TreeNode rootNode = null;
		#endregion

		public PatherForm(PPather pather)
		{
			this.pather = pather;
			InitializeComponent();
			//Application.EnableVisualStyles(); // Changes the look of Glider too
			SetTaskFileName();
			GetSettings();

			lblPatherVersion.Text += PPather.VERSION;

			dumpCombo.SelectedIndex = 0;
		}

		private void GetSettings()
		{
			this.Text = PPather.PatherSettings.FormTitle;
			nicon.Text = PPather.PatherSettings.FormTitle;
			txt_setting_formtitle.Text = PPather.PatherSettings.FormTitle;
			cb_setting_usemount.SelectedItem = PPather.PatherSettings.UseMount;
			txt_setting_mountrange.Text = PPather.PatherSettings.MountRange.ToString();
			txt_setting_maxres.Text = PPather.PatherSettings.MaxResurrectionAmount.ToString();
			chk_setting_maxres.Checked = PPather.PatherSettings.MaxResurrection;
			txt_setting_stoplevel.Text = PPather.PatherSettings.StopAtLevel.ToString();
		}

		private TreeNode CreateNodeFromTask(Task task)
		{
			Task[] children = task.GetChildren();
			TreeNode[] childNodes = null;
			if (children != null)
			{
				childNodes = new TreeNode[children.Length];
				for (int i = 0; i < children.Length; i++)
				{
					childNodes[i] = CreateNodeFromTask(children[i]);
				}
			}

			TreeNode n;
			if (childNodes != null)
				n = new TreeNode(task.ToString(), childNodes);
			else
				n = new TreeNode(task.ToString());
			n.Tag = task;
			return n;
		}

		#region Colors
		private Color white = System.Drawing.Color.FromArgb(255, 255, 255);
		private Color yellow = System.Drawing.Color.FromArgb(255, 255, 128);
		private Color red = System.Drawing.Color.FromArgb(255, 128, 128);
		private Color green = System.Drawing.Color.FromArgb(128, 255, 128);
		private Color blue = System.Drawing.Color.FromArgb(155, 165, 255);
		#endregion

		private void UpdateTreeNodeStatus(TreeNode n)
		{
			Task t = (Task)n.Tag;

			if (t != null)
			{
				Task.State_e state = t.State;
				if (state == Task.State_e.Idle)
					n.BackColor = white;
				else if (state == Task.State_e.Done)
					n.BackColor = blue;
				else if (state == Task.State_e.Active)
					n.BackColor = green;
				else if (state == Task.State_e.Want)
					n.BackColor = yellow;

			}

			TreeNode child = n.FirstNode;
			while (child != null)
			{
				UpdateTreeNodeStatus(child);
				child = child.NextNode;
			}
		}

		public void CreateTreeFromTasks(Task rootTask)
		{
			rootNode = CreateNodeFromTask(rootTask);
		}

		#region Set*
		public void SetStatus(int kills, int KPh, int loots, int XPh, int harvests, int ttl, int deaths)
		{
			lbl_kills.Text = "" + kills + ",  " + KPh + "K/h";
			lbl_loots.Text = "" + loots;
			lbl_XPh.Text = "" + XPh;
			lbl_harvest.Text = "" + harvests;
			int mins = ttl % 60;
			int hs = (ttl - mins) / 60;
			lbl_ttl.Text = "" + hs + "h " + mins + "m";
			lbl_deaths.Text = "" + deaths;
		}


		public void SetTarget(GUnit target)
		{
			if (target == null)
			{
				lbl_name.Text = "";
				lbl_reaction.Text = "";
				lbl_level.Text = "";
				lbl_faction.Text = "";
			}
			else
			{
				lbl_name.Text = target.Name;
				lbl_reaction.Text = target.Reaction.ToString();
				string lvl = target.Level.ToString();
				if (target.IsMonster)
				{
					GMonster m = (GMonster)target;
					if (m.IsElite) lvl += "elite";
				}
				lbl_level.Text = lvl;
				lbl_faction.Text = target.FactionID.ToString();
			}
		}

		public void SetActivity(Activity activity)
		{
			if (activity == null)
			{
				lbl_activity.Text = "";
			}
			else
			{
				lbl_activity.Text = activity.ToString();
			}
		}

		public void SetLocation(string location)
		{
			if (location != null)
				lbl_loc.Text = location;
		}

		public void SetZone(string zone, string subzone)
		{
			if (subzone != null)
				lbl_subzone.Text = subzone;
			if (zone != null)
				lbl_zone.Text = zone;

		}

		public void SetTaskFileName()
		{
			string file;
			string[] parts = PPather.PatherSettings.TaskFile.Split('\\');
			file = parts[parts.Length - 1];
			label15.Text = file;
		}
		#endregion

		private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			TreeNode n = e.Node;
			lb_params.Items.Clear();
			if (n == null) return;
			Task t = (Task)n.Tag;
			if (t == null)
			{

			}
			else
			{
				if (!t.IsParserTask())
				{

				}
				else
				{
					//PPather.WriteLine("marked a task " + t);
					ParserTask pt = (ParserTask)t;
					List<string> parms = new List<string>();
					pt.GetParams(parms);
					NodeTask tn = pt.nodetask;

					foreach (string par in parms)
					{
						Value v = tn.GetValueOfId(par);
						string val = "undefined";
						if (v != null) val = v.GetStringValue();
						lb_params.Items.Add(par + " = " + val);
					}

				}
			}
		}

		#region Buttons
		private void btn_go_Click(object sender, EventArgs e)
		{
			if (GContext.Main.IsAttached && !GContext.Main.IsGliding)
				GContext.Main.StartGlide();
			pather.WantedState = PPather.RunState_e.Running;
		}

		private void btn_pause_Click(object sender, EventArgs e)
		{
			pather.WantedState = PPather.RunState_e.Paused;
			GContext.Main.DisableCursorHook();
		}

		private void btn_stop_Click(object sender, EventArgs e)
		{
			pather.WantedState = PPather.RunState_e.Stopped;
			GContext.Main.DisableCursorHook();
		}

		private void btn_getloc_Click(object sender, EventArgs e)
		{
			if (GContext.Main.IsAttached)
				PPather.WriteLine(pather.GetCurrentLocation());
			else
				PPather.WriteLine("!Warning:Cannot get current location");
		}

		private void btn_load_Click(object sender, EventArgs e)
		{
			OpenFileDialog oF = new OpenFileDialog();
			oF.Filter = "PSC Files (*.psc)|*.psc";
			oF.Title = "Select task file...";
			oF.RestoreDirectory = true;
			oF.FileName = PPather.PatherSettings.TaskFile;

			if (oF.ShowDialog() == DialogResult.OK)
			{
				PPather.PatherSettings.TaskFile = oF.FileName;
				SetTaskFileName();
				PPather.PatherSettings.Save();
			}
		}

		private void btn_tray_Click(object sender, EventArgs e)
		{
			this.Hide();
		}


		private void btn_random_formtitle_Click(object sender, EventArgs e)
		{
			string newTitle = Functions.GenFormTitle(10);
			txt_setting_formtitle.Text = newTitle;
		}
		#endregion

		#region UI Dumper
		private void dumpUiBtn_Click(object sender, EventArgs e)
		{
			string s = dumpCombo.SelectedItem.ToString();

			switch (s)
			{
				case "": break;
				case "UI": Functions.dumpUi(); break;
				case "ItemCount": Functions.dumpItemCount(); break;
				default: Functions.dumpGObjects(s); break;
			}
		}
		#endregion

		private void nicon_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (this.Visible) this.Activate();
			else this.Show();
		}

		// copy the label's text to the clipboard
		private void generic_lbl_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			try
			{
				if (sender is Label)
				{
					Label l = (Label)sender;

					if (l.Text != "")
						Clipboard.SetData(DataFormats.Text, l.Text);
				}
			}
			catch
			{
				PPather.WriteLine("!Error:Couldn't copy location to clipboard.");
			}
		}

		private GSpellTimer npc_update = new GSpellTimer(3000);

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (GContext.Main != null && GContext.Main.Me != null && GContext.Main.Me.IsValid)
			{
				GUnit target = GContext.Main.Me.Target;
				SetTarget(target);
				if (npc_update.IsReady)
				{
					pather.UpdateNPCs();
					npc_update.Reset();
				}
			}
			if (pather.RunState != pather.WantedState)
				lbl_state.Text = pather.RunState.ToString() + " (" + pather.WantedState.ToString() + ")";
			else
				lbl_state.Text = pather.RunState.ToString();

			if (GContext.Main.IsAttached)
				SetLocation(pather.GetCurrentLocation());

			#region Load Button Stuff
			if (pather.RunState == PPather.RunState_e.Stopped
				|| pather.RunState == PPather.RunState_e.Paused)
			{
				btn_load.Enabled = true;
			}
			else
			{
				btn_load.Enabled = false;
			}
			#endregion
			#region Setting Stuff
			if (chk_setting_maxres.Checked)
				txt_setting_maxres.Enabled = true;
			else
				txt_setting_maxres.Enabled = false;
			#endregion

			if (GContext.Main.CurrentMode == GGlideMode.Glide)
			{
				if (rootNode != null)
				{
					treeView1.Nodes.Clear();
					treeView1.Nodes.Add(rootNode);
					rootNode = null;
				}
				foreach (TreeNode n in treeView1.Nodes)
				{
					UpdateTreeNodeStatus(n);
				}
			}
		}

		#region Required stuff
		private System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PatherForm));
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.nicon = new System.Windows.Forms.NotifyIcon(this.components);
			this.btn_pause = new System.Windows.Forms.Button();
			this.btn_go = new System.Windows.Forms.Button();
			this.btn_getloc = new System.Windows.Forms.Button();
			this.btn_stop = new System.Windows.Forms.Button();
			this.btn_load = new System.Windows.Forms.Button();
			this.btn_tray = new System.Windows.Forms.Button();
			this.tabPage7 = new System.Windows.Forms.TabPage();
			this.dumpCombo = new System.Windows.Forms.ComboBox();
			this.dumpUiVisibleCB = new System.Windows.Forms.CheckBox();
			this.devTree = new System.Windows.Forms.TreeView();
			this.dumpUiFilter = new System.Windows.Forms.TextBox();
			this.dumpUiBtn = new System.Windows.Forms.Button();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.btn_default = new System.Windows.Forms.Button();
			this.settings_scroll_pannel = new System.Windows.Forms.Panel();
			this.txt_setting_stoplevel = new System.Windows.Forms.TextBox();
			this.txt_setting_maxres = new System.Windows.Forms.TextBox();
			this.txt_setting_mountrange = new System.Windows.Forms.TextBox();
			this.txt_setting_formtitle = new System.Windows.Forms.TextBox();
			this.label30 = new System.Windows.Forms.Label();
			this.chk_setting_maxres = new System.Windows.Forms.CheckBox();
			this.label23 = new System.Windows.Forms.Label();
			this.label22 = new System.Windows.Forms.Label();
			this.label21 = new System.Windows.Forms.Label();
			this.cb_setting_usemount = new System.Windows.Forms.ComboBox();
			this.btn_random_formtitle = new System.Windows.Forms.Button();
			this.label20 = new System.Windows.Forms.Label();
			this.label19 = new System.Windows.Forms.Label();
			this.label40 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.btn_save_settings = new System.Windows.Forms.Button();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.lb_params = new System.Windows.Forms.ListBox();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lbl_deaths = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.lbl_ttl = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.lbl_harvest = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.lbl_XPh = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.lbl_loots = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.lbl_kills = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label7 = new System.Windows.Forms.Label();
			this.lbl_loc = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.lbl_subzone = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.lbl_zone = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.lbl_activity = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.lbl_state = new System.Windows.Forms.Label();
			this.grpBox = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.lbl_name = new System.Windows.Forms.Label();
			this.lbl_reaction = new System.Windows.Forms.Label();
			this.lbl_level = new System.Windows.Forms.Label();
			this.lbl_faction = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.lblPatherVersion = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.label24 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.label25 = new System.Windows.Forms.Label();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.label26 = new System.Windows.Forms.Label();
			this.label27 = new System.Windows.Forms.Label();
			this.label28 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.button1 = new System.Windows.Forms.Button();
			this.label29 = new System.Windows.Forms.Label();
			this.label31 = new System.Windows.Forms.Label();
			this.label32 = new System.Windows.Forms.Label();
			this.textBox5 = new System.Windows.Forms.TextBox();
			this.textBox6 = new System.Windows.Forms.TextBox();
			this.textBox7 = new System.Windows.Forms.TextBox();
			this.textBox8 = new System.Windows.Forms.TextBox();
			this.label33 = new System.Windows.Forms.Label();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.label34 = new System.Windows.Forms.Label();
			this.label35 = new System.Windows.Forms.Label();
			this.label36 = new System.Windows.Forms.Label();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.button2 = new System.Windows.Forms.Button();
			this.label37 = new System.Windows.Forms.Label();
			this.label38 = new System.Windows.Forms.Label();
			this.textBox9 = new System.Windows.Forms.TextBox();
			this.tabPage7.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.settings_scroll_pannel.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.grpBox.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 333;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// nicon
			// 
			this.nicon.Icon = ((System.Drawing.Icon)(resources.GetObject("nicon.Icon")));
			this.nicon.Visible = true;
			this.nicon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.nicon_MouseDoubleClick);
			// 
			// btn_pause
			// 
			this.btn_pause.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btn_pause.BackColor = System.Drawing.SystemColors.Control;
			this.btn_pause.FlatAppearance.BorderSize = 0;
			this.btn_pause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btn_pause.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btn_pause.Location = new System.Drawing.Point(64, 264);
			this.btn_pause.Name = "btn_pause";
			this.btn_pause.Size = new System.Drawing.Size(62, 27);
			this.btn_pause.TabIndex = 19;
			this.btn_pause.Text = "Pause";
			this.btn_pause.UseVisualStyleBackColor = false;
			this.btn_pause.Click += new System.EventHandler(this.btn_pause_Click);
			// 
			// btn_go
			// 
			this.btn_go.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btn_go.BackColor = System.Drawing.SystemColors.Control;
			this.btn_go.FlatAppearance.BorderSize = 0;
			this.btn_go.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btn_go.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btn_go.Location = new System.Drawing.Point(5, 264);
			this.btn_go.Name = "btn_go";
			this.btn_go.Size = new System.Drawing.Size(55, 27);
			this.btn_go.TabIndex = 20;
			this.btn_go.Text = "Start";
			this.btn_go.UseVisualStyleBackColor = false;
			this.btn_go.Click += new System.EventHandler(this.btn_go_Click);
			// 
			// btn_getloc
			// 
			this.btn_getloc.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btn_getloc.BackColor = System.Drawing.SystemColors.Control;
			this.btn_getloc.FlatAppearance.BorderSize = 0;
			this.btn_getloc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btn_getloc.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btn_getloc.Location = new System.Drawing.Point(188, 264);
			this.btn_getloc.Name = "btn_getloc";
			this.btn_getloc.Size = new System.Drawing.Size(73, 27);
			this.btn_getloc.TabIndex = 30;
			this.btn_getloc.Text = "HotSpot";
			this.btn_getloc.UseVisualStyleBackColor = false;
			this.btn_getloc.Click += new System.EventHandler(this.btn_getloc_Click);
			// 
			// btn_stop
			// 
			this.btn_stop.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btn_stop.BackColor = System.Drawing.SystemColors.Control;
			this.btn_stop.FlatAppearance.BorderSize = 0;
			this.btn_stop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btn_stop.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btn_stop.Location = new System.Drawing.Point(130, 264);
			this.btn_stop.Name = "btn_stop";
			this.btn_stop.Size = new System.Drawing.Size(54, 27);
			this.btn_stop.TabIndex = 21;
			this.btn_stop.Text = "Stop";
			this.btn_stop.UseVisualStyleBackColor = false;
			this.btn_stop.Click += new System.EventHandler(this.btn_stop_Click);
			// 
			// btn_load
			// 
			this.btn_load.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btn_load.BackColor = System.Drawing.SystemColors.Control;
			this.btn_load.FlatAppearance.BorderSize = 0;
			this.btn_load.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btn_load.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btn_load.Location = new System.Drawing.Point(265, 264);
			this.btn_load.Name = "btn_load";
			this.btn_load.Size = new System.Drawing.Size(53, 27);
			this.btn_load.TabIndex = 31;
			this.btn_load.Text = "Load";
			this.btn_load.UseVisualStyleBackColor = false;
			this.btn_load.Click += new System.EventHandler(this.btn_load_Click);
			// 
			// btn_tray
			// 
			this.btn_tray.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btn_tray.BackColor = System.Drawing.SystemColors.Control;
			this.btn_tray.FlatAppearance.BorderSize = 0;
			this.btn_tray.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btn_tray.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btn_tray.Location = new System.Drawing.Point(322, 264);
			this.btn_tray.Name = "btn_tray";
			this.btn_tray.Size = new System.Drawing.Size(49, 27);
			this.btn_tray.TabIndex = 35;
			this.btn_tray.Text = "Tray";
			this.btn_tray.UseVisualStyleBackColor = false;
			this.btn_tray.Click += new System.EventHandler(this.btn_tray_Click);
			// 
			// tabPage7
			// 
			this.tabPage7.AutoScroll = true;
			this.tabPage7.AutoScrollMinSize = new System.Drawing.Size(330, 150);
			this.tabPage7.Controls.Add(this.dumpCombo);
			this.tabPage7.Controls.Add(this.dumpUiVisibleCB);
			this.tabPage7.Controls.Add(this.devTree);
			this.tabPage7.Controls.Add(this.dumpUiFilter);
			this.tabPage7.Controls.Add(this.dumpUiBtn);
			this.tabPage7.Location = new System.Drawing.Point(4, 22);
			this.tabPage7.Margin = new System.Windows.Forms.Padding(2);
			this.tabPage7.Name = "tabPage7";
			this.tabPage7.Padding = new System.Windows.Forms.Padding(2);
			this.tabPage7.Size = new System.Drawing.Size(361, 229);
			this.tabPage7.TabIndex = 3;
			this.tabPage7.Text = "Dev";
			this.tabPage7.UseVisualStyleBackColor = true;
			// 
			// dumpCombo
			// 
			this.dumpCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.dumpCombo.BackColor = System.Drawing.SystemColors.Menu;
			this.dumpCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.dumpCombo.FormattingEnabled = true;
			this.dumpCombo.Items.AddRange(new object[] {
			"UI",
			"ItemCount",
			"GetItems",
			"GetMonsters",
			"GetNodes",
			"GetObjects",
			"GetPlayers",
			"GetUnits"});
			this.dumpCombo.Location = new System.Drawing.Point(97, 199);
			this.dumpCombo.Margin = new System.Windows.Forms.Padding(2);
			this.dumpCombo.Name = "dumpCombo";
			this.dumpCombo.Size = new System.Drawing.Size(82, 21);
			this.dumpCombo.TabIndex = 6;
			// 
			// dumpUiVisibleCB
			// 
			this.dumpUiVisibleCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.dumpUiVisibleCB.AutoSize = true;
			this.dumpUiVisibleCB.Checked = true;
			this.dumpUiVisibleCB.CheckState = System.Windows.Forms.CheckState.Checked;
			this.dumpUiVisibleCB.Location = new System.Drawing.Point(241, 202);
			this.dumpUiVisibleCB.Margin = new System.Windows.Forms.Padding(2);
			this.dumpUiVisibleCB.Name = "dumpUiVisibleCB";
			this.dumpUiVisibleCB.Size = new System.Drawing.Size(80, 17);
			this.dumpUiVisibleCB.TabIndex = 5;
			this.dumpUiVisibleCB.Text = "Only Visible";
			this.dumpUiVisibleCB.UseVisualStyleBackColor = true;
			// 
			// devTree
			// 
			this.devTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.devTree.BackColor = System.Drawing.Color.Ivory;
			this.devTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.devTree.Location = new System.Drawing.Point(3, 3);
			this.devTree.Margin = new System.Windows.Forms.Padding(2);
			this.devTree.Name = "devTree";
			this.devTree.Size = new System.Drawing.Size(359, 192);
			this.devTree.TabIndex = 4;
			// 
			// dumpUiFilter
			// 
			this.dumpUiFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.dumpUiFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dumpUiFilter.Location = new System.Drawing.Point(3, 199);
			this.dumpUiFilter.Margin = new System.Windows.Forms.Padding(2);
			this.dumpUiFilter.Name = "dumpUiFilter";
			this.dumpUiFilter.Size = new System.Drawing.Size(90, 20);
			this.dumpUiFilter.TabIndex = 2;
			// 
			// dumpUiBtn
			// 
			this.dumpUiBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.dumpUiBtn.BackColor = System.Drawing.Color.LightGray;
			this.dumpUiBtn.FlatAppearance.BorderSize = 0;
			this.dumpUiBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.dumpUiBtn.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dumpUiBtn.Location = new System.Drawing.Point(183, 199);
			this.dumpUiBtn.Margin = new System.Windows.Forms.Padding(2);
			this.dumpUiBtn.Name = "dumpUiBtn";
			this.dumpUiBtn.Size = new System.Drawing.Size(54, 21);
			this.dumpUiBtn.TabIndex = 1;
			this.dumpUiBtn.Text = "Dump";
			this.dumpUiBtn.UseVisualStyleBackColor = false;
			this.dumpUiBtn.Click += new System.EventHandler(this.dumpUiBtn_Click);
			// 
			// tabPage3
			// 
			this.tabPage3.AutoScroll = true;
			this.tabPage3.Controls.Add(this.btn_default);
			this.tabPage3.Controls.Add(this.settings_scroll_pannel);
			this.tabPage3.Controls.Add(this.btn_save_settings);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage3.Size = new System.Drawing.Size(361, 229);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Settings";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// btn_default
			// 
			this.btn_default.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_default.BackColor = System.Drawing.SystemColors.ControlLight;
			this.btn_default.FlatAppearance.BorderSize = 0;
			this.btn_default.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btn_default.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btn_default.Location = new System.Drawing.Point(211, 200);
			this.btn_default.MinimumSize = new System.Drawing.Size(54, 21);
			this.btn_default.Name = "btn_default";
			this.btn_default.Size = new System.Drawing.Size(71, 21);
			this.btn_default.TabIndex = 54;
			this.btn_default.Text = "Default";
			this.btn_default.UseVisualStyleBackColor = false;
			this.btn_default.Click += new System.EventHandler(this.btn_default_Click);
			// 
			// settings_scroll_pannel
			// 
			this.settings_scroll_pannel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.settings_scroll_pannel.AutoScroll = true;
			this.settings_scroll_pannel.AutoScrollMinSize = new System.Drawing.Size(340, 150);
			this.settings_scroll_pannel.BackColor = System.Drawing.Color.Ivory;
			this.settings_scroll_pannel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.settings_scroll_pannel.Controls.Add(this.txt_setting_stoplevel);
			this.settings_scroll_pannel.Controls.Add(this.txt_setting_maxres);
			this.settings_scroll_pannel.Controls.Add(this.txt_setting_mountrange);
			this.settings_scroll_pannel.Controls.Add(this.txt_setting_formtitle);
			this.settings_scroll_pannel.Controls.Add(this.label30);
			this.settings_scroll_pannel.Controls.Add(this.chk_setting_maxres);
			this.settings_scroll_pannel.Controls.Add(this.label23);
			this.settings_scroll_pannel.Controls.Add(this.label22);
			this.settings_scroll_pannel.Controls.Add(this.label21);
			this.settings_scroll_pannel.Controls.Add(this.cb_setting_usemount);
			this.settings_scroll_pannel.Controls.Add(this.btn_random_formtitle);
			this.settings_scroll_pannel.Controls.Add(this.label20);
			this.settings_scroll_pannel.Controls.Add(this.label19);
			this.settings_scroll_pannel.Controls.Add(this.label40);
			this.settings_scroll_pannel.Controls.Add(this.label10);
			this.settings_scroll_pannel.Location = new System.Drawing.Point(3, 3);
			this.settings_scroll_pannel.Name = "settings_scroll_pannel";
			this.settings_scroll_pannel.Size = new System.Drawing.Size(353, 190);
			this.settings_scroll_pannel.TabIndex = 53;
			// 
			// txt_setting_stoplevel
			// 
			this.txt_setting_stoplevel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txt_setting_stoplevel.Location = new System.Drawing.Point(83, 130);
			this.txt_setting_stoplevel.MaxLength = 4;
			this.txt_setting_stoplevel.Name = "txt_setting_stoplevel";
			this.txt_setting_stoplevel.Size = new System.Drawing.Size(35, 20);
			this.txt_setting_stoplevel.TabIndex = 82;
			// 
			// txt_setting_maxres
			// 
			this.txt_setting_maxres.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txt_setting_maxres.Location = new System.Drawing.Point(123, 101);
			this.txt_setting_maxres.MaxLength = 4;
			this.txt_setting_maxres.Name = "txt_setting_maxres";
			this.txt_setting_maxres.Size = new System.Drawing.Size(35, 20);
			this.txt_setting_maxres.TabIndex = 79;
			// 
			// txt_setting_mountrange
			// 
			this.txt_setting_mountrange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txt_setting_mountrange.Location = new System.Drawing.Point(120, 70);
			this.txt_setting_mountrange.MaxLength = 4;
			this.txt_setting_mountrange.Name = "txt_setting_mountrange";
			this.txt_setting_mountrange.Size = new System.Drawing.Size(35, 20);
			this.txt_setting_mountrange.TabIndex = 75;
			// 
			// txt_setting_formtitle
			// 
			this.txt_setting_formtitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txt_setting_formtitle.Location = new System.Drawing.Point(62, 8);
			this.txt_setting_formtitle.Name = "txt_setting_formtitle";
			this.txt_setting_formtitle.Size = new System.Drawing.Size(113, 20);
			this.txt_setting_formtitle.TabIndex = 71;
			// 
			// label30
			// 
			this.label30.AutoSize = true;
			this.label30.BackColor = System.Drawing.Color.Ivory;
			this.label30.Location = new System.Drawing.Point(6, 133);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(66, 13);
			this.label30.TabIndex = 81;
			this.label30.Text = "Stop at level";
			// 
			// chk_setting_maxres
			// 
			this.chk_setting_maxres.AutoSize = true;
			this.chk_setting_maxres.BackColor = System.Drawing.Color.Transparent;
			this.chk_setting_maxres.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.chk_setting_maxres.Location = new System.Drawing.Point(102, 105);
			this.chk_setting_maxres.Name = "chk_setting_maxres";
			this.chk_setting_maxres.Size = new System.Drawing.Size(12, 11);
			this.chk_setting_maxres.TabIndex = 78;
			this.chk_setting_maxres.UseVisualStyleBackColor = false;
			// 
			// label23
			// 
			this.label23.AutoSize = true;
			this.label23.BackColor = System.Drawing.Color.White;
			this.label23.Location = new System.Drawing.Point(3, 104);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(90, 13);
			this.label23.TabIndex = 77;
			this.label23.Text = "Max resurrections";
			// 
			// label22
			// 
			this.label22.AutoSize = true;
			this.label22.BackColor = System.Drawing.Color.Ivory;
			this.label22.Location = new System.Drawing.Point(3, 73);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(111, 13);
			this.label22.TabIndex = 76;
			this.label22.Text = "Min distance to mount";
			// 
			// label21
			// 
			this.label21.AutoSize = true;
			this.label21.BackColor = System.Drawing.Color.White;
			this.label21.Location = new System.Drawing.Point(3, 43);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(58, 13);
			this.label21.TabIndex = 74;
			this.label21.Text = "Use mount";
			// 
			// cb_setting_usemount
			// 
			this.cb_setting_usemount.BackColor = System.Drawing.SystemColors.Menu;
			this.cb_setting_usemount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cb_setting_usemount.FormattingEnabled = true;
			this.cb_setting_usemount.Items.AddRange(new object[] {
			"Always Mount",
			"Never Mount",
			"Let Task Decide"});
			this.cb_setting_usemount.Location = new System.Drawing.Point(67, 39);
			this.cb_setting_usemount.Margin = new System.Windows.Forms.Padding(2);
			this.cb_setting_usemount.Name = "cb_setting_usemount";
			this.cb_setting_usemount.Size = new System.Drawing.Size(113, 21);
			this.cb_setting_usemount.TabIndex = 80;
			// 
			// btn_random_formtitle
			// 
			this.btn_random_formtitle.FlatAppearance.BorderSize = 0;
			this.btn_random_formtitle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btn_random_formtitle.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btn_random_formtitle.Location = new System.Drawing.Point(183, 8);
			this.btn_random_formtitle.Name = "btn_random_formtitle";
			this.btn_random_formtitle.Size = new System.Drawing.Size(73, 21);
			this.btn_random_formtitle.TabIndex = 73;
			this.btn_random_formtitle.Text = "Random";
			this.btn_random_formtitle.UseVisualStyleBackColor = true;
			this.btn_random_formtitle.Click += new System.EventHandler(this.btn_random_formtitle_Click);
			// 
			// label20
			// 
			this.label20.AutoSize = true;
			this.label20.BackColor = System.Drawing.Color.Ivory;
			this.label20.Location = new System.Drawing.Point(3, 11);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(49, 13);
			this.label20.TabIndex = 72;
			this.label20.Text = "Form title";
			// 
			// label19
			// 
			this.label19.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label19.BackColor = System.Drawing.Color.White;
			this.label19.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label19.Location = new System.Drawing.Point(-28, 34);
			this.label19.MinimumSize = new System.Drawing.Size(338, 30);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(438, 30);
			this.label19.TabIndex = 84;
			// 
			// label40
			// 
			this.label40.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label40.BackColor = System.Drawing.Color.White;
			this.label40.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label40.Location = new System.Drawing.Point(-28, 96);
			this.label40.MinimumSize = new System.Drawing.Size(338, 30);
			this.label40.Name = "label40";
			this.label40.Size = new System.Drawing.Size(438, 30);
			this.label40.TabIndex = 86;
			// 
			// label10
			// 
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label10.BackColor = System.Drawing.Color.White;
			this.label10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label10.Location = new System.Drawing.Point(-38, 153);
			this.label10.MinimumSize = new System.Drawing.Size(338, 30);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(438, 30);
			this.label10.TabIndex = 87;
			// 
			// btn_save_settings
			// 
			this.btn_save_settings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btn_save_settings.BackColor = System.Drawing.SystemColors.ControlLight;
			this.btn_save_settings.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlText;
			this.btn_save_settings.FlatAppearance.BorderSize = 0;
			this.btn_save_settings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btn_save_settings.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btn_save_settings.Location = new System.Drawing.Point(288, 200);
			this.btn_save_settings.MinimumSize = new System.Drawing.Size(54, 21);
			this.btn_save_settings.Name = "btn_save_settings";
			this.btn_save_settings.Size = new System.Drawing.Size(71, 21);
			this.btn_save_settings.TabIndex = 52;
			this.btn_save_settings.Text = "Save";
			this.btn_save_settings.UseVisualStyleBackColor = false;
			this.btn_save_settings.Click += new System.EventHandler(this.btn_save_settings_Click);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.treeView1);
			this.tabPage2.Controls.Add(this.lb_params);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(361, 229);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Script";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// treeView1
			// 
			this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.treeView1.BackColor = System.Drawing.Color.Ivory;
			this.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeView1.Location = new System.Drawing.Point(3, 3);
			this.treeView1.Name = "treeView1";
			this.treeView1.Size = new System.Drawing.Size(358, 121);
			this.treeView1.TabIndex = 18;
			this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
			// 
			// lb_params
			// 
			this.lb_params.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lb_params.BackColor = System.Drawing.Color.Ivory;
			this.lb_params.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lb_params.FormattingEnabled = true;
			this.lb_params.Location = new System.Drawing.Point(3, 127);
			this.lb_params.Name = "lb_params";
			this.lb_params.Size = new System.Drawing.Size(358, 93);
			this.lb_params.TabIndex = 22;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.pictureBox1);
			this.tabPage1.Controls.Add(this.groupBox2);
			this.tabPage1.Controls.Add(this.groupBox1);
			this.tabPage1.Controls.Add(this.grpBox);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(361, 229);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "General";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(208, 122);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(106, 106);
			this.pictureBox1.TabIndex = 35;
			this.pictureBox1.TabStop = false;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.lbl_deaths);
			this.groupBox2.Controls.Add(this.label16);
			this.groupBox2.Controls.Add(this.lbl_ttl);
			this.groupBox2.Controls.Add(this.label18);
			this.groupBox2.Controls.Add(this.lbl_harvest);
			this.groupBox2.Controls.Add(this.label14);
			this.groupBox2.Controls.Add(this.lbl_XPh);
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Controls.Add(this.lbl_loots);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.lbl_kills);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Location = new System.Drawing.Point(4, 4);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(116, 112);
			this.groupBox2.TabIndex = 17;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Status";
			// 
			// lbl_deaths
			// 
			this.lbl_deaths.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_deaths.Location = new System.Drawing.Point(50, 31);
			this.lbl_deaths.Name = "lbl_deaths";
			this.lbl_deaths.Size = new System.Drawing.Size(56, 13);
			this.lbl_deaths.TabIndex = 26;
			this.lbl_deaths.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.BackColor = System.Drawing.SystemColors.Control;
			this.label16.Location = new System.Drawing.Point(2, 31);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(41, 13);
			this.label16.TabIndex = 25;
			this.label16.Text = "Deaths";
			// 
			// lbl_ttl
			// 
			this.lbl_ttl.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_ttl.Location = new System.Drawing.Point(50, 91);
			this.lbl_ttl.Name = "lbl_ttl";
			this.lbl_ttl.Size = new System.Drawing.Size(56, 13);
			this.lbl_ttl.TabIndex = 24;
			this.lbl_ttl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.BackColor = System.Drawing.SystemColors.Control;
			this.label18.Location = new System.Drawing.Point(2, 92);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(27, 13);
			this.label18.TabIndex = 23;
			this.label18.Text = "TTL";
			// 
			// lbl_harvest
			// 
			this.lbl_harvest.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_harvest.Location = new System.Drawing.Point(50, 61);
			this.lbl_harvest.Name = "lbl_harvest";
			this.lbl_harvest.Size = new System.Drawing.Size(56, 13);
			this.lbl_harvest.TabIndex = 22;
			this.lbl_harvest.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.BackColor = System.Drawing.SystemColors.Control;
			this.label14.Location = new System.Drawing.Point(2, 62);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(49, 13);
			this.label14.TabIndex = 21;
			this.label14.Text = "Harvests";
			// 
			// lbl_XPh
			// 
			this.lbl_XPh.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_XPh.Location = new System.Drawing.Point(50, 76);
			this.lbl_XPh.Name = "lbl_XPh";
			this.lbl_XPh.Size = new System.Drawing.Size(56, 13);
			this.lbl_XPh.TabIndex = 20;
			this.lbl_XPh.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.BackColor = System.Drawing.SystemColors.Control;
			this.label11.Location = new System.Drawing.Point(2, 77);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(32, 13);
			this.label11.TabIndex = 19;
			this.label11.Text = "XP/h";
			// 
			// lbl_loots
			// 
			this.lbl_loots.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_loots.Location = new System.Drawing.Point(50, 46);
			this.lbl_loots.Name = "lbl_loots";
			this.lbl_loots.Size = new System.Drawing.Size(56, 13);
			this.lbl_loots.TabIndex = 18;
			this.lbl_loots.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.BackColor = System.Drawing.SystemColors.Control;
			this.label8.Location = new System.Drawing.Point(2, 47);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(33, 13);
			this.label8.TabIndex = 17;
			this.label8.Text = "Loots";
			// 
			// lbl_kills
			// 
			this.lbl_kills.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_kills.Location = new System.Drawing.Point(50, 16);
			this.lbl_kills.Name = "lbl_kills";
			this.lbl_kills.Size = new System.Drawing.Size(56, 13);
			this.lbl_kills.TabIndex = 16;
			this.lbl_kills.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.BackColor = System.Drawing.SystemColors.Control;
			this.label6.Location = new System.Drawing.Point(2, 16);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(25, 13);
			this.label6.TabIndex = 15;
			this.label6.Text = "Kills";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.lbl_loc);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.lbl_subzone);
			this.groupBox1.Controls.Add(this.label17);
			this.groupBox1.Controls.Add(this.lbl_zone);
			this.groupBox1.Controls.Add(this.label13);
			this.groupBox1.Controls.Add(this.label15);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.lbl_activity);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.lbl_state);
			this.groupBox1.Location = new System.Drawing.Point(126, 4);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(230, 112);
			this.groupBox1.TabIndex = 34;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Misc Info";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.BackColor = System.Drawing.SystemColors.Control;
			this.label7.Location = new System.Drawing.Point(3, 91);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(38, 13);
			this.label7.TabIndex = 38;
			this.label7.Text = "X/Y/Z";
			// 
			// lbl_loc
			// 
			this.lbl_loc.AutoEllipsis = true;
			this.lbl_loc.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_loc.Location = new System.Drawing.Point(58, 91);
			this.lbl_loc.Name = "lbl_loc";
			this.lbl_loc.Size = new System.Drawing.Size(163, 13);
			this.lbl_loc.TabIndex = 39;
			this.lbl_loc.UseCompatibleTextRendering = true;
			this.lbl_loc.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.BackColor = System.Drawing.SystemColors.Control;
			this.label4.Location = new System.Drawing.Point(3, 61);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(54, 13);
			this.label4.TabIndex = 36;
			this.label4.Text = "Sub Zone";
			// 
			// lbl_subzone
			// 
			this.lbl_subzone.AutoEllipsis = true;
			this.lbl_subzone.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_subzone.Location = new System.Drawing.Point(58, 61);
			this.lbl_subzone.Name = "lbl_subzone";
			this.lbl_subzone.Size = new System.Drawing.Size(163, 13);
			this.lbl_subzone.TabIndex = 37;
			this.lbl_subzone.UseCompatibleTextRendering = true;
			this.lbl_subzone.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.BackColor = System.Drawing.SystemColors.Control;
			this.label17.Location = new System.Drawing.Point(3, 46);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(32, 13);
			this.label17.TabIndex = 34;
			this.label17.Text = "Zone";
			// 
			// lbl_zone
			// 
			this.lbl_zone.AutoEllipsis = true;
			this.lbl_zone.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_zone.Location = new System.Drawing.Point(58, 46);
			this.lbl_zone.Name = "lbl_zone";
			this.lbl_zone.Size = new System.Drawing.Size(163, 13);
			this.lbl_zone.TabIndex = 35;
			this.lbl_zone.UseCompatibleTextRendering = true;
			this.lbl_zone.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.BackColor = System.Drawing.SystemColors.Control;
			this.label13.Location = new System.Drawing.Point(3, 76);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(50, 13);
			this.label13.TabIndex = 32;
			this.label13.Text = "Task File";
			// 
			// label15
			// 
			this.label15.AutoEllipsis = true;
			this.label15.BackColor = System.Drawing.SystemColors.Control;
			this.label15.Location = new System.Drawing.Point(58, 76);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(163, 13);
			this.label15.TabIndex = 33;
			this.label15.UseCompatibleTextRendering = true;
			this.label15.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.SystemColors.Control;
			this.label2.Location = new System.Drawing.Point(3, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(41, 13);
			this.label2.TabIndex = 28;
			this.label2.Text = "Activity";
			// 
			// lbl_activity
			// 
			this.lbl_activity.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_activity.Location = new System.Drawing.Point(58, 16);
			this.lbl_activity.Name = "lbl_activity";
			this.lbl_activity.Size = new System.Drawing.Size(163, 13);
			this.lbl_activity.TabIndex = 29;
			this.lbl_activity.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.BackColor = System.Drawing.SystemColors.Control;
			this.label5.Location = new System.Drawing.Point(3, 31);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(32, 13);
			this.label5.TabIndex = 26;
			this.label5.Text = "State";
			// 
			// lbl_state
			// 
			this.lbl_state.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_state.Location = new System.Drawing.Point(58, 31);
			this.lbl_state.Name = "lbl_state";
			this.lbl_state.Size = new System.Drawing.Size(163, 13);
			this.lbl_state.TabIndex = 27;
			this.lbl_state.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// grpBox
			// 
			this.grpBox.Controls.Add(this.label1);
			this.grpBox.Controls.Add(this.lbl_name);
			this.grpBox.Controls.Add(this.lbl_reaction);
			this.grpBox.Controls.Add(this.lbl_level);
			this.grpBox.Controls.Add(this.lbl_faction);
			this.grpBox.Controls.Add(this.label12);
			this.grpBox.Controls.Add(this.label3);
			this.grpBox.Controls.Add(this.label9);
			this.grpBox.Location = new System.Drawing.Point(4, 118);
			this.grpBox.Name = "grpBox";
			this.grpBox.Size = new System.Drawing.Size(185, 105);
			this.grpBox.TabIndex = 15;
			this.grpBox.TabStop = false;
			this.grpBox.Text = "Target";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.SystemColors.Control;
			this.label1.Location = new System.Drawing.Point(2, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Name";
			// 
			// lbl_name
			// 
			this.lbl_name.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_name.Location = new System.Drawing.Point(50, 16);
			this.lbl_name.Name = "lbl_name";
			this.lbl_name.Size = new System.Drawing.Size(125, 13);
			this.lbl_name.TabIndex = 1;
			this.lbl_name.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// lbl_reaction
			// 
			this.lbl_reaction.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_reaction.Location = new System.Drawing.Point(50, 61);
			this.lbl_reaction.Name = "lbl_reaction";
			this.lbl_reaction.Size = new System.Drawing.Size(125, 13);
			this.lbl_reaction.TabIndex = 3;
			this.lbl_reaction.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// lbl_level
			// 
			this.lbl_level.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_level.Location = new System.Drawing.Point(50, 31);
			this.lbl_level.Name = "lbl_level";
			this.lbl_level.Size = new System.Drawing.Size(125, 13);
			this.lbl_level.TabIndex = 9;
			this.lbl_level.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// lbl_faction
			// 
			this.lbl_faction.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_faction.Location = new System.Drawing.Point(50, 46);
			this.lbl_faction.Name = "lbl_faction";
			this.lbl_faction.Size = new System.Drawing.Size(125, 13);
			this.lbl_faction.TabIndex = 11;
			this.lbl_faction.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.generic_lbl_MouseDoubleClick);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.BackColor = System.Drawing.SystemColors.Control;
			this.label12.Location = new System.Drawing.Point(2, 46);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(42, 13);
			this.label12.TabIndex = 10;
			this.label12.Text = "Faction";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.BackColor = System.Drawing.SystemColors.Control;
			this.label3.Location = new System.Drawing.Point(2, 61);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(50, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Reaction";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.BackColor = System.Drawing.SystemColors.Control;
			this.label9.Location = new System.Drawing.Point(2, 31);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(33, 13);
			this.label9.TabIndex = 8;
			this.label9.Text = "Level";
			// 
			// lblPatherVersion
			// 
			this.lblPatherVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lblPatherVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPatherVersion.ForeColor = System.Drawing.Color.ForestGreen;
			this.lblPatherVersion.Location = new System.Drawing.Point(12, 534);
			this.lblPatherVersion.Name = "lblPatherVersion";
			this.lblPatherVersion.Size = new System.Drawing.Size(353, 14);
			this.lblPatherVersion.TabIndex = 37;
			this.lblPatherVersion.Text = "PPather ";
			this.lblPatherVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage7);
			this.tabControl1.Location = new System.Drawing.Point(5, 3);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(369, 255);
			this.tabControl1.TabIndex = 36;
			// 
			// label24
			// 
			this.label24.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label24.AutoSize = true;
			this.label24.BackColor = System.Drawing.Color.FloralWhite;
			this.label24.Location = new System.Drawing.Point(3, 34);
			this.label24.MinimumSize = new System.Drawing.Size(354, 30);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(354, 30);
			this.label24.TabIndex = 46;
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(83, 137);
			this.textBox1.MaxLength = 4;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(35, 20);
			this.textBox1.TabIndex = 44;
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(128, 109);
			this.textBox2.MaxLength = 4;
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(35, 20);
			this.textBox2.TabIndex = 41;
			// 
			// textBox3
			// 
			this.textBox3.Location = new System.Drawing.Point(123, 81);
			this.textBox3.MaxLength = 4;
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(35, 20);
			this.textBox3.TabIndex = 37;
			// 
			// textBox4
			// 
			this.textBox4.Location = new System.Drawing.Point(68, 8);
			this.textBox4.Name = "textBox4";
			this.textBox4.Size = new System.Drawing.Size(113, 20);
			this.textBox4.TabIndex = 33;
			// 
			// label25
			// 
			this.label25.AutoSize = true;
			this.label25.Location = new System.Drawing.Point(6, 140);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(71, 13);
			this.label25.TabIndex = 43;
			this.label25.Text = "Stop At Level";
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point(107, 112);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(15, 14);
			this.checkBox1.TabIndex = 40;
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// label26
			// 
			this.label26.AutoSize = true;
			this.label26.Location = new System.Drawing.Point(6, 112);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(95, 13);
			this.label26.TabIndex = 39;
			this.label26.Text = "Max Resurrections";
			// 
			// label27
			// 
			this.label27.AutoSize = true;
			this.label27.Location = new System.Drawing.Point(6, 84);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(111, 13);
			this.label27.TabIndex = 38;
			this.label27.Text = "Min distance to mount";
			// 
			// label28
			// 
			this.label28.AutoSize = true;
			this.label28.Location = new System.Drawing.Point(6, 56);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(59, 13);
			this.label28.TabIndex = 36;
			this.label28.Text = "Use Mount";
			// 
			// comboBox1
			// 
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
			"Always Mount",
			"Never Mount",
			"Let Task Decide"});
			this.comboBox1.Location = new System.Drawing.Point(70, 53);
			this.comboBox1.Margin = new System.Windows.Forms.Padding(2);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(113, 21);
			this.comboBox1.TabIndex = 42;
			// 
			// button1
			// 
			this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button1.Location = new System.Drawing.Point(187, 11);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(43, 16);
			this.button1.TabIndex = 35;
			this.button1.Text = "Random";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// label29
			// 
			this.label29.AutoSize = true;
			this.label29.Location = new System.Drawing.Point(9, 11);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(53, 13);
			this.label29.TabIndex = 34;
			this.label29.Text = "Form Title";
			// 
			// label31
			// 
			this.label31.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label31.AutoSize = true;
			this.label31.BackColor = System.Drawing.Color.WhiteSmoke;
			this.label31.Location = new System.Drawing.Point(3, 3);
			this.label31.MinimumSize = new System.Drawing.Size(354, 30);
			this.label31.Name = "label31";
			this.label31.Size = new System.Drawing.Size(354, 30);
			this.label31.TabIndex = 45;
			// 
			// label32
			// 
			this.label32.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label32.AutoSize = true;
			this.label32.BackColor = System.Drawing.Color.FloralWhite;
			this.label32.Location = new System.Drawing.Point(3, 34);
			this.label32.MinimumSize = new System.Drawing.Size(354, 30);
			this.label32.Name = "label32";
			this.label32.Size = new System.Drawing.Size(354, 30);
			this.label32.TabIndex = 46;
			// 
			// textBox5
			// 
			this.textBox5.Location = new System.Drawing.Point(83, 137);
			this.textBox5.MaxLength = 4;
			this.textBox5.Name = "textBox5";
			this.textBox5.Size = new System.Drawing.Size(35, 20);
			this.textBox5.TabIndex = 44;
			// 
			// textBox6
			// 
			this.textBox6.Location = new System.Drawing.Point(128, 109);
			this.textBox6.MaxLength = 4;
			this.textBox6.Name = "textBox6";
			this.textBox6.Size = new System.Drawing.Size(35, 20);
			this.textBox6.TabIndex = 41;
			// 
			// textBox7
			// 
			this.textBox7.Location = new System.Drawing.Point(123, 81);
			this.textBox7.MaxLength = 4;
			this.textBox7.Name = "textBox7";
			this.textBox7.Size = new System.Drawing.Size(35, 20);
			this.textBox7.TabIndex = 37;
			// 
			// textBox8
			// 
			this.textBox8.Location = new System.Drawing.Point(68, 8);
			this.textBox8.Name = "textBox8";
			this.textBox8.Size = new System.Drawing.Size(113, 20);
			this.textBox8.TabIndex = 33;
			// 
			// label33
			// 
			this.label33.AutoSize = true;
			this.label33.Location = new System.Drawing.Point(6, 140);
			this.label33.Name = "label33";
			this.label33.Size = new System.Drawing.Size(71, 13);
			this.label33.TabIndex = 43;
			this.label33.Text = "Stop At Level";
			// 
			// checkBox2
			// 
			this.checkBox2.AutoSize = true;
			this.checkBox2.Location = new System.Drawing.Point(107, 112);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(15, 14);
			this.checkBox2.TabIndex = 40;
			this.checkBox2.UseVisualStyleBackColor = true;
			// 
			// label34
			// 
			this.label34.AutoSize = true;
			this.label34.Location = new System.Drawing.Point(6, 112);
			this.label34.Name = "label34";
			this.label34.Size = new System.Drawing.Size(95, 13);
			this.label34.TabIndex = 39;
			this.label34.Text = "Max Resurrections";
			// 
			// label35
			// 
			this.label35.AutoSize = true;
			this.label35.Location = new System.Drawing.Point(6, 84);
			this.label35.Name = "label35";
			this.label35.Size = new System.Drawing.Size(111, 13);
			this.label35.TabIndex = 38;
			this.label35.Text = "Min distance to mount";
			// 
			// label36
			// 
			this.label36.AutoSize = true;
			this.label36.Location = new System.Drawing.Point(6, 56);
			this.label36.Name = "label36";
			this.label36.Size = new System.Drawing.Size(59, 13);
			this.label36.TabIndex = 36;
			this.label36.Text = "Use Mount";
			// 
			// comboBox2
			// 
			this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox2.FormattingEnabled = true;
			this.comboBox2.Items.AddRange(new object[] {
			"Always Mount",
			"Never Mount",
			"Let Task Decide"});
			this.comboBox2.Location = new System.Drawing.Point(70, 53);
			this.comboBox2.Margin = new System.Windows.Forms.Padding(2);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(113, 21);
			this.comboBox2.TabIndex = 42;
			// 
			// button2
			// 
			this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button2.Location = new System.Drawing.Point(187, 11);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(43, 16);
			this.button2.TabIndex = 35;
			this.button2.Text = "Random";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// label37
			// 
			this.label37.AutoSize = true;
			this.label37.Location = new System.Drawing.Point(9, 11);
			this.label37.Name = "label37";
			this.label37.Size = new System.Drawing.Size(53, 13);
			this.label37.TabIndex = 34;
			this.label37.Text = "Form Title";
			// 
			// label38
			// 
			this.label38.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.label38.AutoSize = true;
			this.label38.BackColor = System.Drawing.Color.WhiteSmoke;
			this.label38.Location = new System.Drawing.Point(3, 3);
			this.label38.MinimumSize = new System.Drawing.Size(354, 30);
			this.label38.Name = "label38";
			this.label38.Size = new System.Drawing.Size(354, 30);
			this.label38.TabIndex = 45;
			// 
			// textBox9
			// 
			this.textBox9.ForeColor = System.Drawing.SystemColors.WindowText;
			this.textBox9.Location = new System.Drawing.Point(5, 297);
			this.textBox9.MaxLength = 147483647;
			this.textBox9.Multiline = true;
			this.textBox9.Name = "textBox9";
			this.textBox9.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox9.Size = new System.Drawing.Size(366, 234);
			this.textBox9.TabIndex = 42;
			// 
			// PatherForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(385, 580);
			this.ControlBox = false;
			this.Controls.Add(this.textBox9);
			this.Controls.Add(this.lblPatherVersion);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.btn_tray);
			this.Controls.Add(this.btn_load);
			this.Controls.Add(this.btn_stop);
			this.Controls.Add(this.btn_getloc);
			this.Controls.Add(this.btn_go);
			this.Controls.Add(this.btn_pause);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximumSize = new System.Drawing.Size(993, 1978);
			this.MinimumSize = new System.Drawing.Size(393, 588);
			this.Name = "PatherForm";
			this.Text = "Form1";
			this.tabPage7.ResumeLayout(false);
			this.tabPage7.PerformLayout();
			this.tabPage3.ResumeLayout(false);
			this.settings_scroll_pannel.ResumeLayout(false);
			this.settings_scroll_pannel.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.grpBox.ResumeLayout(false);
			this.grpBox.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		#region More form declarations

		private System.Windows.Forms.Timer timer1;
		#endregion

		#endregion

		private void btn_save_settings_Click(object sender, EventArgs e)
		{
			Functions.SaveSettings();
			GetSettings();
		}

		private void btn_default_Click(object sender, EventArgs e)
		{
			PPather.PatherSettings.MakeDefault();
			GetSettings();
		}

		#region PPather Log

		private MessageType ParseMessageType(string message)
		{
			if (message.StartsWith("!Warning:"))
			{
				return MessageType.Warning;
			}
			else if (message.StartsWith("!Error:"))
			{
				return MessageType.Error;
			}
			else if (message.StartsWith("!Good:"))
			{
				return MessageType.Good;
			}
			else if (message.StartsWith("!Info:"))
			{
				return MessageType.Info;
			}
			return MessageType.Normal;
		}

		private enum MessageType
		{
			Warning,
			Error,
			Normal,
			Info,
			Good
		}

		public void Write(string message)
		{
			MessageType choosetype = ParseMessageType(message);
			//richTextBox1.SelectionFont = new Font("Arial", (float)8.5, FontStyle.Regular);
			switch (choosetype)
			{
				case MessageType.Error:
					message = message.Remove(0, 7);
					// richTextBox1.SelectionColor = Color.Red;
					// textBox9.ForeColor = System.Drawing.Color.Red;    was testing colors with regular txt box. I'm leaving them out.
					break;
				case MessageType.Warning:
					message = message.Remove(0, 9);
					// richTextBox1.SelectionColor = Color.OrangeRed;
					// textBox9.ForeColor = System.Drawing.Color.OrangeRed;
					break;
				case MessageType.Info:
					message = message.Remove(0, 6);
				   // richTextBox1.SelectionColor = Color.SteelBlue;
				   // textBox9.ForeColor = System.Drawing.Color.SteelBlue;
					break;
				case MessageType.Good:
					message = message.Remove(0, 6);
				   // richTextBox1.SelectionColor = Color.ForestGreen;
				   // textBox9.ForeColor = System.Drawing.Color.ForestGreen;
					
					break;
				default:
					// richTextBox1.SelectionColor = Color.Black;
					// textBox9.ForeColor = System.Drawing.Color.Black;
					break;
			}
			//richTextBox1.AppendText(" " + message);
			//richTextBox1.ScrollToCaret();
			textBox9.AppendText(" " + message);
			textBox9.ScrollToCaret();

			
		}

		public void WriteLine(string message)
		{
			this.Write(message + Environment.NewLine);
		}

		#endregion


	}
}
