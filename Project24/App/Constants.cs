/*  Constants.cs
 *  Version: 1.0 (2022.10.20)
 *
 *  Contributor
 *      Arime-chan
 */

namespace Project24
{
    /*  Project24 Directory Structure:
     * 
     *  root (/home/pi)
     *  |---wwwApp
     *  |   |---appData
     *  |   |   |---p24
     *  |   |   |   |---publish         <- Utils.AppRoot        (contains p24 executable(./Project24))
     *  |   |   |   |---db                                      (contains database binary files)
     *  |   |   |---p24_next            <- AppNextRoot          (contains next version of p24 app)
     *  |   |---nasTmp                  <- TmpRoot              (contains temprary files to copy to nasData)
     *  |       |---...
     *  |---wwwNas     
     *      |---appData
     *      |   |---p24
     *      |       |---data            <- DataRoot             (contains Project24 data)
     *      |       |---deletedData                             (contains deleted data)
     *      |---nasData                 <- NasRoot              (contains nas data)
     *
     */
    public static class AppConfig
    {

        public const string AppNextRoot = "/../../p24_next";
        public const string TmpRoot = "/../../../nasTmp";

        public const string DataRoot = "/../../../../wwwNas/appData/p24/data";
        public const string NasRoot = "/../../../../wwwNas/nasData";
    }

    public static class ErrorMessage
    {
        public const string FirstNameTooLong = "First name is too long";
        public const string LastNameTooLong = "Last name is too long";

        public const string InvalidModelState = "Invalid Model state";
        public const string CustomerNotFound = "Customer not found";
        public const string ImageNotFound = "Image not found";

        public const string CurrentUserIsNull = "currentUser is null";

    }

    public static class CustomInfoKey
    {
        public const string Error = "err";
        public const string Message = "msg";
        public const string Path = "path";

        public const string CustomerCode = "customerCode";
        public const string ImageId = "imgId";

        public const string AddedList = "added";
        public const string Malfunctions = "malfunctions";
    }

    public static class P24RoleName
    {
        public const string Power = "Arime";
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string NasUser = "NasUser";
        public const string NasTester = "NasTester";

        public static string[] GetAllRoleNames()
        {
            return new string[]
            {
                Power, Admin,
                Manager,
                NasUser, NasTester
            };
        }
    }







}
