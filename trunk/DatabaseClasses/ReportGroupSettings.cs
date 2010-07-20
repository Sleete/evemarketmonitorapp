using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Drawing;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class ReportGroupSettings
    {
        private XmlDocument _xml;
        private bool _changed;

        public ReportGroupSettings(int reportGroupID)
        {
            BuildDefaultSettings(reportGroupID);
            _changed = true;
        }

        public ReportGroupSettings(XmlDocument xml)
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
        private void BuildDefaultSettings(int reportGroupID)
        {
            _xml = new XmlDocument();

            XmlDeclaration declNode = _xml.CreateXmlDeclaration("1.0", "UTF-16", String.Empty);
            XmlComment commentNode = _xml.CreateComment("EMMA settings file");
            XmlElement settingsNode = _xml.CreateElement("ReportGroupSettings");
            XmlAttribute idAttrib = _xml.CreateAttribute("ReportGroupID");
            idAttrib.Value = reportGroupID.ToString();
            settingsNode.Attributes.Append(idAttrib);

            _xml.AppendChild(declNode);
            _xml.AppendChild(commentNode);
            _xml.AppendChild(settingsNode);

            // Calling GetValue will create the xml nodes for each setting with default data.
            GetValue(Setting.rpt_LogoFile);
            GetValue(Setting.rpt_AltRowFont);
            GetValue(Setting.rpt_AltRowInUse);
            GetValue(Setting.rpt_ColHeaderFont);
            GetValue(Setting.rpt_DataFont);
            GetValue(Setting.rpt_NegDataFont);
            GetValue(Setting.rpt_DangerDataFont);
            GetValue(Setting.rpt_WarningDataFont);
            GetValue(Setting.rpt_GoodDataFont);
            GetValue(Setting.rpt_RowHeaderFont);
            GetValue(Setting.rpt_SectionHeaderFont);
            GetValue(Setting.rpt_TitleFont);
            GetValue(Setting.autoAddItemsBy);
            GetValue(Setting.autoAddMin);
            GetValue(Setting.autoAddBuyMin);
            GetValue(Setting.autoAddSellMin);
            GetValue(Setting.autoAddStartDate);
            GetValue(Setting.autoAddBuyStations);
            GetValue(Setting.autoAddSellStations);
            GetValue(Setting.autocon_allowStackSplitting);
            GetValue(Setting.autocon_destinationStation);
            GetValue(Setting.autocon_pickupLocations);
            GetValue(Setting.autocon_maxCollateral);
            GetValue(Setting.autocon_maxVolume);
            GetValue(Setting.autocon_minCollateral);
            GetValue(Setting.autocon_minReward);
            GetValue(Setting.autocon_minVolume);
            GetValue(Setting.autocon_excludeContainers);
            GetValue(Setting.autocon_tradedItems);
            GetValue(Setting.collateralBasedOn);
            GetValue(Setting.collateralPercentage);
            GetValue(Setting.rewardBasedOn);
            GetValue(Setting.maxReward);
            GetValue(Setting.maxRewardPercentage);
            GetValue(Setting.minReward);
            GetValue(Setting.minRewardPercentage);
            GetValue(Setting.rewardPercPerJump);
            GetValue(Setting.volumeBasedRewardPerc);
            GetValue(Setting.lowSecPickupBonusPerc);
            GetValue(Setting.useEveCentral);
            GetValue(Setting.eveMarketValueToUse);
            GetValue(Setting.recentItemsList);
            GetValue(Setting.recentStationsList);
            GetValue(Setting.recentSystemsList);
            GetValue(Setting.ordersNotifyEnabled);
            GetValue(Setting.ordersNotifyBuy);
            GetValue(Setting.ordersNotifySell);
            GetValue(Setting.route_highSecWeight);
            GetValue(Setting.route_lowSecWeight);
            GetValue(Setting.route_nullSecWeight);
            GetValue(Setting.reproc_arkonorprocessing);
            GetValue(Setting.reproc_bistotprocessing);
            GetValue(Setting.reproc_crokiteprocessing);
            GetValue(Setting.reproc_darkochreprocessing);
            GetValue(Setting.reproc_gneissprocessing);
            GetValue(Setting.reproc_hedbergiteprocessing);
            GetValue(Setting.reproc_hemorphiteprocessing);
            GetValue(Setting.reproc_jaspetprocessing);
            GetValue(Setting.reproc_kerniteprocessing);
            GetValue(Setting.reproc_mercoxitprocessing);
            GetValue(Setting.reproc_omberprocessing);
            GetValue(Setting.reproc_plagioclaseprocessing);
            GetValue(Setting.reproc_pyroxeresprocessing);
            GetValue(Setting.reproc_scorditeprocessing);
            GetValue(Setting.reproc_scrapmetalprocessing);
            GetValue(Setting.reproc_spodumainprocessing);
            GetValue(Setting.reproc_veldsparprocessing);
            GetValue(Setting.reproc_station);
            GetValue(Setting.reproc_stationYield);
            GetValue(Setting.reproc_theyTake);
            GetValue(Setting.reproc_implant);
            GetValue(Setting.reproc_refining);
            GetValue(Setting.reproc_refineryefficiency);
            GetValue(Setting.reproc_reprocessor);
            GetValue(Setting.useEveMetrics);
            GetValue(Setting.itemValueWebExpiryDays);

        }

        /// <summary>
        /// Internal method to return the value of a specified setting from the in-memory settings file.
        /// If the value does not exist then it is added from the default values returned by GetDefaultValue.
        /// </summary>
        /// <param name="node">The setting to return the value for</param>
        /// <returns></returns>
        private string GetValue(Setting node)
        {
            return GetValueNode(node).FirstChild.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private XmlNode GetValueNode(Setting node)
        {
            XmlNode xmlNode = _xml.SelectSingleNode("/ReportGroupSettings/" + node.ToString());

            try
            {
                if (xmlNode == null)
                {
                    XmlNode parent = _xml.SelectSingleNode("/ReportGroupSettings");
                    xmlNode = _xml.CreateElement(node.ToString());
                    parent.AppendChild(xmlNode);
                }
                if (xmlNode.FirstChild == null)
                {
                    XmlText xmlValue = _xml.CreateTextNode(GetDefaultValue(node));
                    xmlNode.AppendChild(xmlValue);
                }
            }
            catch (Exception ex)
            {
                EMMASettingsException emmaEx = new EMMASettingsException(ExceptionSeverity.Error,
                    "Error getting value from report group settings file for " + node.ToString(), ex);
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
            XmlNode xmlNode = _xml.SelectSingleNode("/ReportGroupSettings/" + node.ToString());

            try
            {
                if (xmlNode == null)
                {
                    XmlNode parent = _xml.SelectSingleNode("/ReportGroupSettings");
                    xmlNode = _xml.CreateElement(node.ToString());
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
                    "Error setting value in report group settings file for " + node.ToString(), ex);
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

            switch (node)
            {
                // Traded items auto add settings.
                case Setting.autoAddItemsBy:
                    retVal = "Transactions";
                    break;
                case Setting.autoAddMin:
                    retVal = "10";
                    break;
                case Setting.autoAddBuyMin:
                    retVal = "0";
                    break;
                case Setting.autoAddSellMin:
                    retVal = "0";
                    break;
                case Setting.autoAddStartDate:
                    retVal = (UserAccount.Settings.UseLocalTimezone ? 
                        DateTime.Now.Subtract(new TimeSpan(14, 0, 0, 0)) :
                        DateTime.UtcNow.Subtract(new TimeSpan(14, 0, 0, 0))).ToString(
                        System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
                    break;
                case Setting.autoAddBuyStations:
                    retVal = "";
                    break;
                case Setting.autoAddSellStations:
                    retVal = "";
                    break;
                // Courier contract settings
                case Setting.autocon_allowStackSplitting:
                    retVal = bool.TrueString;
                    break;
                case Setting.autocon_tradedItems:
                    retVal = bool.FalseString;
                    break;
                case Setting.autocon_destinationStation:
                    retVal = "60003760";
                    break;
                case Setting.autocon_pickupLocations:
                    retVal = "All Regions";
                    break;
                case Setting.autocon_maxCollateral:
                    retVal = "300000000";
                    break;
                case Setting.autocon_maxVolume:
                    retVal = "30000";
                    break;
                case Setting.autocon_minCollateral:
                    retVal = "10000000";
                    break;
                case Setting.autocon_minReward:
                    retVal = "0";
                    break;
                case Setting.autocon_minVolume:
                    retVal = "0";
                    break;
                case Setting.autocon_excludeContainers:
                    retVal = bool.TrueString;
                    break;
                case Setting.collateralBasedOn:
                    retVal = "Sell";
                    break;
                case Setting.collateralPercentage:
                    retVal = "105";
                    break;
                case Setting.lowSecPickupBonusPerc:
                    retVal = "1";
                    break;
                case Setting.maxReward:
                    retVal = "4000000";
                    break;
                case Setting.maxRewardPercentage:
                    retVal = "4";
                    break;
                case Setting.minReward:
                    retVal = "0";
                    break;
                case Setting.minRewardPercentage:
                    retVal = "0";
                    break;
                case Setting.rewardBasedOn:
                    retVal = "Collateral";
                    break;
                case Setting.rewardPercPerJump:
                    retVal = "0.16";
                    break;
                case Setting.volumeBasedRewardPerc:
                    retVal = "0.0015";
                    break;
                // Report style settings
                // Note - format for fonts stored as text is:
                // <name>|<size>|<bold>|<italic>|<underline>|<forecolour>|<backcolour>
                // colours are the usual R,G,B format
                case Setting.rpt_AltRowFont:
                    retVal = "Arial|9.75|False|False|False|0,0,0|222,222,255";
                    break;
                case Setting.rpt_AltRowInUse:
                    retVal = Boolean.TrueString;
                    break;
                case Setting.rpt_ColHeaderFont:
                    retVal = "Arial|9.75|False|False|True|0,0,0|255,255,255";
                    break;
                case Setting.rpt_DataFont:
                    retVal = "Arial|9.75|False|False|False|0,0,0|255,255,255";
                    break;
                case Setting.rpt_LogoFile:
                    retVal = "";
                    break;
                case Setting.rpt_NegDataFont:
                    retVal = "Arial|9.75|False|False|False|255,0,0|255,255,255";
                    break;
                case Setting.rpt_DangerDataFont:
                    retVal = "Arial|9.75|False|False|False|255,100,0|255,255,255";
                    break;
                case Setting.rpt_WarningDataFont:
                    retVal = "Arial|9.75|False|False|False|230,200,0|255,255,255";
                    break;
                case Setting.rpt_GoodDataFont:
                    retVal = "Arial|9.75|False|False|False|0,200,0|255,255,255";
                    break;
                case Setting.rpt_RowHeaderFont:
                    retVal = "Arial|9.75|False|False|False|0,0,0|255,255,255";
                    break;
                case Setting.rpt_SectionHeaderFont:
                    retVal = "Arial|9.75|True|False|False|0,0,0|255,255,255";
                    break;
                case Setting.rpt_TitleFont:
                    retVal = "Arial|15.75|True|False|True|0,0,0|255,255,255";
                    break;
                case Setting.rpt_CustomCols:
                    retVal = "0,0,0|0,0,0|0,0,0|0,0,0|0,0,0|0,0,0|0,0,0|0,0,0";
                    break;
                // Item pricing settings
                case Setting.useEveCentral:
                    retVal = bool.TrueString;
                    break;
                case Setting.eveMarketValueToUse:
                    retVal = ((short)EveMarketValueToUse.medianBuy).ToString();
                    break;
                case Setting.useEveMetrics:
                    retVal = bool.FalseString;
                    break;
                // Other settings
                case Setting.courierCalcOnlyItemsTraded:
                    retVal = bool.FalseString;
                    break;
                case Setting.recentStationsList:
                    retVal = "";
                    break;
                case Setting.recentSystemsList:
                    retVal = "";
                    break;
                case Setting.recentItemsList:
                    retVal = "";
                    break;
                case Setting.ordersNotifyEnabled:
                    retVal = bool.TrueString;
                    break;
                case Setting.ordersNotifyBuy:
                    retVal = bool.TrueString;
                    break;
                case Setting.ordersNotifySell:
                    retVal = bool.TrueString;
                    break;
                case Setting.route_highSecWeight:
                    retVal = "1";
                    break;
                case Setting.route_lowSecWeight:
                    retVal = "5";
                    break;
                case Setting.route_nullSecWeight:
                    retVal = "20";
                    break;
                case Setting.reproc_arkonorprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_bistotprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_crokiteprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_darkochreprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_gneissprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_hedbergiteprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_hemorphiteprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_jaspetprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_kerniteprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_mercoxitprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_omberprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_plagioclaseprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_pyroxeresprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_scorditeprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_scrapmetalprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_spodumainprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_veldsparprocessing:
                    retVal = "0";
                    break;
                case Setting.reproc_station:
                    retVal = "60003760";
                    break;
                case Setting.reproc_stationYield:
                    retVal = "50";
                    break;
                case Setting.reproc_reprocessor:
                    retVal = "0";
                    break;
                case Setting.reproc_theyTake:
                    retVal = "5";
                    break;
                case Setting.reproc_implant:
                    retVal = "0";
                    break;
                case Setting.reproc_refining:
                    retVal = "0";
                    break;
                case Setting.reproc_refineryefficiency:
                    retVal = "0";
                    break;
                case Setting.itemValueWebExpiryDays:
                    retVal = "3";
                    break;
                default:
                    break;
            }

            return retVal;
        }
        #endregion

        #region Public properties to access settings
        #region Reprocessing settings
        public int ReprocessStation
        {
            get
            {
                return int.Parse(GetValue(Setting.reproc_station),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.reproc_station, value.ToString(
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public float ReprocessStationYieldPerc
        {
            get
            {
                return float.Parse(GetValue(Setting.reproc_stationYield),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.reproc_stationYield, value.ToString(
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public int ReprocessCharacter
        {
            get
            {
                return int.Parse(GetValue(Setting.reproc_reprocessor),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.reproc_reprocessor, value.ToString(
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public float ReprocessStationWillTakePerc
        {
            get
            {
                return float.Parse(GetValue(Setting.reproc_theyTake),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.reproc_theyTake, value.ToString(
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public int ReprocessImplantPerc
        {
            get
            {
                return int.Parse(GetValue(Setting.reproc_implant),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.reproc_implant, value.ToString(
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public int GetReprocessSkillLevel(Skills skill)
        {
            Setting setting = Setting.reproc_refining;
            string[] possibleSettings = Enum.GetNames(typeof(Setting));
            bool matched = false;
            int retVal = 0;

            foreach (string settingName in possibleSettings)
            {
                if (settingName.ToString().Equals("reproc_" + skill.ToString().ToLower()))
                {
                    setting = (Setting)Enum.Parse(typeof(Setting), settingName);
                    matched = true;
                }
            }

            if (matched)
            {
                retVal = int.Parse(GetValue(setting), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            else
            {
                throw new EMMAException(ExceptionSeverity.Warning, "Unexpected reprocess skill parameter - " +
                    skill.ToString());
            }

            return retVal;
        }
        public void SetReprocessSkillLevel(Skills skill, int level)
        {
            Setting setting = Setting.reproc_refining;
            string[] possibleSettings = Enum.GetNames(typeof(Setting));
            bool matched = false;

            foreach (string settingName in possibleSettings)
            {
                if (settingName.ToString().Equals("reproc_" + skill.ToString().ToLower()))
                {
                    setting = (Setting)Enum.Parse(typeof(Setting), settingName);
                    matched = true;
                }
            }

            if (matched)
            {
                SetValue(setting, level.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
            else
            {
                throw new EMMAException(ExceptionSeverity.Warning, "Unexpected reprocess skill parameter - " +
                    skill.ToString());
            }
        }
        #endregion
        #region Route calculator settings
        public int RouteHighSecWeight
        {
            get
            {
                return int.Parse(GetValue(Setting.route_highSecWeight),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.route_highSecWeight, value.ToString(
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public int RouteLowSecWeight
        {
            get
            {
                return int.Parse(GetValue(Setting.route_lowSecWeight),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.route_lowSecWeight, value.ToString(
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public int RouteNullSecWeight
        {
            get
            {
                return int.Parse(GetValue(Setting.route_nullSecWeight),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.route_nullSecWeight, value.ToString(
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        #endregion
        #region Order notification settings
        public bool OrdersNotifyEnabled
        {
            get { return bool.Parse(GetValue(Setting.ordersNotifyEnabled)); }
            set { SetValue(Setting.ordersNotifyEnabled, value.ToString()); }
        }
        public bool OrdersNotifyBuy
        {
            get { return bool.Parse(GetValue(Setting.ordersNotifyBuy)); }
            set { SetValue(Setting.ordersNotifyBuy, value.ToString()); }
        }
        public bool OrdersNotifySell
        {
            get { return bool.Parse(GetValue(Setting.ordersNotifySell)); }
            set { SetValue(Setting.ordersNotifySell, value.ToString()); }
        }
        #endregion
        #region Traded items auto add settings
        public string AutoAddItemsBy
        {
            get { return GetValue(Setting.autoAddItemsBy); }
            set { SetValue(Setting.autoAddItemsBy, value); }
        }

        public int AutoAddMin
        {
            get
            {
                return int.Parse(GetValue(Setting.autoAddMin),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.autoAddMin,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public int AutoAddBuyMin
        {
            get
            {
                return int.Parse(GetValue(Setting.autoAddBuyMin),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.autoAddBuyMin,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public int AutoAddSellMin
        {
            get
            {
                return int.Parse(GetValue(Setting.autoAddSellMin),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.autoAddSellMin,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public DateTime AutoAddStartDate
        {
            get
            {
                DateTime retVal = DateTime.Now;
                try
                {
                    retVal = DateTime.Parse(GetValue(Setting.autoAddStartDate),
                      System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
                }
                catch (FormatException) { }
                return retVal;
            }
            set
            {
                SetValue(Setting.autoAddStartDate,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat));
            }
        }
        public List<int> AutoAddBuyStations
        {
            get
            {
                List<int> retVal = new List<int>();
                string textList = GetValue(Setting.autoAddBuyStations);
                char[] delim = { '|' };
                string[] stations = textList.Split(delim);
                foreach(string station in stations) 
                {
                    if (station.Trim().Length > 0)
                    {
                        retVal.Add(int.Parse(station,
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                    }
                }
                return retVal;
            }
            set
            {
                StringBuilder newList = new StringBuilder();
                foreach (int station in value)
                {
                    if (newList.ToString().Length != 0) { newList.Append("|"); }
                    newList.Append(station.ToString(
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                }
                SetValue(Setting.autoAddBuyStations, newList.ToString());
            }
        }
        public List<int> AutoAddSellStations
        {
            get
            {
                List<int> retVal = new List<int>();
                string textList = GetValue(Setting.autoAddSellStations);
                char[] delim = { '|' };
                string[] stations = textList.Split(delim);
                foreach(string station in stations) 
                {
                    if (station.Trim().Length > 0)
                    {
                        retVal.Add(int.Parse(station,
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                    }
                }
                return retVal;
            }
            set
            {
                StringBuilder newList = new StringBuilder();
                foreach (int station in value)
                {
                    if (newList.ToString().Length != 0) { newList.Append("|"); }
                    newList.Append(station.ToString(
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                }
                SetValue(Setting.autoAddSellStations, newList.ToString());
            }
        }
        #endregion
        #region Contractor Settings
        public bool AutoCon_AllowStackSplitting
        {
            get { return bool.Parse(GetValue(Setting.autocon_allowStackSplitting)); }
            set { SetValue(Setting.autocon_allowStackSplitting, value.ToString()); }
        }
        public bool AutoCon_ExcludeContainers
        {
            get { return bool.Parse(GetValue(Setting.autocon_excludeContainers)); }
            set { SetValue(Setting.autocon_excludeContainers, value.ToString()); }
        }
        public bool AutoCon_TradedItems
        {
            get { return bool.Parse(GetValue(Setting.autocon_tradedItems)); }
            set { SetValue(Setting.autocon_tradedItems, value.ToString()); }
        }
        public int AutoCon_DestiantionStation
        {
            get
            {
                return int.Parse(GetValue(Setting.autocon_destinationStation),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.autocon_destinationStation,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public string AutoCon_PickupLocations
        {
            get { return GetValue(Setting.autocon_pickupLocations); }
            set { SetValue(Setting.autocon_pickupLocations, value); }
        }
        public decimal AutoCon_MaxCollateral
        {
            get
            {
                return decimal.Parse(GetValue(Setting.autocon_maxCollateral),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.autocon_maxCollateral,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public decimal AutoCon_MaxVolume
        {
            get
            {
                return decimal.Parse(GetValue(Setting.autocon_maxVolume),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.autocon_maxVolume,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public decimal AutoCon_MinCollateral
        {
            get
            {
                return decimal.Parse(GetValue(Setting.autocon_minCollateral),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.autocon_minCollateral,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public decimal AutoCon_MinVolume
        {
            get
            {
                return decimal.Parse(GetValue(Setting.autocon_minVolume),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.autocon_minVolume,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public decimal AutoCon_MinReward
        {
            get
            {
                return decimal.Parse(GetValue(Setting.autocon_minReward),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.autocon_minReward,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public string CollateralBasedOn
        {
            get { return GetValue(Setting.collateralBasedOn); }
            set { SetValue(Setting.collateralBasedOn, value); }
        }
        public decimal CollateralPercentage
        {
            get
            {
                return decimal.Parse(GetValue(Setting.collateralPercentage),
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.collateralPercentage,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public decimal LowSecPickupBonusPerc
        {
            get
            {
                return decimal.Parse(GetValue(Setting.lowSecPickupBonusPerc),
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.lowSecPickupBonusPerc,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public decimal MaxReward
        {
            get
            {
                return decimal.Parse(GetValue(Setting.maxReward),
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.maxReward,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public decimal MaxRewardPercentage
        {
            get
            {
                return decimal.Parse(GetValue(Setting.maxRewardPercentage),
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.maxRewardPercentage,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public decimal MinReward
        {
            get
            {
                return decimal.Parse(GetValue(Setting.minReward),
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.minReward,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public decimal MinRewardPercentage
        {
            get
            {
                return decimal.Parse(GetValue(Setting.minRewardPercentage),
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.minRewardPercentage,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public string RewardBasedOn
        {
            get { return GetValue(Setting.rewardBasedOn); }
            set { SetValue(Setting.rewardBasedOn, value); }
        }
        public decimal RewardPercPerJump
        {
            get
            {
                return decimal.Parse(GetValue(Setting.rewardPercPerJump),
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.rewardPercPerJump,
                    value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        public decimal VolumeBasedRewardPerc
        {
            get
            {
                return decimal.Parse(GetValue(Setting.volumeBasedRewardPerc),
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.volumeBasedRewardPerc,
                    value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }

        public bool CourierCalcOnlyItemsTraded
        {
            get { return bool.Parse(GetValue(Setting.courierCalcOnlyItemsTraded)); }
            set { SetValue(Setting.courierCalcOnlyItemsTraded, value.ToString()); }
        }
        #endregion
        #region General item value settings
        public bool UseEveCentral
        {
            get { return bool.Parse(GetValue(Setting.useEveCentral)); }
            set { SetValue(Setting.useEveCentral, value.ToString()); }
        }
        public EveMarketValueToUse EveMarketType
        {
            get
            {
                EveMarketValueToUse retVal;
                try
                {
                    retVal = (EveMarketValueToUse)short.Parse(GetValue(Setting.eveMarketValueToUse));
                }
                catch
                {
                    retVal = EveMarketValueToUse.medianBuy;
                }
                return retVal;
            }
            set
            {
                short val = (short)value;
                SetValue(Setting.eveMarketValueToUse, val.ToString());
            }
        }
        public bool UseEveMetrics
        {
            get { return bool.Parse(GetValue(Setting.useEveMetrics)); }
            set { SetValue(Setting.useEveMetrics, value.ToString()); }
        }
        public int ItemValueWebExpiryDays
        {
            get
            {
                return int.Parse(GetValue(Setting.itemValueWebExpiryDays),
                  System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            set
            {
                SetValue(Setting.itemValueWebExpiryDays,
                  value.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
            }
        }
        #endregion
        #region Recent lists
        public List<string> RecentItems
        {
            get
            {
                List<string> retVal = new List<string>();
                string textList = GetValue(Setting.recentItemsList);
                char[] delim = { '|' };
                string[] items = textList.Split(delim);
                retVal.AddRange(items);
                return retVal;
            }
            set
            {
                StringBuilder newList = new StringBuilder();
                foreach (string item in value)
                {
                    if (newList.ToString().Length != 0) { newList.Append("|"); }
                    newList.Append(item);
                }
                SetValue(Setting.recentItemsList, newList.ToString());
            }
        }
        public List<string> RecentStations
        {
            get
            {
                List<string> retVal = new List<string>();
                string textList = GetValue(Setting.recentStationsList);
                char[] delim = { '|' };
                string[] stations = textList.Split(delim);
                retVal.AddRange(stations);
                return retVal;
            }
            set
            {
                StringBuilder newList = new StringBuilder();
                foreach (string station in value)
                {
                    if (newList.ToString().Length != 0) { newList.Append("|"); }
                    newList.Append(station);
                }
                SetValue(Setting.recentStationsList, newList.ToString());
            }
        }
        public List<string> RecentSystems
        {
            get
            {
                List<string> retVal = new List<string>();
                string textList = GetValue(Setting.recentSystemsList);
                char[] delim = { '|' };
                string[] systems = textList.Split(delim);
                retVal.AddRange(systems);
                return retVal;
            }
            set
            {
                StringBuilder newList = new StringBuilder();
                foreach (string system in value)
                {
                    if (newList.ToString().Length != 0) { newList.Append("|"); }
                    newList.Append(system);
                }
                SetValue(Setting.recentSystemsList, newList.ToString());
            }
        }
        #endregion
        #region Other settings
        #endregion

        #region Row header font
        public Font Rpt_RowHeaderFont
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_RowHeaderFont);
                return FontFromString(fontInfo); 
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_RowHeaderFont);
                fontInfo = UpdateStringFromFont(fontInfo, value);
                SetValue(Setting.rpt_RowHeaderFont, fontInfo);
            }
        }
        public Color Rpt_RowHeaderTextColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_RowHeaderFont);
                return TextColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_RowHeaderFont);
                fontInfo = UpdateStringFromTextCol(fontInfo, value);
                SetValue(Setting.rpt_RowHeaderFont, fontInfo);
            }
        }
        public Color Rpt_RowHeaderBackColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_RowHeaderFont);
                return BackColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_RowHeaderFont);
                fontInfo = UpdateStringFromBackCol(fontInfo, value);
                SetValue(Setting.rpt_RowHeaderFont, fontInfo);
            }
        }
        #endregion
        #region Alt row font
        public Font Rpt_AltRowFont
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_AltRowFont);
                return FontFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_AltRowFont);
                fontInfo = UpdateStringFromFont(fontInfo, value);
                SetValue(Setting.rpt_AltRowFont, fontInfo);
            }
        }
        public Color Rpt_AltRowTextColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_AltRowFont);
                return TextColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_AltRowFont);
                fontInfo = UpdateStringFromTextCol(fontInfo, value);
                SetValue(Setting.rpt_AltRowFont, fontInfo);
            }
        }
        public Color Rpt_AltRowBackColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_AltRowFont);
                return BackColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_AltRowFont);
                fontInfo = UpdateStringFromBackCol(fontInfo, value);
                SetValue(Setting.rpt_AltRowFont, fontInfo);
            }
        }
        #endregion
        #region Column header font
        public Font Rpt_ColHeaderFont
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_ColHeaderFont);
                return FontFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_ColHeaderFont);
                fontInfo = UpdateStringFromFont(fontInfo, value);
                SetValue(Setting.rpt_ColHeaderFont, fontInfo);
            }
        }
        public Color Rpt_ColHeaderTextColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_ColHeaderFont);
                return TextColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_ColHeaderFont);
                fontInfo = UpdateStringFromTextCol(fontInfo, value);
                SetValue(Setting.rpt_ColHeaderFont, fontInfo);
            }
        }
        public Color Rpt_ColHeaderBackColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_ColHeaderFont);
                return BackColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_ColHeaderFont);
                fontInfo = UpdateStringFromBackCol(fontInfo, value);
                SetValue(Setting.rpt_ColHeaderFont, fontInfo);
            }
        }
        #endregion
        #region Data font
        public Font Rpt_DataFont
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_DataFont);
                return FontFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_DataFont);
                fontInfo = UpdateStringFromFont(fontInfo, value);
                SetValue(Setting.rpt_DataFont, fontInfo);
            }
        }
        public Color Rpt_DataTextColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_DataFont);
                return TextColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_DataFont);
                fontInfo = UpdateStringFromTextCol(fontInfo, value);
                SetValue(Setting.rpt_DataFont, fontInfo);
            }
        }
        public Color Rpt_DataBackColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_DataFont);
                return BackColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_DataFont);
                fontInfo = UpdateStringFromBackCol(fontInfo, value);
                SetValue(Setting.rpt_DataFont, fontInfo);
            }
        }
        #endregion
        #region Negative Data font
        public Font Rpt_NegDataFont
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_NegDataFont);
                return FontFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_NegDataFont);
                fontInfo = UpdateStringFromFont(fontInfo, value);
                SetValue(Setting.rpt_NegDataFont, fontInfo);
            }
        }
        public Color Rpt_NegDataTextColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_NegDataFont);
                return TextColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_NegDataFont);
                fontInfo = UpdateStringFromTextCol(fontInfo, value);
                SetValue(Setting.rpt_NegDataFont, fontInfo);
            }
        }
        public Color Rpt_NegDataBackColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_NegDataFont);
                return BackColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_NegDataFont);
                fontInfo = UpdateStringFromBackCol(fontInfo, value);
                SetValue(Setting.rpt_NegDataFont, fontInfo);
            }
        }
        #endregion
        #region Danger Data font
        public Color Rpt_DangerDataTextColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_DangerDataFont);
                return TextColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_DangerDataFont);
                fontInfo = UpdateStringFromTextCol(fontInfo, value);
                SetValue(Setting.rpt_DangerDataFont, fontInfo);
            }
        }
        #endregion
        #region Warning Data font
        public Color Rpt_WarningDataTextColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_WarningDataFont);
                return TextColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_WarningDataFont);
                fontInfo = UpdateStringFromTextCol(fontInfo, value);
                SetValue(Setting.rpt_WarningDataFont, fontInfo);
            }
        }
        #endregion
        #region Warning Data font
        public Color Rpt_GoodDataTextColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_GoodDataFont);
                return TextColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_GoodDataFont);
                fontInfo = UpdateStringFromTextCol(fontInfo, value);
                SetValue(Setting.rpt_GoodDataFont, fontInfo);
            }
        }
        #endregion
        #region Section header font
        public Font Rpt_SectionHeaderFont
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_SectionHeaderFont);
                return FontFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_SectionHeaderFont);
                fontInfo = UpdateStringFromFont(fontInfo, value);
                SetValue(Setting.rpt_SectionHeaderFont, fontInfo);
            }
        }
        public Color Rpt_SectionHeaderTextColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_SectionHeaderFont);
                return TextColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_SectionHeaderFont);
                fontInfo = UpdateStringFromTextCol(fontInfo, value);
                SetValue(Setting.rpt_SectionHeaderFont, fontInfo);
            }
        }
        public Color Rpt_SectionHeaderBackColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_SectionHeaderFont);
                return BackColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_SectionHeaderFont);
                fontInfo = UpdateStringFromBackCol(fontInfo, value);
                SetValue(Setting.rpt_SectionHeaderFont, fontInfo);
            }
        }
        #endregion
        #region Title font
        public Font Rpt_TitleFont
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_TitleFont);
                return FontFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_TitleFont);
                fontInfo = UpdateStringFromFont(fontInfo, value);
                SetValue(Setting.rpt_TitleFont, fontInfo);
            }
        }
        public Color Rpt_TitleTextColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_TitleFont);
                return TextColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_TitleFont);
                fontInfo = UpdateStringFromTextCol(fontInfo, value);
                SetValue(Setting.rpt_TitleFont, fontInfo);
            }
        }
        public Color Rpt_TitleBackColour
        {
            get
            {
                string fontInfo = GetValue(Setting.rpt_TitleFont);
                return BackColFromString(fontInfo);
            }
            set
            {
                string fontInfo = GetValue(Setting.rpt_TitleFont);
                fontInfo = UpdateStringFromBackCol(fontInfo, value);
                SetValue(Setting.rpt_TitleFont, fontInfo);
            }
        }
        #endregion
        #region Custom Colours
        public Color Rpt_CustomColourGet(int index)
        {
            Color retVal = Color.Black;
            string customCols = GetValue(Setting.rpt_CustomCols);
            char[] delim = { '|' };
            string[] cols = customCols.Split(delim);
            if (index >= 0 && index < cols.Length)
            {
                char[] delim2 ={ ',' };
                string[] colData = cols[index].Split(delim2);
                retVal = Color.FromArgb(0, int.Parse(colData[0]), int.Parse(colData[1]), int.Parse(colData[2]));
            }
            return retVal;
        }
        public void Rpt_CustomColourSet(int index, Color newCol)
        {
            if (index < 0 || index > 7)
            {
                throw new EMMAException(ExceptionSeverity.Error, "Only eight custom report colours" +
                    "can be defined, the index parameter must be between 0 and 7 inclusive");
            }
            else
            {
                string customCols = GetValue(Setting.rpt_CustomCols);
                char[] delim = { '|' };
                string[] cols = customCols.Split(delim);

                cols[index] = newCol.R + "," + newCol.G + "," + newCol.B;
                StringBuilder retVal = new StringBuilder();
                foreach (string data in cols)
                {
                    if (retVal.Length != 0) { retVal.Append("|"); }
                    retVal.Append(data);
                }
                SetValue(Setting.rpt_CustomCols, retVal.ToString());
            }
        }
        #endregion
        #region Other report settings
        public string Rpt_LogoFile
        {
            get { return GetValue(Setting.rpt_LogoFile); }
            set { SetValue(Setting.rpt_LogoFile, value); }
        }
        public bool Rpt_AltRowInUse
        {
            get { return bool.Parse(GetValue(Setting.rpt_AltRowInUse)); }
            set { SetValue(Setting.rpt_AltRowInUse, value.ToString()); }
        }
        #endregion
        #endregion


        #region Helper methods for font string data access
        private static Font FontFromString(string fontInfo)
        {
            char[] delim = { '|' };
            string[] fontData = fontInfo.Split(delim);
            int style = 0;
            if (bool.Parse(fontData[2])) style += 1;
            if (bool.Parse(fontData[3])) style += 2;
            if (bool.Parse(fontData[4])) style += 4;
            Font retVal = new Font(fontData[0], float.Parse(fontData[1],
                    System.Globalization.CultureInfo.InvariantCulture.NumberFormat), (FontStyle)style);
            return retVal;
        }
        private static Color TextColFromString(string fontInfo)
        {
            char[] delim = { '|' };
            string[] fontData = fontInfo.Split(delim);
            char[] delim2 ={','};
            string[] colData = fontData[5].Split(delim2);
            return Color.FromArgb(int.Parse(colData[0]), int.Parse(colData[1]), int.Parse(colData[2]));
        }
        private static Color BackColFromString(string fontInfo)
        {
            char[] delim = { '|' };
            string[] fontData = fontInfo.Split(delim);
            char[] delim2 ={ ',' };
            string[] colData = fontData[6].Split(delim2);
            return Color.FromArgb(int.Parse(colData[0]), int.Parse(colData[1]), int.Parse(colData[2]));
        }

        private static string UpdateStringFromFont(string fontInfo, Font newFont)
        {
            char[] delim = { '|' };
            string[] fontData = fontInfo.Split(delim);
            fontData[0] = newFont.FontFamily.Name;
            fontData[1] = newFont.SizeInPoints.ToString(System.Globalization.CultureInfo.InvariantCulture);
            fontData[2] = newFont.Bold.ToString();
            fontData[3] = newFont.Italic.ToString();
            fontData[4] = newFont.Underline.ToString();
            StringBuilder retVal = new StringBuilder();
            foreach (string data in fontData)
            {
                if (retVal.Length != 0) { retVal.Append("|"); }
                retVal.Append(data);
            }
            return retVal.ToString();
        }
        private static string UpdateStringFromTextCol(string fontInfo, Color newCol)
        {
            char[] delim = { '|' };
            string[] fontData = fontInfo.Split(delim);
            fontData[5] = newCol.R + "," + newCol.G + "," + newCol.B;
            StringBuilder retVal = new StringBuilder();
            foreach (string data in fontData)
            {
                if (retVal.Length != 0) { retVal.Append("|"); }
                retVal.Append(data);
            }
            return retVal.ToString();
        }
        private static string UpdateStringFromBackCol(string fontInfo, Color newCol)
        {
            char[] delim = { '|' };
            string[] fontData = fontInfo.Split(delim);
            fontData[6] = newCol.R + "," + newCol.G + "," + newCol.B;
            StringBuilder retVal = new StringBuilder();
            foreach (string data in fontData)
            {
                if (retVal.Length != 0) { retVal.Append("|"); }
                retVal.Append(data);
            }
            return retVal.ToString();
        }
        #endregion

        private enum Setting : short
        {
            rpt_LogoFile,
            rpt_RowHeaderFont,
            rpt_ColHeaderFont,
            rpt_DataFont,
            rpt_SectionHeaderFont,
            rpt_AltRowFont,
            rpt_AltRowInUse,
            rpt_NegDataFont,
            rpt_DangerDataFont,
            rpt_WarningDataFont,
            rpt_GoodDataFont,
            rpt_TitleFont,
            rpt_CustomCols,
            autoAddItemsBy,
            autoAddMin,
            collateralBasedOn,
            collateralPercentage,
            autocon_minCollateral,
            autocon_minReward,
            autocon_minVolume,
            autocon_maxCollateral,
            autocon_maxVolume,
            autocon_destinationStation,
            autocon_pickupLocations,
            autocon_allowStackSplitting,
            autocon_excludeContainers,
            rewardBasedOn,
            minReward,
            maxReward,
            minRewardPercentage,
            maxRewardPercentage,
            rewardPercPerJump,
            lowSecPickupBonusPerc,
            volumeBasedRewardPerc,
            useEveCentral,
            eveMarketValueToUse,
            courierCalcOnlyItemsTraded,
            recentStationsList,
            recentItemsList,
            recentSystemsList,
            ordersNotifyEnabled,
            ordersNotifyBuy,
            ordersNotifySell,
            route_highSecWeight,
            route_lowSecWeight,
            route_nullSecWeight,
            reproc_station,
            reproc_stationYield,
            reproc_theyTake,
            reproc_implant,
            reproc_refining,
            reproc_refineryefficiency,
            reproc_arkonorprocessing,
            reproc_bistotprocessing,
            reproc_crokiteprocessing,
            reproc_darkochreprocessing,
            reproc_gneissprocessing,
            reproc_hedbergiteprocessing,
            reproc_hemorphiteprocessing,
            reproc_jaspetprocessing,
            reproc_kerniteprocessing,
            reproc_mercoxitprocessing,
            reproc_omberprocessing,
            reproc_plagioclaseprocessing,
            reproc_pyroxeresprocessing,
            reproc_scorditeprocessing,
            reproc_scrapmetalprocessing,
            reproc_spodumainprocessing,
            reproc_veldsparprocessing,
            reproc_reprocessor,
            useEveMetrics,
            autoAddBuyMin,
            autoAddSellMin,
            autoAddStartDate,
            autoAddBuyStations,
            autoAddSellStations,
            autocon_tradedItems,
            itemValueWebExpiryDays
        }


        public enum EveMarketValueToUse : short
        {
            medianBuy = 0,
            maxBuy = 1
        }

    }

}
