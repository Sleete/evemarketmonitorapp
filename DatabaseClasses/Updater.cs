using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;

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
                        defaults.Add("www.eve-files.com");
                        defaults.Add("www.starfreeze.com");
                        Properties.Settings.Default.UpdateServers = defaults;
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
    }
}