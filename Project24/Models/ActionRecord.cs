/*  ActionLog.cs
 *  Version: 1.18 (2023.02.14)
 *
 *  Contributor
 *      Arime-chan
 */
using System;
using System.ComponentModel.DataAnnotations;

namespace Project24.Models
{
    public sealed class ActionRecord
    {
        public static class Operation_
        {
            public const string Account_AttemptLogin = "Attempt Login";
            public const string Account_ChangePassword = "Account Change Password";

            public const string AttendanceRequest = "Attendance Request";

            public const string CreateUser = "Create User";
            public const string DeleteUser = "Delete User";
            public const string UpdateUser = "Update User";
            public const string DetailUser = "Detail User";

            public const string ImageManager_RenameCustomerImage = "Rename Customer Image";
            public const string ImageManager_RenameTicketImage = "Rename Ticket Image";
            public const string ImageManager_DeleteCustomerImage = "Delete Customer Image";
            public const string ImageManager_DeleteTicketImage = "Delete Ticket Image";

            public const string CreateCustomer = "Create Customer";
            public const string CreateCustomer_CreateImage = "Create Customer Image";
            public const string DeleteCustomer = "Delete Customer";
            public const string DeleteCustomer_DeleteImage = "Delete Customer Image";
            public const string UpdateCustomer = "Update Customer";
            public const string DetailCustomer = "Detail Customer";

            public const string CreateTicket = "Create Visiting Ticket";
            public const string CreateTicket_CreateImage = "Create Ticket Image";
            public const string DeleteTicket = "Delete Visiting Ticket";
            public const string DeleteTicket_DeleteImage = "Delete Ticket Image";
            public const string UpdateTicket = "Update Visiting Ticket";
            public const string DetailTicket = "Detail Visiting Ticket";

            public const string HideUnhideDrug = "Hide/Unhide Drug";
            public const string ValidateDrugStorage = "Verify Drug Storage";
            public const string InventoryImportCreate = "Inventory Import Create";
            public const string InventoryImportDelete = "Inventory Import Delete";
            public const string InventoryImportBatchDelete = "Inventory Import Batch Delete";
            public const string InventoryExportCreate = "Inventory Export Create";
            public const string InventoryExportDelete = "Inventory Export Delete";
            public const string InventoryExportBatchDelete = "Inventory Export Batch Delete";

            public const string CreateNasFolder = "Create NAS Folder";
            public const string DeleteNasFile = "Delete NAS File";
            public const string CopyNasFile = "Copy NAS File";


            public const string UploadNasFile = "UploadNasFile";


            public const string Updater_UploadNextFiles = "Upload Next version files";
            public const string Updater_InitVersionUp = "Init Version Up";
            public const string Updater_AbortVersionUp = "Abort Version Up";
            public const string Updater_UpdateStaticFiles = "Update Static Files";
            public const string Updater_PurgeNextFiles = "Purge Next version files";

            public const string Nas_CreateFolder = "Nas Create Folder";
            public const string Nas_RenameFile = "Nas Rename File";
            public const string Nas_CopyTo = "Nas Copy To";
            public const string Nas_MoveTo = "Nas Move To";
            public const string Nas_DeleteFile = "Nas Delete File";

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
        public int Id { get; private set; }

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
