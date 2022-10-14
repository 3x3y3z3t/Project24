/*  DriveUtils.cs
 *  Version: 1.0 (2022.10.10)
 *
 *  Contributor
 *      Arime-chan
 */
using System;
using System.IO;
using System.Text;

namespace Project24.App
{
    public class NasDriveUtils
    {
        public static bool IsReady { get; private set; } = false;
        public static DriveInfo DriveInfo { get; private set; }

        public static long TotalSize { get { if (!IsReady) return -1L; return DriveInfo.TotalSize; } }
        public static long AvailableFreeSpace { get { if (!IsReady) return 0L; return DriveInfo.AvailableFreeSpace; } }
        public static long TotalFreeSpace { get { if (!IsReady) return 0L; return DriveInfo.TotalFreeSpace; } }


        public static void Init()
        {
            if (IsReady)
                return;

            string nasPath = Path.GetFullPath(Constants.WorkingDir + "/" + Constants.NasRoot + "/../");
            DriveInfo = new DriveInfo(nasPath);

            IsReady = true;
        }

        public static void WriteStatsFile(bool _force = true)
        {
            if (!IsReady)
                Init();

            string nasPath = Path.GetFullPath(Constants.WorkingDir + "/" + Constants.NasRoot);
            string statusFileFullname = nasPath + "/stats/txt";
            if (_force || !File.Exists(statusFileFullname))
            {
                string fileContent = GetStatsString();

                try
                {
                    File.WriteAllText(statusFileFullname, fileContent, Encoding.UTF8);
                }
                catch (Exception)
                { }
            }
        }

        public static string GetStatsString()
        {
            if (!IsReady)
                Init();

            const int barCount = 50;
            long used = DriveInfo.TotalSize - DriveInfo.TotalFreeSpace;
            float usedPercent = (float)used / DriveInfo.TotalSize;

            int filledBar = (int)(usedPercent * barCount);
            //int emptyBar = barCount - filledBar;

            string bars = new string('#', filledBar);

            string fileContent = string.Format(
                "Nas stats:\r\n" +
                "\r\n" +
                "[{0,-50}]\r\n" +
                "\r\n",
                bars);

            //fileContent += "|   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |   |       |\r\n";
            //fileContent += "Total:      999.9 GB      99.99%    2147147147147147\r\n\r\n";

            fileContent += string.Format("Total:{0,14}        {1,24}\r\n",
                Utils.FormatDataSize(DriveInfo.TotalSize), DriveInfo.TotalSize);
            fileContent += string.Format("  Used:{0,13}{1,11:#0.00}%{2,20}\r\n",
                Utils.FormatDataSize(DriveInfo.TotalSize), usedPercent * 100.0f, used);
            fileContent += string.Format("  Free:{0,13}{1,11:#0.00}%{2,20}\r\n",
                Utils.FormatDataSize(DriveInfo.TotalSize), DriveInfo.TotalFreeSpace * 100.0f / DriveInfo.TotalSize, DriveInfo.TotalFreeSpace);

            return fileContent;
        }
    }

}

//private void WriteDriveStatsFile(bool _force = true)
//{
//    string statusFileFullname = Path.GetFullPath(Constants.NasRoot, Constants.WorkingDir) + "/stats.txt";

//    if (_force || !System.IO.File.Exists(statusFileFullname))
//    {

//        string fileContent = "";

//        DriveInfo[] dis = DriveInfo.GetDrives();
//        foreach (var di in dis)
//        {
//            if (di.RootDirectory.ToString() == dirRoot)
//            {
//                fileContent += "NAS stats:\r\n";

//                fileContent += "Total:                  " + di.TotalSize + "\r\n";
//                fileContent += "    Used:               " + (di.TotalSize - di.AvailableFreeSpace) + "\r\n";
//                fileContent += "    Free:               " + di.AvailableFreeSpace + "\r\n";

//                break;
//            }
//        }

//        try
//        {
//            System.IO.File.WriteAllText(statusFileFullname, fileContent, System.Text.Encoding.UTF8);
//        }
//        catch (Exception _e)
//        { }
//    }
//}
