/*  Model/UserAction.cs
 *  Version: v1.1 (2023.11.19)
 *  
 *  Author
 *      Arime-chan
 */

using System.ComponentModel.DataAnnotations;
using System;
using Microsoft.EntityFrameworkCore;

namespace Project24.Model
{
    [Index(nameof(Username))]
    [Index(nameof(Operation))]
    [Index(nameof(OperationStatus))]
    public class UserAction
    {
        public static class Operation_
        {
            public const string Account_AttemptPasswordLogin = "Account Attempt Password Login";
            public const string Account_ChangePassword = "Account Change Password";

            public const string AttendanceRequest = "Attendance Request";

            public const string Updater_UploadNextFiles = "Upload Next version files";
            public const string Updater_InitVersionUp = "Init Version Up";
            public const string Updater_AbortVersionUp = "Abort Version Up";
            public const string Updater_UpdateStaticFiles = "Update Static Files";
            public const string Updater_PurgeNextFiles = "Purge Next version files";

            public const string Home_Account_Manage_UpdateRole = nameof(Home_Account_Manage_UpdateRole);
        }

        public static class OperationStatus_
        {
            public const string UnexpectedError = "Unexpected Error";

            public const string Success = "Success";
            public const string Failed = "Failed";
            public const string Denied = "Denied";

            public const string HasUpload = "Has Upload";
            public const string HasMalfunction = "Has Malfunction";
        }


        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        [StringLength(256)]
        public string Username { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(255)]
        public string Operation { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(255)]
        public string OperationStatus { get; set; }

        public string CustomInfo { get; set; }


        public UserAction()
        { }
    }

}
