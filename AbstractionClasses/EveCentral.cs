using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.DatabaseClasses;

namespace EveMarketMonitorApp.AbstractionClasses
{
    /// <summary>
    /// Uses Eve-central.com to retrieve price data for market based items.
    /// </summary>
    public static class EveCentral
    {
        public const string URL_PriceStats = @"http://eve-central.com/api/marketstat";

        public static decimal GetPrice(int itemID, long regionID, bool buyPrice)
        {
            decimal retVal = 0;
            XmlDocument xml = GetXml(URL_PriceStats, "hours=144&typeid=" + 
                itemID.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) +
                (regionID == -1 ? "" : "&regionlimit=" + 
                regionID.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)));

            if (xml != null)
            {
                XmlNode node = null;
                ReportGroupSettings.EveMarketValueToUse valueToUse = UserAccount.CurrentGroup.Settings.EveMarketType;
                if (buyPrice)
                {
                    switch (valueToUse)
                    {
                        case ReportGroupSettings.EveMarketValueToUse.medianBuy:
                            node = xml.SelectSingleNode("/evec_api/marketstat/type/buy/median");
                            break;
                        case ReportGroupSettings.EveMarketValueToUse.maxBuy:
                            node = xml.SelectSingleNode("/evec_api/marketstat/type/buy/max");
                            break;
                        default:
                            node = xml.SelectSingleNode("/evec_api/marketstat/type/buy/median");
                            break;
                    }
                }
                else
                {
                    node = xml.SelectSingleNode("/evec_api/marketstat/type/sell/min");
                }
                if (node != null)
                {
                    try
                    {
                        retVal = decimal.Parse(node.FirstChild.Value, 
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    }
                    catch { }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Method to retrieve XML from the specified address using the specified parameters.
        /// NOTE: This method was copied from EveAPI.GetXML()
        /// </summary>
        /// <param name="url">The web address of the API service to POST to</param>
        /// <param name="parameters">The HTTP POST parameters</param>
        /// <returns></returns>
        private static XmlDocument GetXml(string url, string parameters)
        {
            HttpWebRequest request;
            HttpWebResponse response = null;
            XmlDocument xml = null;
            byte[] data;

            if (!Globals.EveCentralDown)
            {
                System.Net.ServicePointManager.Expect100Continue = false;

                request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "EMMA 1.0";
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
                        "Problem retrieving data from the Eve-central web service", webEx);
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
                                string desc = "Eve Central Price Stats";

                                int itemLoc = parameters.ToUpper().IndexOf("TYPEID=");
                                if (itemLoc >= 0)
                                {
                                    itemLoc += 7;
                                    int endPos = parameters.IndexOf('&', itemLoc);
                                    if (endPos <= 0) endPos = parameters.Length;
                                    desc = desc + " Item=" + parameters.Substring(itemLoc, endPos - itemLoc);
                                }
                                int regionLoc = parameters.ToUpper().IndexOf("REGIONLIMIT=");
                                if (regionLoc >= 0)
                                {
                                    regionLoc += 12;
                                    int endPos = parameters.IndexOf('&', regionLoc);
                                    if (endPos <= 0) endPos = parameters.Length;
                                    desc = desc + " Region=" + parameters.Substring(regionLoc, endPos - regionLoc);
                                }

                                string xmlLogFile = string.Format("{0}Logging{1}Eve Central History{1}{2} {3}.xml",
                                    Globals.AppDataDir, Path.DirectorySeparatorChar,
                                    desc, DateTime.UtcNow.Ticks.ToString());
                                xml.Save(xmlLogFile);
                            }
                            catch { }
                            // Also delete any historical data older than 48 hours.
                            try
                            {
                                string[] files = Directory.GetFiles(string.Format("{0}Logging{1}Eve Central History",
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
                                "Problem recovering XML message from Eve-central response stream", ex);
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
    }
}
