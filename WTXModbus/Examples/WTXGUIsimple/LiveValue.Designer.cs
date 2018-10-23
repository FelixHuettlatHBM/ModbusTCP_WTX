namespace WTXGUIsimple
{
    partial class LiveValue
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LiveValue));
            this.txtIPAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdConnect = new System.Windows.Forms.Button();
            this.grrpSetup = new System.Windows.Forms.GroupBox();
            this.rbtConnectionModbus = new System.Windows.Forms.RadioButton();
            this.rbtConnectionJet = new System.Windows.Forms.RadioButton();
            this.picConnectionType = new System.Windows.Forms.PictureBox();
            this.cmdGrossNet = new System.Windows.Forms.Button();
            this.cmdTare = new System.Windows.Forms.Button();
            this.cmdZero = new System.Windows.Forms.Button();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.picNE107 = new System.Windows.Forms.PictureBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.calibrationWithWeightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calibrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.grrpSetup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picConnectionType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNE107)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtIPAddress
            // 
            this.txtIPAddress.Location = new System.Drawing.Point(111, 28);
            this.txtIPAddress.Name = "txtIPAddress";
            this.txtIPAddress.Size = new System.Drawing.Size(100, 20);
            this.txtIPAddress.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP address";
            // 
            // cmdConnect
            // 
            this.cmdConnect.Location = new System.Drawing.Point(255, 28);
            this.cmdConnect.Name = "cmdConnect";
            this.cmdConnect.Size = new System.Drawing.Size(61, 36);
            this.cmdConnect.TabIndex = 2;
            this.cmdConnect.Text = "Connect";
            this.cmdConnect.UseVisualStyleBackColor = true;
            this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
            // 
            // grrpSetup
            // 
            this.grrpSetup.Controls.Add(this.rbtConnectionModbus);
            this.grrpSetup.Controls.Add(this.rbtConnectionJet);
            this.grrpSetup.Controls.Add(this.txtIPAddress);
            this.grrpSetup.Controls.Add(this.picConnectionType);
            this.grrpSetup.Controls.Add(this.label1);
            this.grrpSetup.Controls.Add(this.cmdConnect);
            this.grrpSetup.Location = new System.Drawing.Point(10, 31);
            this.grrpSetup.Name = "grrpSetup";
            this.grrpSetup.Size = new System.Drawing.Size(376, 81);
            this.grrpSetup.TabIndex = 13;
            this.grrpSetup.TabStop = false;
            this.grrpSetup.Text = "Connection";
            // 
            // rbtConnectionModbus
            // 
            this.rbtConnectionModbus.AutoSize = true;
            this.rbtConnectionModbus.Location = new System.Drawing.Point(147, 54);
            this.rbtConnectionModbus.Name = "rbtConnectionModbus";
            this.rbtConnectionModbus.Size = new System.Drawing.Size(89, 17);
            this.rbtConnectionModbus.TabIndex = 4;
            this.rbtConnectionModbus.TabStop = true;
            this.rbtConnectionModbus.Text = "Modbus/TCP";
            this.rbtConnectionModbus.UseVisualStyleBackColor = true;
            this.rbtConnectionModbus.CheckedChanged += new System.EventHandler(this.rbtConnectionModbus_CheckedChanged);
            // 
            // rbtConnectionJet
            // 
            this.rbtConnectionJet.AutoSize = true;
            this.rbtConnectionJet.Checked = true;
            this.rbtConnectionJet.Location = new System.Drawing.Point(102, 54);
            this.rbtConnectionJet.Name = "rbtConnectionJet";
            this.rbtConnectionJet.Size = new System.Drawing.Size(39, 17);
            this.rbtConnectionJet.TabIndex = 3;
            this.rbtConnectionJet.TabStop = true;
            this.rbtConnectionJet.Text = "Jet";
            this.rbtConnectionJet.UseVisualStyleBackColor = true;
            // 
            // picConnectionType
            // 
            this.picConnectionType.Location = new System.Drawing.Point(322, 24);
            this.picConnectionType.Name = "picConnectionType";
            this.picConnectionType.Size = new System.Drawing.Size(42, 47);
            this.picConnectionType.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picConnectionType.TabIndex = 25;
            this.picConnectionType.TabStop = false;
            this.picConnectionType.Click += new System.EventHandler(this.picConnectionType_Click);
            // 
            // cmdGrossNet
            // 
            this.cmdGrossNet.Location = new System.Drawing.Point(172, 141);
            this.cmdGrossNet.Name = "cmdGrossNet";
            this.cmdGrossNet.Size = new System.Drawing.Size(75, 23);
            this.cmdGrossNet.TabIndex = 24;
            this.cmdGrossNet.Text = "Gross/Net";
            this.cmdGrossNet.UseVisualStyleBackColor = true;
            this.cmdGrossNet.Click += new System.EventHandler(this.cmdGrossNet_Click);
            // 
            // cmdTare
            // 
            this.cmdTare.Location = new System.Drawing.Point(10, 141);
            this.cmdTare.Name = "cmdTare";
            this.cmdTare.Size = new System.Drawing.Size(75, 23);
            this.cmdTare.TabIndex = 22;
            this.cmdTare.Text = "Tare";
            this.cmdTare.UseVisualStyleBackColor = true;
            this.cmdTare.Click += new System.EventHandler(this.cmdTare_Click);
            // 
            // cmdZero
            // 
            this.cmdZero.Location = new System.Drawing.Point(91, 141);
            this.cmdZero.Name = "cmdZero";
            this.cmdZero.Size = new System.Drawing.Size(75, 23);
            this.cmdZero.TabIndex = 23;
            this.cmdZero.Text = "Zero";
            this.cmdZero.UseVisualStyleBackColor = true;
            this.cmdZero.Click += new System.EventHandler(this.cmdZero_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInfo.Location = new System.Drawing.Point(10, 196);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.Size = new System.Drawing.Size(364, 105);
            this.txtInfo.TabIndex = 26;
            // 
            // picNE107
            // 
            this.picNE107.Image = global::WTXGUIsimple.Properties.Resources.NE107_OutOfSpecification;
            this.picNE107.Location = new System.Drawing.Point(290, 118);
            this.picNE107.Name = "picNE107";
            this.picNE107.Size = new System.Drawing.Size(84, 72);
            this.picNE107.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picNE107.TabIndex = 27;
            this.picNE107.TabStop = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(390, 25);
            this.toolStrip1.TabIndex = 29;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.calibrationWithWeightToolStripMenuItem,
            this.calibrationToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(49, 22);
            this.toolStripDropDownButton1.Text = "Tools";
            // 
            // calibrationWithWeightToolStripMenuItem
            // 
            this.calibrationWithWeightToolStripMenuItem.Name = "calibrationWithWeightToolStripMenuItem";
            this.calibrationWithWeightToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.calibrationWithWeightToolStripMenuItem.Text = "Calculate calibration";
            this.calibrationWithWeightToolStripMenuItem.Click += new System.EventHandler(this.calibrationWithWeightToolStripMenuItem_Click_1);
            // 
            // calibrationToolStripMenuItem
            // 
            this.calibrationToolStripMenuItem.Name = "calibrationToolStripMenuItem";
            this.calibrationToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.calibrationToolStripMenuItem.Text = "Calibration with weight";
            this.calibrationToolStripMenuItem.Click += new System.EventHandler(this.calibrationToolStripMenuItem_Click_1);
            // 
            // LiveValue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 313);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.picNE107);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.cmdGrossNet);
            this.Controls.Add(this.cmdTare);
            this.Controls.Add(this.cmdZero);
            this.Controls.Add(this.grrpSetup);
            this.Name = "LiveValue";
            this.Text = "LiveValue";
            this.Load += new System.EventHandler(this.LiveValue_Load);
            this.grrpSetup.ResumeLayout(false);
            this.grrpSetup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picConnectionType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picNE107)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtIPAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdConnect;
        private System.Windows.Forms.GroupBox grrpSetup;
        private System.Windows.Forms.PictureBox picConnectionType;
        private System.Windows.Forms.Button cmdGrossNet;
        private System.Windows.Forms.Button cmdTare;
        private System.Windows.Forms.Button cmdZero;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.PictureBox picNE107;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem calibrationWithWeightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calibrationToolStripMenuItem;
        private System.Windows.Forms.RadioButton rbtConnectionModbus;
        private System.Windows.Forms.RadioButton rbtConnectionJet;
    }
}

