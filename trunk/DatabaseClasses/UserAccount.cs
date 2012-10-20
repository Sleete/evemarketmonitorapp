using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Xml;

using EveMarketMonitorApp.Common;
using EveMarketMonitorApp.AbstractionClasses;

namespace EveMarketMonitorApp.DatabaseClasses
{
    /// <summary>
    /// User account is the top object in the tree of users/accounts/characters.
    /// It is accessed by a user name and a password (if required).
    /// The user account can link to multiple report group objects, each of which can contain
    /// groups of characters and/or corporations, both stored in the API Character object.
    /// 
    /// User Account - User Settings
    ///      |
    ///     /|\
    /// Report Group - Report Group Settings
    ///      |
    ///     /|\
    /// Eve Account
    ///      |
    ///     /|\
    /// API Character - API Char Settings
    /// </summary>
    static class UserAccount
    {
        private static EMMADataSet.UserAccountsDataTable userData = new EMMADataSet.UserAccountsDataTable();
        private static EMMADataSetTableAdapters.UserAccountsTableAdapter userAccountsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.UserAccountsTableAdapter();
        private static EMMADataSetTableAdapters.UserSettingsTableAdapter userSettingsTableAdapter =
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.UserSettingsTableAdapter();

        private static List<ReportGroup> _reportGroups = null;
        private static string _name = "";
        private static ReportGroup _currentGroup = null;
        private static UserSettings _settings = null;

        /// <summary>
        /// Creates a new user account in the database.
        /// This new account becomes the currently active account.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="passwordProtected"></param>
        public static void CreateNewAccount(string name, string password)
        {
            LoadAccountFromDB(name);
            EMMADataSet.UserAccountsRow account = userData.FindByName(name);

            if (account == null)
            {
                EMMADataSet.UserAccountsRow newUser = userData.NewUserAccountsRow();
                newUser.Name = name;

                Password pwd = new Password(password);
                newUser.Password = pwd.GetSaltedHash();
                newUser.Salt = pwd.Salt;
                newUser.Tries = 0;
                newUser.Locked = false;
                newUser.LastReportGroup = 0;

                userData.AddUserAccountsRow(newUser);
                try
                {
                    userAccountsTableAdapter.Update(userData);
                    userData.AcceptChanges();
                }
                catch (Exception ex) 
                {
                    throw new EMMADataException(ExceptionSeverity.Critical, "Unable to create new account.", ex);
                }


            }
            else
            {
                throw new EMMAException(ExceptionSeverity.Error,
                    "Account '" + name + "' already exists.");
            }
        }

        /// <summary>
        /// Open a user account.
        /// If the account does not have a password protection flag then the user is allowed access
        /// regardless of the value of the password paramters, otherwise, the password is encrypted
        /// and this must match the encrypted password stored in the database.
        /// </summary>
        /// <param name="name">The name of the account to open</param>
        /// <param name="password">The password to open the account</param>
        public static void OpenAccount(string name, string password)
        {
            Diagnostics.StartTimer("OpenAccount.LoadAccount");
            LoadAccountFromDB(name);
            EMMADataSet.UserAccountsRow account = userData.FindByName(name);
            Diagnostics.StopTimer("OpenAccount.LoadAccount");

            if (account != null)
            {
                Password pwd = new Password(password, account.Salt);

                if (!account.Locked)
                {
                    if (account.Password.Trim().Equals(pwd.GetSaltedHash()))
                    {
                        _name = account.Name.Trim();
                        Diagnostics.StartTimer("OpenAccount.InitSettings");
                        InitSettings();
                        // Set API base URL
                        EveAPI.URL_EveApiHTTPS = _settings.APIURL; 
                        Diagnostics.StopTimer("OpenAccount.InitSettings");
                        Diagnostics.StartTimer("OpenAccount.GetGroups");
                        _reportGroups = ReportGroups.GetUsersGroups(_name, true);
                        Diagnostics.StopTimer("OpenAccount.GetGroups");
                        // Automatically set the current report group to the last one used. 
                        for (int i = 0; i < _reportGroups.Count; i++)
                        {
                            ReportGroup group = _reportGroups[i];
                            if (group.ID == account.LastReportGroup)
                            {
                                CurrentGroup = group;
                                i = _reportGroups.Count;
                            }
                        }
                    }
                    else
                    {
                        account.Tries = account.Tries + 1;

                        /*if (account.Tries > 3)
                        {
                            account.Locked = true;
                        }*/
                        userAccountsTableAdapter.Update(account);
                        
                        throw new EMMAInvalidPasswordException(ExceptionSeverity.Warning,
                            "Incorrect password entered for account '" + name + "'");
                    }
                }
                else
                {
                    throw new EMMAException(ExceptionSeverity.Error,
                       "This account is currently locked. Please contact an administrator.");
                }
            }
            else
            {
                throw new EMMAException(ExceptionSeverity.Error,
                    "Cannot find account '" + name + "'");
            }
        }

        /// <summary>
        /// Log the current user account off.
        /// </summary>
        public static void Logout()
        {
            if (_currentGroup != null)
            {
                _currentGroup.StoreEveAccounts();
                _currentGroup.StoreSettings();
                _currentGroup.StoreItemsTraded();
            }
            if (_settings != null)
            {
                StoreSettings();
            }
            _name = "";
            _settings = null;
            _currentGroup = null;
        }


        /// <summary>
        /// Initialise the settings object based upon the current account name
        /// </summary>
        private static void InitSettings()
        {
            EMMADataSet.UserSettingsDataTable settingsTable = new EMMADataSet.UserSettingsDataTable();
            userSettingsTableAdapter.FillByName(settingsTable, _name);
            if (settingsTable.Count > 0)
            {
                XmlDocument settingsDoc = new XmlDocument();
                settingsDoc.LoadXml(settingsTable[0].Settings);
                _settings = new UserSettings(settingsDoc);
            }
            else
            {
                _settings = new UserSettings(_name);
            }
        }


        /// <summary>
        /// Load the specified EMMA Account from the database.
        /// </summary>
        /// <param name="name"></param>
        private static void LoadAccountFromDB(string name)
        {
            userAccountsTableAdapter.ClearBeforeFill = true;
            userAccountsTableAdapter.FillByName(userData, name);
        }


        /// <summary>
        /// Get a list of the report groups on the account.
        /// </summary>
        /// <returns></returns>
        public static List<ReportGroup> GetReportGroups(bool includePublic)
        {
            if (!_name.Equals(""))
            {
                _reportGroups = ReportGroups.GetUsersGroups(_name, includePublic);
                return UserAccount._reportGroups;
            }
            else
            {
                throw new EMMAException(ExceptionSeverity.Error, 
                    "Cannot get report groups, no user currently logged in.");
            }
        }


        /// <summary>
        /// Store the ID for the current report group used by this user.
        /// </summary>
        private static void StoreLastGroup()
        {
            LoadAccountFromDB(_name);
            if(userData.Count > 0) 
            {
                EMMADataSet.UserAccountsRow userRow = userData[0];
                userRow.LastReportGroup = _currentGroup.ID;
                userAccountsTableAdapter.Update(userRow);
            }
        }

        /// <summary>
        /// Store the current user settings in the database
        /// </summary>
        private static void StoreSettings()
        {
            if (_settings.Changed)
            {
                EMMADataSet.UserSettingsDataTable settingsTable = new EMMADataSet.UserSettingsDataTable();
                userSettingsTableAdapter.FillByName(settingsTable, _name);
                if (settingsTable.Count == 0)
                {
                    EMMADataSet.UserSettingsRow newRow = settingsTable.NewUserSettingsRow();
                    newRow.AccountName = _name;
                    // Just make this blank temporarilly so we are allowed to add it to the table.
                    newRow.Settings = "";
                    settingsTable.AddUserSettingsRow(newRow);
                }
                settingsTable[0].Settings = _settings.Xml.InnerXml;

                userSettingsTableAdapter.Update(settingsTable);
                _settings.Changed = false;
            }
        }


        public static string Name
        {
            get { return _name; }
            set { _name = value; }
        }


        public static ReportGroup CurrentGroup
        {
            get { return UserAccount._currentGroup; }
            set 
            {
                if (_currentGroup != null)
                {
                    _currentGroup.StoreSettings();
                    _currentGroup.StoreItemsTraded();
                }
                UserAccount._currentGroup = value;
                try
                {
                    if (_currentGroup != null)
                    {
                        _currentGroup.LoadEveAccounts();
                        StoreLastGroup();
                    }
                }
                catch (EMMALicensingException) { _currentGroup = null; throw; }
            }
        }

        public static UserSettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Inner class that deals with the creation and maintenance of passwords.
        /// </summary>
        class Password
        {
            string _password;
            int _salt;


            public Password(string password)
            {
                _password = password;
                _salt = CreateRandomSalt();
            }

            public Password(string password, int salt)
            {
                _password = password;
                _salt = salt;
            }

            public string GetSaltedHash() 
            {
                // Create Byte array of password string
                ASCIIEncoding encoder = new ASCIIEncoding();
                Byte[] _secretBytes = encoder.GetBytes(_password);

                // Create a new salt
                Byte[] _saltBytes = new Byte[4];
                _saltBytes[0] = (byte)(_salt >> 24);
                _saltBytes[1] = (byte)(_salt >> 16);
                _saltBytes[2] = (byte)(_salt >> 8);
                _saltBytes[3] = (byte)(_salt);

                // append the two arrays
                Byte[] toHash = new Byte[_secretBytes.Length + _saltBytes.Length];
                Array.Copy(_secretBytes, 0, toHash, 0, _secretBytes.Length);
                Array.Copy(_saltBytes, 0, toHash, _secretBytes.Length, _saltBytes.Length);

                SHA1 sha1 = SHA1.Create();
                Byte[] computedHash = sha1.ComputeHash(toHash);

                return encoder.GetString(computedHash);
            }

            private int CreateRandomSalt()
            {
                Byte[] _saltBytes = new Byte[4];
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetBytes(_saltBytes);

                return ((((int)_saltBytes[0]) << 24) + (((int)_saltBytes[1]) << 16) +
                  (((int)_saltBytes[2]) << 8) + ((int)_saltBytes[3]));
            }

            public int Salt
            {
                get { return _salt; }
                set { _salt = value; }
            }

        }
    }

}
