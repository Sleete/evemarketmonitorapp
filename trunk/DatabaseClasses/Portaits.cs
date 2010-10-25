using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Net;
using System.IO;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public static class Portaits
    {
        private static EMMADataSetTableAdapters.PortraitsTableAdapter portraitsTableAdapter = 
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.PortraitsTableAdapter();
        private static Cache<long, Image> _cache = new Cache<long, Image>(50);
        private static bool _initalised = false;


        private static void Initialise()
        {
            if (!_initalised)
            {
                _cache.DataUpdateNeeded += new Cache<long, Image>.DataUpdateNeededHandler(Cache_DataUpdateNeeded);
                _initalised = true;
            }
        }

        /// <summary>
        /// Get the Eve character portrait for the specified character ID
        /// </summary>
        /// <param name="charID"></param>
        /// <returns></returns>
        public static Image GetPortrait(long charID)
        {
            if (!_initalised) { Initialise(); }
            Image retVal = _cache.Get(charID);
            return retVal;
        }

        /// <summary>
        /// Fired when the cache needs and image that it does not currently hold.
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="args"></param>
        static void Cache_DataUpdateNeeded(object myObject, DataUpdateNeededArgs<long, Image> args)
        {
            EMMADataSet.PortraitsRow rowData = LoadPortraitFromDB(args.Key);
            if (rowData != null)
            {
                MemoryStream stream = new MemoryStream(rowData.portrait);
                Image pic = Image.FromStream(stream);
                args.Data = pic;
            }
            else
            {
                // If there is nothing in the database then try and get the image from the Eve API
                Image pic = GetImageFromAPI(args.Key);
                StorePortrait(args.Key, pic);
                args.Data = pic;
            }
        }

        /// <summary>
        /// Return the specified protrait row direct from the EMMA database
        /// </summary>
        /// <returns></returns>
        private static EMMADataSet.PortraitsRow LoadPortraitFromDB(long charID)
        {
            EMMADataSet.PortraitsRow retVal = null;
            EMMADataSet.PortraitsDataTable portraitData = new EMMADataSet.PortraitsDataTable();

            portraitsTableAdapter.ClearBeforeFill = true;
            portraitsTableAdapter.FillByID(portraitData, charID);
            if (portraitData != null)
            {
                if (portraitData.Count == 1)
                {
                    retVal = portraitData[0];
                }
            }
            return retVal;
        }

        /// <summary>
        /// Store the specified image against the specified char ID in the database.
        /// NOTE: This method assumes that we need to create a new row in the database...
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="portrait"></param>
        private static void StorePortrait(long charID, Image portrait) 
        {
            EMMADataSet.PortraitsDataTable table = new EMMADataSet.PortraitsDataTable();
            EMMADataSet.PortraitsRow data = null;

            data = table.NewPortraitsRow();
            data.charID = charID;
            MemoryStream stream = new MemoryStream();
            portrait.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] imageData = stream.ToArray();
            data.portrait = imageData;
            table.AddPortraitsRow(data);

            portraitsTableAdapter.Update(table);
        }

        /// <summary>
        /// Return the character portrait for the specified character ID from the Eve portrait server.
        /// </summary>
        /// <param name="charID"></param>
        /// <returns></returns>
        private static Image GetImageFromAPI(long charID)
        {
            HttpWebRequest request;
            HttpWebResponse response = null;
            Image retVal = null;
            byte[] data;

            request = (HttpWebRequest)HttpWebRequest.Create(@"http://img.eve.is/serv.asp" );
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            ASCIIEncoding enc = new ASCIIEncoding();
            data = enc.GetBytes("s=256&c=" + charID);

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
                    "Problem retrieving data from character portrait web service", webEx);
            }

            if (response != null)
            {
                Stream respStream = response.GetResponseStream();

                if (respStream != null)
                {
                    try
                    {
                        retVal = Image.FromStream(respStream);
                        return retVal;
                    }
                    catch (Exception ex)
                    {
                        throw new EMMAEveAPIException(ExceptionSeverity.Error,
                            "Problem recovering XML message from character portrait response stream", ex);
                    }
                    finally
                    {
                        respStream.Close();
                    }
                }
            }

            return null;
        }
    }
}
