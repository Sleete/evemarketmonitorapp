using System;
using System.Collections.Generic;
using System.Text;

namespace EveMarketMonitorApp.Common
{
    public delegate void StatusChangeHandler(object myObject, StatusChangeArgs args);

    /// <summary>
    /// Classes wishing to use the progress indicator dialog must implement this interface and fire the 
    /// OnStatusChange event to update the dialog.
    /// </summary>
    public interface IProvideStatus
    {
        event StatusChangeHandler StatusChange;
    }

    public class StatusChangeArgs : EventArgs
    {
        private int maxProgress;
        private int currentProgress;
        private int maxSubProgress;
        private int currentSubProgress;
        private string section;
        private string sectionStatus;
        private string subProgressDescription;
        private bool done;

        public StatusChangeArgs(int currentProgress, int maxProgress, string section, string sectionStatus, bool done)
        {
            this.section = section;
            this.sectionStatus = sectionStatus;
            this.maxProgress = maxProgress;
            this.currentProgress = currentProgress;
            this.done = done;
            this.currentSubProgress = 0;
            this.maxSubProgress = 0;
            this.subProgressDescription = "";
        }
        public StatusChangeArgs(int currentProgress, int maxProgress, string section, string sectionStatus, bool done,
            int currentSubProgress, int maxSubProgress, string subProgDesc)
        {
            this.section = section;
            this.sectionStatus = sectionStatus;
            this.maxProgress = maxProgress;
            this.currentProgress = currentProgress;
            this.currentSubProgress = currentSubProgress;
            this.maxSubProgress = maxSubProgress;
            this.subProgressDescription = subProgDesc;
            this.done = done;
        }


        public int MaxProgress
        {
            get { return maxProgress; }
            set { maxProgress = value; }
        }

        public int CurrentProgress
        {
            get { return currentProgress; }
            set { currentProgress = value; }
        }

        public string Section
        {
            get { return section; }
            set { section = value; }
        }

        public string SectionStatus
        {
            get { return sectionStatus; }
            set { sectionStatus = value; }
        }

        public bool Done
        {
            get { return done; }
            set { done = value; }
        }

        public int MaxSubProgress
        {
            get { return maxSubProgress; }
            set { maxSubProgress = value; }
        }

        public int CurrentSubProgress
        {
            get { return currentSubProgress; }
            set { currentSubProgress = value; }
        }

        public string SubProgressDescription
        {
            get { return subProgressDescription; }
            set { subProgressDescription = value; }
        }

    }
}
