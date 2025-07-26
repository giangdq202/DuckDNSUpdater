namespace DuckDNSUpdater;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    // UI Controls
    private TextBox txtSubdomain = null!;
    private TextBox txtToken = null!;
    private NumericUpDown numUpdateInterval = null!;
    private Button btnSave = null!;
    private Button btnUpdateNow = null!;
    private Button btnStartStop = null!;
    private CheckBox chkAutoStart = null!;
    private CheckBox chkMinimizeToTray = null!;
    private Label lblCurrentIP = null!;
    private Label lblLastUpdate = null!;
    private Label lblStatus = null!;
    private RichTextBox txtLog = null!;
    private ProgressBar progressBar = null!;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.SuspendLayout();
        
        // Form properties
        this.Text = "DuckDNS DDNS Updater";
        this.Size = new Size(500, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.Icon = Properties.Resources.ApplicationIcon;
        
        // Configuration Group
        GroupBox grpConfig = new GroupBox();
        grpConfig.Text = "DuckDNS Configuration";
        grpConfig.Location = new Point(10, 10);
        grpConfig.Size = new Size(460, 120);
        
        Label lblSubdomain = new Label();
        lblSubdomain.Text = "Subdomain:";
        lblSubdomain.Location = new Point(10, 25);
        lblSubdomain.Size = new Size(80, 20);
        
        txtSubdomain = new TextBox();
        txtSubdomain.Location = new Point(100, 23);
        txtSubdomain.Size = new Size(200, 20);
        txtSubdomain.PlaceholderText = "your-subdomain";
        
        Label lblToken = new Label();
        lblToken.Text = "Token:";
        lblToken.Location = new Point(10, 55);
        lblToken.Size = new Size(80, 20);
        
        txtToken = new TextBox();
        txtToken.Location = new Point(100, 53);
        txtToken.Size = new Size(340, 20);
        txtToken.UseSystemPasswordChar = true;
        txtToken.PlaceholderText = "your-duckdns-token";
        
        Label lblInterval = new Label();
        lblInterval.Text = "Update every:";
        lblInterval.Location = new Point(10, 85);
        lblInterval.Size = new Size(80, 20);
        
        numUpdateInterval = new NumericUpDown();
        numUpdateInterval.Location = new Point(100, 83);
        numUpdateInterval.Size = new Size(60, 20);
        numUpdateInterval.Minimum = 1;
        numUpdateInterval.Maximum = 1440;
        numUpdateInterval.Value = 5;
        
        Label lblMinutes = new Label();
        lblMinutes.Text = "minutes";
        lblMinutes.Location = new Point(170, 85);
        lblMinutes.Size = new Size(50, 20);
        
        btnSave = new Button();
        btnSave.Text = "Save Config";
        btnSave.Location = new Point(350, 83);
        btnSave.Size = new Size(90, 25);
        btnSave.Click += BtnSave_Click;
        
        grpConfig.Controls.AddRange(new Control[] { 
            lblSubdomain, txtSubdomain, lblToken, txtToken, 
            lblInterval, numUpdateInterval, lblMinutes, btnSave 
        });
        
        // Control Group
        GroupBox grpControl = new GroupBox();
        grpControl.Text = "Controls";
        grpControl.Location = new Point(10, 140);
        grpControl.Size = new Size(460, 80);
        
        btnUpdateNow = new Button();
        btnUpdateNow.Text = "Update Now";
        btnUpdateNow.Location = new Point(10, 25);
        btnUpdateNow.Size = new Size(100, 30);
        btnUpdateNow.Click += BtnUpdateNow_Click;
        
        btnStartStop = new Button();
        btnStartStop.Text = "Start";
        btnStartStop.Location = new Point(120, 25);
        btnStartStop.Size = new Size(100, 30);
        btnStartStop.Click += BtnStartStop_Click;
        
        chkAutoStart = new CheckBox();
        chkAutoStart.Text = "Auto Start";
        chkAutoStart.Location = new Point(230, 25);
        chkAutoStart.Size = new Size(100, 20);
        
        chkMinimizeToTray = new CheckBox();
        chkMinimizeToTray.Text = "Minimize to Tray";
        chkMinimizeToTray.Location = new Point(230, 50);
        chkMinimizeToTray.Size = new Size(150, 20);
        
        grpControl.Controls.AddRange(new Control[] { 
            btnUpdateNow, btnStartStop, chkAutoStart, chkMinimizeToTray 
        });
        
        // Status Group
        GroupBox grpStatus = new GroupBox();
        grpStatus.Text = "Status";
        grpStatus.Location = new Point(10, 230);
        grpStatus.Size = new Size(460, 100);
        
        lblCurrentIP = new Label();
        lblCurrentIP.Text = "Current IP: Not determined";
        lblCurrentIP.Location = new Point(10, 25);
        lblCurrentIP.Size = new Size(250, 20);
        
        lblLastUpdate = new Label();
        lblLastUpdate.Text = "Last Update: None";
        lblLastUpdate.Location = new Point(10, 45);
        lblLastUpdate.Size = new Size(250, 20);
        
        lblStatus = new Label();
        lblStatus.Text = "Status: Stopped";
        lblStatus.Location = new Point(10, 65);
        lblStatus.Size = new Size(200, 20);
        
        progressBar = new ProgressBar();
        progressBar.Location = new Point(270, 25);
        progressBar.Size = new Size(170, 20);
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.Visible = false;
        
        grpStatus.Controls.AddRange(new Control[] { 
            lblCurrentIP, lblLastUpdate, lblStatus, progressBar 
        });
        
        // Log Group
        GroupBox grpLog = new GroupBox();
        grpLog.Text = "Log";
        grpLog.Location = new Point(10, 340);
        grpLog.Size = new Size(460, 200);
        
        txtLog = new RichTextBox();
        txtLog.Location = new Point(10, 20);
        txtLog.Size = new Size(440, 170);
        txtLog.ReadOnly = true;
        txtLog.BackColor = Color.Black;
        txtLog.ForeColor = Color.Lime;
        txtLog.Font = new Font("Consolas", 8);
        
        grpLog.Controls.Add(txtLog);
        
        // Add all groups to form
        this.Controls.AddRange(new Control[] { 
            grpConfig, grpControl, grpStatus, grpLog 
        });
        
        this.ResumeLayout(false);
    }

    #endregion
}
