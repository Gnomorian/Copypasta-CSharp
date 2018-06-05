using System;
using System.Drawing;
using System.Windows.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.Menu;
using Microsoft.Win32;

//TODO: modify click event so settings menu updates the settings
//TODO: reverse the list of items so exit is always at the bottom for people who set 100s of clips and therefore cant close
//TODO: add MenuItem to open settings window, then hide settings window at startup
//TODO: in ConfirmClicked add handling for reducing maxClicks below current max when it is currently filled bigger than new max.
//TODO: do i need a log?

namespace Copypasta
{
    public class CopypastaApp : Form
    {
        // Tray Variables
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        // Registry key to save to
        private static string key = "HKEY_CURRENT_USER\\Copypasta";

        // a timer on the same thread as clipboard only works on the main thread.
        private static DispatcherTimer timer = new DispatcherTimer();

        // settings form elements
        private GroupBox grpMaxClips;
        private Label lblMaxClips;
        private NumericUpDown numMaxClips;
        private GroupBox grpTimerInterval;
        private Label lblTimerInterval;
        private NumericUpDown numTimerInterval;
        private Button btnConfirm;

        // modifyable settings
        private int maxClips = 10;
        private int timerInterval = 200;

        [STAThread]
        public static void Main()
        {
            CopypastaApp app = new CopypastaApp();
            Application.Run(app);
        }

        public int GetSetting(string subKey)
        {
            int rInt = (int)Registry.GetValue(key, subKey, -1);

            return rInt;
        }
        /* set the key in the registry to the given value */
        public void SetSetting(string subKey, int value)
        {
            Registry.SetValue(key, subKey, value);
        }
        /* Initialize settings from the registry */
        public void InitSettings()
        {
            string userRoot = "HKEY_CURRENT_USER";
            string key = "Copypasta";
            string keyName = userRoot + "\\" + key;

            int regKey = GetSetting("timer_interval");
            if (regKey == -1)
            {
                SetSetting("timer_interval", timerInterval);
                regKey = timerInterval;
            }
            timerInterval = regKey;

            regKey = GetSetting("max_clips");
            if (regKey == -1)
            {
                SetSetting("max_clips", maxClips);
                regKey = maxClips;
            }
            maxClips = regKey;
        }

        /* set settings of the timer */
        private void SetTimer(int interval)
        {
            timer.Interval = TimeSpan.FromMilliseconds(interval);
            timer.Tick += UpdateClipboard;
            timer.Start();
        }

        /* Method called on timer to add clips to Tray Menu */
        private void UpdateClipboard(object sender, EventArgs eventArgs)
        {
            if(!Clipboard.ContainsText())
                return;

            string clip = Clipboard.GetText();
            if (!ContainsClip(clip))
            {
                AddClip(clip);
            }
        }
        /* tests if the provided clip is in the menu already */
        private bool ContainsClip(string clip)
        {
            foreach (MenuItem item in trayIcon.ContextMenu.MenuItems)
            {
                if(item.Text == clip)
                {
                    return true;
                }
            }
            return false;
        }

        /* Method called on Menu Click, Replaces the current clipboard text with that of the menu clicked */
        private void ReplaceClipboard(object sender, EventArgs e)
        {
            Clipboard.SetText(((MenuItem)sender).Text);
        }

        /* Adds a new Clip to the Menu and removes eldest clip if we are adding more than the maxClips */
        private void AddClip(string clip)
        {
            MenuItemCollection items = trayIcon.ContextMenu.MenuItems;
            if (items.Count > maxClips)
            {
                items.RemoveAt(1);
            }
            items.Add(clip, ReplaceClipboard);
        }
        /* event handler run when the settings menu button Confirm is clicked */
        private void ConfirmClicked(object sender, EventArgs e)
        {
            if(this.numMaxClips.Value != maxClips)
            {
                maxClips = (int)this.numMaxClips.Value;
                SetSetting("max_clips", maxClips);
            }
            if (this.numTimerInterval.Value != timerInterval)
            {
                timerInterval = (int)this.numTimerInterval.Value;
                timer.Stop();
                timer.Interval = TimeSpan.FromMilliseconds(timerInterval);
                SetSetting("timer_interval", timerInterval);
            }
        }

        /* Setup the tray */
        public CopypastaApp()
        {
            // initialize settings from the registry
            InitSettings();
            
            // setup the timer
            SetTimer(timerInterval);

            // Create a tray menu with an Exit menu item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Create a tray icon.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Copypasta";
            trayIcon.Icon = new Icon("./pasta.ico");

            // Add the menu to the tray icon.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = true;
            ShowInTaskbar = false;
            // initialize the settings form
            InitializeComponent();

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            timer.Stop();
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if(e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
            }
        }

        private void InitializeComponent()
        {
            this.grpMaxClips = new System.Windows.Forms.GroupBox();
            this.numMaxClips = new System.Windows.Forms.NumericUpDown();
            this.lblMaxClips = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.grpTimerInterval = new System.Windows.Forms.GroupBox();
            this.numTimerInterval = new System.Windows.Forms.NumericUpDown();
            this.lblTimerInterval = new System.Windows.Forms.Label();
            this.grpMaxClips.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxClips)).BeginInit();
            this.grpTimerInterval.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimerInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // grpMaxClips
            // 
            this.grpMaxClips.Controls.Add(this.numMaxClips);
            this.grpMaxClips.Controls.Add(this.lblMaxClips);
            this.grpMaxClips.Location = new System.Drawing.Point(12, 12);
            this.grpMaxClips.Name = "grpMaxClips";
            this.grpMaxClips.Size = new System.Drawing.Size(185, 41);
            this.grpMaxClips.TabIndex = 3;
            this.grpMaxClips.TabStop = false;
            // 
            // numMaxClips
            // 
            this.numMaxClips.Location = new System.Drawing.Point(6, 10);
            this.numMaxClips.Minimum = 0;
            this.numMaxClips.Name = "numMaxClips";
            this.numMaxClips.Size = new System.Drawing.Size(56, 20);
            this.numMaxClips.TabIndex = 2;
            this.numMaxClips.Value = maxClips;
            // 
            // lblMaxClips
            // 
            this.lblMaxClips.AutoSize = true;
            this.lblMaxClips.Location = new System.Drawing.Point(93, 16);
            this.lblMaxClips.Name = "lblMaxClips";
            this.lblMaxClips.Size = new System.Drawing.Size(86, 13);
            this.lblMaxClips.TabIndex = 1;
            this.lblMaxClips.Text = "Max Clips Saved";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(50, 137);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(113, 35);
            this.btnConfirm.TabIndex = 4;
            this.btnConfirm.Text = "Confirm";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += ConfirmClicked;
            // 
            // grpTimerInterval
            // 
            this.grpTimerInterval.Controls.Add(this.numTimerInterval);
            this.grpTimerInterval.Controls.Add(this.lblTimerInterval);
            this.grpTimerInterval.Location = new System.Drawing.Point(12, 59);
            this.grpTimerInterval.Name = "grpTimerInterval";
            this.grpTimerInterval.Size = new System.Drawing.Size(185, 35);
            this.grpTimerInterval.TabIndex = 5;
            this.grpTimerInterval.TabStop = false;
            // 
            // numTimerInterval
            // 
            this.numTimerInterval.Location = new System.Drawing.Point(6, 10);
            this.numTimerInterval.Name = "numTimerInterval";
            this.numTimerInterval.Size = new System.Drawing.Size(56, 20);
            this.numTimerInterval.TabIndex = 3;
            this.numTimerInterval.Value = timerInterval;
            this.numTimerInterval.Minimum = 0;
            // 
            // lblTimerInterval
            // 
            this.lblTimerInterval.AutoSize = true;
            this.lblTimerInterval.Location = new System.Drawing.Point(93, 16);
            this.lblTimerInterval.Name = "lblTimerInterval";
            this.lblTimerInterval.Size = new System.Drawing.Size(76, 13);
            this.lblTimerInterval.TabIndex = 1;
            this.lblTimerInterval.Text = "Polling Interval";
            // 
            // CopypastaApp
            // 
            this.ClientSize = new System.Drawing.Size(214, 184);
            this.Controls.Add(this.grpTimerInterval);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.grpMaxClips);
            this.Name = "Copypasta";
            this.grpMaxClips.ResumeLayout(false);
            this.grpMaxClips.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxClips)).EndInit();
            this.grpTimerInterval.ResumeLayout(false);
            this.grpTimerInterval.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTimerInterval)).EndInit();
            this.ResumeLayout(false);
        }

    }
}