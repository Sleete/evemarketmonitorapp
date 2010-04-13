using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

using EveMarketMonitorApp.Common;

namespace EveMarketMonitorApp.Common
{
    static class Compression
    {
        private const int BUFFERSIZE = 100000;
        private const long FOURGIG = 4000000000;
        private static string _tempDataDir = string.Format("{0}Temp{1}Compression{1}",
                    Globals.AppDataDir, Path.DirectorySeparatorChar);
        private static string _tempDataFile = string.Format("{0}Data.tmp", _tempDataDir);

        /// <summary>
        /// Uncompress the specified compound file and write the individual files to the specified destination
        /// directory.
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="dstDir"></param>
        public static float DecompressDirectory(string srcFile, string dstDir)
        {
            if (!dstDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                dstDir = dstDir + Path.DirectorySeparatorChar;
            }
            if (!Directory.Exists(_tempDataDir)) { Directory.CreateDirectory(_tempDataDir); }


            // First, decompress the file.
            DecompressFile(srcFile, _tempDataFile);
            // Build destination directory and files.
            float fileVersion = BuildDirFromFile(_tempDataFile, dstDir);

            File.Delete(_tempDataFile);

            return fileVersion;
        }

        /// <summary>
        /// Build the contents of the specified destination directory from the specified EMMA compound file.
        /// </summary>
        /// <param name="srcFile"></param>
        /// <param name="dstDir"></param>
        private static float BuildDirFromFile(string srcFile, string dstDir)
        {
            int bytesInBuffer = 0;
            long bytesSoFar = 0;
            byte[] buffer = new byte[BUFFERSIZE];
            string line = "";
            string currentFileName = "";
            long currentFileSize = 0;
            float fileVersion = 0;
            FileStream reader = File.OpenRead(srcFile);

            try
            {
                string firstLine = ReadLineFromStream(reader);
                // First, make sure it's an EMMA save file..
                if (!firstLine.StartsWith("**-- EMMA Compound Save File"))
                {
                    throw new EMMACompressionException(ExceptionSeverity.Error,
                        "Supplied file is not an EMMA compound file. (" + srcFile + ")");
                }

                if (firstLine.Contains("Version:"))
                {
                    fileVersion = float.Parse(firstLine.Substring(firstLine.IndexOf("Version:") + 8),
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                }

                // Create the destination directory if required.
                if (!Directory.Exists(dstDir))
                {
                    Directory.CreateDirectory(dstDir);
                }
                else
                {
                    Directory.Delete(dstDir, true);
                    Directory.CreateDirectory(dstDir);
                }

                while (reader.Position < reader.Length)
                {
                    line = ReadLineFromStream(reader);
                    FileStream writer = null;
                    bytesSoFar = 0;

                    try
                    {
                        // Search for file headers, if we find one then populate the file name and size variables.
                        if (line.StartsWith("**--"))
                        {
                            int fileNameLoc = line.IndexOf("FileName:") + 9;
                            int fileSizeLoc = line.IndexOf("FileSize:") + 9;
                            currentFileName = line.Substring(fileNameLoc, (fileSizeLoc - 9) - fileNameLoc);
                            currentFileSize = long.Parse(line.Substring(fileSizeLoc, line.Length - fileSizeLoc),
                                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

                            writer = File.Create(dstDir + currentFileName);
                        }
                        // If a file header has been identified then extract the data and write the file.
                        while (!currentFileName.Equals(""))
                        {
                            // Read a block of data into the buffer but do not go past the end of this file
                            // within the compound file.
                            bytesInBuffer = reader.Read(buffer, 0,
                                (int)Math.Min(currentFileSize - bytesSoFar, (long)BUFFERSIZE));
                            bytesSoFar += bytesInBuffer;

                            // write out the data in the buffer to the destination file.
                            writer.Write(buffer, 0, bytesInBuffer);

                            // If we've reached the end of the file within the compound file then clear the
                            // current file variables to allow us to go back round and start looking for the 
                            // next file.
                            if (bytesInBuffer < BUFFERSIZE)
                            {
                                currentFileName = "";
                                currentFileSize = 0;
                            }

                        }
                    }
                    finally
                    {
                        if (writer != null) writer.Close();
                    }
                }

            }
            finally
            {
                reader.Close();
            }

            return fileVersion;
        }


        private static string ReadLineFromStream(FileStream reader)
        {
            string retVal = "";
            List<byte> line = new List<byte>();
            bool done = false;
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            while (!done)
            {
                if (reader.Position < reader.Length)
                {
                    byte newByte = (byte)reader.ReadByte();
                    line.Add(newByte);
                    byte[] byteArray = { newByte };
                    string newChar = encoding.GetString(byteArray);
                    if (newChar.Equals("\n")) { done = true; }
                }
                else
                {
                    done = true;
                }
            }

            retVal = encoding.GetString(line.ToArray());
            return retVal;
        }


        /// <summary>
        /// Produce a compressed file containing all of the files in the specified directory.
        /// Note this does NOT include sub-directories or files in sub-directories.
        /// </summary>
        /// <param name="srcDir"></param>
        /// <param name="dstFile"></param>
        /// <param name="deleteSrcDir"></param>
        public static void CompressDirectory(string srcDir, string dstFile, bool deleteSrcDir)
        {
            // This works by first building a big file containing the data from all the files
            // in the source directory + a bit of header information for each one.
            // This large file is then compressed.
            // Note that because of limitations with GZipStream this will not work on directories
            // with 4Gb or more of data. (Should not be a problem here!)

            // First do some checks...
            if (!Directory.Exists(srcDir))
            {
                throw new EMMACompressionException(ExceptionSeverity.Error, "Source directory for compression does " +
                    "not exist. (" + srcDir + ")");
            }
            if (GetTotalDirSize(srcDir) > FOURGIG)
            {
                throw new EMMACompressionException(ExceptionSeverity.Error, "Source directory for compression " +
                    "contains total data of more than four gigabytes. (" + srcDir + ")");
            }

            try
            {
                if (!Directory.Exists(_tempDataDir)) { Directory.CreateDirectory(_tempDataDir); }

                // Build the single large temp file with all the other files inside it.
                BuildFileFromDir(srcDir, _tempDataFile);
                // Compress the large file and write out to the destination location.
                CompressFile(_tempDataFile, dstFile);

                if (deleteSrcDir)
                {
                    // Delete the source dir if the flag is on.
                    Directory.Delete(srcDir, true);
                }
            }
            catch (Exception ex)
            {
                throw new EMMACompressionException(ExceptionSeverity.Error, "Problem compressing directory to file.\r\n" +
                    "Source directory: " + srcDir + "\r\nDestination File: " + dstFile, ex);
            }
            finally
            {
                // Delete the temp file.
                File.Delete(_tempDataFile);
            }
        }

        /// <summary>
        /// Take all the files in the source directory and put them into one big file separated by
        /// header information.
        /// </summary>
        /// <param name="srcDir"></param>
        /// <param name="dstFile"></param>
        private static void BuildFileFromDir(string srcDir, string dstFile)
        {
            int charsInBuffer = 0;
            byte[] buffer = new byte[BUFFERSIZE];
            string[] files = Directory.GetFiles(srcDir);
            FileStream writer = File.Create(dstFile);

            try
            {
                byte[] bytesToWrite = StrToByteArray("**-- EMMA Compound Save File Version: 1.2");
                writer.Write(bytesToWrite, 0, bytesToWrite.Length);

                foreach (string file in files)
                {
                    // Loop through all files in the source directory
                    FileInfo fileInfo = new FileInfo(file);
                    FileStream reader = File.OpenRead(file);
                    long realFileSize = reader.Length;

                    try
                    {
                        // write the header for this file.
                        byte[] bytesToWrite2 = StrToByteArray(string.Format("\r\n**-- FileName:{0} FileSize:{1}\r\n",
                            fileInfo.Name, realFileSize));
                        writer.Write(bytesToWrite2, 0, bytesToWrite2.Length); //reader.BaseStream.Length));

                        // Read blocks of the source file and write them out to the destination file.
                        charsInBuffer = BUFFERSIZE;
                        while (charsInBuffer == BUFFERSIZE)
                        {
                            charsInBuffer = reader.Read(buffer, 0, BUFFERSIZE);
                            writer.Write(buffer, 0, charsInBuffer);
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            finally
            {
                writer.Close();
            }
        }

        /// <summary>
        /// Decompress the specified file and create a new file that contains the uncompressed data.
        /// </summary>
        /// <param name="srcFile">The file to be decompressed</param>
        /// <param name="dstFile">The destination file for the decompressed data</param>
        public static void DecompressFile(string srcFile, string dstFile)
        {
            int bytesInBuffer = BUFFERSIZE;
            int bytesSoFar = 0;
            byte[] buffer = new byte[BUFFERSIZE];

            if (!File.Exists(srcFile))
            {
                throw new EMMACompressionException(ExceptionSeverity.Error, "Source file for decompression does " +
                    "not exist. (" + srcFile + ")");
            }

            FileStream inputStream = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream outputStream = new FileStream(dstFile, FileMode.Create, FileAccess.Write,
                FileShare.None);
            GZipStream zipStream = new GZipStream(inputStream, CompressionMode.Decompress, true);
            try
            {
                while (bytesInBuffer == BUFFERSIZE)
                {
                    bytesInBuffer = zipStream.Read(buffer, 0, BUFFERSIZE);
                    outputStream.Write(buffer, 0, bytesInBuffer);
                    bytesSoFar += bytesInBuffer;
                }
            }
            catch (Exception ex)
            {
                throw new EMMACompressionException(ExceptionSeverity.Error, "Problem decompressing data.\r\n" +
                    "Source File: " + srcFile + "\r\nDestination File: " + dstFile, ex);
            }
            finally
            {
                inputStream.Close();
                outputStream.Close();
                zipStream.Close();
            }
        }

        /// <summary>
        /// Read the specified file, compress the data and create a new file containing the compressed data.
        /// </summary>
        /// <param name="srcFile">The file to be compressed</param>
        /// <param name="dstFile">The destination file for the compressed data</param>
        public static void CompressFile(string srcFile, string dstFile)
        {
            int bytesInBuffer = 0;
            int bytesSoFar = 0;
            byte[] buffer = new byte[BUFFERSIZE];

            if (!File.Exists(srcFile))
            {
                throw new EMMACompressionException(ExceptionSeverity.Error, "Source file for decompression does " +
                    "not exist. (" + srcFile + ")");
            }

            FileStream outputStream = new FileStream(dstFile, FileMode.Create, FileAccess.Write,
                FileShare.None);
            GZipStream zipStream = new GZipStream(outputStream, CompressionMode.Compress, true);

            FileStream inputStream = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read);

            try
            {
                while (bytesSoFar < inputStream.Length)
                {
                    bytesInBuffer = inputStream.Read(buffer, 0, BUFFERSIZE);
                    zipStream.Write(buffer, 0, bytesInBuffer);
                    bytesSoFar += bytesInBuffer;
                }
            }
            catch (Exception ex)
            {
                throw new EMMACompressionException(ExceptionSeverity.Error, "Problem compressing data.\r\n" +
                    "Source File: " + srcFile + "\r\nDestination File: " + dstFile, ex);
            }
            finally
            {
                inputStream.Close();
                zipStream.Close();
                outputStream.Close();
            }

        }


        /// <summary>
        /// Get the size of all the files in the specified directory.
        /// Note this does NOT include sub-directories or files in sub-directories.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private static long GetTotalDirSize(string directory)
        {
            // don't really need to know about sub dirs so removed the relevant lines
            string[] files;
            //string[] dirs;
            long retVal = 0;

            files = Directory.GetFiles(directory);
            //dirs = Directory.GetDirectories(directory);

            foreach (string file in files)
            {
                retVal += new FileInfo(file).Length;
            }
            //foreach (string dir in dirs)
            //{
            //    retVal += GetTotalDirSize(dirs);
            //}

            return retVal;
        }

        public static byte[] StrToByteArray(string str)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            return encoding.GetBytes(str);
        }

    }
}
