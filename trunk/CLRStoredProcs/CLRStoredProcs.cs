using System;
using System.Collections;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

namespace EveMarketMonitorApp.DatabaseClasses
{
    public class ListSplit
    {
        [SqlFunction(FillRowMethodName = "CharlistFillRow", TableDefinition = "str nvarchar(4000)")]
        public static IEnumerable CLR_charlist_split(SqlString str)
        {
            char[] delim = { '|' };
            return str.Value.Split(delim, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void CharlistFillRow(object row, out string str)
        {
            str = (string)row;
            str = str.Trim();
        }




        [SqlFunction(FillRowMethodName = "IntlistFillRow", TableDefinition = "number int")]
        public static IEnumerable CLR_intlist_split(SqlString str)
        {
            char[] delim = { '|', ' ', ',' };
            return str.Value.Split(delim, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void IntlistFillRow(object row, out int n)
        {
            n = Convert.ToInt32((string)row);
        }




        [SqlFunction(FillRowMethodName = "BigIntlistFillRow", TableDefinition = "number bigint")]
        public static IEnumerable CLR_bigintlist_split(SqlString str)
        {
            char[] delim = { '|', ' ', ',' };
            return str.Value.Split(delim, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void BigIntlistFillRow(object row, out long n)
        {
            n = Convert.ToInt64((string)row);
        }




        [SqlFunction(FillRowMethodName = "BuildOwnerAccessRow", TableDefinition = "ownerID bigint, includePersonal bit, includeCorporate bit")]
        public static IEnumerable CLR_accesslist_split(SqlString str)
        {
            char[] delim = { '|' };
            return str.Value.Split(delim, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void BuildOwnerAccessRow(object row, out long id, out bool personal, out bool corporate)
        {
            char[] delim = { ',' };
            string[] values = ((string)row).Split(delim);
            if (values.Length == 3)
            {
                id = long.Parse(values[0]);
                personal = bool.Parse(values[1]);
                corporate = bool.Parse(values[2]);
            }
            else
            {
                throw new Exception("Bad parameter passed to BuildOwnerAccessRow: '" + row.ToString() + "'.\r\nExpecting '<int - ownerID>,<bool - includePersonal>,<bool - includeCorp>'");
            }
        }




        [SqlFunction(FillRowMethodName = "BuildFinanceAccessRow", TableDefinition = "ownerID bigint, walletID1 smallint, walletID2 smallint, walletID3 smallint, walletID4 smallint, walletID5 smallint, walletID6 smallint")]
        public static IEnumerable CLR_financelist_split(SqlString str)
        {
            char[] delim = { '|' };
            return str.Value.Split(delim, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void BuildFinanceAccessRow(object row, out long id, out short walletID1, out short walletID2, out short walletID3, out short walletID4, out short walletID5, out short walletID6)
        {
            char[] delim = { ',' };
            string[] values = ((string)row).Split(delim);
            if (values.Length == 7)
            {
                id = long.Parse(values[0]);
                walletID1 = short.Parse(values[1]);
                walletID2 = short.Parse(values[2]);
                walletID3 = short.Parse(values[3]);
                walletID4 = short.Parse(values[4]);
                walletID5 = short.Parse(values[5]);
                walletID6 = short.Parse(values[6]);
            }
            else
            {
                throw new Exception("Bad parameter passed to BuildOwnerAccessRow: '" + row.ToString() + "'.\r\nExpecting '<int - ownerID>,<short - walletID> x 6'");
            }
        }
    }
}
