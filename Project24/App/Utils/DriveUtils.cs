/*  DriveUtils.cs
 *  Version: 1.3 (2022.10.21)
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

        public static void FixDirectoryStructure()
        {
            // create directory for next version app container;
            string fullPath = Path.GetFullPath(Project24.Utils.AppRoot + "/" + AppConfig.AppNextRoot);
            Directory.CreateDirectory(fullPath);

            // create directory for cache container;
            fullPath = Path.GetFullPath(Project24.Utils.AppRoot + "/" + AppConfig.TmpRoot);
            Directory.CreateDirectory(fullPath);

            // create directory for app data container;
            fullPath = Path.GetFullPath(Project24.Utils.AppRoot + "/" + AppConfig.DataRoot);
            Directory.CreateDirectory(fullPath);
            fullPath = Path.GetFullPath(Project24.Utils.AppRoot + "/" + AppConfig.DataRoot + "/../deletedData");
            Directory.CreateDirectory(fullPath);

            // create directory for app nas container;
            fullPath = Path.GetFullPath(Project24.Utils.AppRoot + "/" + AppConfig.NasRoot);
            Directory.CreateDirectory(fullPath);
        }

        /* Write detailed info to stat file. This will write ONE `stats.txt` file to the default NAS drive directory
         * (which is 1 level above `AppConfig.NasRoot`). */
        public static void WriteStatFile()
        {
            if (!s_IsReady)
                Init();

            string fullPath = Path.GetFullPath(Project24.Utils.AppRoot + "/" + AppConfig.NasRoot + "/..");

            string fileContent = "\r\n";

            fileContent += "AppDrive\r\n";
            fileContent += AppDriveUtils.GetFormattedDetailInfo();
            fileContent += "\r\n";

            fileContent += "NasDrive\r\n";
            fileContent += NasDriveUtils.GetFormattedDetailInfo();
            fileContent += "\r\n";

            try
            {
                File.WriteAllText(fullPath + "/stats.txt", fileContent, Encoding.UTF8);
            }
            catch (Exception)
            { }
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
                Project24.Utils.FormatDataSize(TotalSize), TotalSize);
            detailed += string.Format("  Used:{0,13}{1,11:#0.00}%{2,20}\r\n",
                Project24.Utils.FormatDataSize(UsedSpace), UsedRatio * 100.0f, UsedSpace);
            detailed += string.Format("  Free:{0,13}{1,11:#0.00}%{2,20}\r\n",
                Project24.Utils.FormatDataSize(FreeSpace), FreeRatio * 100.0f, FreeSpace);

            return detailed;
        }


        private DriveUtils(string _path)
        {
            string fullPath = Path.GetFullPath(Project24.Utils.AppRoot + "/" + _path);

            m_DriveInfo = new DriveInfo(fullPath);
        }

        private static bool s_IsReady = false;
        private DriveInfo m_DriveInfo = null;
    }

}
