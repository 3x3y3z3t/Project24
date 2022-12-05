/*  ActionLog.cs
 *  Version: 1.9 (2022.12.04)
 *
 *  Contributor
 *      Arime-chan
 */
using System;
using System.ComponentModel.DataAnnotations;

namespace Project24.Models
{
    public class ActionRecord
    {
        public static class Operation_
        {
            public const string AttemptLogin = "Attempt Login";

            public const string AttendanceRequest = "Attendance Request";

            public const string CreateUser = "Create User";
            public const string DeleteUser = "Delete User";
            public const string UpdateUser = "Update User";
            public const string DetailUser = "Detail User";

            //public const string CreateService = "Create Service";
            //public const string DeleteService = "Delete Service";
            //public const string UpdateService = "Update Service";
            //public const string DetailService = "Detail Service";

            public const string CreateCustomer = "Create Customer";
            public const string CreateCustomer_CreateImage = "Create Customer Image";
            public const string DeleteCustomer = "Delete Customer";
            public const string DeleteCustomer_DeleteImage = "Delete Customer Image";
            public const string UpdateCustomer = "Update Customer";
            public const string DetailCustomer = "Detail Customer";
            //public const string DetailCustomer_AddImage = "Detail Customer: Add Image";
            //public const string DetailCustomer_DelImage = "Detail Customer: Delete Image";

            public const string CreateTicket = "Create Visiting Ticket";
            public const string CreateTicket_CreateImage = "Create Ticket Image";
            public const string DeleteTicket = "Delete Visiting Ticket";
            public const string CreateTicket_DeleteImage = "Delete Ticket Image";
            public const string UpdateTicket = "Update Visiting Ticket";
            public const string DetailTicket = "Detail Visiting Ticket";

            //public const string CreateVisitingProfile = "Create Visiting Profile";
            //public const string RemoveVisitingProfile = "Delete Visiting Profile";
            //public const string UpdateVisitingProfile = "Update Visiting Profile";
            //public const string DetailVisitingProfile = "Detail Visiting Profile";

            public const string CreateNasFolder = "Create NAS Folder";
            public const string DeleteNasFile = "Delete NAS File";
            public const string CopyNasFile = "Copy NAS File";


            public const string UploadNasFile = "UploadNasFile";


            public const string Updater_UploadNextFiles = "Upload Next version files";
            public const string Updater_PurgeNextFiles = "Purge Next version files";
            public const string Updater_VersionUp = "Version Up";

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

        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.DateTime)]
        public DateTime Timestamp { get; set; }

        public string Username { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Operation { get; set; }
        
        [Required(AllowEmptyStrings = false)]
        public string OperationStatus { get; set; }
        
        public string CustomInfo { get; set; }

        public ActionRecord()
        { }
    }

}
