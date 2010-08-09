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
    public partial class UpdatePanelCompact : UserControl, IUpdatePanel
    {
        private APICharacter _character;
        private CharOrCorp _type;

        private Color _upToDateColour = Color.LightGreen;
        private Color _errorColour = Color.Red;
        private Color _overdueUpdateColour = Color.Orange;
        private Color _updatingColour = Color.Yellow;

        private static bool _updating = false;
        private static bool _chkClicked = false;

        //private Dictionary<APIDataType, bool> _showingTT = new Dictionary<APIDataType, bool>();
        private bool _showingTT = false;
        private Dictionary<APIDataType, DateTime> _lastUpdateAttempt = new Dictionary<APIDataType, DateTime>();

        public event APIUpdateEvent UpdateEvent;

        public UpdatePanelCompact()
        {
            InitializeComponent();
        }

        public UpdatePanelCompact(CharOrCorp type, APICharacter character)
        {
            InitializeComponent();
            _type = type;
            _character = character;

            lblTransStatus.BackColor = _upToDateColour;

            _lastUpdateAttempt.Add(APIDataType.Assets, DateTime.MinValue);
            _lastUpdateAttempt.Add(APIDataType.Journal, DateTime.MinValue);
            _lastUpdateAttempt.Add(APIDataType.Orders, DateTime.MinValue);
            _lastUpdateAttempt.Add(APIDataType.Transactions, DateTime.MinValue);
            _lastUpdateAttempt.Add(APIDataType.IndustryJobs, DateTime.MinValue);

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

            chkUpdate.Enabled = !Globals.EveAPIDown;

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

            lblAssetsStatus.Tag = new LabelMetaData(APIDataType.Assets);
            lblJournalStatus.Tag = new LabelMetaData(APIDataType.Journal);
            lblOrdersStatus.Tag = new LabelMetaData(APIDataType.Orders);
            lblTransStatus.Tag = new LabelMetaData(APIDataType.Transactions);
            lblIndustryJobsStatus.Tag = new LabelMetaData(APIDataType.IndustryJobs);

            lblTransStatus.MouseHover += new EventHandler(lblStatus_MouseHover);
            lblTransStatus.MouseLeave += new EventHandler(lblStatus_MouseLeave);
            lblAssetsStatus.MouseHover += new EventHandler(lblStatus_MouseHover);
            lblAssetsStatus.MouseLeave += new EventHandler(lblStatus_MouseLeave);
            lblIndustryJobsStatus.MouseHover += new EventHandler(lblStatus_MouseHover);
            lblIndustryJobsStatus.MouseLeave += new EventHandler(lblStatus_MouseLeave);
            lblOrdersStatus.MouseHover += new EventHandler(lblStatus_MouseHover);
            lblOrdersStatus.MouseLeave += new EventHandler(lblStatus_MouseLeave);
            lblJournalStatus.MouseHover += new EventHandler(lblStatus_MouseHover);
            lblJournalStatus.MouseLeave += new EventHandler(lblStatus_MouseLeave);

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

        void lblStatus_MouseLeave(object sender, EventArgs e)
        {
            _showingTT = false;
        }

        void lblStatus_MouseHover(object sender, EventArgs e)
        {
            if (!_showingTT)
            {
                StringBuilder tipText = new StringBuilder("");
                foreach (string typeName in Enum.GetNames(typeof(APIDataType)))
                {
                    APIDataType type = (APIDataType)Enum.Parse(typeof(APIDataType), typeName);

                    if (tipText.Length > 0) { tipText.Append("\r\n"); }
                    tipText.Append(typeName.ToUpper());
                    tipText.Append(": ");

                    string statusText = _character.GetLastAPIUpdateError(_type, type);

                    if (statusText.Equals("BLOCKED"))
                    {
                        int minutes = UserAccount.Settings.AssetsUpdateMaxMinutes;
                        tipText.Append("This update is currently blocked because transaction and order updates have not occured within the last ");
                        tipText.Append(minutes);
                        tipText.Append(" minutes.\r\n\tTo adjust this setting, goto Settings -> API Update Settings.");
                    }
                    else if (statusText.Equals("AWAITING ACKNOWLEDGEMENT"))
                    {
                        tipText.Append("This update has completed but is currently waiting for other asset updates to complete in order to compare lost/gained items.");
                    }
                    else
                    {
                        tipText.Append(statusText);
                    }
                }
                
                errorToolTip.Show(tipText.ToString(), this.Parent,
                    new Point(MousePosition.X - Parent.PointToScreen(Parent.Location).X + 10,
                    MousePosition.Y - Parent.PointToScreen(Parent.Location).Y), 6000);
                _showingTT = true;
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

                UpdateLabel(lblTransStatus, _type, APIDataType.Transactions, 
                    UserAccount.Settings.APITransUpdatePeriod);
                UpdateLabel(lblJournalStatus, _type, APIDataType.Journal, 
                    UserAccount.Settings.APIJournUpdatePeriod);
                UpdateLabel(lblOrdersStatus, _type, APIDataType.Orders, 
                    UserAccount.Settings.APIOrderUpdatePeriod);
                UpdateLabel(lblAssetsStatus, _type, APIDataType.Assets,
                    UserAccount.Settings.APIAssetUpdatePeriod);
                UpdateLabel(lblIndustryJobsStatus, _type, APIDataType.IndustryJobs,
                    UserAccount.Settings.APIIndustryJobsUpdatePeriod);

                _updating = false;
            }
        }

        private void RefreshCheckboxDisplay()
        {
            chkUpdate.Checked = !Globals.EveAPIDown &&
                _character.GetAPIAutoUpdate(_type, APIDataType.Assets) &&
                _character.GetAPIAutoUpdate(_type, APIDataType.Journal) &&
                _character.GetAPIAutoUpdate(_type, APIDataType.Transactions) &&
                _character.GetAPIAutoUpdate(_type, APIDataType.Orders) &&
                _character.GetAPIAutoUpdate(_type, APIDataType.IndustryJobs);
        }

        private void UpdateLabel(Label label, CharOrCorp corc, APIDataType dataType, 
            TimeSpan minTimeBetweenUpdates)
        {
            DateTime lastDataUpdate = _character.GetLastAPIUpdateTime(corc, dataType);
            TimeSpan time = DateTime.UtcNow.Subtract(lastDataUpdate);
            string errorText = _character.GetLastAPIUpdateError(corc, dataType);
            bool doUpdate = false;
            bool checkForAccess = false;

            if (errorText.Equals("") || (_type == CharOrCorp.Corp && (
                errorText.ToUpper().Contains("CHARACTER MUST BE A") ||
                errorText.ToUpper().Contains("CHARACTER MUST HAVE"))))
            {
                if (_type == CharOrCorp.Corp && !_character.CharHasCorporateAccess(dataType))
                {
                    // No access to corporate data of this type
                    //label.Text = "No Access";
                    label.BackColor = _errorColour;
                    if (chkUpdate.Checked) { checkForAccess = true; }
                }
                else if (minTimeBetweenUpdates.CompareTo(time) > 0)
                {
                    time = minTimeBetweenUpdates.Subtract(time);
                    // Waiting for next update window
                    //label.Text = time.Hours.ToString().PadLeft(2, '0') + ":" +
                    //    time.Minutes.ToString().PadLeft(2, '0') + ":" +
                    //    time.Seconds.ToString().PadLeft(2, '0');
                    label.BackColor = _upToDateColour;
                }
                else
                {
                    // Update overdue
                    //label.Text = "Overdue";
                    label.BackColor = _overdueUpdateColour;
                    doUpdate = true;
                }
            }
            else if (errorText.ToUpper().Equals("UPDATING") || errorText.ToUpper().Equals("DOWNLOADING"))
            {
                // The update is in progress.
                //label.Text = "Updating";
                label.BackColor = _updatingColour;
            }
            else if (errorText.ToUpper().Equals("QUEUED"))
            {
                // The thread performing the update has been started but is currently waiting 
                // for some other update to complete before it can proceed.
                // No transaction, orders, assets or industry jobs update can be running at the 
                // same time for a particular character or corp.
                // No journal or transaction update can be running at the same time for ANY 
                // character or corp.
                //label.Text = "Queued";
                label.BackColor = _updatingColour;
            }
            else if (errorText.ToUpper().Equals("AWAITING ACKNOWLEDGEMENT"))
            {
                //if (!label.Text.Equals(WAITINGTEXT))
                //{
                    //label.Text = WAITINGTEXT;
                    label.BackColor = _updatingColour;
                //}
            }
            else
            {
                // The last update caused an error of some sort.
                if (minTimeBetweenUpdates.CompareTo(time) > 0)
                {
                    time = minTimeBetweenUpdates.Subtract(time);
                    // Waiting for next update window
                    //label.Text = "Error " + time.Hours.ToString().PadLeft(2, '0') + ":" +
                    //    time.Minutes.ToString().PadLeft(2, '0') + ":" +
                    //    time.Seconds.ToString().PadLeft(2, '0');
                    label.BackColor = _errorColour;
                    LabelMetaData metaData = (LabelMetaData)label.Tag;
                    metaData.TimerType = APIUpdateTimerType.Normal;
                }
                else
                {
                    LabelMetaData metaData = (LabelMetaData)label.Tag;
                    if (metaData.TimerType == APIUpdateTimerType.Normal)
                    {
                        // Update overdue
                        label.Text = "Overdue";
                        label.BackColor = _overdueUpdateColour;
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
                            doUpdate = true;
                        }
                        else
                        {
                            // Error on the last update
                            time = new TimeSpan(0, 61, 0);
                            time = time.Subtract(timeSinceLastAttempt);
                            //label.Text = "Error " + time.Hours.ToString().PadLeft(2, '0') + ":" +
                            //        time.Minutes.ToString().PadLeft(2, '0') + ":" +
                            //        time.Seconds.ToString().PadLeft(2, '0'); ;
                            label.BackColor = _errorColour;
                        }
                    }
                }
            }


            if (checkForAccess || (doUpdate && _character.GetAPIAutoUpdate(corc, dataType)))
            {
                LabelMetaData metaData = (LabelMetaData)label.Tag;
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

}
