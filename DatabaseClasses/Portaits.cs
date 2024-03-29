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
            bool success = false;
            Image pic = GetImageFromAPI(args.Key, out success);

            if (success)
            {
                StorePortrait(args.Key, pic);
                args.Data = pic;
            }
            else
            {
                EMMADataSet.PortraitsRow rowData = LoadPortraitFromDB(args.Key);
                //EMMADataSet.PortraitsRow rowData = null;
                if (rowData != null)
                {
                    MemoryStream stream = new MemoryStream(rowData.portrait);
                    pic = Image.FromStream(stream);
                    args.Data = pic;
                }
                else
                {
                    // Just use the placeholder generated by the failed call to 'GetImageFromAPI'
                    args.Data = pic;
                }
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
        /// </summary>
        /// <param name="charID"></param>
        /// <param name="portrait"></param>
        private static void StorePortrait(long charID, Image portrait) 
        {
            EMMADataSet.PortraitsDataTable table = new EMMADataSet.PortraitsDataTable();
            EMMADataSet.PortraitsRow data = null;
            bool newRow = false;

            portraitsTableAdapter.ClearBeforeFill = true;
            portraitsTableAdapter.FillByID(table, charID);
            if (table != null && table.Rows.Count > 0)
            {
                data = table[0];
            }
            else
            {
                newRow = true;
                table = new EMMADataSet.PortraitsDataTable();
                data = table.NewPortraitsRow();
                data.charID = charID;
            }
            MemoryStream stream = new MemoryStream();
            portrait.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] imageData = stream.ToArray();
            data.portrait = imageData;
            if (newRow) { table.AddPortraitsRow(data); }

            portraitsTableAdapter.Update(table);
        }

        /// <summary>
        /// Return the character portrait for the specified character ID from the Eve portrait server.
        /// </summary>
        /// <param name="charID"></param>
        /// <returns></returns>
        private static Image GetImageFromAPI(long charID, out bool success)
        {
            HttpWebRequest request;
            WebResponse response;
            Image retVal = null;
            success = false;

            request = (HttpWebRequest)HttpWebRequest.Create(@"http://image.eveonline.com/Character/" + charID + "_256.jpg");
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "GET";

            try
            {
                response = request.GetResponse();
            }
            catch (WebException)
            {
                //throw new EMMAEveAPIException(ExceptionSeverity.Error,
                //    "Problem retrieving data from character portrait web service", webEx);
                response = null;
                retVal = null;
            }

            if (response != null)
            {
                Stream respStream = response.GetResponseStream();

                if (respStream != null)
                {
                    try
                    {
                        retVal = Image.FromStream(respStream);
                    }
                    catch (Exception)
                    {
                        //throw new EMMAEveAPIException(ExceptionSeverity.Error,
                        //    "Problem recovering image from character portrait response stream", ex);
                        retVal = null;
                    }
                    finally
                    {
                        response.Close();
                    }
                }
            }

            if (retVal == null)
            {
                // If we could not retrieve an image then use a placeholder..
                Bitmap img = new Bitmap(256, 256);
                retVal = (Image)img;
                Graphics g = Graphics.FromImage(retVal);
                g.Clear(Color.Black);

                Font font = new Font("Arial", 18);
                string text = Names.GetName(charID);
                SizeF charNameTextSize = g.MeasureString(text, font);

                g.DrawString(text, font, new SolidBrush(Color.White), 
                    (retVal.Width / 2) - (charNameTextSize.Width / 2), 
                    (retVal.Height / 2) - (charNameTextSize.Height / 2));
            }
            else 
            {
                success = true;
            }

            return retVal;
        }
    }
}
