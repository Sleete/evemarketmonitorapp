using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.AbstractionClasses
{
    /// <summary>
    /// The EveAPI class does two main jobs:
    /// 1) Provide methods to retrieve XML from the Eve API and do generic processing on it.
    /// 2) Provide managed access to the APICorp and APICharacter classes.
    /// </summary>
    public static class EveAPI
    {
        public static string URL_EveApiBase = "http://api.eveonline.com";

        //public const string URL_KeyInfoApi = "/account/APIKeyInfo.xml.aspx";
        public const string URL_CharsApi = "/account/APIKeyInfo.xml.aspx";

        public const string URL_TransApi = "/char/WalletTransactions.xml.aspx";
        public const string URL_TransCorpApi = "/corp/WalletTransactions.xml.aspx";
        public const string URL_JournApi = "/char/WalletJournal.xml.aspx";
        public const string URL_JournCorpApi = "/corp/WalletJournal.xml.aspx";
        public const string URL_WalletApi = "/char/AccountBalance.xml.aspx";
        public const string URL_WalletCorpApi = "/corp/AccountBalance.xml.aspx";
        public const string URL_AssetApi = "/char/AssetList.xml.aspx";
        public const string URL_AssetCorpApi = "/corp/AssetList.xml.aspx";
        public const string URL_CharDataApi = "/char/CharacterSheet.xml.aspx";
        public const string URL_CorpDataApi = "/corp/CorporationSheet.xml.aspx";
        public const string URL_CharOrdersApi = "/char/MarketOrders.xml.aspx";
        public const string URL_CorpOrdersApi = "/corp/MarketOrders.xml.aspx";
        public const string URL_CharStandingsApi = "/char/Standings.xml.aspx";
        public const string URL_CorpStandingsApi = "/corp/Standings.xml.aspx";
        public const string URL_IndustryApi = "/char/IndustryJobs.xml.aspx";
        public const string URL_IndustryCorpApi = "/corp/IndustryJobs.xml.aspx";

        public const string URL_JournRefsApi = "/eve/RefTypes.xml.aspx";
        public const string URL_NameApi = "/eve/CharacterName.xml.aspx";
        public const string URL_OutpostListApi = "/eve/ConquerableStationList.xml.aspx";

        private static Dictionary<string, string> URL_Descriptions = new Dictionary<string, string>();

        private static XmlNodeList _journalRefs;
        private static DateTime _lastJournalRefsAccess = DateTime.MinValue;

        private static List<long> _blockedKeys = new List<long>();


        public static void AddBlockedKey(long keyID)
        {
            if (!_blockedKeys.Contains(keyID)) { _blockedKeys.Add(keyID); }
        }


        #region Old stuff...
        /*
        /// <summary>
        /// Get a list of the eve characters available on this account. 
        /// </summary>
        /// <returns>A list containing the name's and char ID's of the characters on the current account. The char ID is the key element</returns>
        public static SortedList GetAccountChars()
        {
            SortedList retVal = new SortedList();

            XmlDocument xml = GetXml(URL_EveApiBase + URL_CharsApi,
                "keyID=" + Settings.UserID + "&vCode=" + Settings.ApiKey);

            XmlNodeList eveChars = GetResults(xml);
            foreach (XmlNode eveChar in eveChars)
            {
                XmlNode eveCharName = eveChar.SelectSingleNode("@name");
                XmlNode eveCharID = eveChar.SelectSingleNode("@characterID");
                retVal.Add(eveCharID.Value, eveCharName.Value);
            }

            return retVal;
        }

        /// <summary>
        /// Get a list of the corps that the eve characters available on this account are members of. 
        /// </summary>
        /// <returns>A list containing the corp name's and char ID's of the characters on the current account. The char ID is the key element</returns>
        public static SortedList GetAccountCorps()
        {
            SortedList retVal = new SortedList();

            XmlDocument xml = GetXml(URL_EveApiBase + URL_CharsApi,
                "keyID=" + Settings.UserID + "&vCode=" + Settings.ApiKey);

            XmlNodeList eveChars = GetResults(xml);
            foreach (XmlNode eveChar in eveChars)
            {
                XmlNode eveCorpName = eveChar.SelectSingleNode("@corporationName");
                XmlNode eveCharID = eveChar.SelectSingleNode("@characterID");
                retVal.Add(eveCharID.Value, eveCorpName.Value);
            }

            return retVal;
        }

        /// <summary>
        /// Verify Char ID is valid by retrieving account character details and trying to find the current char ID.
        /// </summary>
        /// <returns>True if current char id is valid</returns>
        public bool VerifyChar()
        {
            SortedList eveChars = GetAccountChars();
            SortedList eveCorps = GetAccountCorps();

            if (eveChars.ContainsKey(Settings.CharID))
            {
                Settings.CharName = eveChars[Settings.CharID].ToString();
                charData = new APICharacter(Settings.UserID, Settings.ApiKey, Settings.CharID);
                corpData = new APICorp();
            }
            else
            {
                Settings.CharName = "";
            }

            if (eveCorps.ContainsKey(Settings.CharID))
            {
                Settings.CorpName = eveCorps[Settings.CharID].ToString();
                charData = new APICharacter();
                corpData = new APICorp();
            }
            else
            {
                Settings.CorpName = "";
            }

            return !Settings.CharName.Equals("");
        }*/
        #endregion

        public static string GetURL(CharOrCorp corc, APIDataType type)
        {
            string retVal = "";
            switch (type)
            {
                case APIDataType.Transactions:
                    retVal = corc == CharOrCorp.Char ? URL_TransApi : URL_TransCorpApi;
                    break;
                case APIDataType.Journal:
                    retVal = corc == CharOrCorp.Char ? URL_JournApi : URL_JournCorpApi;
                    break;
                case APIDataType.Assets:
                    retVal = corc == CharOrCorp.Char ? URL_AssetApi : URL_AssetCorpApi;
                    break;
                case APIDataType.Orders:
                    retVal = corc == CharOrCorp.Char ? URL_CharOrdersApi : URL_CorpOrdersApi;
                    break;
                case APIDataType.IndustryJobs:
                    retVal = corc == CharOrCorp.Char ? URL_IndustryApi : URL_IndustryCorpApi;
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

        //These methods all make use of Eve EPI functions that do not need parameters for character, corp, etc.
        #region Static API Calls
                /// <summary>
        /// Update player-owned outpost data from the API.
        /// </summary>
        public static void UpdateOutpostData()
        {
            try
            {
                DateTime nextUpdate = Properties.Settings.Default.LastOutpostUpdate.AddDays(1);

                if (DateTime.UtcNow.CompareTo(nextUpdate) >= 0)
                {
                    XmlDocument xml = GetXml(URL_EveApiBase + URL_OutpostListApi, "");
                    XmlNodeList results = GetResults(xml);

                    foreach (XmlNode outpost in results)
                    {
                        XmlNode stationIDNode = outpost.SelectSingleNode("@stationID");
                        XmlNode stationNameNode = outpost.SelectSingleNode("@stationName");
                        XmlNode solarSystemNode = outpost.SelectSingleNode("@solarSystemID");
                        XmlNode corpIDNode = outpost.SelectSingleNode("@corporationID");

                        long stationID = 0, solarSystemID = 0, corpID = 0;
                        string stationName = "";

                        stationID = long.Parse(stationIDNode.Value,
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        stationName = stationNameNode.Value;
                        solarSystemID = long.Parse(solarSystemNode.Value,
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        corpID = long.Parse(corpIDNode.Value,
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

                        Stations.AddStation(stationID, stationName, solarSystemID, corpID);
                    }

                    Properties.Settings.Default.LastOutpostUpdate = DateTime.UtcNow;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                EMMAException emmaex = ex as EMMAException;
                if (emmaex == null)
                {
                    emmaex = new EMMAException(ExceptionSeverity.Warning, "Problem updating outpost data.", ex);
                }
                // Not much we can do about it and it's not critical so just allow things to continue.
            }
        }

        /// <summary>
        /// Get the latest data from the journal references service.
        /// </summary>
        /// <returns>A list containing the reference codes and descriptions. The code is the key element.</returns>
        public static SortedList GetJournalRefs()
        {
            SortedList retVal = new SortedList();

            if (_journalRefs == null || DateTime.UtcNow.AddDays(-1).CompareTo(_lastJournalRefsAccess) < 0)
            {
                XmlDocument xml = GetXml(URL_EveApiBase + URL_JournRefsApi, "");
                _journalRefs = GetResults(xml);
                _lastJournalRefsAccess = DateTime.UtcNow;
            }

            foreach (XmlNode journRef in _journalRefs)
            {
                XmlNode refCode = journRef.SelectSingleNode("@refTypeID");
                XmlNode refDesc = journRef.SelectSingleNode("@refTypeName");
                retVal.Add(refCode.Value, refDesc.Value);
            }

            return retVal;
        }

        /// <summary>
        /// Return the name of a character, corp or alliance.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetName(long id)
        {
            string retVal = "";

            XmlDocument xml = GetXml(URL_EveApiBase + URL_NameApi, "ids=" + id.ToString());
            try
            {
                XmlNodeList nameXml = GetResults(xml);
                if (nameXml.Count > 0)
                {
                    XmlNode resultNode = nameXml[0];
                    XmlNode nameNode = resultNode.SelectSingleNode("@name");
                    retVal = nameNode.Value;
                }
            }
            catch (EMMAEveAPIException) { }

            return retVal;
        }
        #endregion

        #region XML Retrieval methods
        /// <summary>
        /// Get the type of data contained within the specified EVE API xml
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static APIDataType GetFileType(XmlDocument xml)
        {
            APIDataType retVal;

            if (xml != null)
            {
                XmlNode errorNode = xml.SelectSingleNode("/eveapi/error");
                if (errorNode != null)
                {
                    string file = string.Format("{0}Logging{1}APIError.xml",
                        Globals.AppDataDir, Path.DirectorySeparatorChar);
                    xml.Save(file);

                    XmlNode errCodeNode = errorNode.SelectSingleNode("@code");
                    XmlNode errTextNode = errorNode.FirstChild;


                    throw new EMMAEveAPIException(ExceptionSeverity.Error,
                        (errCodeNode == null ? 0 : int.Parse(errCodeNode.Value)), errTextNode.Value);
                }

                XmlNode rowset = xml.SelectSingleNode("/eveapi/result/rowset");
                XmlNode nameAttribute = rowset.Attributes.GetNamedItem("name");
                string name = nameAttribute.Value;

                if (name.Equals("entries"))
                {
                    retVal = APIDataType.Journal;
                }
                else if (name.Equals("assets"))
                {
                    retVal = APIDataType.Assets;
                }
                else if (name.Equals("transactions"))
                {
                    retVal = APIDataType.Transactions;
                }
                else if (name.Equals("orders"))
                {
                    retVal = APIDataType.Orders;
                }
                else if (name.Equals("jobs"))
                {
                    retVal = APIDataType.IndustryJobs;
                }
                else
                {
                    retVal = APIDataType.Unknown;
                }
            }
            else
            {
                throw new EMMAEveAPIException(ExceptionSeverity.Critical, "No XML document to process");
            }

            return retVal;
        }

        /// <summary>
        /// Get the results from the supplied xml document. This should be unmodified XML returned from a call to
        /// any of the Eve API web services.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        /// <exception cref="EMMAEveAPIException">If the xml document contains Eve API error information then 
        /// an exception is thrown containing the error code and description.
        /// </exception>
        public static XmlNodeList GetResults(XmlDocument xml, bool accessType=false)
        {
            XmlNodeList retVal = null;

            if (xml != null)
            {
                XmlNode errorNode = xml.SelectSingleNode("/eveapi/error");
                if (errorNode != null)
                {
                    string file = string.Format("{0}Logging{1}APIError.xml",
                        Globals.AppDataDir, Path.DirectorySeparatorChar);
                    lock (Globals.APIErrorFileLock)
                    {
                        xml.Save(file);
                    }

                    XmlNode errCodeNode = errorNode.SelectSingleNode("@code");
                    XmlNode errTextNode = errorNode.FirstChild;

                    throw new EMMAEveAPIException(ExceptionSeverity.Error, 
                        (errCodeNode == null ? 0 : int.Parse(errCodeNode.Value)), errTextNode.Value);
                }
                retVal = (accessType == true) ? xml.SelectNodes("/eveapi/result/key") : xml.SelectNodes("/eveapi/result/rowset/row");
                //retVal = xml.SelectNodes("/eveapi/result/key");
                //retVal = xml.SelectNodes("/eveapi/result/key/rowset/row");
            }
            else
            {
                throw new EMMAEveAPIException(ExceptionSeverity.Critical, "No XML document to process");
            }

            return retVal;
        }

        public static DateTime GetCachedUntilTime(XmlDocument xml)
        {
            DateTime retVal = new DateTime(2000, 1, 1);

            if (xml != null)
            {
                XmlNode timeNode = xml.SelectSingleNode("/eveapi/cachedUntil");
                if (timeNode != null)
                {
                    // Get the date/time that the assets data is cached until and then subtract
                    // 23 hours to get the date/time that the snapshot was actually taken.
                    retVal = DateTime.Parse(timeNode.InnerText,
                        System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
                    // C# will asume that the value from the file is for the local time zone.
                    // in fact it is UTC so we need to specify this.
                    retVal = TimeZoneInfo.ConvertTime(retVal, TimeZoneInfo.Utc, TimeZoneInfo.Utc);
                }
            }

            return retVal;
        }

        public static DateTime GetDataTime(XmlDocument xml)
        {
            DateTime retVal = new DateTime(2000, 1, 1);

            if (xml != null)
            {
                XmlNode timeNode = xml.SelectSingleNode("/eveapi/currentTime");
                if (timeNode != null)
                {
                    // Get the date/time that the assets data is cached until and then subtract
                    // 23 hours to get the date/time that the snapshot was actually taken.
                    retVal = DateTime.Parse(timeNode.InnerText,
                        System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
                    // C# will asume that the value from the file is for the local time zone.
                    // in fact it is UTC so we need to specify this.
                    retVal = TimeZoneInfo.ConvertTime(retVal, TimeZoneInfo.Utc, TimeZoneInfo.Utc);
                }
            }

            return retVal;
        }

        ///// <summary>
        ///// This proceedure provides a way to determine the time that the snapshot of assets data 
        ///// in the supplied xml document was taken.
        ///// </summary>
        ///// <param name="xml"></param>
        ///// <returns></returns>
        //public static DateTime GetAssetDataTime(XmlDocument xml)
        //{
        //    DateTime retVal = new DateTime(2000, 1, 1);

        //    if (xml != null)
        //    {
        //        XmlNode timeNode = xml.SelectSingleNode("/eveapi/cachedUntil");
        //        if (timeNode != null)
        //        {
        //            // Get the date/time that the assets data is cached until and then subtract
        //            // 23 hours to get the date/time that the snapshot was actually taken.
        //            retVal = DateTime.Parse(timeNode.InnerText, 
        //                System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        //            // C# will asume that the value from the file is for the local time zone.
        //            // in fact it is UTC so we need to specify this.
        //            retVal = TimeZoneInfo.ConvertTime(retVal, TimeZoneInfo.Utc, TimeZoneInfo.Utc);
        //            // The time from the file is the 'cached until' time. Take off 23 hours to get the
        //            // time that the data in the file was generated
        //            retVal = retVal.AddHours(-23);
        //        }
        //    }

        //    return retVal;
        //}

        ///// <summary>
        ///// This proceedure provides a way to determine the time that the data in a journal file was
        ///// cached
        ///// </summary>
        ///// <param name="xml"></param>
        ///// <returns></returns>
        //public static DateTime GetJournalDataTime(XmlDocument xml)
        //{
        //    DateTime retVal = new DateTime(2000, 1, 1);

        //    if (xml != null)
        //    {
        //        XmlNode timeNode = xml.SelectSingleNode("/eveapi/cachedUntil");
        //        if (timeNode != null)
        //        {
        //            // Get the date/time that the assets data is cached until and then subtract
        //            // 23 hours to get the date/time that the snapshot was actually taken.
        //            retVal = DateTime.Parse(timeNode.InnerText, 
        //                System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
        //            // C# will asume that the value from the file is for the local time zone.
        //            // in fact it is UTC so we need to specify this.
        //            retVal = TimeZoneInfo.ConvertTime(retVal, TimeZoneInfo.Utc, TimeZoneInfo.Utc);
        //            // The time from the file is teh 'cached until' time. Take off one hour to get the
        //            // time that the data in the file was generated
        //            retVal = retVal.AddHours(-1);
        //        }
        //    }

        //    return retVal;
        //}

        public static XmlDocument GetXml(string url, string parameters)
        {
            string xmlFile = "";
            return GetXml(url, parameters, ref xmlFile);
        }
        
        /// <summary>
        /// Method to retrieve XML from the specified address using the specified parameters.
        /// </summary>
        /// <param name="url">The web address of the API service to POST to</param>
        /// <param name="parameters">The HTTP POST parameters</param>
        /// <returns></returns>
        public static XmlDocument GetXml(string url, string parameters, ref string xmlLogFile) 
        {
            HttpWebRequest request;
            HttpWebResponse response = null;
            XmlDocument xml = null;
            byte[] data;

            lock (URL_Descriptions)
            {
                // Use lock to make sure we don't have multiple thread trying to setup the
                // dictionary at the same time.
                if (URL_Descriptions.Count == 0) { SetupURLDescriptions(); }
            }


            try
            {
                int keyIDloc = parameters.ToUpper().IndexOf("KEYID=") + 6;

                if (keyIDloc >= 0)
                {
                    int endIndex = parameters.IndexOf("&", keyIDloc);
                    string keyIDStr = "";
                    if (endIndex == 0)
                    {
                        keyIDStr = parameters.Substring(keyIDloc);
                    }
                    else
                    {
                        keyIDStr = parameters.Substring(keyIDloc, endIndex - keyIDloc);
                    }
                    long keyID = long.Parse(keyIDStr);
                    if (_blockedKeys.Contains(keyID))
                    {
                        throw new EMMAEveAPIException(ExceptionSeverity.Error, 999,
                            "This API Key has previously caused an authentication error so will be blocked from making requests until EMMA is restarted");
                    }
                }
            }
            catch (Exception ex)
            {
                EMMAEveAPIException apiEx = ex as EMMAEveAPIException;
                if (apiEx != null) { throw; }
            }

            if (!Globals.EveAPIDown)
            {
                request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                ASCIIEncoding enc = new ASCIIEncoding();
                data = enc.GetBytes(parameters);

                try
                {

                    Stream reqStream = request.GetRequestStream();
                    try
                    {
                        reqStream.Write(data, 0, data.Length);
                    }
                    finally
                    {
                        reqStream.Close();
                    }

                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException webEx)
                {
                    throw new EMMAEveAPIException(ExceptionSeverity.Error,
                        "Problem retrieving data from Eve web service", webEx);
                }

                if (response != null)
                {
                    Stream respStream = response.GetResponseStream();

                    if (respStream != null)
                    {
                        try
                        {
                            xml = new XmlDocument();
                            xml.Load(respStream);

                            // Convert the XML to UTF-16. SQL server does not like UTF-8...
                            XmlDocument newDoc = new XmlDocument();
                            XmlDeclaration xmldecl;
                            xmldecl = newDoc.CreateXmlDeclaration("1.0", null, null);
                            xmldecl.Encoding = "UTF-16";
                            xmldecl.Standalone = "yes";
                            newDoc.LoadXml(xml.DocumentElement.OuterXml.ToString());
                            XmlElement root = newDoc.DocumentElement;
                            newDoc.InsertBefore(xmldecl, root);
                            xml = newDoc;

                            // Try to save a copy of the XML so that it can be used for troubleshooting if required.
                            // If it goes wrong then no big deal, just leave it.
                            try
                            {
                                string desc = "API Data";
                                Dictionary<string, string>.Enumerator enumerator = URL_Descriptions.GetEnumerator();
                                while (enumerator.MoveNext())
                                {
                                    if (url.EndsWith(enumerator.Current.Key))
                                    {
                                        desc = enumerator.Current.Value;
                                        string filenameparams = parameters.Replace('&', ' ');
                                        //int userIDloc = filenameparams.ToUpper().IndexOf("KEYID=");
                                        //if (userIDloc >= 0)
                                        //{
                                        //    int length = filenameparams.IndexOf(' ', userIDloc) - userIDloc + 1;
                                        //    if (length <= 0) length = filenameparams.Length - userIDloc;
                                        //    filenameparams = filenameparams.Remove(
                                        //        userIDloc, length);
                                        //}
                                        
                                        //int apiKeyloc = filenameparams.ToUpper().IndexOf("APIKEY=");
                                        int apiKeyloc = filenameparams.ToUpper().IndexOf("VCODE=");
                                        if (apiKeyloc >= 0)
                                        {
                                            int length = filenameparams.IndexOf(' ', apiKeyloc) - apiKeyloc + 1;
                                            if (length <= 0) length = filenameparams.Length - apiKeyloc;
                                            filenameparams = filenameparams.Remove(
                                                apiKeyloc, length);
                                        }
                                        
                                        int versionloc = filenameparams.ToUpper().IndexOf("VERSION=");
                                        if (versionloc >= 0)
                                        {
                                            int length = filenameparams.IndexOf(' ', versionloc) - versionloc + 1;
                                            if (length <= 0) length = filenameparams.Length - versionloc;
                                            filenameparams = filenameparams.Remove(
                                                versionloc, length);
                                        }
                                        desc = desc + " " + filenameparams;
                                    }
                                }
                                xmlLogFile = string.Format("{0}Logging{1}API Call History{1}{2} {3}.xml",
                                    Globals.AppDataDir, Path.DirectorySeparatorChar,
                                    desc, DateTime.UtcNow.Ticks.ToString());
                                xml.Save(xmlLogFile);
                            }
                            catch { }
                            // Also delete any historical API data older than 48 hours.
                            try
                            {
                                string[] files = Directory.GetFiles(string.Format("{0}Logging{1}API Call History",
                                    Globals.AppDataDir, Path.DirectorySeparatorChar));
                                foreach (string file in files)
                                {
                                    try
                                    {
                                        DateTime createDT = File.GetCreationTime(file);
                                        if (createDT.CompareTo(DateTime.Now.AddDays(-2)) < 0)
                                        {
                                            File.Delete(file);
                                        }
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                            return xml;
                        }
                        catch (Exception ex)
                        {
                            throw new EMMAEveAPIException(ExceptionSeverity.Error,
                                "Problem recovering XML message from Eve response stream", ex);
                        }
                        finally
                        {
                            respStream.Close();
                        }
                    }
                }

            }

            return null;
        }

        private static void SetupURLDescriptions()
        {
            URL_Descriptions = new Dictionary<string, string>();

            URL_Descriptions.Add(URL_TransApi, "Character transactions");
            URL_Descriptions.Add(URL_JournApi, "Character journal entries");
            URL_Descriptions.Add(URL_TransCorpApi, "Corp transactions");
            URL_Descriptions.Add(URL_JournCorpApi, "Corp journal entries");
            URL_Descriptions.Add(URL_CharsApi, "Characters on an account");
            URL_Descriptions.Add(URL_JournRefsApi, "Journal reference table");
            URL_Descriptions.Add(URL_WalletApi, "Character wallet data");
            URL_Descriptions.Add(URL_WalletCorpApi, "Corp wallet data");
            URL_Descriptions.Add(URL_AssetApi, "Character assets");
            URL_Descriptions.Add(URL_AssetCorpApi, "Corp assets");
            URL_Descriptions.Add(URL_CorpDataApi, "Corp general data");
            URL_Descriptions.Add(URL_CharDataApi, "Character general data");
            URL_Descriptions.Add(URL_CharOrdersApi, "Character market orders");
            URL_Descriptions.Add(URL_CorpOrdersApi, "Corp market orders");
            URL_Descriptions.Add(URL_NameApi, "Entity name");
            URL_Descriptions.Add(URL_IndustryApi, "Character industry jobs");
            URL_Descriptions.Add(URL_IndustryCorpApi, "Corp industry jobs");
        }
        #endregion
    }


}
