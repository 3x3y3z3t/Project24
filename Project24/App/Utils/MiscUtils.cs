/*  App/Utils/MiscUtils.cs
 *  Version: v1.0 (2023.05.11)
 *  
 *  Contributor
 *      Arime-chan
 */

namespace Project24.App
{
    public static class Utils
    {
        public static string WebRootPath = null;

        public static bool IsFlagSet(int _flags, int _flag) => (_flags & _flag) != 0;

    }

}
