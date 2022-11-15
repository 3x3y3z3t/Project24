/*  Utils.cs
 *  Version: 1.15 (2022.11.13)
 *
 *  Contributor
 *      Arime-chan
 */

using System;

namespace Project24
{
    public enum P24Module
    {
        Home = 0,
        Home_Navigate,

        ClinicManager,

        Nas,
    }

    public class Constants
    {


        public static string WorkingDir { get; private set; } = System.IO.Directory.GetCurrentDirectory();


        /*  Project24 Directory Structure (no longer valid):
         *
         *  root_dir
         *  |---wwwApp
         *  |   |---Project24
         *  |       |---publish                 <- (contains executable)
         *  |       |---db
         *  |           |---data                <- (contains Clinic Manager related data, mostly image)
         *  |           |---[MySQL db files]    <- (db)
         *  |---wwwNas                          <- (contains NAS files)
         *  |---wwwTmp
         *  |   |---p24-next                    <- (next version of the app, contains the 'publish' directory)
         */



        //public const string NAS_ROOT = "./../../../wwwNas/";
        //public const string DATA_ROOT = "./../db/data/";


        public const string DEFAULT_PASSWORD = "123@123a";

        public const string BTN_UPDATE = "Cập nhật";
        public const string BTN_RETURN = "Trở lại";


        public const string ERROR_UNKNOWN_ERROR = "Lỗi không xác định.";
        public const string ERROR_UNKNOWN_ERROR_FROM_PROGRAM = "Lỗi không xác định.";
        public const string ERROR_MODEL_STATE_INVALID = "Lỗi Invalid Model State.";

        public const string ERROR_LOGIN_REQUIRED = "Bạn cần đăng nhập để thực hiện chức năng này.";

        public const string ERROR_EMPTY_NAME = "Tên không được để trống.";
        public const string ERROR_EMPTY_FULLNAME = "Họ và Tên không được để trống.";
        public const string ERROR_EMPTY_DATETIME = "Ngày tháng không được để trống.";
        public const string ERROR_EMPTY_CUSTOMER_ID = "Mã khách hàng không được để trống.";

        public const string ERROR_EMPTY_VISITING_ID = "Mã phiếu khám bệnh không được để trống.";

        public const string ERROR_LONG_NAME = "Tên không được dài quá {1} ký tự.";

        public const string ERROR_INVALID_PHONENUMBER = "Số điện thoại không hợp lệ.";
        public const string ERROR_INVALID_PRICE = "Số tiền không hợp lệ.";

        public const string ERROR_NOT_FOUND_SERVICE = "Không tìm thấy dịch vụ này.";
        public const string ERROR_NOT_FOUND_CUSTOMER = "Không tìm thấy bệnh nhân này.";
        public const string ERROR_NOT_FOUND_IMG_ID = "Không tìm thấy ảnh này.";


        public const string ERROR_NOT_ENOUGH_PRIVILEGE = "Error: Bạn không có đủ quyền hạn để thực hiện chức năng này.";
        public const string ERROR_MANAGER_PASSWORD_INCORRECT = "Mật khẩu người quản lý không đúng.";
        public const string ERROR_MANAGER_PASSWORD_REQUIRED = "Bạn phải nhập lại mật khẩu.";

        public const string INFO_USER_CREATED = "Người dùng mới đã được đăng ký.";

        public const string ERROR_EMPTY_USERNAME = "Tên tài khoản không được để trống.";
        public const string ERROR_EMPTY_PASWORD = "Mật khẩu không được để trống.";

    }

    public static class FileSignatures
    {
        public static byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        public static byte[] Jpg = { 0xFF, 0xD8, 0xFF };
    }

    public static class Utils
    {



        /* Summary:
         *  Construct user's Username based on the following template:
         *      Last Name + First letter of Family Name + First letter of every Middle Name.
         */
        public static string ConstructUsernameNoCount(string _familyName, string _middleName, string _lastName)
        {
            string username = _lastName;
            if (!string.IsNullOrEmpty(_familyName))
            {
                username += char.ToUpper(_familyName[0]);
            }
            if (!string.IsNullOrEmpty(_middleName))
            {
                string[] tokens = _middleName.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    username += char.ToUpper(token[0]);
                }
            }

            return username;
        }

        public static Tuple<string, string, string> TokenizeName(string _fullName)
        {
            if (string.IsNullOrEmpty(_fullName))
                return new Tuple<string, string, string>("", "", "");

            string[] tokens = _fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 1)
                return new Tuple<string, string, string>("", "", tokens[0].Trim());

            if (tokens.Length == 2)
                return new Tuple<string, string, string>(tokens[0].Trim(), "", tokens[1]);

            string middlename = "";
            for (int i = 1; i < tokens.Length - 1; ++i)
            {
                middlename += tokens[i].Trim();
            }

            return new Tuple<string, string, string>(tokens[0].Trim(), middlename, tokens[^1].Trim());
        }
    }


}
