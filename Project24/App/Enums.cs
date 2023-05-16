/*  App/Enums.cs
 *  Version: v1.1 (2023.05.16)
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

    public static class MessageTag
    {
        public const string Success = "<done>";
        public const string Warning = "<warn>";
        public const string Error = "<fail>";

        public const string Exception = "<excp>";
    }

}
