/*  App/Enums.cs
 *  Version: v1.2 (2023.06.28)
 *  
 *  Contributor
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

    public enum UpdaterSide : short
    {
        Prev = -1,
        Next = 1
    }

    public static class MessageTag
    {
        public const string Success = "<done>";
        public const string Warning = "<warn>";
        public const string Error = "<fail>";

        public const string Exception = "<excp>";
    }

}
