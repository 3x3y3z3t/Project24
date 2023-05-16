/*  App/Enums.cs
 *  Version: v1.0 (2023.05.11)
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
        public static string Success = "<done>";
        public static string Warning = "<warn>";
        public static string Error = "<fail>";
    }

}
