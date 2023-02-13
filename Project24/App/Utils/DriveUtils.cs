/*  DriveUtils.cs
 *  Version: 1.8 (2023.02.13)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.IO;
using System.Text;

namespace Project24.App
{
    public class DriveUtils
    {
        public static DriveUtils AppDriveUtils { get; private set; }
        public static DriveUtils NasDriveUtils { get; private set; }

        public static void Init()
        {
            NasDriveUtils = new DriveUtils(AppConfig.NasRoot);
            AppDriveUtils = new DriveUtils(AppConfig.AppNextRoot);

            s_IsReady = true;
        }

        public static string AppRootPath = Path.GetFullPath(AppUtils.AppRoot);

        public static string AppNextRootPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.AppNextRoot);
        public static string TmpRootPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.TmpRoot);
        public static string DataRootPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.DataRoot);
        public static string DeletedDataRootPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.DataRoot + "/../deletedData");
        public static string NasRootPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.NasRoot);

        public static void FixDirectoryStructure()
        {
            // create directory for next version app container;
            Directory.CreateDirectory(AppNextRootPath);

            // create directory for cache container;
            Directory.CreateDirectory(TmpRootPath);

            // create directory for app data container;
            Directory.CreateDirectory(DataRootPath);
            Directory.CreateDirectory(DeletedDataRootPath);
            Directory.CreateDirectory(DataRootPath + "/thumb");

            // create directory for app nas container;
            Directory.CreateDirectory(NasRootPath);
        }

        /* Write detailed info to stat file. This will write ONE `stats.txt` file to the default NAS drive directory
         * (which is 1 level above `AppConfig.NasRoot`). */
        public static void WriteStatFile()
        {
            if (!s_IsReady)
                Init();

            string fullPath = Path.GetFullPath(AppUtils.AppRoot + "/" + AppConfig.NasRoot + "/..");

            string fileContent = GetNasDetails() + "\r\n";

            try
            {
                File.WriteAllText(fullPath + "/stats.txt", fileContent, Encoding.UTF8);
            }
            catch (Exception)
            { }
        }

        public static string GetNasDetails()
        {
            if (!s_IsReady)
                Init();

            string content = "\r\n";

            content += "AppDrive\r\n";
            content += AppDriveUtils.GetFormattedDetailInfo();
            content += "\r\n";

            content += "NasDrive\r\n";
            content += NasDriveUtils.GetFormattedDetailInfo();

            return content;
        }


        public DriveInfo RawDriveInfo { get { return m_DriveInfo; } }

        public long TotalSize { get { return m_DriveInfo.TotalSize; } }
        public long FreeSpace { get { return m_DriveInfo.TotalFreeSpace; } }
        public long UsedSpace { get { return TotalSize - FreeSpace; } }

        public float UsedRatio { get { return (float)UsedSpace / TotalSize; } }
        public float FreeRatio { get { return (float)FreeSpace / TotalSize; } }

        public string GetFormattedDetailInfo()
        {
            const int barCount = 50;

            int filledBar = (int)(UsedRatio * barCount);
            //int emptyBar = barCount - filledBar;

            string bars = new string('#', filledBar);

            string detailed = string.Format(
                "[{0,-50}]\r\n" +
                "\r\n",
                bars);

            //detailed += "|   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |       |\r\n";
            //detailed += "Total:     999.99 GB      99.99%    2147147147147147\r\n\r\n";

            detailed += string.Format("Total:{0,14}        {1,24}\r\n",
                AppUtils.FormatDataSize(TotalSize), TotalSize);
            detailed += string.Format("  Used:{0,13}{1,11:#0.00}%{2,20}\r\n",
                AppUtils.FormatDataSize(UsedSpace), UsedRatio * 100.0f, UsedSpace);
            detailed += string.Format("  Free:{0,13}{1,11:#0.00}%{2,20}\r\n",
                AppUtils.FormatDataSize(FreeSpace), FreeRatio * 100.0f, FreeSpace);

            return detailed;
        }


        private DriveUtils(string _path)
        {
            string fullPath = Path.GetFullPath(AppUtils.AppRoot + "/" + _path);

            m_DriveInfo = new DriveInfo(fullPath);
        }

        private static bool s_IsReady = false;
        private readonly DriveInfo m_DriveInfo = null;
    }

}
