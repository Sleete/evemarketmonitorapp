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

namespace EveMarketMonitorApp.GUIElements
{
    public delegate void APIUpdateEvent(object myObject, APIUpdateEventArgs args);

    public partial class UpdatePanel : UserControl
    {
        private APICharacter _character;
        private CharOrCorp _type;

        private Color _upToDateColour = Color.LightGreen;
        private Color _errorColour = Color.Red;
        private Color _overdueUpdateColour = Color.Orange;
        private Color _updatingColour = Color.Yellow;

        private static bool _updating = false;

        private Dictionary<APIDataType, bool> _showingTT = new Dictionary<APIDataType, bool>();
        private Dictionary<APIDataType, DateTime> _lastUpdateAttempt = new Dictionary<APIDataType, DateTime>();

        public event APIUpdateEvent UpdateEvent;

        public bool _toggleAll = false;

        private static string BLOCKEDTEXT = "Blocked, Retrying..";

        public UpdatePanel()
        {
            InitializeComponent();
        }

        public UpdatePanel(CharOrCorp type, APICharacter character)
        {
            InitializeComponent();
            _type = type;
            _character = character;

            _showingTT.Add(APIDataType.Assets, false);
            _showingTT.Add(APIDataType.Journal, false);
            _showingTT.Add(APIDataType.Orders, false);
            _showingTT.Add(APIDataType.Transactions, false);
            _lastUpdateAttempt.Add(APIDataType.Assets, DateTime.MinValue);
            _lastUpdateAttempt.Add(APIDataType.Journal, DateTime.MinValue);
            _lastUpdateAttempt.Add(APIDataType.Orders, DateTime.MinValue);
            _lastUpdateAttempt.Add(APIDataType.Transactions, DateTime.MinValue);

            if (type == CharOrCorp.Char)
            {
                picPortrait.Image = Portaits.GetPortrait(character.CharID);
                lblCorpTag.Visible = false;
                chkAutoAssets.Checked = !Globals.EveAPIDown && character.GetAPIAutoUpdate(CharOrCorp.Char, APIDataType.Assets);
                chkAutoJournal.Checked = !Globals.EveAPIDown && character.GetAPIAutoUpdate(CharOrCorp.Char, APIDataType.Journal);
                chkAutoOrders.Checked = !Globals.EveAPIDown && character.GetAPIAutoUpdate(CharOrCorp.Char, APIDataType.Orders);
                chkAutoTrans.Checked = !Globals.EveAPIDown && character.GetAPIAutoUpdate(CharOrCorp.Char, APIDataType.Transactions);
            }
            else
            {
                picPortrait.Image = null;
                picPortrait.BorderStyle = BorderStyle.FixedSingle;
                lblCorpTag.Visible = true;
                lblCorpTag.Text = "[" + character.CorpTag + "]";
                chkAutoAssets.Checked = !Globals.EveAPIDown && character.GetAPIAutoUpdate(CharOrCorp.Corp, APIDataType.Assets);
                chkAutoJournal.Checked = !Globals.EveAPIDown && character.GetAPIAutoUpdate(CharOrCorp.Corp, APIDataType.Journal);
                chkAutoOrders.Checked = !Globals.EveAPIDown && character.GetAPIAutoUpdate(CharOrCorp.Corp, APIDataType.Orders);
                chkAutoTrans.Checked = !Globals.EveAPIDown && character.GetAPIAutoUpdate(CharOrCorp.Corp, APIDataType.Transactions);
            
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

            chkAutoAssets.Enabled = !Globals.EveAPIDown;
            chkAutoJournal.Enabled = !Globals.EveAPIDown;
            chkAutoOrders.Enabled = !Globals.EveAPIDown;
            chkAutoTrans.Enabled = !Globals.EveAPIDown;

            chkAutoAssets.CheckedChanged += new EventHandler(chkAutoAssets_CheckedChanged);
            chkAutoJournal.CheckedChanged += new EventHandler(chkAutoJournal_CheckedChanged);
            chkAutoOrders.CheckedChanged += new EventHandler(chkAutoOrders_CheckedChanged);
            chkAutoTrans.CheckedChanged += new EventHandler(chkAutoTrans_CheckedChanged);

            lblAssets.Tag = new LabelMetaData(APIDataType.Assets);
            lblAssetsStatus.Tag = new LabelMetaData(APIDataType.Assets);
            lblJournal.Tag = new LabelMetaData(APIDataType.Journal);
            lblJournalStatus.Tag = new LabelMetaData(APIDataType.Journal);
            lblOrders.Tag = new LabelMetaData(APIDataType.Orders);
            lblOrdersStatus.Tag = new LabelMetaData(APIDataType.Orders);
            lblTransactions.Tag = new LabelMetaData(APIDataType.Transactions);
            lblTransStatus.Tag =new LabelMetaData( APIDataType.Transactions);

            lblAssets.MouseHover += new EventHandler(Label_MouseHover);
            lblAssetsStatus.MouseHover += new EventHandler(Label_MouseHover);
            lblJournal.MouseHover += new EventHandler(Label_MouseHover);
            lblJournalStatus.MouseHover += new EventHandler(Label_MouseHover);
            lblOrders.MouseHover += new EventHandler(Label_MouseHover);
            lblOrdersStatus.MouseHover += new EventHandler(Label_MouseHover);
            lblTransactions.MouseHover += new EventHandler(Label_MouseHover);
            lblTransStatus.MouseHover += new EventHandler(Label_MouseHover);

            lblAssets.MouseLeave += new EventHandler(Label_MouseLeave);
            lblAssetsStatus.MouseLeave += new EventHandler(Label_MouseLeave);
            lblJournal.MouseLeave += new EventHandler(Label_MouseLeave);
            lblJournalStatus.MouseLeave += new EventHandler(Label_MouseLeave);
            lblOrders.MouseLeave += new EventHandler(Label_MouseLeave);
            lblOrdersStatus.MouseLeave += new EventHandler(Label_MouseLeave);
            lblTransactions.MouseLeave += new EventHandler(Label_MouseLeave);
            lblTransStatus.MouseLeave += new EventHandler(Label_MouseLeave);

            // Removed this because it causes the update to run before the creating
            // proceedure has had a chance to attach it's event listeners..
            //UpdateData();
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
                errorToolTip.Show(_character.GetLastAPIUpdateError(_type, type), this.Parent,
                    new Point(MousePosition.X - Parent.PointToScreen(Parent.Location).X + 10, 
                    MousePosition.Y - Parent.PointToScreen(Parent.Location).Y), 3000);
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

                _updating = false;
            }
        }

        private void UpdateLabel(Label label, Label otherLabel, CharOrCorp corc, APIDataType dataType, TimeSpan minTimeBetweenUpdates)
        {
            DateTime lastDataUpdate = _character.GetLastAPIUpdateTime(corc, dataType);
            TimeSpan time = DateTime.UtcNow.Subtract(lastDataUpdate);
            string errorText = _character.GetLastAPIUpdateError(corc, dataType);
            bool doUpdate = false;
            bool checkForAccess = false;

            if (label.Text.ToUpper().Equals("UPDATING") && !errorText.ToUpper().Equals("UPDATING")
                && !errorText.ToUpper().Equals("BLOCKED"))
            {
                // If the label currently says 'updating' but the error text no longer says 'updating' (or blocked)
                // then fire the update completed event.
                if (UpdateEvent != null)
                {
                    UpdateEvent(this, new APIUpdateEventArgs(dataType, corc ==
                        CharOrCorp.Char ? _character.CharID : _character.CorpID,
                        APIUpdateEventType.UpdateCompleted));
                }
            }

            if (errorText.Equals(""))
            {
                if (_type == CharOrCorp.Corp && !_character.CharHasCorporateAccess(dataType))
                {
                    label.Text = "No Access";
                    label.BackColor = _errorColour;
                    otherLabel.BackColor = _errorColour;
                    switch (dataType)
                    {
                        case APIDataType.Transactions:
                            if (chkAutoTrans.Checked) { checkForAccess = true; chkAutoTrans.Checked = false; }
                            break;
                        case APIDataType.Journal:
                            if (chkAutoJournal.Checked) { checkForAccess = true; chkAutoJournal.Checked = false; }
                            break;
                        case APIDataType.Assets:
                            if (chkAutoAssets.Checked) { checkForAccess = true; chkAutoAssets.Checked = false; }
                            break;
                        case APIDataType.Orders:
                            if (chkAutoOrders.Checked) { checkForAccess = true; chkAutoOrders.Checked = false; }
                            break;
                        default:
                            break;
                    }
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
                    // Make sure we let the rest of EMMA know that the update has stoped.
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
                _character.UpdateDataFromAPI(corc, dataType);
                if (corc == CharOrCorp.Corp && dataType == APIDataType.Orders)
                {
                    // If we're dealing with corporate orders then we need to grab corporate orders for 
                    // all characters in this report group that are part of the corp.
                    // This is because orders will only be returned that were actually created by
                    // the character we are retrieving data for.
                    foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                    {
                        foreach(APICharacter character in account.Chars) 
                        {
                            if (character.CorpID == _character.CorpID && character.CharID != _character.CharID)
                            {
                                if (character.CharHasCorporateAccess(APIDataType.Orders))
                                {
                                    character.UpdateDataFromAPI(corc, dataType);
                                }
                            }
                        }
                    }
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
                    default:
                        break;
                }
            }
        }

        private void chkAutoTrans_CheckedChanged(object sender, EventArgs e)
        {
            _character.SetAPIAutoUpdate(_type, APIDataType.Transactions, chkAutoTrans.Checked);
        }

        private void chkAutoJournal_CheckedChanged(object sender, EventArgs e)
        {
            _character.SetAPIAutoUpdate(_type, APIDataType.Journal, chkAutoJournal.Checked);
        }

        private void chkAutoOrders_CheckedChanged(object sender, EventArgs e)
        {
            _character.SetAPIAutoUpdate(_type, APIDataType.Orders, chkAutoOrders.Checked);
        }

        private void chkAutoAssets_CheckedChanged(object sender, EventArgs e)
        {
            _character.SetAPIAutoUpdate(_type, APIDataType.Assets, chkAutoAssets.Checked);
        }

        private void picPortrait_Click(object sender, EventArgs e)
        {
            chkAutoAssets.Checked = _toggleAll;
            chkAutoJournal.Checked = _toggleAll;
            chkAutoOrders.Checked = _toggleAll;
            chkAutoTrans.Checked = _toggleAll;
            _toggleAll = !_toggleAll;
        }

        private void lblCorpTag_Click(object sender, EventArgs e)
        {
            chkAutoAssets.Checked = _toggleAll;
            chkAutoJournal.Checked = _toggleAll;
            chkAutoOrders.Checked = _toggleAll;
            chkAutoTrans.Checked = _toggleAll;
            _toggleAll = !_toggleAll;
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
        OrderHasExpiredOrCompleted
    }

    public enum APIUpdateTimerType
    {
        Normal,
        Error
    }
}
