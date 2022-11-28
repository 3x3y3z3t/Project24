/*  Utils.cs
 *  Version: 1.16 (2022.11.20)
 *
 *  Contributor
 *      Arime-chan
 */

using System;

namespace Project24
{
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

}
