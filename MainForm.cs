using System;
using System.Drawing;
using System.Windows.Forms;
using DuckDNSUpdater.Models;
using DuckDNSUpdater.Services;

namespace DuckDNSUpdater
{
    public partial class MainForm : Form
    {
        private readonly DuckDNSService _duckDNSService;
        private readonly ConfigurationManager _configManager;
        
        private System.Windows.Forms.Timer? updateTimer;
        private NotifyIcon? trayIcon;
        private bool isRunning = false;
        private string lastKnownIP = "";
        private DuckDNSConfig? currentConfig;
        
        public MainForm()
        {
            _duckDNSService = new DuckDNSService();
            _configManager = new ConfigurationManager();
            
            // Subscribe to events
            _duckDNSService.LogUpdated += OnServiceLogUpdated;
            _configManager.LogUpdated += OnServiceLogUpdated;
            
            InitializeComponent();
            LoadConfiguration();
            SetupTrayIcon();
            
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Tick += UpdateTimer_Tick;
        }
        
        private void OnServiceLogUpdated(object? sender, string message)
        {
            AddLog(message, Color.Cyan);
        }
        
        private void LoadConfiguration()
        {
            currentConfig = _configManager.LoadConfiguration();
            
            if (txtSubdomain != null) txtSubdomain.Text = currentConfig.Subdomain;
            if (txtToken != null) txtToken.Text = currentConfig.Token;
            if (numUpdateInterval != null) numUpdateInterval.Value = currentConfig.UpdateIntervalMinutes;
            if (chkAutoStart != null) chkAutoStart.Checked = currentConfig.AutoStart;
            if (chkMinimizeToTray != null) chkMinimizeToTray.Checked = currentConfig.MinimizeToTray;
            
            if (currentConfig.AutoStart && _configManager.ValidateConfiguration(currentConfig))
            {
                BtnStartStop_Click(null, null);
            }
        }
        
        private void SaveConfiguration()
        {
            if (currentConfig == null) return;
            
            currentConfig.Subdomain = txtSubdomain?.Text.Trim() ?? "";
            currentConfig.Token = txtToken?.Text.Trim() ?? "";
            currentConfig.UpdateIntervalMinutes = (int)(numUpdateInterval?.Value ?? 5);
            currentConfig.AutoStart = chkAutoStart?.Checked ?? false;
            currentConfig.MinimizeToTray = chkMinimizeToTray?.Checked ?? false;
            
            _configManager.SaveConfiguration(currentConfig);
        }
        
        private void BtnSave_Click(object? sender, EventArgs? e)
        {
            var tempConfig = new DuckDNSConfig 
            { 
                Subdomain = txtSubdomain?.Text.Trim() ?? "", 
                Token = txtToken?.Text.Trim() ?? "" 
            };
            
            if (!_configManager.ValidateConfiguration(tempConfig))
            {
                MessageBox.Show("Please enter both subdomain and token!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            SaveConfiguration();
        }
        
        private async void BtnUpdateNow_Click(object? sender, EventArgs? e)
        {
            if (currentConfig == null || !_configManager.ValidateConfiguration(currentConfig))
            {
                MessageBox.Show("Please enter subdomain and token first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            await UpdateDuckDNS();
        }
        
        private void BtnStartStop_Click(object? sender, EventArgs? e)
        {
            if (currentConfig == null || !_configManager.ValidateConfiguration(currentConfig))
            {
                MessageBox.Show("Please enter subdomain and token first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (isRunning)
            {
                StopUpdater();
            }
            else
            {
                StartUpdater();
            }
        }
        
        private void StartUpdater()
        {
            isRunning = true;
            if (btnStartStop != null) btnStartStop.Text = "Stop";
            if (lblStatus != null)
            {
                lblStatus.Text = "Status: Running";
                lblStatus.ForeColor = Color.Green;
            }
            
            if (updateTimer != null && numUpdateInterval != null)
            {
                updateTimer.Interval = (int)(numUpdateInterval.Value * 60 * 1000); // Convert to milliseconds
                updateTimer.Start();
            }
            
            AddLog($"Started automatic updates every {numUpdateInterval?.Value ?? 5} minutes", Color.Green);
        }
        
        private void StopUpdater()
        {
            isRunning = false;
            if (btnStartStop != null) btnStartStop.Text = "Start";
            if (lblStatus != null)
            {
                lblStatus.Text = "Status: Stopped";
                lblStatus.ForeColor = Color.Red;
            }
            
            updateTimer?.Stop();
            if (progressBar != null) progressBar.Visible = false;
            
            AddLog("Stopped automatic updates", Color.Yellow);
        }
        
        private async void UpdateTimer_Tick(object? sender, EventArgs? e)
        {
            await UpdateDuckDNS();
        }
        
        private async System.Threading.Tasks.Task UpdateDuckDNS()
        {
            if (currentConfig == null) return;
            
            try
            {
                if (progressBar != null) progressBar.Visible = true;
                
                var result = await _duckDNSService.CheckAndUpdateAsync(
                    currentConfig.Subdomain, 
                    currentConfig.Token, 
                    lastKnownIP);
                
                if (lblCurrentIP != null) lblCurrentIP.Text = $"Current IP: {result.CurrentIP}";
                
                if (result.UpdateSuccess && result.IPChanged)
                {
                    lastKnownIP = result.CurrentIP;
                    if (lblLastUpdate != null) lblLastUpdate.Text = $"Last Update: {DateTime.Now:HH:mm:ss MM/dd/yyyy}";
                }
                
                if (!result.UpdateSuccess)
                {
                    AddLog("Update failed", Color.Red);
                }
            }
            catch (Exception ex)
            {
                AddLog($"✗ Error: {ex.Message}", Color.Red);
            }
            finally
            {
                if (progressBar != null) progressBar.Visible = false;
            }
        }
        
        private void AddLog(string message, Color color)
        {
            if (txtLog == null) return;
            
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AddLog(message, color)));
                return;
            }
            
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;
            txtLog.SelectionColor = Color.Gray;
            txtLog.AppendText($"[{timestamp}] ");
            txtLog.SelectionColor = color;
            txtLog.AppendText($"{message}\n");
            txtLog.ScrollToCaret();
            
            // Keep only last 100 lines
            if (txtLog.Lines.Length > 100)
            {
                var lines = txtLog.Lines;
                txtLog.Lines = lines[^50..]; // Keep last 50 lines
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (chkMinimizeToTray?.Checked == true && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                if (trayIcon != null) 
                {
                    trayIcon.Visible = true;
                    trayIcon.ShowBalloonTip(2000, "DuckDNS Updater", "Application minimized to tray", ToolTipIcon.Info);
                }
            }
            else
            {
                SaveConfiguration();
                if (trayIcon != null) trayIcon.Visible = false;
                
                // Cleanup
                _duckDNSService.LogUpdated -= OnServiceLogUpdated;
                _configManager.LogUpdated -= OnServiceLogUpdated;
            }
            base.OnFormClosing(e);
        }
        
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            if (WindowState == FormWindowState.Minimized && chkMinimizeToTray?.Checked == true)
            {
                this.Hide();
                if (trayIcon != null) 
                {
                    trayIcon.Visible = true;
                    trayIcon.ShowBalloonTip(2000, "DuckDNS Updater", "Application minimized to tray", ToolTipIcon.Info);
                }
            }
        }
        
        private void SetupTrayIcon()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Icon = Properties.Resources.TrayIcon;
            trayIcon.Text = "DuckDNS Updater";
            trayIcon.Visible = false;
            
            ContextMenuStrip trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show Window", null, (sender, e) => {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
                if (trayIcon != null) trayIcon.Visible = false;
            });
            trayMenu.Items.Add("Update Now", null, (sender, e) => BtnUpdateNow_Click(sender, e));
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("Exit", null, (sender, e) => {
                if (trayIcon != null) trayIcon.Visible = false;
                Application.Exit();
            });
            
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.DoubleClick += (sender, e) => {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
                if (trayIcon != null) trayIcon.Visible = false;
            };
        }
        
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(value);
            if (!value && chkMinimizeToTray?.Checked == true && trayIcon != null)
            {
                trayIcon.Visible = true;
            }
        }
    }
}
