using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

using EveMarketMonitorApp.DatabaseClasses;
using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.AbstractionClasses
{
    public class EVEAccount
    {
        private long _userID;
        private string _apiKey;
        private XmlDocument _charList;
        private DateTime _lastcharListUpdate = DateTime.MinValue;
        private List<APICharacter> _chars;

        #region Properties
        public long UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        public string ApiKey
        {
            get { return _apiKey; }
            set { _apiKey = value; }
        }

        public XmlDocument CharList
        {
            get { return _charList; }
            set { _charList = value; }
        }

        public DateTime LastcharListUpdate
        {
            get { return _lastcharListUpdate; }
            set { _lastcharListUpdate = value; }
        }

        private CharOrCorp _type;
        public CharOrCorp Type
        {
            get
            {
                CharOrCorp retVal = CharOrCorp.Char;
                XmlNode keyNode = CharList.SelectSingleNode("/eveapi/result/key");
                if (keyNode != null)
                {
                    XmlNode typeNode = keyNode.SelectSingleNode("@type");
                    if (typeNode != null)
                    {
                        retVal = typeNode.Value.ToString() == "Character" || typeNode.Value.ToString() == "Account" ? CharOrCorp.Char : CharOrCorp.Corp;
                    }
                }
                return retVal;
            }
        }
      
        public List<APICharacter> Chars
        {
            get
            {
                if (_chars == null || _chars.Count == 0)
                {
                    try
                    {
                        UpdateCharList(false);
                    }
                    catch (EMMAEveAPIException) { }
                    PopulateChars();
                }
                return _chars; 
            }
            set { _chars = value; }
        }
        #endregion

        public EVEAccount(EMMADataSet.EveAccountsRow data)
        {
            _userID = data.UserID;
            _apiKey = data.APIKey.Trim();
            _lastcharListUpdate = data.LastCharListUpdate;
            if (_charList == null)
            {
                _charList = new XmlDocument();
            }
            _charList.LoadXml(data.CharList);
        }

        public EVEAccount(int userID, string apiKey) 
        {
            _userID = userID;
            _apiKey = apiKey;
            _lastcharListUpdate = DateTime.MinValue;
            _charList = new XmlDocument();
            _chars = new List<APICharacter>();
        }

        /// <summary>
        /// Get an APICharacter object containing cached data and methods to retrieve more data
        /// </summary>
        /// <param name="charID"></param>
        /// <returns></returns>
        public APICharacter GetCharcter(int charID)
        {
            APICharacter retVal = null;
            List<APICharacter> chars = this.Chars;

            foreach (APICharacter apiChar in chars)
            {
                if (apiChar.CharID == charID)
                {
                    retVal = apiChar;
                }
            }
            return retVal;
        }
        
        /// <summary>
        /// Update the local copy of the character XML document with the latest from the Eve API.
        /// </summary>
        /// <returns></returns>
        public void UpdateCharList(bool forceUpdate)
        {
            //if (DateTime.UtcNow.AddHours(-48).CompareTo(_lastcharListUpdate) > 0 || forceUpdate)
            //{
                XmlDocument xml = EveAPI.GetXml(EveAPI.URL_EveApiBase + EveAPI.URL_CharsApi,
                    "keyID=" + _userID + "&vCode=" + _apiKey);
                _lastcharListUpdate = DateTime.UtcNow;

                if (!_charList.Equals(xml))
                {
                    try
                    {
                        // If there is some problem with the API or XML then this method will throw an exception.
                        // This will prevent _charList from being set as we don't want to put the wrong thing in it.
                        XmlNodeList results = EveAPI.GetResults(xml);

                        _charList = xml;
                        // Clear the current collection of character objects
                        _chars = null;
                    }
                    catch (EMMAEveAPIException apiEx)
                    {
                        if (apiEx.EveCode == 203 || apiEx.EveCode == 222)
                        {
                            EveAPI.AddBlockedKey(_userID);
                        }
                        throw;
                    }
                }
            //}
        }

        /// <summary>
        /// Populate the List of APICharacters with entries based upon the data in the _charList
        /// XML document.
        /// </summary>
        public void PopulateChars()
        {
            if (_charList != null)
            {
                _chars = new List<APICharacter>();

                try
                {
                    CharOrCorp accessType;
                    XmlNodeList results = EveAPI.GetResults(_charList,true);

                    foreach (XmlNode node in results)
                    {
                        accessType = node.SelectSingleNode("@type").Value.ToString() == "Character" ||
                            node.SelectSingleNode("@type").Value.ToString() == "Account" ? CharOrCorp.Char : CharOrCorp.Corp;

                        foreach (XmlNode node2 in node.SelectNodes("rowset/row"))
                        {
                            APICharacter apiChar = APICharacters.GetCharacter(_userID, _apiKey,
                                int.Parse(node2.SelectSingleNode("@characterID").Value.ToString()));
                            if (apiChar == null)
                            {
                                // Need to create a new API char in the database.
                                apiChar = new APICharacter(_userID, _apiKey, accessType,
                                    long.Parse(node2.SelectSingleNode("@characterID").Value.ToString()));
                                APICharacters.Store(apiChar);
                            }
                            apiChar.AccessType = accessType;
                            _chars.Add(apiChar);
                        }
                    }
                }
                catch (EMMAEveAPIException) { }
            }
        }

       
        /// <summary>
        /// Verify User ID and API Key settings by attempting to retrieve account character data.
        /// </summary>
        /// <returns>True if current user id and api key vaules are valid</returns>
        public bool VerifyAccount()
        {
            bool retVal = false;

            try
            {
                this.UpdateCharList(true);
                retVal = true;
            }
            catch (EMMAEveAPIException emmaApiEx)
            {
                // If this is anything other than an authentication failure then just re-throw it.
                // Otherwise, we want to return false to indicate invalid settings so just drop out the 
                // bottom of this section.
                if (emmaApiEx.EveCode != 203)
                {
                    throw emmaApiEx;
                }
            }

            return retVal;
        }


        public override string ToString()
        {
            StringBuilder retVal = new StringBuilder("");
            retVal.Append(_userID);
            return retVal.ToString();
        }
    }
}
