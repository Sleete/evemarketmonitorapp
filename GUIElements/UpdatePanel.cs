using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.GUIElements.Interfaces;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class UpdatePanel : UserControl, IUpdatePanel
    {
        private APICharacter _character;
        private CharOrCorp _type;

        private Color _upToDateColour = Color.LightGreen;
        private Color _errorColour = Color.Red;
        private Color _overdueUpdateColour = Color.Orange;
        private Color _updatingColour = Color.Yellow;

        private static bool _updating = false;
        private static bool _chkClicked = false;

        private Dictionary<APIDataType, bool> _showingTT = new Dictionary<APIDataType, bool>();
        private Dictionary<APIDataType, DateTime> _lastUpdateAttempt = new Dictionary<APIDataType, DateTime>();

        public event APIUpdateEvent UpdateEvent;

        public bool _toggleAll = false;
        public bool _individualUpdate = false;

        private static string BLOCKEDTEXT = "Blocked, Retrying..";
        private static string WAITINGTEXT = "Waiting..";

        public UpdatePanel()
        {
            InitializeComponent();
        }

        public UpdatePanel(CharOrCorp type, APICharacter character)
        {
            InitializeComponent();
            _type = type;
            _character = character;

            lblJournal.BackColor = _upToDateColour;
            lblJournalStatus.BackColor = _upToDateColour;
            lblTransactions.BackColor = _upToDateColour;
            lblTransStatus.BackColor = _upToDateColour;
            lblOrders.BackColor = _upToDateColour;
            lblOrdersStatus.BackColor = _upToDateColour;
            lblAssets.BackColor = _upToDateColour;
            lblAssetsStatus.BackColor = _upToDateColour;
            lblIndustryJobs.BackColor = _upToDateColour;
            lblIndustryJobsStatus.BackColor = _upToDateColour;

            _showingTT.Add(APIDataType.Assets, false);
            _showingTT.Add(APIDataType.Journal, false);
            _showingTT.Add(APIDataType.Orders, false);
            _showingTT.Add(APIDataType.Transactions, false);
            _lastUpdateAttempt.Add(APIDataType.Assets, DateTime.MinValue);
            _lastUpdateAttempt.Add(APIDataType.Journal, DateTime.MinValue);
            _lastUpdateAttempt.Add(APIDataType.Orders, DateTime.MinValue);
            _lastUpdateAttempt.Add(APIDataType.Transactions, DateTime.MinValue);

            _individualUpdate = UserAccount.Settings.APIIndividualUpdate;

            RefreshCheckboxDisplay();

            if (type == CharOrCorp.Char)
            {
                picPortrait.Image = Portaits.GetPortrait(character.CharID);
                lblCorpTag.Visible = false;
            }
            else
            {
                picPortrait.Image = null;
                picPortrait.BorderStyle = BorderStyle.FixedSingle;
                lblCorpTag.Visible = true;
                lblCorpTag.Text = "[" + character.CorpTag + "]";
            
                // Get any other characters in the group with the same corp.
                List<APICharacter> otherCorpChars = new List<APICharacter>();
                foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                {
                    foreach (APICharacter tmpchar in account.Chars)
                    {
                        if (tmpchar.CharID != _character.CharID && tmpchar.CorpID == _character.CorpID)
                        {
                            otherCorpChars.Add(tmpchar);
                        }
                    }
                }
                otherCorpChars.Add(_character);
                _character.OtherCorpChars = otherCorpChars;
            }

            _toggleAll = !chkAutoTrans.Checked && !chkAutoOrders.Checked && !chkAutoJournal.Checked && !chkAutoAssets.Checked;

            chkUpdate.Enabled = !Globals.EveAPIDown;
            chkAutoAssets.Enabled = !Globals.EveAPIDown;
            chkAutoJournal.Enabled = !Globals.EveAPIDown;
            chkAutoOrders.Enabled = !Globals.EveAPIDown;
            chkAutoTrans.Enabled = !Globals.EveAPIDown;

            chkAutoAssets.Tag = APIDataType.Assets;
            chkAutoIndustryJobs.Tag = APIDataType.IndustryJobs;
            chkAutoJournal.Tag = APIDataType.Journal;
            chkAutoOrders.Tag = APIDataType.Orders;
            chkAutoTrans.Tag = APIDataType.Transactions;
            chkAutoAssets.CheckedChanged += new EventHandler(chk_CheckedChanged);
            chkAutoJournal.CheckedChanged += new EventHandler(chk_CheckedChanged);
            chkAutoOrders.CheckedChanged += new EventHandler(chk_CheckedChanged);
            chkAutoTrans.CheckedChanged += new EventHandler(chk_CheckedChanged);
            chkAutoIndustryJobs.CheckedChanged += new EventHandler(chk_CheckedChanged);

            lblAssets.Tag = new LabelMetaData(APIDataType.Assets);
            lblAssetsStatus.Tag = new LabelMetaData(APIDataType.Assets);
            lblJournal.Tag = new LabelMetaData(APIDataType.Journal);
            lblJournalStatus.Tag = new LabelMetaData(APIDataType.Journal);
            lblOrders.Tag = new LabelMetaData(APIDataType.Orders);
            lblOrdersStatus.Tag = new LabelMetaData(APIDataType.Orders);
            lblTransactions.Tag = new LabelMetaData(APIDataType.Transactions);
            lblTransStatus.Tag =new LabelMetaData( APIDataType.Transactions);
            lblIndustryJobs.Tag = new LabelMetaData(APIDataType.IndustryJobs);
            lblIndustryJobsStatus.Tag = new LabelMetaData(APIDataType.IndustryJobs);

            lblAssets.MouseHover += new EventHandler(Label_MouseHover);
            lblAssetsStatus.MouseHover += new EventHandler(Label_MouseHover);
            lblJournal.MouseHover += new EventHandler(Label_MouseHover);
            lblJournalStatus.MouseHover += new EventHandler(Label_MouseHover);
            lblOrders.MouseHover += new EventHandler(Label_MouseHover);
            lblOrdersStatus.MouseHover += new EventHandler(Label_MouseHover);
            lblTransactions.MouseHover += new EventHandler(Label_MouseHover);
            lblTransStatus.MouseHover += new EventHandler(Label_MouseHover);
            lblIndustryJobs.MouseHover += new EventHandler(Label_MouseHover);
            lblIndustryJobsStatus.MouseHover += new EventHandler(Label_MouseHover);

            lblAssets.MouseLeave += new EventHandler(Label_MouseLeave);
            lblAssetsStatus.MouseLeave += new EventHandler(Label_MouseLeave);
            lblJournal.MouseLeave += new EventHandler(Label_MouseLeave);
            lblJournalStatus.MouseLeave += new EventHandler(Label_MouseLeave);
            lblOrders.MouseLeave += new EventHandler(Label_MouseLeave);
            lblOrdersStatus.MouseLeave += new EventHandler(Label_MouseLeave);
            lblTransactions.MouseLeave += new EventHandler(Label_MouseLeave);
            lblTransStatus.MouseLeave += new EventHandler(Label_MouseLeave);
            lblIndustryJobs.MouseLeave += new EventHandler(Label_MouseLeave);
            lblIndustryJobsStatus.MouseLeave += new EventHandler(Label_MouseLeave);

            // Removed this because it causes the update to run before the creating
            // proceedure has had a chance to attach it's event listeners..
            //UpdateData();
        }

        private void SetOverallUpdateState()
        {
            if (chkAutoTrans.Checked && chkAutoOrders.Checked &&
                chkAutoJournal.Checked && chkAutoAssets.Checked && chkAutoIndustryJobs.Checked)
            {
                chkUpdate.CheckState = CheckState.Checked;
            }
            else if (!chkAutoTrans.Checked && !chkAutoOrders.Checked &&
                !chkAutoJournal.Checked && !chkAutoAssets.Checked && !chkAutoIndustryJobs.Checked)
            {
                chkUpdate.CheckState = CheckState.Unchecked;
            }
            else
            {
                chkUpdate.CheckState = CheckState.Indeterminate;
            }
        }

        void Label_MouseLeave(object sender, EventArgs e)
        {
            Label lbl = sender as Label;
            LabelMetaData metaData = (LabelMetaData)lbl.Tag;
            APIDataType type = metaData.UpdateType;
            _showingTT[type] = false;
        }

        void Label_MouseHover(object sender, EventArgs e)
        {
            Label lbl = sender as Label;
            LabelMetaData metaData = (LabelMetaData)lbl.Tag;
            APIDataType type = metaData.UpdateType;

            if (!_showingTT[type])
            {
                string tipText = _character.GetLastAPIUpdateError(_type, type);
                /*if (metaData.UpdateType == APIDataType.Orders && _type == CharOrCorp.Corp)
                {
                    tipText = "";
                    List<APICharacter> chars = _character.OtherCorpChars;
                    foreach (APICharacter character in chars)
                    {
                        if(tipText.Length != 0) { tipText = tipText + "\r\n"; }
                        string detail = character.GetLastAPIUpdateError(_type, type);
                        detail = detail.Replace("\r\n", " ");
                        if (detail.Trim().Length == 0) { detail = "Success"; }
                        tipText = tipText + character.CharName + ": " + detail;
                    }
                }*/
                if (tipText.Equals("BLOCKED"))
                {
                    int minutes = UserAccount.Settings.AssetsUpdateMaxMinutes;
                    tipText = "This update is currently blocked because transaction and order updates " +
                        "have not occured within the last " + minutes + " minutes.\r\n" +
                        "To adjust this setting, goto Settings -> API Update Settings.";
                }
                else if (tipText.Equals("AWAITING ACKNOWLEDGEMENT"))
                {
                    tipText = "This update has completed but is currently waiting for other asset " +
                        "updates to complete in order to compare lost/gained items.";
                }

                errorToolTip.Show(tipText, this.Parent,
                    new Point(MousePosition.X - Parent.PointToScreen(Parent.Location).X + 10,
                    MousePosition.Y - Parent.PointToScreen(Parent.Location).Y), 6000);
                _showingTT[type] = true;
            }
        }

        /// <summary>
        /// Update the data diaplyed in the control.
        /// Also kick off API updates if required.
        /// </summary>
        public void UpdateData()
        {
            if (!_updating)
            {
                _updating = true;

                UpdateLabel(lblTransStatus, lblTransactions, _type, APIDataType.Transactions, 
                    UserAccount.Settings.APITransUpdatePeriod);
                UpdateLabel(lblJournalStatus, lblJournal, _type, APIDataType.Journal, 
                    UserAccount.Settings.APIJournUpdatePeriod);
                UpdateLabel(lblOrdersStatus, lblOrders, _type, APIDataType.Orders, 
                    UserAccount.Settings.APIOrderUpdatePeriod);
                UpdateLabel(lblAssetsStatus, lblAssets, _type, APIDataType.Assets,
                    UserAccount.Settings.APIAssetUpdatePeriod);
                UpdateLabel(lblIndustryJobsStatus, lblIndustryJobs, _type, APIDataType.IndustryJobs,
                    UserAccount.Settings.APIIndustryJobsUpdatePeriod);

                _updating = false;
            }

            if (UserAccount.Settings.APIIndividualUpdate != _individualUpdate)
            {
                RefreshCheckboxDisplay();
            }
        }

        private void RefreshCheckboxDisplay()
        {
            if (UserAccount.Settings.APIIndividualUpdate)
            {
                _individualUpdate = true;
                chkAutoAssets.Visible = true;
                chkAutoJournal.Visible = true;
                chkAutoOrders.Visible = true;
                chkAutoTrans.Visible = true;
                chkAutoIndustryJobs.Visible = true;
                lblAssetsStatus.Width -= chkAutoAssets.Width;
                lblJournalStatus.Width -= chkAutoAssets.Width;
                lblOrdersStatus.Width -= chkAutoAssets.Width;
                lblTransStatus.Width -= chkAutoAssets.Width;
                lblIndustryJobsStatus.Width -= chkAutoIndustryJobs.Width;
                chkAutoAssets.Checked = !Globals.EveAPIDown && _character.GetAPIAutoUpdate(_type, APIDataType.Assets);
                chkAutoJournal.Checked = !Globals.EveAPIDown && _character.GetAPIAutoUpdate(_type, APIDataType.Journal);
                chkAutoOrders.Checked = !Globals.EveAPIDown && _character.GetAPIAutoUpdate(_type, APIDataType.Orders);
                chkAutoTrans.Checked = !Globals.EveAPIDown && _character.GetAPIAutoUpdate(_type, APIDataType.Transactions);
                chkAutoIndustryJobs.Checked = !Globals.EveAPIDown && _character.GetAPIAutoUpdate(_type, APIDataType.IndustryJobs);
                SetOverallUpdateState();
            }
            else
            {
                if (_individualUpdate)
                {
                    // Only resize status labels if the individual API update setting has just been changed.
                    lblAssetsStatus.Width += chkAutoAssets.Width;
                    lblJournalStatus.Width += chkAutoAssets.Width;
                    lblOrdersStatus.Width += chkAutoAssets.Width;
                    lblTransStatus.Width += chkAutoAssets.Width;
                    lblIndustryJobsStatus.Width += chkAutoIndustryJobs.Width;
                }
                _individualUpdate = false;
                chkAutoAssets.Visible = false;
                chkAutoJournal.Visible = false;
                chkAutoOrders.Visible = false;
                chkAutoTrans.Visible = false;
                chkAutoIndustryJobs.Visible = false;
                chkUpdate.Checked = !Globals.EveAPIDown &&
                    _character.GetAPIAutoUpdate(_type, APIDataType.Assets) &&
                    _character.GetAPIAutoUpdate(_type, APIDataType.Journal) &&
                    _character.GetAPIAutoUpdate(_type, APIDataType.Transactions) &&
                    _character.GetAPIAutoUpdate(_type, APIDataType.Orders) &&
                    _character.GetAPIAutoUpdate(_type, APIDataType.IndustryJobs);
            }
        }

        private void UpdateLabel(Label label, Label otherLabel, CharOrCorp corc, APIDataType dataType, TimeSpan minTimeBetweenUpdates)
        {
            DateTime lastDataUpdate = _character.GetLastAPIUpdateTime(corc, dataType);
            TimeSpan time = DateTime.UtcNow.Subtract(lastDataUpdate);
            string errorText = _character.GetLastAPIUpdateError(corc, dataType);
            bool doUpdate = false;
            bool checkForAccess = false;

            // No need for this.
            // The 'update completed' event is fired by the APICharacter object and handled by the
            // UpdateStatus window anyway.
            //if (label.Text.ToUpper().Equals("UPDATING") && !errorText.ToUpper().Equals("UPDATING")
            //    && !errorText.ToUpper().Equals("BLOCKED") && !errorText.ToUpper().Equals("AWAITING ACKNOWLEDGEMENT"))
            //{
            //    // If the label currently says 'updating' but the error text no longer says 'updating' (or blocked)
            //    // then fire the update completed event.
            //    if (UpdateEvent != null)
            //    {
            //        UpdateEvent(this, new APIUpdateEventArgs(dataType, corc ==
            //            CharOrCorp.Char ? _character.CharID : _character.CorpID,
            //            APIUpdateEventType.UpdateCompleted));
            //    }
            //}

            if (errorText.Equals("") || (_type == CharOrCorp.Corp && (
                errorText.ToUpper().Contains("CHARACTER MUST BE A") ||
                errorText.ToUpper().Contains("CHARACTER MUST HAVE"))))
            {
                if (_type == CharOrCorp.Corp && !_character.CharHasCorporateAccess(dataType))
                {
                    label.Text = "No Access";
                    label.BackColor = _errorColour;
                    otherLabel.BackColor = _errorColour;
                    if (chkUpdate.Checked) { checkForAccess = true; }
                    //switch (dataType)
                    //{
                    //    case APIDataType.Transactions:
                    //        if (chkAutoTrans.Checked) { checkForAccess = true; chkAutoTrans.Checked = false; }
                    //        break;
                    //    case APIDataType.Journal:
                    //        if (chkAutoJournal.Checked) { checkForAccess = true; chkAutoJournal.Checked = false; }
                    //        break;
                    //    case APIDataType.Assets:
                    //        if (chkAutoAssets.Checked) { checkForAccess = true; chkAutoAssets.Checked = false; }
                    //        break;
                    //    case APIDataType.Orders:
                    //        if (chkAutoOrders.Checked) { checkForAccess = true; chkAutoOrders.Checked = false; }
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
                else if (minTimeBetweenUpdates.CompareTo(time) > 0)
                {
                    time = minTimeBetweenUpdates.Subtract(time);
                    // Waiting for next update window
                    label.Text = time.Hours.ToString().PadLeft(2, '0') + ":" +
                        time.Minutes.ToString().PadLeft(2, '0') + ":" +
                        time.Seconds.ToString().PadLeft(2, '0');
                    label.BackColor = _upToDateColour;
                    otherLabel.BackColor = _upToDateColour;
                }
                else
                {
                    // Update overdue
                    label.Text = "Overdue";
                    label.BackColor = _overdueUpdateColour;
                    otherLabel.BackColor = _overdueUpdateColour;
                    doUpdate = true;
                }
            }
            else if (errorText.ToUpper().Equals("UPDATING"))
            {
                if (label.Text.Equals(BLOCKEDTEXT))
                {
                    // If the update was previously blocked then need to let the rest of EMMA 
                    // know that the update is now restarted.
                    if (UpdateEvent != null)
                    {
                        UpdateEvent(this, new APIUpdateEventArgs(dataType, corc ==
                            CharOrCorp.Char ? _character.CharID : _character.CorpID,
                            APIUpdateEventType.UpdateStarted));
                    }
                }
                // The update is in progress.
                label.Text = "Updating";
                label.BackColor = _updatingColour;
                otherLabel.BackColor = _updatingColour;
            }
            else if (errorText.ToUpper().Equals("QUEUED"))
            {
                // The thread performing the update has been started but is currently waiting 
                // for some other update to complete before it can proceed.
                // No transaction, orders or assets update can be running at the same time for a particular 
                // character or corp.
                // No journal update can be running at the same time for ANY character or corp.
                label.Text = "Queued";
                label.BackColor = _updatingColour;
                otherLabel.BackColor = _updatingColour;
            }
            else if (errorText.ToUpper().Equals("BLOCKED"))
            {
                // An assets update has been blocked because the most recent transaction and orders 
                // updates are not within the timeframe specified.
                // Ask the user if they want to reconfigure this to always allow asset updates. 
                if (!label.Text.Equals(BLOCKEDTEXT))
                {
                    label.Text = BLOCKEDTEXT;
                    label.BackColor = _updatingColour;
                    
                    otherLabel.BackColor = _updatingColour;
                    // Make sure we let the rest of EMMA know that the update has stopped.
                    // Otherwise, the user will be unable to use reports, exit, etc while
                    // waiting for it to unblock.
                    if (UpdateEvent != null)
                    {
                        UpdateEvent(this, new APIUpdateEventArgs(dataType, corc ==
                            CharOrCorp.Char ? _character.CharID : _character.CorpID,
                            APIUpdateEventType.UpdateCompleted));
                    }
                    DialogResult result = MessageBox.Show("An assets update for " + (corc == CharOrCorp.Char ?
                        _character.CharName : _character.CorpName) + " has been blocked because transaction " +
                        " & orders updates have not occured within the last " +
                        UserAccount.Settings.AssetsUpdateMaxMinutes + " minutes.\r\n" +
                        "The assets update is only allowed to run when the number of minutes since transactions & " +
                        "orders updates is less than a configured number. This setting can be changed in " +
                        "Settings -> API Update Settings.\r\n" +
                        "Do you wish to set this to zero now? (i.e. always allow assets updates regardless of " +
                        "the last time a transaction/orders update occured)", "Question",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        UserAccount.Settings.AssetsUpdateMaxMinutes = 0;
                    }
                }
            }
            else if (errorText.ToUpper().Equals("AWAITING ACKNOWLEDGEMENT"))
            {
                if (!label.Text.Equals(WAITINGTEXT))
                {
                    label.Text = WAITINGTEXT;
                    label.BackColor = _updatingColour;
                    otherLabel.BackColor = _updatingColour;
                }
            }
            else
            {
                // The last update caused an error of some sort.

                if (minTimeBetweenUpdates.CompareTo(time) > 0)
                {
                    time = minTimeBetweenUpdates.Subtract(time);
                    // Waiting for next update window
                    label.Text = "Error " + time.Hours.ToString().PadLeft(2, '0') + ":" +
                        time.Minutes.ToString().PadLeft(2, '0') + ":" +
                        time.Seconds.ToString().PadLeft(2, '0');
                    label.BackColor = _errorColour;
                    LabelMetaData metaData = (LabelMetaData)otherLabel.Tag;
                    metaData.TimerType = APIUpdateTimerType.Normal;
                    otherLabel.BackColor = _errorColour;
                }
                else
                {
                    LabelMetaData metaData = (LabelMetaData)otherLabel.Tag;
                    if (metaData.TimerType == APIUpdateTimerType.Normal)
                    {
                        // Update overdue
                        label.Text = "Overdue";
                        label.BackColor = _overdueUpdateColour;
                        otherLabel.BackColor = _overdueUpdateColour;
                        doUpdate = true;
                    }
                    else
                    {
                        // If we didn't even get as far as setting the update time when
                        // requesting data from the API last time then just use a one hour
                        // timer to make sure we don't request updates every few seconds
                        // after an error occurs.
                        metaData.TimerType = APIUpdateTimerType.Error;
                        DateTime lastAttempt = _lastUpdateAttempt[dataType];
                        TimeSpan timeSinceLastAttempt = DateTime.UtcNow.Subtract(lastAttempt);
                        if (timeSinceLastAttempt.TotalMinutes > 60)
                        {
                            // Update overdue
                            label.Text = "Overdue";
                            label.BackColor = _overdueUpdateColour;
                            otherLabel.BackColor = _overdueUpdateColour;
                            doUpdate = true;
                        }
                        else
                        {
                            // Error on the last update
                            time = new TimeSpan(0, 61, 0);
                            time = time.Subtract(timeSinceLastAttempt);
                            label.Text = "Error " + time.Hours.ToString().PadLeft(2, '0') + ":" +
                                    time.Minutes.ToString().PadLeft(2, '0') + ":" +
                                    time.Seconds.ToString().PadLeft(2, '0'); ;
                            label.BackColor = _errorColour;
                            otherLabel.BackColor = _errorColour;
                        }
                    }
                }
            }


            if (checkForAccess || (doUpdate && _character.GetAPIAutoUpdate(corc, dataType)))
            {
                // If we're updating assets and order or transaction updates are pending then do those first.
                if (dataType == APIDataType.Assets &&
                    ((_character.GetAPIAutoUpdate(corc, APIDataType.Orders) &&
                    (lblOrdersStatus.Text.Equals("Overdue") || lblOrdersStatus.Text.Equals("Queued"))) ||
                    (_character.GetAPIAutoUpdate(corc, APIDataType.Transactions) &&
                    (lblTransStatus.Text.Equals("Overdue") || lblTransStatus.Text.Equals("Queued")))))
                {
                }
                else
                {
                    // If we're auto updating then kick it off.
                    LabelMetaData metaData = (LabelMetaData)otherLabel.Tag;
                    metaData.TimerType = APIUpdateTimerType.Normal;
                    if (UpdateEvent != null)
                    {
                        UpdateEvent(this, new APIUpdateEventArgs(dataType, corc ==
                            CharOrCorp.Char ? _character.CharID : _character.CorpID,
                            APIUpdateEventType.UpdateStarted));
                    }
                    if (_lastUpdateAttempt.ContainsKey(dataType))
                    {
                        _lastUpdateAttempt.Remove(dataType);
                    }
                    _lastUpdateAttempt.Add(dataType, DateTime.UtcNow);
                    _character.DownloadXMLFromAPI(corc, dataType);
                    //if (corc == CharOrCorp.Corp && dataType == APIDataType.Orders)
                    //{
                    //    // If we're dealing with corporate orders then we need to grab corporate orders for 
                    //    // all characters in this report group that are part of the corp.
                    //    // This is because orders will only be returned that were actually created by
                    //    // the character we are retrieving data for.
                    //    foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                    //    {
                    //        foreach (APICharacter character in account.Chars)
                    //        {
                    //            if (character.CorpID == _character.CorpID && character.CharID != _character.CharID)
                    //            {
                    //                if (character.CharHasCorporateAccess(APIDataType.Orders))
                    //                {
                    //                    character.UpdateDataFromAPI(corc, dataType);
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }
            }

            // Make sure the state of the auto update checkboxes reflects the true values.
            // This only needs to be done for corps because the only way the auto-update
            // setting can change without user intervention is if a char does not have corp
            // data access.
            if (corc == CharOrCorp.Corp)
            {
                switch (dataType)
                {
                    case APIDataType.Transactions:
                        chkAutoTrans.Checked = _character.GetAPIAutoUpdate(corc, dataType);
                        break;
                    case APIDataType.Journal:
                        chkAutoJournal.Checked = _character.GetAPIAutoUpdate(corc, dataType);
                        break;
                    case APIDataType.Assets:
                        chkAutoAssets.Checked = _character.GetAPIAutoUpdate(corc, dataType);
                        break;
                    case APIDataType.Orders:
                        chkAutoOrders.Checked = _character.GetAPIAutoUpdate(corc, dataType);
                        break;
                    case APIDataType.IndustryJobs:
                        chkAutoIndustryJobs.Checked = _character.GetAPIAutoUpdate(corc, dataType);
                        break;
                    default:
                        break;
                }
                SetOverallUpdateState();
            }
        }

        
        //private void chkAutoTrans_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (!_chkClicked)
        //    {
        //        _chkClicked = true;
        //        _character.SetAPIAutoUpdate(_type, APIDataType.Transactions, chkAutoTrans.Checked);
        //        SetOverallUpdateState();
        //        _chkClicked = false;
        //    }
        //}

        //private void chkAutoJournal_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (!_chkClicked)
        //    {
        //        _chkClicked = true;
        //        _character.SetAPIAutoUpdate(_type, APIDataType.Journal, chkAutoJournal.Checked);
        //        SetOverallUpdateState();
        //        _chkClicked = false;
        //    }
        //}

        //private void chkAutoOrders_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (!_chkClicked)
        //    {
        //        _chkClicked = true;
        //        _character.SetAPIAutoUpdate(_type, APIDataType.Orders, chkAutoOrders.Checked);
        //        SetOverallUpdateState();
        //        _chkClicked = false;
        //    }
        //}

        //private void chkAutoAssets_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (!_chkClicked)
        //    {
        //        _chkClicked = true;
        //        _character.SetAPIAutoUpdate(_type, APIDataType.Assets, chkAutoAssets.Checked);
        //        SetOverallUpdateState();
        //        _chkClicked = false;
        //    }
        //}
        void chk_CheckedChanged(object sender, EventArgs e)
        {
            if (!_chkClicked)
            {
                _chkClicked = true;
                CheckBox checkbox = sender as CheckBox;
                APIDataType APItype = (APIDataType)checkbox.Tag;
                _character.SetAPIAutoUpdate(_type, APItype, checkbox.Checked);
                SetOverallUpdateState();
                _chkClicked = false;
            }
        }

        private void picPortrait_Click(object sender, EventArgs e)
        {
            _chkClicked = true;
            chkAutoAssets.Checked = _toggleAll;
            chkAutoJournal.Checked = _toggleAll;
            chkAutoOrders.Checked = _toggleAll;
            chkAutoTrans.Checked = _toggleAll;
            chkAutoIndustryJobs.Checked = _toggleAll;
            SetOverallUpdateState();
            _toggleAll = !_toggleAll;
            _chkClicked = false;
        }

        private void lblCorpTag_Click(object sender, EventArgs e)
        {
            _chkClicked = true;
            chkAutoAssets.Checked = _toggleAll;
            chkAutoJournal.Checked = _toggleAll;
            chkAutoOrders.Checked = _toggleAll;
            chkAutoTrans.Checked = _toggleAll;
            chkAutoIndustryJobs.Checked = _toggleAll;
            SetOverallUpdateState();
            _toggleAll = !_toggleAll;
            _chkClicked = false;
        }

        private void chkUpdate_CheckedChanged(object sender, EventArgs e)
        {
            /*_character.SetAPIAutoUpdate(_type, APIDataType.Assets, chkUpdate.Checked);
            _character.SetAPIAutoUpdate(_type, APIDataType.Orders, chkUpdate.Checked);
            _character.SetAPIAutoUpdate(_type, APIDataType.Journal, chkUpdate.Checked);
            _character.SetAPIAutoUpdate(_type, APIDataType.Transactions, chkUpdate.Checked);*/
            if (!_chkClicked && chkUpdate.CheckState != CheckState.Indeterminate)
            {
                bool state = chkUpdate.Checked;
                chkAutoAssets.Checked = state;
                chkAutoJournal.Checked = state;
                chkAutoOrders.Checked = state;
                chkAutoTrans.Checked = state;
                chkAutoIndustryJobs.Checked = state;
            }
        }


    }


    public class LabelMetaData
    {
        private APIDataType _type;
        private APIUpdateTimerType _timerType;

        public LabelMetaData(APIDataType type)
        {
            _type = type;
            _timerType = APIUpdateTimerType.Normal;
        }

        public APIDataType UpdateType
        {
            get { return _type; }
            set { _type = value; }
        }

        public APIUpdateTimerType TimerType
        {
            get { return _timerType; }
            set { _timerType = value; }
        }
    }

    public class APIUpdateEventArgs : EventArgs
    {
        private APIDataType _updateType;
        private int _id;
        private APIUpdateEventType _eventType;

        public APIUpdateEventArgs(APIDataType updateType, int ownerID, APIUpdateEventType eventType)
        {
            _updateType = updateType;
            _id = ownerID;
            _eventType = eventType;
        }

        public APIDataType UpdateType
        {
            get { return _updateType; }
            set { _updateType = value; }
        }

        public int OwnerID
        {
            get { return _id; }
            set { _id = value; }
        }

        public APIUpdateEventType EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }
    }

    public enum APIUpdateEventType
    {
        UpdateStarted,
        UpdateCompleted,
        OrderHasExpiredOrCompleted,
        AssetsAwaitingAcknowledgement
    }

    public enum APIUpdateTimerType
    {
        Normal,
        Error
    }
}
