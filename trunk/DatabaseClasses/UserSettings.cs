using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Drawing;
using System.Windows.Forms;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class UserSettings
    {
        private XmlDocument _xml;
        private bool _changed;

        public UserSettings(string userName)
        {
            BuildDefaultSettings(userName);
            _changed = true;
        }

        public UserSettings(XmlDocument xml)
        {
            _xml = xml;
            _changed = false;
        }

        public XmlDocument Xml 
        {
            get { return _xml; }
        }

        public bool Changed
        {
            get { return _changed; }
            set { _changed = value; }
        }

        #region Private methods
        /// <summary>
        /// Build a new settings file containing default data.
        /// </summary>
        /// <param name="keepAccountDetails"></param>
        private void BuildDefaultSettings(string userName)
        {
            _xml = new XmlDocument();

            XmlDeclaration declNode = _xml.CreateXmlDeclaration("1.0", "UTF-16", String.Empty);
            XmlComment commentNode = _xml.CreateComment("EMMA settings file");
            XmlElement settingsNode = _xml.CreateElement("UserSettings");
            XmlAttribute userNameAttrib = _xml.CreateAttribute("UserName");
            userNameAttrib.Value = userName;
            settingsNode.Attributes.Append(userNameAttrib);

            _xml.AppendChild(declNode);
            _xml.AppendChild(commentNode);
            _xml.AppendChild(settingsNode);

            // Calling GetValue will create the xml nodes for each setting with default data.
            GetValue(Setting.FirstRun);
            GetValue(Setting.ControlPanelXPos);
            GetValue(Setting.ControlPanelYPos);
            GetValue(Setting.UpdatePanelXPos);
            GetValue(Setting.UpdatePanelYPos);
            GetValue(Setting.UpdatePanelXSize);
            GetValue(Setting.UpdatePanelYSize);
            GetValue(Setting.APIAssetUpdatePeriod);
            GetValue(Setting.APIJournUpdatePeriod);
            GetValue(Setting.APIOrderUpdatePeriod);
            GetValue(Setting.APITransUpdatePeriod);
            GetValue(Setting.APIIndustryJobsUpdatePeriod);
            GetValue(Setting.UseLocalTimezone);
            GetValue(Setting.GridCalcEnabled);
            GetValue(Setting.CSVExportDir);
            GetValue(Setting.api_assetsUpdateMaxMinutes);
            GetValue(Setting.ManufacturingMode);
            GetValue(Setting.AssetsViewWarning);
            GetValue(Setting.APIIndividualUpdate);
            GetValue(Setting.CalcCostInAssetView);
            GetValue(Setting.CalcProfitInTransView);
            GetValue(Setting.UseCompactUpdatePanel);
            GetValue(Setting.ShowEMMAInTaskBarWhenMinimised);
            GetValue(Setting.ExtendedDiags);
        }

        /// <summary>
        /// Internal method to return the value of a specified setting from the in-memory settings file.
        /// If the value does not exist then it is added from the default values returned by GetDefaultValue.
        /// </summary>
        /// <param name="node">The setting to return the value for</param>
        /// <returns></returns>
        private string GetValue(Setting node)
        {
            return GetValue(node.ToString());
        }
        private string GetValue(string node)
        {
            string retVal = "";
            XmlNode xmlNode = GetValueNode(node);
            if (xmlNode != null)
            {
                if (xmlNode.FirstChild != null)
                {
                    retVal = xmlNode.FirstChild.Value;
                }
            }
            return retVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private XmlNode GetValueNode(string node)
        {
            XmlNode xmlNode = _xml.SelectSingleNode("/UserSettings/" + node);

            try
            {
                if (xmlNode == null)
                {
                    XmlNode parent = _xml.SelectSingleNode("/UserSettings");
                    xmlNode = _xml.CreateElement(node);
                    parent.AppendChild(xmlNode);
                }
                if (xmlNode.FirstChild == null)
                {
                    Setting setting = Setting.APIAssetUpdatePeriod;
                    bool createDefault = false;
                    try
                    {
                        setting = (Setting)Enum.Parse(typeof(Setting), node);
                        createDefault = true;
                    }
                    catch { }
                    if (createDefault)
                    {
                        XmlText xmlValue = _xml.CreateTextNode(GetDefaultValue(setting));
                        xmlNode.AppendChild(xmlValue);
                    }
                }
            }
            catch (Exception ex)
            {
                EMMASettingsException emmaEx = new EMMASettingsException(ExceptionSeverity.Error,
                    "Error getting value from user settings file for " + node, ex);
            }

            return xmlNode;
        }

        /// <summary>
        /// Internal method to set the value of a specified setting in the in-memory settings file.
        /// If the specified setting does not exist then it is added.
        /// </summary>
        /// <param name="node">The setting to set the value of</param>
        /// <param name="value">The value to assign to the specified setting</param>
        private void SetValue(Setting node, string value)
        {
            SetValue(node.ToString(), value);
        }
        /// <summary>
        /// Internal method to set the value of a specified setting in the in-memory settings file.
        /// If the specified setting does not exist then it is added.
        /// </summary>
        /// <param name="node">The setting to set the value of</param>
        /// <param name="value">The value to assign to the specified setting</param>
        private void SetValue(string node, string value)
        {
            XmlNode xmlNode = _xml.SelectSingleNode("/UserSettings/" + node);

            try
            {
                if (xmlNode == null)
                {
                    XmlNode parent = _xml.SelectSingleNode("/UserSettings");
                    xmlNode = _xml.CreateElement(node);
                    parent.AppendChild(xmlNode);
                }
                if (xmlNode.FirstChild == null)
                {
                    XmlText xmlValue = _xml.CreateTextNode(value);
                    xmlNode.AppendChild(xmlValue);
                }
                else
                {
                    xmlNode.FirstChild.Value = value;
                }
                _changed = true;
            }
            catch (Exception ex)
            {
                EMMASettingsException emmaEx = new EMMASettingsException(ExceptionSeverity.Error,
                    "Error setting value in user settings file for " + node, ex);
            }
        }

        /// <summary>
        /// Returns a hardcoded default value for the specified setting
        /// </summary>
        /// <param name="node">The setting to return the default value for</param>
        /// <returns>The default value of the specified setting</returns>
        private string GetDefaultValue(Setting node)
        {
            string retVal = "";
            int val;

            switch (node)
            {
                case Setting.FirstRun:
                    retVal = bool.TrueString;
                    break;
                // Note that if control or update panels x/y pos is set to 0 then the actual position used
                // is decided by the main form.
                // If the panels are then moved/resized then the settings will be updated at that time.
                case Setting.ControlPanelXPos:
                    val = 0;
                    retVal = val.ToString();
                    break;
                case Setting.ControlPanelYPos:
                    val = 0;
                    retVal = val.ToString();
                    break;
                case Setting.UpdatePanelXPos:
                    val = 0;
                    retVal = val.ToString();
                    break;
                case Setting.UpdatePanelYPos:
                    val = 0;
                    retVal = val.ToString();
                    break;
                case Setting.UpdatePanelXSize:
                    val = 0;
                    retVal = val.ToString();
                    break;
                case Setting.UpdatePanelYSize:
                    val = 0;
                    retVal = val.ToString();
                    break;
                case Setting.APITransUpdatePeriod:
                    TimeSpan time = new TimeSpan(1, 1, 0);
                    retVal = time.ToString();
                    break;
                case Setting.APIJournUpdatePeriod:
                    TimeSpan time2 = new TimeSpan(1, 1, 0);
                    retVal = time2.ToString();
                    break;
                case Setting.APIOrderUpdatePeriod:
                    TimeSpan time3 = new TimeSpan(1, 1, 0);
                    retVal = time3.ToString();
                    break;
                case Setting.APIAssetUpdatePeriod:
                    TimeSpan time4 = new TimeSpan(23, 1, 0);
                    retVal = time4.ToString();
                    break;
                case Setting.APIIndustryJobsUpdatePeriod:
                    TimeSpan time5 = new TimeSpan(1, 1, 0);
                    retVal = time5.ToString();
                    break;
                case Setting.UseLocalTimezone:
                    retVal = bool.TrueString;
                    break;
                case Setting.GridCalcEnabled:
                    retVal = bool.TrueString;
                    break;
                case Setting.CSVExportDir:
                    retVal = System.Environment.CurrentDirectory;
                    break;
                case Setting.api_assetsUpdateMaxMinutes:
                    retVal = "10";
                    break;
                case Setting.ManufacturingMode:
                    retVal = bool.FalseString;
                    break;
                case Setting.AssetsViewWarning:
                    retVal = "WARN"; // Can be 'WARN', 'FORCE YES', 'FORCE NO'
                    break;
                case Setting.APIIndividualUpdate:
                    retVal = bool.FalseString;
                    break;
                case Setting.CalcCostInAssetView:
                    retVal = bool.FalseString;
                    break;
                case Setting.CalcProfitInTransView:
                    retVal = bool.FalseString;
                    break;
                case Setting.UseCompactUpdatePanel:
                    retVal = bool.FalseString;
                    break;
                case Setting.ShowEMMAInTaskBarWhenMinimised:
                    retVal = bool.TrueString;
                    break;
                case Setting.ExtendedDiags:
                    retVal = bool.FalseString;
                    break;
                default:
                    break;
            }

            return retVal;
        }
        #endregion

        #region Public properties to access settings
        public bool FirstRun
        {
            get { return bool.Parse(GetValue(Setting.FirstRun)); }
            set { SetValue(Setting.FirstRun, value.ToString()); }
        }

        public bool GridCalcEnabled
        {
            get { return bool.Parse(GetValue(Setting.GridCalcEnabled)); }
            set { SetValue(Setting.GridCalcEnabled, value.ToString()); }
        }

        public bool UseLocalTimezone
        {
            get { return bool.Parse(GetValue(Setting.UseLocalTimezone)); }
            set { SetValue(Setting.UseLocalTimezone, value.ToString()); }
        }

        public Point ControlPanelPos
        {
            get
            {
                int xPos = int.Parse(GetValue(Setting.ControlPanelXPos));
                int yPos = int.Parse(GetValue(Setting.ControlPanelYPos));
                return new Point(xPos, yPos);
            }
            set
            {
                SetValue(Setting.ControlPanelXPos, value.X.ToString());
                SetValue(Setting.ControlPanelYPos, value.Y.ToString());
            }
        }

        public Point UpdatePanelPos
        {
            get
            {
                int xPos = int.Parse(GetValue(Setting.UpdatePanelXPos));
                int yPos = int.Parse(GetValue(Setting.UpdatePanelYPos));
                return new Point(xPos, yPos);
            }
            set
            {
                SetValue(Setting.UpdatePanelXPos, value.X.ToString());
                SetValue(Setting.UpdatePanelYPos, value.Y.ToString());
            }
        }

        public Size UpdatePanelSize
        {
            get
            {
                int xSize = int.Parse(GetValue(Setting.UpdatePanelXSize));
                int ySize = int.Parse(GetValue(Setting.UpdatePanelYSize));
                return new Size(xSize, ySize);
            }
            set
            {
                SetValue(Setting.UpdatePanelXSize, value.Width.ToString());
                SetValue(Setting.UpdatePanelYSize, value.Height.ToString());
            }
        }

        public TimeSpan APITransUpdatePeriod
        {
            get
            {
                TimeSpan retVal;
                try
                {
                    retVal = TimeSpan.Parse(GetValue(Setting.APITransUpdatePeriod));
                }
                catch
                {
                    retVal = new TimeSpan(1, 1, 0);
                    SetValue(Setting.APITransUpdatePeriod, retVal.ToString());
                }
                return retVal;
            }
            set
            {
                TimeSpan newVal = value;
                if (newVal.CompareTo(new TimeSpan(1, 1, 0)) < 0)
                {
                    newVal = new TimeSpan(1, 1, 0);
                }
                SetValue(Setting.APITransUpdatePeriod, newVal.ToString());
            }
        }

        public TimeSpan APIJournUpdatePeriod
        {
            get
            {
                TimeSpan retVal;
                try
                {
                    retVal = TimeSpan.Parse(GetValue(Setting.APIJournUpdatePeriod));
                }
                catch
                {
                    retVal = new TimeSpan(1, 1, 0);
                    SetValue(Setting.APIJournUpdatePeriod, retVal.ToString());
                }
                return retVal;
            }
            set
            {
                TimeSpan newVal = value;
                if (newVal.CompareTo(new TimeSpan(1, 1, 0)) < 0)
                {
                    newVal = new TimeSpan(1, 1, 0);
                }
                SetValue(Setting.APIJournUpdatePeriod, newVal.ToString());
            }
        }

        public TimeSpan APIOrderUpdatePeriod
        {
            get
            {
                TimeSpan retVal;
                try
                {
                    retVal = TimeSpan.Parse(GetValue(Setting.APIOrderUpdatePeriod));
                }
                catch
                {
                    retVal = new TimeSpan(1, 1, 0);
                    SetValue(Setting.APIOrderUpdatePeriod, retVal.ToString());
                }
                return retVal;
            }
            set
            {
                TimeSpan newVal = value;
                if (newVal.CompareTo(new TimeSpan(1, 1, 0)) < 0)
                {
                    newVal = new TimeSpan(1, 1, 0);
                }
                SetValue(Setting.APIOrderUpdatePeriod, newVal.ToString());
            }
        }

        public TimeSpan APIAssetUpdatePeriod
        {
            get
            {
                TimeSpan retVal;
                try
                {
                    retVal = TimeSpan.Parse(GetValue(Setting.APIAssetUpdatePeriod));
                }
                catch
                {
                    retVal = new TimeSpan(23, 1, 0);
                    SetValue(Setting.APIAssetUpdatePeriod, retVal.ToString());
                }
                return retVal;
            }
            set
            {
                TimeSpan newVal = value;
                if (newVal.CompareTo(new TimeSpan(23, 1, 0)) < 0)
                {
                    newVal = new TimeSpan(23, 1, 0);
                }
                SetValue(Setting.APIAssetUpdatePeriod, newVal.ToString());
            }
        }

        public TimeSpan APIIndustryJobsUpdatePeriod
        {
            get
            {
                TimeSpan retVal;
                try
                {
                    retVal = TimeSpan.Parse(GetValue(Setting.APIIndustryJobsUpdatePeriod));
                }
                catch
                {
                    retVal = new TimeSpan(1, 1, 0);
                    SetValue(Setting.APIIndustryJobsUpdatePeriod, retVal.ToString());
                }
                return retVal;
            }
            set
            {
                TimeSpan newVal = value;
                if (newVal.CompareTo(new TimeSpan(1, 1, 0)) < 0)
                {
                    newVal = new TimeSpan(1, 1, 0);
                }
                SetValue(Setting.APIIndustryJobsUpdatePeriod, newVal.ToString());
            }
        }

        public TimeSpan GetAPIUpdatePeriod(APIDataType type)
        {
            TimeSpan retVal = new TimeSpan(1, 1, 0);
            switch (type)
            {
                case APIDataType.Transactions:
                    retVal = TimeSpan.Parse(GetValue(Setting.APITransUpdatePeriod));
                    break;
                case APIDataType.Journal:
                    retVal = TimeSpan.Parse(GetValue(Setting.APIJournUpdatePeriod));
                    break;
                case APIDataType.Assets:
                    retVal = TimeSpan.Parse(GetValue(Setting.APIAssetUpdatePeriod));
                    break;
                case APIDataType.Orders:
                    retVal = TimeSpan.Parse(GetValue(Setting.APIOrderUpdatePeriod));
                    break;
                case APIDataType.IndustryJobs:
                    retVal = TimeSpan.Parse(GetValue(Setting.APIIndustryJobsUpdatePeriod));
                    break;
                case APIDataType.Unknown:
                    break;
                case APIDataType.Full:
                    break;
                default:
                    break;
            }
            return retVal;
        }

        public int AssetsUpdateMaxMinutes
        {
            get
            {
                return int.Parse(GetValue(Setting.api_assetsUpdateMaxMinutes),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.api_assetsUpdateMaxMinutes, value.ToString(
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }

        public string CSVExportDir
        {
            get { return GetValue(Setting.CSVExportDir); }
            set { SetValue(Setting.CSVExportDir, value); }
        }

        public bool ManufacturingMode
        {
            get { return bool.Parse(GetValue(Setting.ManufacturingMode)); }
            set { SetValue(Setting.ManufacturingMode, value.ToString()); }
        }

        public bool ExtendedDiagnostics
        {
            get { return bool.Parse(GetValue(Setting.ExtendedDiags)); }
            set { SetValue(Setting.ExtendedDiags, value.ToString()); }
        }

        public string AssetsViewWarning
        {
            get { return GetValue(Setting.AssetsViewWarning); }
            set { SetValue(Setting.AssetsViewWarning, value); }
        }

        public bool APIIndividualUpdate
        {
            get { return bool.Parse(GetValue(Setting.APIIndividualUpdate)); }
            set { SetValue(Setting.APIIndividualUpdate, value.ToString()); }
        }

        public bool CalcCostInAssetView
        {
            get { return bool.Parse(GetValue(Setting.CalcCostInAssetView)); }
            set { SetValue(Setting.CalcCostInAssetView, value.ToString()); }
        }
        public bool CalcProfitInTransView
        {
            get { return bool.Parse(GetValue(Setting.CalcProfitInTransView)); }
            set { SetValue(Setting.CalcProfitInTransView, value.ToString()); }
        }
        public bool UseCompactUpdatePanel
        {
            get { return bool.Parse(GetValue(Setting.UseCompactUpdatePanel)); }
            set { SetValue(Setting.UseCompactUpdatePanel, value.ToString()); }
        }
        public bool ShowInTaskbarWhenMinimised
        {
            get { return bool.Parse(GetValue(Setting.ShowEMMAInTaskBarWhenMinimised)); }
            set { SetValue(Setting.ShowEMMAInTaskBarWhenMinimised, value.ToString()); }
        }
        #endregion

        #region Public methods
        public void GetColumnWidths(string formName, DataGridView gridView)
        {
            string widthData = GetValue(formName + "_" + gridView.Name);
            if (!widthData.Equals(""))
            {
                char[] delim = {','};
                string[] widths = widthData.Split(delim);

                foreach (DataGridViewColumn column in gridView.Columns)
                {
                    for (int i = 0; i < widths.Length; i++)
                    {
                        string widthText = widths[i];
                        if (widthText.StartsWith(column.Name))
                        {
                            try
                            {
                                int width = int.Parse(widthText.Substring(widthText.IndexOf("=") + 1));
                                if (width < 10) { width = 10; }
                                if (width > 500) { width = 500; }
                                column.Width = width;
                            }
                            catch { }
                            i = widths.Length;
                        }
                    }
                }
            }
        }

        public void StoreColumnWidths(string formName, DataGridView gridView)
        {
            StringBuilder widthData = new StringBuilder();

            foreach (DataGridViewColumn column in gridView.Columns)
            {
                if (widthData.Length != 0) { widthData.Append(","); }
                widthData.Append(column.Name);
                widthData.Append("=");
                widthData.Append(column.Width);
            }

            SetValue(formName + "_" + gridView.Name, widthData.ToString());
        }

        public void GetFormSizeLoc(Form form)
        {
            string formData = GetValue(form.Name + "_Details");
            if (!formData.Equals(""))
            {
                char[] delim = { ',' };
                string[] data = formData.Split(delim);
                int width = 100, height = 100, xpos = 0, ypos = 0;

                for (int i = 0; i < data.Length; i++)
                {
                    string dataText = data[i];
                    int value = 0;
                    try
                    {
                        value = int.Parse(dataText.Substring(dataText.IndexOf("=") + 1));
                    }
                    catch { }
                    if (value != 0)
                    {
                        if (dataText.StartsWith("Width"))
                        {
                            width = value;
                        }
                        if (dataText.StartsWith("Height"))
                        {
                            height = value;
                        }
                        if (dataText.StartsWith("XPos"))
                        {
                            xpos = value;
                        }
                        if (dataText.StartsWith("YPos"))
                        {
                            ypos = value;
                        }
                    }
                }

                if (width < 0) { width = 100; }
                if (width > Screen.PrimaryScreen.Bounds.Width) { width = Screen.PrimaryScreen.Bounds.Width; }

                if (height < 0) { height = 100; }
                if (height > Screen.PrimaryScreen.Bounds.Height) { width = Screen.PrimaryScreen.Bounds.Height; }

                if (xpos + width < 50) { xpos = 0; }
                if (xpos > Screen.PrimaryScreen.Bounds.Width) { xpos = Screen.PrimaryScreen.Bounds.Width - 50; }

                if (ypos < 0) { ypos = 0; }
                if (ypos > Screen.PrimaryScreen.Bounds.Height) { ypos = Screen.PrimaryScreen.Bounds.Height - 50; }


                form.Width = width;
                form.Height = height;
                form.Location = new Point(xpos, ypos);
            }
        }

        public void StoreFormSizeLoc(Form form)
        {
            StringBuilder data = new StringBuilder();

            data.Append("Width=");
            data.Append(form.Width);
            data.Append(",");
            data.Append("Height=");
            data.Append(form.Height);
            data.Append(",");
            data.Append("XPos=");
            data.Append(form.Location.X);
            data.Append(",");
            data.Append("YPos=");
            data.Append(form.Location.Y);

            SetValue(form.Name + "_Details", data.ToString());
        }
        #endregion

        private enum Setting
        {
            FirstRun,
            ControlPanelXPos,
            ControlPanelYPos,
            UpdatePanelXPos,
            UpdatePanelYPos,
            UpdatePanelXSize,
            UpdatePanelYSize,
            APITransUpdatePeriod,
            APIJournUpdatePeriod,
            APIOrderUpdatePeriod,
            APIAssetUpdatePeriod,
            UseLocalTimezone,
            GridCalcEnabled,
            CSVExportDir,
            api_assetsUpdateMaxMinutes,
            ManufacturingMode,
            AssetsViewWarning,
            APIIndividualUpdate,
            CalcCostInAssetView,
            CalcProfitInTransView,
            APIIndustryJobsUpdatePeriod,
            UseCompactUpdatePanel,
            ShowEMMAInTaskBarWhenMinimised,
            ExtendedDiags
        }

    }

}
