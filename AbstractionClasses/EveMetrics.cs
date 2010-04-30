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
    /// Uses Eve-Metrics.com to retrieve price data for market based items.
    /// </summary>
    public static class EveMetrics
    {
        //private const string developmentApiKey = "75AF4D882523F9D766A6C";
        private const string developmentApiKey = "E8B06707A4BDC5A588F6F";

        private const string URL_PriceStats = @"http://www.eve-metrics.com/api/item.xml";

        public static decimal GetPrice(int itemID, int regionID, bool buyPrice)
        {
            decimal retVal = 0;
            StringBuilder parameters = new StringBuilder();
            parameters.Append("type_ids=").Append(itemID);
            if (regionID != -1)
            {
                parameters.Append("&").Append("region_ids=").Append(regionID);
            }
            parameters.Append("&key=").Append(developmentApiKey);

            // Create and process the request, retrieve an xml response
            XmlDocument xml = GetXml(URL_PriceStats, parameters.ToString());
            if (xml != null)
            {
                XmlNode node = null;
                ReportGroupSettings.EveMarketValueToUse valueToUse = UserAccount.CurrentGroup.Settings.EveMarketType;
                if (buyPrice)
                {
                    switch (valueToUse)
                    {
                        case ReportGroupSettings.EveMarketValueToUse.medianBuy:
                            node = xml.SelectSingleNode("/evemetrics/type/@buy_avg");
                            break;
                        case ReportGroupSettings.EveMarketValueToUse.maxBuy:
                            node = xml.SelectSingleNode("/evemetrics/type/@buy_high");
                            break;
                        default:
                            node = xml.SelectSingleNode("/evemetrics/type/@buy_avg");
                            break;
                    }
                }
                else
                {
                    node = xml.SelectSingleNode("/evemetrics/type/@sell_low");
                }
                if (node != null)
                {
                    try
                    {
                        retVal = decimal.Parse(node.Value, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
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

            if (!Globals.EveMetricsDown)
            {
                System.Net.ServicePointManager.Expect100Continue = false;

                request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "EMMA";
                request.Method = "POST";
                ASCIIEncoding enc = new ASCIIEncoding();
                data = enc.GetBytes(parameters);

                // Try to write our request and get a response
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
                        "Problem retrieving data from the Eve-metrics web service", webEx);
                }

                // If we received a response, lets load it into a DOM object
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
                            return xml;
                        }
                        catch (Exception ex)
                        {
                            throw new EMMAEveAPIException(ExceptionSeverity.Error,
                                "Problem recovering XML message from Eve-metrics response stream", ex);
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
