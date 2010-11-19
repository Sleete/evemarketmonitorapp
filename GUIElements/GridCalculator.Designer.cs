namespace EveMarketMonitorApp.GUIElements
{
    partial class GridCalculator
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
            this.components = new System.ComponentModel.Container();
            this.lblNumValues = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lblMedian = new System.Windows.Forms.Label();
            this.lblAverage = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblSum = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.boundGrids = new System.Windows.Forms.CheckedListBox();
            this.chkQuantityAsFrequency = new System.Windows.Forms.CheckBox();
            this.calculationTimer = new System.Windows.Forms.Timer(this.components);
            this.lblMaximum = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblMinimum = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblStdDev = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblValFreqHelp = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblNumValues
            // 
            this.lblNumValues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNumValues.Location = new System.Drawing.Point(116, 9);
            this.lblNumValues.Name = "lblNumValues";
            this.lblNumValues.Size = new System.Drawing.Size(153, 13);
            this.lblNumValues.TabIndex = 7;
            this.lblNumValues.Text = "0";
            this.lblNumValues.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 9);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "Total values";
            // 
            // lblMedian
            // 
            this.lblMedian.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMedian.Location = new System.Drawing.Point(116, 48);
            this.lblMedian.Name = "lblMedian";
            this.lblMedian.Size = new System.Drawing.Size(153, 13);
            this.lblMedian.TabIndex = 5;
            this.lblMedian.Text = "0";
            this.lblMedian.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblAverage
            // 
            this.lblAverage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAverage.Location = new System.Drawing.Point(116, 35);
            this.lblAverage.Name = "lblAverage";
            this.lblAverage.Size = new System.Drawing.Size(153, 13);
            this.lblAverage.TabIndex = 4;
            this.lblAverage.Text = "0";
            this.lblAverage.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 48);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "Median";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 35);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Mean/Average";
            // 
            // lblSum
            // 
            this.lblSum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSum.Location = new System.Drawing.Point(116, 22);
            this.lblSum.Name = "lblSum";
            this.lblSum.Size = new System.Drawing.Size(153, 13);
            this.lblSum.TabIndex = 1;
            this.lblSum.Text = "0";
            this.lblSum.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(28, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Sum";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 110);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Using data from";
            // 
            // boundGrids
            // 
            this.boundGrids.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.boundGrids.FormattingEnabled = true;
            this.boundGrids.Location = new System.Drawing.Point(12, 126);
            this.boundGrids.Name = "boundGrids";
            this.boundGrids.Size = new System.Drawing.Size(257, 79);
            this.boundGrids.TabIndex = 9;
            this.boundGrids.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.boundGrids_ItemCheck);
            // 
            // chkQuantityAsFrequency
            // 
            this.chkQuantityAsFrequency.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkQuantityAsFrequency.AutoSize = true;
            this.chkQuantityAsFrequency.Location = new System.Drawing.Point(12, 211);
            this.chkQuantityAsFrequency.Name = "chkQuantityAsFrequency";
            this.chkQuantityAsFrequency.Size = new System.Drawing.Size(238, 17);
            this.chkQuantityAsFrequency.TabIndex = 10;
            this.chkQuantityAsFrequency.Text = "Get value frequency from quantity if available";
            this.chkQuantityAsFrequency.UseVisualStyleBackColor = true;
            this.chkQuantityAsFrequency.CheckedChanged += new System.EventHandler(this.chkQuantityAsFrequency_CheckedChanged);
            // 
            // calculationTimer
            // 
            this.calculationTimer.Interval = 500;
            this.calculationTimer.Tick += new System.EventHandler(this.calculationTimer_Tick);
            // 
            // lblMaximum
            // 
            this.lblMaximum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMaximum.Location = new System.Drawing.Point(116, 61);
            this.lblMaximum.Name = "lblMaximum";
            this.lblMaximum.Size = new System.Drawing.Size(153, 13);
            this.lblMaximum.TabIndex = 12;
            this.lblMaximum.Text = "0";
            this.lblMaximum.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Maximum";
            // 
            // lblMinimum
            // 
            this.lblMinimum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMinimum.Location = new System.Drawing.Point(116, 74);
            this.lblMinimum.Name = "lblMinimum";
            this.lblMinimum.Size = new System.Drawing.Size(153, 13);
            this.lblMinimum.TabIndex = 14;
            this.lblMinimum.Text = "0";
            this.lblMinimum.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 74);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Minimum";
            // 
            // lblStdDev
            // 
            this.lblStdDev.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStdDev.Location = new System.Drawing.Point(116, 87);
            this.lblStdDev.Name = "lblStdDev";
            this.lblStdDev.Size = new System.Drawing.Size(153, 13);
            this.lblStdDev.TabIndex = 16;
            this.lblStdDev.Text = "0";
            this.lblStdDev.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 87);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(98, 13);
            this.label10.TabIndex = 15;
            this.label10.Text = "Standard Deviation";
            // 
            // lblValFreqHelp
            // 
            this.lblValFreqHelp.AutoSize = true;
            this.lblValFreqHelp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.lblValFreqHelp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblValFreqHelp.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblValFreqHelp.Location = new System.Drawing.Point(256, 212);
            this.lblValFreqHelp.Name = "lblValFreqHelp";
            this.lblValFreqHelp.Size = new System.Drawing.Size(15, 15);
            this.lblValFreqHelp.TabIndex = 17;
            this.lblValFreqHelp.Text = "?";
            this.lblValFreqHelp.Click += new System.EventHandler(this.lblValFreqHelp_Click);
            // 
            // GridCalculator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(281, 240);
            this.Controls.Add(this.lblValFreqHelp);
            this.Controls.Add(this.lblStdDev);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.lblMinimum);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblMaximum);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkQuantityAsFrequency);
            this.Controls.Add(this.boundGrids);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblNumValues);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.lblMedian);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblAverage);
            this.Controls.Add(this.lblSum);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(289, 0);
            this.Name = "GridCalculator";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Grid Calculator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblNumValues;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblMedian;
        private System.Windows.Forms.Label lblAverage;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblSum;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox boundGrids;
        private System.Windows.Forms.CheckBox chkQuantityAsFrequency;
        private System.Windows.Forms.Timer calculationTimer;
        private System.Windows.Forms.Label lblMaximum;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblMinimum;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblStdDev;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lblValFreqHelp;
    }
}