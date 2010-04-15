namespace EveMarketMonitorApp.GUIElements
{
    partial class CourierSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CourierSettings));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtVolumePerc = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtMinPerc = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtLowSecPerc = new System.Windows.Forms.TextBox();
            this.txtJumpPerc = new System.Windows.Forms.TextBox();
            this.lblLowSecPerc = new System.Windows.Forms.Label();
            this.lblJumpPerc = new System.Windows.Forms.Label();
            this.lblMaxPerc = new System.Windows.Forms.Label();
            this.txtMaxPerc = new System.Windows.Forms.TextBox();
            this.rdbProfit = new System.Windows.Forms.RadioButton();
            this.rdbCollateral = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMinReward = new System.Windows.Forms.TextBox();
            this.txtMaxReward = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtCollateralPerc = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.rdbBuy = new System.Windows.Forms.RadioButton();
            this.rdbSell = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnModifyLocation = new System.Windows.Forms.Button();
            this.btnNewLocation = new System.Windows.Forms.Button();
            this.cmbPickupLocation = new System.Windows.Forms.ComboBox();
            this.label20 = new System.Windows.Forms.Label();
            this.chkExcludeContainers = new System.Windows.Forms.CheckBox();
            this.txtAutoMinVolume = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.chkSplitStack = new System.Windows.Forms.CheckBox();
            this.cmbAutoStation = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtAutoMaxVolume = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtAutoMaxCollateral = new System.Windows.Forms.TextBox();
            this.txtAutoMinReward = new System.Windows.Forms.TextBox();
            this.txtAutoMinCollateral = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblFieldText = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.lblExampleProfitPerc = new System.Windows.Forms.Label();
            this.btnRecalc = new System.Windows.Forms.Button();
            this.lblExampleCollateral = new System.Windows.Forms.Label();
            this.lblExampleReward = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.lblExampleResult = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.chkExampleLowSec = new System.Windows.Forms.CheckBox();
            this.txtExampleJumps = new System.Windows.Forms.TextBox();
            this.txtExampleVolume = new System.Windows.Forms.TextBox();
            this.txtExamplePurchase = new System.Windows.Forms.TextBox();
            this.txtExampleSale = new System.Windows.Forms.TextBox();
            this.lblExample = new System.Windows.Forms.Label();
            this.chkTradedItems = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.txtVolumePerc);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.txtMinPerc);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtLowSecPerc);
            this.groupBox1.Controls.Add(this.txtJumpPerc);
            this.groupBox1.Controls.Add(this.lblLowSecPerc);
            this.groupBox1.Controls.Add(this.lblJumpPerc);
            this.groupBox1.Controls.Add(this.lblMaxPerc);
            this.groupBox1.Controls.Add(this.txtMaxPerc);
            this.groupBox1.Controls.Add(this.rdbProfit);
            this.groupBox1.Controls.Add(this.rdbCollateral);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtMinReward);
            this.groupBox1.Controls.Add(this.txtMaxReward);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 183);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(330, 279);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Reward Calculation";
            // 
            // txtVolumePerc
            // 
            this.txtVolumePerc.Location = new System.Drawing.Point(121, 221);
            this.txtVolumePerc.Name = "txtVolumePerc";
            this.txtVolumePerc.Size = new System.Drawing.Size(159, 20);
            this.txtVolumePerc.TabIndex = 7;
            this.txtVolumePerc.Leave += new System.EventHandler(this.field_Leave);
            this.txtVolumePerc.Enter += new System.EventHandler(this.field_Enter);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 224);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(108, 13);
            this.label13.TabIndex = 18;
            this.label13.Text = "Reward % per 100m3";
            // 
            // txtMinPerc
            // 
            this.txtMinPerc.Location = new System.Drawing.Point(121, 117);
            this.txtMinPerc.Name = "txtMinPerc";
            this.txtMinPerc.Size = new System.Drawing.Size(159, 20);
            this.txtMinPerc.TabIndex = 4;
            this.txtMinPerc.Leave += new System.EventHandler(this.field_Leave);
            this.txtMinPerc.Enter += new System.EventHandler(this.field_Enter);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 120);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Min reward %";
            // 
            // txtLowSecPerc
            // 
            this.txtLowSecPerc.Location = new System.Drawing.Point(121, 247);
            this.txtLowSecPerc.Name = "txtLowSecPerc";
            this.txtLowSecPerc.Size = new System.Drawing.Size(159, 20);
            this.txtLowSecPerc.TabIndex = 8;
            this.txtLowSecPerc.Leave += new System.EventHandler(this.field_Leave);
            this.txtLowSecPerc.Enter += new System.EventHandler(this.field_Enter);
            // 
            // txtJumpPerc
            // 
            this.txtJumpPerc.Location = new System.Drawing.Point(121, 195);
            this.txtJumpPerc.Name = "txtJumpPerc";
            this.txtJumpPerc.Size = new System.Drawing.Size(159, 20);
            this.txtJumpPerc.TabIndex = 6;
            this.txtJumpPerc.Leave += new System.EventHandler(this.field_Leave);
            this.txtJumpPerc.Enter += new System.EventHandler(this.field_Enter);
            // 
            // lblLowSecPerc
            // 
            this.lblLowSecPerc.AutoSize = true;
            this.lblLowSecPerc.Location = new System.Drawing.Point(6, 250);
            this.lblLowSecPerc.Name = "lblLowSecPerc";
            this.lblLowSecPerc.Size = new System.Drawing.Size(109, 13);
            this.lblLowSecPerc.TabIndex = 15;
            this.lblLowSecPerc.Text = "Reward % for low sec";
            // 
            // lblJumpPerc
            // 
            this.lblJumpPerc.AutoSize = true;
            this.lblJumpPerc.Location = new System.Drawing.Point(6, 198);
            this.lblJumpPerc.Name = "lblJumpPerc";
            this.lblJumpPerc.Size = new System.Drawing.Size(98, 13);
            this.lblJumpPerc.TabIndex = 14;
            this.lblJumpPerc.Text = "Reward % per jump";
            // 
            // lblMaxPerc
            // 
            this.lblMaxPerc.AutoSize = true;
            this.lblMaxPerc.Location = new System.Drawing.Point(6, 146);
            this.lblMaxPerc.Name = "lblMaxPerc";
            this.lblMaxPerc.Size = new System.Drawing.Size(73, 13);
            this.lblMaxPerc.TabIndex = 10;
            this.lblMaxPerc.Text = "Max reward %";
            // 
            // txtMaxPerc
            // 
            this.txtMaxPerc.Location = new System.Drawing.Point(121, 143);
            this.txtMaxPerc.Name = "txtMaxPerc";
            this.txtMaxPerc.Size = new System.Drawing.Size(159, 20);
            this.txtMaxPerc.TabIndex = 5;
            this.txtMaxPerc.Leave += new System.EventHandler(this.field_Leave);
            this.txtMaxPerc.Enter += new System.EventHandler(this.field_Enter);
            // 
            // rdbProfit
            // 
            this.rdbProfit.AutoSize = true;
            this.rdbProfit.Location = new System.Drawing.Point(121, 19);
            this.rdbProfit.Name = "rdbProfit";
            this.rdbProfit.Size = new System.Drawing.Size(96, 17);
            this.rdbProfit.TabIndex = 0;
            this.rdbProfit.TabStop = true;
            this.rdbProfit.Text = "Expected profit";
            this.rdbProfit.UseVisualStyleBackColor = true;
            this.rdbProfit.Leave += new System.EventHandler(this.field_Leave);
            this.rdbProfit.Enter += new System.EventHandler(this.field_Enter);
            // 
            // rdbCollateral
            // 
            this.rdbCollateral.AutoSize = true;
            this.rdbCollateral.Location = new System.Drawing.Point(121, 42);
            this.rdbCollateral.Name = "rdbCollateral";
            this.rdbCollateral.Size = new System.Drawing.Size(68, 17);
            this.rdbCollateral.TabIndex = 1;
            this.rdbCollateral.TabStop = true;
            this.rdbCollateral.Text = "Collateral";
            this.rdbCollateral.UseVisualStyleBackColor = true;
            this.rdbCollateral.Leave += new System.EventHandler(this.field_Leave);
            this.rdbCollateral.Enter += new System.EventHandler(this.field_Enter);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Minimum reward";
            // 
            // txtMinReward
            // 
            this.txtMinReward.Location = new System.Drawing.Point(121, 65);
            this.txtMinReward.Name = "txtMinReward";
            this.txtMinReward.Size = new System.Drawing.Size(159, 20);
            this.txtMinReward.TabIndex = 2;
            this.txtMinReward.Leave += new System.EventHandler(this.field_Leave);
            this.txtMinReward.Enter += new System.EventHandler(this.field_Enter);
            // 
            // txtMaxReward
            // 
            this.txtMaxReward.Location = new System.Drawing.Point(121, 91);
            this.txtMaxReward.Name = "txtMaxReward";
            this.txtMaxReward.Size = new System.Drawing.Size(159, 20);
            this.txtMaxReward.TabIndex = 3;
            this.txtMaxReward.Leave += new System.EventHandler(this.field_Leave);
            this.txtMaxReward.Enter += new System.EventHandler(this.field_Enter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Maximum reward";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Reward based on:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtCollateralPerc);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.rdbBuy);
            this.groupBox2.Controls.Add(this.rdbSell);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(12, 85);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(330, 92);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Collateral Calculation";
            // 
            // txtCollateralPerc
            // 
            this.txtCollateralPerc.Location = new System.Drawing.Point(115, 66);
            this.txtCollateralPerc.Name = "txtCollateralPerc";
            this.txtCollateralPerc.Size = new System.Drawing.Size(156, 20);
            this.txtCollateralPerc.TabIndex = 2;
            this.txtCollateralPerc.Leave += new System.EventHandler(this.field_Leave);
            this.txtCollateralPerc.Enter += new System.EventHandler(this.field_Enter);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 68);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Collateral %";
            // 
            // rdbBuy
            // 
            this.rdbBuy.AutoSize = true;
            this.rdbBuy.Location = new System.Drawing.Point(121, 42);
            this.rdbBuy.Name = "rdbBuy";
            this.rdbBuy.Size = new System.Drawing.Size(96, 17);
            this.rdbBuy.TabIndex = 1;
            this.rdbBuy.TabStop = true;
            this.rdbBuy.Text = "Purchase price";
            this.rdbBuy.UseVisualStyleBackColor = true;
            this.rdbBuy.Leave += new System.EventHandler(this.field_Leave);
            this.rdbBuy.Enter += new System.EventHandler(this.field_Enter);
            // 
            // rdbSell
            // 
            this.rdbSell.AutoSize = true;
            this.rdbSell.Location = new System.Drawing.Point(121, 19);
            this.rdbSell.Name = "rdbSell";
            this.rdbSell.Size = new System.Drawing.Size(119, 17);
            this.rdbSell.TabIndex = 0;
            this.rdbSell.TabStop = true;
            this.rdbSell.Text = "Estimated sale price";
            this.rdbSell.UseVisualStyleBackColor = true;
            this.rdbSell.Leave += new System.EventHandler(this.field_Leave);
            this.rdbSell.Enter += new System.EventHandler(this.field_Enter);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Collateral based on:";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(597, 595);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(86, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(505, 595);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(86, 30);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.chkTradedItems);
            this.groupBox3.Controls.Add(this.btnModifyLocation);
            this.groupBox3.Controls.Add(this.btnNewLocation);
            this.groupBox3.Controls.Add(this.cmbPickupLocation);
            this.groupBox3.Controls.Add(this.label20);
            this.groupBox3.Controls.Add(this.chkExcludeContainers);
            this.groupBox3.Controls.Add(this.txtAutoMinVolume);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.chkSplitStack);
            this.groupBox3.Controls.Add(this.cmbAutoStation);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.txtAutoMaxVolume);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.txtAutoMaxCollateral);
            this.groupBox3.Controls.Add(this.txtAutoMinReward);
            this.groupBox3.Controls.Add(this.txtAutoMinCollateral);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Location = new System.Drawing.Point(348, 85);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(330, 377);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Auto-Contractor Settings";
            // 
            // btnModifyLocation
            // 
            this.btnModifyLocation.Location = new System.Drawing.Point(217, 166);
            this.btnModifyLocation.Name = "btnModifyLocation";
            this.btnModifyLocation.Size = new System.Drawing.Size(88, 29);
            this.btnModifyLocation.TabIndex = 22;
            this.btnModifyLocation.Text = "Modify";
            this.btnModifyLocation.UseVisualStyleBackColor = true;
            this.btnModifyLocation.Click += new System.EventHandler(this.btnModifyLocation_Click);
            // 
            // btnNewLocation
            // 
            this.btnNewLocation.Location = new System.Drawing.Point(123, 166);
            this.btnNewLocation.Name = "btnNewLocation";
            this.btnNewLocation.Size = new System.Drawing.Size(88, 29);
            this.btnNewLocation.TabIndex = 21;
            this.btnNewLocation.Text = "Create New";
            this.btnNewLocation.UseVisualStyleBackColor = true;
            this.btnNewLocation.Click += new System.EventHandler(this.btnNewLocation_Click);
            // 
            // cmbPickupLocation
            // 
            this.cmbPickupLocation.FormattingEnabled = true;
            this.cmbPickupLocation.Location = new System.Drawing.Point(10, 201);
            this.cmbPickupLocation.Name = "cmbPickupLocation";
            this.cmbPickupLocation.Size = new System.Drawing.Size(295, 21);
            this.cmbPickupLocation.TabIndex = 20;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(6, 166);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(92, 13);
            this.label20.TabIndex = 23;
            this.label20.Text = "Pickup Locations:";
            // 
            // chkExcludeContainers
            // 
            this.chkExcludeContainers.AutoSize = true;
            this.chkExcludeContainers.Location = new System.Drawing.Point(37, 291);
            this.chkExcludeContainers.Name = "chkExcludeContainers";
            this.chkExcludeContainers.Size = new System.Drawing.Size(190, 17);
            this.chkExcludeContainers.TabIndex = 18;
            this.chkExcludeContainers.Text = "Exclude fitted ships and containers";
            this.chkExcludeContainers.UseVisualStyleBackColor = true;
            // 
            // txtAutoMinVolume
            // 
            this.txtAutoMinVolume.Location = new System.Drawing.Point(115, 71);
            this.txtAutoMinVolume.Name = "txtAutoMinVolume";
            this.txtAutoMinVolume.Size = new System.Drawing.Size(190, 20);
            this.txtAutoMinVolume.TabIndex = 2;
            this.txtAutoMinVolume.Leave += new System.EventHandler(this.field_Leave);
            this.txtAutoMinVolume.Enter += new System.EventHandler(this.field_Enter);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 74);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(61, 13);
            this.label12.TabIndex = 17;
            this.label12.Text = "Min volume";
            // 
            // chkSplitStack
            // 
            this.chkSplitStack.AutoSize = true;
            this.chkSplitStack.Location = new System.Drawing.Point(37, 268);
            this.chkSplitStack.Name = "chkSplitStack";
            this.chkSplitStack.Size = new System.Drawing.Size(123, 17);
            this.chkSplitStack.TabIndex = 6;
            this.chkSplitStack.Text = "Allow splitting stacks";
            this.chkSplitStack.UseVisualStyleBackColor = true;
            this.chkSplitStack.Leave += new System.EventHandler(this.field_Leave);
            this.chkSplitStack.Enter += new System.EventHandler(this.field_Enter);
            // 
            // cmbAutoStation
            // 
            this.cmbAutoStation.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cmbAutoStation.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbAutoStation.FormattingEnabled = true;
            this.cmbAutoStation.Location = new System.Drawing.Point(10, 241);
            this.cmbAutoStation.Name = "cmbAutoStation";
            this.cmbAutoStation.Size = new System.Drawing.Size(295, 21);
            this.cmbAutoStation.TabIndex = 5;
            this.cmbAutoStation.Leave += new System.EventHandler(this.field_Leave);
            this.cmbAutoStation.Enter += new System.EventHandler(this.field_Enter);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 225);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(94, 13);
            this.label10.TabIndex = 10;
            this.label10.Text = "Destination station";
            // 
            // txtAutoMaxVolume
            // 
            this.txtAutoMaxVolume.Location = new System.Drawing.Point(115, 138);
            this.txtAutoMaxVolume.Name = "txtAutoMaxVolume";
            this.txtAutoMaxVolume.Size = new System.Drawing.Size(190, 20);
            this.txtAutoMaxVolume.TabIndex = 4;
            this.txtAutoMaxVolume.Leave += new System.EventHandler(this.field_Leave);
            this.txtAutoMaxVolume.Enter += new System.EventHandler(this.field_Enter);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 141);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(64, 13);
            this.label11.TabIndex = 8;
            this.label11.Text = "Max volume";
            // 
            // txtAutoMaxCollateral
            // 
            this.txtAutoMaxCollateral.Location = new System.Drawing.Point(115, 112);
            this.txtAutoMaxCollateral.Name = "txtAutoMaxCollateral";
            this.txtAutoMaxCollateral.Size = new System.Drawing.Size(190, 20);
            this.txtAutoMaxCollateral.TabIndex = 3;
            this.txtAutoMaxCollateral.Leave += new System.EventHandler(this.field_Leave);
            this.txtAutoMaxCollateral.Enter += new System.EventHandler(this.field_Enter);
            // 
            // txtAutoMinReward
            // 
            this.txtAutoMinReward.Location = new System.Drawing.Point(115, 45);
            this.txtAutoMinReward.Name = "txtAutoMinReward";
            this.txtAutoMinReward.Size = new System.Drawing.Size(190, 20);
            this.txtAutoMinReward.TabIndex = 1;
            this.txtAutoMinReward.Leave += new System.EventHandler(this.field_Leave);
            this.txtAutoMinReward.Enter += new System.EventHandler(this.field_Enter);
            // 
            // txtAutoMinCollateral
            // 
            this.txtAutoMinCollateral.Location = new System.Drawing.Point(115, 19);
            this.txtAutoMinCollateral.Name = "txtAutoMinCollateral";
            this.txtAutoMinCollateral.Size = new System.Drawing.Size(190, 20);
            this.txtAutoMinCollateral.TabIndex = 0;
            this.txtAutoMinCollateral.Leave += new System.EventHandler(this.field_Leave);
            this.txtAutoMinCollateral.Enter += new System.EventHandler(this.field_Enter);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 48);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(59, 13);
            this.label9.TabIndex = 2;
            this.label9.Text = "Min reward";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 115);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(72, 13);
            this.label8.TabIndex = 1;
            this.label8.Text = "Max collateral";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(69, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Min collateral";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.lblFieldText);
            this.groupBox4.Location = new System.Drawing.Point(12, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(666, 67);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Field Information";
            // 
            // lblFieldText
            // 
            this.lblFieldText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFieldText.Location = new System.Drawing.Point(3, 16);
            this.lblFieldText.Name = "lblFieldText";
            this.lblFieldText.Size = new System.Drawing.Size(660, 48);
            this.lblFieldText.TabIndex = 0;
            this.lblFieldText.Text = "Select any field to display information about it.";
            this.lblFieldText.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.lblExampleProfitPerc);
            this.groupBox5.Controls.Add(this.btnRecalc);
            this.groupBox5.Controls.Add(this.lblExampleCollateral);
            this.groupBox5.Controls.Add(this.lblExampleReward);
            this.groupBox5.Controls.Add(this.label18);
            this.groupBox5.Controls.Add(this.lblExampleResult);
            this.groupBox5.Controls.Add(this.label17);
            this.groupBox5.Controls.Add(this.label16);
            this.groupBox5.Controls.Add(this.label15);
            this.groupBox5.Controls.Add(this.label14);
            this.groupBox5.Controls.Add(this.chkExampleLowSec);
            this.groupBox5.Controls.Add(this.txtExampleJumps);
            this.groupBox5.Controls.Add(this.txtExampleVolume);
            this.groupBox5.Controls.Add(this.txtExamplePurchase);
            this.groupBox5.Controls.Add(this.txtExampleSale);
            this.groupBox5.Controls.Add(this.lblExample);
            this.groupBox5.Location = new System.Drawing.Point(12, 468);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(666, 120);
            this.groupBox5.TabIndex = 6;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Examples";
            // 
            // lblExampleProfitPerc
            // 
            this.lblExampleProfitPerc.AutoSize = true;
            this.lblExampleProfitPerc.Location = new System.Drawing.Point(283, 102);
            this.lblExampleProfitPerc.Name = "lblExampleProfitPerc";
            this.lblExampleProfitPerc.Size = new System.Drawing.Size(118, 13);
            this.lblExampleProfitPerc.TabIndex = 16;
            this.lblExampleProfitPerc.Text = "(50% of expected profit)";
            // 
            // btnRecalc
            // 
            this.btnRecalc.Location = new System.Drawing.Point(573, 87);
            this.btnRecalc.Name = "btnRecalc";
            this.btnRecalc.Size = new System.Drawing.Size(87, 27);
            this.btnRecalc.TabIndex = 15;
            this.btnRecalc.Text = "Recalculate";
            this.btnRecalc.UseVisualStyleBackColor = true;
            this.btnRecalc.Click += new System.EventHandler(this.btnRecalc_Click);
            // 
            // lblExampleCollateral
            // 
            this.lblExampleCollateral.Location = new System.Drawing.Point(78, 89);
            this.lblExampleCollateral.Name = "lblExampleCollateral";
            this.lblExampleCollateral.Size = new System.Drawing.Size(179, 13);
            this.lblExampleCollateral.TabIndex = 14;
            this.lblExampleCollateral.Text = "10,000,000.00 ISK";
            this.lblExampleCollateral.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblExampleReward
            // 
            this.lblExampleReward.Location = new System.Drawing.Point(75, 102);
            this.lblExampleReward.Name = "lblExampleReward";
            this.lblExampleReward.Size = new System.Drawing.Size(182, 15);
            this.lblExampleReward.TabIndex = 13;
            this.lblExampleReward.Text = "10,000,000.00 ISK";
            this.lblExampleReward.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(6, 102);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(44, 13);
            this.label18.TabIndex = 12;
            this.label18.Text = "Reward";
            // 
            // lblExampleResult
            // 
            this.lblExampleResult.AutoSize = true;
            this.lblExampleResult.Location = new System.Drawing.Point(6, 89);
            this.lblExampleResult.Name = "lblExampleResult";
            this.lblExampleResult.Size = new System.Drawing.Size(50, 13);
            this.lblExampleResult.TabIndex = 11;
            this.lblExampleResult.Text = "Collateral";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(343, 64);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(42, 13);
            this.label17.TabIndex = 10;
            this.label17.Text = "Volume";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(343, 38);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(37, 13);
            this.label16.TabIndex = 9;
            this.label16.Text = "Jumps";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 64);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(155, 13);
            this.label15.TabIndex = 8;
            this.label15.Text = "Sell price of all items in contract";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 38);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(183, 13);
            this.label14.TabIndex = 7;
            this.label14.Text = "Purchase price of all items in contract";
            // 
            // chkExampleLowSec
            // 
            this.chkExampleLowSec.AutoSize = true;
            this.chkExampleLowSec.Location = new System.Drawing.Point(519, 15);
            this.chkExampleLowSec.Name = "chkExampleLowSec";
            this.chkExampleLowSec.Size = new System.Drawing.Size(101, 17);
            this.chkExampleLowSec.TabIndex = 0;
            this.chkExampleLowSec.Text = "Low sec pickup";
            this.chkExampleLowSec.UseVisualStyleBackColor = true;
            this.chkExampleLowSec.Leave += new System.EventHandler(this.field_Leave);
            this.chkExampleLowSec.Enter += new System.EventHandler(this.field_Enter);
            // 
            // txtExampleJumps
            // 
            this.txtExampleJumps.Location = new System.Drawing.Point(457, 35);
            this.txtExampleJumps.Name = "txtExampleJumps";
            this.txtExampleJumps.Size = new System.Drawing.Size(159, 20);
            this.txtExampleJumps.TabIndex = 2;
            this.txtExampleJumps.Leave += new System.EventHandler(this.field_Leave);
            this.txtExampleJumps.Enter += new System.EventHandler(this.field_Enter);
            // 
            // txtExampleVolume
            // 
            this.txtExampleVolume.Location = new System.Drawing.Point(457, 61);
            this.txtExampleVolume.Name = "txtExampleVolume";
            this.txtExampleVolume.Size = new System.Drawing.Size(159, 20);
            this.txtExampleVolume.TabIndex = 4;
            this.txtExampleVolume.Leave += new System.EventHandler(this.field_Leave);
            this.txtExampleVolume.Enter += new System.EventHandler(this.field_Enter);
            // 
            // txtExamplePurchase
            // 
            this.txtExamplePurchase.Location = new System.Drawing.Point(195, 35);
            this.txtExamplePurchase.Name = "txtExamplePurchase";
            this.txtExamplePurchase.Size = new System.Drawing.Size(129, 20);
            this.txtExamplePurchase.TabIndex = 1;
            this.txtExamplePurchase.Leave += new System.EventHandler(this.field_Leave);
            this.txtExamplePurchase.Enter += new System.EventHandler(this.field_Enter);
            // 
            // txtExampleSale
            // 
            this.txtExampleSale.Location = new System.Drawing.Point(195, 61);
            this.txtExampleSale.Name = "txtExampleSale";
            this.txtExampleSale.Size = new System.Drawing.Size(129, 20);
            this.txtExampleSale.TabIndex = 3;
            this.txtExampleSale.Leave += new System.EventHandler(this.field_Leave);
            this.txtExampleSale.Enter += new System.EventHandler(this.field_Enter);
            // 
            // lblExample
            // 
            this.lblExample.Location = new System.Drawing.Point(6, 16);
            this.lblExample.Name = "lblExample";
            this.lblExample.Size = new System.Drawing.Size(507, 16);
            this.lblExample.TabIndex = 0;
            this.lblExample.Text = "Use the fields below to see what collateral and reward values would be given for " +
                "different contracts.";
            // 
            // chkTradedItems
            // 
            this.chkTradedItems.AutoSize = true;
            this.chkTradedItems.Location = new System.Drawing.Point(37, 314);
            this.chkTradedItems.Name = "chkTradedItems";
            this.chkTradedItems.Size = new System.Drawing.Size(268, 17);
            this.chkTradedItems.TabIndex = 24;
            this.chkTradedItems.Text = "Only include items if they are on the traded items list";
            this.chkTradedItems.UseVisualStyleBackColor = true;
            this.chkTradedItems.Leave += new System.EventHandler(this.field_Leave);
            this.chkTradedItems.Enter += new System.EventHandler(this.field_Enter);
            // 
            // CourierSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(695, 637);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CourierSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Courier Settings";
            this.Load += new System.EventHandler(this.CourierSettings_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMinReward;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMaxReward;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton rdbProfit;
        private System.Windows.Forms.RadioButton rdbCollateral;
        private System.Windows.Forms.Label lblMaxPerc;
        private System.Windows.Forms.TextBox txtMaxPerc;
        private System.Windows.Forms.TextBox txtLowSecPerc;
        private System.Windows.Forms.TextBox txtJumpPerc;
        private System.Windows.Forms.Label lblLowSecPerc;
        private System.Windows.Forms.Label lblJumpPerc;
        private System.Windows.Forms.TextBox txtCollateralPerc;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton rdbBuy;
        private System.Windows.Forms.RadioButton rdbSell;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtMinPerc;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtAutoMaxCollateral;
        private System.Windows.Forms.TextBox txtAutoMinReward;
        private System.Windows.Forms.TextBox txtAutoMinCollateral;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtAutoMaxVolume;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cmbAutoStation;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lblFieldText;
        private System.Windows.Forms.CheckBox chkSplitStack;
        private System.Windows.Forms.TextBox txtAutoMinVolume;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtVolumePerc;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label lblExample;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckBox chkExampleLowSec;
        private System.Windows.Forms.TextBox txtExampleJumps;
        private System.Windows.Forms.TextBox txtExampleVolume;
        private System.Windows.Forms.TextBox txtExamplePurchase;
        private System.Windows.Forms.TextBox txtExampleSale;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label lblExampleCollateral;
        private System.Windows.Forms.Label lblExampleReward;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label lblExampleResult;
        private System.Windows.Forms.Button btnRecalc;
        private System.Windows.Forms.Label lblExampleProfitPerc;
        private System.Windows.Forms.CheckBox chkExcludeContainers;
        private System.Windows.Forms.Button btnModifyLocation;
        private System.Windows.Forms.Button btnNewLocation;
        private System.Windows.Forms.ComboBox cmbPickupLocation;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.CheckBox chkTradedItems;
    }
}