namespace ArduinoIDE.net
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ContinueButton = new System.Windows.Forms.Button();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.BreakpointDetailsLabel = new System.Windows.Forms.Label();
            this.SerialOutputTextbox = new System.Windows.Forms.TextBox();
            this.DisassemblyTextbox = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel2 = new System.Windows.Forms.Panel();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.panel3 = new System.Windows.Forms.Panel();
            this.WatchesListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitter3 = new System.Windows.Forms.Splitter();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // ContinueButton
            // 
            this.ContinueButton.Enabled = false;
            this.ContinueButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.ContinueButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ContinueButton.Location = new System.Drawing.Point(5, 4);
            this.ContinueButton.Name = "ContinueButton";
            this.ContinueButton.Size = new System.Drawing.Size(70, 56);
            this.ContinueButton.TabIndex = 0;
            this.ContinueButton.UseVisualStyleBackColor = true;
            this.ContinueButton.Click += new System.EventHandler(this.ContinueButton_Click);
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(85, 46);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(0, 13);
            this.StatusLabel.TabIndex = 1;
            // 
            // BreakpointDetailsLabel
            // 
            this.BreakpointDetailsLabel.AutoSize = true;
            this.BreakpointDetailsLabel.Location = new System.Drawing.Point(84, 9);
            this.BreakpointDetailsLabel.Name = "BreakpointDetailsLabel";
            this.BreakpointDetailsLabel.Size = new System.Drawing.Size(0, 13);
            this.BreakpointDetailsLabel.TabIndex = 2;
            // 
            // SerialOutputTextbox
            // 
            this.SerialOutputTextbox.BackColor = System.Drawing.Color.White;
            this.SerialOutputTextbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SerialOutputTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SerialOutputTextbox.Location = new System.Drawing.Point(5, 0);
            this.SerialOutputTextbox.Multiline = true;
            this.SerialOutputTextbox.Name = "SerialOutputTextbox";
            this.SerialOutputTextbox.ReadOnly = true;
            this.SerialOutputTextbox.Size = new System.Drawing.Size(1018, 123);
            this.SerialOutputTextbox.TabIndex = 3;
            // 
            // DisassemblyTextbox
            // 
            this.DisassemblyTextbox.BackColor = System.Drawing.Color.White;
            this.DisassemblyTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DisassemblyTextbox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DisassemblyTextbox.Location = new System.Drawing.Point(5, 0);
            this.DisassemblyTextbox.MaxLength = 0;
            this.DisassemblyTextbox.Multiline = true;
            this.DisassemblyTextbox.Name = "DisassemblyTextbox";
            this.DisassemblyTextbox.ReadOnly = true;
            this.DisassemblyTextbox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.DisassemblyTextbox.Size = new System.Drawing.Size(703, 507);
            this.DisassemblyTextbox.TabIndex = 4;
            this.DisassemblyTextbox.WordWrap = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ContinueButton);
            this.panel1.Controls.Add(this.StatusLabel);
            this.panel1.Controls.Add(this.BreakpointDetailsLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5, 5, 0, 0);
            this.panel1.Size = new System.Drawing.Size(1028, 65);
            this.panel1.TabIndex = 5;
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 65);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1028, 3);
            this.splitter1.TabIndex = 6;
            this.splitter1.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.SerialOutputTextbox);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 578);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(5, 0, 5, 5);
            this.panel2.Size = new System.Drawing.Size(1028, 128);
            this.panel2.TabIndex = 7;
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter2.Location = new System.Drawing.Point(0, 575);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(1028, 3);
            this.splitter2.TabIndex = 8;
            this.splitter2.TabStop = false;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.WatchesListView);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(711, 68);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(317, 507);
            this.panel3.TabIndex = 9;
            // 
            // WatchesListView
            // 
            this.WatchesListView.CheckBoxes = true;
            this.WatchesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.WatchesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WatchesListView.FullRowSelect = true;
            this.WatchesListView.Location = new System.Drawing.Point(0, 0);
            this.WatchesListView.Name = "WatchesListView";
            this.WatchesListView.Size = new System.Drawing.Size(317, 507);
            this.WatchesListView.TabIndex = 0;
            this.WatchesListView.UseCompatibleStateImageBehavior = false;
            this.WatchesListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 158;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Address";
            this.columnHeader2.Width = 74;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Value";
            // 
            // splitter3
            // 
            this.splitter3.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter3.Location = new System.Drawing.Point(708, 68);
            this.splitter3.Name = "splitter3";
            this.splitter3.Size = new System.Drawing.Size(3, 507);
            this.splitter3.TabIndex = 10;
            this.splitter3.TabStop = false;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.DisassemblyTextbox);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 68);
            this.panel4.Name = "panel4";
            this.panel4.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.panel4.Size = new System.Drawing.Size(708, 507);
            this.panel4.TabIndex = 11;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1028, 706);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.splitter3);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Arduino soft debugger";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ContinueButton;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Label BreakpointDetailsLabel;
        private System.Windows.Forms.TextBox SerialOutputTextbox;
        private System.Windows.Forms.TextBox DisassemblyTextbox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Splitter splitter3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.ListView WatchesListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}

