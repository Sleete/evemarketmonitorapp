using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Xml;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

using EveMarketMonitorApp.Common;


namespace EveMarketMonitorApp.DatabaseClasses
{
    // The updater will check the database and update it if required with new stored proceedures, tables, etc.

    static class Updater
    {

        public static void WaitForAcknowledge()
        {
            // Wait for up to a minute to establish connection
            SqlConnection connection = new SqlConnection(
                Properties.Settings.Default.EMMA_DatabaseConnectionString + ";Connection Timeout=60;Pooling=false");

            try
            {
                SqlCommand command = null;
                connection.Open();

                command = new SqlCommand("_GetVersion", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandTimeout = 60;
                string value = "";
                SqlParameter param = new SqlParameter("@version", SqlDbType.VarChar, 50,
                    ParameterDirection.InputOutput, 0, 0, null, DataRowVersion.Current,
                    false, value, "", "", "");
                command.Parameters.Add(param);
                command.ExecuteNonQuery();
            }
            finally
            {
                if (connection != null) { connection.Close(); }
            }
        }

        public static void Update()
        {
            bool migrated = false;
            try
            {
                migrated = Properties.Settings.Default.Migrated;
            }
            catch { }

            if (!migrated)
            {
                MigrateSettings();
            }

            #region Update EMMA Database
            SqlConnection connection = new SqlConnection(Properties.Settings.Default.EMMA_DatabaseConnectionString +
                ";Pooling=false");
            connection.Open();

            try
            {
                SqlCommand command = null;
                SqlDataAdapter adapter = null;
                string commandText = "";

                command = new SqlCommand("_GetVersion", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                string value = "";
                SqlParameter param = new SqlParameter("@version", SqlDbType.VarChar, 50,
                    ParameterDirection.InputOutput, 0, 0, null, DataRowVersion.Current,
                    false, value, "", "", "");
                command.Parameters.Add(param);
                command.ExecuteNonQuery();
                Version dbVersion = new Version(param.Value.ToString());

                // Setup server connection for SMO 
                Server server = new Server(new ServerConnection(connection));


                if (dbVersion.CompareTo(new Version("1.1.1.12")) < 0)
                {
                    #region 1.0.0.0 - 1.1.1.12
                    if (dbVersion.CompareTo(new Version("1.0.0.1")) < 0)
                    {
                        #region Update RptGroupSetAccounts stored procedure
                        commandText =
                                "ALTER PROCEDURE dbo.RptGroupSetAccounts\r\n" +
                                "@rptGroupID		int,\r\n" +
                                "@eveAccountIDs	varchar(max)\r\n" +
                                "AS\r\n" +
                                "DELETE FROM	RptGroupAccounts\r\n" +
                                "WHERE	RptGroupID = @rptGroupID\r\n" +
                                "INSERT INTO RptGroupAccounts (RptGroupID, EveAccountID)\r\n" +
                                "SELECT @rptGroupID, eveaccounts.number\r\n" +
                                "FROM CLR_intlist_split(@eveAccountIDs) AS eveaccounts\r\n" +
                                "RETURN\r\n";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.1"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'RptGroupSetAccounts'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.2")) < 0)
                    {
                        #region Update ReportGroupNew stored procedure
                        commandText =
                                "ALTER PROCEDURE dbo.ReportGroupNew\r\n" +
                                "@username char(50),\r\n" +
                                "@groupname char(50),\r\n" +
                                "@publicGroup bit\r\n" +
                                "AS\r\n" +
                                "DECLARE @newID AS int\r\n" +
                                "SELECT @newID = (\r\n" +
                                "   SELECT MAX(ID)\r\n" +
                                "   FROM ReportGroups) + 1\r\n" +
                                "INSERT INTO ReportGroups (ID, Name, PublicGroup)\r\n" +
                                "VALUES (@newID, @groupname, @publicGroup)\r\n" +
                                "SELECT *\r\n" +
                                "FROM UserRptGroups\r\n" +
                                "WHERE (UserName = @username) AND (RptGroupID = @newID)\r\n" +
                                "IF(@@ROWCOUNT = 0)\r\n" +
                                "BEGIN\r\n" +
                                "   INSERT INTO UserRptGroups (UserName, RptGroupID)\r\n" +
                                "   VALUES (@username, @newID)\r\n" +
                                "END\r\n" +
                                "RETURN";


                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.2"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'ReportGroupNew'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.3")) < 0)
                    {
                        #region Update ReportGroupNew stored procedure
                        commandText =
                                "ALTER PROCEDURE dbo.ReportGroupNew\r\n" +
                                "@username char(50),\r\n" +
                                "@groupname char(50),\r\n" +
                                "@publicGroup bit\r\n" +
                                "AS\r\n" +
                                "DECLARE @newID AS int\r\n" +
                                "SELECT @newID = (\r\n" +
                                "   SELECT MAX(ID)\r\n" +
                                "   FROM ReportGroups) + 1\r\n" +
                                "IF(@newID IS NULL)\r\n" +
                                "BEGIN\r\n" +
                                "SET @newID = 1\r\n" +
                                "END\r\n" +
                                "INSERT INTO ReportGroups (ID, Name, PublicGroup)\r\n" +
                                "VALUES (@newID, @groupname, @publicGroup)\r\n" +
                                "SELECT *\r\n" +
                                "FROM UserRptGroups\r\n" +
                                "WHERE (UserName = @username) AND (RptGroupID = @newID)\r\n" +
                                "IF(@@ROWCOUNT = 0)\r\n" +
                                "BEGIN\r\n" +
                                "   INSERT INTO UserRptGroups (UserName, RptGroupID)\r\n" +
                                "   VALUES (@username, @newID)\r\n" +
                                "END\r\n" +
                                "RETURN";


                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.3"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'ReportGroupNew'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.4")) < 0)
                    {
                        #region Update DividendFindByCorpAndDate stored procedure
                        commandText =
                                "ALTER PROCEDURE dbo.DividendFindByCorpAndDate\r\n" +
                                "@corpID int,\r\n" +
                                "@startDate datetime,\r\n" +
                                "@endDate datetime,\r\n" +
                                "@mustHaveJournal	bit,\r\n" +
                                "@mustNotHaveJournal	bit\r\n" +
                                "AS\r\n" +
                                "IF(@mustHaveJournal = 0 AND @mustNotHaveJournal = 0)\r\n" +
                                "BEGIN\r\n" +
                                "   SELECT Dividends.*\r\n" +
                                "   FROM Dividends\r\n" +
                                "   WHERE (CorpID = @corpID OR @corpID = 0) AND (DateTime BETWEEN @startDate AND @endDate)\r\n" +
                                "END\r\n" +
                                "ELSE IF (@mustNotHaveJournal = 1)\r\n" +
                                "BEGIN\r\n" +
                                "   SELECT Dividends.*\r\n" +
                                "   FROM Dividends\r\n" +
                                "   WHERE (CorpID = @corpID OR @corpID = 0) AND (DateTime BETWEEN @startDate AND @endDate) AND\r\n" +
                                "       (Dividends.DividendID NOT IN\r\n" +
                                "       (\r\n" +
                                "           SELECT JournalDividendLink.DividendID\r\n" +
                                "           FROM JournalDividendLink\r\n" +
                                "       ))\r\n" +
                                "END\r\n" +
                                "ELSE IF (@mustHaveJournal = 1)\r\n" +
                                "BEGIN\r\n" +
                                "   SELECT DISTINCT Dividends.*\r\n" +
                                "   FROM Dividends\r\n" +
                                "   INNER JOIN JournalDividendLink AS link ON link.DividendID = Dividends.DividendID\r\n" +
                                "   WHERE (CorpID = @corpID OR @corpID = 0) AND (DateTime BETWEEN @startDate AND @endDate)\r\n" +
                                "END\r\n" +
                                "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.4"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'DividendFindByCorpAndDate'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.5")) < 0)
                    {
                        #region Create EveAccountInUse stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'EveAccountInUse') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.EveAccountInUse\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText =
                                "CREATE PROCEDURE dbo.EveAccountInUse\r\n" +
                                "@accountID		int,\r\n" +
                                "@inUse			bit		OUTPUT\r\n" +
                                "AS\r\n" +
                                "SET NOCOUNT ON\r\n" +
                                "SET @inUse = 0\r\n" +
                                "SELECT RptGroupAccounts.*\r\n" +
                                "FROM RptGroupAccounts\r\n" +
                                "WHERE RptGroupAccounts.EveAccountID = @accountID\r\n" +
                                "IF(@@ROWCOUNT > 0)\r\n" +
                                "BEGIN\r\n" +
                                "   SET @inUse = 1\r\n" +
                                "END\r\n" +
                                "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.5"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'EveAccountInUse'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.6")) < 0)
                    {
                        #region Create PublicCorpAllowDelete stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'PublicCorpAllowDelete') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.PublicCorpAllowDelete\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText =
                                "CREATE PROCEDURE dbo.PublicCorpAllowDelete\r\n" +
                                "@corpID		int,\r\n" +
                                "@allowDelete	bit		OUTPUT\r\n" +
                                "AS\r\n" +
                                "SET NOCOUNT ON\r\n" +
                                "DECLARE\r\n" +
                                "   @counter1	int,\r\n" +
                                "   @counter2	int,\r\n" +
                                "   @counter3	int\r\n" +
                                "SET @allowDelete = 0\r\n" +
                                "SELECT @counter1 = COUNT(*) FROM Dividends WHERE CorpID = @corpID\r\n" +
                                "SELECT @counter2 = COUNT(*) FROM WebLinks WHERE CorpID = @corpID\r\n" +
                                "SELECT @counter3 = COUNT(*) FROM ShareTransaction WHERE CorpID = @corpID\r\n" +
                                "IF(@counter1 = 0 AND @counter2 = 0 AND @counter3 = 0)\r\n" +
                                "BEGIN\r\n" +
                                "   SET @allowDelete = 1\r\n" +
                                "END\r\n" +
                                "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.6"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'PublicCorpAllowDelete'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.7")) < 0)
                    {
                        #region Create BankTransactionGetByID stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'BankTransactionGetByID') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.BankTransactionGetByID\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText =
                                "CREATE PROCEDURE dbo.BankTransactionGetByID\r\n" +
                                "@transactionID		bigint\r\n" +
                                "AS\r\n" +
                                "SET NOCOUNT ON\r\n" +
                                "SELECT * FROM BankTransaction WHERE TransactionID = @transactionID\r\n" +
                                "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.7"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'BankTransactionGetByID'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.8")) < 0)
                    {
                        #region Create ItemValueHistory table
                        commandText =
                            "if exists (select * from dbo.sysobjects where id = " +
                            "object_id(N'[dbo].[ItemValueHistory]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\n" +
                            "   drop table [dbo].[ItemValueHistory]\r\n\r\n" +
                            "CREATE TABLE [dbo].[ItemValueHistory]\r\n" +
                            "(\r\n" +
                            "   [ValueDate] [datetime] NOT NULL ,\r\n" +
                            "   [ItemID] [int] NOT NULL ,\r\n" +
                            "   [RegionID] [int] NOT NULL ,\r\n" +
                            "   [ReportGroupID] [int] NOT NULL ,\r\n" +
                            "   [BuyValue] [numeric](18,2) NOT NULL ,\r\n" +
                            "   [SellValue] [numeric](18,2) NOT NULL ,\r\n" +
                            "   CONSTRAINT [PK_ItemValueHistory] PRIMARY KEY  CLUSTERED\r\n" +
                            "   (\r\n" +
                            "       [ValueDate],\r\n" +
                            "       [ItemID],\r\n" +
                            "       [RegionID],\r\n" +
                            "       [ReportGroupID]\r\n" +
                            "   )\r\n" +
                            ")\r\n" +
                            "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.8"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create database table 'ItemValueHistory'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.9")) < 0)
                    {
                        #region Create ItemWebValueHistory table
                        commandText =
                            "if exists (select * from dbo.sysobjects where id = " +
                            "object_id(N'[dbo].[ItemWebValueHistory]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\n" +
                            "   drop table [dbo].[ItemWebValueHistory]\r\n\r\n" +
                            "CREATE TABLE [dbo].[ItemWebValueHistory]\r\n" +
                            "(\r\n" +
                            "   [ValueDate] [datetime] NOT NULL ,\r\n" +
                            "   [ItemID] [int] NOT NULL ,\r\n" +
                            "   [RegionID] [int] NOT NULL ,\r\n" +
                            "   [BuyValue] [numeric](18,2) NOT NULL ,\r\n" +
                            "   [SellValue] [numeric](18,2) NOT NULL ,\r\n" +
                            "   CONSTRAINT [PK_ItemWebValueHistory] PRIMARY KEY  CLUSTERED\r\n" +
                            "   (\r\n" +
                            "       [ValueDate],\r\n" +
                            "       [ItemID],\r\n" +
                            "       [RegionID]\r\n" +
                            "   )\r\n" +
                            ")\r\n" +
                            "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.9"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create database table 'ItemWebValueHistory'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.10")) < 0)
                    {
                        #region Change 'active' order status code from 0 to 999
                        commandText =
                            "SELECT * FROM OrderStates WHERE StateID = 999\r\n" +
                            "IF(@@ROWCOUNT <= 0)\r\n" +
                            "BEGIN\r\n" +
                            "   INSERT INTO OrderStates ([StateID], [Description])\r\n" +
                            "   VALUES (999, 'Active')\r\n" +
                            "   DELETE FROM OrderStates WHERE StateID = 0\r\n" +
                            "   UPDATE Orders\r\n" +
                            "   SET OrderState = 999\r\n" +
                            "   WHERE OrderState = 0\r\n" +
                            "END";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.10"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to Change 'active' order status code from 0 to 999.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.11")) < 0)
                    {
                        #region Create ItemValueGet stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ItemValueGet') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ItemValueGet\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText =
                                "CREATE PROCEDURE dbo.ItemValueGet\r\n" +
                                "@valueDate		datetime,\r\n" +
                                "@itemID		int,\r\n" +
                                "@regionID		int,\r\n" +
                                "@reportGroupID	int,\r\n" +
                                "@buyPrice  	bit,\r\n" +
                                "@webPrice  	bit,\r\n" +
                                "@value         numeric(18,2)   OUTPUT,\r\n" +
                                "@trueDate      datetime        OUTPUT\r\n" +
                                "AS\r\n" +
                                "SET NOCOUNT ON\r\n\r\n" +

                                "IF (@buyPrice = 0 AND @webPrice = 0)\r\n" +
                                "BEGIN\r\n" +
                                "SELECT @value = SellValue, @trueDate = ValueDate FROM ItemValueHistory\r\n" +
                                "WHERE ItemID = @itemID AND RegionID = @regionID AND ReportGroupID = @reportGroupID AND\r\n" +
                                "   (ABS(DATEDIFF(hh, @valueDate, ValueDate)) =\r\n" +
                                "   (\r\n" +
                                "       SELECT MIN(ABS(DATEDIFF(hh, @valueDate, ValueDate)))\r\n" +
                                "       FROM ItemValueHistory\r\n" +
                                "       WHERE ItemID = @itemID AND RegionID = @regionID AND " +
                                "           ReportGroupID = @reportGroupID AND NOT SellValue = 0\r\n" +
                                "       ))\r\n" +
                                "END\r\n\r\n" +

                                "IF (@buyPrice = 0 AND NOT @webPrice = 0)\r\n" +
                                "BEGIN\r\n" +
                                "SELECT @value = SellValue, @trueDate = ValueDate FROM ItemWebValueHistory\r\n" +
                                "WHERE ItemID = @itemID AND RegionID = @regionID AND\r\n" +
                                "   (ABS(DATEDIFF(hh, @valueDate, ValueDate)) =\r\n" +
                                "   (\r\n" +
                                "       SELECT MIN(ABS(DATEDIFF(hh, @valueDate, ValueDate)))\r\n" +
                                "       FROM ItemWebValueHistory\r\n" +
                                "       WHERE ItemID = @itemID AND RegionID = @regionID AND NOT SellValue = 0\r\n" +
                                "       ))\r\n" +
                                "END\r\n\r\n" +

                                "IF (NOT @buyPrice = 0 AND @webPrice = 0)\r\n" +
                                "BEGIN\r\n" +
                                "SELECT @value = BuyValue, @trueDate = ValueDate FROM ItemValueHistory\r\n" +
                                "WHERE ItemID = @itemID AND RegionID = @regionID AND ReportGroupID = @reportGroupID AND\r\n" +
                                "   (ABS(DATEDIFF(hh, @valueDate, ValueDate)) =\r\n" +
                                "   (\r\n" +
                                "       SELECT MIN(ABS(DATEDIFF(hh, @valueDate, ValueDate)))\r\n" +
                                "       FROM ItemValueHistory\r\n" +
                                "       WHERE ItemID = @itemID AND RegionID = @regionID AND " +
                                "           ReportGroupID = @reportGroupID AND NOT BuyValue = 0\r\n" +
                                "       ))\r\n" +
                                "END\r\n\r\n" +

                                "IF (NOT @buyPrice = 0 AND NOT @webPrice = 0)\r\n" +
                                "BEGIN\r\n" +
                                "SELECT @value = BuyValue, @trueDate = ValueDate FROM ItemWebValueHistory\r\n" +
                                "WHERE ItemID = @itemID AND RegionID = @regionID AND\r\n" +
                                "   (ABS(DATEDIFF(hh, @valueDate, ValueDate)) =\r\n" +
                                "   (\r\n" +
                                "       SELECT MIN(ABS(DATEDIFF(hh, @valueDate, ValueDate)))\r\n" +
                                "       FROM ItemWebValueHistory\r\n" +
                                "       WHERE ItemID = @itemID AND RegionID = @regionID AND NOT BuyValue = 0\r\n" +
                                "       ))\r\n" +
                                "END\r\n" +
                                "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.11"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ItemValueGet'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.12")) < 0)
                    {
                        #region Create ItemValueSet stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ItemValueSet') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ItemValueSet\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText =
                            "CREATE PROCEDURE dbo.ItemValueSet\r\n" +
                            "@valueDate		datetime,\r\n" +
                            "@itemID		int,\r\n" +
                            "@regionID		int,\r\n" +
                            "@reportGroupID	int,\r\n" +
                            "@buyPrice  	bit,\r\n" +
                            "@webPrice  	bit,\r\n" +
                            "@value         numeric(18,2)\r\n" +
                            "AS\r\n" +
                            "DECLARE    @sellValue  numeric(18,2),\r\n" +
                            "           @buyValue  numeric(18,2)\r\n" +
                            "SET NOCOUNT ON\r\n\r\n" +

                            "IF (@buyPrice = 0)\r\n" +
                            "BEGIN\r\n" +
                            "   SET @sellValue = @value\r\n" +
                            "   SET @buyValue = 0\r\n" +
                            "END\r\n" +
                            "ELSE\r\n" +
                            "BEGIN\r\n" +
                            "   SET @sellValue = 0\r\n" +
                            "   SET @buyValue = @value\r\n" +
                            "END\r\n\r\n" +

                            "IF (@webPrice = 0)\r\n" +
                            "BEGIN\r\n" +
                            "   SELECT * FROM ItemValueHistory\r\n" +
                            "   WHERE ValueDate = @valueDate AND ItemID = @itemID AND RegionID = @regionID AND\r\n" +
                            "       ReportGroupID = @reportGroupID\r\n" +
                            "   IF (@@ROWCOUNT <= 0)\r\n" +
                            "   BEGIN\r\n" +
                            "       INSERT INTO ItemValueHistory ([ValueDate], [ItemID], [RegionID], [ReportGroupID], [BuyValue], [SellValue])\r\n" +
                            "       VALUES (@valueDate, @itemID, @regionID, @reportGroupID, @buyValue, @sellValue)\r\n" +
                            "   END\r\n" +
                            "   ELSE\r\n" +
                            "   BEGIN\r\n" +
                            "       IF (@buyPrice = 0)\r\n" +
                            "       BEGIN\r\n" +
                            "           UPDATE ItemValueHistory\r\n" +
                            "           SET [SellValue] = @value\r\n" +
                            "           WHERE ValueDate = @valueDate AND ItemID = @itemID AND RegionID = @regionID AND\r\n" +
                            "               ReportGroupID = @reportGroupID\r\n" +
                            "       END\r\n" +
                            "       ELSE\r\n" +
                            "       BEGIN\r\n" +
                            "           UPDATE ItemValueHistory\r\n" +
                            "           SET [BuyValue] = @value\r\n" +
                            "           WHERE ValueDate = @valueDate AND ItemID = @itemID AND RegionID = @regionID AND\r\n" +
                            "               ReportGroupID = @reportGroupID\r\n" +
                            "       END\r\n" +
                            "   END\r\n" +
                            "END\r\n" +
                            "ELSE\r\n" +
                            "BEGIN\r\n" +
                            "   SELECT * FROM ItemWebValueHistory\r\n" +
                            "   WHERE ValueDate = @valueDate AND ItemID = @itemID AND RegionID = @regionID\r\n" +
                            "   IF (@@ROWCOUNT <= 0)\r\n" +
                            "   BEGIN\r\n" +
                            "       INSERT INTO ItemWebValueHistory ([ValueDate], [ItemID], [RegionID], [BuyValue], [SellValue])\r\n" +
                            "       VALUES (@valueDate, @itemID, @regionID, @buyValue, @sellValue)\r\n" +
                            "   END\r\n" +
                            "   ELSE\r\n" +
                            "   BEGIN\r\n" +
                            "       IF (@buyPrice = 0)\r\n" +
                            "       BEGIN\r\n" +
                            "           UPDATE ItemWebValueHistory\r\n" +
                            "           SET [SellValue] = @value\r\n" +
                            "           WHERE ValueDate = @valueDate AND ItemID = @itemID AND RegionID = @regionID\r\n" +
                            "       END\r\n" +
                            "       ELSE\r\n" +
                            "       BEGIN\r\n" +
                            "           UPDATE ItemWebValueHistory\r\n" +
                            "           SET [BuyValue] = @value\r\n" +
                            "           WHERE ValueDate = @valueDate AND ItemID = @itemID AND RegionID = @regionID\r\n" +
                            "       END\r\n" +
                            "   END\r\n" +
                            "END\r\n" +
                            "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.12"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ItemValueSet'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.13")) < 0)
                    {
                        #region Update OrderGetByAnySingle stored procedure
                        commandText =
                                "ALTER PROCEDURE dbo.OrderGetByAnySingle\r\n" +
                                "@ownerID			int,\r\n" +
                                "@forCorp			bit,\r\n" +
                                "@walletID			smallint,\r\n" +
                                "@itemID			int,\r\n" +
                                "@stationID			int,\r\n" +
                                "@state             int,\r\n" +
                                "@type              varchar(4)\r\n" +
                                "AS\r\n" +
                                "SET NOCOUNT ON\r\n" +
                                "SELECT Orders.*\r\n" +
                                "FROM Orders\r\n" +
                                "WHERE (Orders.OwnerID = @ownerID) AND (Orders.ForCorp = @forCorp) AND (@walletID = 0 OR WalletID = @walletID)\r\n" +
                                "   AND (@state = 0 OR Orders.OrderState = @state) AND (@itemID = 0 OR Orders.ItemID = @itemID)\r\n" +
                                "   AND (@stationID = 0 OR Orders.StationID = @stationID)\r\n" +
                                "   AND ((@type LIKE 'Sell' AND Orders.BuyOrder = 0) OR (@type LIKE 'Buy' AND Orders.BuyOrder = 1) OR (@type NOT LIKE 'Buy' AND @type NOT LIKE 'Sell'))\r\n" +
                                "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.13"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'OrderGetByAnySingle'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.14")) < 0)
                    {
                        #region Update OrderGetAny stored procedure
                        commandText =
                                "ALTER PROCEDURE dbo.OrderGetAny\r\n" +
                                "@accessList			varchar(max),\r\n" +
                                "@itemIDs			varchar(max),\r\n" +
                                "@stationIDs			varchar(max),\r\n" +
                                "@state				int,\r\n" +
                                "@type				varchar(4)\r\n" +
                                "AS\r\n" +
                                "SET NOCOUNT ON\r\n" +
                                "SELECT Orders.*\r\n" +
                                "FROM Orders\r\n" +
                                "JOIN CLR_accesslist_split(@accessList) a ON (Orders.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Orders.ForCorp = 1) OR (a.includePersonal = 1 AND Orders.ForCorp = 0)))\r\n" +
                                "JOIN CLR_intlist_split(@itemIDs) i ON (Orders.ItemID = i.number OR i.number = 0)\r\n" +
                                "JOIN CLR_intlist_split(@stationIDs) s ON (Orders.StationID = s.number OR s.number = 0)\r\n" +
                                "WHERE (Orders.OrderState = @state OR @state = 0) AND ((@type LIKE 'Sell' AND Orders.BuyOrder = 0) OR (@type LIKE 'Buy' AND Orders.BuyOrder = 1) OR (@type NOT LIKE 'Buy' AND @type NOT LIKE 'Sell'))\r\n" +
                                "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.14"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'OrderGetAny'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.15")) < 0)
                    {
                        #region Update JournalGetByInvName stored procedure
                        commandText =
                                "ALTER PROCEDURE dbo.JournalGetByInvName\r\n" +
                                "@accessParams	varchar(MAX),\r\n" +
                                "@typeIDs		varchar(max),\r\n" +
                                "@startDate		datetime,\r\n" +
                                "@endDate		datetime,\r\n" +
                                "@text			varchar(100)\r\n" +
                                "AS\r\n" +
                                "SET NOCOUNT ON\r\n" +
                                "SELECT Journal.*\r\n" +
                                "FROM Journal INNER JOIN (SELECT ID FROM Names WHERE (Name LIKE @text)) AS charList\r\n" +
                                "   ON (Journal.RecieverID = charList.ID OR Journal.RCorpID = charList.ID OR Journal.SenderID = charList.ID OR Journal.SCorpID = charList.ID)\r\n" +
                                "JOIN CLR_financelist_split(@accessParams) a ON(\r\n" +
                                "   ((Journal.SenderID = a.ownerID OR Journal.SCorpID = a.ownerID) AND (a.walletID1 = 0 OR (Journal.SWalletID = a.walletID1 OR Journal.SWalletID = a.walletID2 OR Journal.SWalletID = a.walletID3 OR Journal.SWalletID = a.walletID4 OR Journal.SWalletID = a.walletID5 OR Journal.SWalletID = a.walletID6))) OR \r\n" +
                                "   ((Journal.RecieverID = a.ownerID OR Journal.RCorpID = a.ownerID) AND (a.walletID1 = 0 OR  (Journal.RWalletID = a.walletID1 OR Journal.RWalletID = a.walletID2 OR Journal.RWalletID = a.walletID3 OR Journal.RWalletID = a.walletID4 OR Journal.RWalletID = a.walletID5 OR Journal.RWalletID = a.walletID6))) OR a.ownerID = 0)\r\n" +
                                "JOIN CLR_intlist_split(@typeIDs) i ON (Journal.TypeID = i.number OR i.number = 0)\r\n" +
                                "WHERE (Date BETWEEN @startDate AND @endDate)\r\n" +
                                "ORDER BY Date\r\n" +
                                "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.15"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'JournalGetByInvName'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.16")) < 0)
                    {
                        #region Change type of Assets.Quantity column to bigint
                        commandText =
                                "ALTER TABLE dbo.Assets\r\n" +
                                "ALTER COLUMN Quantity bigint NOT NULL";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.16"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem changing type of Assets.Quantity column to bigint.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.17")) < 0)
                    {
                        #region Update AssetsAddQuantity stored procedure
                        commandText = @"ALTER PROCEDURE dbo.AssetsAddQuantity 
	@ownerID		int,
	@corpAsset		bit,
	@itemID			int,
	@stationID		int,
	@systemID		int,
	@regionID		int,
	@status			int,
	@containerID	int,
	@autoConExclude	bit,
	@deltaQuantity	bigint
AS
	DECLARE @oldQuantity bigint, @newQuantity bigint
	DECLARE	@assetID bigint
		
	SET @assetID = 0
	SELECT @oldQuantity = Quantity, @assetID = ID
	FROM Assets
	WHERE OwnerID = @ownerID AND CorpAsset = @corpAsset AND LocationID = @stationID AND ItemID = @itemID AND Status = @status AND ContainerID = @containerID AND AutoConExclude = @autoConExclude
	
	IF(@assetID = 0)
	BEGIN
		INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer]) 
		VALUES (@ownerID, @corpAsset, @stationID, @itemID, @systemID, @regionID, 0, @deltaQuantity, @status, @autoConExclude, 0, 0);
	END 
	ELSE
	BEGIN
		SET @newQuantity = @oldQuantity + @deltaQuantity

		UPDATE [Assets] SET [Quantity] = @newQuantity
		WHERE [OwnerID] = @ownerID AND [CorpAsset] = @corpAsset AND [LocationID] = @stationID AND [ItemID] = @itemID AND [Status] = @status AND [ContainerID] = @containerID AND [AutoConExclude] = @autoConExclude
	END
	
	RETURN";
                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.17"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'AssetsAddQuantity'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.18")) < 0)
                    {
                        #region Update AssetsInsert stored procedure
                        commandText = @"ALTER PROCEDURE dbo.AssetsInsert
	@OwnerID		int,
	@CorpAsset		bit,
	@LocationID		int,
	@ItemID			int,
	@SystemID		int,
	@RegionID		int,
	@ContainerID	bigint,
	@Quantity		bigint,
	@Status			int,
	@Processed		bit,
	@AutoConExclude	bit,
	@IsContainer	bit,
	@newID			bigint OUT
AS
	INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer]) 
	VALUES (@OwnerID, @CorpAsset, @LocationID, @ItemID, @SystemID, @RegionID, @ContainerID, @Quantity, @Status, @AutoConExclude, @Processed, @IsContainer);

	SET @newID = SCOPE_IDENTITY()

	SELECT * 
	FROM Assets 
	WHERE (ID = @newID)
	RETURN";
                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.18"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'AssetsInsert'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.0.0.19")) < 0)
                    {
                        #region Update AssetsUpdate stored procedure
                        commandText = @"ALTER PROCEDURE dbo.AssetsUpdate
	@ID				bigint,
	@OwnerID		int,
	@CorpAsset		bit,
	@LocationID		int,
	@ItemID			int,
	@SystemID		int,
	@RegionID		int,
	@ContainerID	bigint,
	@Quantity		bigint,
	@Status			int,
	@Processed		bit,
	@AutoConExclude	bit,
	@IsContainer	bit,
	@Original_ID	bigint
AS
	UPDATE [Assets] SET [OwnerID] = @OwnerID, [CorpAsset] = @CorpAsset, [LocationID] = @LocationID, [ItemID] = @ItemID, [SystemID] = @SystemID, [RegionID] = @RegionID, [ContainerID] = @ContainerID, [Quantity] = @Quantity, [Status] = @Status, [AutoConExclude] = @AutoConExclude, [Processed] = @Processed, [IsContainer] = @IsContainer
	WHERE ([ID] = @Original_ID);
	
	SELECT * 
	FROM Assets 
	WHERE (ID = @ID)
	
	RETURN";
                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.0.0.19"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'AssetsUpdate'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.3")) < 0)
                    {
                        #region Create ReprocessJob table
                        commandText =
                            "if exists (select * from dbo.sysobjects where id = " +
                            "object_id(N'[dbo].[ReprocessJob]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\n" +
                            "   drop table [dbo].[ReprocessJob]\r\n\r\n" +
                            "CREATE TABLE [dbo].[ReprocessJob]\r\n" +
                            "(\r\n" +
                            "   [ID] [int] IDENTITY ,\r\n" +
                            "   [JobDate] [datetime] NOT NULL ,\r\n" +
                            "   [StationID] [int] NOT NULL ,\r\n" +
                            "   [GroupID] [int] NOT NULL ,\r\n" +
                            "   [OwnerID] [int] NOT NULL ,\r\n" +
                            "   CONSTRAINT [PK_ReprocessJob] PRIMARY KEY  NONCLUSTERED\r\n" +
                            "   (\r\n" +
                            "       [ID]\r\n" +
                            "   )\r\n" +
                            ")\r\n" +
                            "\r\n" +
                            "CREATE INDEX ix_reprocessjob_date ON dbo.ReprocessJob (JobDate)\r\n" +
                            "CREATE CLUSTERED INDEX ix_reprocessjob_group ON dbo.ReprocessJob (GroupID)\r\n" +
                            "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.3"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create database table 'ReprocessJob'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.4")) < 0)
                    {
                        #region Create ReprocessItem table
                        commandText =
                            "if exists (select * from dbo.sysobjects where id = " +
                            "object_id(N'[dbo].[ReprocessItem]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\n" +
                            "   drop table [dbo].[ReprocessItem]\r\n\r\n" +
                            "CREATE TABLE [dbo].[ReprocessItem]\r\n" +
                            "(\r\n" +
                            "   [JobID] [int] NOT NULL ,\r\n" +
                            "   [ItemID] [int] NOT NULL ,\r\n" +
                            "   [Quantity] [bigint] NOT NULL ,\r\n" +
                            "   [BuyPrice] [numeric](18,2) NOT NULL ,\r\n" +
                            "   CONSTRAINT [PK_ReprocessItem] PRIMARY KEY  CLUSTERED\r\n" +
                            "   (\r\n" +
                            "       [JobID],\r\n" +
                            "       [ItemID]\r\n" +
                            "   )\r\n" +
                            ")\r\n" +
                            "CREATE INDEX ix_reprocessitem_item ON dbo.ReprocessItem (ItemID)\r\n" +
                            "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.4"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create database table 'ReprocessItem'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.5")) < 0)
                    {
                        #region Create ReprocessResult table
                        commandText =
                            "if exists (select * from dbo.sysobjects where id = " +
                            "object_id(N'[dbo].[ReprocessResult]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\n" +
                            "   drop table [dbo].[ReprocessResult]\r\n\r\n" +
                            "CREATE TABLE [dbo].[ReprocessResult]\r\n" +
                            "(\r\n" +
                            "   [JobID] [int] NOT NULL ,\r\n" +
                            "   [ItemID] [int] NOT NULL ,\r\n" +
                            "   [JobDate] [datetime] NOT NULL, \r\n" +
                            "   [Quantity] [bigint] NOT NULL ,\r\n" +
                            "   [EffectiveBuyPrice] [numeric](18,2) NOT NULL ,\r\n" +
                            "   [EstSellPrice] [numeric](18,2) NOT NULL ,\r\n" +
                            "   CONSTRAINT [PK_ReprocessResult] PRIMARY KEY  CLUSTERED\r\n" +
                            "   (\r\n" +
                            "       [JobID],\r\n" +
                            "       [ItemID]\r\n" +
                            "   )\r\n" +
                            ")\r\n" +
                            "CREATE INDEX ix_reprocessresult_item ON dbo.ReprocessResult (ItemID)\r\n" +
                            "RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.5"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create database table 'ReprocessResult'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.6")) < 0)
                    {
                        #region Create ReprocessItemGetByJob stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ReprocessItemGetByJob') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ReprocessItemGetByJob\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.ReprocessItemGetByJob
	@jobID		int
AS
	SELECT * 
	FROM ReprocessItem
	WHERE (JobID = @jobID)
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.6"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ReprocessItemGetByJob'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.7")) < 0)
                    {
                        #region Create ReprocessResultsGetByJob stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ReprocessResultsGetByJob') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ReprocessResultsGetByJob\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.ReprocessResultsGetByJob
	@jobID		int
AS
	SELECT * 
	FROM ReprocessResult 
	WHERE (JobID = @jobID)
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.7"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ReprocessResultsGetByJob'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.8")) < 0)
                    {
                        #region Create ReprocessJobsGetByGroup stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ReprocessJobsGetByGroup') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ReprocessJobsGetByGroup\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.ReprocessJobsGetByGroup
	@groupID		int
AS
	SELECT * 
	FROM ReprocessJob
	WHERE (GroupID = @groupID)
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.8"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ReprocessJobsGetByGroup'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.9")) < 0)
                    {
                        #region Create ReprocessResultsGetByItem stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ReprocessResultsGetByItem') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ReprocessResultsGetByItem\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.ReprocessResultsGetByItem
	@itemID		int,
    @groupID    int
AS
	SELECT ReprocessResult.* 
	FROM ReprocessResult
    JOIN 
    (
        SELECT ID 
        FROM ReprocessJob 
        WHERE GroupID = @groupID
    ) AS jobs ON ReprocessResult.JobID = jobs.ID
	WHERE (ReprocessResult.ItemID = @itemID)
    ORDER BY ReprocessResult.JobID DESC
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.9"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ReprocessResultsGetByItem'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.10")) < 0)
                    {
                        #region Create ReprocessJobsGetByID stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ReprocessJobsGetByID') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ReprocessJobsGetByID\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.ReprocessJobsGetByID
	@jobID		int
AS
	SELECT * 
	FROM ReprocessJob
	WHERE (ID = @jobID)
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.10"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ReprocessJobsGetByID'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.11")) < 0)
                    {
                        #region Create ReprocessItemClearByJob stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ReprocessItemClearByJob') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ReprocessItemClearByJob\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.ReprocessItemClearByJob
	@jobID		int
AS
	DELETE 
	FROM ReprocessItem
	WHERE (JobID = @jobID)
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.11"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ReprocessItemClearByJob'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.12")) < 0)
                    {
                        #region Create ReprocessResultsClearByJob stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ReprocessResultsClearByJob') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ReprocessResultsClearByJob\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.ReprocessResultsClearByJob
	@jobID		int
AS
	DELETE 
	FROM ReprocessResult 
	WHERE (JobID = @jobID)
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.12"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ReprocessResultsClearByJob'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.13")) < 0)
                    {
                        #region Update AssetsGetByLocationAndItem stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'AssetsGetByLocationAndItem') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.AssetsGetByLocationAndItem\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.AssetsGetByLocationAndItem 
	@accessList			varchar(max),
	@regionIDs			varchar(max),
	@systemID			int,
	@locationID			int,
	@itemID				int,
	@containersOnly		bit,
	@getContained		bit,
	@status				int
AS
IF(NOT @regionIDs LIKE '')
BEGIN
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
	JOIN CLR_intlist_split(@regionIDs) r ON Assets.RegionID = r.number 
	WHERE (Assets.Status = @status OR @status = 0) AND (Assets.SystemID = @systemID OR @systemID = 0) AND (Assets.LocationID = @locationID OR @locationID = 0) AND (Assets.ItemID = @itemID OR @itemID = 0) AND (Assets.IsContainer = 1 OR @containersOnly = 0) AND (Assets.ContainerID = 0 OR @getContained = 1)
END
ELSE
BEGIN
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
	WHERE (Assets.Status = @status OR @status = 0) AND (Assets.SystemID = @systemID OR @systemID = 0) AND (Assets.LocationID = @locationID OR @locationID = 0) AND (Assets.ItemID = @itemID OR @itemID = 0) AND (Assets.IsContainer = 1 OR @containersOnly = 0) AND (Assets.ContainerID = 0 OR @getContained = 1)
END

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.13"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'AssetsGetByLocationAndItem'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.14")) < 0)
                    {
                        #region Create ReprocessJobStoreNew stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ReprocessJobStoreNew') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ReprocessJobStoreNew\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.ReprocessJobStoreNew 
	@jobDate	datetime,
	@stationID	int,
	@groupID	int,
	@ownerID	int,
	@newID		int     OUTPUT
AS
	INSERT INTO [ReprocessJob] ([JobDate], [StationID], [GroupID], [OwnerID]) 
	VALUES (@jobDate, @stationID, @groupID, @ownerID);

	SET @newID = SCOPE_IDENTITY()

	SELECT * 
	FROM ReprocessJob 
	WHERE (ID = @newID)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.14"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ReprocessJobStoreNew'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.15")) < 0)
                    {
                        #region Update TransGetItemIDs stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'TransGetItemIDs') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.TransGetItemIDs\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.TransGetItemIDs
	@accessParams		varchar(max),
	@minUnits			int,
	@minDate			datetime,
	@maxDate			datetime
AS
	SELECT ItemID AS [ID]
	FROM Transactions
	JOIN CLR_financelist_split(@accessParams) a ON(
		((Transactions.BuyerID = a.ownerID OR Transactions.BuyerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.BuyerWalletID = a.walletID1 OR Transactions.BuyerWalletID = a.walletID2 OR Transactions.BuyerWalletID = a.walletID3 OR Transactions.BuyerWalletID = a.walletID4 OR Transactions.BuyerWalletID = a.walletID5 OR Transactions.BuyerWalletID = a.walletID6))) OR 
		((Transactions.SellerID = a.ownerID OR Transactions.SellerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.SellerWalletID = a.walletID1 OR Transactions.SellerWalletID = a.walletID2 OR Transactions.SellerWalletID = a.walletID3 OR Transactions.SellerWalletID = a.walletID4 OR Transactions.SellerWalletID = a.walletID5 OR Transactions.SellerWalletID = a.walletID6))) OR a.ownerID = 0)
	WHERE DateTime <= @maxDate AND DateTime >= @minDate
	GROUP BY ItemID
	HAVING SUM(CAST(Quantity AS bigint)) > @minUnits
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.15"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'TransGetItemIDs'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.16")) < 0)
                    {
                        #region Update OrdersFinishUnProcessed stored procedure
                        commandText = @"ALTER PROCEDURE dbo.OrdersFinishUnProcessed 
	@ownerID		int,
	@notify			bit,
	@notifyBuy		bit,
	@notifySell		bit,
	@forCorp		bit
AS
	UPDATE Orders
	SET OrderState = 1000, Escrow = 0
	WHERE (OwnerID = @ownerID) AND (ForCorp = @forCorp) AND (Processed = 0) AND (OrderState = 999 OR OrderState = 2) AND
		(@notify = 1 AND ((@notifyBuy = 1 AND BuyOrder = 1) OR (@notifySell = 1 AND BuyOrder = 0)))
		
	UPDATE Orders
	SET OrderState = 2000, Escrow = 0
	WHERE (OwnerID = @ownerID) AND (ForCorp = @forCorp) AND (Processed = 0) AND (OrderState = 999 OR OrderState = 2) AND
		(@notify = 0 OR ((@notifyBuy = 0 AND BuyOrder = 1) OR (@notifySell = 0 AND BuyOrder = 0)))
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.16"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'OrdersFinishUnProcessed'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.17")) < 0)
                    {
                        #region Update BankTransactionClearAfter stored procedure
                        commandText = @"ALTER PROCEDURE dbo.BankTransactionClearAfter 
	@accountID	int,
	@corpID		int,
	@date		datetime,
	@typeID		smallint
AS
	IF(@corpID = 0)
	BEGIN
		DELETE
		FROM BankTransaction
		WHERE (AccountID = @accountID) AND (DateTime > @date) AND (Type = @typeID OR @typeID = 0)
	END
	ELSE
	BEGIN
		DELETE 
		FROM BankTransaction
		WHERE Exists
		(
			SELECT *
			FROM BankAccount
			WHERE BankTransaction.AccountID = BankAccount.AccountID AND PublicCorpID = @corpID 
		) 
		AND (DateTime > @date) AND (Type = @typeID OR @typeID = 0)	
	END
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.17"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'BankTransactionClearAfter'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.0.18")) < 0)
                    {
                        #region Add 'DefaultBuyPrice' column to TradedItems table
                        commandText =
                                "ALTER TABLE dbo.TradedItems\r\n" +
                                "ADD DefaultBuyPrice numeric(18,2) NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.0.18"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'DefaultBuyPrice' column to TradedItems table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.1.1")) < 0)
                    {
                        #region Add 'UseReprocessVal' column to TradedItems table
                        commandText =
                                "ALTER TABLE dbo.TradedItems\r\n" +
                                "ADD UseReprocessVal bit NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.1.1"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'UseReprocessVal' column to TradedItems table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.1.8")) < 0)
                    {
                        #region Add 'ForceDefaultSellPrice' and 'ForceDefaultBuyPrice' columns to TradedItems table
                        commandText =
                                "ALTER TABLE dbo.TradedItems\r\n" +
                                "ADD ForceDefaultSellPrice bit NOT NULL DEFAULT 0\r\n" +
                                "ALTER TABLE dbo.TradedItems\r\n" +
                                "ADD ForceDefaultBuyPrice bit NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.1.8"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'ForceDefaultSellPrice' and 'ForceDefaultBuyPrice' columns to TradedItems table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.1.9")) < 0)
                    {
                        #region Add 'APICharID' column to RptGroupCorps table
                        commandText =
                                "ALTER TABLE dbo.RptGroupCorps\r\n" +
                                "ADD APICharID int NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            commandText = @"ALTER TABLE dbo.RptGroupCorps
DROP CONSTRAINT PK_RptGroupCorps";
                            adapter = new SqlDataAdapter(commandText, connection);
                            adapter.SelectCommand.ExecuteNonQuery();

                            commandText = @"ALTER TABLE dbo.RptGroupCorps
ADD CONSTRAINT PK_RptGroupCorps PRIMARY KEY (ReportGroupID, APICorpID, APICharID) ";
                            adapter = new SqlDataAdapter(commandText, connection);
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.1.1.9"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'APICharID' column to RptGroupCorps table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.1.10")) < 0)
                    {
                        #region Update RptGroupSetHasCorp stored procedure
                        commandText = @"ALTER PROCEDURE dbo.RptGroupSetHasCorp 
	@rptGroupID		int,
	@apiCorpID		int,
	@included		bit,
	@autoTrans		bit,
	@autoJournal	bit,
	@autoAssets		bit,
	@autoOrders		bit,
	@apiCharID		int
AS
	DELETE FROM	RptGroupCorps
	WHERE	ReportGroupID = @rptGroupID AND APICorpID = @apiCorpID AND APICharID = @apiCharID

	IF(@included = 1)
	BEGIN
		INSERT INTO RptGroupCorps (ReportGroupID, APICorpID, AutoUpdateTrans, AutoUpdateJournal, AutoUpdateAssets, AutoUpdateOrders, APICharID) 
		VALUES (@rptGroupID, @apiCorpID, @autoTrans, @autoJournal, @autoAssets, @autoOrders, @apiCharID)
	END
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.1.10"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'RptGroupSetHasCorp'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.1.11")) < 0)
                    {
                        #region Update RptGroupCorpSettings stored procedure
                        commandText = @"ALTER PROCEDURE dbo.RptGroupCorpSettings 
	@groupID	int,
	@corpID		int,
	@charID		int
AS
	SELECT *
	FROM RptGroupCorps
	WHERE ReportGroupID = @groupID AND APICorpID = @corpID AND APICharID = @charID
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.1.11"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'RptGroupCorpSettings'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.1.1.12")) < 0)
                    {
                        #region Correct spelling mistake in OrderStates table
                        commandText =
                            @"UPDATE [OrderStates] SET [Description] = 'Expired/Filled'
                        WHERE [StateID] = 2";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.1.1.12"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to correct spelling mistake in 'OrderStates' table.", ex);
                        }
                        #endregion
                    }
                    #endregion
                }
                if (dbVersion.CompareTo(new Version("1.3.2.9")) < 0)
                {
                    #region 1.2.0.0 - 1.3.2.9
                    if (dbVersion.CompareTo(new Version("1.2.0.0")) < 0)
                    {
                        #region Add 'OwnerID' column to BankAccount table
                        commandText =
                                "ALTER TABLE dbo.BankAccount\r\n" +
                                "ADD OwnerID int NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.2.0.0"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'OwnerID' column to BankAccount table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.0.1")) < 0)
                    {
                        #region Add 'RiskRatingID' column to PublicCorps table
                        commandText =
                                "ALTER TABLE dbo.PublicCorps\r\n" +
                                "ADD RiskRatingID smallint NOT NULL DEFAULT 1";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.2.0.1"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'RiskRatingID' column to PublicCorps table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.0.2")) < 0)
                    {
                        #region Create and populate 'RiskRating' table
                        commandText =
                                @"CREATE TABLE dbo.RiskRating
(
    ID  [smallint]  NOT NULL,
    Description [varchar](50) NOT NULL,
    CONSTRAINT [PK_RiskRating] PRIMARY KEY CLUSTERED
    (
        [ID]
    )
)
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            commandText =
                                @"INSERT INTO  dbo.RiskRating([ID], [Description])
VALUES (1, 'Not rated')
INSERT INTO  dbo.RiskRating([ID], [Description])
VALUES (2, 'Low Risk')
INSERT INTO  dbo.RiskRating([ID], [Description])
VALUES (3, 'Medium Risk')
INSERT INTO  dbo.RiskRating([ID], [Description])
VALUES (4, 'High Risk')
INSERT INTO  dbo.RiskRating([ID], [Description])
VALUES (5, 'Scam')
RETURN";

                            adapter = new SqlDataAdapter(commandText, connection);
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.2.0.2"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating and populating 'RiskRating' table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.0.3")) < 0)
                    {
                        #region Create RiskRatingGetDesc stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'RiskRatingGetDesc') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.RiskRatingGetDesc\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.RiskRatingGetDesc 
	@ratingID		smallint,
    @description    varchar(50)     OUTPUT
AS
	SELECT @description = Description
    FROM RiskRating
    WHERE ID = @ratingID

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.2.0.3"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'RiskRatingGetDesc'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.0.4")) < 0)
                    {
                        #region Update BankAccountGetByID stored procedure
                        commandText = @"ALTER PROCEDURE dbo.BankAccountGetByID 
	@publicCorpID	int,
	@reportGroupID	int,
	@ownerID		int
AS
	SELECT BankAccount.*
	FROM BankAccount
	WHERE PublicCorpID = @publicCorpID AND ReportGroupID = @reportGroupID AND (OwnerID = @ownerID OR @ownerID = 0)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.2.0.4"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'BankAccountGetByID'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.1.0")) < 0)
                    {
                        #region Add 'ReprocExclude' column to Assets table
                        commandText =
                                "ALTER TABLE dbo.Assets\r\n" +
                                "ADD ReprocExclude bit NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.2.1.0"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'ReprocExclude' column to Assets table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.1.1")) < 0)
                    {
                        #region Create AssetsSetReprocExclude stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'AssetsSetReprocExclude') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.AssetsSetReprocExclude\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.AssetsSetReprocExclude		
	@assetID		bigint,
	@ownerID		int,
	@locationID		int,
	@itemID			int,
	@status			int,
	@containerID	bigint,
	@exclude		bit,
	@corpAsset		bit
AS
	UPDATE Assets
	SET ReprocExclude = @exclude
	WHERE (ID = @assetID OR (@AssetID = 0 AND (OwnerID = @ownerID) AND (LocationID = @locationID OR @locationID = 0) AND (ItemID = @itemID OR @itemID = 0) AND (ContainerID = @containerID OR @containerID = 0) AND (CorpAsset = @corpAsset) AND (Status = @status)))
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.2.1.1"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'AssetsSetReprocExclude'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.1.2")) < 0)
                    {
                        #region Update AssetsUpdate stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'AssetsUpdate') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.AssetsUpdate\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.AssetsUpdate
	@ID				bigint,
	@OwnerID		int,
	@CorpAsset		bit,
	@LocationID		int,
	@ItemID			int,
	@SystemID		int,
	@RegionID		int,
	@ContainerID	bigint,
	@Quantity		bigint,
	@Status			int,
	@Processed		bit,
	@AutoConExclude	bit,
	@IsContainer	bit,
    @ReprocExclude  bit,
	@Original_ID	bigint
AS
	UPDATE [Assets] SET [OwnerID] = @OwnerID, [CorpAsset] = @CorpAsset, [LocationID] = @LocationID, [ItemID] = @ItemID, [SystemID] = @SystemID, [RegionID] = @RegionID, [ContainerID] = @ContainerID, [Quantity] = @Quantity, [Status] = @Status, [AutoConExclude] = @AutoConExclude, [Processed] = @Processed, [IsContainer] = @IsContainer, [ReprocExclude] = @ReprocExclude
	WHERE ([ID] = @Original_ID);
	
	SELECT * 
	FROM Assets 
	WHERE (ID = @ID)
	
	RETURN   ";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.2.1.2"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'AssetsUpdate'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.1.3")) < 0)
                    {
                        #region Update AssetsInsert stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'AssetsInsert') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.AssetsInsert\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.AssetsInsert
	@OwnerID		int,
	@CorpAsset		bit,
	@LocationID		int,
	@ItemID			int,
	@SystemID		int,
	@RegionID		int,
	@ContainerID	bigint,
	@Quantity		bigint,
	@Status			int,
	@Processed		bit,
	@AutoConExclude	bit,
	@IsContainer	bit,
    @ReprocExclude  bit,
	@newID			bigint OUT
AS
	INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [ReprocExclude]) 
	VALUES (@OwnerID, @CorpAsset, @LocationID, @ItemID, @SystemID, @RegionID, @ContainerID, @Quantity, @Status, @AutoConExclude, @Processed, @IsContainer, @ReprocExclude);

	SET @newID = SCOPE_IDENTITY()

	SELECT * 
	FROM Assets 
	WHERE (ID = @newID)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.2.1.3"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'AssetsInsert'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.1.4")) < 0)
                    {
                        #region Create AssetsGetReproc stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'AssetsGetReproc') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.AssetsGetReproc\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.AssetsGetReproc		
	@ownerID				int,
	@corpAsset				bit,
    @stationID				int,
    @status					int,
    @includeContainers		bit,
    @includeNonContainers   bit
AS
	SELECT *
    FROM Assets
	WHERE (ReprocExclude = 0) AND (OwnerID = @ownerID) AND (CorpAsset = @corpAsset) AND (LocationID = @stationID) AND (Status = @status) AND (ContainerID = 0) AND ((@includeContainers = 1 AND @includeNonContainers = 1) OR (@includeContainers = 1 AND IsContainer = 1) OR (@includeNonContainers = 1 AND IsContainer = 0)) 
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.2.1.4"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'AssetsGetReproc'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.1.5")) < 0)
                    {
                        #region Update AssetsGetAutoConByOwner stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'AssetsGetAutoConByOwner') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.AssetsGetAutoConByOwner\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.AssetsGetAutoConByOwner
	@ownerID			int,
	@corpAsset			bit,
	@stationID			int,
	@excludeContainers	bit
AS

	SELECT Assets.*
	FROM Assets
	WHERE (OwnerID = @ownerID AND CorpAsset = @corpAsset AND (LocationID = @stationID OR @stationID = 0) AND (AutoConExclude = 0) AND (Status = 1) AND (@excludeContainers = 0 OR (ContainerID = 0 AND IsContainer = 0)) AND (Quantity > 0))
	ORDER BY LocationID
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.2.1.5"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'AssetsGetAutoConByOwner'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.1.6")) < 0)
                    {
                        #region Update AssetsGetAutoConByAny stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'AssetsGetAutoConByAny') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.AssetsGetAutoConByAny\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.AssetsGetAutoConByAny
	@ownerID			int,
	@corpAsset			bit,
	@stationIDs			varchar(max),
	@regionIDs			varchar(max),
	@excludeContainers	bit
AS

	SELECT Assets.*
	FROM Assets
	JOIN CLR_intlist_split(@stationIDs) s ON (Assets.LocationID = s.number OR s.number = 0)
	JOIN CLR_intlist_split(@regionIDs) r ON (Assets.RegionID = r.number OR r.number = 0)
	WHERE (OwnerID = @ownerID AND CorpAsset = @corpAsset AND 
		(AutoConExclude = 0) AND (Status = 1) AND (@excludeContainers = 0 OR (ContainerID = 0 AND IsContainer = 0)) AND Quantity > 0)
	ORDER BY LocationID
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.2.1.6"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'AssetsGetAutoConByAny'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.1.7")) < 0)
                    {
                        #region Update FixDuplicatedOrders stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'FixDuplicatedOrders') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.FixDuplicatedOrders\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE FixDuplicatedOrders 
AS
BEGIN
	UPDATE Orders
	SET Processed = 0

	UPDATE Orders 
	SET Processed = 1 WHERE ID IN 
	(
		SELECT MAX(ID)
		FROM Orders
		GROUP BY OwnerID, ForCorp, StationID, TotalVol, MinVolume, ItemID, Range, WalletID, Duration, Price, BuyOrder, Issued
	)

	DELETE	
	FROM Orders
	WHERE Processed = 0
END";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.2.1.7"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'FixDuplicatedOrders'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.1.8")) < 0)
                    {
                        #region Update OrderExists stored procedure
                        commandText = @"ALTER PROCEDURE dbo.OrderExists
	@ownerID		int,
	@forCorp		bit,
	@walletID		smallint,
	@stationID		int,
	@itemID			int,
	@totalVol		int,
	@remainingVol	int,
	@range			smallint,
	@orderState		smallint,
	@buyOrder		bit,
	@price			decimal(18,2),
	@exists			bit		OUT,
	@orderID		int		OUT
AS
	SET @exists = 0
	SET	@orderID = 0

	SELECT @orderID = ID
	FROM Orders
	WHERE (OwnerID = @ownerID) AND (ForCorp = @forCorp) AND (WalletID = @walletID) AND 
		(StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
		(TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND ((OrderState = @orderState) OR 
		(@orderState = 2 AND (OrderState = 2 OR OrderState = 1000 OR OrderState = 2000))) AND
		(Processed = 0) AND (Price = @price)
	IF(@orderID = 0 AND @orderState = 2) 
	BEGIN
		-- If we are trying to find a filled/expired order but failed first time around then first try
		-- looking for an active order (i.e. one that was active and is now competed/expired)
		SELECT @orderID = ID
		FROM Orders
		WHERE (OwnerID = @ownerID) AND (ForCorp = @forCorp) AND (WalletID = @walletID) AND 
			(StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
			(TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND (Processed = 0) AND
			(OrderState = 0 OR OrderState = 999) AND (Price = @price)
	END
	IF(@orderID = 0) 
	BEGIN
		-- Still couldn't match an order so try finding one that matches all parameters except price.
		SELECT @orderID = ID
		FROM Orders
		WHERE (OwnerID = @ownerID) AND (ForCorp = @forCorp) AND (WalletID = @walletID) AND 
			(StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
			(TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND (Processed = 0) AND 
			((OrderState = @orderState) OR (@orderState = 2 AND 
			(OrderState = 2 OR OrderState = 1000 OR OrderState = 2000)))
	END
	IF(@orderID = 0) 
	BEGIN
		-- Still couldn't match an order so try finding one that matches all parameters except state and price.
		SELECT @orderID = ID
		FROM Orders
		WHERE (OwnerID = @ownerID) AND (ForCorp = @forCorp) AND (WalletID = @walletID) AND 
			(StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
			(TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND (Processed = 0)
	END
		
	UPDATE Orders
	SET Processed = 1
	WHERE (ID = @orderID)
		
	SELECT Orders.*
	FROM Orders
	WHERE (ID = @orderID)

	IF(@@ROWCOUNT = 1)
	BEGIN
		SET @exists = 1
	END

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.2.1.8"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'OrderExists'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.2.1.9")) < 0)
                    {
                        #region Run FixDuplicatedOrders stored procedure
                        commandText = "FixDuplicatedOrders";
                        adapter = new SqlDataAdapter(commandText, connection);
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.2.1.9"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to run stored procedure 'FixDuplicatedOrders'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.0")) < 0)
                    {
                        #region Add 'Type' column to Contracts table
                        commandText =
                                "ALTER TABLE dbo.Contracts\r\n" +
                                "ADD Type smallint NOT NULL DEFAULT 1";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.0"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'Type' column to Contracts table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.1")) < 0)
                    {
                        #region Create and populate 'ContractType' table
                        commandText =
                                @"CREATE TABLE dbo.ContractType
(
    ID  [smallint]  NOT NULL,
    Description [varchar](50) NOT NULL,
    CONSTRAINT [PK_ContractType] PRIMARY KEY CLUSTERED
    (
        [ID]
    )
)
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            commandText =
                                @"INSERT INTO  dbo.ContractType([ID], [Description])
VALUES (1, 'Courier Contract')
INSERT INTO  dbo.ContractType([ID], [Description])
VALUES (2, 'Item Exchange/Auction')
RETURN";

                            adapter = new SqlDataAdapter(commandText, connection);
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.1"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating and populating 'ContractType' table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.3")) < 0)
                    {
                        #region Update ContractsNew stored procedure
                        commandText = @"ALTER PROCEDURE dbo.ContractsNew 
	@ownerID	int,
	@status		int,
	@pickupID	int,
	@destID		int,
	@collateral	numeric(18,2),
	@reward		numeric(18,2),
	@datetime	datetime,
    @type       smallint,
	@newID		bigint			OUTPUT
AS
	

	SELECT @newID =
	(SELECT MAX(ID) AS MaxID
		FROM Contracts) + 1
		
	IF(@newID IS NULL)
	BEGIN
		SET @newID = 1
	END
	
	INSERT INTO Contracts (ID, OwnerID, Status, PickupStationID, DestinationStationID, Collateral, Reward, DateTime, Type)
	VALUES (@newID, @ownerID, @status, @pickupID, @destID, @collateral, @reward, @datetime, @type)

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.3.0.3"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'ContractsNew'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.5")) < 0)
                    {
                        #region Create ContractTypeGetDesc stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ContractTypeGetDesc') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ContractTypeGetDesc\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.ContractTypeGetDesc		
	@id             smallint,
    @description    varchar(50)     OUTPUT
AS
	SELECT @description = Description
    FROM ContractType
	WHERE ID = @id
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.5"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ContractTypeGetDesc'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.7")) < 0)
                    {
                        #region Create TransGetResultsPage stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'TransGetResultsPage') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.TransGetResultsPage\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE [dbo].[TransGetResultsPage]
    @startRow           int,
    @pageSize           int
AS
SET NOCOUNT ON

	SELECT ID, DateTime, Quantity, ItemID, Price, BuyerID, SellerID, BuyerCharacterID,
		SellerCharacterID, StationID, RegionID, BuyerForCorp, SellerForCorp,
		BuyerWalletID, SellerWalletID
	FROM tmp_TransResults
	WHERE RowNumber BETWEEN (@startRow) AND (@startRow + @pageSize - 1)
	ORDER BY RowNumber
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.7"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'TransGetResultsPage'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.8")) < 0)
                    {
                        #region Create new index for Transactions
                        try
                        {
                            commandText = @"IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Transactions]') AND name = N'IX_Transactions_DT')
        DROP INDEX [IX_Transactions_DT] ON [dbo].[Transactions] WITH ( ONLINE = OFF )

        CREATE NONCLUSTERED INDEX [IX_Transactions_DateTime] ON [dbo].[Transactions] 
        (
            [DateTime] ASC
        )
        INCLUDE ( [ID],
        [BuyerID],
        [SellerID],
        [ItemID],
        [Quantity],
        [Price],
        [BuyerCharacterID],
        [SellerCharacterID],
        [StationID],
        [RegionID],
        [BuyerForCorp],
        [SellerForCorp],
        [BuyerWalletID],
        [SellerWalletID])";
                            adapter = new SqlDataAdapter(commandText, connection);
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.8"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create new index for Transactions.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.9")) < 0)
                    {
                        #region Create AssetsBuildResults stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'AssetsBuildResults') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.AssetsBuildResults\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE [dbo].[AssetsBuildResults]
	@accessList			varchar(max),
	@itemIDs			varchar(max),
	@status				int,
	@groupBy			varchar(50)
    --@totalRows          int         OUTPUT
AS
SET NOCOUNT ON

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_AssetResults]') AND type in (N'U'))
DROP TABLE [dbo].[tmp_AssetResults]

IF(@groupBy LIKE 'Owner')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, 0 AS SystemID, 
		0AS RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0)
	GROUP BY Assets.ItemID, Assets.OwnerID, Assets.CorpAsset
END
ELSE IF(@groupBy LIKE 'Region')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, 0 AS SystemID, 
		Assets.RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0)
	GROUP BY Assets.ItemID, Assets.RegionID, Assets.OwnerID, Assets.CorpAsset
END
ELSE IF(@groupBy LIKE 'System')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, Assets.SystemID, 
		MAX(Assets.RegionID) AS RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0)
	GROUP BY Assets.ItemID, Assets.SystemID, Assets.OwnerID, Assets.CorpAsset
END
ELSE
BEGIN
	SELECT Assets.*, row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0)
END

CREATE CLUSTERED INDEX [PK_tmp_AssetResults] ON [dbo].[tmp_AssetResults] 
(
	[RowNumber]
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF) ON [PRIMARY]	
 
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.9"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'AssetsBuildResults'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.10")) < 0)
                    {
                        #region Create AssetsGetResultsPage stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'AssetsGetResultsPage') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.AssetsGetResultsPage\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE [dbo].[AssetsGetResultsPage]
    @startRow           int,
    @pageSize           int
    --@totalRows          int         OUTPUT
AS
SET NOCOUNT ON

	SELECT ID, OwnerID, CorpAsset, LocationID, ItemID, SystemID, RegionID, ContainerID,
		Quantity, Status, AutoConExclude, Processed, ISContainer, ReprocExclude
	FROM tmp_AssetResults
	WHERE RowNumber BETWEEN (@startRow) AND (@startRow + @pageSize - 1)
	ORDER BY RowNumber";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.10"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'AssetsGetResultsPage'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.11")) < 0)
                    {
                        #region Update AssetsGetLimitedSystemIDs stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'AssetsGetLimitedSystemIDs') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.AssetsGetLimitedSystemIDs\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.AssetsGetLimitedSystemIDs
	@ownerID			int,
	@corpAsset			bit,
	@regionIDs			varchar(MAX),
	@stationIDs			varchar(MAX),
	@includeContainers	bit,
	@includeContents	bit
AS
	SELECT SystemID AS [ID]
	FROM Assets
		JOIN CLR_intlist_split(@regionIDs) r ON (Assets.RegionID = r.number OR r.number = 0)
		JOIN CLR_intlist_split(@stationIDs) s ON (Assets.LocationID = s.number OR s.number = 0)
	WHERE (OwnerID = @ownerID) AND (CorpAsset = @corpAsset) AND (Assets.ContainerID = 0 OR @includeContents = 1) AND
		(Assets.IsContainer = 0 OR @includeContainers = 1)
	GROUP BY SystemID
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.11"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'AssetsGetLimitedSystemIDs'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.12")) < 0)
                    {
                        #region Update TransGetBySingleAndWallets stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'TransGetBySingleAndWallets') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.TransGetBySingleAndWallets\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.TransGetBySingleAndWallets 
	@characterID		int,
	@walletID1			smallint,
	@walletID2			smallint,
	@walletID3			smallint,
	@walletID4			smallint,
	@walletID5			smallint,
	@walletID6			smallint,
	@itemID				int,
	@stationID			int,
	@regionID			int,
	@startDate			datetime,
	@endDate			datetime,
	@transType			char(4)
AS
	
	SELECT DISTINCT Transactions.*
	FROM Transactions
	WHERE (
		((@transType != 'sell' AND 
			(Transactions.BuyerID = @characterID AND 
			(Transactions.BuyerWalletID = @walletID1 OR Transactions.BuyerWalletID = @walletID2 OR 
			Transactions.BuyerWalletID = @walletID3 OR Transactions.BuyerWalletID = @walletID4 OR
			Transactions.BuyerWalletID = @walletID5 OR Transactions.BuyerWalletID = @walletID6))) OR 
		(@transType != 'buy' AND 
			(Transactions.SellerID = @characterID AND 
			(Transactions.SellerWalletID = @walletID1 OR Transactions.SellerWalletID = @walletID2 OR 
			Transactions.SellerWalletID = @walletID3 OR Transactions.SellerWalletID = @walletID4 OR
			Transactions.SellerWalletID = @walletID5 OR Transactions.SellerWalletID = @walletID6)))) AND 
		(Transactions.ItemID = @itemID OR @itemID = 0) AND 
		(Transactions.StationID = @stationID OR @stationID = 0) AND 
		(Transactions.RegionID = @regionID OR @regionID = 0) AND 
		DateTime BETWEEN @startDate AND @endDate)
	ORDER BY DateTime DESC

	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.12"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'TransGetBySingleAndWallets'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.13")) < 0)
                    {
                        #region Update TransGetBySingleAndID stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'TransGetBySingleAndID') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.TransGetBySingleAndID\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.TransGetBySingleAndID 
	@characterID		int,
	@includeCorp		bit,
	@itemID				int,
	@stationID			int,
	@regionID			int,
	@minID				bigint,
	@transType			char(4)
AS
	
	SELECT DISTINCT Transactions.*
	FROM Transactions
	WHERE (
		ID > @minID AND
		((@transType != 'sell' AND 
			(Transactions.BuyerID = @characterID OR 
			(@includeCorp = 1 AND Transactions.BuyerCharacterID = @characterID))) OR 
		(@transType != 'buy' AND 
			(Transactions.SellerID = @characterID OR 
			(@includeCorp = 1 AND Transactions.SellerCharacterID = @characterID)))) AND 
		(Transactions.ItemID = @itemID OR @itemID = 0) AND 
		(Transactions.StationID = @stationID OR @stationID = 0) AND 
		(Transactions.RegionID = @regionID OR @regionID = 0))
	ORDER BY DateTime DESC

	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.13"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'TransGetBySingleAndID'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.14")) < 0)
                    {
                        #region Update TransGetBySingle stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'TransGetBySingle') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.TransGetBySingle\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.TransGetBySingle 
	@characterID		int,
	@includeCorp		bit,
	@itemID				int,
	@stationID			int,
	@regionID			int,
	@startDate			datetime,
	@endDate			datetime,
	@transType			char(4)
AS
	
	SELECT DISTINCT Transactions.* 
	FROM Transactions
	WHERE (
		((@transType != 'sell' AND 
			(Transactions.BuyerID = @characterID OR 
			(@includeCorp = 1 AND Transactions.BuyerCharacterID = @characterID))) OR 
		(@transType != 'buy' AND 
			(Transactions.SellerID = @characterID OR 
			(@includeCorp = 1 AND Transactions.SellerCharacterID = @characterID)))) AND 
		(Transactions.ItemID = @itemID OR @itemID = 0) AND 
		(Transactions.StationID = @stationID OR @stationID = 0) AND 
		(Transactions.RegionID = @regionID OR @regionID = 0) AND 
		DateTime BETWEEN @startDate AND @endDate)
	ORDER BY DateTime DESC

	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.14"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'TransGetBySingle'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.15")) < 0)
                    {
                        #region Update TransGetByOwnersAndSingle stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'TransGetByOwnersAndSingle') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.TransGetByOwnersAndSingle\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.TransGetByOwnersAndSingle 
	@accessParams		varchar(max),
	@itemID				int,
	@stationID			int,
	@regionID			int,
	@startDate			datetime,
	@endDate			datetime,
	@transType			char(4)
AS
	
	SELECT DISTINCT Transactions.*
	FROM Transactions
	JOIN CLR_financelist_split(@accessParams) a ON(
		(@transType != 'sell' AND (Transactions.BuyerID = a.ownerID OR Transactions.BuyerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.BuyerWalletID = a.walletID1 OR Transactions.BuyerWalletID = a.walletID2 OR Transactions.BuyerWalletID = a.walletID3 OR Transactions.BuyerWalletID = a.walletID4 OR Transactions.BuyerWalletID = a.walletID5 OR Transactions.BuyerWalletID = a.walletID6))) OR 
		(@transType != 'buy' AND (Transactions.SellerID = a.ownerID OR Transactions.SellerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.SellerWalletID = a.walletID1 OR Transactions.SellerWalletID = a.walletID2 OR Transactions.SellerWalletID = a.walletID3 OR Transactions.SellerWalletID = a.walletID4 OR Transactions.SellerWalletID = a.walletID5 OR Transactions.SellerWalletID = a.walletID6))) OR a.ownerID = 0)
	WHERE ((Transactions.ItemID = @itemID OR @itemID = 0) AND (Transactions.StationID = @stationID OR @stationID = 0) AND (Transactions.RegionID = @regionID OR @regionID = 0) AND DateTime BETWEEN @startDate AND @endDate)
	ORDER BY DateTime DESC
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.15"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'TransGetByOwnersAndSingle'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.16")) < 0)
                    {
                        #region Update AssetsGetResultsPage stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'TransGetByAny') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.TransGetByAny\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.TransGetByAny
	@accessParams		varchar(max),
	@itemIDs			varchar(max),
	@stationIDs			varchar(max),
	@regionIDs			varchar(max),
	@startDate			datetime,
	@endDate			datetime,
	@transType			char(4)
AS
	--DECLARE @time datetime

	--SET @time = GETDATE()
	
IF(@regionIDs LIKE '' OR @regionIDs LIKE '0')
BEGIN
	SELECT DISTINCT Transactions.*
	FROM Transactions
	JOIN CLR_financelist_split(@accessParams) a ON(
		(@transType != 'sell' AND (Transactions.BuyerID = a.ownerID OR Transactions.BuyerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.BuyerWalletID = a.walletID1 OR Transactions.BuyerWalletID = a.walletID2 OR Transactions.BuyerWalletID = a.walletID3 OR Transactions.BuyerWalletID = a.walletID4 OR Transactions.BuyerWalletID = a.walletID5 OR Transactions.BuyerWalletID = a.walletID6))) OR 
		(@transType != 'buy' AND (Transactions.SellerID = a.ownerID OR Transactions.SellerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.SellerWalletID = a.walletID1 OR Transactions.SellerWalletID = a.walletID2 OR Transactions.SellerWalletID = a.walletID3 OR Transactions.SellerWalletID = a.walletID4 OR Transactions.SellerWalletID = a.walletID5 OR Transactions.SellerWalletID = a.walletID6))) OR a.ownerID = 0)
	JOIN CLR_intlist_split(@itemIDs) i ON (Transactions.ItemID = i.number OR i.number = 0)
	JOIN CLR_intlist_split(@stationIDs) s ON (Transactions.StationID = s.number OR s.number = 0)
	WHERE (DateTime BETWEEN @startDate AND @endDate)
	ORDER BY DateTime DESC
END
ELSE
BEGIN
	SELECT DISTINCT Transactions.*
	FROM Transactions 
	JOIN CLR_financelist_split(@accessParams) a ON(
		(@transType != 'sell' AND (Transactions.BuyerID = a.ownerID OR Transactions.BuyerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.BuyerWalletID = a.walletID1 OR Transactions.BuyerWalletID = a.walletID2 OR Transactions.BuyerWalletID = a.walletID3 OR Transactions.BuyerWalletID = a.walletID4 OR Transactions.BuyerWalletID = a.walletID5 OR Transactions.BuyerWalletID = a.walletID6))) OR 
		(@transType != 'buy' AND (Transactions.SellerID = a.ownerID OR Transactions.SellerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.SellerWalletID = a.walletID1 OR Transactions.SellerWalletID = a.walletID2 OR Transactions.SellerWalletID = a.walletID3 OR Transactions.SellerWalletID = a.walletID4 OR Transactions.SellerWalletID = a.walletID5 OR Transactions.SellerWalletID = a.walletID6))) OR a.ownerID = 0)
	JOIN CLR_intlist_split(@itemIDs) i ON (Transactions.ItemID = i.number OR i.number = 0)
	JOIN CLR_intlist_split(@regionIDs) r ON (Transactions.RegionID = r.number OR r.number = 0)
	WHERE (DateTime BETWEEN @startDate AND @endDate)
	ORDER BY DateTime DESC
END
	
	RETURN --Datediff(ms, @time, GETDATE())";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.16"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'TransGetByAny'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.17")) < 0)
                    {
                        #region Update ContractsGetByPickupAndDest stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ContractsGetByPickupAndDest') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ContractsGetByPickupAndDest\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.ContractsGetByPickupAndDest 
	@ownerIDs	varchar(max),
	@pickupID	int,
	@destID		int,
	@status		smallint,
	@type		smallint
AS
	SELECT Contracts.* 
	FROM Contracts
	JOIN CLR_intlist_split(@ownerIDs) o ON (Contracts.OwnerID = o.number OR o.number = 0)
	WHERE (PickupStationID = @pickupID OR @pickupID = 0) AND (DestinationStationID = @destID OR @destID = 0) AND 
		(Status = @status OR @status = 0) AND (Type = @type OR @type = 0)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.17"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ContractsGetByPickupAndDest'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.18")) < 0)
                    {
                        #region Update ContractsGetByItem stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ContractsGetByItem') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ContractsGetByItem\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.ContractsGetByItem 
	@itemID			int,
	@destinationID	int,
	@minDate		datetime,
	@type			smallint	
AS
	SELECT Contracts.*
	FROM Contracts
	JOIN (
		SELECT ContractItem.*
		FROM ContractItem
		WHERE ItemID = @itemID AND DateTime > @minDate
	) c ON Contracts.ID = c.ContractID
	WHERE Contracts.DestinationStationID = @destinationID AND (Contracts.Status = 1 OR Contracts.Status = 2) AND
		(Contracts.Type = @type OR @type = 0)
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.18"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ContractsGetByItem'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.19")) < 0)
                    {
                        #region Update ContractsGetByStatus stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'ContractsGetByStatus') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.ContractsGetByStatus\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.ContractsGetByStatus
	@ownerIDs	varchar(max),
	@status		smallint,
	@type		smallint
AS
	SELECT Contracts.*
	FROM  Contracts
	JOIN CLR_intlist_split(@ownerIDs) o ON (Contracts.OwnerID = o.number OR o.number = 0) AND
		(Contracts.Type = @type OR @type = 0)
	WHERE Status = @status
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.19"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'ContractsGetByStatus'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.21")) < 0)
                    {
                        #region Clear AssetsHistory

                        commandText = @"DELETE FROM AssetsHistory";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.21"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Clear 'AssetsHistory' table.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.22")) < 0)
                    {
                        #region Create TransBuildResults stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'TransBuildResults') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.TransBuildResults\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE [dbo].[TransBuildResults]
	@accessParams		varchar(max),
	@itemIDs			varchar(max),
	@stationIDs			varchar(max),
	@regionIDs			varchar(max),
	@startDate			datetime,
	@endDate			datetime,
	@transType			char(4)
    --@totalRows          int         OUTPUT
AS
SET NOCOUNT ON

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_TransResults]') AND type in (N'U'))
DROP TABLE [dbo].[tmp_TransResults]

IF(@transType = 'sell')
BEGIN	
	SELECT Transactions.*, row_number() OVER(ORDER BY Transactions.DateTime DESC) as RowNumber
	INTO tmp_TransResults
	FROM Transactions
		JOIN CLR_financelist_split(@accessParams) a ON
			((Transactions.SellerID = a.ownerID OR Transactions.SellerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.SellerWalletID = a.walletID1 OR Transactions.SellerWalletID = a.walletID2 OR Transactions.SellerWalletID = a.walletID3 OR Transactions.SellerWalletID = a.walletID4 OR Transactions.SellerWalletID = a.walletID5 OR Transactions.SellerWalletID = a.walletID6))) 
		JOIN CLR_intlist_split(@itemIDs) i ON (Transactions.ItemID = i.number OR i.number = 0)
		JOIN CLR_intlist_split(@stationIDs) s ON (Transactions.StationID = s.number OR s.number = 0)
		JOIN CLR_intlist_split(@regionIDs) r ON (Transactions.RegionID = r.number OR r.number = 0)
	WHERE (DateTime BETWEEN @startDate AND @endDate)
	
END
ELSE IF(@transType = 'buy')
BEGIN
	SELECT Transactions.*, row_number() OVER(ORDER BY Transactions.DateTime DESC) as RowNumber
	INTO tmp_TransResults
	FROM Transactions
		JOIN CLR_financelist_split(@accessParams) a ON
			((Transactions.BuyerID = a.ownerID OR Transactions.BuyerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.BuyerWalletID = a.walletID1 OR Transactions.BuyerWalletID = a.walletID2 OR Transactions.BuyerWalletID = a.walletID3 OR Transactions.BuyerWalletID = a.walletID4 OR Transactions.BuyerWalletID = a.walletID5 OR Transactions.BuyerWalletID = a.walletID6))) 
		JOIN CLR_intlist_split(@itemIDs) i ON (Transactions.ItemID = i.number OR i.number = 0)
		JOIN CLR_intlist_split(@stationIDs) s ON (Transactions.StationID = s.number OR s.number = 0)
		JOIN CLR_intlist_split(@regionIDs) r ON (Transactions.RegionID = r.number OR r.number = 0)
	WHERE (DateTime BETWEEN @startDate AND @endDate)
END
ELSE
BEGIN
	SELECT Transactions.*, row_number() OVER(ORDER BY Transactions.DateTime DESC) as RowNumber
	INTO tmp_TransResults
	FROM Transactions
		JOIN CLR_financelist_split(@accessParams) a ON
        (a.ownerID = 0 OR
		((Transactions.BuyerID = a.ownerID OR Transactions.BuyerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.BuyerWalletID = a.walletID1 OR Transactions.BuyerWalletID = a.walletID2 OR Transactions.BuyerWalletID = a.walletID3 OR Transactions.BuyerWalletID = a.walletID4 OR Transactions.BuyerWalletID = a.walletID5 OR Transactions.BuyerWalletID = a.walletID6))) OR 
		((Transactions.SellerID = a.ownerID OR Transactions.SellerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.SellerWalletID = a.walletID1 OR Transactions.SellerWalletID = a.walletID2 OR Transactions.SellerWalletID = a.walletID3 OR Transactions.SellerWalletID = a.walletID4 OR Transactions.SellerWalletID = a.walletID5 OR Transactions.SellerWalletID = a.walletID6))))
		JOIN CLR_intlist_split(@itemIDs) i ON (Transactions.ItemID = i.number OR i.number = 0)
		JOIN CLR_intlist_split(@stationIDs) s ON (Transactions.StationID = s.number OR s.number = 0)
		JOIN CLR_intlist_split(@regionIDs) r ON (Transactions.RegionID = r.number OR r.number = 0)
	WHERE (DateTime BETWEEN @startDate AND @endDate)
END

CREATE CLUSTERED INDEX [PK_tmp_TransResults] ON [dbo].[tmp_TransResults] 
(
	[RowNumber]
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF) ON [PRIMARY]	
 
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.22"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'TransBuildResults'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.23")) < 0)
                    {
                        #region Create TransNew stored procedure
                        commandText =
                                "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                "'TransNew') AND type LIKE 'P')\r\n" +
                                "BEGIN\r\n" +
                                "DROP PROCEDURE dbo.TransNew\r\n" +
                                "END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText = @"CREATE PROCEDURE dbo.TransNew
	@datetime		datetime,
	@quantity		int,
	@itemID			int,
	@price			decimal(18, 2),
	@buyerID		int,
	@sellerID		int,
	@buyerCharID	int,
	@sellerCharID	int,
	@stationID		int,
	@regionID		int,
	@buyerForCorp	bit,
	@sellerForCorp	bit,
	@buyerWalletID	smallint,
	@sellerWalletID	smallint,
	@newID			bigint		OUTPUT
AS
	SELECT @newID =
	(SELECT MAX(ID) AS MaxID
		FROM Transactions
		WHERE ID >= 9000000000000000000
		) + 1
		
	IF(@newID IS NULL OR @newID < 9000000000000000000)
	BEGIN
		SET @newID = 9000000000000000000
	END
	
	INSERT INTO Transactions (ID, DateTime, Quantity, ItemID, Price, BuyerID, SellerID, 
		BuyerCharacterID, SellerCharacterID, StationID, RegionID, BuyerForCorp, SellerForCorp, 
		BuyerWalletID, SellerWalletID)
	VALUES (@newID, @datetime, @quantity, @itemID, @price, @buyerID, @sellerID, @buyerCharID, @sellerCharID,
		@stationID, @regionID, @buyerForCorp, @sellerForCorp, @buyerWalletID, @sellerWalletID)

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.23"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to create stored procedure 'TransNew'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.24")) < 0)
                    {
                        #region Add 'ForcePrice' column to ContractItem table
                        commandText =
                                "ALTER TABLE dbo.ContractItem\r\n" +
                                "ADD ForcePrice bit NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.24"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'ForcePrice' column to ContractItem table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.26")) < 0)
                    {
                        #region Add 'TransactionID' column to ContractItem table
                        commandText =
                                @"
    Declare @Qry1		Varchar(1000)
    Declare @DFName		Varchar(500)

    -- All this is just to find and remove the default constraint if there is an existing column...
    -- Taken from: http://www.databasejournal.com/img/DropColumn.txt

	-- Check to see that the column already exist
	IF (SELECT COLUMNPROPERTY( OBJECT_ID('ContractItem'),'TransactionID','AllowsNull')) IS NOT NULL
	BEGIN
		IF Exists (	SELECT syscolumns.* FROM syscolumns INNER JOIN sysobjects
				ON syscolumns.id = sysobjects.id
				INNER JOIN sysobjects so
				ON syscolumns.cdefault = so.id
				WHERE sysobjects.name = 'ContractItem'
				AND syscolumns.name = 'TransactionID'
				AND syscolumns.cdefault <> 0
			  )
		BEGIN
			SELECT @DFName = so.name
			FROM syscolumns INNER JOIN sysobjects ON syscolumns.id = sysobjects.id
			INNER JOIN sysobjects so ON syscolumns.cdefault = so.id
			WHERE sysobjects.name = 'ContractItem'
			AND syscolumns.name = 'TransactionID'
			AND syscolumns.cdefault <> 0

			SET @Qry1 = 'ALTER TABLE ContractItem DROP Constraint ' + @DFName
			Print 'Dropping Default constraint ' + @DFName
			Exec(@Qry1)

			IF @@Error <> 0
			BEGIN
				Raiserror('ERROR : Failed to Drop constraint %s From ContractItem Table.',16,1, 
@DFName)
				Rollback Tran
			END
			ELSE
				Print 'Default ' + @DFName + ' Dropped from ContractItem Table.'
		END
 
        -- Now we can actually drop the column.
        ALTER TABLE ContractItem
        DROP COLUMN TransactionID
    END";
                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        commandText =
                                "ALTER TABLE dbo.ContractItem\r\n" +
                                "ADD TransactionID bigint NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.3.0.26"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'TransactionID' column to ContractItem table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.0.27")) < 0)
                    {
                        #region Update ContractItemsNew stored procedure
                        commandText = @"ALTER PROCEDURE dbo.ContractItemsNew 
	@contractID		bigint,
	@itemID			int,
	@quantity		int,
	@buyPrice		decimal(18, 2),
	@sellPrice		decimal(18, 2),
	@issueDate		datetime,
    @transactionID  bigint,
    @forcePrice     bit
AS

	INSERT INTO ContractItem (ContractID, ItemID, Quantity, BuyPrice, SellPrice, DateTime, TransactionID, ForcePrice)
	VALUES (@contractID, @itemID, @quantity, @buyPrice, @sellPrice, @issueDate, @transactionID, @forcePrice)

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.3.0.27"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'ContractItemsNew'.", ex);
                        }
                        #endregion
                    }
                    // Note: 1.3.0.28 did exist and was the same 1.3.2.1 but missing the removal of constraints 
                    // that is required for SQL 2008 databases. 
                    if (dbVersion.CompareTo(new Version("1.3.2.1")) < 0)
                    {
                        #region Change type various columns on APICharacters table from int to bigint
                        commandText = @"alter table dbo.APICharacters 
drop Constraint DF_APICharacters_HighestCharJournalID
alter table dbo.APICharacters 
drop Constraint DF_APICharacters_HighestCharTransID
alter table dbo.APICharacters 
drop Constraint DF_APICharacters_HighestCorpJournalID
alter table dbo.APICharacters 
drop Constraint DF_APICharacters_HighestCorpTransID
alter table dbo.APICharacters 
drop Constraint DF_APICharacters_LastCorpJournalUpdate
alter table dbo.APICharacters 
drop Constraint DF_APICharacters_LastCorpTransUpdate
alter table dbo.APICharacters 
drop Constraint DF_APICharacters_LastCharJournalUpdate";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            // If the error contains the text "is not a constraint" then the constraints
                            // we are trying to remove are not in the database anyway so just ignore it.
                            if (!ex.Message.Contains("is not a constraint"))
                            {
                                throw new EMMADataException(ExceptionSeverity.Critical,
                                    "Problem removing contraints from 'APICharacters' table", ex);
                            }
                        }

                        commandText =
                                @"ALTER TABLE dbo.APICharacters
                    ALTER COLUMN HighestCharTransID bigint NOT NULL;
                    ALTER TABLE dbo.APICharacters
                    ALTER COLUMN HighestCorpTransID bigint NOT NULL;
                    ALTER TABLE dbo.APICharacters
                    ALTER COLUMN HighestCharJournalID bigint NOT NULL;
                    ALTER TABLE dbo.APICharacters
                    ALTER COLUMN HighestCorpJournalID bigint NOT NULL;";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem changing type of APICharaters' columns to bigint.", ex);
                        }

                        commandText =
                                @"ALTER PROCEDURE dbo.APICharInsert 
	@ID						int,
	@LastCharSheetUpdate	datetime,
	@CharSheet				xml,
	@LastCorpSheetUpdate	datetime,
	@CorpSheet				xml,
	@LastCharTransUpdate	datetime,
	@LastCorpTransUpdate	datetime,
	@LastCharJournalUpdate	datetime,
	@LastCorpJournalUpdate	datetime,
	@LastCharAssetsUpdate	datetime,
	@LastCorpAssetsUpdate	datetime,
	@LastCharOrdersUpdate	datetime,
	@LastCorpOrdersUpdate	datetime,
	@CorpFinanceAccess		bit,
	@HighestCharTransID		bigint,
	@HighestCorpTransID		bigint,
	@HighestCharJournalID	bigint,
	@HighestCorpJournalID	bigint
AS
	INSERT INTO [APICharacters] ([ID], [LastCharSheetUpdate], [CharSheet], [LastCorpSheetUpdate], [CorpSheet], [LastCharTransUpdate], [LastCorpTransUpdate], [LastCharJournalUpdate], [LastCorpJournalUpdate], [LastCharAssetsUpdate], [LastCorpAssetsUpdate], [LastCharOrdersUpdate], [LastCorpOrdersUpdate], [CorpFinanceAccess], [HighestCharTransID], [HighestCorpTransID], [HighestCharJournalID], [HighestCorpJournalID]) VALUES (@ID, @LastCharSheetUpdate, @CharSheet, @LastCorpSheetUpdate, @CorpSheet, @LastCharTransUpdate, @LastCorpTransUpdate, @LastCharJournalUpdate, @LastCorpJournalUpdate, @LastCharAssetsUpdate, @LastCorpAssetsUpdate, @LastCharOrdersUpdate, @LastCorpOrdersUpdate, @CorpFinanceAccess, @HighestCharTransID, @HighestCorpTransID, @HighestCharJournalID, @HighestCorpJournalID);
	SELECT * 
	FROM APICharacters 
	WHERE (ID = @ID) 
	RETURN";
                        adapter = new SqlDataAdapter(commandText, connection);


                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'APICharInsert'.", ex);
                        }

                        commandText =
            @"ALTER PROCEDURE dbo.APICharUpdate 
	@ID						int,
	@LastCharSheetUpdate	datetime,
	@CharSheet				xml,
	@LastCorpSheetUpdate	datetime,
	@CorpSheet				xml,
	@LastCharTransUpdate	datetime,
	@LastCorpTransUpdate	datetime,
	@LastCharJournalUpdate	datetime,
	@LastCorpJournalUpdate	datetime,
	@LastCharAssetsUpdate	datetime,
	@LastCorpAssetsUpdate	datetime,
	@LastCharOrdersUpdate	datetime,
	@LastCorpOrdersUpdate	datetime,	
	@CorpFinanceAccess		bit,
	@HighestCharTransID		bigint,
	@HighestCorpTransID		bigint,
	@HighestCharJournalID	bigint,
	@HighestCorpJournalID	bigint,
	@Original_ID			int
AS

	UPDATE [APICharacters] SET [ID] = @ID, [LastCharSheetUpdate] = @LastCharSheetUpdate, [CharSheet] = @CharSheet, [LastCorpSheetUpdate] = @LastCorpSheetUpdate, [CorpSheet] = @CorpSheet, [LastCharTransUpdate] = @LastCharTransUpdate, [LastCorpTransUpdate] = @LastCorpTransUpdate, [LastCharJournalUpdate] = @LastCharJournalUpdate, [LastCorpJournalUpdate] = @LastCorpJournalUpdate, [LastCharAssetsUpdate] = @LastCharAssetsUpdate, [LastCorpAssetsUpdate] = @LastCorpAssetsUpdate, [LastCharOrdersUpdate] = @LastCharOrdersUpdate, [LastCorpOrdersUpdate] = @LastCorpOrdersUpdate, [CorpFinanceAccess] = @CorpFinanceAccess, [HighestCharTransID] = @HighestCharTransID, [HighestCorpTransID] = @HighestCorpTransID, [HighestCharJournalID] = @HighestCharJournalID, [HighestCorpJournalID] = @HighestCorpJournalID			 
	WHERE ([ID] = @Original_ID);
	
	SELECT * 
	FROM APICharacters 
	WHERE (ID = @ID)
	RETURN";
                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.3.2.1"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Unable to update stored procedure 'APICharUpdate'.", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.2.2")) < 0)
                    {
                        #region Create 'ItemValues' table, move data from 'TradedItems' to 'ItemValues' then drop 'TradedItems'
                        commandText = @"SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[ItemValues](
	[ReportGroupID] [int] NOT NULL,
	[ItemID] [int] NOT NULL,
	[RegionID] [int] NOT NULL,
	[DefaultSellPrice] [numeric](18, 2) NOT NULL,
	[CurrentSellPrice] [numeric](18, 2) NOT NULL,
	[LastSellPriceCalc] [datetime] NOT NULL,
	[CurrentBuyPrice] [numeric](18, 2) NOT NULL,
	[LastBuyPriceCalc] [datetime] NOT NULL,
	[DefaultBuyPrice] [numeric](18, 2) NOT NULL,
	[UseReprocessVal] [bit] NOT NULL,
	[ForceDefaultSellPrice] [bit] NOT NULL,
	[ForceDefaultBuyPrice] [bit] NOT NULL,
 CONSTRAINT [PK_ItemValues] PRIMARY KEY CLUSTERED 
(
	[ReportGroupID] ASC,
	[RegionID] ASC,
	[ItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[ItemValues] ADD  CONSTRAINT [DF_ItemValues_OwnerID]  DEFAULT ((0)) FOR [ReportGroupID]

ALTER TABLE [dbo].[ItemValues] ADD  CONSTRAINT [DF_ItemValues_CurrentSellPrice]  DEFAULT ((0)) FOR [CurrentSellPrice]

ALTER TABLE [dbo].[ItemValues] ADD  CONSTRAINT [DF_ItemValues_CurrentBuyPrice]  DEFAULT ((0)) FOR [CurrentBuyPrice]

ALTER TABLE [dbo].[ItemValues] ADD  CONSTRAINT [DF_ItemValues_LastBuyPriceCalc]  DEFAULT (((1)/(1))/(2000)) FOR [LastBuyPriceCalc]

ALTER TABLE [dbo].[ItemValues] ADD  DEFAULT ((0)) FOR [DefaultBuyPrice]

ALTER TABLE [dbo].[ItemValues] ADD  DEFAULT ((0)) FOR [UseReprocessVal]

ALTER TABLE [dbo].[ItemValues] ADD  DEFAULT ((0)) FOR [ForceDefaultSellPrice]

ALTER TABLE [dbo].[ItemValues] ADD  DEFAULT ((0)) FOR [ForceDefaultBuyPrice]";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.3.2.2"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'ItemValues' table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.2.3")) < 0)
                    {
                        #region Move data from 'TradedItems' to 'ItemValues'

                        commandText =
                                @"INSERT INTO [ItemValues]
           ([ReportGroupID]
           ,[ItemID]
           ,[RegionID]
           ,[DefaultSellPrice]
           ,[CurrentSellPrice]
           ,[LastSellPriceCalc]
           ,[CurrentBuyPrice]
           ,[LastBuyPriceCalc]
           ,[DefaultBuyPrice]
           ,[UseReprocessVal]
           ,[ForceDefaultSellPrice]
           ,[ForceDefaultBuyPrice])
SELECT * FROM TradedItems";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.3.2.3"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem moving data from 'TradedItems' to 'ItemValues'", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.2.4")) < 0)
                    {
                        #region Create ItemValueGetData stored procedure

                        commandText =
                                @"CREATE PROCEDURE [dbo].[ItemValueGetData]
	@reportGroupID	int, 
	@regionID		int,
	@itemID			int
AS
	SELECT ItemValues.*
	FROM ItemValues
	WHERE ReportGroupID = @reportGroupID AND (RegionID = @regionID OR @regionID = 0) AND (ItemID = @itemID OR @itemID = 0)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.3.2.4"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'ItemValueGetData' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.2.5")) < 0)
                    {
                        #region Create ItemValuesClear stored procedure

                        commandText = @"CREATE PROCEDURE [dbo].[ItemValuesClear]
	@reportGroupID	int
AS
	DELETE FROM ItemValues
	WHERE ReportGroupID = @reportGroupID
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.3.2.2"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'ItemValuesClear' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.2.6")) < 0)
                    {
                        #region Drop TradedItems table

                        commandText =
                                @"DROP TABLE TradedItems";
                        adapter = new SqlDataAdapter(commandText, connection);


                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.3.2.6"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem dropping table 'TradedItems'", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.2.7")) < 0)
                    {
                        #region Create new 'TradedItems' table
                        commandText = @"SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[TradedItems](
	[ReportGroupID] [int] NOT NULL,
	[ItemID] [int] NOT NULL,
 CONSTRAINT [PK_TradedItems] PRIMARY KEY CLUSTERED 
(
	[ReportGroupID] ASC,
	[ItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.3.2.7"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'TradedItems' table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.2.8")) < 0)
                    {
                        #region Update 'TradedItemGet' stored proc

                        commandText = @"ALTER PROCEDURE [dbo].[TradedItemGet]
	@reportGroupID	int,
	@itemID			int
AS
	SELECT TradedItems.*
	FROM TradedItems
	WHERE ReportGroupID = @reportGroupID AND (ItemID = @itemID OR @itemID = 0)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.3.2.8"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'TradedItemGet' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.3.2.9")) < 0)
                    {
                        #region Create 'AssetsLost' table
                        commandText = @"SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[AssetsLost](
	[ID] [bigint] NOT NULL,
	[OwnerID] [int] NOT NULL,
	[CorpAsset] [bit] NOT NULL,
	[ItemID] [int] NOT NULL,
	[LossDatetime] [datetime] NOT NULL,
	[Quantity] [bigint] NOT NULL,
 CONSTRAINT [PK_AssetsLost] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.3.2.9"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetsLost' table", ex);
                        }
                        #endregion
                    }
                    #endregion
                }
                if (dbVersion.CompareTo(new Version("1.4.2.11")) < 0)
                {
                    #region 1.4.1.0 - 1.4.2.11
                    if (dbVersion.CompareTo(new Version("1.4.1.0")) < 0)
                    {
                        #region Create 'AssetsProduced' table
                        commandText = @"SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[AssetsProduced](
	[ID] [bigint] NOT NULL,
	[OwnerID] [int] NOT NULL,
	[CorpAsset] [bit] NOT NULL,
	[ItemID] [int] NOT NULL,
	[ProductionDateTime] [datetime] NOT NULL,
	[Cost] [numeric](18, 2) NOT NULL,
 CONSTRAINT [PK_AssetsProduced] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.4.1.0"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetsProduced' table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.1")) < 0)
                    {
                        #region Update 'AssetsGetAutoConByAny' stored procedure
                        commandText = @"ALTER PROCEDURE dbo.AssetsGetAutoConByAny
	@ownerID			int,
	@corpAsset			bit,
	@stationIDs			varchar(max),
	@regionIDs			varchar(max),
	@itemIDs			varchar(max),
	@excludeContainers	bit
AS

	SELECT Assets.*
	FROM Assets
	JOIN CLR_intlist_split(@stationIDs) s ON (Assets.LocationID = s.number OR s.number = 0)
	JOIN CLR_intlist_split(@regionIDs) r ON (Assets.RegionID = r.number OR r.number = 0)
	JOIN CLR_intlist_split(@itemIDs) i ON (Assets.ItemID = i.number OR i.number = 0)
	WHERE (OwnerID = @ownerID AND CorpAsset = @corpAsset AND 
		(AutoConExclude = 0) AND (Status = 1) AND (@excludeContainers = 0 OR (ContainerID = 0 AND IsContainer = 0)) AND Quantity > 0)
	ORDER BY LocationID
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.4.1.1"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetAutoConByAny' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.2")) < 0)
                    {
                        #region Update 'AssetsGetAutoConByOwner' stored procedure
                        commandText = @"ALTER PROCEDURE dbo.AssetsGetAutoConByOwner
	@ownerID			int,
	@corpAsset			bit,
	@stationID			int,
	@itemIDs			varchar(max),
	@excludeContainers	bit
AS

	SELECT Assets.*
	FROM Assets
	JOIN CLR_intlist_split(@itemIDs) i ON (Assets.ItemID = i.number OR i.number = 0)
	WHERE (OwnerID = @ownerID AND CorpAsset = @corpAsset AND (LocationID = @stationID OR @stationID = 0) AND (AutoConExclude = 0) AND (Status = 1) AND (@excludeContainers = 0 OR (ContainerID = 0 AND IsContainer = 0)) AND (Quantity > 0))
	ORDER BY LocationID
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.4.1.2"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetAutoConByOwner' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.4")) < 0)
                    {
                        #region Add 'Quantity' column to AssetsProduced table
                        commandText =
                                "ALTER TABLE dbo.AssetsProduced\r\n" +
                                "ADD Quantity bigint NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.4"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'Quantity' column to AssetsProduced table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.7")) < 0)
                    {
                        #region Create 'AssetsLostNew' stored procedure
                        commandText = @"CREATE PROCEDURE dbo.AssetsLostNew
	@OwnerID			int,
	@CorpAsset			bit,
	@ItemID				int,
	@LossDateTime	    datetime,
	@Quantity			int,
	@newID				bigint		OUTPUT
AS
	SELECT @newID =
	(SELECT MAX(ID) AS MaxID
		FROM AssetsLost) + 1
		
	IF(@newID IS NULL)
	BEGIN
		SET @newID = 1
	END
	
	INSERT INTO AssetsLost (ID, OwnerID, CorpAsset, ItemID, LossDateTime, Quantity)
	VALUES (@newID, @OwnerID, @CorpAsset, @ItemID, @LossDateTime, @Quantity)

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.4.1.7"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetsLostNew' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.10")) < 0)
                    {
                        #region Create 'AssetsProducedNew' stored procedure
                        commandText = @"CREATE PROCEDURE dbo.AssetsProducedNew
	@OwnerID			int,
	@CorpAsset			bit,
	@ItemID				int,
	@ProductionDateTime	datetime,
	@Cost				decimal(18, 2),
    @Quantity           bigint,
	@newID				bigint		OUTPUT
AS
	SELECT @newID =
	(SELECT MAX(ID) AS MaxID
		FROM AssetsProduced) + 1
		
	IF(@newID IS NULL)
	BEGIN
		SET @newID = 1
	END
	
	INSERT INTO AssetsProduced (ID, OwnerID, CorpAsset, ItemID, ProductionDateTime, Cost, Quantity)
	VALUES (@newID, @OwnerID, @CorpAsset, @ItemID, @ProductionDateTime, @Cost, @Quantity)

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                            SetDBVersion(connection, new Version("1.4.1.10"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetsProducedNew' stored procedure", ex);
                        }
                        #endregion
                    }


                    if (dbVersion.CompareTo(new Version("1.4.1.12")) < 0)
                    {
                        #region Add 'Cost' column to Assets table
                        commandText =
                                @"ALTER TABLE dbo.Assets
ADD Cost decimal(18, 2) NOT NULL DEFAULT 0

ALTER TABLE dbo.Assets
ADD CostCalc bit NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.12"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'Cost' column to Assets table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.13")) < 0)
                    {
                        #region Update 'AssetsAddQuantity' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsAddQuantity 
	@ownerID		int,
	@corpAsset		bit,
	@itemID			int,
	@stationID		int,
	@systemID		int,
	@regionID		int,
	@status			int,
	@containerID	int,
	@autoConExclude	bit,
	@deltaQuantity	bigint,
	@addedItemsCost	decimal(18,2),
    @costCalc       bit
AS
	DECLARE @oldQuantity bigint, @newQuantity bigint
	DECLARE	@assetID bigint
	DECLARE @oldCost decimal(18, 2), @newCost decimal(18, 2)
    DECLARE @oldCostCalc bit, @newCostCalc bit
		
	SET @assetID = 0
	SELECT @oldQuantity = Quantity, @assetID = ID, @oldCost = Cost, @oldCostCalc = CostCalc
	FROM Assets
	WHERE OwnerID = @ownerID AND CorpAsset = @corpAsset AND LocationID = @stationID AND ItemID = @itemID AND Status = @status AND ContainerID = @containerID AND AutoConExclude = @autoConExclude
	
	IF(@assetID = 0)
	BEGIN
		INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [Cost], [CostCalc]) 
		VALUES (@ownerID, @corpAsset, @stationID, @itemID, @systemID, @regionID, 0, @deltaQuantity, @status, @autoConExclude, 0, 0, @addedItemsCost, @costCalc);
	END 
	ELSE
	BEGIN
		SET @newQuantity = @oldQuantity + @deltaQuantity
		IF(@deltaQuantity > 0)
		BEGIN
            -- If new items are being added to the stack then calculate the average cost from the 
            -- old and new values.
            -- If the old cost had not been calculated then just use the new cost
            SET @newCostCalc = 1
            IF(@oldCostCalc = 0 AND @costCalc = 0)
            BEGIN
                SET @newCost = 0
                SET @newCostCalc = 0
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 0)
            BEGIN
                SET @newCost = @oldCost
            END
            ELSE IF(@oldCostCalc = 0 AND @costCalc = 1)
            BEGIN
                SET @newCost = @addedItemsCost
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 1)
            BEGIN
			    SET @newCost = (@oldCost * @oldQuantity + @addedItemsCost * @deltaQuantity) / (@oldQuantity + @deltaQuantity)
		    END
        END
		ELSE
		BEGIN
            -- If items are being removed from the stack then just use the old cost value
			SET @newCost = @oldCost
            SET @newCostCalc = @oldCostCalc
		END		
		
		UPDATE [Assets] SET [Quantity] = @newQuantity, [Cost] = @newCost, [CostCalc] = @newCostCalc
		WHERE [OwnerID] = @ownerID AND [CorpAsset] = @corpAsset AND [LocationID] = @stationID AND [ItemID] = @itemID AND [Status] = @status AND [ContainerID] = @containerID AND [AutoConExclude] = @autoConExclude
	END
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.13"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsAddQuantity' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.14")) < 0)
                    {
                        #region Update 'AssetsBuildResults' stored procedure
                        commandText =
                                @"ALTER PROCEDURE [dbo].[AssetsBuildResults]
	@accessList			varchar(max),
	@itemIDs			varchar(max),
	@status				int,
	@groupBy			varchar(50)
    --@totalRows          int         OUTPUT
AS
SET NOCOUNT ON

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_AssetResults]') AND type in (N'U'))
DROP TABLE [dbo].[tmp_AssetResults]

IF(@groupBy LIKE 'Owner')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, 0 AS SystemID, 
		0 AS RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		SUM(Assets.Cost * Assets.Quantity * Assets.CostCalc) / SUM(Assets.Quantity * Assets.CostCalc) AS Cost,
		MAX(CAST(Assets.CostCalc as int)) AS CostCalc,
        row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0)
	GROUP BY Assets.ItemID, Assets.OwnerID, Assets.CorpAsset
END
ELSE IF(@groupBy LIKE 'Region')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, 0 AS SystemID, 
		Assets.RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		SUM(Assets.Cost * Assets.Quantity * Assets.CostCalc) / SUM(Assets.Quantity * Assets.CostCalc) AS Cost,
		MAX(CAST(Assets.CostCalc as int)) AS CostCalc,
		row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0)
	GROUP BY Assets.ItemID, Assets.RegionID, Assets.OwnerID, Assets.CorpAsset
END
ELSE IF(@groupBy LIKE 'System')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, Assets.SystemID, 
		MAX(Assets.RegionID) AS RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		SUM(Assets.Cost * Assets.Quantity * Assets.CostCalc) / SUM(Assets.Quantity * Assets.CostCalc) AS Cost,
		MAX(CAST(Assets.CostCalc as int)) AS CostCalc,
		row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0)
	GROUP BY Assets.ItemID, Assets.SystemID, Assets.OwnerID, Assets.CorpAsset
END
ELSE
BEGIN
	SELECT Assets.*, row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0)
END

CREATE CLUSTERED INDEX [PK_tmp_AssetResults] ON [dbo].[tmp_AssetResults] 
(
	[RowNumber]
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF) ON [PRIMARY]	
 
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.14"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsBuildResults' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.15")) < 0)
                    {
                        #region Update 'AssetsGetResultsPage' stored procedure
                        commandText =
                                @"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_AssetResults]') AND type in (N'U'))
DROP TABLE [dbo].[tmp_AssetResults]


CREATE TABLE [dbo].[tmp_AssetResults](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[OwnerID] [int] NOT NULL,
	[CorpAsset] [bit] NOT NULL,
	[LocationID] [int] NOT NULL,
	[ItemID] [int] NOT NULL,
	[SystemID] [int] NOT NULL,
	[RegionID] [int] NOT NULL,
	[ContainerID] [bigint] NOT NULL,
	[Quantity] [bigint] NOT NULL,
	[Status] [int] NOT NULL,
	[AutoConExclude] [bit] NOT NULL,
	[Processed] [bit] NOT NULL,
	[IsContainer] [bit] NOT NULL,
	[ReprocExclude] [bit] NOT NULL,
	[Cost] [decimal](18,2) NOT NULL,
    [CostCalc] [bit] NOT NULL,
	[RowNumber] [bigint] NULL
) ON [PRIMARY]";
                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            commandText =
                                    @"ALTER PROCEDURE [dbo].[AssetsGetResultsPage]
    @startRow           int,
    @pageSize           int
    --@totalRows          int         OUTPUT
AS
SET NOCOUNT ON

	SELECT ID, OwnerID, CorpAsset, LocationID, ItemID, SystemID, RegionID, ContainerID,
		Quantity, Status, AutoConExclude, Processed, IsContainer, ReprocExclude, Cost, CostCalc
	FROM tmp_AssetResults
	WHERE RowNumber BETWEEN (@startRow) AND (@startRow + @pageSize - 1)
	ORDER BY RowNumber";

                            adapter = new SqlDataAdapter(commandText, connection);
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.15"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetResultsPage' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.16")) < 0)
                    {
                        #region Update 'AssetsInsert' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsInsert
	@OwnerID		int,
	@CorpAsset		bit,
	@LocationID		int,
	@ItemID			int,
	@SystemID		int,
	@RegionID		int,
	@ContainerID	bigint,
	@Quantity		bigint,
	@Status			int,
	@Processed		bit,
	@AutoConExclude	bit,
	@IsContainer	bit,
    @ReprocExclude  bit,
    @Cost			decimal(18, 2),
    @CostCalc       bit,
	@newID			bigint OUT
AS
	INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [ReprocExclude], [Cost], [CostCalc]) 
	VALUES (@OwnerID, @CorpAsset, @LocationID, @ItemID, @SystemID, @RegionID, @ContainerID, @Quantity, @Status, @AutoConExclude, @Processed, @IsContainer, @ReprocExclude, @Cost, @CostCalc);

	SET @newID = SCOPE_IDENTITY()

	SELECT * 
	FROM Assets 
	WHERE (ID = @newID)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.16"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsInsert' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.17")) < 0)
                    {
                        #region Update 'AssetsUpdate' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsUpdate
	@ID				bigint,
	@OwnerID		int,
	@CorpAsset		bit,
	@LocationID		int,
	@ItemID			int,
	@SystemID		int,
	@RegionID		int,
	@ContainerID	bigint,
	@Quantity		bigint,
	@Status			int,
	@Processed		bit,
	@AutoConExclude	bit,
	@IsContainer	bit,
    @ReprocExclude  bit,
    @Cost			decimal(18,2),
    @CostCalc       bit, 
	@Original_ID	bigint
AS
	UPDATE [Assets] SET [OwnerID] = @OwnerID, [CorpAsset] = @CorpAsset, [LocationID] = @LocationID, [ItemID] = @ItemID, [SystemID] = @SystemID, [RegionID] = @RegionID, [ContainerID] = @ContainerID, [Quantity] = @Quantity, [Status] = @Status, [AutoConExclude] = @AutoConExclude, [Processed] = @Processed, [IsContainer] = @IsContainer, [ReprocExclude] = @ReprocExclude, [Cost] = @Cost, [CostCalc] = @CostCalc
	WHERE ([ID] = @Original_ID);
	
	SELECT * 
	FROM Assets 
	WHERE (ID = @ID)
	
	RETURN   ";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.17"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsUpdate' stored procedure", ex);
                        }
                        #endregion
                    }


                    if (dbVersion.CompareTo(new Version("1.4.1.30")) < 0)
                    {
                        #region Add 'SellerUnitProfit' column to Transactions table
                        commandText =
                                @"ALTER TABLE dbo.Transactions
ADD SellerUnitProfit decimal(18, 2) NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.13"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'SellerUnitProfit' column to Transactions table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.31")) < 0)
                    {
                        #region Update 'TransGetResultsPage' stored procedure
                        commandText =
                                @"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_TransResults]') AND type in (N'U'))
DROP TABLE [dbo].[tmp_TransResults]


CREATE TABLE [dbo].[tmp_TransResults](
	[ID] [bigint] NOT NULL,
	[DateTime] [datetime] NOT NULL,
	[Quantity] [int] NOT NULL,
	[ItemID] [int] NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[BuyerID] [int] NOT NULL,
	[SellerID] [int] NOT NULL,
	[BuyerCharacterID] [int] NOT NULL,
	[SellerCharacterID] [int] NOT NULL,
	[StationID] [int] NOT NULL,
	[RegionID] [int] NOT NULL,
	[BuyerForCorp] [bit] NOT NULL,
	[SellerForCorp] [bit] NOT NULL,
	[BuyerWalletID] [smallint] NOT NULL,
	[SellerWalletID] [smallint] NOT NULL,
	[SellerUnitProfit] [decimal](18,2) NOT NULL,
	[RowNumber] [bigint] NULL
) ON [PRIMARY]";

                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            commandText =
                                    @"ALTER PROCEDURE [dbo].[TransGetResultsPage]
    @startRow           int,
    @pageSize           int
AS
SET NOCOUNT ON

	SELECT ID, DateTime, Quantity, ItemID, Price, BuyerID, SellerID, BuyerCharacterID,
		SellerCharacterID, StationID, RegionID, BuyerForCorp, SellerForCorp,
		BuyerWalletID, SellerWalletID, SellerUnitProfit
	FROM tmp_TransResults
	WHERE RowNumber BETWEEN (@startRow) AND (@startRow + @pageSize - 1)
	ORDER BY RowNumber
RETURN";

                            adapter = new SqlDataAdapter(commandText, connection);
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.31"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'TransGetResultsPage' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.32")) < 0)
                    {
                        #region Update 'TransInsert' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.TransInsert 
	@ID					bigint,
	@DateTime			datetime,
	@Quantity			int,
	@ItemID				int,
	@Price				decimal(18,2),
	@BuyerID			int,
	@SellerID			int,
	@BuyerCharacterID	int,
	@SellerCharacterID	int,
	@StationID			int,
	@RegionID			int,
	@BuyerForCorp		bit,
	@SellerForCorp		bit,
	@BuyerWalletID		smallint,
	@SellerWalletID		smallint,
	@SellerUnitProfit	decimal(18,2)
AS
	INSERT INTO [dbo].[Transactions] ([ID], [DateTime], [Quantity], [ItemID], [Price], [BuyerID], [SellerID], [BuyerCharacterID], [SellerCharacterID], [StationID], [RegionID], [BuyerForCorp], [SellerForCorp], [BuyerWalletID], [SellerWalletID], [SellerUnitProfit]) 
	VALUES (@ID, @DateTime, @Quantity, @ItemID, @Price, @BuyerID, @SellerID, @BuyerCharacterID, @SellerCharacterID, @StationID, @RegionID, @BuyerForCorp, @SellerForCorp, @BuyerWalletID, @SellerWalletID, @SellerUnitProfit);
	
	SELECT *
	FROM Transactions 
	WHERE (ID = @ID)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.32"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'TransInsert' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.33")) < 0)
                    {
                        #region Update 'TransNew' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.TransNew
	@datetime		datetime,
	@quantity		int,
	@itemID			int,
	@price			decimal(18, 2),
	@buyerID		int,
	@sellerID		int,
	@buyerCharID	int,
	@sellerCharID	int,
	@stationID		int,
	@regionID		int,
	@buyerForCorp	bit,
	@sellerForCorp	bit,
	@buyerWalletID	smallint,
	@sellerWalletID	smallint,
	@sellerUnitProfit	decimal(18, 2),
	@newID			bigint		OUTPUT
AS
	SELECT @newID =
	(SELECT MAX(ID) AS MaxID
		FROM Transactions
		WHERE ID >= 9000000000000000000
		) + 1
		
	IF(@newID IS NULL OR @newID < 9000000000000000000)
	BEGIN
		SET @newID = 9000000000000000000
	END
	
	INSERT INTO Transactions (ID, DateTime, Quantity, ItemID, Price, BuyerID, SellerID, 
		BuyerCharacterID, SellerCharacterID, StationID, RegionID, BuyerForCorp, SellerForCorp, 
		BuyerWalletID, SellerWalletID, SellerUnitProfit)
	VALUES (@newID, @datetime, @quantity, @itemID, @price, @buyerID, @sellerID, @buyerCharID, @sellerCharID,
		@stationID, @regionID, @buyerForCorp, @sellerForCorp, @buyerWalletID, @sellerWalletID, @sellerUnitProfit)

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.33"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'TransNew' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.1.34")) < 0)
                    {
                        #region Update 'TransUpdate' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.TransUpdate 
	@ID					bigint,
	@DateTime			datetime,
	@Quantity			int,
	@ItemID				int,
	@Price				decimal(18,2),
	@BuyerID			int,
	@SellerID			int,
	@BuyerCharacterID	int,
	@SellerCharacterID	int,
	@StationID			int,
	@RegionID			int,
	@BuyerForCorp		bit,
	@SellerForCorp		bit,
	@BuyerWalletID		smallint,
	@SellerWalletID		smallint,
	@SellerUnitProfit	decimal(18,2),
	@Original_ID		bigint
AS
	UPDATE [Transactions] SET [ID] = @ID, [DateTime] = @DateTime, [Quantity] = @Quantity, [ItemID] = @ItemID, [Price] = @Price, [BuyerID] = @BuyerID, [SellerID] = @SellerID, [BuyerCharacterID] = @BuyerCharacterID, [SellerCharacterID] = @SellerCharacterID, [StationID] = @StationID, [RegionID] = @RegionID, [BuyerForCorp] = @BuyerForCorp, [SellerForCorp] = @SellerForCorp, [BuyerWalletID] = @BuyerWalletID, [SellerWalletID] = @SellerWalletID, [SellerUnitProfit] = @SellerUnitProfit 
	WHERE ([ID] = @Original_ID);

	SELECT *
	FROM Transactions 
	WHERE (ID = @ID)
	
	RETURN
";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.34"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'TransUpdate' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.4.1.40")) < 0)
                    {
                        #region Add 'For Sale Via Contract' state to AssetStatuses table
                        commandText =
                                @"INSERT INTO AssetStatuses ([StatusID], [Description])
VALUES (3, 'For Sale Via Contract                             ')";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.40"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'For Sale Via Contract' state to AssetStatuses table", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.4.1.41")) < 0)
                    {
                        #region Update 'JournalSumAmtByType' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.JournalSumAmtByType 
	@accessParams	varchar(MAX),
	@refType		int,
	@startTime		DateTime,
	@endTime		DateTime,
	@type			varchar(7),	-- 'revenue' or 'expense'. Setting this param to anything else will return both types.
	@sumAmt			decimal(18,2)	OUTPUT
AS
	SELECT @sumAmt = SUM(Journal.Amount)
	FROM Journal
	JOIN CLR_financelist_split(@accessParams) a ON(
		(@type != 'revenue' AND ((Journal.SenderID = a.ownerID AND Journal.SCorpID = 0) OR Journal.SCorpID = a.ownerID) AND (a.walletID1 = 0 OR (Journal.SWalletID = a.walletID1 OR Journal.SWalletID = a.walletID2 OR Journal.SWalletID = a.walletID3 OR Journal.SWalletID = a.walletID4 OR Journal.SWalletID = a.walletID5 OR Journal.SWalletID = a.walletID6))) OR 
		(@type != 'expense' AND ((Journal.RecieverID = a.ownerID AND Journal.RCorpID = 0) OR Journal.RCorpID = a.ownerID) AND (a.walletID1 = 0 OR  (Journal.RWalletID = a.walletID1 OR Journal.RWalletID = a.walletID2 OR Journal.RWalletID = a.walletID3 OR Journal.RWalletID = a.walletID4 OR Journal.RWalletID = a.walletID5 OR Journal.RWalletID = a.walletID6))) OR a.ownerID = 0) 
	WHERE ((Journal.Date > @startTime) AND 
		(Journal.Date <= @endTime) AND 
		(Journal.TypeID = @refType OR @refType = 0))
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.1.41"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'JournalSumAmtByType' stored procedure", ex);
                        }
                        #endregion
                    }


                    if (dbVersion.CompareTo(new Version("1.4.2.0")) < 0)
                    {
                        #region Add 'CalcProfitFromAssets' column to Transactions table
                        commandText =
                                @"ALTER TABLE dbo.Transactions
ADD CalcProfitFromAssets bit NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.2.0"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'CalcProfitFromAssets' column to Transactions table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.2.1")) < 0)
                    {
                        #region Update 'TransGetResultsPage' stored procedure
                        commandText =
                                @"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_TransResults]') AND type in (N'U'))
DROP TABLE [dbo].[tmp_TransResults]


CREATE TABLE [dbo].[tmp_TransResults](
	[ID] [bigint] NOT NULL,
	[DateTime] [datetime] NOT NULL,
	[Quantity] [int] NOT NULL,
	[ItemID] [int] NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[BuyerID] [int] NOT NULL,
	[SellerID] [int] NOT NULL,
	[BuyerCharacterID] [int] NOT NULL,
	[SellerCharacterID] [int] NOT NULL,
	[StationID] [int] NOT NULL,
	[RegionID] [int] NOT NULL,
	[BuyerForCorp] [bit] NOT NULL,
	[SellerForCorp] [bit] NOT NULL,
	[BuyerWalletID] [smallint] NOT NULL,
	[SellerWalletID] [smallint] NOT NULL,
	[SellerUnitProfit] [decimal](18,2) NOT NULL,
    [CalcProfitFromAssets] [bit] NOT NULL,
	[RowNumber] [bigint] NULL
) ON [PRIMARY]";

                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            commandText =
                                    @"ALTER PROCEDURE [dbo].[TransGetResultsPage]
    @startRow           int,
    @pageSize           int
AS
SET NOCOUNT ON

	SELECT ID, DateTime, Quantity, ItemID, Price, BuyerID, SellerID, BuyerCharacterID,
		SellerCharacterID, StationID, RegionID, BuyerForCorp, SellerForCorp,
		BuyerWalletID, SellerWalletID, SellerUnitProfit, CalcProfitFromAssets
	FROM tmp_TransResults
	WHERE RowNumber BETWEEN (@startRow) AND (@startRow + @pageSize - 1)
	ORDER BY RowNumber
RETURN";

                            adapter = new SqlDataAdapter(commandText, connection);
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.2.1"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'TransGetResultsPage' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.2.2")) < 0)
                    {
                        #region Update 'TransInsert' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.TransInsert 
	@ID					bigint,
	@DateTime			datetime,
	@Quantity			int,
	@ItemID				int,
	@Price				decimal(18,2),
	@BuyerID			int,
	@SellerID			int,
	@BuyerCharacterID	int,
	@SellerCharacterID	int,
	@StationID			int,
	@RegionID			int,
	@BuyerForCorp		bit,
	@SellerForCorp		bit,
	@BuyerWalletID		smallint,
	@SellerWalletID		smallint,
	@SellerUnitProfit	decimal(18,2),
    @CalcProfitFromAssets   bit
AS
	INSERT INTO [dbo].[Transactions] ([ID], [DateTime], [Quantity], [ItemID], [Price], [BuyerID], [SellerID], [BuyerCharacterID], [SellerCharacterID], [StationID], [RegionID], [BuyerForCorp], [SellerForCorp], [BuyerWalletID], [SellerWalletID], [SellerUnitProfit], [CalcProfitFromAssets]) 
	VALUES (@ID, @DateTime, @Quantity, @ItemID, @Price, @BuyerID, @SellerID, @BuyerCharacterID, @SellerCharacterID, @StationID, @RegionID, @BuyerForCorp, @SellerForCorp, @BuyerWalletID, @SellerWalletID, @SellerUnitProfit, @CalcProfitFromAssets);
	
	SELECT *
	FROM Transactions 
	WHERE (ID = @ID)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.2.2"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'TransInsert' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.2.3")) < 0)
                    {
                        #region Update 'TransNew' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.TransNew
	@datetime		datetime,
	@quantity		int,
	@itemID			int,
	@price			decimal(18, 2),
	@buyerID		int,
	@sellerID		int,
	@buyerCharID	int,
	@sellerCharID	int,
	@stationID		int,
	@regionID		int,
	@buyerForCorp	bit,
	@sellerForCorp	bit,
	@buyerWalletID	smallint,
	@sellerWalletID	smallint,
	@sellerUnitProfit	decimal(18, 2),
    @calcProfitFromAssets   bit,
	@newID			bigint		OUTPUT
AS
	SELECT @newID =
	(SELECT MAX(ID) AS MaxID
		FROM Transactions
		WHERE ID >= 9000000000000000000
		) + 1
		
	IF(@newID IS NULL OR @newID < 9000000000000000000)
	BEGIN
		SET @newID = 9000000000000000000
	END
	
	INSERT INTO Transactions (ID, DateTime, Quantity, ItemID, Price, BuyerID, SellerID, 
		BuyerCharacterID, SellerCharacterID, StationID, RegionID, BuyerForCorp, SellerForCorp, 
		BuyerWalletID, SellerWalletID, SellerUnitProfit, CalcProfitFromAssets)
	VALUES (@newID, @datetime, @quantity, @itemID, @price, @buyerID, @sellerID, @buyerCharID, @sellerCharID,
		@stationID, @regionID, @buyerForCorp, @sellerForCorp, @buyerWalletID, @sellerWalletID, @sellerUnitProfit, 
        @calcProfitFromAssets)

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.2.3"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'TransNew' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.4.2.4")) < 0)
                    {
                        #region Update 'TransUpdate' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.TransUpdate 
	@ID					bigint,
	@DateTime			datetime,
	@Quantity			int,
	@ItemID				int,
	@Price				decimal(18,2),
	@BuyerID			int,
	@SellerID			int,
	@BuyerCharacterID	int,
	@SellerCharacterID	int,
	@StationID			int,
	@RegionID			int,
	@BuyerForCorp		bit,
	@SellerForCorp		bit,
	@BuyerWalletID		smallint,
	@SellerWalletID		smallint,
	@SellerUnitProfit	decimal(18,2),
    @CalcProfitFromAssets   bit,
	@Original_ID		bigint
AS
	UPDATE [Transactions] SET [ID] = @ID, [DateTime] = @DateTime, [Quantity] = @Quantity, [ItemID] = @ItemID, [Price] = @Price, [BuyerID] = @BuyerID, [SellerID] = @SellerID, [BuyerCharacterID] = @BuyerCharacterID, [SellerCharacterID] = @SellerCharacterID, [StationID] = @StationID, [RegionID] = @RegionID, [BuyerForCorp] = @BuyerForCorp, [SellerForCorp] = @SellerForCorp, [BuyerWalletID] = @BuyerWalletID, [SellerWalletID] = @SellerWalletID, [SellerUnitProfit] = @SellerUnitProfit, [CalcProfitFromAssets] = @CalcProfitFromAssets 
	WHERE ([ID] = @Original_ID);

	SELECT *
	FROM Transactions 
	WHERE (ID = @ID)
	
	RETURN
";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.2.4"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'TransUpdate' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.4.2.5")) < 0)
                    {
                        #region Add 'For Sale Via Market' state to AssetStatuses table
                        commandText =
                                @"INSERT INTO AssetStatuses ([StatusID], [Description])
VALUES (4, 'For Sale Via Market                               ')";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.2.5"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'For Sale Via Market' state to AssetStatuses table", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.4.2.6")) < 0)
                    {
                        #region Create 'AssetsGetByProcessed' stored procedure
                        commandText =
                                @"CREATE PROCEDURE dbo.AssetsGetByProcessed
	@accessList			varchar(max),
	@systemID			int,
	@locationID			int,
	@itemID				int,
	@status				int,
	@processed			bit
AS
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
	WHERE (Assets.Status = @status OR @status = 0) AND (Assets.SystemID = @systemID OR @systemID = 0) AND (Assets.LocationID = @locationID OR @locationID = 0) AND (Assets.ItemID = @itemID OR @itemID = 0) AND (Assets.Processed = @processed)

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.2.6"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetsGetByProcessed' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.4.2.7")) < 0)
                    {
                        #region Create 'OrdersSetProcessedByID' stored procedure
                        commandText =
                                @"CREATE PROCEDURE dbo.OrdersSetProcessedByID
	@orderID	int,
	@processed	bit
AS
	UPDATE Orders
	SET Processed = @processed
	WHERE (ID = @orderID)
 
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.2.7"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'OrdersSetProcessedByID' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.4.2.8")) < 0)
                    {
                        #region Create 'TransGetByCalcProfitFromAssets' stored procedure
                        commandText =
                                @"CREATE PROCEDURE dbo.TransGetByCalcProfitFromAssets
	@accessParams			varchar(max),
	@itemID					int,
	@calcProfitFromAssets	bit
AS
	
	SELECT DISTINCT Transactions.*
	FROM Transactions
	JOIN CLR_financelist_split(@accessParams) a ON(
		(Transactions.BuyerID = a.ownerID OR Transactions.BuyerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.BuyerWalletID = a.walletID1 OR Transactions.BuyerWalletID = a.walletID2 OR Transactions.BuyerWalletID = a.walletID3 OR Transactions.BuyerWalletID = a.walletID4 OR Transactions.BuyerWalletID = a.walletID5 OR Transactions.BuyerWalletID = a.walletID6)) OR 
		(Transactions.SellerID = a.ownerID OR Transactions.SellerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.SellerWalletID = a.walletID1 OR Transactions.SellerWalletID = a.walletID2 OR Transactions.SellerWalletID = a.walletID3 OR Transactions.SellerWalletID = a.walletID4 OR Transactions.SellerWalletID = a.walletID5 OR Transactions.SellerWalletID = a.walletID6)) OR a.ownerID = 0)
	WHERE (Transactions.ItemID = @itemID OR @itemID = 0) AND (Transactions.CalcProfitFromAssets = @calcProfitFromAssets)
	ORDER BY DateTime DESC
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.2.8"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'TransGetByCalcProfitFromAssets' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.4.2.9")) < 0)
                    {
                        #region Create 'AssetsAddQuantity' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsAddQuantity 
	@ownerID		int,
	@corpAsset		bit,
	@itemID			int,
	@stationID		int,
	@systemID		int,
	@regionID		int,
	@status			int,
	@containerID	int,
	@autoConExclude	bit,
	@deltaQuantity	bigint,
	@addedItemsCost	decimal(18,2),
    @costCalc       bit
AS
	DECLARE @oldQuantity bigint, @newQuantity bigint
	DECLARE	@assetID bigint
	DECLARE @oldCost decimal(18, 2), @newCost decimal(18, 2)
    DECLARE @oldCostCalc bit, @newCostCalc bit
		
	SET @assetID = 0
	SELECT @oldQuantity = Quantity, @assetID = ID, @oldCost = Cost, @oldCostCalc = CostCalc
	FROM Assets
	WHERE OwnerID = @ownerID AND CorpAsset = @corpAsset AND LocationID = @stationID AND ItemID = @itemID AND Status = @status AND ContainerID = @containerID AND AutoConExclude = @autoConExclude
	
	IF(@assetID = 0)
	BEGIN
		INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [Cost], [CostCalc]) 
		VALUES (@ownerID, @corpAsset, @stationID, @itemID, @systemID, @regionID, 0, @deltaQuantity, @status, @autoConExclude, 0, 0, @addedItemsCost, @costCalc);
	END 
	ELSE
	BEGIN
		SET @newQuantity = @oldQuantity + @deltaQuantity
		-- If old quantity is less than zero then just use the old cost
		IF(@deltaQuantity > 0 AND @oldQuantity > 0)
		BEGIN
            -- If new items are being added to the stack then calculate the average cost from the 
            -- old and new values.
            -- If the old cost had not been calculated then just use the new cost
            SET @newCostCalc = 1
            IF(@oldCostCalc = 0 AND @costCalc = 0)
            BEGIN
                SET @newCost = 0
                SET @newCostCalc = 0
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 0)
            BEGIN
                SET @newCost = @oldCost
            END
            ELSE IF(@oldCostCalc = 0 AND @costCalc = 1)
            BEGIN
                SET @newCost = @addedItemsCost
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 1)
            BEGIN
			    SET @newCost = (@oldCost * @oldQuantity + @addedItemsCost * @deltaQuantity) / (@oldQuantity + @deltaQuantity)
		    END
        END
		ELSE
		BEGIN
            -- If items are being removed from the stack then just use the old cost value
			SET @newCost = @oldCost
            SET @newCostCalc = @oldCostCalc
		END		
		
		UPDATE [Assets] SET [Quantity] = @newQuantity, [Cost] = @newCost, [CostCalc] = @newCostCalc
		WHERE [OwnerID] = @ownerID AND [CorpAsset] = @corpAsset AND [LocationID] = @stationID AND [ItemID] = @itemID AND [Status] = @status AND [ContainerID] = @containerID AND [AutoConExclude] = @autoConExclude
	END
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.2.9"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetsAddQuantity' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.4.2.10")) < 0)
                    {
                        #region Set 'CostCalc' flag to false for any assets with a cost of 0
                        // This was done because some previous database changes went live with a bug 
                        // that caused the costcalc flag to be set to true when it should be false.
                        // This was caught before the main release and at the time, there was no 
                        // legitimate way for the costcalc flag to get set to true with out a value 
                        // being calculated.
                        // Thanks to this bit of luck, we can just reset the flag for any assets with
                        // no cost set.
                        commandText =
                                @"UPDATE [Assets] SET [CostCalc] = 0
                        WHERE [Cost] = 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.2.10"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem setting 'CostCalc' flag to false for any assets with a cost of 0", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.4.2.11")) < 0)
                    {
                        #region Update 'AssetsLostNew' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsLostNew
	@OwnerID			int,
	@CorpAsset			bit,
	@ItemID				int,
	@LossDateTime	    datetime,
	@Quantity			bigint,
	@newID				bigint		OUTPUT
AS
	SELECT @newID =
	(SELECT MAX(ID) AS MaxID
		FROM AssetsLost) + 1
		
	IF(@newID IS NULL)
	BEGIN
		SET @newID = 1
	END
	
	INSERT INTO AssetsLost (ID, OwnerID, CorpAsset, ItemID, LossDateTime, Quantity)
	VALUES (@newID, @OwnerID, @CorpAsset, @ItemID, @LossDateTime, @Quantity)

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.4.2.11"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsLostNew' stored procedure", ex);
                        }
                        #endregion
                    }
                    #endregion
                }
                if (dbVersion.CompareTo(new Version("1.5.1.0")) < 0)
                {
                    #region 1.5.0.0 - 1.5.1.0
                    if (dbVersion.CompareTo(new Version("1.5.0.0")) < 0)
                    {
                        #region Add 'EveItemID' column to Assets table
                        commandText =
                                @"ALTER TABLE dbo.Assets
ADD EveItemID bigint NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.0"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'EveItemID' column to Assets table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.1")) < 0)
                    {
                        #region Update 'AssetsAddQuantity' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsAddQuantity 
	@ownerID		int,
	@corpAsset		bit,
	@itemID			int,
	@stationID		int,
	@systemID		int,
	@regionID		int,
	@status			int,
	@containerID	int,
	@autoConExclude	bit,
	@deltaQuantity	bigint,
	@addedItemsCost	decimal(18,2),
    @costCalc       bit
AS
	DECLARE @oldQuantity bigint, @newQuantity bigint
	DECLARE	@assetID bigint
	DECLARE @oldCost decimal(18, 2), @newCost decimal(18, 2)
    DECLARE @oldCostCalc bit, @newCostCalc bit
		
	SET @assetID = 0
	SELECT @oldQuantity = Quantity, @assetID = ID, @oldCost = Cost, @oldCostCalc = CostCalc
	FROM Assets
	WHERE OwnerID = @ownerID AND CorpAsset = @corpAsset AND LocationID = @stationID AND ItemID = @itemID AND Status = @status AND ContainerID = @containerID AND AutoConExclude = @autoConExclude
	
	IF(@assetID = 0)
	BEGIN
		INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [Cost], [CostCalc], [EveItemID]) 
		VALUES (@ownerID, @corpAsset, @stationID, @itemID, @systemID, @regionID, 0, @deltaQuantity, @status, @autoConExclude, 0, 0, @addedItemsCost, @costCalc, 0);
	END 
	ELSE
	BEGIN
		SET @newQuantity = @oldQuantity + @deltaQuantity
		IF(@deltaQuantity > 0)
		BEGIN
            -- If new items are being added to the stack then calculate the average cost from the 
            -- old and new values.
            -- If the old cost had not been calculated then just use the new cost
            SET @newCostCalc = 1
            IF(@oldCostCalc = 0 AND @costCalc = 0)
            BEGIN
                SET @newCost = 0
                SET @newCostCalc = 0
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 0)
            BEGIN
                SET @newCost = @oldCost
            END
            ELSE IF(@oldCostCalc = 0 AND @costCalc = 1)
            BEGIN
                SET @newCost = @addedItemsCost
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 1)
            BEGIN
			    SET @newCost = (@oldCost * @oldQuantity + @addedItemsCost * @deltaQuantity) / (@oldQuantity + @deltaQuantity)
		    END
        END
		ELSE
		BEGIN
            -- If items are being removed from the stack then just use the old cost value
			SET @newCost = @oldCost
            SET @newCostCalc = @oldCostCalc
		END		
		
		UPDATE [Assets] SET [Quantity] = @newQuantity, [Cost] = @newCost, [CostCalc] = @newCostCalc
		WHERE [OwnerID] = @ownerID AND [CorpAsset] = @corpAsset AND [LocationID] = @stationID AND [ItemID] = @itemID AND [Status] = @status AND [ContainerID] = @containerID AND [AutoConExclude] = @autoConExclude
	END
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.1"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsAddQuantity' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.2")) < 0)
                    {
                        #region Update 'AssetsBuildResults' stored procedure
                        commandText =
                                @"ALTER PROCEDURE [dbo].[AssetsBuildResults]
	@accessList			varchar(max),
	@itemIDs			varchar(max),
	@status				int,
	@groupBy			varchar(50)
    --@totalRows          int         OUTPUT
AS
SET NOCOUNT ON

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_AssetResults]') AND type in (N'U'))
DROP TABLE [dbo].[tmp_AssetResults]

IF(@groupBy LIKE 'Owner')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, 0 AS SystemID, 
		0 AS RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		SUM(Assets.Cost * Assets.Quantity * Assets.CostCalc) / SUM(Assets.Quantity * Assets.CostCalc) AS Cost,
		MAX(CAST(Assets.CostCalc as int)) AS CostCalc,
        0 AS EveItemID,
        row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0)
	GROUP BY Assets.ItemID, Assets.OwnerID, Assets.CorpAsset
END
ELSE IF(@groupBy LIKE 'Region')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, 0 AS SystemID, 
		Assets.RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		SUM(Assets.Cost * Assets.Quantity * Assets.CostCalc) / SUM(Assets.Quantity * Assets.CostCalc) AS Cost,
		MAX(CAST(Assets.CostCalc as int)) AS CostCalc,
        0 AS EveItemID,
		row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0)
	GROUP BY Assets.ItemID, Assets.RegionID, Assets.OwnerID, Assets.CorpAsset
END
ELSE IF(@groupBy LIKE 'System')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, Assets.SystemID, 
		MAX(Assets.RegionID) AS RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		SUM(Assets.Cost * Assets.Quantity * Assets.CostCalc) / SUM(Assets.Quantity * Assets.CostCalc) AS Cost,
		MAX(CAST(Assets.CostCalc as int)) AS CostCalc,
        0 AS EveItemID,
		row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0)
	GROUP BY Assets.ItemID, Assets.SystemID, Assets.OwnerID, Assets.CorpAsset
END
ELSE
BEGIN
	SELECT Assets.*, row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0)
END

CREATE CLUSTERED INDEX [PK_tmp_AssetResults] ON [dbo].[tmp_AssetResults] 
(
	[RowNumber]
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF) ON [PRIMARY]	
 
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.2"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsBuildResults' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.3")) < 0)
                    {
                        #region Update 'AssetsGetResultsPage' stored procedure
                        commandText =
                                @"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_AssetResults]') AND type in (N'U'))
DROP TABLE [dbo].[tmp_AssetResults]


CREATE TABLE [dbo].[tmp_AssetResults](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[OwnerID] [int] NOT NULL,
	[CorpAsset] [bit] NOT NULL,
	[LocationID] [int] NOT NULL,
	[ItemID] [int] NOT NULL,
	[SystemID] [int] NOT NULL,
	[RegionID] [int] NOT NULL,
	[ContainerID] [bigint] NOT NULL,
	[Quantity] [bigint] NOT NULL,
	[Status] [int] NOT NULL,
	[AutoConExclude] [bit] NOT NULL,
	[Processed] [bit] NOT NULL,
	[IsContainer] [bit] NOT NULL,
	[ReprocExclude] [bit] NOT NULL,
	[Cost] [decimal](18,2) NOT NULL,
    [CostCalc] [bit] NOT NULL,
    [EveItemID] [bigint] NOT NULL,
	[RowNumber] [bigint] NULL
) ON [PRIMARY]";
                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            commandText =
                                    @"ALTER PROCEDURE [dbo].[AssetsGetResultsPage]
    @startRow           int,
    @pageSize           int
    --@totalRows          int         OUTPUT
AS
SET NOCOUNT ON

	SELECT ID, OwnerID, CorpAsset, LocationID, ItemID, SystemID, RegionID, ContainerID,
		Quantity, Status, AutoConExclude, Processed, IsContainer, ReprocExclude, Cost, CostCalc, EveItemID
	FROM tmp_AssetResults
	WHERE RowNumber BETWEEN (@startRow) AND (@startRow + @pageSize - 1)
	ORDER BY RowNumber";

                            adapter = new SqlDataAdapter(commandText, connection);
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.3"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetResultsPage' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.4")) < 0)
                    {
                        #region Update 'AssetsInsert' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsInsert
	@OwnerID		int,
	@CorpAsset		bit,
	@LocationID		int,
	@ItemID			int,
	@SystemID		int,
	@RegionID		int,
	@ContainerID	bigint,
	@Quantity		bigint,
	@Status			int,
	@Processed		bit,
	@AutoConExclude	bit,
	@IsContainer	bit,
    @ReprocExclude  bit,
    @Cost			decimal(18, 2),
    @CostCalc       bit,
    @EveItemID      bigint,
	@newID			bigint OUT
AS
	INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [ReprocExclude], [Cost], [CostCalc], [EveItemID]) 
	VALUES (@OwnerID, @CorpAsset, @LocationID, @ItemID, @SystemID, @RegionID, @ContainerID, @Quantity, @Status, @AutoConExclude, @Processed, @IsContainer, @ReprocExclude, @Cost, @CostCalc, @EveItemID);

	SET @newID = SCOPE_IDENTITY()

	SELECT * 
	FROM Assets 
	WHERE (ID = @newID)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.4"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsInsert' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.5")) < 0)
                    {
                        #region Update 'AssetsUpdate' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsUpdate
	@ID				bigint,
	@OwnerID		int,
	@CorpAsset		bit,
	@LocationID		int,
	@ItemID			int,
	@SystemID		int,
	@RegionID		int,
	@ContainerID	bigint,
	@Quantity		bigint,
	@Status			int,
	@Processed		bit,
	@AutoConExclude	bit,
	@IsContainer	bit,
    @ReprocExclude  bit,
    @Cost			decimal(18,2),
    @CostCalc       bit, 
    @EveItemID      bigint,
	@Original_ID	bigint
AS
	UPDATE [Assets] SET [OwnerID] = @OwnerID, [CorpAsset] = @CorpAsset, [LocationID] = @LocationID, [ItemID] = @ItemID, [SystemID] = @SystemID, [RegionID] = @RegionID, [ContainerID] = @ContainerID, [Quantity] = @Quantity, [Status] = @Status, [AutoConExclude] = @AutoConExclude, [Processed] = @Processed, [IsContainer] = @IsContainer, [ReprocExclude] = @ReprocExclude, [Cost] = @Cost, [CostCalc] = @CostCalc, [EveItemID] = @EveItemID
	WHERE ([ID] = @Original_ID);
	
	SELECT * 
	FROM Assets 
	WHERE (ID = @ID)
	
	RETURN   ";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.5"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsUpdate' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.6")) < 0)
                    {
                        #region Update 'AssetExists' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetExists 
	@ownerID			int,
	@corpAsset			bit,
	@locationID			int,
	@itemID				int,
	@status				int,
	@isContained		bit,
	@containerID		bigint,
	@isContainer		bit,
	@processed			bit,			
	@ignoreProcessed	bit,
	@autoConExclude		bit,
	@ignoreAutoConEx	bit,
	@eveItemID			bigint,
	@exists				bit			OUT,
	@assetID			bigint		OUT
AS
	SET @exists = 0
	SET	@assetID = 0

	SELECT @assetID = ID
	FROM Assets
	WHERE (OwnerID = @ownerID) AND (CorpAsset = @corpAsset) AND (LocationID = @locationID) AND (ItemID = @itemID) AND (Status = @status) AND (ContainerID = @containerID OR (@isContained = 1 AND @containerID = 0 AND ContainerID != 0)) AND (IsContainer = @isContainer) AND (Processed = @processed OR @ignoreProcessed = 1) AND (AutoConExclude = @autoConExclude OR @ignoreAutoConEx = 1) AND (EveItemID = @eveItemID OR @eveItemID = 0)
	IF(@eveItemID > 0 AND @assetID = 0)
	BEGIN
		-- If eve ID is set and we didn't find a match with it then
		-- just try without it.
		SELECT @assetID = ID
		FROM Assets
		WHERE (OwnerID = @ownerID) AND (CorpAsset = @corpAsset) AND (LocationID = @locationID) AND (ItemID = @itemID) AND (Status = @status) AND (ContainerID = @containerID OR (@isContained = 1 AND @containerID = 0 AND ContainerID != 0)) AND (IsContainer = @isContainer) AND (Processed = @processed OR @ignoreProcessed = 1) AND (AutoConExclude = @autoConExclude OR @ignoreAutoConEx = 1)
	END
	SELECT *
	FROM Assets
	WHERE (ID = @assetID)

	IF(@@ROWCOUNT >= 1)
	BEGIN
		SET @exists = 1
	END

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.6"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetExists' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.5.0.7")) < 0)
                    {
                        #region Add 'EveOrderID' column to Orders table
                        commandText =
                                @"ALTER TABLE dbo.Orders
ADD EveOrderID bigint NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.7"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'EveOrderID' column to Orders table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.8")) < 0)
                    {
                        #region Update 'OrderExists' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.OrderExists
	@ownerID		int,
	@forCorp		bit,
	@walletID		smallint,
	@stationID		int,
	@itemID			int,
	@totalVol		int,
	@remainingVol	int,
	@range			smallint,
	@orderState		smallint,
	@buyOrder		bit,
	@price			decimal(18,2),
	@eveOrderID		bigint,
	@exists			bit		OUT,
	@orderID		int		OUT
AS
	SET @exists = 0
	SET	@orderID = 0

	SELECT @orderID = ID
	FROM Orders
	WHERE (OwnerID = @ownerID) AND (ForCorp = @forCorp) AND (WalletID = @walletID) AND 
		(StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
		(TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND ((OrderState = @orderState) OR 
		(@orderState = 2 AND (OrderState = 2 OR OrderState = 1000 OR OrderState = 2000))) AND
		(Processed = 0) AND (Price = @price) AND (EveOrderID = @eveOrderID)
	IF(@orderID = 0 AND @orderState = 2) 
	BEGIN
		-- If we are trying to find a filled/expired order but failed first time around then try
		-- looking for an active order (i.e. one that was active and is now competed/expired)
		SELECT @orderID = ID
		FROM Orders
		WHERE (OwnerID = @ownerID) AND (ForCorp = @forCorp) AND (WalletID = @walletID) AND 
			(StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
			(TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND (Processed = 0) AND
			(OrderState = 0 OR OrderState = 999) AND (Price = @price) AND (EveOrderID = @eveOrderID)
	END
	IF(@orderID = 0) 
	BEGIN
		-- Try matching to an order in the database that matches all other parameters but has a blank eve ID.
		-- This can happen if the user has recently installed the update that adds EveOrderID to the Orders table.
		SELECT @orderID = ID
		FROM Orders
		WHERE (OwnerID = @ownerID) AND (ForCorp = @forCorp) AND (WalletID = @walletID) AND 
			(StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
			(TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND ((OrderState = @orderState) OR 
			(@orderState = 2 AND (OrderState = 2 OR OrderState = 1000 OR OrderState = 2000))) AND
			(Processed = 0) AND (Price = @price) AND (EveOrderID = 0)
	    IF(@orderID = 0) 
	    BEGIN
		    -- Still couldn't match an order so try finding one that matches all parameters except price and 
		    -- eve order id (changing the price changes the ID).
		    SELECT @orderID = ID
		    FROM Orders
		    WHERE (OwnerID = @ownerID) AND (ForCorp = @forCorp) AND (WalletID = @walletID) AND 
			    (StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
			    (TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND (Processed = 0) AND 
			    ((OrderState = @orderState) OR (@orderState = 2 AND 
			    (OrderState = 2 OR OrderState = 1000 OR OrderState = 2000)))
	        IF(@orderID = 0) 
	        BEGIN
		        -- Still couldn't match an order so try finding one that matches all parameters except state and price.
		        SELECT @orderID = ID
		        FROM Orders
		        WHERE (OwnerID = @ownerID) AND (ForCorp = @forCorp) AND (WalletID = @walletID) AND 
			        (StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
			        (TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND (Processed = 0)
	        END
	    END
	END
	
	UPDATE Orders
	SET Processed = 1
	WHERE (ID = @orderID)
		
	SELECT Orders.*
	FROM Orders
	WHERE (ID = @orderID)

	IF(@@ROWCOUNT = 1)
	BEGIN
		SET @exists = 1
	END

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.8"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'OrderExists' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.9")) < 0)
                    {
                        #region Update 'OrdersInsert' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.OrdersInsert 
	@OwnerID		int,
	@ForCorp		bit,
	@StationID		int,
	@TotalVol		int,
	@RemainingVol	int,
	@MinVolume		int,
	@OrderState		smallint,
	@ItemID			int,
	@Range			smallint,
	@walletID		smallint,
	@Duration		smallint,
	@Escrow			decimal(18,2),
	@Price			decimal(18,2),
	@BuyOrder		bit,
	@Issued			datetime,
	@Processed		bit,
	@EveOrderID		bigint	
AS
	INSERT INTO [dbo].[Orders] ([OwnerID], [ForCorp], [StationID], [TotalVol], [RemainingVol], [MinVolume], [OrderState], [ItemID], [Range], [WalletID], [Duration], [Escrow], [Price], [BuyOrder], [Issued], [Processed], [EveOrderID]) 
	VALUES (@OwnerID, @ForCorp, @StationID, @TotalVol, @RemainingVol, @MinVolume, @OrderState, @ItemID, @Range, @WalletID, @Duration, @Escrow, @Price, @BuyOrder, @Issued, @Processed, @EveOrderID);

	SELECT ID, OwnerID, ForCorp, StationID, TotalVol, RemainingVol, MinVolume, OrderState, ItemID, Range, WalletID, Duration, Escrow, Price, BuyOrder, Issued, Processed, EveOrderID
	FROM Orders 
	WHERE (ID = SCOPE_IDENTITY())
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.9"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'OrdersInsert' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.10")) < 0)
                    {
                        #region Update 'OrdersUpdate' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.OrdersUpdate 
	@ID					int,
	@OwnerID			int,
	@ForCorp			bit,
	@StationID			int,
	@TotalVol			int,
	@RemainingVol		int,
	@MinVolume			int,
	@OrderState			smallint,
	@ItemID				int,
	@Range				smallint,
	@WalletID			smallint,
	@Duration			smallint,
	@Escrow				decimal(18,2),
	@Price				decimal(18,2),
	@BuyOrder			bit,
	@Issued				datetime,
	@Processed			bit,
	@EveOrderID			bigint,
	@Original_ID		int
AS

	UPDATE [dbo].[Orders] SET [OwnerID] = @OwnerID, [ForCorp] = @ForCorp, [StationID] = @StationID, [TotalVol] = @TotalVol, [RemainingVol] = @RemainingVol, [MinVolume] = @MinVolume, [OrderState] = @OrderState, [ItemID] = @ItemID, [Range] = @Range, [WalletID] = @WalletID, [Duration] = @Duration, [Escrow] = @Escrow, [Price] = @Price, [BuyOrder] = @BuyOrder, [Issued] = @Issued, [Processed] = @Processed, [EveOrderID] = @EveOrderID
	WHERE ([ID] = @Original_ID);

	SELECT ID, OwnerID, ForCorp, StationID, TotalVol, RemainingVol, MinVolume, OrderState, ItemID, Range, WalletID, Duration, Escrow, Price, BuyOrder, Issued, Processed, EveOrderID
	FROM Orders 
	WHERE (ID = @ID)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.10"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'OrdersUpdate' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.12")) < 0)
                    {
                        #region Update 'AssetsAddQuantity' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsAddQuantity 
	@ownerID		int,
	@corpAsset		bit,
	@itemID			int,
	@stationID		int,
	@systemID		int,
	@regionID		int,
	@status			int,
	@containerID	bigint,
	@autoConExclude	bit,
	@deltaQuantity	bigint,
	@addedItemsCost	decimal(18,2),
    @costCalc       bit
AS
	DECLARE @oldQuantity bigint, @newQuantity bigint
	DECLARE	@assetID bigint
	DECLARE @oldCost decimal(18, 2), @newCost decimal(18, 2)
    DECLARE @oldCostCalc bit, @newCostCalc bit
		
	SET @assetID = 0
	SELECT @oldQuantity = Quantity, @assetID = ID, @oldCost = Cost, @oldCostCalc = CostCalc
	FROM Assets
	WHERE OwnerID = @ownerID AND CorpAsset = @corpAsset AND LocationID = @stationID AND ItemID = @itemID AND Status = @status AND ContainerID = @containerID AND AutoConExclude = @autoConExclude AND IsContainer = 0
	
	IF(@assetID = 0)
	BEGIN
		INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [Cost], [CostCalc], [EveItemID]) 
		VALUES (@ownerID, @corpAsset, @stationID, @itemID, @systemID, @regionID, 0, @deltaQuantity, @status, @autoConExclude, 0, 0, @addedItemsCost, @costCalc, 0);
	END 
	ELSE
	BEGIN
		SET @newQuantity = @oldQuantity + @deltaQuantity
		IF(@deltaQuantity > 0)
		BEGIN
            -- If new items are being added to the stack then calculate the average cost from the 
            -- old and new values.
            -- If the old cost had not been calculated then just use the new cost
            SET @newCostCalc = 1
            IF(@oldCostCalc = 0 AND @costCalc = 0)
            BEGIN
                SET @newCost = 0
                SET @newCostCalc = 0
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 0)
            BEGIN
                SET @newCost = @oldCost
            END
            ELSE IF(@oldCostCalc = 0 AND @costCalc = 1)
            BEGIN
                SET @newCost = @addedItemsCost
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 1)
            BEGIN
			    SET @newCost = (@oldCost * @oldQuantity + @addedItemsCost * @deltaQuantity) / (@oldQuantity + @deltaQuantity)
		    END
        END
		ELSE
		BEGIN
            -- If items are being removed from the stack then just use the old cost value
			SET @newCost = @oldCost
            SET @newCostCalc = @oldCostCalc
		END		
		
		UPDATE [Assets] SET [Quantity] = @newQuantity, [Cost] = @newCost, [CostCalc] = @newCostCalc
		WHERE [OwnerID] = @ownerID AND [CorpAsset] = @corpAsset AND [LocationID] = @stationID AND [ItemID] = @itemID AND [Status] = @status AND [ContainerID] = @containerID AND [AutoConExclude] = @autoConExclude AND IsContainer = 0
	END
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.12"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsAddQuantity' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.5.0.13")) < 0)
                    {
                        #region Create 'AssetsGetByItem' stored procedure
                        commandText =
                               @"CREATE PROCEDURE dbo.AssetsGetByItem
	@accessList			varchar(max),
	@regionIDs			varchar(max),
	@stationIDs			varchar(max),
	@itemID				int,
	@includeInTransit	bit,
	@includeContainers	bit
AS
IF(NOT @regionIDs LIKE '')
BEGIN
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
	JOIN CLR_intlist_split(@regionIDs) r ON (Assets.RegionID = r.number OR r.number = 0)
	JOIN CLR_intlist_split(@stationIDs) s ON (Assets.LocationID = s.number OR s.number = 0)
	WHERE (Assets.ItemID = @itemID OR @itemID = 0) AND (@includeInTransit = 1 OR NOT Assets.Status = 2) AND (@includeContainers = 1 OR (Assets.IsContainer = 0 AND Assets.ContainerID = 0))
END

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.13"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetsGetByItem' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.5.0.14")) < 0)
                    {
                        #region Update 'AssetsAddQuantity' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsAddQuantity 
	@ownerID		int,
	@corpAsset		bit,
	@itemID			int,
	@stationID		int,
	@systemID		int,
	@regionID		int,
	@status			int,
	@containerID	bigint,
	@autoConExclude	bit,
	@deltaQuantity	bigint,
	@addedItemsCost	decimal(18,2),
    @costCalc       bit
AS
	DECLARE @oldQuantity bigint, @newQuantity bigint
	DECLARE	@assetID bigint
	DECLARE @oldCost decimal(18, 2), @newCost decimal(18, 2)
    DECLARE @oldCostCalc bit, @newCostCalc bit
		
	SET @assetID = 0
	SELECT @oldQuantity = Quantity, @assetID = ID, @oldCost = Cost, @oldCostCalc = CostCalc
	FROM Assets
	WHERE OwnerID = @ownerID AND CorpAsset = @corpAsset AND LocationID = @stationID AND ItemID = @itemID AND Status = @status AND ContainerID = @containerID AND AutoConExclude = @autoConExclude AND IsContainer = 0
	
	IF(@assetID = 0)
	BEGIN
		INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [Cost], [CostCalc], [EveItemID]) 
		VALUES (@ownerID, @corpAsset, @stationID, @itemID, @systemID, @regionID, 0, @deltaQuantity, @status, @autoConExclude, 0, 0, @addedItemsCost, @costCalc, 0);
	END 
	ELSE
	BEGIN
		SET @newQuantity = @oldQuantity + @deltaQuantity
		IF(@deltaQuantity > 0)
		BEGIN
            -- If new items are being added to the stack then calculate the average cost from the 
            -- old and new values.
            -- If the old cost had not been calculated then just use the new cost
            SET @newCostCalc = 1
            IF(@oldCostCalc = 0 AND @costCalc = 0)
            BEGIN
                SET @newCost = 0
                SET @newCostCalc = 0
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 0)
            BEGIN
                SET @newCost = @oldCost
            END
            ELSE IF(@oldCostCalc = 0 AND @costCalc = 1)
            BEGIN
                SET @newCost = @addedItemsCost
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 1)
            BEGIN
				IF(@oldQuantity + @deltaQuantity = 0)
				BEGIN
					SET @newCost = 0
				END
				ELSE
				BEGIN
					SET @newCost = (@oldCost * @oldQuantity + @addedItemsCost * @deltaQuantity) / (@oldQuantity + @deltaQuantity)
				END
		    END
        END
		ELSE
		BEGIN
            -- If items are being removed from the stack then just use the old cost value
			SET @newCost = @oldCost
            SET @newCostCalc = @oldCostCalc
		END		
		
		UPDATE [Assets] SET [Quantity] = @newQuantity, [Cost] = @newCost, [CostCalc] = @newCostCalc
		WHERE [OwnerID] = @ownerID AND [CorpAsset] = @corpAsset AND [LocationID] = @stationID AND [ItemID] = @itemID AND [Status] = @status AND [ContainerID] = @containerID AND [AutoConExclude] = @autoConExclude AND IsContainer = 0
	END
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.14"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsAddQuantity' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.5.0.16")) < 0)
                    {
                        #region Add 'BoughtViaContract' flag to Assets table
                        commandText =
                               @"ALTER TABLE dbo.Assets
ADD BoughtViaContract bit NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.16"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'BoughtViaContract' flag to Assets table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.17")) < 0)
                    {
                        #region Update 'AssetsAddQuantity' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsAddQuantity 
	@ownerID		int,
	@corpAsset		bit,
	@itemID			int,
	@stationID		int,
	@systemID		int,
	@regionID		int,
	@status			int,
	@containerID	bigint,
	@autoConExclude	bit,
	@deltaQuantity	bigint,
	@addedItemsCost	decimal(18,2),
    @costCalc       bit
AS
	DECLARE @oldQuantity bigint, @newQuantity bigint
	DECLARE	@assetID bigint
	DECLARE @oldCost decimal(18, 2), @newCost decimal(18, 2)
    DECLARE @oldCostCalc bit, @newCostCalc bit
		
	SET @assetID = 0
	SELECT @oldQuantity = Quantity, @assetID = ID, @oldCost = Cost, @oldCostCalc = CostCalc
	FROM Assets
	WHERE OwnerID = @ownerID AND CorpAsset = @corpAsset AND LocationID = @stationID AND ItemID = @itemID AND Status = @status AND ContainerID = @containerID AND AutoConExclude = @autoConExclude
	
	IF(@assetID = 0)
	BEGIN
		INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [Cost], [CostCalc], [EveItemID], [BoughtViaContract]) 
		VALUES (@ownerID, @corpAsset, @stationID, @itemID, @systemID, @regionID, 0, @deltaQuantity, @status, @autoConExclude, 0, 0, @addedItemsCost, @costCalc, 0, 0);
	END 
	ELSE
	BEGIN
		SET @newQuantity = @oldQuantity + @deltaQuantity
		IF(@deltaQuantity > 0)
		BEGIN
            -- If new items are being added to the stack then calculate the average cost from the 
            -- old and new values.
            -- If the old cost had not been calculated then just use the new cost
            SET @newCostCalc = 1
            IF(@oldCostCalc = 0 AND @costCalc = 0)
            BEGIN
                SET @newCost = 0
                SET @newCostCalc = 0
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 0)
            BEGIN
                SET @newCost = @oldCost
            END
            ELSE IF(@oldCostCalc = 0 AND @costCalc = 1)
            BEGIN
                SET @newCost = @addedItemsCost
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 1)
            BEGIN
			    SET @newCost = (@oldCost * @oldQuantity + @addedItemsCost * @deltaQuantity) / (@oldQuantity + @deltaQuantity)
		    END
        END
		ELSE
		BEGIN
            -- If items are being removed from the stack then just use the old cost value
			SET @newCost = @oldCost
            SET @newCostCalc = @oldCostCalc
		END		
		
		UPDATE [Assets] SET [Quantity] = @newQuantity, [Cost] = @newCost, [CostCalc] = @newCostCalc
		WHERE [OwnerID] = @ownerID AND [CorpAsset] = @corpAsset AND [LocationID] = @stationID AND [ItemID] = @itemID AND [Status] = @status AND [ContainerID] = @containerID AND [AutoConExclude] = @autoConExclude
	END
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.17"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsAddQuantity' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.18")) < 0)
                    {
                        #region Update 'AssetsBuildResults' stored procedure
                        commandText =
                                @"ALTER PROCEDURE [dbo].[AssetsBuildResults]
	@accessList			varchar(max),
	@itemIDs			varchar(max),
	@status				int,
	@groupBy			varchar(50)
    --@totalRows          int         OUTPUT
AS
SET NOCOUNT ON

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_AssetResults]') AND type in (N'U'))
DROP TABLE [dbo].[tmp_AssetResults]

IF(@groupBy LIKE 'Owner')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, 0 AS SystemID, 
		0 AS RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		-- This causes div by 0 errors. Just drop the inclusing of the CostCalc flag and 
		-- include quantity > 0 in the WHERE clause. Not ideal but best simple solution I can think of.
		--SUM(Assets.Cost * Assets.Quantity * Assets.CostCalc) / SUM(Assets.Quantity * Assets.CostCalc) AS Cost,
		SUM(Assets.Cost * Assets.Quantity) / SUM(Assets.Quantity) AS Cost,
		MAX(CAST(Assets.CostCalc as int)) AS CostCalc,
        0 AS EveItemID, CAST(0 AS bit) AS BoughtViaContract,
        row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0) AND Assets.Quantity > 0
	GROUP BY Assets.ItemID, Assets.OwnerID, Assets.CorpAsset
END
ELSE IF(@groupBy LIKE 'Region')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, 0 AS SystemID, 
		Assets.RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		-- This causes div by 0 errors. Just drop the inclusing of the CostCalc flag and 
		-- include quantity > 0 in the WHERE clause. Not ideal but best simple solution I can think of.
		--SUM(Assets.Cost * Assets.Quantity * Assets.CostCalc) / SUM(Assets.Quantity * Assets.CostCalc) AS Cost,
		SUM(Assets.Cost * Assets.Quantity) / SUM(Assets.Quantity) AS Cost,		
		MAX(CAST(Assets.CostCalc as int)) AS CostCalc,
        0 AS EveItemID, CAST(0 AS bit) AS BoughtViaContract,
		row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0) AND Assets.Quantity > 0
	GROUP BY Assets.ItemID, Assets.RegionID, Assets.OwnerID, Assets.CorpAsset
END
ELSE IF(@groupBy LIKE 'System')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, Assets.SystemID, 
		MAX(Assets.RegionID) AS RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		-- This causes div by 0 errors. Just drop the inclusing of the CostCalc flag and 
		-- include quantity > 0 in the WHERE clause. Not ideal but best simple solution I can think of.
		--SUM(Assets.Cost * Assets.Quantity * Assets.CostCalc) / SUM(Assets.Quantity * Assets.CostCalc) AS Cost,
		SUM(Assets.Cost * Assets.Quantity) / SUM(Assets.Quantity) AS Cost,
		MAX(CAST(Assets.CostCalc as int)) AS CostCalc,
        0 AS EveItemID, CAST(0 AS bit) AS BoughtViaContract,
		row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0) AND Assets.Quantity > 0
	GROUP BY Assets.ItemID, Assets.SystemID, Assets.OwnerID, Assets.CorpAsset
END
ELSE
BEGIN
	SELECT Assets.*, row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0) 
END

CREATE CLUSTERED INDEX [PK_tmp_AssetResults] ON [dbo].[tmp_AssetResults] 
(
	[RowNumber]
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF) ON [PRIMARY]	
 
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.18"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsBuildResults' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.19")) < 0)
                    {
                        #region Update 'AssetsGetResultsPage' stored procedure
                        commandText =
                                @"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_AssetResults]') AND type in (N'U'))
DROP TABLE [dbo].[tmp_AssetResults]


CREATE TABLE [dbo].[tmp_AssetResults](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[OwnerID] [int] NOT NULL,
	[CorpAsset] [bit] NOT NULL,
	[LocationID] [int] NOT NULL,
	[ItemID] [int] NOT NULL,
	[SystemID] [int] NOT NULL,
	[RegionID] [int] NOT NULL,
	[ContainerID] [bigint] NOT NULL,
	[Quantity] [bigint] NOT NULL,
	[Status] [int] NOT NULL,
	[AutoConExclude] [bit] NOT NULL,
	[Processed] [bit] NOT NULL,
	[IsContainer] [bit] NOT NULL,
	[ReprocExclude] [bit] NOT NULL,
	[Cost] [decimal](18,2) NOT NULL,
    [CostCalc] [bit] NOT NULL,
    [EveItemID] [bigint] NOT NULL,
    [BoughtViaContract] [bit] NOT NULL,
	[RowNumber] [bigint] NULL
) ON [PRIMARY]";
                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            commandText =
                                    @"ALTER PROCEDURE [dbo].[AssetsGetResultsPage]
    @startRow           int,
    @pageSize           int
    --@totalRows          int         OUTPUT
AS
SET NOCOUNT ON

	SELECT ID, OwnerID, CorpAsset, LocationID, ItemID, SystemID, RegionID, ContainerID,
		Quantity, Status, AutoConExclude, Processed, IsContainer, ReprocExclude, Cost, CostCalc, 
        EveItemID, BoughtViaContract
	FROM tmp_AssetResults
	WHERE RowNumber BETWEEN (@startRow) AND (@startRow + @pageSize - 1)
	ORDER BY RowNumber";

                            adapter = new SqlDataAdapter(commandText, connection);
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.19"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetResultsPage' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.20")) < 0)
                    {
                        #region Update 'AssetsInsert' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsInsert
	@OwnerID		    int,
	@CorpAsset		    bit,
	@LocationID		    int,
	@ItemID			    int,
	@SystemID		    int,
	@RegionID		    int,
	@ContainerID	    bigint,
	@Quantity		    bigint,
	@Status			    int,
	@Processed		    bit,
	@AutoConExclude	    bit,
	@IsContainer	    bit,
    @ReprocExclude      bit,
    @Cost			    decimal(18, 2),
    @CostCalc           bit,
    @EveItemID          bigint,
    @BoughtViaContract  bit,
	@newID			    bigint OUT
AS
	INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [ReprocExclude], [Cost], [CostCalc], [EveItemID], [BoughtViaContract]) 
	VALUES (@OwnerID, @CorpAsset, @LocationID, @ItemID, @SystemID, @RegionID, @ContainerID, @Quantity, @Status, @AutoConExclude, @Processed, @IsContainer, @ReprocExclude, @Cost, @CostCalc, @EveItemID, @BoughtViaContract);

	SET @newID = SCOPE_IDENTITY()

	SELECT * 
	FROM Assets 
	WHERE (ID = @newID)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.20"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsInsert' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.21")) < 0)
                    {
                        #region Update 'AssetsUpdate' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsUpdate
	@ID				    bigint,
	@OwnerID		    int,
	@CorpAsset		    bit,
	@LocationID		    int,
	@ItemID			    int,
	@SystemID		    int,
	@RegionID		    int,
	@ContainerID	    bigint,
	@Quantity		    bigint,
	@Status			    int,
	@Processed		    bit,
	@AutoConExclude	    bit,
	@IsContainer	    bit,
    @ReprocExclude      bit,
    @Cost			    decimal(18,2),
    @CostCalc           bit, 
    @EveItemID          bigint,
    @BoughtViaContract  bit,
	@Original_ID	    bigint
AS
	UPDATE [Assets] SET [OwnerID] = @OwnerID, [CorpAsset] = @CorpAsset, [LocationID] = @LocationID, [ItemID] = @ItemID, [SystemID] = @SystemID, [RegionID] = @RegionID, [ContainerID] = @ContainerID, [Quantity] = @Quantity, [Status] = @Status, [AutoConExclude] = @AutoConExclude, [Processed] = @Processed, [IsContainer] = @IsContainer, [ReprocExclude] = @ReprocExclude, [Cost] = @Cost, [CostCalc] = @CostCalc, [EveItemID] = @EveItemID, [BoughtViaContract] = @BoughtViaContract
	WHERE ([ID] = @Original_ID);
	
	SELECT * 
	FROM Assets 
	WHERE (ID = @ID)
	
	RETURN   ";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.21"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsUpdate' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.22")) < 0)
                    {
                        #region Create 'AssetsGetBoughtViaContract' stored procedure
                        commandText =
                                @"CREATE PROCEDURE dbo.AssetsGetBoughtViaContract
	@itemID				int
AS
	SELECT Assets.*
	FROM Assets 
	WHERE (Assets.BoughtViaContract = 1) AND (Assets.ItemID = @itemID OR @itemID = 0)
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.22"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetsGetBoughtViaContract' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.23")) < 0)
                    {
                        #region Update 'AssetsGetBoughtViaContract' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsGetBoughtViaContract
	@accessList			varchar(max),
	@itemID				int
AS
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
	WHERE (Assets.BoughtViaContract = 1) AND (Assets.ItemID = @itemID OR @itemID = 0)
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.23"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updaing 'AssetsGetBoughtViaContract' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.5.0.24")) < 0)
                    {
                        #region Create 'TransGetItemIDsNoLimits' stored procedure
                        commandText =
                                @"CREATE PROCEDURE dbo.TransGetItemIDsNoLimits
	@accessParams		varchar(max)
AS
	SELECT ItemID AS [ID]
	FROM Transactions
	JOIN CLR_financelist_split(@accessParams) a ON(
		((Transactions.BuyerID = a.ownerID OR Transactions.BuyerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.BuyerWalletID = a.walletID1 OR Transactions.BuyerWalletID = a.walletID2 OR Transactions.BuyerWalletID = a.walletID3 OR Transactions.BuyerWalletID = a.walletID4 OR Transactions.BuyerWalletID = a.walletID5 OR Transactions.BuyerWalletID = a.walletID6))) OR 
		((Transactions.SellerID = a.ownerID OR Transactions.SellerCharacterID = a.ownerID) AND (a.walletID1 = 0 OR (Transactions.SellerWalletID = a.walletID1 OR Transactions.SellerWalletID = a.walletID2 OR Transactions.SellerWalletID = a.walletID3 OR Transactions.SellerWalletID = a.walletID4 OR Transactions.SellerWalletID = a.walletID5 OR Transactions.SellerWalletID = a.walletID6))) OR a.ownerID = 0)
	GROUP BY ItemID
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.24"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'TransGetItemIDsNoLimits' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.25")) < 0)
                    {
                        #region Create 'AssetContains' function
                        commandText =
                                @"CREATE FUNCTION AssetContains(@containerID bigint, @itemID int)
RETURNS bit
AS
BEGIN
-- Note this is not perfect as it does not return true an item matching itemID is in a container within 
-- the current container. However, that's not a common scenario and it's tricky to solve efficiently.
        DECLARE @result bit
        DECLARE @totalRows int
        
        SELECT @totalRows = count(*) FROM Assets WHERE ContainerID = @containerID AND ItemID = @itemID
        IF(@totalRows > 0) 
        BEGIN
			SET @result = 1
		END
        
        RETURN @result
END ";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.25"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetContains' function", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.26")) < 0)
                    {
                        #region Create 'AssetsGetItemAndContainersOfItem' stored procedure
                        commandText =
                                @"CREATE PROCEDURE dbo.AssetsGetItemAndContainersOfItem
	@accessList			varchar(max),
	@regionIDs			varchar(max),
	@systemID			int,
	@locationID			int,
	@itemID				int,
	@containersOnly		bit,
	@getContained		bit,
	@status				int
AS
IF(NOT @regionIDs LIKE '')
BEGIN
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
	JOIN CLR_intlist_split(@regionIDs) r ON Assets.RegionID = r.number 
	WHERE (Assets.Status = @status OR @status = 0) AND (Assets.SystemID = @systemID OR @systemID = 0) AND (Assets.LocationID = @locationID OR @locationID = 0) AND (Assets.ItemID = @itemID OR @itemID = 0 OR (Assets.IsContainer = 1 AND dbo.AssetContains(Assets.ID, @itemID) = 1)) AND (Assets.IsContainer = 1 OR @containersOnly = 0) AND (Assets.ContainerID = 0 OR @getContained = 1)
END
ELSE
BEGIN
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_accesslist_split(@accessList) a ON (Assets.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Assets.CorpAsset = 1) OR (a.includePersonal = 1 AND Assets.CorpAsset = 0)))
	WHERE (Assets.Status = @status OR @status = 0) AND (Assets.SystemID = @systemID OR @systemID = 0) AND (Assets.LocationID = @locationID OR @locationID = 0) AND (Assets.ItemID = @itemID OR @itemID = 0 OR (Assets.IsContainer = 1 AND dbo.AssetContains(Assets.ID, @itemID) = 1)) AND (Assets.IsContainer = 1 OR @containersOnly = 0) AND (Assets.ContainerID = 0 OR @getContained = 1)
END
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.26"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetsGetItemAndContainersOfItem' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.5.0.27")) < 0)
                    {
                        #region Update 'AssetsAddQuantity' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsAddQuantity 
	@ownerID		int,
	@corpAsset		bit,
	@itemID			int,
	@stationID		int,
	@systemID		int,
	@regionID		int,
	@status			int,
	@containerID	bigint,
	@autoConExclude	bit,
	@deltaQuantity	bigint,
	@addedItemsCost	decimal(18,2),
    @costCalc       bit
AS
	DECLARE @oldQuantity bigint, @newQuantity bigint
	DECLARE	@assetID bigint
	DECLARE @oldCost decimal(18, 2), @newCost decimal(18, 2)
    DECLARE @oldCostCalc bit, @newCostCalc bit
		
	SET @assetID = 0
	SELECT @oldQuantity = Quantity, @assetID = ID, @oldCost = Cost, @oldCostCalc = CostCalc
	FROM Assets
	WHERE OwnerID = @ownerID AND CorpAsset = @corpAsset AND LocationID = @stationID AND ItemID = @itemID AND Status = @status AND ContainerID = @containerID AND AutoConExclude = @autoConExclude AND IsContainer = 0
	
	IF(@assetID = 0)
	BEGIN
		INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [Cost], [CostCalc], [EveItemID], [BoughtViaContract]) 
		VALUES (@ownerID, @corpAsset, @stationID, @itemID, @systemID, @regionID, 0, @deltaQuantity, @status, @autoConExclude, 0, 0, @addedItemsCost, @costCalc, 0, 0);
	END 
	ELSE
	BEGIN
		SET @newQuantity = @oldQuantity + @deltaQuantity
		IF(@deltaQuantity > 0)
		BEGIN
            -- If new items are being added to the stack then calculate the average cost from the 
            -- old and new values.
            -- If the old cost had not been calculated then just use the new cost
            SET @newCostCalc = 1
            IF(@oldCostCalc = 0 AND @costCalc = 0)
            BEGIN
                SET @newCost = 0
                SET @newCostCalc = 0
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 0)
            BEGIN
                SET @newCost = @oldCost
            END
            ELSE IF(@oldCostCalc = 0 AND @costCalc = 1)
            BEGIN
                SET @newCost = @addedItemsCost
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 1)
            BEGIN
			    SET @newCost = (@oldCost * @oldQuantity + @addedItemsCost * @deltaQuantity) / (@oldQuantity + @deltaQuantity)
		    END
        END
		ELSE
		BEGIN
            -- If items are being removed from the stack then just use the old cost value
			SET @newCost = @oldCost
            SET @newCostCalc = @oldCostCalc
		END		
		
		UPDATE [Assets] SET [Quantity] = @newQuantity, [Cost] = @newCost, [CostCalc] = @newCostCalc
		WHERE [OwnerID] = @ownerID AND [CorpAsset] = @corpAsset AND [LocationID] = @stationID AND [ItemID] = @itemID AND [Status] = @status AND [ContainerID] = @containerID AND [AutoConExclude] = @autoConExclude AND [IsContainer] = 0
	END
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.27"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsAddQuantity' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.5.0.28")) < 0)
                    {
                        #region Update 'AssetsAddQuantity' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsAddQuantity 
	@ownerID		int,
	@corpAsset		bit,
	@itemID			int,
	@stationID		int,
	@systemID		int,
	@regionID		int,
	@status			int,
	@containerID	bigint,
	@autoConExclude	bit,
	@deltaQuantity	bigint,
	@addedItemsCost	decimal(18,2),
    @costCalc       bit
AS
	DECLARE @oldQuantity bigint, @newQuantity bigint
	DECLARE	@assetID bigint
	DECLARE @oldCost decimal(18, 2), @newCost decimal(18, 2)
    DECLARE @oldCostCalc bit, @newCostCalc bit
		
	SET @assetID = 0
	SELECT @oldQuantity = Quantity, @assetID = ID, @oldCost = Cost, @oldCostCalc = CostCalc
	FROM Assets
	WHERE OwnerID = @ownerID AND CorpAsset = @corpAsset AND LocationID = @stationID AND ItemID = @itemID AND Status = @status AND ContainerID = @containerID AND AutoConExclude = @autoConExclude AND IsContainer = 0
	
	IF(@assetID = 0)
	BEGIN
		INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [Cost], [CostCalc], [EveItemID], [BoughtViaContract]) 
		VALUES (@ownerID, @corpAsset, @stationID, @itemID, @systemID, @regionID, 0, @deltaQuantity, @status, @autoConExclude, 0, 0, @addedItemsCost, @costCalc, 0, 0);
	END 
	ELSE
	BEGIN
		SET @newQuantity = @oldQuantity + @deltaQuantity
		IF(@deltaQuantity > 0)
		BEGIN
            -- If new items are being added to the stack then calculate the average cost from the 
            -- old and new values.
            -- If the old cost had not been calculated then just use the new cost
            SET @newCostCalc = 1
            IF(@oldCostCalc = 0 AND @costCalc = 0)
            BEGIN
                SET @newCost = 0
                SET @newCostCalc = 0
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 0)
            BEGIN
                SET @newCost = @oldCost
            END
            ELSE IF(@oldCostCalc = 0 AND @costCalc = 1)
            BEGIN
                SET @newCost = @addedItemsCost
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 1)
            BEGIN
			    SET @newCost = (@oldCost * @oldQuantity + @addedItemsCost * @deltaQuantity) / (@oldQuantity + @deltaQuantity)
		    END
        END
		ELSE
		BEGIN
            -- If items are being removed from the stack then just use the old cost value
			SET @newCost = @oldCost
            SET @newCostCalc = @oldCostCalc
		END		
		
		UPDATE [Assets] SET [Quantity] = @newQuantity, [Cost] = ISNULL(@newCost, 0), [CostCalc] = ISNULL(@newCostCalc, 0)
		WHERE [OwnerID] = @ownerID AND [CorpAsset] = @corpAsset AND [LocationID] = @stationID AND [ItemID] = @itemID AND [Status] = @status AND [ContainerID] = @containerID AND [AutoConExclude] = @autoConExclude AND [IsContainer] = 0
	END
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.28"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsAddQuantity' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.5.0.29")) < 0)
                    {
                        #region Update 'AssetsGetContained' stored procedure
                        commandText =
                                @"ALTER PROCEDURE dbo.AssetsGetContained 
	@containerID	bigint,
	@itemID			int
AS
	SELECT * 
	FROM Assets
	WHERE ContainerID = @containerID AND (@itemID = 0 OR ItemID = @itemID)
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.29"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetContained' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.30")) < 0)
                    {
                        #region Add 'Cost' column to AssetsLost table
                        commandText =
                               @"ALTER TABLE dbo.AssetsLost
ADD Cost decimal(18,2) NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.30"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'Cost' column to AssetsLost table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.31")) < 0)
                    {
                        #region Add 'Value' column to AssetsLost table
                        commandText =
                               @"ALTER TABLE dbo.AssetsLost
ADD Value decimal(18,2) NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.31"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'Value' column to AssetsLost table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.32")) < 0)
                    {
                        #region Update 'AssetsLostNew' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsLostNew
	@OwnerID			int,
	@CorpAsset			bit,
	@ItemID				int,
	@LossDateTime	    datetime,
	@Quantity			bigint,
	@Cost				decimal(18,2),
	@Value				decimal(18,2),
	@newID				bigint		OUTPUT
AS
	SELECT @newID =
	(SELECT MAX(ID) AS MaxID
		FROM AssetsLost) + 1
		
	IF(@newID IS NULL)
	BEGIN
		SET @newID = 1
	END
	
	INSERT INTO AssetsLost (ID, OwnerID, CorpAsset, ItemID, LossDateTime, Quantity, Cost, Value)
	VALUES (@newID, @OwnerID, @CorpAsset, @ItemID, @LossDateTime, @Quantity, @Cost, @Value)

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.32"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsLostNew' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.33")) < 0)
                    {
                        #region Create 'AssetsMigrateToCorpID' stored procedure
                        commandText =
                             @"CREATE PROCEDURE dbo.AssetsMigrateToCorpID
	@charID		int,
	@corpID		int
AS
	UPDATE Assets 
	SET [OwnerID] = @corpID
	WHERE [OwnerID] = @charID AND [CorpAsset] = 1
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.33"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetsMigrateToCorpID' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.34")) < 0)
                    {
                        #region Create 'OrdersMigrateToCorpID' stored procedure
                        commandText =
                             @"CREATE PROCEDURE dbo.OrdersMigrateToCorpID
	@charID		int,
	@corpID		int
AS
	UPDATE Orders 
	SET [OwnerID] = @corpID
	WHERE [OwnerID] = @charID AND [ForCorp] = 1
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.34"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'OrdersMigrateToCorpID' stored procedure", ex);
                        }
                        #endregion
                    }

                    if (dbVersion.CompareTo(new Version("1.5.0.35")) < 0)
                    {
                        #region Update 'AssetsBuildResults' stored procedure
                        commandText =
                               @"ALTER PROCEDURE [dbo].[AssetsBuildResults]
	@accessList			varchar(max),
	@itemIDs			varchar(max),
	@status				int,
	@groupBy			varchar(50)
    --@totalRows          int         OUTPUT
AS
SET NOCOUNT ON

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tmp_AssetResults]') AND type in (N'U'))
DROP TABLE [dbo].[tmp_AssetResults]

IF(@groupBy LIKE 'Owner')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, 0 AS SystemID, 
		0 AS RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		-- This causes div by 0 errors. Just drop the inclusing of the CostCalc flag and 
		-- include quantity > 0 in the WHERE clause. Not ideal but best simple solution I can think of.
		--SUM(Assets.Cost * Assets.Quantity * Assets.CostCalc) / SUM(Assets.Quantity * Assets.CostCalc) AS Cost,
		SUM(Assets.Cost * Assets.Quantity) / SUM(Assets.Quantity) AS Cost,
		MAX(CAST(Assets.CostCalc as int)) AS CostCalc,
        0 AS EveItemID, CAST(0 AS bit) AS BoughtViaContract,
        row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0) AND Assets.Quantity > 0
	GROUP BY Assets.ItemID, Assets.OwnerID, Assets.CorpAsset
END
ELSE IF(@groupBy LIKE 'Region')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, 0 AS SystemID, 
		Assets.RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		-- This causes div by 0 errors. Just drop the inclusing of the CostCalc flag and 
		-- include quantity > 0 in the WHERE clause. Not ideal but best simple solution I can think of.
		--SUM(Assets.Cost * Assets.Quantity * Assets.CostCalc) / SUM(Assets.Quantity * Assets.CostCalc) AS Cost,
		SUM(Assets.Cost * Assets.Quantity) / SUM(Assets.Quantity) AS Cost,		
		MAX(CAST(Assets.CostCalc as int)) AS CostCalc,
        0 AS EveItemID, CAST(0 AS bit) AS BoughtViaContract,
		row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0) AND Assets.Quantity > 0
	GROUP BY Assets.ItemID, Assets.RegionID, Assets.OwnerID, Assets.CorpAsset
END
ELSE IF(@groupBy LIKE 'System')
BEGIN
	SELECT MIN(Assets.ID) AS ID, Assets.OwnerID, Assets.CorpAsset, 
		0 AS LocationID, Assets.ItemID, Assets.SystemID, 
		MAX(Assets.RegionID) AS RegionID, 0 AS ContainerID, 
		SUM(Assets.Quantity) AS Quantity, 0 AS Status, 
		CAST(0 AS bit) AS AutoConExclude, CAST(0 AS bit) AS Processed, 
		CAST(0 AS bit) AS ISContainer, CAST(0 AS bit) AS ReprocExclude, 
		-- This causes div by 0 errors. Just drop the inclusing of the CostCalc flag and 
		-- include quantity > 0 in the WHERE clause. Not ideal but best simple solution I can think of.
		--SUM(Assets.Cost * Assets.Quantity * Assets.CostCalc) / SUM(Assets.Quantity * Assets.CostCalc) AS Cost,
		SUM(Assets.Cost * Assets.Quantity) / SUM(Assets.Quantity) AS Cost,
		MAX(CAST(Assets.CostCalc as int)) AS CostCalc,
        0 AS EveItemID, CAST(0 AS bit) AS BoughtViaContract,
		row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0) AND Assets.Quantity > 0
	GROUP BY Assets.ItemID, Assets.SystemID, Assets.OwnerID, Assets.CorpAsset
END
ELSE
BEGIN
	SELECT Assets.*, row_number() OVER(ORDER BY Assets.OwnerID) as RowNumber
	INTO tmp_AssetResults
	FROM Assets 
		JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
		JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Assets.ItemID = i.number)
	WHERE (Assets.Status = @status OR @status = 0) 
END

CREATE CLUSTERED INDEX [PK_tmp_AssetResults] ON [dbo].[tmp_AssetResults] 
(
	[RowNumber]
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF) ON [PRIMARY]	
 
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.35"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsBuildResults' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.36")) < 0)
                    {
                        #region Update 'AssetsClearUnProc' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsClearUnProc
	@ownerID			int,
	@onlyContainers		bit
AS
	DECLARE	@nextID	int
	
	SELECT @nextID = MIN(ID) FROM Assets
	WHERE (IsContainer = 1) AND (OwnerID = @ownerID) AND (Processed = 0)
	
	WHILE @nextID IS NOT NULL
	BEGIN
		EXEC dbo.AssetsClearByID @nextID
		SELECT @nextID = MIN(ID) FROM Assets
		WHERE (ID > @nextID) AND (IsContainer = 1) AND (OwnerID = @ownerID) AND (Processed = 0)
	END

	IF(@onlyContainers = 0)
	BEGIN
		DELETE FROM Assets
		WHERE (OwnerID = @ownerID) AND (Processed = 0)
	END
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.36"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsClearUnProc' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.37")) < 0)
                    {
                        #region Update 'AssetsGetAutoConByAny' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetAutoConByAny
	@ownerID			int,
	@stationIDs			varchar(max),
	@regionIDs			varchar(max),
	@itemIDs			varchar(max),
	@excludeContainers	bit
AS

	SELECT Assets.*
	FROM Assets
	JOIN CLR_intlist_split(@stationIDs) s ON (Assets.LocationID = s.number OR s.number = 0)
	JOIN CLR_intlist_split(@regionIDs) r ON (Assets.RegionID = r.number OR r.number = 0)
	JOIN CLR_intlist_split(@itemIDs) i ON (Assets.ItemID = i.number OR i.number = 0)
	WHERE (OwnerID = @ownerID AND (AutoConExclude = 0) AND (Status = 1) AND 
		(@excludeContainers = 0 OR (ContainerID = 0 AND IsContainer = 0)) AND Quantity > 0)
	ORDER BY LocationID
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.37"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetAutoConByAny' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.38")) < 0)
                    {
                        #region Update 'AssetsGetAutoConByOwner' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetAutoConByOwner
	@ownerID			int,
	@stationID			int,
	@itemIDs			varchar(max),
	@excludeContainers	bit
AS

	SELECT Assets.*
	FROM Assets
	JOIN CLR_intlist_split(@itemIDs) i ON (Assets.ItemID = i.number OR i.number = 0)
	WHERE (OwnerID = @ownerID AND (LocationID = @stationID OR @stationID = 0) AND (AutoConExclude = 0) AND (Status = 1) AND (@excludeContainers = 0 OR (ContainerID = 0 AND IsContainer = 0)) AND (Quantity > 0))
	ORDER BY LocationID
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.38"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetAutoConByOwner' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.39")) < 0)
                    {
                        #region Update 'AssetsGetBoughtViaContract' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetBoughtViaContract
	@accessList			varchar(max),
	@itemID				int
AS
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	WHERE (Assets.BoughtViaContract = 1) AND (Assets.ItemID = @itemID OR @itemID = 0)
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.39"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetBoughtViaContract' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.40")) < 0)
                    {
                        #region Update 'AssetsGetByItem' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetByItem
	@accessList			varchar(max),
	@regionIDs			varchar(max),
	@stationIDs			varchar(max),
	@itemID				int,
	@includeInTransit	bit,
	@includeContainers	bit
AS
IF(NOT @regionIDs LIKE '')
BEGIN
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	JOIN CLR_intlist_split(@regionIDs) r ON (Assets.RegionID = r.number OR r.number = 0)
	JOIN CLR_intlist_split(@stationIDs) s ON (Assets.LocationID = s.number OR s.number = 0)
	WHERE (Assets.ItemID = @itemID OR @itemID = 0) AND (@includeInTransit = 1 OR NOT Assets.Status = 2) AND (@includeContainers = 1 OR (Assets.IsContainer = 0 AND Assets.ContainerID = 0))
END

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.40"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetByItem' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.41")) < 0)
                    {
                        #region Update 'AssetsGetByLocationAndItem' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetByLocationAndItem 
	@accessList			varchar(max),
	@regionIDs			varchar(max),
	@systemID			int,
	@locationID			int,
	@itemID				int,
	@containersOnly		bit,
	@getContained		bit,
	@status				int
AS
IF(NOT @regionIDs LIKE '')
BEGIN
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	JOIN CLR_intlist_split(@regionIDs) r ON Assets.RegionID = r.number 
	WHERE (Assets.Status = @status OR @status = 0) AND (Assets.SystemID = @systemID OR @systemID = 0) AND (Assets.LocationID = @locationID OR @locationID = 0) AND (Assets.ItemID = @itemID OR @itemID = 0) AND (Assets.IsContainer = 1 OR @containersOnly = 0) AND (Assets.ContainerID = 0 OR @getContained = 1)
END
ELSE
BEGIN
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	WHERE (Assets.Status = @status OR @status = 0) AND (Assets.SystemID = @systemID OR @systemID = 0) AND (Assets.LocationID = @locationID OR @locationID = 0) AND (Assets.ItemID = @itemID OR @itemID = 0) AND (Assets.IsContainer = 1 OR @containersOnly = 0) AND (Assets.ContainerID = 0 OR @getContained = 1)
END

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.41"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetByLocationAndItem' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.42")) < 0)
                    {
                        #region Update 'AssetsGetByProcessed' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetByProcessed
	@accessList			varchar(max),
	@systemID			int,
	@locationID			int,
	@itemID				int,
	@status				int,
	@processed			bit
AS
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	WHERE (Assets.Status = @status OR @status = 0) AND (Assets.SystemID = @systemID OR @systemID = 0) AND (Assets.LocationID = @locationID OR @locationID = 0) AND (Assets.ItemID = @itemID OR @itemID = 0) AND (Assets.Processed = @processed)

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.42"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetByProcessed' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.43")) < 0)
                    {
                        #region Update 'AssetsGetItemAndContainersOfItem' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetItemAndContainersOfItem
	@accessList			varchar(max),
	@regionIDs			varchar(max),
	@systemID			int,
	@locationID			int,
	@itemID				int,
	@containersOnly		bit,
	@getContained		bit,
	@status				int
AS
IF(NOT @regionIDs LIKE '')
BEGIN
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	JOIN CLR_intlist_split(@regionIDs) r ON Assets.RegionID = r.number 
	WHERE (Assets.Status = @status OR @status = 0) AND (Assets.SystemID = @systemID OR @systemID = 0) AND (Assets.LocationID = @locationID OR @locationID = 0) AND (Assets.ItemID = @itemID OR @itemID = 0 OR (Assets.IsContainer = 1 AND dbo.AssetContains(Assets.ID, @itemID) = 1)) AND (Assets.IsContainer = 1 OR @containersOnly = 0) AND (Assets.ContainerID = 0 OR @getContained = 1)
END
ELSE
BEGIN
	SELECT Assets.*
	FROM Assets 
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	WHERE (Assets.Status = @status OR @status = 0) AND (Assets.SystemID = @systemID OR @systemID = 0) AND (Assets.LocationID = @locationID OR @locationID = 0) AND (Assets.ItemID = @itemID OR @itemID = 0 OR (Assets.IsContainer = 1 AND dbo.AssetContains(Assets.ID, @itemID) = 1)) AND (Assets.IsContainer = 1 OR @containersOnly = 0) AND (Assets.ContainerID = 0 OR @getContained = 1)
END
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.43"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetItemAndContainersOfItem' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.44")) < 0)
                    {
                        #region Update 'AssetsGetItemIDs' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetItemIDs
	@accessList		varchar(max),
	@locationID		int
AS
	SELECT ItemID AS [ID]
	FROM Assets
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	WHERE Assets.Quantity > 0 AND (LocationID = @locationID OR @locationID = 0)
	GROUP BY ItemID
		
	RETURN
";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.44"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetItemIDs' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.45")) < 0)
                    {
                        #region Update 'AssetsGetLimitedSystemIDs' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetLimitedSystemIDs
	@ownerID			int,
	@regionIDs			varchar(MAX),
	@stationIDs			varchar(MAX),
	@includeContainers	bit,
	@includeContents	bit
AS
	SELECT SystemID AS [ID]
	FROM Assets
		JOIN CLR_intlist_split(@regionIDs) r ON (Assets.RegionID = r.number OR r.number = 0)
		JOIN CLR_intlist_split(@stationIDs) s ON (Assets.LocationID = s.number OR s.number = 0)
	WHERE (OwnerID = @ownerID) AND (Assets.ContainerID = 0 OR @includeContents = 1) AND
		(Assets.IsContainer = 0 OR @includeContainers = 1)
	GROUP BY SystemID
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.45"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetLimitedSystemIDs' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.46")) < 0)
                    {
                        #region Update 'AssetsGetRegionIDs' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetRegionIDs
	@accessList		varchar(MAX),
	@itemID			int
AS
	SELECT RegionID AS [ID]
	FROM Assets
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	WHERE (ItemID = @itemID OR @itemID = 0)
	GROUP BY RegionID
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.46"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetRegionIDs' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.47")) < 0)
                    {
                        #region Update 'AssetsGetReproc' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetReproc		
	@ownerID				int,
    @stationID				int,
    @status					int,
    @includeContainers		bit,
    @includeNonContainers   bit
AS
	SELECT *
    FROM Assets
	WHERE (ReprocExclude = 0) AND (OwnerID = @ownerID) AND (LocationID = @stationID) AND (Status = @status) AND (ContainerID = 0) AND ((@includeContainers = 1 AND @includeNonContainers = 1) OR (@includeContainers = 1 AND IsContainer = 1) OR (@includeNonContainers = 1 AND IsContainer = 0)) 
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.47"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetReproc' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.48")) < 0)
                    {
                        #region Update 'AssetsGetStationIDs' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetStationIDs
	@accessList		varchar(MAX),
	@itemID			int,
	@systemID		int
AS
	SELECT LocationID AS [ID]
	FROM Assets
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	WHERE (ItemID = @itemID OR @itemID = 0) AND (SystemID = @systemID OR @systemID = 0) AND (SystemID != LocationID)
	GROUP BY LocationID
	
	RETURN
";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.48"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetStationIDs' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.49")) < 0)
                    {
                        #region Update 'AssetsGetSystemIDs' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsGetSystemIDs
	@accessList		varchar(MAX),
	@itemID			int,
	@regionID		int
AS
	SELECT SystemID AS [ID]
	FROM Assets
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	WHERE (ItemID = @itemID OR @itemID = 0) AND (RegionID = @regionID OR @regionID = 0) 
	GROUP BY SystemID
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.49"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetSystemIDs' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.50")) < 0)
                    {
                        #region Update 'AssetsHistoryGetClosest' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsHistoryGetClosest
	@ownerID		int,
	@datetime		datetime
AS
	SELECT AssetsHistory.*
	FROM AssetsHistory
	WHERE (OwnerID = @ownerID) AND
		(ABS(DATEDIFF(hh, @datetime, Date)) = 
		(
			SELECT MIN(ABS(DATEDIFF(hh, @datetime, Date)))
			FROM AssetsHistory
			WHERE (OwnerID = @ownerID)
		))
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.50"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsHistoryGetClosest' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.51")) < 0)
                    {
                        #region Update 'AssetsSetExcludeFlag' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsSetExcludeFlag		
	@assetID		bigint,
	@ownerID		int,
	@locationID		int,
	@itemID			int,
	@status			int,
	@containerID	bigint,
	@exclude		bit
AS
	UPDATE Assets
	SET AutoConExclude = @exclude
	WHERE (ID = @assetID OR (@AssetID = 0 AND (OwnerID = @ownerID) AND (LocationID = @locationID OR @locationID = 0) AND (ItemID = @itemID OR @itemID = 0) AND (ContainerID = @containerID OR @containerID = 0) AND (Status = @status)))
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.51"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsSetExcludeFlag' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.52")) < 0)
                    {
                        #region Update 'AssetsSetProcFlag' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsSetProcFlag
	@assetID	bigint,
	@ownerID	int,
	@status		int,
	@processed	bit
AS
	UPDATE Assets
	SET Processed = @processed
	WHERE (ID = @assetID OR (@assetID = 0 AND (OwnerID = @ownerID) AND (Status = @status OR @status = 0)))
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.52"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsSetProcFlag' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.53")) < 0)
                    {
                        #region Update 'AssetsSetReprocExclude' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsSetReprocExclude		
	@assetID		bigint,
	@ownerID		int,
	@locationID		int,
	@itemID			int,
	@status			int,
	@containerID	bigint,
	@exclude		bit
AS
	UPDATE Assets
	SET ReprocExclude = @exclude
	WHERE (ID = @assetID OR (@AssetID = 0 AND (OwnerID = @ownerID) AND (LocationID = @locationID OR @locationID = 0) AND (ItemID = @itemID OR @itemID = 0) AND (ContainerID = @containerID OR @containerID = 0) AND (Status = @status)))
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.53"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsSetReprocExclude' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.54")) < 0)
                    {
                        #region Update 'AssetsTotalQuantityByAny' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsTotalQuantityByAny 
	@accessList			varchar(MAX),
	@itemID				int,
	@stationIDs			varchar(MAX),
	@regionIDs			varchar(MAX),
	@includeInTransit	bit,	
	@includeContainers	bit,
	@totQuantity		bigint OUTPUT
AS
IF(@stationIDs LIKE '0' AND @regionIDs LIKE '0')
BEGIN
	SELECT @totQuantity = SUM(Quantity)
	FROM Assets
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	WHERE ItemID = @itemID AND (@includeInTransit = 1 OR Status = 1) AND (@includeContainers = 1 OR (ContainerID = 0 AND IsContainer = 0))
END
ELSE IF(@regionIDs LIKE '0')
BEGIN
	SELECT @totQuantity = SUM(Quantity)
	FROM Assets
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	JOIN CLR_intlist_split(@stationIDs) s ON (Assets.LocationID = s.number)
	WHERE ItemID = @itemID AND (@includeInTransit = 1 OR Status = 1)AND (@includeContainers = 1 OR (ContainerID = 0 AND IsContainer = 0))
END
ELSE IF(@stationIDs LIKE '0')
BEGIN
	SELECT @totQuantity = SUM(Quantity)
	FROM Assets
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	JOIN CLR_intlist_split(@regionIDs) r ON (Assets.RegionID = r.number)
	WHERE ItemID = @itemID AND (@includeInTransit = 1 OR Status = 1)AND (@includeContainers = 1 OR (ContainerID = 0 AND IsContainer = 0))
END
ELSE
BEGIN
	SELECT @totQuantity = SUM(Quantity)
	FROM Assets
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	JOIN CLR_intlist_split(@regionIDs) r ON (Assets.RegionID = r.number)
	JOIN CLR_intlist_split(@stationIDs) s ON (Assets.LocationID = s.number)
	WHERE ItemID = @itemID AND (@includeInTransit = 1 OR Status = 1)AND (@includeContainers = 1 OR (ContainerID = 0 AND IsContainer = 0))
END
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.54"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsTotalQuantityByAny' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.55")) < 0)
                    {
                        #region Update 'AssetsTotalsTable' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsTotalsTable 
	@accessList		varchar(MAX),
	@autoConOnly	bit
AS

	SELECT LocationID, ItemID, Sum(Quantity) 
	FROM Assets
	JOIN CLR_intlist_split(@accessList) a ON (Assets.OwnerID = a.number)
	WHERE Status = 1 AND (AutoConExclude != @autoConOnly OR @autoConOnly = 0)
	GROUP BY LocationID, ItemID 
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.55"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsTotalsTable' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.56")) < 0)
                    {
                        #region Update 'OrderExists' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.OrderExists
	@ownerID		int,
	@walletID		smallint,
	@stationID		int,
	@itemID			int,
	@totalVol		int,
	@remainingVol	int,
	@range			smallint,
	@orderState		smallint,
	@buyOrder		bit,
	@price			decimal(18,2),
	@eveOrderID		bigint,
	@exists			bit		OUT,
	@orderID		int		OUT
AS
	SET @exists = 0
	SET	@orderID = 0

	SELECT @orderID = ID
	FROM Orders
	WHERE (OwnerID = @ownerID) AND (WalletID = @walletID) AND 
		(StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
		(TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND ((OrderState = @orderState) OR 
		(@orderState = 2 AND (OrderState = 2 OR OrderState = 1000 OR OrderState = 2000))) AND
		(Processed = 0) AND (Price = @price) AND (EveOrderID = @eveOrderID)
	IF(@orderID = 0 AND @orderState = 2) 
	BEGIN
		-- If we are trying to find a filled/expired order but failed first time around then try
		-- looking for an active order (i.e. one that was active and is now competed/expired)
		SELECT @orderID = ID
		FROM Orders
		WHERE (OwnerID = @ownerID) AND (WalletID = @walletID) AND 
			(StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
			(TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND (Processed = 0) AND
			(OrderState = 0 OR OrderState = 999) AND (Price = @price) AND (EveOrderID = @eveOrderID)
	END
	IF(@orderID = 0) 
	BEGIN
		-- Try matching to an order in the database that matches all other parameters but has a blank eve ID.
		-- This can happen if the user has recently installed the update that adds EveOrderID to the Orders table.
		SELECT @orderID = ID
		FROM Orders
		WHERE (OwnerID = @ownerID) AND (WalletID = @walletID) AND 
			(StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
			(TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND ((OrderState = @orderState) OR 
			(@orderState = 2 AND (OrderState = 2 OR OrderState = 1000 OR OrderState = 2000))) AND
			(Processed = 0) AND (Price = @price) AND (EveOrderID = 0)
	    IF(@orderID = 0) 
	    BEGIN
		    -- Still couldn't match an order so try finding one that matches all parameters except price and 
		    -- eve order id (changing the price changes the ID).
		    SELECT @orderID = ID
		    FROM Orders
		    WHERE (OwnerID = @ownerID) AND (WalletID = @walletID) AND 
			    (StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
			    (TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND (Processed = 0) AND 
			    ((OrderState = @orderState) OR (@orderState = 2 AND 
			    (OrderState = 2 OR OrderState = 1000 OR OrderState = 2000)))
	        IF(@orderID = 0) 
	        BEGIN
		        -- Still couldn't match an order so try finding one that matches all parameters except state and price.
		        SELECT @orderID = ID
		        FROM Orders
		        WHERE (OwnerID = @ownerID) AND (WalletID = @walletID) AND 
			        (StationID = @stationID) AND (ItemID = @itemID) AND (Range = @range) AND (BuyOrder = @buyOrder) AND
			        (TotalVol = @totalVol) AND (RemainingVol >= @remainingVol) AND (Processed = 0)
	        END
	    END
	END
	
	UPDATE Orders
	SET Processed = 1
	WHERE (ID = @orderID)
		
	SELECT Orders.*
	FROM Orders
	WHERE (ID = @orderID)

	IF(@@ROWCOUNT = 1)
	BEGIN
		SET @exists = 1
	END

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.56"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'OrderExists' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.57")) < 0)
                    {
                        #region Update 'OrderGetAny' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.OrderGetAny
@accessList			varchar(max),
@itemIDs			varchar(max),
@stationIDs			varchar(max),
@state				int,
@type				varchar(4)
AS
SET NOCOUNT ON
SELECT Orders.*
FROM Orders
JOIN CLR_intlist_split(@accessList) a ON (Orders.OwnerID = a.number)
JOIN CLR_intlist_split(@itemIDs) i ON (Orders.ItemID = i.number OR i.number = 0)
JOIN CLR_intlist_split(@stationIDs) s ON (Orders.StationID = s.number OR s.number = 0)
WHERE (Orders.OrderState = @state OR @state = 0) AND ((@type LIKE 'Sell' AND Orders.BuyOrder = 0) OR (@type LIKE 'Buy' AND Orders.BuyOrder = 1) OR (@type NOT LIKE 'Buy' AND @type NOT LIKE 'Sell'))
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.57"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'OrderGetAny' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.58")) < 0)
                    {
                        #region Update 'OrderGetByAnySingle' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.OrderGetByAnySingle
@ownerID			int,
@walletID			smallint,
@itemID			int,
@stationID			int,
@state             int,
@type              varchar(4)
AS
SET NOCOUNT ON
SELECT Orders.*
FROM Orders
WHERE (Orders.OwnerID = @ownerID) AND (@walletID = 0 OR WalletID = @walletID)
   AND (@state = 0 OR Orders.OrderState = @state) AND (@itemID = 0 OR Orders.ItemID = @itemID)
   AND (@stationID = 0 OR Orders.StationID = @stationID)
   AND ((@type LIKE 'Sell' AND Orders.BuyOrder = 0) OR (@type LIKE 'Buy' AND Orders.BuyOrder = 1) OR (@type NOT LIKE 'Buy' AND @type NOT LIKE 'Sell'))
RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.58"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'OrderGetByAnySingle' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.59")) < 0)
                    {
                        #region Update 'OrdersFinishUnProcessed' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.OrdersFinishUnProcessed 
	@ownerID		int,
	@notify			bit,
	@notifyBuy		bit,
	@notifySell		bit
AS
	UPDATE Orders
	SET OrderState = 1000, Escrow = 0
	WHERE (OwnerID = @ownerID) AND (Processed = 0) AND (OrderState = 999 OR OrderState = 2) AND
		(@notify = 1 AND ((@notifyBuy = 1 AND BuyOrder = 1) OR (@notifySell = 1 AND BuyOrder = 0)))
		
	UPDATE Orders
	SET OrderState = 2000, Escrow = 0
	WHERE (OwnerID = @ownerID) AND (Processed = 0) AND (OrderState = 999 OR OrderState = 2) AND
		(@notify = 0 OR ((@notifyBuy = 0 AND BuyOrder = 1) OR (@notifySell = 0 AND BuyOrder = 0)))
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.59"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'OrdersFinishUnProcessed' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.60")) < 0)
                    {
                        #region Update 'OrdersGetByDate' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.OrdersGetByDate
	@ownerID				int,
	@walletID				smallint,
	@earliestIssueDate		datetime,
	@latestIssueDate		datetime
AS
	SELECT Orders.*
	FROM Orders 
	WHERE (OwnerID = @ownerID) AND (WalletID = @walletID) AND 
		(Issued BETWEEN @earliestIssueDate AND @latestIssueDate)
	RETURN
";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.60"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'OrdersGetByDate' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.61")) < 0)
                    {
                        #region Update 'OrdersSetProcessed' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.OrdersSetProcessed 
	@ownerID	int,
	@processed	bit
AS
	UPDATE Orders
	SET Processed = @processed
	WHERE (OwnerID = @ownerID)
 
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.61"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'OrdersSetProcessed' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.62")) < 0)
                    {
                        #region Update 'AssetsAddQuantity' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetsAddQuantity 
	@ownerID		int,
	@itemID			int,
	@stationID		int,
	@systemID		int,
	@regionID		int,
	@status			int,
	@containerID	bigint,
	@autoConExclude	bit,
	@deltaQuantity	bigint,
	@addedItemsCost	decimal(18,2),
    @costCalc       bit
AS
	DECLARE @oldQuantity bigint, @newQuantity bigint
	DECLARE	@assetID bigint
	DECLARE @oldCost decimal(18, 2), @newCost decimal(18, 2)
    DECLARE @oldCostCalc bit, @newCostCalc bit
		
	SET @assetID = 0
	SELECT @oldQuantity = Quantity, @assetID = ID, @oldCost = Cost, @oldCostCalc = CostCalc
	FROM Assets
	WHERE OwnerID = @ownerID AND LocationID = @stationID AND ItemID = @itemID AND Status = @status AND ContainerID = @containerID AND AutoConExclude = @autoConExclude AND IsContainer = 0
	
	IF(@assetID = 0)
	BEGIN
		INSERT INTO [Assets] ([OwnerID], [CorpAsset], [LocationID], [ItemID], [SystemID], [RegionID], [ContainerID], [Quantity], [Status], [AutoConExclude], [Processed], [IsContainer], [Cost], [CostCalc], [EveItemID], [BoughtViaContract]) 
		VALUES (@ownerID, 0, @stationID, @itemID, @systemID, @regionID, 0, @deltaQuantity, @status, @autoConExclude, 0, 0, @addedItemsCost, @costCalc, 0, 0);
	END 
	ELSE
	BEGIN
		SET @newQuantity = @oldQuantity + @deltaQuantity
		IF(@deltaQuantity > 0)
		BEGIN
            -- If new items are being added to the stack then calculate the average cost from the 
            -- old and new values.
            -- If the old cost had not been calculated then just use the new cost
            SET @newCostCalc = 1
            IF(@oldCostCalc = 0 AND @costCalc = 0)
            BEGIN
                SET @newCost = 0
                SET @newCostCalc = 0
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 0)
            BEGIN
                SET @newCost = @oldCost
            END
            ELSE IF(@oldCostCalc = 0 AND @costCalc = 1)
            BEGIN
                SET @newCost = @addedItemsCost
            END
            ELSE IF(@oldCostCalc = 1 AND @costCalc = 1)
            BEGIN
				IF(@oldQuantity > 0)
				BEGIN
					SET @newCost = (@oldCost * @oldQuantity + @addedItemsCost * @deltaQuantity) / (@oldQuantity + @deltaQuantity)
				END
				ELSE
				BEGIN
					SET @newCost = @addedItemsCost
				END
		    END
        END
		ELSE
		BEGIN
            -- If items are being removed from the stack then just use the old cost value
			SET @newCost = @oldCost
            SET @newCostCalc = @oldCostCalc
		END		
		
		UPDATE [Assets] SET [Quantity] = @newQuantity, [Cost] = ISNULL(@newCost, 0), [CostCalc] = ISNULL(@newCostCalc, 0)
		WHERE [OwnerID] = @ownerID AND [LocationID] = @stationID AND [ItemID] = @itemID AND [Status] = @status AND [ContainerID] = @containerID AND [AutoConExclude] = @autoConExclude AND [IsContainer] = 0
	END
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.62"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsAddQuantity' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.63")) < 0)
                    {
                        #region Update 'AssetExists' stored procedure
                        commandText =
                               @"ALTER PROCEDURE dbo.AssetExists 
	@ownerID			int,
	@locationID			int,
	@itemID				int,
	@status				int,
	@isContained		bit,
	@containerID		bigint,
	@isContainer		bit,
	@processed			bit,			
	@ignoreProcessed	bit,
	@autoConExclude		bit,
	@ignoreAutoConEx	bit,
	@eveItemID			bigint,
	@exists				bit			OUT,
	@assetID			bigint		OUT
AS
	SET @exists = 0
	SET	@assetID = 0

	SELECT @assetID = ID
	FROM Assets
	WHERE (OwnerID = @ownerID) AND (LocationID = @locationID) AND (ItemID = @itemID) AND (Status = @status) AND (ContainerID = @containerID OR (@isContained = 1 AND @containerID = 0 AND ContainerID != 0)) AND (IsContainer = @isContainer) AND (Processed = @processed OR @ignoreProcessed = 1) AND (AutoConExclude = @autoConExclude OR @ignoreAutoConEx = 1) AND (EveItemID = @eveItemID OR @eveItemID = 0)
	IF(@eveItemID > 0 AND @assetID = 0)
	BEGIN
		-- If eve ID is set and we didn't find a match with it then
		-- just try without it.
		SELECT @assetID = ID
		FROM Assets
		WHERE (OwnerID = @ownerID) AND (LocationID = @locationID) AND (ItemID = @itemID) AND (Status = @status) AND (ContainerID = @containerID OR (@isContained = 1 AND @containerID = 0 AND ContainerID != 0)) AND (IsContainer = @isContainer) AND (Processed = @processed OR @ignoreProcessed = 1) AND (AutoConExclude = @autoConExclude OR @ignoreAutoConEx = 1)
	END
	SELECT *
	FROM Assets
	WHERE (ID = @assetID)

	IF(@@ROWCOUNT >= 1)
	BEGIN
		SET @exists = 1
	END

	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.63"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetExists' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.0.64")) < 0)
                    {
                        #region Delete assets 'ItTransit' and 'ForSaleViaContract'
                        commandText =
                               @"DELETE FROM Assets
	WHERE Status = 2 OR Status = 3";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.0.64"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem deleting assets 'ItTransit' and 'ForSaleViaContract'", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.0")) < 0)
                    {
                        SetDBVersion(connection, new Version("1.5.1.0"));
                    }
                    #endregion
                }
                if (dbVersion.CompareTo(new Version("2.0.0.0")) < 0)
                {
                    if (dbVersion.CompareTo(new Version("1.5.1.1")) < 0)
                    {
                        #region Create IndustryJobs table
                        commandText =
                               @"CREATE TABLE [dbo].[IndustryJobs](
	[ID] [bigint] NOT NULL,
	[AssemblyLineID] [int] NOT NULL,
	[ContainerID] [int] NOT NULL,
	[InstalledItemID] [int] NOT NULL,
	[InstalledItemLocationID] [int] NOT NULL,
	[InstalledItemQuantity] [int] NOT NULL,
	[InstalledItemPL] [int] NOT NULL,
	[InstalledItemME] [int] NOT NULL,
	[InstalledItemRunsRemaining] [int] NOT NULL,
	[OutputLcoationID] [int] NOT NULL,
	[InstallerID] [int] NOT NULL,
	[JobRuns] [int] NOT NULL,
	[OutputRuns] [int] NOT NULL,
	[MaterialModifier] [float] NOT NULL,
	[CharMaterialModifier] [float] NOT NULL,
	[TimeMultiplier] [float] NOT NULL,
	[CharTimeMultiplier] [float] NOT NULL,
	[InstalledItemTypeID] [int] NOT NULL,
	[OutputTypeID] [int] NOT NULL,
	[ContainerTypeID] [int] NOT NULL,
	[InstalledItemCopy] [bit] NOT NULL,
	[Completed] [bit] NOT NULL,
	[CompletedSuccessfully] [bit] NOT NULL,
	[InstalledItemFlag] [int] NOT NULL,
	[OutputFlag] [int] NOT NULL,
	[ActivityID] [int] NOT NULL,
	[CompletedStatus] [int] NOT NULL,
	[InstallTime] [datetime] NOT NULL,
	[BeginProductionTime] [datetime] NOT NULL,
	[EndProductionTime] [datetime] NOT NULL,
	[PauseProductionTime] [datetime] NOT NULL,
 CONSTRAINT [PK_IndustryJobs] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.1"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'Industry Jobs' table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.2")) < 0)
                    {
                        #region Create indexes for IndustryJobs table
                        commandText =
                               @"CREATE NONCLUSTERED INDEX [IX_IndustryJobs_InstallerOutput] ON [dbo].[IndustryJobs] 
(
	[InstallerID] ASC,
	[OutputTypeID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_IndustryJobs_InstallerID] ON [dbo].[IndustryJobs] 
(
	[InstallerID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_IndustryJobs_InstallerBlueprint] ON [dbo].[IndustryJobs] 
(
	[InstallerID] ASC,
	[InstalledItemTypeID] ASC,
	[InstalledItemID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.2"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating indexes for 'Industry Jobs' table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.3")) < 0)
                    {
                        #region Create IndustryJobsGetByBlueprint stored proc
                        commandText =
                               @"CREATE PROCEDURE dbo.IndustryJobsGetByBlueprint
	@installerID			int,
	@blueprintTypeID		int,
	@blueprintEveAssetID	int,
	@jobStartDateAfter		datetime,
	@jobEndDateBefore		datetime
AS
	SELECT *
	FROM IndustryJobs
	WHERE InstallerID = @installerID AND InstalledItemTypeID = @blueprintTypeID AND (InstalledItemID = @blueprintEveAssetID OR @blueprintEveAssetID = 0) AND InstallTime > @jobStartDateAfter AND EndProductionTime < @jobEndDateBefore
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.3"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'IndustryJobsGetByBlueprint' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.4")) < 0)
                    {
                        #region Create IndustryJobsGetByProduct stored proc
                        commandText =
                               @"CREATE PROCEDURE dbo.IndustryJobsGetByProduct
	@installerID			int,
	@productTypeID			int,
	@jobCompletedAfter		datetime
AS
	SELECT *
	FROM IndustryJobs
	WHERE InstallerID = @installerID AND OutputTypeID = @productTypeID AND EndProductionTime > @jobCompletedAfter
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.4"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'IndustryJobsGetByProduct' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.5")) < 0)
                    {
                        #region Add 'AutoUpdateIndustryJobs' column to RptGroupChars table
                        commandText =
                               @"ALTER TABLE dbo.RptGroupChars
ADD AutoUpdateIndustryJobs bit NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.5"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'AutoUpdateIndustryJobs' column to RptGroupChars table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.6")) < 0)
                    {
                        #region Add 'AutoUpdateIndustryJobs' column to RptGroupCorps table
                        commandText =
                               @"ALTER TABLE dbo.RptGroupCorps
ADD AutoUpdateIndustryJobs bit NOT NULL DEFAULT 0";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.6"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem adding 'AutoUpdateIndustryJobs' column to RptGroupCorps table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.7")) < 0)
                    {
                        #region Updating RptGroupSetHasCorp stored proc
                        commandText =
                               @"ALTER PROCEDURE dbo.RptGroupSetHasCorp 
	@rptGroupID		int,
	@apiCorpID		int,
	@included		bit,
	@autoTrans		bit,
	@autoJournal	bit,
	@autoAssets		bit,
	@autoOrders		bit,
	@autoIndustry	bit,
	@apiCharID		int
AS
	DELETE FROM	RptGroupCorps
	WHERE	ReportGroupID = @rptGroupID AND APICorpID = @apiCorpID AND APICharID = @apiCharID

	IF(@included = 1)
	BEGIN
		INSERT INTO RptGroupCorps (ReportGroupID, APICorpID, AutoUpdateTrans, AutoUpdateJournal, AutoUpdateAssets, AutoUpdateOrders, APICharID, AutoUpdateIndustryJobs) 
		VALUES (@rptGroupID, @apiCorpID, @autoTrans, @autoJournal, @autoAssets, @autoOrders, @apiCharID, @autoIndustry)
	END
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.7"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'RptGroupSetHasCorp' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.8")) < 0)
                    {
                        #region Updating RptGroupSetHasChar stored proc
                        commandText =
                               @"ALTER PROCEDURE dbo.RptGroupSetHasChar 
	@rptGroupID		int,
	@apiCharID		int,
	@included		bit,
	@autoTrans		bit,
	@autoJournal	bit,
	@autoAssets		bit,
	@autoOrders		bit,
	@autoIndustry	bit
AS
	DELETE FROM	RptGroupChars
	WHERE	ReportGroupID = @rptGroupID AND APICharID = @apiCharID

	IF(@included = 1)
	BEGIN
		INSERT INTO RptGroupChars (ReportGroupID, APICharID, AutoUpdateTrans, AutoUpdateJournal, AutoUpdateAssets, AutoUpdateOrders, AutoUpdateIndustryJobs) 
		VALUES (@rptGroupID, @apiCharID, @autoTrans, @autoJournal, @autoAssets, @autoOrders, @autoIndustry)
	END
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.8"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'RptGroupSetHasChar' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.9")) < 0)
                    {
                        #region Create IndustryJobGetByID stored proc
                        commandText =
                               @"CREATE PROCEDURE dbo.IndustryJobGetByID 
	@jobID			bigint
AS
	SELECT *
	FROM IndustryJobs
	WHERE ID = @jobID
	
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.9"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'IndustryJobGetByID' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.10")) < 0)
                    {
                        #region A bug had caused any items bought from contract to have a negative cost. This corrects the data.
                        commandText =
                               @"UPDATE    Assets
SET              Cost = Cost * - 1
WHERE     (Cost < 0)";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.10"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem correcting Asset data", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.11")) < 0)
                    {
                        #region Updating JournalGetClosest stored proc
                        commandText =
                               @"ALTER PROCEDURE dbo.JournalGetClosest 
	@ownerID	int,
	@corp		bit,
	@walletID	smallint,
	@datetime	datetime
AS
	SELECT Journal.*
	FROM Journal
	WHERE ((((@corp = 0 AND SenderID = @ownerID) OR (@corp = 1 AND SCorpID = @ownerID)) AND (SWalletID = @walletID OR @walletID = 0)) OR
		(((@corp = 0 AND RecieverID = @ownerID) OR (@corp = 1 AND RCorpID = @ownerID)) AND (RWalletID = @walletID OR @walletID = 0))) AND
		(ABS(DATEDIFF(s, @datetime, Date)) = 
		(
			SELECT MIN(ABS(DATEDIFF(s, @datetime, Date)))
			FROM Journal
			WHERE ((((@corp = 0 AND SenderID = @ownerID) OR (@corp = 1 AND SCorpID = @ownerID)) AND (SWalletID = @walletID OR @walletID = 0)) OR
				(((@corp = 0 AND RecieverID = @ownerID) OR (@corp = 1 AND RCorpID = @ownerID)) AND (RWalletID = @walletID OR @walletID = 0)))
		))
 
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.11"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'JournalGetClosest' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.12")) < 0)
                    {
                        #region Creating AssetsLostGetByDate stored proc
                        commandText =
                               @"CREATE PROCEDURE dbo.AssetsLostGetByDate	
	@ownerID		int,
	@startDate		datetime,
	@endDate		datetime
AS
	SELECT *
	FROM AssetsLost
	WHERE OwnerID = @ownerID AND LossDateTime BETWEEN @startDate AND @endDate
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.12"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetsLostGetByDate' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.13")) < 0)
                    {
                        #region Creating AssetsProducedGetByDate stored proc
                        commandText =
                               @"CREATE PROCEDURE dbo.AssetsProducedGetByDate	
	@ownerID		int,
	@startDate		datetime,
	@endDate		datetime
AS
	SELECT *
	FROM AssetsProduced
	WHERE OwnerID = @ownerID AND ProductionDateTime BETWEEN @startDate AND @endDate
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.13"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating 'AssetsProducedGetByDate' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.14")) < 0)
                    {
                        #region Updating JournalGetClosest stored proc
                        commandText =
                               @"ALTER PROCEDURE dbo.JournalGetClosest 
	@ownerID	int,
	@corp		bit,
	@walletID	smallint,
	@datetime	datetime
AS
	SELECT Journal.*
	FROM Journal
	WHERE ((((@corp = 0 AND SenderID = @ownerID AND SCorpID = 0) OR (@corp = 1 AND SCorpID = @ownerID)) AND (SWalletID = @walletID OR @walletID = 0)) OR
		(((@corp = 0 AND RecieverID = @ownerID AND RCorpID = 0) OR (@corp = 1 AND RCorpID = @ownerID)) AND (RWalletID = @walletID OR @walletID = 0))) AND
		(ABS(DATEDIFF(s, @datetime, Date)) = 
		(
			SELECT MIN(ABS(DATEDIFF(s, @datetime, Date)))
			FROM Journal
			WHERE ((((@corp = 0 AND SenderID = @ownerID AND SCorpID = 0) OR (@corp = 1 AND SCorpID = @ownerID)) AND (SWalletID = @walletID OR @walletID = 0)) OR
				(((@corp = 0 AND RecieverID = @ownerID AND RCorpID = 0) OR (@corp = 1 AND RCorpID = @ownerID)) AND (RWalletID = @walletID OR @walletID = 0)))
		))
 
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.14"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'JournalGetClosest' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.15")) < 0)
                    {
                        #region Creating Blueprints table
                        commandText =
                               @"CREATE TABLE [dbo].[Blueprints](
	[AssetID] [bigint] NOT NULL,
	[PE] [int] NOT NULL,
	[ME] [int] NOT NULL,
	[BPO] [bit] NOT NULL,
	[Value] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_Blueprints] PRIMARY KEY CLUSTERED 
(
	[AssetID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]";

                        adapter = new SqlDataAdapter(commandText, connection);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.15"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem creating Blueprints table", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.1.16")) < 0)
                    {
                        #region Updating AssetsGetLimitedSystemIDs stored proc
                        commandText =
                           @"ALTER PROCEDURE dbo.AssetsGetLimitedSystemIDs
	@ownerID			int,
	@regionIDs			varchar(MAX),
	@stationIDs			varchar(MAX),
	@itemID				int,
	@minQuantity		int,
	@includeContainers	bit,
	@includeContents	bit
AS
	SELECT SystemID AS [ID]
	FROM Assets
		JOIN CLR_intlist_split(@regionIDs) r ON (Assets.RegionID = r.number OR r.number = 0)
		JOIN CLR_intlist_split(@stationIDs) s ON (Assets.LocationID = s.number OR s.number = 0)
	WHERE (OwnerID = @ownerID) AND (Assets.ContainerID = 0 OR @includeContents = 1) AND
		(Assets.IsContainer = 0 OR @includeContainers = 1) AND (Assets.ItemID = @itemID OR @itemID = 0) AND
		(Assets.Quantity > @minQuantity)
	GROUP BY SystemID
	RETURN";

                        adapter = new SqlDataAdapter(commandText, connection);

                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();

                            SetDBVersion(connection, new Version("1.5.1.16"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem updating 'AssetsGetLimitedSystemIDs' stored procedure", ex);
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.2.999")) < 0)
                    {
                        #region Update CLR assembly
                        if (!File.Exists(Globals.AppDataDir + "CLRStoredProcs_1_5_3_0.dll"))
                        {
                            throw new EMMAException(ExceptionSeverity.Critical, "Need to update database to version 1.5.3.0. However, the expected SQL CLR functions assembly (" +
                                Globals.AppDataDir + "CLRStoredProcs_1_5_3_0.dll) is missing. Unable to continue. If you have just updated EMMA, please update again and then retry.");
                        }
                        try
                        {
                            // First remove the old functions and assembly...
                            commandText = @"/****** Object:  UserDefinedFunction [dbo].[CLR_financelist_split]    Script Date: 10/11/2010 12:54:28 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CLR_financelist_split]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CLR_financelist_split]

/****** Object:  UserDefinedFunction [dbo].[CLR_intlist_split]    Script Date: 10/11/2010 12:54:31 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CLR_intlist_split]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CLR_intlist_split]

/****** Object:  UserDefinedFunction [dbo].[CLR_bigintlist_split]    Script Date: 10/11/2010 12:54:31 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CLR_bigintlist_split]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CLR_bigintlist_split]

/****** Object:  UserDefinedFunction [dbo].[CLR_charlist_split]    Script Date: 10/11/2010 12:54:25 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CLR_charlist_split]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CLR_charlist_split]

/****** Object:  UserDefinedFunction [dbo].[CLR_accesslist_split]    Script Date: 10/11/2010 12:54:22 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CLR_accesslist_split]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[CLR_accesslist_split]

/****** Object:  SqlAssembly [CLRStoredProcs]    Script Date: 10/11/2010 12:55:02 ******/
IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'CLRStoredProcs' and is_user_defined = 1)
DROP ASSEMBLY [CLRStoredProcs]";

                            try { server.ConnectionContext.ExecuteNonQuery(commandText); }
                            catch (Exception ex)
                            {
                                throw new EMMADataException(ExceptionSeverity.Critical,
                                    "Problem removing old CLRStoredProcs assembly", ex);
                            }

                            // Next, deploy the new assembly and re-create the functions.
////                            commandText = @"/****** Object:  SqlAssembly [CLRStoredProcs]    Script Date: 10/11/2010 13:09:51 ******/
////CREATE ASSEMBLY [CLRStoredProcs]
////AUTHORIZATION [dbo]
////FROM 0x4D5A90000300000004000000FFFF0000B800000000000000400000000000000000000000000000000000000000000000000000000000000000000000800000000E1FBA0E00B409CD21B8014CCD21546869732070726F6772616D2063616E6E6F742062652072756E20696E20444F53206D6F64652E0D0D0A2400000000000000504500004C0103005CFDB24C0000000000000000E00002210B010800001200000006000000000000CE3000000020000000400000000040000020000000020000040000000000000004000000000000000080000000020000000000000300408500001000001000000000100000100000000000001000000000000000000000007C3000004F000000004000006803000000000000000000000000000000000000006000000C00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000200000080000000000000000000000082000004800000000000000000000002E74657874000000D4100000002000000012000000020000000000000000000000000000200000602E7273726300000068030000004000000004000000140000000000000000000000000000400000402E72656C6F6300000C0000000060000000020000001800000000000000000000000000004000004200000000000000000000000000000000B030000000000000480000000200050078220000040E00000100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000133003001D00000001000011178D120000010B07161F7C9D070A0F00280F00000A06176F1000000A2A4A03027413000001510303506F1100000A512A000000007C0020002C000000133003002100000002000011198D1200000125D001000004281300000A0A0F00280F00000A06176F1000000A2A3A03027413000001281400000A542A000000007C0020002C000000133003002100000002000011198D1200000125D002000004281300000A0A0F00280F00000A06176F1000000A2A3A03027413000001281500000A552A133003001D00000001000011178D120000010B07161F7C9D070A0F00280F00000A06176F1000000A2A000000133003005B00000003000011178D120000010C08161F2C9D080A027413000001066F1600000A0B078E6919331F0307169A281700000A550407179A281800000A520507189A281800000A522A7201000070026F1900000A725F000070281A00000A731B00000A7A00133003001D00000001000011178D120000010B07161F7C9D070A0F00280F00000A06176F1000000A2A000000133003008700000003000011178D120000010C08161F2C9D080A027413000001066F1600000A0B078E691D334B0307169A281700000A550407179A281C00000A530507189A281C00000A530E0407199A281C00000A530E05071A9A281C00000A530E06071B9A281C00000A530E07071C9A281C00000A532A7201000070026F1900000A72FC000070281A00000A731B00000A7A1E02281D00000A2A0042534A4201000100000000000C00000076322E302E35303732370000000005006C000000E4030000237E0000500400001005000023537472696E677300000000600900006C01000023555300CC0A0000100000002347554944000000DC0A00002803000023426C6F620000000000000002000001579502200902000000FA013300160000010000001E00000004000000020000000B000000170000001D0000001000000001000000030000000200000001000000020000000100000000000A0001000000000006005B0054000600750062000A00A2008D00060099017A0106000C02FA0106002302FA0106004002FA0106005F02FA0106007802FA0106009102FA010600AC02FA010600C702FA010600E0027A010600F402FA0106002D030D0306004D030D030A0095037A030600AA0354000600B90354000600C0035400060023040D0306003E045400060078040D0306008704540006008D0454000600B00454000600DC0454000600E804540006000005540006000A0554000000000001000000000001000100010010001D00270005000100010000000000DE030000050001000C001301000048040000590003000C001301640403011301C00403015020000000009600AC000A0001007920000000009600BF00110002009820000000009600CF000A000400C520000000009600E10018000500E020000000009600F0000A0007000D2100000000960005011F0008001C2100000000960017010A000A0048210000000096002C0126000B00B02100000000960040010A000F00DC210000000096005601310010006F220000000086186C014400180000000100720100000100760102000200720100000100720100000100760102000200A60100000100720100000100760102000200A60100000100720100000100760102000200A80102000300AB0102000400B40100000100720100000100760102000200A80102000300BE0102000400C80102000500D20102000600DC0102000700E60102000800F00121006C01440029006C01480031006C01480039006C01480041006C01480049006C01480051006C01480059006C01480061006C01480069006C014D0071006C01480079006C01520081006C01440089006C0144001900AF03A5009900D303A9009900D903A500A9006C014400B900A0040701D100B8041401D100D40464019900D303E201D900E2046401E100E204E9010900F004A5009900F904EE01E9006C014800F100E204C80209006C0144002000730057002E001B00E1022E002300E1022E002B00E1022E006B0008032E001300CD022E003B00E7022E006300FF022E003300CD022E004300E1022E005300E10260007300B90063009300FE00A00073001901E0007300690120017300FE010100060000000400B2000F01F501902000000100D820000002000480000001000000600FEE540000000000006B03000002000000000000000000000001004B000000000002000000000000000000000001008100000000000400030000000000003C4D6F64756C653E00434C5253746F72656450726F63732E646C6C004C69737453706C6974004576654D61726B65744D6F6E69746F724170702E4461746162617365436C6173736573006D73636F726C69620053797374656D004F626A6563740053797374656D2E436F6C6C656374696F6E730049456E756D657261626C650053797374656D2E446174610053797374656D2E446174612E53716C54797065730053716C537472696E6700434C525F636861726C6973745F73706C697400436861726C69737446696C6C526F7700434C525F696E746C6973745F73706C697400496E746C69737446696C6C526F7700434C525F626967696E746C6973745F73706C697400426967496E746C69737446696C6C526F7700434C525F6163636573736C6973745F73706C6974004275696C644F776E6572416363657373526F7700434C525F66696E616E63656C6973745F73706C6974004275696C6446696E616E6365416363657373526F77002E63746F720073747200726F770053797374656D2E52756E74696D652E496E7465726F705365727669636573004F7574417474726962757465006E00696400706572736F6E616C00636F72706F726174650077616C6C65744944310077616C6C65744944320077616C6C65744944330077616C6C65744944340077616C6C65744944350077616C6C65744944360053797374656D2E5265666C656374696F6E00417373656D626C795469746C6541747472696275746500417373656D626C794465736372697074696F6E41747472696275746500417373656D626C79436F6E66696775726174696F6E41747472696275746500417373656D626C79436F6D70616E7941747472696275746500417373656D626C7950726F6475637441747472696275746500417373656D626C79436F7079726967687441747472696275746500417373656D626C7954726164656D61726B41747472696275746500417373656D626C7943756C7475726541747472696275746500436F6D56697369626C6541747472696275746500417373656D626C7956657273696F6E4174747269627574650053797374656D2E52756E74696D652E436F6D70696C6572536572766963657300436F6D70696C6174696F6E52656C61786174696F6E734174747269627574650052756E74696D65436F6D7061746962696C69747941747472696275746500434C5253746F72656450726F6373004D6963726F736F66742E53716C5365727665722E5365727665720053716C46756E6374696F6E4174747269627574650043686172006765745F56616C756500537472696E6700537472696E6753706C69744F7074696F6E730053706C6974005472696D003C50726976617465496D706C656D656E746174696F6E44657461696C733E7B31313333323039412D343944362D343139432D424631352D4536354243334231394644357D00436F6D70696C657247656E6572617465644174747269627574650056616C756554797065005F5F5374617469634172726179496E69745479706553697A653D360024246D6574686F643078363030303030332D310052756E74696D6548656C706572730041727261790052756E74696D654669656C6448616E646C6500496E697469616C697A65417272617900436F6E7665727400546F496E7433320024246D6574686F643078363030303030352D3100546F496E74363400496E74363400506172736500426F6F6C65616E00546F537472696E6700436F6E63617400457863657074696F6E00496E74313600005D420061006400200070006100720061006D0065007400650072002000700061007300730065006400200074006F0020004200750069006C0064004F0077006E006500720041006300630065007300730052006F0077003A002000270001809B27002E000D000A0045007800700065006300740069006E006700200027003C0069006E00740020002D0020006F0077006E0065007200490044003E002C003C0062006F006F006C0020002D00200069006E0063006C0075006400650050006500720073006F006E0061006C003E002C003C0062006F006F006C0020002D00200069006E0063006C0075006400650043006F00720070003E002700016D27002E000D000A0045007800700065006300740069006E006700200027003C0069006E00740020002D0020006F0077006E0065007200490044003E002C003C00730068006F007200740020002D002000770061006C006C0065007400490044003E00200078002000360027000100009A203311D6499C41BF15E65BC3B19FD50008B77A5C561934E0890600011209110D060002011C100E060002011C1008060002011C100A0A0004011C100A10021002120008011C100A10061006100610061006100603200001042001010E042001010204200101084D01000200540E1146696C6C526F774D6574686F644E616D650F436861726C69737446696C6C526F77540E0F5461626C65446566696E6974696F6E12737472206E766172636861722834303030290320000E0820021D0E1D0311510607021D031D034401000200540E1146696C6C526F774D6574686F644E616D650E496E746C69737446696C6C526F77540E0F5461626C65446566696E6974696F6E0A6E756D62657220696E7404010000000306111007000201126111650407011D03040001080E4A01000200540E1146696C6C526F774D6574686F644E616D6511426967496E746C69737446696C6C526F77540E0F5461626C65446566696E6974696F6E0D6E756D62657220626967696E740400010A0E7801000200540E1146696C6C526F774D6574686F644E616D65134275696C644F776E6572416363657373526F77540E0F5461626C65446566696E6974696F6E396F776E6572494420626967696E742C20696E636C756465506572736F6E616C206269742C20696E636C756465436F72706F72617465206269740620011D0E1D03040001020E0600030E0E0E0E0807031D031D0E1D0380C801000200540E1146696C6C526F774D6574686F644E616D65154275696C6446696E616E6365416363657373526F77540E0F5461626C65446566696E6974696F6E80866F776E6572494420626967696E742C2077616C6C657449443120736D616C6C696E742C2077616C6C657449443220736D616C6C696E742C2077616C6C657449443320736D616C6C696E742C2077616C6C657449443420736D616C6C696E742C2077616C6C657449443520736D616C6C696E742C2077616C6C657449443620736D616C6C696E74040001060E1301000E434C5253746F72656450726F6373000005010000000017010012436F7079726967687420C2A920203230303800000801000800000000001E01000100540216577261704E6F6E457863657074696F6E5468726F77730100A43000000000000000000000BE300000002000000000000000000000000000000000000000000000B0300000000000000000000000005F436F72446C6C4D61696E006D73636F7265652E646C6C0000000000FF250020400000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100100000001800008000000000000000000000000000000100010000003000008000000000000000000000000000000100000000004800000058400000100300000000000000000000100334000000560053005F00560045005200530049004F004E005F0049004E0046004F0000000000BD04EFFE0000010000000100EE54600F00000100EE54600F3F000000000000000400000002000000000000000000000000000000440000000100560061007200460069006C00650049006E0066006F00000000002400040000005400720061006E0073006C006100740069006F006E00000000000000B00470020000010053007400720069006E006700460069006C00650049006E0066006F0000004C020000010030003000300030003000340062003000000048000F000100460069006C0065004400650073006300720069007000740069006F006E000000000043004C005200530074006F00720065006400500072006F00630073000000000040000F000100460069006C006500560065007200730069006F006E000000000031002E0030002E0033003900330036002E00320031003700340032000000000048001300010049006E007400650072006E0061006C004E0061006D006500000043004C005200530074006F00720065006400500072006F00630073002E0064006C006C00000000004800120001004C006500670061006C0043006F007000790072006900670068007400000043006F0070007900720069006700680074002000A90020002000320030003000380000005000130001004F0072006900670069006E0061006C00460069006C0065006E0061006D006500000043004C005200530074006F00720065006400500072006F00630073002E0064006C006C000000000040000F000100500072006F0064007500630074004E0061006D0065000000000043004C005200530074006F00720065006400500072006F00630073000000000044000F000100500072006F006400750063007400560065007200730069006F006E00000031002E0030002E0033003900330036002E00320031003700340032000000000048000F00010041007300730065006D0062006C0079002000560065007200730069006F006E00000031002E0030002E0033003900330036002E0032003100370034003200000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000003000000C000000D03000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
////WITH PERMISSION_SET = SAFE
////
////ALTER ASSEMBLY [CLRStoredProcs]
////ADD FILE FROM 0x7573696E672053797374656D3B0D0A7573696E672053797374656D2E436F6C6C656374696F6E733B0D0A7573696E672053797374656D2E446174612E53716C54797065733B0D0A7573696E67204D6963726F736F66742E53716C5365727665722E5365727665723B0D0A0D0A6E616D657370616365204576654D61726B65744D6F6E69746F724170702E4461746162617365436C61737365730D0A7B0D0A202020207075626C696320636C617373204C69737453706C69740D0A202020207B0D0A20202020202020205B53716C46756E6374696F6E2846696C6C526F774D6574686F644E616D65203D2022436861726C69737446696C6C526F77222C205461626C65446566696E6974696F6E203D2022737472206E7661726368617228343030302922295D0D0A20202020202020207075626C6963207374617469632049456E756D657261626C6520434C525F636861726C6973745F73706C69742853716C537472696E6720737472290D0A20202020202020207B0D0A202020202020202020202020636861725B5D2064656C696D203D207B20277C27207D3B0D0A20202020202020202020202072657475726E207374722E56616C75652E53706C69742864656C696D2C20537472696E6753706C69744F7074696F6E732E52656D6F7665456D707479456E7472696573293B0D0A20202020202020207D0D0A0D0A20202020202020207075626C69632073746174696320766F696420436861726C69737446696C6C526F77286F626A65637420726F772C206F757420737472696E6720737472290D0A20202020202020207B0D0A202020202020202020202020737472203D2028737472696E6729726F773B0D0A202020202020202020202020737472203D207374722E5472696D28293B0D0A20202020202020207D0D0A0D0A0D0A0D0A0D0A20202020202020205B53716C46756E6374696F6E2846696C6C526F774D6574686F644E616D65203D2022496E746C69737446696C6C526F77222C205461626C65446566696E6974696F6E203D20226E756D62657220696E7422295D0D0A20202020202020207075626C6963207374617469632049456E756D657261626C6520434C525F696E746C6973745F73706C69742853716C537472696E6720737472290D0A20202020202020207B0D0A202020202020202020202020636861725B5D2064656C696D203D207B20277C272C202720272C20272C27207D3B0D0A20202020202020202020202072657475726E207374722E56616C75652E53706C69742864656C696D2C20537472696E6753706C69744F7074696F6E732E52656D6F7665456D707479456E7472696573293B0D0A20202020202020207D0D0A0D0A20202020202020207075626C69632073746174696320766F696420496E746C69737446696C6C526F77286F626A65637420726F772C206F757420696E74206E290D0A20202020202020207B0D0A2020202020202020202020206E203D20436F6E766572742E546F496E7433322828737472696E6729726F77293B0D0A20202020202020207D0D0A0D0A0D0A0D0A0D0A20202020202020205B53716C46756E6374696F6E2846696C6C526F774D6574686F644E616D65203D2022426967496E746C69737446696C6C526F77222C205461626C65446566696E6974696F6E203D20226E756D62657220626967696E7422295D0D0A20202020202020207075626C6963207374617469632049456E756D657261626C6520434C525F626967696E746C6973745F73706C69742853716C537472696E6720737472290D0A20202020202020207B0D0A202020202020202020202020636861725B5D2064656C696D203D207B20277C272C202720272C20272C27207D3B0D0A20202020202020202020202072657475726E207374722E56616C75652E53706C69742864656C696D2C20537472696E6753706C69744F7074696F6E732E52656D6F7665456D707479456E7472696573293B0D0A20202020202020207D0D0A0D0A20202020202020207075626C69632073746174696320766F696420426967496E746C69737446696C6C526F77286F626A65637420726F772C206F7574206C6F6E67206E290D0A20202020202020207B0D0A2020202020202020202020206E203D20436F6E766572742E546F496E7436342828737472696E6729726F77293B0D0A20202020202020207D0D0A0D0A0D0A0D0A0D0A20202020202020205B53716C46756E6374696F6E2846696C6C526F774D6574686F644E616D65203D20224275696C644F776E6572416363657373526F77222C205461626C65446566696E6974696F6E203D20226F776E6572494420626967696E742C20696E636C756465506572736F6E616C206269742C20696E636C756465436F72706F726174652062697422295D0D0A20202020202020207075626C6963207374617469632049456E756D657261626C6520434C525F6163636573736C6973745F73706C69742853716C537472696E6720737472290D0A20202020202020207B0D0A202020202020202020202020636861725B5D2064656C696D203D207B20277C27207D3B0D0A20202020202020202020202072657475726E207374722E56616C75652E53706C69742864656C696D2C20537472696E6753706C69744F7074696F6E732E52656D6F7665456D707479456E7472696573293B0D0A20202020202020207D0D0A0D0A20202020202020207075626C69632073746174696320766F6964204275696C644F776E6572416363657373526F77286F626A65637420726F772C206F7574206C6F6E672069642C206F757420626F6F6C20706572736F6E616C2C206F757420626F6F6C20636F72706F72617465290D0A20202020202020207B0D0A202020202020202020202020636861725B5D2064656C696D203D207B20272C27207D3B0D0A202020202020202020202020737472696E675B5D2076616C756573203D202828737472696E6729726F77292E53706C69742864656C696D293B0D0A2020202020202020202020206966202876616C7565732E4C656E677468203D3D2033290D0A2020202020202020202020207B0D0A202020202020202020202020202020206964203D206C6F6E672E50617273652876616C7565735B305D293B0D0A20202020202020202020202020202020706572736F6E616C203D20626F6F6C2E50617273652876616C7565735B315D293B0D0A20202020202020202020202020202020636F72706F72617465203D20626F6F6C2E50617273652876616C7565735B325D293B0D0A2020202020202020202020207D0D0A202020202020202020202020656C73650D0A2020202020202020202020207B0D0A202020202020202020202020202020207468726F77206E657720457863657074696F6E282242616420706172616D657465722070617373656420746F204275696C644F776E6572416363657373526F773A202722202B20726F772E546F537472696E672829202B2022272E5C725C6E457870656374696E6720273C696E74202D206F776E657249443E2C3C626F6F6C202D20696E636C756465506572736F6E616C3E2C3C626F6F6C202D20696E636C756465436F72703E2722293B0D0A2020202020202020202020207D0D0A20202020202020207D0D0A0D0A0D0A0D0A0D0A20202020202020205B53716C46756E6374696F6E2846696C6C526F774D6574686F644E616D65203D20224275696C6446696E616E6365416363657373526F77222C205461626C65446566696E6974696F6E203D20226F776E6572494420626967696E742C2077616C6C657449443120736D616C6C696E742C2077616C6C657449443220736D616C6C696E742C2077616C6C657449443320736D616C6C696E742C2077616C6C657449443420736D616C6C696E742C2077616C6C657449443520736D616C6C696E742C2077616C6C657449443620736D616C6C696E7422295D0D0A20202020202020207075626C6963207374617469632049456E756D657261626C6520434C525F66696E616E63656C6973745F73706C69742853716C537472696E6720737472290D0A20202020202020207B0D0A202020202020202020202020636861725B5D2064656C696D203D207B20277C27207D3B0D0A20202020202020202020202072657475726E207374722E56616C75652E53706C69742864656C696D2C20537472696E6753706C69744F7074696F6E732E52656D6F7665456D707479456E7472696573293B0D0A20202020202020207D0D0A0D0A20202020202020207075626C69632073746174696320766F6964204275696C6446696E616E6365416363657373526F77286F626A65637420726F772C206F7574206C6F6E672069642C206F75742073686F72742077616C6C65744944312C206F75742073686F72742077616C6C65744944322C206F75742073686F72742077616C6C65744944332C206F75742073686F72742077616C6C65744944342C206F75742073686F72742077616C6C65744944352C206F75742073686F72742077616C6C6574494436290D0A20202020202020207B0D0A202020202020202020202020636861725B5D2064656C696D203D207B20272C27207D3B0D0A202020202020202020202020737472696E675B5D2076616C756573203D202828737472696E6729726F77292E53706C69742864656C696D293B0D0A2020202020202020202020206966202876616C7565732E4C656E677468203D3D2037290D0A2020202020202020202020207B0D0A202020202020202020202020202020206964203D206C6F6E672E50617273652876616C7565735B305D293B0D0A2020202020202020202020202020202077616C6C6574494431203D2073686F72742E50617273652876616C7565735B315D293B0D0A2020202020202020202020202020202077616C6C6574494432203D2073686F72742E50617273652876616C7565735B325D293B0D0A2020202020202020202020202020202077616C6C6574494433203D2073686F72742E50617273652876616C7565735B335D293B0D0A2020202020202020202020202020202077616C6C6574494434203D2073686F72742E50617273652876616C7565735B345D293B0D0A2020202020202020202020202020202077616C6C6574494435203D2073686F72742E50617273652876616C7565735B355D293B0D0A2020202020202020202020202020202077616C6C6574494436203D2073686F72742E50617273652876616C7565735B365D293B0D0A2020202020202020202020207D0D0A202020202020202020202020656C73650D0A2020202020202020202020207B0D0A202020202020202020202020202020207468726F77206E657720457863657074696F6E282242616420706172616D657465722070617373656420746F204275696C644F776E6572416363657373526F773A202722202B20726F772E546F537472696E672829202B2022272E5C725C6E457870656374696E6720273C696E74202D206F776E657249443E2C3C73686F7274202D2077616C6C657449443E207820362722293B0D0A2020202020202020202020207D0D0A20202020202020207D0D0A202020207D0D0A7D0D0A
////AS N'CLRStoredProcs.cs'
////
////ALTER ASSEMBLY [CLRStoredProcs]
////ADD FILE FROM 0xEFBBBF7573696E672053797374656D2E5265666C656374696F6E3B0D0A7573696E672053797374656D2E52756E74696D652E436F6D70696C657253657276696365733B0D0A7573696E672053797374656D2E52756E74696D652E496E7465726F7053657276696365733B0D0A7573696E672053797374656D2E446174612E53716C3B0D0A0D0A2F2F2047656E6572616C20496E666F726D6174696F6E2061626F757420616E20617373656D626C7920697320636F6E74726F6C6C6564207468726F7567682074686520666F6C6C6F77696E670D0A2F2F20736574206F6620617474726962757465732E204368616E6765207468657365206174747269627574652076616C75657320746F206D6F646966792074686520696E666F726D6174696F6E0D0A2F2F206173736F636961746564207769746820616E20617373656D626C792E0D0A5B617373656D626C793A20417373656D626C795469746C652822434C5253746F72656450726F637322295D0D0A5B617373656D626C793A20417373656D626C794465736372697074696F6E282222295D0D0A5B617373656D626C793A20417373656D626C79436F6E66696775726174696F6E282222295D0D0A5B617373656D626C793A20417373656D626C79436F6D70616E79282222295D0D0A5B617373656D626C793A20417373656D626C7950726F647563742822434C5253746F72656450726F637322295D0D0A5B617373656D626C793A20417373656D626C79436F707972696768742822436F7079726967687420C2A920203230303822295D0D0A5B617373656D626C793A20417373656D626C7954726164656D61726B282222295D0D0A5B617373656D626C793A20417373656D626C7943756C74757265282222295D0D0A0D0A5B617373656D626C793A20436F6D56697369626C652866616C7365295D0D0A0D0A2F2F0D0A2F2F2056657273696F6E20696E666F726D6174696F6E20666F7220616E20617373656D626C7920636F6E7369737473206F662074686520666F6C6C6F77696E6720666F75722076616C7565733A0D0A2F2F0D0A2F2F2020202020204D616A6F722056657273696F6E0D0A2F2F2020202020204D696E6F722056657273696F6E0D0A2F2F2020202020204275696C64204E756D6265720D0A2F2F2020202020205265766973696F6E0D0A2F2F0D0A2F2F20596F752063616E207370656369667920616C6C207468652076616C756573206F7220796F752063616E2064656661756C7420746865205265766973696F6E20616E64204275696C64204E756D626572730D0A2F2F206279207573696E672074686520272A272061732073686F776E2062656C6F773A0D0A5B617373656D626C793A20417373656D626C7956657273696F6E2822312E302E2A22295D0D0A0D0A
////AS N'Properties\AssemblyInfo.cs'
////GO
////EXEC sys.sp_addextendedproperty @name=N'AutoDeployed', @value=N'yes' , @level0type=N'ASSEMBLY',@level0name=N'CLRStoredProcs'
////GO
////EXEC sys.sp_addextendedproperty @name=N'SqlAssemblyProjectRoot', @value=N'C:\Documents and Settings\stephenb\My Documents\Visual Studio 2008\Projects\EveMarketMonitorApp\CLRStoredProcs' , @level0type=N'ASSEMBLY',@level0name=N'CLRStoredProcs'";
                            commandText = @"CREATE ASSEMBLY CLRStoredProcs from '" + Globals.AppDataDir + "CLRStoredProcs_1_5_3_0.dll' WITH PERMISSION_SET = SAFE";

                            try { server.ConnectionContext.ExecuteNonQuery(commandText); }
                            catch (Exception ex)
                            {
                                throw new EMMADataException(ExceptionSeverity.Critical,
                                    "Problem deploying new CLRStoredProcs assembly", ex);
                            }

                            commandText = @"/****** Object:  UserDefinedFunction [dbo].[CLR_intlist_split]    Script Date: 10/11/2010 13:09:51 ******/
CREATE FUNCTION [dbo].[CLR_intlist_split](@str [nvarchar](4000))
RETURNS  TABLE (
	[number] [int] NULL
) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [CLRStoredProcs].[EveMarketMonitorApp.DatabaseClasses.ListSplit].[CLR_intlist_split]
GO
EXEC sys.sp_addextendedproperty @name=N'AutoDeployed', @value=N'yes' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_intlist_split'
GO
EXEC sys.sp_addextendedproperty @name=N'SqlAssemblyFile', @value=N'CLRStoredProcs.cs' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_intlist_split'
GO
EXEC sys.sp_addextendedproperty @name=N'SqlAssemblyFileLine', @value=26 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_intlist_split'";

                            try { server.ConnectionContext.ExecuteNonQuery(commandText); }
                            catch (Exception ex)
                            {
                                throw new EMMADataException(ExceptionSeverity.Critical,
                                    "Problem re-creating CLR_intlist_split function", ex);
                            }

                            commandText = @"/****** Object:  UserDefinedFunction [dbo].[CLR_financelist_split]    Script Date: 10/11/2010 13:09:51 ******/
CREATE FUNCTION [dbo].[CLR_financelist_split](@str [nvarchar](4000))
RETURNS  TABLE (
	[ownerID] [bigint] NULL,
	[walletID1] [smallint] NULL,
	[walletID2] [smallint] NULL,
	[walletID3] [smallint] NULL,
	[walletID4] [smallint] NULL,
	[walletID5] [smallint] NULL,
	[walletID6] [smallint] NULL
) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [CLRStoredProcs].[EveMarketMonitorApp.DatabaseClasses.ListSplit].[CLR_financelist_split]
GO
EXEC sys.sp_addextendedproperty @name=N'AutoDeployed', @value=N'yes' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_financelist_split'
GO
EXEC sys.sp_addextendedproperty @name=N'SqlAssemblyFile', @value=N'CLRStoredProcs.cs' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_financelist_split'
GO
EXEC sys.sp_addextendedproperty @name=N'SqlAssemblyFileLine', @value=82 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_financelist_split'";

                            try { server.ConnectionContext.ExecuteNonQuery(commandText); }
                            catch (Exception ex)
                            {
                                throw new EMMADataException(ExceptionSeverity.Critical,
                                    "Problem re-creating CLR_financelist_split function", ex);
                            }

                            commandText = @"/****** Object:  UserDefinedFunction [dbo].[CLR_charlist_split]    Script Date: 10/11/2010 13:09:51 ******/
CREATE FUNCTION [dbo].[CLR_charlist_split](@str [nvarchar](4000))
RETURNS  TABLE (
	[str] [nvarchar](4000) NULL
) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [CLRStoredProcs].[EveMarketMonitorApp.DatabaseClasses.ListSplit].[CLR_charlist_split]
GO
EXEC sys.sp_addextendedproperty @name=N'AutoDeployed', @value=N'yes' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_charlist_split'
GO
EXEC sys.sp_addextendedproperty @name=N'SqlAssemblyFile', @value=N'CLRStoredProcs.cs' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_charlist_split'
GO
EXEC sys.sp_addextendedproperty @name=N'SqlAssemblyFileLine', @value=10 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_charlist_split'";

                            try { server.ConnectionContext.ExecuteNonQuery(commandText); }
                            catch (Exception ex)
                            {
                                throw new EMMADataException(ExceptionSeverity.Critical,
                                    "Problem re-creating CLR_charlist_split function", ex);
                            }

                            commandText = @"/****** Object:  UserDefinedFunction [dbo].[CLR_bigintlist_split]    Script Date: 10/11/2010 13:09:51 ******/
CREATE FUNCTION [dbo].[CLR_bigintlist_split](@str [nvarchar](4000))
RETURNS  TABLE (
	[number] [bigint] NULL
) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [CLRStoredProcs].[EveMarketMonitorApp.DatabaseClasses.ListSplit].[CLR_bigintlist_split]
GO
EXEC sys.sp_addextendedproperty @name=N'AutoDeployed', @value=N'yes' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_bigintlist_split'
GO
EXEC sys.sp_addextendedproperty @name=N'SqlAssemblyFile', @value=N'CLRStoredProcs.cs' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_bigintlist_split'
GO
EXEC sys.sp_addextendedproperty @name=N'SqlAssemblyFileLine', @value=41 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_bigintlist_split'";

                            try { server.ConnectionContext.ExecuteNonQuery(commandText); }
                            catch (Exception ex)
                            {
                                throw new EMMADataException(ExceptionSeverity.Critical,
                                    "Problem creating CLR_bigintlist_split function", ex);
                            }

                            commandText = @"/****** Object:  UserDefinedFunction [dbo].[CLR_accesslist_split]    Script Date: 10/11/2010 13:09:51 ******/
CREATE FUNCTION [dbo].[CLR_accesslist_split](@str [nvarchar](4000))
RETURNS  TABLE (
	[ownerID] [bigint] NULL,
	[includePersonal] [bit] NULL,
	[includeCorporate] [bit] NULL
) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [CLRStoredProcs].[EveMarketMonitorApp.DatabaseClasses.ListSplit].[CLR_accesslist_split]
GO
EXEC sys.sp_addextendedproperty @name=N'AutoDeployed', @value=N'yes' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_accesslist_split'
GO
EXEC sys.sp_addextendedproperty @name=N'SqlAssemblyFile', @value=N'CLRStoredProcs.cs' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_accesslist_split'
GO
EXEC sys.sp_addextendedproperty @name=N'SqlAssemblyFileLine', @value=56 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'FUNCTION',@level1name=N'CLR_accesslist_split'";

                            try { server.ConnectionContext.ExecuteNonQuery(commandText); }
                            catch (Exception ex)
                            {
                                throw new EMMADataException(ExceptionSeverity.Critical,
                                    "Problem re-creating CLR_accesslist_split function", ex);
                            }

                            File.Delete(Globals.AppDataDir + "CLRStoredProcs_1_5_3_0.dll");

                            SetDBVersion(connection, new Version("1.5.2.999"));
                        }
                        catch (Exception ex)
                        {
                            if (ex is EMMAException) { throw; }
                            else
                            {
                                throw new EMMADataException(ExceptionSeverity.Critical,
                                   "Problem updating database CLR assembly", ex);
                            }
                        }
                        #endregion
                    }
                    if (dbVersion.CompareTo(new Version("1.5.3.0")) < 0)
                    {
                        #region ---- MAJOR UPDATE ----
                        // This update is a little unusual in that it actually runs tons of scripts.
                        // The purpose is to update various fields, parameters, etc to 64 bit
                        // integers.
                        // This also involves updating the SQL CLR assembly with a new version that
                        // can cope with 64 bit integers.
                        // (Note the CLR assembly is now deployed above for version 1.5.2.999)

                        // All the scripts are delivered to the user's machine via an XML file
                        // that is downloaded through the normal update process.
                        // The new SQL CLR assembly is also delviered in this way.
                        // Consequently, at this point we should have the XML containing all
                        // the SQL scripts and the SQL CLR assembly ready to go. First thing
                        // to do is to check they are where they should be...
                        if (!File.Exists(Globals.AppDataDir + "DatabaseUpdate_1_5_3_0.xml"))
                        {
                            throw new EMMAException(ExceptionSeverity.Critical, "Need to update database to version 1.5.3.0. However, the expected SQL script data file (" +
                                Globals.AppDataDir + "DatabaseUpdate_1_5_3_0.xml) is missing. Unable to continue. If you have just updated EMMA, please update again and then retry.");
                        }

                        // Extract the scripts contained in the update xml file then run
                        // each one in turn.
                        // Delete each script file as we go so that if one fails, we can
                        // just pick up from where we left off.
                        Updater.ProcessUpdateXML(Globals.AppDataDir + "DatabaseUpdate_1_5_3_0.xml");
                        try
                        {
                            string scriptDir = Globals.AppDataDir + "DatabaseUpdate_1_5_3_0";
                            string[] scripts = Directory.GetFiles(scriptDir);

                            foreach (string script in scripts)
                            {
                                commandText = File.ReadAllText(script);
                                server.ConnectionContext.ExecuteNonQuery(commandText); 

                                File.Delete(script);
                            }

                            File.Delete(Globals.AppDataDir + "DatabaseUpdate_1_5_3_0.xml");
                            Directory.Delete(Globals.AppDataDir + "DatabaseUpdate_1_5_3_0");
                            SetDBVersion(connection, new Version("1.5.3.0"));
                        }
                        catch (Exception ex)
                        {
                            throw new EMMADataException(ExceptionSeverity.Critical,
                                "Problem running database update script", ex);
                        }

                        #endregion
                    }

         
                }

            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    new EMMAException(ExceptionSeverity.Error, "Problem updating database", ex);
                }
                else
                {
                    if (emmaEx.Severity == ExceptionSeverity.Critical)
                    {
                        throw;
                    }
                }
            }
            finally
            {
                connection.Close();
            }
            #endregion




            #region Update EveData Database
            SqlConnection connection2 = new SqlConnection(Properties.Settings.Default.ebs_DATADUMPConnectionString + 
                ";Pooling=false");
            connection2.Open();

            try
            {
                SqlCommand command = null;
                SqlDataAdapter adapter = null;
                string commandText = "";

                command = new SqlCommand("_GetVersion", connection2);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                string value = "";
                SqlParameter param = new SqlParameter("@version", SqlDbType.VarChar, 50,
                    ParameterDirection.InputOutput, 0, 0, null, DataRowVersion.Current,
                    false, value, "", "", "");
                command.Parameters.Add(param);
                command.ExecuteNonQuery();
                Version dbVersion = new Version(param.Value.ToString());


                if (dbVersion.CompareTo(new Version("1.0.0.1")) < 0)
                {
                    #region Update SolarSystemDistancesGen stored procedure
                    commandText = @"ALTER PROCEDURE [dbo].[SolarSystemDistancesGen] 
	@startSystemID		int,
	@range				int
AS
	-- Thanks go to 'Jethro Jechonias' for the original version of this code.

	/* Populate the distances table with all from/to pairs that match the supplied parameters */
	
	WHILE @@ROWCOUNT > 0
	BEGIN
	INSERT INTO   SolarSystemDistances
	SELECT DISTINCT  A.FromSolarSystemID,  B.ToSolarSystemID,  A.Distance+1
	FROM  SolarSystemDistances AS A
	JOIN  SolarSystemJumps AS B
	ON  A.ToSolarSystemID = B.FromSolarSystemID AND B.ToSolarSystemID != A.FromSolarSystemID
	WHERE (A.Distance < @range) AND (A.FromSolarSystemID = @startSystemID) AND (B.ToSolarSystemID NOT IN 
	( 
		SELECT ToSolarSystemID 
		FROM SolarSystemDistances 
		WHERE FromSolarSystemID = A.FromSolarSystemID 
	))
	END
	
	SELECT SolarSystemDistances.*
	FROM SolarSystemDistances
	WHERE FromSolarSystemID = @startSystemID AND Distance <= @range
	
	RETURN";
                    adapter = new SqlDataAdapter(commandText, connection2);

                    try
                    {
                        adapter.SelectCommand.ExecuteNonQuery();
                        SetDBVersion(connection2, new Version("1.0.0.1"));
                    }
                    catch (Exception ex)
                    {
                        throw new EMMADataException(ExceptionSeverity.Critical,
                            "Unable to update stored procedure 'SolarSystemDistancesGen'.", ex);
                    }
                    #endregion
                }
                if (dbVersion.CompareTo(new Version("1.0.0.2")) < 0)
                {
                    #region Update GetReprocesResults stored procedure
                    commandText = @"ALTER PROCEDURE [dbo].[GetReprocesResults]
	@itemID		int
AS
	DECLARE @blueprintID	int
	DECLARE @baseItemID		int
	DECLARE @techLevel		int
	DECLARE	@metaGroupID	int
	
	SET @baseItemID = 0
	SELECT @baseItemID = parentTypeID, @metaGroupID = metaGroupID
	FROM invMetaTypes
	WHERE typeID = @itemID
	
	IF(@baseItemID = 0)
	BEGIN
		SET @baseItemID = @itemID
	END
	
	SET @techLevel = 0
	SET @blueprintID = 0
	SELECT @blueprintID = blueprintTypeID, @techLevel = techLevel
	FROM invBlueprintTypes
	WHERE productTypeID = @baseItemID
	
	IF(@techLevel = 1 OR @blueprintID = 0)
	BEGIN
		-- If this is a T1 item or we couldn't find a blueprint then try just getting the
		-- reprocess values directly..
		SELECT typeActivityMaterials.requiredTypeID, typeActivityMaterials.quantity
		FROM typeActivityMaterials
		INNER JOIN invTypes AS typeReq ON typeActivityMaterials.requiredTypeID = typeReq.typeID
		INNER JOIN invGroups AS typeGroup ON typeReq.groupID = typeGroup.groupID
		WHERE typeActivityMaterials.typeID = @itemID AND typeActivityMaterials.activityID = 6 AND
				typeGroup.categoryID != 16 AND typeGroup.categoryID != 17
		
		IF(@@ROWCOUNT = 0 AND @blueprintID != 0)
		BEGIN
			-- If the item does not have reprocess values (some, like 425mm prototype gun don't)
			-- and we do have a blueprint then use that to work out the values instead.
			SELECT typeActivityMaterials.requiredTypeID, typeActivityMaterials.quantity
			FROM typeActivityMaterials
			INNER JOIN invTypes AS typeReq ON typeActivityMaterials.requiredTypeID = typeReq.typeID
			INNER JOIN invGroups AS typeGroup ON typeReq.groupID = typeGroup.groupID
			WHERE typeActivityMaterials.typeID = @blueprintID AND typeActivityMaterials.activityID = 1 AND
				typeGroup.categoryID != 16 AND typeGroup.categoryID != 17
		END
	END
	ELSE IF(@techLevel = 2)
	BEGIN
		-- Tech 2
		-- This requires a little more processing than T1 because the original T1 item
		-- used in construction is not returned.
		-- Instead, the component materials of that T1 item are returned, however the quantities
		-- are modified by the amounts in the T2 blueprint's data.
		SELECT typeActivityMaterials.requiredTypeID, SUM(typeActivityMaterials.quantity) AS quantity
		FROM typeActivityMaterials
		JOIN
		(
			SELECT invBlueprintTypes.blueprintTypeID
			FROM invBlueprintTypes
			JOIN
			(
				SELECT typeActivityMaterials.requiredTypeID
				FROM typeActivityMaterials
				INNER JOIN invTypes AS typeReq ON typeActivityMaterials.requiredTypeID = typeReq.typeID
				INNER JOIN invGroups AS typeGroup ON typeReq.groupID = typeGroup.groupID
				WHERE typeActivityMaterials.typeID = @blueprintID AND typeActivityMaterials.activityID = 1 AND
					typeActivityMaterials.recycle = 1
			) AS tam ON invBlueprintTypes.productTypeID = tam.requiredTypeID
		) AS bp ON typeActivityMaterials.typeID = bp.blueprintTypeID OR typeActivityMaterials.typeID = @blueprintID 	
		INNER JOIN invTypes AS typeReq ON typeActivityMaterials.requiredTypeID = typeReq.typeID
		INNER JOIN invGroups AS typeGroup ON typeReq.groupID = typeGroup.groupID
		WHERE typeActivityMaterials.activityID = 1 AND typeActivityMaterials.recycle = 0 AND
			typeGroup.categoryID != 16 AND typeGroup.categoryID != 17
		GROUP BY requiredTypeID
	END

	RETURN";
                    adapter = new SqlDataAdapter(commandText, connection2);

                    try
                    {
                        adapter.SelectCommand.ExecuteNonQuery();
                        SetDBVersion(connection2, new Version("1.0.0.2"));
                    }
                    catch (Exception ex)
                    {
                        throw new EMMADataException(ExceptionSeverity.Critical,
                            "Unable to update stored procedure 'GetReprocesResults'.", ex);
                    }
                    #endregion
                }
                if (dbVersion.CompareTo(new Version("1.5.0.0")) < 0)
                {
                    #region Create INIT2 stored procedure
                    commandText = @"CREATE PROCEDURE dbo.INIT2
AS
	-- Thanks to Fubar for this SQL (and to the many others who contributed to this topic) -- 
	-- http://www.eveonline.com/ingameboard.asp?a=topic&threadID=1220024&page=2#31 --


	IF OBJECT_ID('[dbo].[typeBuildReqs]') IS NOT NULL
	  DROP TABLE [dbo].[typeBuildReqs]

	CREATE TABLE [dbo].[typeBuildReqs]
	(
	  typeID          smallint,
	  activityID      tinyint,
	  requiredTypeID  smallint,
	  quantity        int,
	  damagePerJob    float,
	  wasted		  tinyint,
	  
	  CONSTRAINT typeBuildReqs_PK PRIMARY KEY CLUSTERED 
                      (typeID, activityID, requiredTypeID, wasted)
	)


INSERT INTO typeBuildReqs (typeID, activityID, requiredTypeID, quantity,
                          damagePerJob,wasted)
	
(SELECT itm1.typeID, 1 AS activityID, itm1.requiredTypeID, (itm1.quantity-
isnull(itm2.quantity,0)) as quantity, 1 AS damagePerJob, 1 AS wasted
	
	FROM
	     (SELECT invBlueprintTypes.blueprintTypeID as typeID, 1 AS
             activityID, invTypeMaterials.materialTypeID AS requiredTypeID, 
	     invTypeMaterials.quantity, 1 AS damagePerJob, 1 AS wasted 
             FROM invTypeMaterials
				
	     INNER JOIN invBlueprintTypes 
                 ON invTypeMaterials.typeID =
                 invBlueprintTypes.productTypeID) as itm1
			
	     LEFT OUTER JOIN		
					
		(SELECT typeID, activityID, requiredTypeID , 
		sum(quantity) as quantity, damagePerJob, wasted
		FROM
			(SELECT t.typeID, 1 AS activityID, itm.materialTypeID
			as requiredTypeID , (itm.quantity * t.quantity) 
			AS quantity, 1 AS damagePerJob, 1 AS wasted

			FROM		
				(SELECT DISTINCT rtr.typeID, rtr.requiredTypeID,
				rtr.quantity
					FROM ramTypeRequirements AS rtr
			
					INNER JOIN	invTypes AS iT 
						ON rtr.requiredTypeID = iT.typeID 
			
					INNER JOIN	invGroups AS iG 
						ON iT.groupID = iG.groupID

				WHERE ((rtr.activityID = 1) AND (rtr.recycle = 1) 
					AND (iG.categoryID <> 4) 
					AND (iG.categoryID <> 17))) AS t
		
				INNER JOIN invTypeMaterials AS itm 
					ON t.requiredTypeID = itm.typeID) as itm3
		Group by typeID, activityID, requiredTypeID , damagePerJob, wasted)
		as itm2 
						
     on itm2.typeID = itm1.typeID and 
        itm2.activityID = itm1.activityID and
        itm2.requiredTypeID = itm1.requiredTypeID
		
WHERE (itm1.quantity-isnull(itm2.quantity,0)) > 0) 

UNION

(SELECT rtr2.typeID, rtr2.activityID, rtr2.requiredTypeID, rtr2.quantity,
        rtr2.damagePerJob, 0 AS wasted
	FROM ramTypeRequirements AS rtr2 
		
	INNER JOIN	invTypes AS types 
		ON rtr2.requiredTypeID = types.typeID 
			
	INNER JOIN	invBlueprintTypes AS bps 
		ON rtr2.typeID = bps.blueprintTypeID
			
	INNER JOIN	invGroups AS groups 
		ON types.groupID = groups.groupID 
			
	LEFT OUTER JOIN (SELECT typeID, materialTypeID, quantity 
                FROM invTypeMaterials) AS itm 
                ON (bps.productTypeID = itm.typeID AND 
                rtr2.requiredTypeID = itm.materialTypeID AND 
   	        (rtr2.quantity <= itm.quantity OR 
                rtr2.quantity > itm.quantity OR itm.quantity is null))
		
	WHERE ((groups.categoryID <> 16) AND (rtr2.activityID = 1) AND
                (rtr2.quantity > 0)))		

	RETURN";
                    adapter = new SqlDataAdapter(commandText, connection2);
                    try
                    {
                        adapter.SelectCommand.ExecuteNonQuery();
                        SetDBVersion(connection2, new Version("1.5.0.0"));
                    }
                    catch (Exception ex)
                    {
                        throw new EMMADataException(ExceptionSeverity.Critical,
                            "Unable to create stored procedure 'INIT2'.", ex);
                    }
                    #endregion
                }
                if (dbVersion.CompareTo(new Version("1.5.0.1")) < 0)
                {
                    #region Execute INIT2 stored procedure
                    try
                    {
                        adapter = new SqlDataAdapter("INIT2", connection2);
                        adapter.SelectCommand.ExecuteNonQuery();
                        SetDBVersion(connection2, new Version("1.5.0.1"));
                    }
                    catch (Exception ex)
                    {
                        throw new EMMADataException(ExceptionSeverity.Critical,
                            "Unable to execute stored procedure 'INIT2'.", ex);
                    }
                    #endregion
                }
                if (dbVersion.CompareTo(new Version("1.5.0.2")) < 0)
                {
                    #region Create typeBuildReqsGet stored procedure
                    commandText = @"CREATE PROCEDURE dbo.typeBuildReqsGet
	@itemID		int
AS
	SELECT *
	FROM typeBuildReqs
	WHERE typeID = @itemID
	RETURN";
                    adapter = new SqlDataAdapter(commandText, connection2);
                    try
                    {
                        adapter.SelectCommand.ExecuteNonQuery();
                        SetDBVersion(connection2, new Version("1.5.0.2"));
                    }
                    catch (Exception ex)
                    {
                        throw new EMMADataException(ExceptionSeverity.Critical,
                            "Unable to create stored procedure 'typeBuildReqsGet'.", ex);
                    }
                    #endregion
                }
                



            }
            catch (Exception ex)
            {
                EMMAException emmaEx = ex as EMMAException;
                if (emmaEx == null)
                {
                    new EMMAException(ExceptionSeverity.Error, "Problem updating database", ex);
                }
            }
            finally
            {
                connection2.Close();
            }
            #endregion
        }

        public static void InitDBs()
        {
            SqlConnection connection = new SqlConnection(Properties.Settings.Default.EMMA_DatabaseConnectionString + 
                ";Pooling=false");
            SqlConnection connection2 = new SqlConnection(Properties.Settings.Default.ebs_DATADUMPConnectionString + 
                ";Pooling=false");

            try
            {
                SqlDataAdapter adapter = null;
                string commandText = "";
                connection.Open();
                commandText =
                    //"sp_configure 'show advanced options', 1\r\n" +
                    //"RECONFIGURE\r\n" +
                    "sp_configure 'clr enabled', 1\r\n" +
                    "RECONFIGURE";
                adapter = new SqlDataAdapter(commandText, connection);

                try
                {
                    adapter.SelectCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new EMMADataException(ExceptionSeverity.Critical,
                        "Unable to enable the CLR on SQL server express.", ex);
                }
            }
            finally
            {
                connection.Close();
            }

            try
            {
                SqlDataAdapter adapter = null;
                string commandText = "";

                connection2.Open();
                commandText =
                    //"sp_configure 'show advanced options', 1\r\n" +
                    //"RECONFIGURE\r\n" +
                    "sp_configure 'clr enabled', 1\r\n" +
                    "RECONFIGURE";
                adapter = new SqlDataAdapter(commandText, connection2);

                try
                {
                    adapter.SelectCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new EMMADataException(ExceptionSeverity.Critical,
                        "Unable to enable the CLR on SQL server express.", ex);
                }
            }
            finally
            {
                connection2.Close();
            }
        }

        private static void SetDBVersion(SqlConnection connection, Version newVersion)
        {
            SqlDataAdapter adapter = null;
            string commandText = "";

            commandText =
                "ALTER PROCEDURE dbo._GetVersion\r\n" +
                "@version	varchar(50)		OUTPUT\r\n" +
                "AS\r\n" +
                "SET @version = '" + newVersion.ToString() + "'\r\n" +
                "RETURN\r\n";

            adapter = new SqlDataAdapter(commandText, connection);

            // Exceptions to be caught by the calling method.
            adapter.SelectCommand.ExecuteNonQuery();
        }



        private static void MigrateSettings()
        {
            try
            {
                // Added this check to prevent an error being logged if there is no previous version
                // of settings to migrate.
                if (Properties.Settings.Default.GetPreviousVersion("FirstRun") != null)
                {
                    Properties.Settings.Default.Upgrade();
                    //Properties.Settings.Default.ebs_DATADUMPConnectionString = (string)
                    //    Properties.Settings.Default.GetPreviousVersion("ebs_DATADUMPConnectionString");
                    //Properties.Settings.Default.EMMA_DatabaseConnectionString = (string)
                    //    Properties.Settings.Default.GetPreviousVersion("EMMA_DatabaseConnectionString");
                    Properties.Settings.Default.FirstRun = (bool)
                        Properties.Settings.Default.GetPreviousVersion("FirstRun");
                    #region Main window settings
                    try
                    {
                        Properties.Settings.Default.WindowState = (System.Windows.Forms.FormWindowState)
                            Properties.Settings.Default.GetPreviousVersion("WindowState");
                    }
                    catch
                    {
                        Properties.Settings.Default.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                    }
                    try
                    {
                        Properties.Settings.Default.WindowPos = (System.Drawing.Point)
                            Properties.Settings.Default.GetPreviousVersion("WindowPos");
                    }
                    catch
                    {
                        Properties.Settings.Default.WindowPos = new System.Drawing.Point(0, 0);
                    }
                    try
                    {
                        Properties.Settings.Default.WindowSize = (System.Drawing.Size)
                            Properties.Settings.Default.GetPreviousVersion("WindowSize");
                    }
                    catch
                    {
                        Properties.Settings.Default.WindowSize = new System.Drawing.Size(800, 600);
                    }
                    #endregion
                    #region Auto update settings
                    try
                    {
                        Properties.Settings.Default.AutoUpdate =
                            (bool)Properties.Settings.Default.GetPreviousVersion("AutoUpdate");
                    }
                    catch
                    {
                        Properties.Settings.Default.AutoUpdate = true;
                    }
                    try
                    {
                        Properties.Settings.Default.BetaUpdates =
                            (bool)Properties.Settings.Default.GetPreviousVersion("BetaUpdates");
                    }
                    catch
                    {
                        Properties.Settings.Default.BetaUpdates = false;
                    }
                    try
                    {
                        Properties.Settings.Default.UpdateServers =
                            (System.Collections.Specialized.StringCollection)
                            Properties.Settings.Default.GetPreviousVersion("UpdateServers");
                    }
                    catch
                    {
                        System.Collections.Specialized.StringCollection defaults =
                            new System.Collections.Specialized.StringCollection();
                        defaults.Add("www.starfreeze.com");
                        defaults.Add("www.eve-files.com");
                        Properties.Settings.Default.UpdateServers = defaults;
                    }
                    try
                    {
                        Properties.Settings.Default.LastEMMAUpdateCheck =
                            (DateTime)Properties.Settings.Default.GetPreviousVersion("LastEMMAUpdateCheck");
                    }
                    catch
                    {
                        Properties.Settings.Default.LastEMMAUpdateCheck = new DateTime(2000, 1, 1);
                    }
                    try
                    {
                        Properties.Settings.Default.DoDocCheck =
                            (bool)Properties.Settings.Default.GetPreviousVersion("DoDocCheck");
                    }
                    catch
                    {
                        Properties.Settings.Default.DoDocCheck = true;
                    }
                    try
                    {
                        Properties.Settings.Default.DocumentationVersion =
                            (int)Properties.Settings.Default.GetPreviousVersion("DocumentationVersion");
                    }
                    catch
                    {
                        Properties.Settings.Default.DocumentationVersion = Globals.CurrentDocVersion - 1;
                    }
                    #endregion
                    #region Connection check settings
                    try
                    {
                        Properties.Settings.Default.SkipConnectionCheck =
                            (bool)Properties.Settings.Default.GetPreviousVersion("SkipConnectionCheck");
                    }
                    catch
                    {
                        Properties.Settings.Default.SkipConnectionCheck = false;
                    }
                    #endregion
                }

                Properties.Settings.Default.Migrated = true;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                new EMMAException(ExceptionSeverity.Error, "Problem migrating settings from previous version", ex);
            }
        }

        private static void ProcessUpdateXML(string xmlFile)
        {
            string scriptDir = Path.Combine(Path.GetDirectoryName(xmlFile),
                Path.GetFileNameWithoutExtension(xmlFile));
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                // If the script directory already exists then we don't want to extract the script 
                // files as this indicates the update failed part-way through last time.
                // We just want to continue from where we were.
                if (!Directory.Exists(scriptDir))
                {
                    Directory.CreateDirectory(scriptDir);

                    XmlNodeList nodes = xmlDoc.SelectNodes("/EMMADatabaseUpdate/Script");
                    int counter = 0;
                    foreach (XmlNode node in nodes)
                    {
                        string sqlScript = node.InnerText;
                        string fileName = Path.Combine(scriptDir, "SQLScript" + counter.ToString("00000"));
                        if (File.Exists(fileName)) { File.Delete(fileName); }
                        File.WriteAllText(fileName, sqlScript);
                        counter++;
                    }
                }
            }
            catch (Exception ex)
            {
                // Since we've had a problem, we need to make sure the update script directory is
                // removed in order for the update to try again next time
                try { if (Directory.Exists(scriptDir)) { Directory.Delete(scriptDir, true); } }
                catch { }

                throw new EMMAException(ExceptionSeverity.Critical, 
                    "Problem processing database update script XML file", ex);
            }
        }


        #region old Stuff
        /*      public static void ClearTables()
                {
                    SqlConnection connection = new SqlConnection(Properties.Settings.Default.EMMA_DatabaseConnectionString);

                    try
                    {
                        SqlDataAdapter adapter = null;
                        string commandText = "";
                        SqlCommand command = null;
                        connection.Open();

                        commandText = @"
                            DELETE FROM Transactions
                            DELETE FROM Journal
                            DELETE FROM Assets
                            DELETE FROM Orders
                            DELETE FROM Contracts";

                        command = new SqlCommand(commandText, connection);
                        command.CommandType = System.Data.CommandType.Text;
                        command.CommandTimeout = 1000;

                        adapter = new SqlDataAdapter(command);
                        try
                        {
                            adapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch { }
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
         * */


        /*                if (dbVersion.CompareTo(new Version("x.x.x.x")) < 0)
                        {
                            #region Create OrderGetPage stored procedure
                            commandText =
                                    "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                    "'OrderGetPage') AND type LIKE 'P')\r\n" +
                                    "BEGIN\r\n" +
                                    "DROP PROCEDURE dbo.OrderGetPage\r\n" +
                                    "END";
                            adapter = new SqlDataAdapter(commandText, connection);
                            try
                            {
                                adapter.SelectCommand.ExecuteNonQuery();
                            }
                            catch (Exception) { }

                            commandText = @"CREATE PROCEDURE dbo.OrderGetPage
            @accessList			varchar(max),
            @itemIDs			varchar(max),
            @stationIDs			varchar(max),
            @type				varchar(4),
            @state				int,
            @startRow           int,
            @pageSize           int
            --@totalRows          int         OUTPUT
        AS
            SET NOCOUNT ON
    
            --SELECT @totalRows = COUNT(*)
            --FROM Orders
            --    JOIN CLR_accesslist_split(@accessList) a ON (Orders.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Orders.ForCorp = 1) OR (a.includePersonal = 1 AND Orders.ForCorp = 0)))
            --    JOIN CLR_intlist_split(@stationIDs) s ON (Orders.StationID = s.number OR s.number = 0)
            --    JOIN CLR_intlist_split(@itemIDs) i ON (Orders.ItemID = i.number OR i.number = 0)
            --WHERE (Orders.OrderState = @state OR @state = 0) AND ((@type LIKE 'Sell' AND Orders.BuyOrder = 0) OR (@type LIKE 'Buy' AND Orders.BuyOrder = 1) OR (@type NOT LIKE 'Buy' AND @type NOT LIKE 'Sell'))
   
            SELECT data.*
            FROM
            (
                SELECT Orders.*, row_number() OVER(ORDER BY Orders.Issued) as RowNumber
                FROM Orders
                    JOIN CLR_accesslist_split(@accessList) a ON (Orders.OwnerID = a.ownerID AND ((a.includeCorporate = 1 AND Orders.ForCorp = 1) OR (a.includePersonal = 1 AND Orders.ForCorp = 0)))
                    JOIN CLR_intlist_split(@stationIDs) s ON (Orders.StationID = s.number OR s.number = 0)
                    JOIN CLR_intlist_split(@itemIDs) i ON (Orders.ItemID = i.number OR i.number = 0)
                WHERE (Orders.OrderState = @state OR @state = 0) AND ((@type LIKE 'Sell' AND Orders.BuyOrder = 0) OR (@type LIKE 'Buy' AND Orders.BuyOrder = 1) OR (@type NOT LIKE 'Buy' AND @type NOT LIKE 'Sell'))
            ) AS data
            WHERE data.RowNumber BETWEEN (@startRow) AND (@startRow + @pageSize - 1)
        RETURN";

                            adapter = new SqlDataAdapter(commandText, connection);

                            try
                            {
                                adapter.SelectCommand.ExecuteNonQuery();

                                SetDBVersion(connection, new Version("x.x.x.x"));
                            }
                            catch (Exception ex)
                            {
                                throw new EMMADataException(ExceptionSeverity.Critical,
                                    "Unable to create stored procedure 'OrderGetPage'.", ex);
                            }
                            #endregion
                        }
                        if (dbVersion.CompareTo(new Version("x.x.x.x")) < 0)
                        {
                            #region Create TransGetPageBySingle stored procedure
                            commandText =
                                    "IF EXISTS (SELECT * FROM dbo.SYSOBJECTS WHERE (name LIKE " +
                                    "'TransGetPageBySingle') AND type LIKE 'P')\r\n" +
                                    "BEGIN\r\n" +
                                    "DROP PROCEDURE dbo.TransGetPageBySingle\r\n" +
                                    "END";
                            adapter = new SqlDataAdapter(commandText, connection);
                            try
                            {
                                adapter.SelectCommand.ExecuteNonQuery();
                            }
                            catch (Exception) { }

                            commandText = @"CREATE PROCEDURE [dbo].[TransGetPageBySingle]
            @ownerID			int,
            @itemIDs			varchar(max),
            @startDate			datetime,
            @endDate			datetime,
            @transType			varchar(4),
            @startRow           int,
            @pageSize           int
            --@totalRows          int       OUTPUT
        AS
        SET NOCOUNT ON

        DECLARE @rowsToGet	int
        SET @rowsToGet = @pageSize - 1

        --SELECT @totalRows = COUNT(*)
        --FROM Transactions
        --	JOIN CLR_intlist_split(@itemIDs) i ON (Transactions.ItemID = i.number OR i.number = 0)
        --WHERE (DateTime BETWEEN @startDate AND @endDate) AND 
        --	((@transType = 'buy' AND Transactions.BuyerID = @ownerID) OR 
        --	(@transType = 'sell' AND Transactions.SellerID = @ownerID))


        --SELECT @rowsToGet = CASE WHEN (@totalRows - @startRow) > (@pageSize - 1) 
        --	THEN (@pageSize - 1) ELSE (@totalRows - @startRow) END

        SELECT data.ID, data.DateTime, data.Quantity, data.ItemID, data.Price, data.BuyerID, data.SellerID, data.BuyerCharacterID, data.SellerCharacterID, data.StationID, data.RegionID, data.BuyerForCorp, data.SellerForCorp, data.BuyerWalletID, data.SellerWalletID
        FROM
        (
            SELECT Transactions.*, row_number() OVER(ORDER BY Transactions.DateTime) as RowNumber
            FROM Transactions
                INNER JOIN CLR_intlist_split(@itemIDs) i ON (i.number = 0 OR Transactions.ItemID = i.number)
            WHERE (Transactions.DateTime BETWEEN @startDate AND @endDate) AND 
                ((@transType = 'buy' AND Transactions.BuyerID = @ownerID) OR 
                (@transType = 'sell' AND Transactions.SellerID = @ownerID))
        ) AS data
        WHERE data.RowNumber BETWEEN (@startRow) AND (@startRow + @rowsToGet)
        ORDER BY RowNumber";

                            adapter = new SqlDataAdapter(commandText, connection);

                            try
                            {
                                adapter.SelectCommand.ExecuteNonQuery();

                                SetDBVersion(connection, new Version("x.x.x.x"));
                            }
                            catch (Exception ex)
                            {
                                throw new EMMADataException(ExceptionSeverity.Critical,
                                    "Unable to create stored procedure 'TransGetPageBySingle'.", ex);
                            }
                            #endregion
                        }
                        if (dbVersion.CompareTo(new Version("x.x.x.x")) < 0)
                        {
                            #region Update indicies for Transactions, Orders and Assets
                            try
                            {
                                adapter.SelectCommand.ExecuteNonQuery();

                                commandText = @"CREATE NONCLUSTERED INDEX [IX_Transactions_TimeOwnerItem] ON [dbo].[Transactions] 
        (
            [DateTime] ASC,
            [BuyerID] ASC,
            [SellerID] ASC,
            [ItemID] ASC
        )
        INCLUDE ( [ID],
        [Quantity],
        [Price],
        [BuyerCharacterID],
        [SellerCharacterID],
        [StationID],
        [RegionID],
        [BuyerForCorp],
        [SellerForCorp],
        [BuyerWalletID],
        [SellerWalletID])";
                                adapter = new SqlDataAdapter(commandText, connection);
                                adapter.SelectCommand.ExecuteNonQuery();

                                commandText = @"CREATE NONCLUSTERED INDEX [IX_Orders_TimeOwnerItem] ON [dbo].[Orders] 
        (
            [OrderState] ASC,
            [BuyOrder] ASC,
            [ItemID] ASC,
            [StationID] ASC,
            [OwnerID] ASC,
            [ForCorp] ASC
        )
        INCLUDE ( [TotalVol],
        [RemainingVol],
        [Price],
        [MinVolume],
        [Range],
        [Duration],
        [Escrow],
        [Price],
        [Issued],
        [Processed])";
                                adapter = new SqlDataAdapter(commandText, connection);
                                adapter.SelectCommand.ExecuteNonQuery();



                                SetDBVersion(connection, new Version("x.x.x.x"));
                            }
                            catch (Exception ex)
                            {
                                throw new EMMADataException(ExceptionSeverity.Critical,
                                    "Unable to update indicies for Transactions, Orders and Assets.", ex);
                            }
                            #endregion
                        }

        */

        #endregion
    }
}