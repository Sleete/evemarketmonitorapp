using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EveMarketMonitorApp.AbstractionClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.GUIElements
{
    public partial class Tutorial : Form
    {
        private TutSectionsList _sections;
        private TutorialSection _currentSection;
        private TutorialIndex _index;

        public Tutorial()
        {
            InitializeComponent();
        }

        private void Tutorial_Load(object sender, EventArgs e)
        {
            _sections = TutorialData.GetTutorialData();
            if (_sections != null && _sections.Count > 0)
            {
                SetSection(_sections[0].Title, true);
            }
            else
            {
                MessageBox.Show("No tutorial data to display", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
            }
        }


        private void SetSection(string name, bool display)
        {
            TutorialSection section = _sections.GetSection(name);
            if (section != null)
            {
                _currentSection = section;
                if (display)
                {
                    lblTitle.Text = _currentSection.TitleText + "(Entry " + _sections.IndexOf(_currentSection) + " of " + _sections.Count + ")";
                    lblText.Text = _currentSection.Text;
                    btnNext.Enabled = !_currentSection.NextSection.Equals("");
                    btnPrevious.Enabled = !_currentSection.PrevSection.Equals("");
                    btnSkip.Enabled = _currentSection.Level != 0;
                }
            }
            else if (display)
            {
                MessageBox.Show("Cannot find tutorial section '" + name + "'", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                throw new EMMAException(ExceptionSeverity.Error, "Cannot find tutorial section '" + name + "'");
            }
        }

        private void btnIndex_Click(object sender, EventArgs e)
        {
            if (_index == null || !_index.Visible)
            {
                _index = new TutorialIndex(_sections);
                _index.MdiParent = this.MdiParent;
                _index.Show();
                _index.SetSelected(_currentSection.Text);
                _index.TutorialSectionSelected += new TutorialSectionSelectedHandler(index_TutorialSectionSelected);
            }
        }

        void index_TutorialSectionSelected(object myObject, TutorialSectionArgs args)
        {
            SetSection(args.SectionName, true);
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            SetSection(_currentSection.PrevSection, true);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            SetSection(_currentSection.NextSection, true);
        }

        private void btnSkip_Click(object sender, EventArgs e)
        {
            int currentLevel = _currentSection.Level;
            try
            {
                while (currentLevel <= _currentSection.Level && !_currentSection.NextSection.Equals(""))
                {
                    SetSection(_currentSection.NextSection, false);
                }
            }
            catch (EMMAException) { /*If moving to the next section fails then just stop where we are*/ }
            SetSection(_currentSection.Title, true);
        }

        private void showSystemLanguage_Click(object sender, EventArgs e)
        {
            string tag = System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag;
            MessageBox.Show(tag, "System language", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



    }


}