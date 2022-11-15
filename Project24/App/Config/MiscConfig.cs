/*  MiscUtils.cs
 *  Version: 1.0 (2022.10.15)
 *
 *  Contributor
 *      Arime-chan
 */

namespace Project24.App
{
    public static partial class DefaultUsers
    {
        public class UserCredential
        {
            public string Username = null;
            public string Password = null;
        }

        public static UserCredential PowerUser { get; private set; }
        public static UserCredential ArimeUser { get; private set; }
        public static UserCredential DefaultClinicManager { get; private set; }

    }

}
