using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WalletWasabi.Fluent.TorSysTray
{
	class SysTrayApplicationContext : ApplicationContext
	{
		//Component declarations
		private NotifyIcon _trayIcon;
		private ContextMenuStrip _trayIconContextMenu;
		private ToolStripMenuItem _closeMenuItem;

		private Timer _timer;

		System.Diagnostics.Process _torClientProcess;

		public SysTrayApplicationContext(string[] args)
		{
			_torClientProcess = Process.GetProcessById(int.Parse(args[0]));

			Application.ApplicationExit += new EventHandler(OnApplicationExit);
			InitializeComponent();
			_trayIcon.Visible = true;			
		}

		private void InitializeComponent()
		{
			_trayIcon = new NotifyIcon();
			_trayIcon.Text = "Wasabi Wallet Tor";
			_trayIcon.Icon = Properties.Resources.TrayIcon;

			_trayIconContextMenu = new ContextMenuStrip();
			_closeMenuItem = new ToolStripMenuItem();

			_trayIconContextMenu.SuspendLayout();

			// TrayIconContextMenu
			_trayIconContextMenu.Items.AddRange(new ToolStripItem[] {_closeMenuItem});
			_trayIconContextMenu.Name = "TrayIconContextMenu";
			_trayIconContextMenu.Size = new Size(153, 70);

			// CloseMenuItem
			_closeMenuItem.Name = "CloseMenuItem";
			_closeMenuItem.Size = new Size(152, 22);
			_closeMenuItem.Text = "Quit";
			_closeMenuItem.Click += new EventHandler(CloseMenuItem_Click);

			_trayIconContextMenu.ResumeLayout(false);
			_trayIcon.ContextMenuStrip = _trayIconContextMenu;

			_timer = new Timer();
			_timer.Interval = 500;
			_timer.Tick += new EventHandler(Timer_Tick);
			_timer.Enabled = true;
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			if(_torClientProcess.HasExited)
			{
				Application.Exit();
			}
		}

		private void OnApplicationExit(object sender, EventArgs e)
		{
			_trayIcon.Visible = false;
		}

		private void CloseMenuItem_Click(object sender, EventArgs e)
		{
			_torClientProcess.Kill();
		}
	}
}
