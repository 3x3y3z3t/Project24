/*  App/Constants.cs
 *  Version: v1.4 (2023.10.07)
 *  
 *  Author
 *      Arime-chan
 */

using System;

namespace Project24.App
{
    /*  Project24 Directory Structure:
     * 
     *  root (/home/pi)
     *  |---wwwApp
     *  |   |---appBin
     *  |   |   |---curr            <- AppRoot          (contains p24 executable(./Project24))
     *  |   |   |---next            <- AppNext          (contains next version of p24 app)
     *  |   |   |---prev            <- AppPrev          (contains previous version of p24 app)
     *  |   |   |---log
     *----------------------------------------------------------below is external drive
     *  |   |---appData
     *  |   |   |---data            <- DataRoot         (contains Project24 data)
     *  |   |   |---db              <- DbRoot           (contains database binary files)
     *  |   |   |---deleted         <- DeletedRoot      (contains deleted data)
     *  |   |   |---nasTmp          <- NasTmpRoot       (contains temprary files to copy to nasData)
     *  |---wwwNas
     *  |   |---nasRoot             <- NasRoot          (contains nas data)
     */

    public static class Constants
    {
        public const string AppMainDir = "/main";
        public const string AppNextDir = "/next";
        public const string AppPrevDir = "/prev";
        public const string AppLogDir = "/log";

        public const string DataRootDir = "../../appData/data";
        public const string DbRootDir = "../../appData/db";
        public const string DeletedRootDir = "../../appData/deleted";
        public const string NasTmpRootDir = "../../appData/nasTmp";

        public const string NasRootDir = "../../../wwwNas/nasRoot";

        public const int MaxRequestSize = 32 * 1024 * 1024;
        public const long MaxLogFileSize = 50 * 1024 * 1024;

        public static DateTime Epoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime AppReleaseDate = new DateTime(2022, 8, 31, 2, 18, 37, 135);
    }

    public static class ErrCode
    {
        public const string NotImplemented = "NOT_IMPLEMENTED";
        public const string InvalidBlockName = "INVALID_BLOCK_NAME";

        public const string Updater_BatchOversize = "UPDATER_BATCH_OVERSIZE";
        public const string Updater_BatchCountMismatch = "UPDATER_BATCH_COUNT_MÍMATCH";
        public const string Updater_BatchSizeMismatch = "UPDATER_BATCH_SIZE_MISMATCH";

    }

    public static class Message
    {
        public const string InvalidModelState = "Invalid ModelState";
    }

    public static class DbInternalStateKey
    {
        public const string UPDATER_STATUS = nameof(UPDATER_STATUS);
        public const string UPDATER_DUE_TIME = nameof(UPDATER_DUE_TIME);
    }

    public enum ExitCodes
    {
        Ok = 0,

        InvalidAppSide,

        PowerUserNoPassword,
    }
}
