/*  App/Enums.cs
 *  Version: v1.4 (2023.10.07)
 *  
 *  Author
 *      Arime-chan
 */

namespace Project24.App
{
    public enum ViewDataFlags
    {
        None = 0,
        NoTitle = 1 << 0,
        NoAnnouncement = 1 << 1,
    }

    public enum UpdaterStatus : short
    {
        None = 0,

        PrevPurgeQueued = -1,
        PrevPurgeRunning = -2,
        PrevApplyQueued = -3,
        PrevApplyRunning = -4,

        NextPurgeQueued = 1,
        NextPurgeRunning = 2,
        NextApplyQueued = 3,
        NextApplyRunning = 4,
    }

    public enum UpdaterInternalState : short
    {
        None = 0,

        Step1_Countdown,
        Step2_DeleteFiles,
        Step3_BackupFiles,
        Step4_SwitchExecutableToTmp,
        Step5_CopyNewFiles,
        Step6_SwitchExecutableBackToMain,
    }

    public enum UpdaterQueuedAction : short
    {
        None = 0,

        Countdown,

        DeleteFilesInMain,
        DeleteFilesInPrev,
        DeleteFilesInNext,

        CopyFilesFromMainToPrev,
        CopyFilesFromPrevToMain,
        CopyFilesFromNextToMain,

        SwitchExecutableToPrev,
        SwitchExecutableToMain
    }

    public enum UpdaterSide : short
    {
        Prev = -1,
        Next = 1
    }

    public enum ErrorFlagBit : short
    {
        Error = 0,
        NoError = 1
    }


    

    public static class AppSide_
    {
        public const string MAIN = "main";
        public const string PREV = "prev";
        public const string NEXT = "next";
    }

    public static class MessageTag
    {
        public const int TagLength = 6;

        public const string Info = "<info>";
        public const string Success = "<done>";
        public const string Warning = "<warn>";
        public const string Error = "<fail>";

        public const string Exception = "<excp>";
    }

    public static class P24PageLayout
    {
        public const string CommonLayout = "_LayoutCommon";
        public const string NoScrollLayout = "_NoScrollLayout";
    }


}
