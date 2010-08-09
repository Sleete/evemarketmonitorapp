using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.GUIElements.Interfaces;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class UpdateStatus : Form
    {
        private Dictionary<string, IUpdatePanel> panels = new Dictionary<string, IUpdatePanel>();
        private static bool _allowClose = false;
        private static bool _closing = false;
        private static bool _updating = false;
        private static List<APICharacter> charsListeningTo = new List<APICharacter>();

        public event APIUpdateEvent UpdateEvent;

        public bool AllowClose
        {
            get { return _allowClose; }
            set { _allowClose = value; }
        }

        public UpdateStatus()
        {
            InitializeComponent();
        }

        private void UpdateStatus_Load(object sender, EventArgs e)
        {
            IUpdatePanel tmpPanel;
            if (UserAccount.Settings.UseCompactUpdatePanel)
            {
                tmpPanel = new UpdatePanelCompact();
            }
            else
            {
                tmpPanel = new UpdatePanel();
            }
            this.Width = tmpPanel.Width + 8 + 6;
            mainPanel.Width = tmpPanel.Width + 6;
            PopulatePanels();
            updateTimer.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Never allow user to close this. 
            // If the close command has come from the parent window then we need to allow it though.
            if (_allowClose)
            {
                _closing = true;
                updateTimer.Stop();
            }
            e.Cancel = !_allowClose;
            base.OnClosing(e);
        }

        public void PopulatePanels()
        {
            PauseUpdates();
            int position = 4;
            List<int> ids = new List<int>();

            Dictionary<string, IUpdatePanel>.Enumerator e = panels.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.Value.Dispose();
            }
            mainPanel.Controls.Clear();
            panels.Clear();
            

            if (UserAccount.CurrentGroup != null)
            {
                foreach (EVEAccount account in UserAccount.CurrentGroup.Accounts)
                {
                    foreach (APICharacter character in account.Chars)
                    {
                        if (!charsListeningTo.Contains(character))
                        {
                            character.UpdateEvent += new APIUpdateEvent(character_UpdateEvent);
                            charsListeningTo.Add(character);
                        }
                        if (character.CharIncWithRptGroup)
                        {
                            if (!ids.Contains(character.CharID))
                            {
                                IUpdatePanel panel;
                                if (UserAccount.Settings.UseCompactUpdatePanel)
                                {
                                    panel = new UpdatePanelCompact(CharOrCorp.Char, character);
                                }
                                else
                                {
                                    panel = new UpdatePanel(CharOrCorp.Char, character);
                                }
                                panel.Size = new Size(this.Width - 12 - (position >= this.Height - 
                                    System.Windows.Forms.SystemInformation.CaptionHeight ? 
                                    System.Windows.Forms.SystemInformation.VerticalScrollBarWidth : 0), 
                                    panel.Size.Height);
                                panel.Location = new Point(4, position);
                                panel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                                panel.UpdateEvent += new APIUpdateEvent(panel_UpdateEvent);
                                mainPanel.Controls.Add(panel as UserControl);
                                position += panel.Height + 8;
                                panels.Add("C" + character.CharID, panel);
                                ids.Add(character.CharID);
                            }
                        }
                        if (character.CorpIncWithRptGroup)
                        {
                            if (!ids.Contains(character.CorpID))
                            {
                                IUpdatePanel panel;
                                if (UserAccount.Settings.UseCompactUpdatePanel)
                                {
                                    panel = new UpdatePanelCompact(CharOrCorp.Corp, character);
                                }
                                else
                                {
                                    panel = new UpdatePanel(CharOrCorp.Corp, character);
                                }
                                panel.Size = new Size(this.Width - 12 - (position > this.Height ? 
                                    System.Windows.Forms.SystemInformation.VerticalScrollBarWidth : 0), 
                                    panel.Size.Height);
                                panel.Location = new Point(4, position);
                                panel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                                panel.UpdateEvent += new APIUpdateEvent(panel_UpdateEvent);
                                mainPanel.Controls.Add(panel as UserControl);
                                position += panel.Height + 8;
                                panels.Add("O" + character.CharID, panel);
                                ids.Add(character.CorpID);
                            }
                        }
                    }
                }
            }
            ResumeUpdates();
        }

        void character_UpdateEvent(object myObject, APIUpdateEventArgs args)
        {
            if (UpdateEvent != null)
            {
                UpdateEvent(myObject, args);
            }
        }

        void panel_UpdateEvent(object myObject, APIUpdateEventArgs args)
        {
            if (UpdateEvent != null)
            {
                UpdateEvent(myObject, args);
            }
        }

        public void PauseUpdates() 
        {
            updateTimer.Stop();
        }

        public void ResumeUpdates()
        {
            updateTimer.Start();
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (!_closing && !_updating)
            {
                _updating = true;
                Dictionary<string, IUpdatePanel>.Enumerator enumerator = panels.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Value.UpdateData();
                }
                _updating = false;
            }
        }

        public void BlockUntilUpdated()
        {
            DateTime startBlockTime = DateTime.UtcNow;
            while (_updating && startBlockTime.AddMinutes(1).CompareTo(DateTime.UtcNow) > 0) { }
            PauseUpdates();
        }


    }
}