/*  Utils.cs
 *  Version: 1.11 (2022.10.19)
 *
 *  Contributor
 *      Arime-chan
 */

using Microsoft.AspNetCore.Hosting;
using Project24.Data;
using Project24.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project24
{
    public static class P24Roles
    {
        public const string Power = "Arime";
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Employee = "Employee";
        public const string User = "User";
        public const string NasUser = "NasUser";
        public const string NasTester = "NasTester";

        public const string Vie_Power = "Arime";
        public const string Vie_Admin = "Admin";
        public const string Vie_Manager = "Quản lý";
        public const string Vie_Employee = "Nhân viên";
        public const string Vie_User = "Người dùng";
        public const string Vie_NasUser = "Người dùng NAS";
        public const string Vie_NasTester = "Người test NAS";

        public static string Vie_GetLocalized(string _role)
        {
            if (m_Vie_Localized.ContainsKey(_role))
                return m_Vie_Localized[_role];

            return "";
        }

        public static ReadOnlyDictionary<string, string> Vie_GetAllLocalized()
        {
            return new ReadOnlyDictionary<string, string>(m_Vie_Localized);
        }

        public static List<string> GetAllRoles()
        {
            return new List<string>()
            {
                Power, Admin, Manager, Employee, User, NasTester
            };
        }

        private static readonly Dictionary<string, int> m_RoleLevel = new Dictionary<string, int>()
        {
            { Power, 0 },
            { Admin, 1 },
            { Manager, 2 },
            { Employee, 3 },
            { User, 999 },
            { NasTester, 4 },
        };

        private static readonly Dictionary<string, string> m_Vie_Localized = new Dictionary<string, string>()
        {
            { Power, Vie_Power },
            { Admin, Vie_Admin },
            { Manager, Vie_Manager },
            { Employee, Vie_Employee },
            { User, Vie_User },
            { NasTester, Vie_NasTester },
        };
    }

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

        /*  Project24 Directory Structure:
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
        public static string NasRoot { get; set; }
        public static string DataRoot { get; set; }



        //public const string NAS_ROOT = "./../../../wwwNas/";
        //public const string DATA_ROOT = "./../db/data/";

        public const string ROLE_POWER = "Arime";
        public const string ROLE_ADMIN = "Admin";
        public const string ROLE_MANAGER = "Manager";
        public const string ROLE_EMPLOYEE = "Employee";
        public const string ROLE_USER = "User";
        public const string ROLE_NAS_USER = "NasUser";

        public const string ROLE_POWER_LOCALIZED = "Arime-chan";
        public const string ROLE_ADMIN_LOCALIZED = "Admin";
        public const string ROLE_MANAGER_LOCALIZED = "Quản lý";
        public const string ROLE_EMPLOYEE_LOCALIZED = "Nhân viên";
        public const string ROLE_USER_LOCALIZED = "Người dùng";
        public const string ROLE_NAS_USER_LOCALIZED = "Nas User";

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

        public static string[] s_Roles = { ROLE_POWER, ROLE_ADMIN, ROLE_MANAGER, ROLE_EMPLOYEE, ROLE_USER };
        public static string[] s_RolesLocalized =
        {
            ROLE_POWER_LOCALIZED,
            ROLE_ADMIN_LOCALIZED,
            ROLE_MANAGER_LOCALIZED,
            ROLE_EMPLOYEE_LOCALIZED,
            ROLE_USER_LOCALIZED
        };
    }

    public static class FileSignatures
    {
        public static byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        public static byte[] Jpg = { 0xFF, 0xD8, 0xFF };
    }

    public static class Utils
    {
        
        /* This is equivalent to Directory.GetCurrentDirectory() */
        public static string AppRoot { get; set; } 

        public static string DataRoot { get; set; }
        public static string NasRoot { get; set; }
        public static string TmpRoot { get; set; }

        public static string CurrentVersion { get; set; }

        public static string FormatDataSize(long _size)
        {
            const long oneKiB = 1024L;
            const long oneMiB = 1024L * oneKiB;
            const long oneGiB = 1024L * oneMiB;
            const long oneTiB = 1024L * oneGiB;

            if (_size >= oneTiB)
            {
                return string.Format("{0:##0.00} TB", (float)_size / oneTiB);
            }

            if (_size >= oneGiB)
            {
                return string.Format("{0:##0.00} GB", (float)_size / oneGiB);
            }

            if (_size >= oneMiB)
            {
                return string.Format("{0:##0.00} MB", (float)_size / oneMiB);
            }

            if (_size >= oneKiB)
            {
                return string.Format("{0:##0.00} KB", (float)_size / oneKiB);
            }

            return string.Format("{0:##0.00}   B", (float)_size);
        }

        public static async Task UpdateCurrentVersion(IWebHostEnvironment _webHostEnv)
        {
            string webRootPath = _webHostEnv.WebRootPath;

            string markdown = await System.IO.File.ReadAllTextAsync(webRootPath + "/ReleaseNote.md", Encoding.UTF8);

            Regex regex = new Regex(@"\A(#+ v[0-9]+\.[0-9]+\.[0-9]+-*([a-z0-9])*)");

            var match = regex.Match(markdown);
            if (match.Success)
            {
                Utils.CurrentVersion = match.Value[4..]; // equivalent to .Substring(4);
            }
            else
            {
                Utils.CurrentVersion = "Unknown";
            }
        }


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

        public static string ConstructFullName(string _familyName, string _middleName, string _lastName)
        {
            if (string.IsNullOrEmpty(_middleName))
            {
                return _familyName + " " + _lastName;
            }

            return _familyName + " " + _middleName + " " + _lastName;
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

            return new Tuple<string, string, string>(tokens[0].Trim(), middlename, tokens[tokens.Length - 1].Trim());
        }


        public static async Task<int> RecordAction(
            string _username,
            string _operation,
            string _status,
            string _description = null)
        {
            ActionRecord record = new ActionRecord()
            {
                Timestamp = DateTime.Now,
                Username = _username,
                Operation = _operation,
                OperationStatus = _status,
                Description = _description,
            };
            ApplicationDbContext.Instance.Add(record);

            return await ApplicationDbContext.Instance.SaveChangesAsync();
        }

        public static async Task<int> RecordAction(
            ApplicationDbContext _db,
            string _username,
            string _operation,
            string _status,
            string _description = null)
        {
            ActionRecord record = new ActionRecord()
            {
                Timestamp = DateTime.Now,
                Username = _username,
                Operation = _operation,
                OperationStatus = _status,
                Description = _description,
            };
            _db.Add(record);

            return await _db.SaveChangesAsync();
        }
    }


}
