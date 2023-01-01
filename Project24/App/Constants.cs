/*  Constants.cs
 *  Version: 1.10 (2023.01.02)
 *
 *  Contributor
 *      Arime-chan
 */

using System;

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

    public static class Constants
    {
        public const string DefaultPassword = "123@123a";

    }

    public static class AppConfig
    {

        public const string AppNextRoot = "/../../p24_next";
        public const string TmpRoot = "/../../../nasTmp";

        public const string DataRoot = "/../../../../wwwNas/appData/p24/data";
        public const string NasRoot = "/../../../../wwwNas/nasData";

        public const long MaxLogFileSize = 16L * 1024L * 1024L;

        public const int ThisYear = 2022;

        public const string CustomerCodeFormatString = "BN{0:yy}{0:MM}{0:dd}{1:00}";
        public const string TicketCodeFormatString = "PK{0:yy}{0:MM}{0:dd}{1:00}";
    }

    public static class ErrorMessage
    {
        public const string FirstNameTooLong = "First name is too long";
        public const string LastNameTooLong = "Last name is too long";

        public const string InvalidModelState = "Invalid ModelState";
        public const string CurrentUserIsNull = "currentUser is null";

        public const string CustomerNotFound = "Customer not found";
        public const string CustomerDeleted = "Customer already deleted";
        
        public const string TicketDeleted = "Ticket already deleted";
        
        public const string ImageNotFound = "Image not found";

    }

    /// <summary> This class define error message constants for Clinic Manager module, or Project24 (P24) side.</summary>
    public static class P24Message
    {
        public const string DoBMustBeInRange = "Năm sinh phải nằm trong phạm vi {1} - {2}.";
        public const string DoBCannotBeEmpty = "Năm sinh không được để trống.";

        public const string NameCannotBeEmpty = "Họ Tên không được để trống.";
        public const string GenderCannotBeEmpty = "Giới tính không được để trống.";

        public const string PhoneNumberCannotBeEmpty = "Số điện thoại không được để trống.";
        public const string PhoneNumberExisted = "Số điện thoại đã được đăng ký cho bệnh nhân khác ({0}).";

        public const string DiagnoseCannotBeEmpty = "Thông tin chẩn đoán không được để trống.";
        public const string TreatmentCannotBeEmpty = "Thông tin xử trí không được để trống.";

        public const string PasswordCannotBeEmpty = "Mật khẩu không được để trống.";

    }

    public static class P24Constants
    {
        public const string Customer = "bệnh nhân";
        public const string Ticket = "phiếu khám bệnh";

        public const string GenderMale = "Nam";
        public const string GenderFemale = "Nữ";

        public const string No = "Không";
        public const string Yes = "Có";
        public const string Restricted = "Giới hạn";

        public static DateTime MinDate = new DateTime(2000, 01, 01);
    }

    public static class ServerAnnouncementMessage
    {
        public const string Maintenance = "Server will be down in {0} minutes for maintenance.";
        public const string Maintenance_Vi = "Hệ thống sẽ tạm ngưng để bảo trì sau {0} phút nữa.";
    }

    public static class CustomInfoKey
    {
        public const string UseFullWidth = "UseFullWidth";

        public const string Error = "error";
        public const string Added = "added";
        public const string Invalid = "invalid";
        public const string Message = "msg";
        public const string Path = "path";
        public const string Filename = "fileName";
        public const string Size = "size";

        public const string SuccessCount = "successCount";
        public const string ErrorCount = "errorCount";

        public const string HasNewCustomer = "newCustomer";
        public const string CustomerCode = "customerCode";
        public const string TicketCode = "ticketCode";
        public const string ImageId = "imgId";

        public const string AddedList = "added";
        public const string DeletedList = "deleted";
        public const string Malfunctions = "malfunctions";

        public const string ParentDir = "dir";
        public const string FolderName = "name";

        public const string MoveMode = "move";

        public const string DisableUserInfo = "DisableUserInfo";

        public const string Language = "lang";

        public const string Lang_Vi_VN = "vi-VN";
        public const string Lang_En_US = "en-US";
    }

    public static class CustomInfoTag
    {
        public const int TagLength = 6;

        public const string Success = "<done>";
        public const string Info = "<info>";
        public const string Warning = "<warn>";
        public const string Error = "<fail>";
    }

    public static class P24RoleName
    {
        public const string Power = "Arime";
        public const string Admin = "Admin";

        public const string Manager = "Manager";

        public const string NasUser = "NasUser";
        public const string NasTester = "NasTester";
        public const string NasUploader = "NasUploader";

        public static string[] GetAllRoleNames()
        {
            return new string[]
            {
                Power, Admin,
                Manager,
                NasUser, NasTester, NasUploader
            };
        }

        public static string GetLocalized_Vi_VN(string _role)
        {
            switch (_role)
            {
                case Manager:
                    return "Quản lý";
                case NasUser:
                    return "Người dùng NAS";
            }

            return _role;
        }
    }





    //public enum P24Module
    //{
    //    Customer,
    //    Ticket
    //}

}
