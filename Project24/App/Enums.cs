/*  App/Enums.cs
 *  Version: v1.5 (2023.12.24)
 *  Spec:    v0.1
 *  
 *  Contributor
 *      Arime-chan (Author)
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

        /// <summary>Don't use this tag.</summary>
        public const string Info = "<info>";

        /// <summary>Attach this tag when the operation was a success.</summary>
        public const string Success = "<done>";

        /// <summary>
        /// Attach this tag when the operation could have caused data corruption, or could
        /// have lead to fauty states of main program. You can accept the data or reject it.
        /// </summary>
        public const string Warning = "<warn>";

        /// <summary>
        /// Attach this tag when the operation would have caused an error (of any kind),
        /// but the data have been checked and the operation have been rejected properly.<br />
        /// Use <see cref="Critical"/> if the error would cause main program to crash
        /// (e.g error not caught by ExceptionHandler middleware).<br />
        /// Use <see cref="Exception"/> instead if the error was an exception and was caught
        /// in a <c>try..catch</c> block.
        /// </summary>
        public const string Error = "<fail>";

        /// <summary>
        /// Attach this tag when the operation would have caused main program to crash,
        /// but the data have been check and the operation have been reject properly.<br />
        /// Use <see cref="Error"/> if the error would not cause main program to crash
        /// (e.g caught by ExceptionHandler middleware).<br />
        /// Use <see cref="Exception"/> instead if the error was an exception and was caught
        /// in a <c>try..catch</c> block.
        /// </summary>
        public const string Critical = "<crit>";

        /// <summary>
        /// Attach this tag when the operation caused an exception and it have been caught.
        /// </summary>
        public const string Exception = "<excp>";
    }

    public static class P24PageLayout
    {
        public const string CommonLayout = "_LayoutCommon";
        public const string NoScrollLayout = "_NoScrollLayout";
    }


}
