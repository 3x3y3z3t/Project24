/*  App/Constants.cs
 *  Version: v1.0 (2023.08.24)
 *  
 *  Contributor
 *      Arime-chan
 */

using Project24.App;

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
    }

    public static class ErrCode
    {
        public const string NotImplemented = "<code>NOT_IMPLEMENTED</code>";
        public const string InvalidBlockName = "<code>INVALID_BLOCK_NAME</code>";

        public const string Updater_BatchOversize = "<code>UPDATER_BATCH_OVERSIZE</code>";
        public const string Updater_BatchCountMismatch = "<code>UPDATER_BATCH_COUNT_MÍMATCH</code>";
        public const string Updater_BatchSizeMismatch = "<code>UPDATER_BATCH_SIZE_MISMATCH</code>";

    }

    public static class DbInternalStateKey
    {
        public const string UPDATER_STATUS = nameof(UPDATER_STATUS);
        public const string UPDATER_DUE_TIME = nameof(UPDATER_DUE_TIME);
    }
}
