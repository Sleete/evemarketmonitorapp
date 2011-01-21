using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.Reporting;
using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class CourierSettings : Form
    {
        private ReportGroupSettings _settings;
        private List<string> _recentStations;
        private string _lastDestination = "";

        public CourierSettings()
        {
            InitializeComponent();
            _settings = UserAccount.CurrentGroup.Settings;
        }

        private void CourierSettings_Load(object sender, EventArgs e)
        {
            _recentStations = UserAccount.CurrentGroup.Settings.RecentStations;

            cmbAutoStation.Items.AddRange(_recentStations.ToArray());
            cmbAutoStation.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbAutoStation.AutoCompleteMode = AutoCompleteMode.Suggest;
            cmbAutoStation.KeyDown += new KeyEventHandler(cmbAutoStation_KeyDown);
            cmbAutoStation.SelectedIndexChanged += new EventHandler(cmbAutoStation_SelectedIndexChanged);
            cmbAutoStation.Tag = (long)0;

            List<string> locations = GroupLocations.GetLocationNames();
            if (!locations.Contains("All Regions"))
            {
                locations.Add("All Regions");
            }
            cmbPickupLocation.Items.AddRange(locations.ToArray());
            cmbPickupLocation.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbPickupLocation.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

            InitDisplay();
        }

        
        private void btnOk_Click(object sender, EventArgs e)
        {
            StoreData();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void InitDisplay()
        {
            string collateralBasedOn = _settings.CollateralBasedOn;
            if (collateralBasedOn.Equals("Buy"))
            {
                rdbBuy.Checked = true;
            }
            else if (collateralBasedOn.Equals("Sell"))
            {
                rdbSell.Checked = true;
            }

            string rewardBasedOn = _settings.RewardBasedOn;
            if (rewardBasedOn.Equals("Profit"))
            {
                rdbProfit.Checked = true;
            }
            else if (rewardBasedOn.Equals("Collateral"))
            {
                rdbCollateral.Checked = true;
            }
            else if (rewardBasedOn.Equals("Jumps"))
            {
                rdbJumps.Checked = true;
            }
            else if (rewardBasedOn.Equals("Volume"))
            {
                rdbVolume.Checked = true;
            }
            else if (rewardBasedOn.Equals("VolumeAndJumps"))
            {
                rdbVolumeAndJumps.Checked = true;
            }

            txtCollateralPerc.Tag = new TagData(TagDataType.Percentage, _settings.CollateralPercentage);

            txtMinReward.Tag = new TagData(TagDataType.IskAmount, _settings.MinReward);
            txtMaxReward.Tag = new TagData(TagDataType.IskAmount, _settings.MaxReward);
            txtMinPerc.Tag = new TagData(TagDataType.Percentage, _settings.MinRewardPercentage);
            txtMaxPerc.Tag = new TagData(TagDataType.Percentage, _settings.MaxRewardPercentage);
            txtJumpPerc.Tag = new TagData(TagDataType.Percentage, _settings.RewardPercPerJump);
            txtLowSecPerc.Tag = new TagData(TagDataType.Percentage, _settings.LowSecPickupBonusPerc);
            txtVolumePerc.Tag = new TagData(TagDataType.Percentage, _settings.VolumeBasedRewardPerc);
            txtRewardPerJump.Tag = new TagData(TagDataType.IskAmount, _settings.RewardPerJump);
            txtRewardPerVolume.Tag = new TagData(TagDataType.IskAmount, _settings.RewardPerVolume);
            txtBaseReward.Tag = new TagData(TagDataType.IskAmount, _settings.PickupReward);

            txtAutoMinCollateral.Tag = new TagData(TagDataType.IskAmount, _settings.AutoCon_MinCollateral);
            txtAutoMinReward.Tag = new TagData(TagDataType.IskAmount, _settings.AutoCon_MinReward);
            txtAutoMinVolume.Tag = new TagData(TagDataType.Volume, _settings.AutoCon_MinVolume);
            txtAutoMaxCollateral.Tag = new TagData(TagDataType.IskAmount, _settings.AutoCon_MaxCollateral);
            txtAutoMaxVolume.Tag = new TagData(TagDataType.Volume, _settings.AutoCon_MaxVolume);
            cmbAutoStation.Tag = _settings.AutoCon_DestiantionStation;
            cmbAutoStation.Text = Stations.GetStationName((long)cmbAutoStation.Tag);
            cmbPickupLocation.SelectedItem = _settings.AutoCon_PickupLocations; 
            chkSplitStack.Checked = _settings.AutoCon_AllowStackSplitting;
            chkExcludeContainers.Checked = _settings.AutoCon_ExcludeContainers;
            chkTradedItems.Checked = _settings.AutoCon_TradedItems;

            txtExampleJumps.Tag = new TagData(TagDataType.Numeric, 5);
            txtExamplePurchase.Tag = new TagData(TagDataType.IskAmount, 70000000);
            txtExampleSale.Tag = new TagData(TagDataType.IskAmount, 100000000);
            txtExampleVolume.Tag = new TagData(TagDataType.Volume, 10000);
            chkExampleLowSec.Checked = false;

            DisplayReadableValue(txtCollateralPerc);

            DisplayReadableValue(txtMinReward);
            DisplayReadableValue(txtMaxReward);
            DisplayReadableValue(txtMinPerc);
            DisplayReadableValue(txtMaxPerc);
            DisplayReadableValue(txtJumpPerc);
            DisplayReadableValue(txtLowSecPerc);
            DisplayReadableValue(txtVolumePerc);
            DisplayReadableValue(txtRewardPerJump);
            DisplayReadableValue(txtBaseReward);
            DisplayReadableValue(txtRewardPerVolume);

            DisplayReadableValue(txtAutoMinCollateral);
            DisplayReadableValue(txtAutoMinReward);
            DisplayReadableValue(txtAutoMinVolume);
            DisplayReadableValue(txtAutoMaxCollateral);
            DisplayReadableValue(txtAutoMaxVolume);

            DisplayReadableValue(txtExampleJumps);
            DisplayReadableValue(txtExamplePurchase);
            DisplayReadableValue(txtExampleSale);
            DisplayReadableValue(txtExampleVolume);

            SetFieldsEnabled();
            RecalcExample();
        }

        private void StoreData()
        {
            string collateralBasedOn = "";
            if (rdbBuy.Checked)
            {
                collateralBasedOn = "Buy";
            }
            else if (rdbSell.Checked)
            {
                collateralBasedOn = "Sell";
            }
            _settings.CollateralBasedOn = collateralBasedOn;

            string rewardBasedOn = "";
            if (rdbProfit.Checked)
            {
                rewardBasedOn = "Profit";
            }
            else if (rdbCollateral.Checked)
            {
                rewardBasedOn = "Collateral";
            }
            else if (rdbJumps.Checked)
            {
                rewardBasedOn = "Jumps";
            }
            else if (rdbVolume.Checked)
            {
                rewardBasedOn = "Volume";
            }
            else if (rdbVolumeAndJumps.Checked)
            {
                rewardBasedOn = "VolumeAndJumps";
            }
            _settings.RewardBasedOn = rewardBasedOn;

            _settings.CollateralPercentage = ((TagData)txtCollateralPerc.Tag).Value;
            _settings.MinReward = ((TagData)txtMinReward.Tag).Value;
            _settings.MaxReward = ((TagData)txtMaxReward.Tag).Value;
            _settings.MinRewardPercentage = ((TagData)txtMinPerc.Tag).Value;
            _settings.MaxRewardPercentage = ((TagData)txtMaxPerc.Tag).Value;
            _settings.RewardPercPerJump = ((TagData)txtJumpPerc.Tag).Value;
            _settings.LowSecPickupBonusPerc = ((TagData)txtLowSecPerc.Tag).Value;
            _settings.VolumeBasedRewardPerc = ((TagData)txtVolumePerc.Tag).Value;
            _settings.RewardPerJump = ((TagData)txtRewardPerJump.Tag).Value;
            _settings.PickupReward = ((TagData)txtBaseReward.Tag).Value;
            _settings.RewardPerVolume = ((TagData)txtRewardPerVolume.Tag).Value;

            _settings.AutoCon_MinCollateral = ((TagData)txtAutoMinCollateral.Tag).Value;
            _settings.AutoCon_MinReward = ((TagData)txtAutoMinReward.Tag).Value;
            _settings.AutoCon_MinVolume = ((TagData)txtAutoMinVolume.Tag).Value;
            _settings.AutoCon_MaxCollateral = ((TagData)txtAutoMaxCollateral.Tag).Value;
            _settings.AutoCon_MaxVolume = ((TagData)txtAutoMaxVolume.Tag).Value;
            _settings.AutoCon_DestiantionStation = (long)cmbAutoStation.Tag;
            if (cmbPickupLocation.Text.Equals("All Regions"))
            {
                _settings.AutoCon_PickupLocations = cmbPickupLocation.SelectedItem.ToString();
            }
            else if (GroupLocations.NameExists(cmbPickupLocation.Text))
            {
                _settings.AutoCon_PickupLocations = cmbPickupLocation.SelectedItem.ToString();
            }
            _settings.AutoCon_AllowStackSplitting = chkSplitStack.Checked;
            _settings.AutoCon_ExcludeContainers = chkExcludeContainers.Checked;
            _settings.AutoCon_TradedItems = chkTradedItems.Checked;

            EveMarketMonitorApp.Properties.Settings.Default.Save();
        }

        #region General enter/leave field handlers for displaying help text
        private void field_Enter(object sender, EventArgs e)
        {
            TextBox field = sender as TextBox;
            if (field != null)
            {
                txtField_Enter(field, e);
            }

            ShowFieldHelp(sender);
        }

        private void field_Leave(object sender, EventArgs e)
        {
            TextBox field = sender as TextBox;
            if (field != null)
            {
                txtField_Leave(field, e);
            }
            ClearFieldHelp();
        }
        #endregion

        #region enter/leave field handlers for text boxes.
        private void txtField_Enter(TextBox field, EventArgs e)
        {
            try
            {
                TagData data = (TagData)field.Tag;
                field.Text = data.Value.ToString();
            }
            catch { }
        }

        private void txtField_Leave(TextBox field, EventArgs e)
        {
            try
            {
                TagData data = (TagData)field.Tag;

                try
                {
                    data.Value = decimal.Parse(field.Text, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                }
                catch
                {
                    data.Value = 0;
                }

                DisplayReadableValue(field);
            }
            catch { }
        }
        #endregion

        private void DisplayReadableValue(TextBox field)
        {
            try
            {
                TagData data = (TagData)field.Tag;

                switch (data.Type)
                {
                    case TagDataType.Numeric:
                        field.Text = data.Value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case TagDataType.IskAmount:
                        field.Text = new IskAmount(data.Value).ToString();
                        break;
                    case TagDataType.Percentage:
                        field.Text = data.Value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "%";
                        break;
                    case TagDataType.Volume:
                        field.Text = data.Value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + " m3";
                        break;
                    default:
                        field.Text = "";
                        break;
                }
            }
            catch { }
        }

        private void ClearFieldHelp()
        {
            lblFieldText.Text = "Select any field to display information about it.";
        }

        private void ShowFieldHelp(object field)
        {
            string helpText = "";

            if (field == txtCollateralPerc)
            {
                helpText = "Collateral percentage: This field specifies the percentage of either estimated sale price or" +
                    " purchase price that is use as collateral.\r\nFor example, if collateral is based on estimated sale" +
                    " price and collateral percentage is 110% then collateral will equal estimated sale price + 10% extra.";
            }
            if(field == txtMinReward) 
            {
                helpText = "Minimum reward: This field specifies the minimum ISK amount that will be given as a reward.";
            }
            if (field == txtMaxReward)
            {
                helpText = "Maximum reward: This field specifies the maximum ISK amount that will be given as a reward.";
            }
            if (field == txtMinPerc)
            {
                helpText = "Minimum reward percentage: This field specifies the minimum percentage of either expected" +
                    " profit or colalteral that will be given as a reward";
            }
            if (field == txtMaxPerc)
            {
                helpText = "Maximum reward percentage: This field specifies the maximum percentage of either expected" +
                    " profit or collateral that will be given as a reward.";
            }
            //if (field == chkJumpReward)
            //{
            //    helpText = "If enabled then the reward will be calculated as x% of either expected profit or collateral" +
            //        " per jump on the shortest route from the pickup point to the destination point.";
            //}
            if (field == txtJumpPerc)
            {
                helpText = "Reward % per jump: This field specifies the percentage of either expected profit or" +
                    " collateral that is given as a reward per jump.";
            }
            if (field == txtRewardPerJump)
            {
                if (rdbVolumeAndJumps.Checked)
                {
                    helpText = "Reward per jump per m³: This field specifies the isk amount" +
                        " that is given as a reward for one m³ of cargo moved one jump.";
                }
                else
                {
                    helpText = "Reward per jump: This field specifies the isk amount" +
                        " that is given as a reward per jump.";
                }
            }
            if (field == txtRewardPerVolume)
            {
                helpText = "Reward per m³: This field specifies the isk amount" +
                    " that is given as a reward per m³ of cargo delivered.";
            }
            if (field == txtBaseReward)
            {
                helpText = "Base reward: This field specifies the base isk amount" +
                    " that is given as a reward for the contract, regardless of anything else.\r\n" +
                    "This amount will be added to based upon the other parameters (e.g. volume, etc).";
            }
            //if (field == chkLowSecReward)
            //{
            //    helpText = "If enabled then x% of either expected profit or collateral will be added to the reward amount" +
            //        " if the pickup or destination point is not in high security space.";
            //}
            if (field == txtLowSecPerc)
            {
                helpText = "Reward % for lowsec: The percentage of either expected profit or collateral that will be" +
                    " given as a reward if the pickup or destination point is not in high sec space.";
            }
            if (field == txtVolumePerc)
            {
                helpText = "Reward % per 100m3: The percentage of either expected profit or collateral that will be" +
                    " given as a reward per 100m3 of the contract's volume";
            }
            if (field == txtAutoMinCollateral)
            {
                helpText = "Minimum collateral: Contracts will only be listed by the auto-contracter if the collateral" +
                    "on the contract equals or exceeds the specified value.";
            }
            if (field == txtAutoMinReward)
            {
                helpText = "Minimum reward: Contracts will only be listed by the auto-contracter if the reward" +
                    "on the contract equals or exceeds the specified value.";
            }
            if (field == txtAutoMinVolume)
            {
                helpText = "Minimum volume: Contracts will only be listed by the auto-contracter if the total volume" +
                    "of the items in the contract equals or exceeds the specified value.";
            }
            if (field == txtAutoMaxVolume)
            {
                helpText = "Maximum volume: Contracts created by the auto-contractor will not have a total volume" +
                    " greater than that which is specified.\r\nWhere a station has items that would take the contract over" +
                    " the limit, multiple contracts will be created from that station to the destination.";
            }
            if (field == txtAutoMaxCollateral)
            {
                helpText = "Maximum collateral: Contracts created by the auto-contractor will not have a collateral" +
                    " greater than that which is specified.\r\nWhere a station has items that would take the contract over" +
                    " the limit, multiple contracts will be created from that station to the destination.";
            }
            if (field == cmbAutoStation)
            {
                helpText = "Destination station: This is the station that will used as the destination when using the" +
                    " auto-contractor.";
            }
            if (field == cmbPickupLocation)
            {
                helpText = "Pickup Locations: This is a filter that limits the systems that the auto-contractor" +
                    " will create contracts from. Only those included in the selected location will be used as" +
                    " pickup destinations.";
            }
            if (field == chkSplitStack)
            {
                helpText = "Allow splitting stacks: If enabled, the auto contractor will split stacks of items into" +
                    " smaller quantities if required to meet maximum volume and collateral limits. Note that EMMA" +
                    " will always see items as fully stacked regardless of how they are arranged in-game.";
            }
            if (field == txtExampleJumps || field == txtExamplePurchase || field == txtExampleSale ||
                field == txtExampleVolume || field == chkExampleLowSec)
            {
                helpText = "Enter example values in order to calculate collateral/reward with the" +
                    " currenct settings";
            }
            if (field == chkTradedItems)
            {
                helpText = "Include only traded items: If enabled, the auto contractor will only create contracts " +
                    "for items that are on the list of traded items (configure this list in settings->traded items)";
            }

            lblFieldText.Text = helpText;
        }


        private void btnRecalc_Click(object sender, EventArgs e)
        {
            RecalcExample();
        }

        private void RecalcExample()
        {
            decimal collateral = 0;
            string collateralBasedOn = "";
            if (rdbBuy.Checked)
            {
                collateralBasedOn = "Buy";
            }
            else if (rdbSell.Checked)
            {
                collateralBasedOn = "Sell";
            }

            collateral = AutoContractor.CalcCollateral(collateralBasedOn, ((TagData)txtCollateralPerc.Tag).Value,
                ((TagData)txtExamplePurchase.Tag).Value, ((TagData)txtExampleSale.Tag).Value);
            lblExampleCollateral.Text = new IskAmount(collateral).ToString();

            decimal reward = 0;
            string rewardBasedOn = "";
            if (rdbProfit.Checked)
            {
                rewardBasedOn = "Profit";
            }
            else if (rdbCollateral.Checked)
            {
                rewardBasedOn = "Collateral";
            }
            else if (rdbJumps.Checked)
            {
                rewardBasedOn = "Jumps";
            }
            decimal expectedProfit = ((TagData)txtExampleSale.Tag).Value - ((TagData)txtExamplePurchase.Tag).Value;

            reward = AutoContractor.CalcReward(rewardBasedOn, collateral,
                expectedProfit,
                ((TagData)txtMinReward.Tag).Value, ((TagData)txtMaxReward.Tag).Value,
                ((TagData)txtMinPerc.Tag).Value, ((TagData)txtMaxPerc.Tag).Value,
                ((TagData)txtJumpPerc.Tag).Value, ((TagData)txtVolumePerc.Tag).Value,
                ((TagData)txtLowSecPerc.Tag).Value, (int)((TagData)txtExampleJumps.Tag).Value,
                ((TagData)txtExampleVolume.Tag).Value, chkExampleLowSec.Checked,
                ((TagData)txtRewardPerJump.Tag).Value, ((TagData)txtRewardPerVolume.Tag).Value,
                ((TagData)txtBaseReward.Tag).Value);
            lblExampleReward.Text = new IskAmount(reward).ToString();

            lblExampleProfitPerc.Text = "(" + Math.Round((reward / collateral) * 100, 2) + "% of collateral / " +
                Math.Round((reward / expectedProfit) * 100, 2) + "% of expected profit)";
        }


        void cmbAutoStation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SetSelectedAutoStation();
            }
        }

        void cmbAutoStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSelectedAutoStation();
        }

        private void SetSelectedAutoStation()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (!cmbAutoStation.Text.Equals("") && !cmbAutoStation.Text.Equals(_lastDestination))
                {
                    try
                    {
                        EveDataSet.staStationsRow station = Stations.GetStation(cmbAutoStation.Text);
                        if (station != null)
                        {
                            cmbAutoStation.Tag = (long)station.stationID;
                            string name = station.stationName;
                            cmbAutoStation.Text = name;
                            if (!cmbAutoStation.Items.Contains(name))
                            {
                                cmbAutoStation.Items.Add(name);
                                _recentStations.Add(name);
                            }
                        }
                        else
                        {
                            cmbAutoStation.Tag = (long)0;
                        }
                    }
                    catch (EMMADataException) { }
                    _lastDestination = cmbAutoStation.Text;
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnNewLocation_Click(object sender, EventArgs e)
        {
            GroupLocationMaint newLocation = new GroupLocationMaint();
            if (newLocation.ShowDialog() == DialogResult.OK)
            {
                cmbPickupLocation.Items.Add(newLocation.FilterName);
                cmbPickupLocation.SelectedItem = newLocation.FilterName;
            }
        }

        private void btnModifyLocation_Click(object sender, EventArgs e)
        {
            if (GroupLocations.NameExists(cmbPickupLocation.Text))
            {
                GroupLocationMaint maintLocation = new GroupLocationMaint(cmbPickupLocation.Text);
                maintLocation.ShowDialog();
            }
            else
            {
                MessageBox.Show("The currently entered pickup location name does not exist in the database." +
                    " It must be created.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void rdbRewardBasedOn_CheckedChanged(object sender, EventArgs e)
        {
            SetFieldsEnabled();
        }


        private void SetFieldsEnabled()
        {
            if (rdbJumps.Checked || rdbVolume.Checked || rdbVolumeAndJumps.Checked)
            {
                txtMinPerc.Enabled = false;
                txtMaxPerc.Enabled = false;
                txtJumpPerc.Enabled = false;
                txtLowSecPerc.Enabled = false;
                txtVolumePerc.Enabled = false;
                txtRewardPerJump.Enabled = rdbJumps.Checked || rdbVolumeAndJumps.Checked;
                //txtBaseReward.Enabled = true;
                txtRewardPerVolume.Enabled = rdbVolume.Checked;
                if (rdbVolumeAndJumps.Checked)
                {
                    lblRewardPer.Text = "Reward per jump per m³";
                }
                else if (rdbJumps.Checked)
                {
                    lblRewardPer.Text = "Reward per jump";
                }
            }
            else
            {
                txtMinPerc.Enabled = true;
                txtMaxPerc.Enabled = true;
                txtJumpPerc.Enabled = true;
                txtLowSecPerc.Enabled = true;
                txtVolumePerc.Enabled = true;
                txtRewardPerJump.Enabled = false;
                //txtBaseReward.Enabled = false;
                txtRewardPerVolume.Enabled = false;
            }
        }



        private class TagData
        {
            private TagDataType _type;
            private decimal _value;

            public TagData(TagDataType type, decimal value)
            {
                this._type = type;
                this._value = value;
            }

            public TagDataType Type
            {
                get { return _type; }
                set { _type = value; }
            }

            public decimal Value
            {
                get { return _value; }
                set { _value = value; }
            }
        }

        private enum TagDataType
        {
            Numeric,
            IskAmount,
            Percentage,
            Volume
        }




    }
}