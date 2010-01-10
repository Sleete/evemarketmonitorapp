using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.DatabaseClasses
{
    static class JournalGenerations
    {
        private static EMMADataSet.JournalGenerationsDataTable dataTable = 
            new EMMADataSet.JournalGenerationsDataTable();
        private static EMMADataSetTableAdapters.JournalGenerationsTableAdapter tableAdapter = 
            new EveMarketMonitorApp.DatabaseClasses.EMMADataSetTableAdapters.JournalGenerationsTableAdapter();

        private static long _currentOffset;
        private static int _currentLargestID;
        private static DateTime _currentEndDate;
        private static DateTime _currentStartDate;
        private static int _currentGenID = 0;

        /// <summary>
        /// Used for getting the ID offset for the specified journal entry. CCP use 32-bit ints as journal
        /// IDs so every now and then, old entries are cleared out and shuffled 'down' to make way for 
        /// new entries.
        /// More information here: http://myeve.eve-online.com/ingameboard.asp?a=topic&threadID=764508
        /// 
        /// Note this should not be called for every journal entry in an import, that would be far too slow.
        /// Instead, use it on the first and last ones. If both throw no exceptions and have the same offset
        /// then it can be assumed that the same offset can be used for all entries in that import.
        /// </summary>
        /// <param name="journalRow"></param>
        /// <returns></returns>
        public static long GetOffset(EMMADataSet.JournalRow journalRow, DateTime fileDate)
        {
            long retVal = 0;

            lock (tableAdapter)
            {
                if (dataTable.Count == 0) { LoadData(); }
                retVal = _currentOffset;

                DateTime entryTime = fileDate;
                long journalID = journalRow.ID;
                DateTime latestDate = LatestAllowedDate();

                if (entryTime.CompareTo(latestDate) > 0)
                {
                    // Looks like the entry is too far beyond the predicted end date to be 
                    // in the current generation.

                    // Attempt to create a new generation entry by calculating the offset.
                    retVal = TryGetNewOffset(journalRow, fileDate);
                }
                else
                {
                    if (entryTime.CompareTo(_currentStartDate) < 0)
                    {
                        #region Get correct generation as well as next and previous ones.
                        // Must be dealing with an entry from an older generation.
                        EMMADataSet.JournalGenerationsRow entryGen = null;
                        EMMADataSet.JournalGenerationsRow previousGen = null;
                        EMMADataSet.JournalGenerationsRow nextGen = null;
                        DateTime prevGenDate = DateTime.MinValue;
                        DateTime nextGenDate = DateTime.MaxValue;

                        // Find the generation containing this entry by date
                        DataRow[] rows = dataTable.Select("EndDate >= '" + entryTime +
                            "' AND StartDate < '" + entryTime + "'");
                        if (rows.Length > 0)
                        {
                            entryGen = (EMMADataSet.JournalGenerationsRow)rows[0];
                        }

                        // Find the closest geneartion before the entry date
                        rows = dataTable.Select("EndDate < '" + entryTime + "'");
                        foreach (DataRow row in rows)
                        {
                            EMMADataSet.JournalGenerationsRow prevGenRow = (EMMADataSet.JournalGenerationsRow)row;
                            if (prevGenRow.EndDate.CompareTo(prevGenDate) > 0)
                            {
                                previousGen = prevGenRow;
                                prevGenDate = previousGen.EndDate;
                            }
                        }

                        // ...and the closest generation after the entry date. 
                        rows = dataTable.Select("StartDate >= '" + entryTime + "'");
                        foreach (DataRow row in rows)
                        {
                            EMMADataSet.JournalGenerationsRow nextGenRow = (EMMADataSet.JournalGenerationsRow)row;
                            if (nextGenRow.StartDate.CompareTo(nextGenDate) < 0)
                            {
                                nextGen = nextGenRow;
                                nextGenDate = nextGen.StartDate;
                            }
                        }
                        #endregion

                        if (entryGen != null)
                        {
                            // The entry date is within a specific geenration's start and end dates.
                            // However, for journal entries that are actually part of two generations (with a 
                            // different ID in each) we need to determine the 'correct' offset to use.
                            //
                            // e.g. An entry is added that is detmined to be part of geneartion 0, it increases
                            // the end date of that generation to the 16th (does not matter what month).
                            // At a later date, generation 1 becomes active and another journal entry is 
                            // added that is also from the 16th. This will end up here with 'entryGen' set
                            // to generation 0.
                            // This is a problem because the journal ID will be much lower. Even though the 
                            // journal entry was part of gen 0, the current entry we are trying to add has a 
                            // gen 1 based ID.
                            if (nextGen != null)
                            {
                                if ((nextGen.Offset - entryGen.Offset) - journalID > 500000000)
                                {
                                    // If the difference from the entry's generation offset to the next generation's
                                    // offset minus the journal ID is more than 500 mil then we know that we've got
                                    // the wrong offset for this ID, use the next one instead.
                                    entryGen = nextGen;
                                }
                            }
                            if (previousGen != null)
                            {
                                // The opposite problem may also occur, though it's highly unlikley.
                                if ((entryGen.Offset - previousGen.Offset) - journalID < 0)
                                {
                                    entryGen = previousGen;
                                }
                            }

                            retVal = entryGen.Offset;
                        }
                        else
                        {
                            // If we cannot match the journal entry date with any know generations then 
                            // if it is between two generations then we can help narrow down the start 
                            // or end date of one of them.

                            if (previousGen != null && nextGen != null)
                            {
                                // If the ID is bigger than the largest recorded one in the older generation
                                // then the entry is part of that older generation.
                                // Otherwise, it is part of the newer generation.
                                if (journalID > previousGen.LargestID)
                                {
                                    retVal = previousGen.Offset;
                                    UpdateGeneration(previousGen.ID, journalID, entryTime, new DateTime(1, 1, 1));
                                }
                                else
                                {
                                    retVal = nextGen.Offset;
                                    UpdateGeneration(nextGen.ID, 0, new DateTime(1, 1, 1), entryTime);
                                }
                            }
                            else
                            {
                                // If the entry date is not even between any generations then throw an exception
                                throw new EMMADataException(ExceptionSeverity.Critical, "Unable to find a journal ID " +
                                    "generation that matches the given file date - " + entryTime.ToString() +
                                    ". Cannot add this entry to the database.");
                            }
                        }
                    }
                    else
                    {
                        // If the entry is within the maximum time range for the current generation and
                        // yet the supplied ID is more than 500 million less than the expected 
                        // ID and the entry time is after our current generation end date then we must 
                        // be dealing with a new generation. 
                        // (If the entry time is NOT after our current generation end date then it's simply
                        // a very old entry from the current generation)
                        if (ExpectedID(entryTime) - journalID > 500000000 && entryTime.CompareTo(_currentEndDate) >= 0)
                        {
                            // Attempt to create a new generation entry by calculating the offset.
                            retVal = TryGetNewOffset(journalRow, fileDate);
                        }
                        else
                        {
                            // Otherwise, we are dealing with transactions for the current generation.
                            // Update largestID and end date if required.
                            bool update = false;
                            if (journalID > _currentLargestID)
                            {
                                _currentLargestID = (int)journalID;
                                update = true;

                                if (entryTime.CompareTo(_currentEndDate) > 0)
                                {
                                    _currentEndDate = entryTime;
                                    update = true;
                                }
                            }
                            if (update)
                            {
                                UpdateGeneration(_currentGenID, _currentLargestID, _currentEndDate, new DateTime(1, 1, 1));
                            }
                        }

                    }
                }
            }

            return retVal;
        }


        /// <summary>
        /// Attempt to determine the offset of the new generation and create it in the database.
        /// </summary>
        /// <param name="journalRow"></param>
        /// <returns></returns>
        private static long TryGetNewOffset(EMMADataSet.JournalRow journalRow, DateTime fileDate)
        {
            long retVal = 0;
            long journalID = journalRow.ID;
            DateTime entryTime = journalRow.Date;

            // Interpolate the current generation's values and use the supplied
            // entry time to estimate what the ID SHOULD be if it was part of the next 
            // generation (as opposed to an even later one).

            // Take 1.5 bil off the estimated ID to account for the expected rough value of 
            // the offset.
            double estimatedID = ExpectedID(entryTime) - 1500000000;

            if (Math.Abs(estimatedID - journalID) < 800000000)
            {
                // If the supplied ID is within 800 mil (random figure) of the estimated ID 
                // then we are almost certainly dealing with the generation after the current one.
                // Try and match the entry to an existing one. If we find one then we
                // can work out the offset for the new geneartion.
                long IDdifference;
                if (MatchEntry(journalRow, out IDdifference))
                {
                    retVal = IDdifference;

                    CreateGeneration(retVal, journalID, fileDate, fileDate.AddDays(1));
                }
                else
                {
                    throw new EMMADataException(ExceptionSeverity.Critical, "The journal entries you are adding" +
                        " appear to be for a generation after the latest one in the EMMA database.\r\n" +
                        "A match cannot be found between existing journal entries and the ones being imported " +
                        "so EMMA cannot workout the ID offset for the new generation.\r\n" +
                        "You will be unable to import journal entries from the EveAPI until the EMMA database " +
                        "is updated with newer journal generation data.");
                }
            }
            else
            {
                throw new EMMADataException(ExceptionSeverity.Critical, "The journal entries you are adding" +
                    " appear to be for a generation after the latest one in the EMMA database.\r\n" +
                    "You will be unable to import journal entries from the EveAPI until the EMMA database " +
                    "is updated with newer journal generation data.");
            }

            return retVal;
        }


        /// <summary>
        /// Try and match the specified journal row to another in the database with a different ID.
        /// i.e. the same entry but from a different ID generation.
        /// </summary>
        /// <param name="journalRow"></param>
        /// <param name="IDdifference"></param>
        /// <returns></returns>
        private static bool MatchEntry(EMMADataSet.JournalRow journalRow, out long IDdifference)
        {
            bool retVal = false;
            IDdifference = 0;

            EMMADataSet.JournalRow matchingRow = Journal.FindEntry(journalRow.Date, journalRow.SenderID, 
                journalRow.RecieverID, journalRow.Amount);

            if (matchingRow != null)
            {
                IDdifference = Math.Abs(matchingRow.ID - journalRow.ID);
                retVal = true;
            }

            return retVal;
        }


        /// <summary>
        /// This is used to return the latest estimated date the next geneartion change could occur.
        /// When a journal file is loaded, if the entries are significantly beyond this date,
        /// the user should be informed and the file should not be saved to the database.
        /// </summary>
        /// <returns></returns>
        private static DateTime LatestAllowedDate()
        {
            /*
            double idsPerDay = GetIDsPerDay();
            // work out how many IDs are left in this generation and divide by the number 
            // of ids being created per cay.
            double daysLeft = (int.MaxValue - _currentLargestID) / (idsPerDay > 0 ? idsPerDay : 1);
            // Just limit it to avoid any wierdness...
            if (daysLeft > 10000) daysLeft = 10000;

            return _currentEndDate.AddDays(daysLeft);
             * */

            // CCP HAVE CHANGED EVE TO USE 64BIT IDS. THEREFORE, JOURNAL ID GENERATIONS ARE NO LONGER NEEDED.
            return new DateTime(2100, 1, 1);
        }

        /// <summary>
        /// Get the expected ID value for a journal entry at the specified date/time assuming that
        /// it is part of the current generation.
        /// </summary>
        /// <param name="IDDateTime"></param>
        /// <returns></returns>
        private static long ExpectedID(DateTime IDDateTime)
        {
            double idsPerDay = GetIDsPerDay();
            // Get the number of days between the last entry of the current generation
            // and the date/time we want to know the expected ID for.
            double daysDifference = ((TimeSpan)IDDateTime.Subtract(_currentEndDate)).TotalDays;
            long expectedID = (long)(_currentLargestID + daysDifference * idsPerDay);

            return expectedID;
        }

        /// <summary>
        /// Get the approximate number of journal IDs generated per day in the current generation
        /// </summary>
        /// <returns></returns>
        private static double GetIDsPerDay()
        {
            // Calculate how many days this generation has been active.
            double daysSoFar = ((TimeSpan)_currentEndDate.Subtract(_currentStartDate)).TotalDays;
            // If the database has a new generation in it but only 1 day's worth of data
            // then the idsPerDay is way too high and causes entries to be rejected just a 
            // few days older than the current end date.
            // Instead, make sure we always assume a reasonable number of days...
            if (daysSoFar < 4) { daysSoFar = 4; }
            // assume approx 350 mil IDs left over from the last generation when it was migrated.
            // Use this to calulate the average number of IDs created each day.
            double idsPerDay = (_currentLargestID - 350000000) / (daysSoFar > 0 ? daysSoFar : 1);
            return idsPerDay;
        }


        private static void CreateGeneration(long offset, long largetID, DateTime startDate, DateTime endDate)
        {
            tableAdapter.ClearBeforeFill = true;
            tableAdapter.Fill(dataTable);

            EMMADataSet.JournalGenerationsRow row = dataTable.NewJournalGenerationsRow();
            row.LargestID = (int)largetID;
            row.EndDate = endDate;
            row.StartDate = startDate;
            row.Offset = offset;
            dataTable.AddJournalGenerationsRow(row);

            _currentGenID = row.ID;
            _currentEndDate = row.EndDate;
            _currentStartDate = row.StartDate;
            _currentOffset = row.Offset;
            _currentLargestID = row.LargestID;

            tableAdapter.Update(dataTable);
        }


        private static void UpdateGeneration(int genID, long newLargestID, DateTime newEndDate, DateTime newStartDate)
        {
            if (genID >= 0)
            {
                tableAdapter.ClearBeforeFill = true;
                try
                {
                    tableAdapter.Fill(dataTable);
                }
                catch (Exception) { }

                EMMADataSet.JournalGenerationsRow row = dataTable.FindByID(genID);
                if (newLargestID != 0)
                {
                    row.LargestID = (int)newLargestID;
                }
                if (newEndDate.Year != 1)
                {
                    row.EndDate = newEndDate;
                }
                if (newStartDate.Year != 1)
                {
                    row.StartDate = newStartDate;
                }

                tableAdapter.Update(row);
            }
        }


        private static void LoadData() 
        {
            tableAdapter.Fill(dataTable);

            _currentEndDate = DateTime.MinValue;
            foreach (EMMADataSet.JournalGenerationsRow row in dataTable)
            {
                if (row.EndDate.CompareTo(_currentEndDate) > 0)
                {
                    _currentEndDate = row.EndDate;
                    _currentStartDate = row.StartDate;
                    _currentLargestID = row.LargestID;
                    _currentOffset = row.Offset;
                    _currentGenID = row.ID;
                }
            }

        }

    }
}
