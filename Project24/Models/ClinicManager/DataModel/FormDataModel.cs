/*  FormDataModel.cs
 *  Version: 1.3 (2022.12.13)
 *
 *  Contributor
 *      Arime-chan
 */

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Project24.Models.ClinicManager.DataModel
{
    #region Customer
    public class P24CreateCustomerFormDataModel
    {
        [Required]
        public string Code { get; set; }

        [Required(ErrorMessage = P24Message.NameCannotBeEmpty)]
        public string FullName { get; set; }

        [Required(ErrorMessage = P24Message.GenderCannotBeEmpty)]
        public char Gender { get; set; }

        [Required(ErrorMessage = P24Message.DoBCannotBeEmpty)]
        [Range(1900, AppConfig.ThisYear, ErrorMessage = P24Message.DoBMustBeInRange)]
        public int DateOfBirth { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public P24CreateCustomerFormDataModel()
        { }
    }

    public class P24CreateCustomerFormDataModelEx : P24CreateCustomerFormDataModel
    {
        [DataType(DataType.Upload)]
        public IFormFile[] UploadedFiles { get; set; }

        //[Required(ErrorMessage = Constants.ERROR_MANAGER_PASSWORD_REQUIRED)]
        //[DataType(DataType.Password)]
        //public string ManagerPassword { get; set; }

        public P24CreateCustomerFormDataModelEx()
            : base()
        { }
    }

    public class P24EditCustomerFormDataModel
    {
        [Required]
        public string Code { get; set; }

        [Required(ErrorMessage = P24Message.NameCannotBeEmpty)]
        public string Fullname { get; set; }

        [Required(ErrorMessage = P24Message.GenderCannotBeEmpty)]
        public char Gender { get; set; }

        [Required(ErrorMessage = P24Message.DoBCannotBeEmpty)]
        [Range(1900, AppConfig.ThisYear, ErrorMessage = P24Message.DoBMustBeInRange)]
        public int DoB { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public P24EditCustomerFormDataModel()
        { }
    }
    #endregion

    #region Ticket
    public class P24CreateTicketFormDataModel
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public string Diagnose { get; set; }

        [Required]
        public string Treatment { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile[] UploadedFiles { get; set; }

        [Required]
        public P24CreateCustomerFormDataModel CustomerFormData { get; set; }

        public P24CreateTicketFormDataModel()
        { }
    }

    public class P24EditTicketFormDataModel
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public string Diagnose { get; set; }

        [Required]
        public string Treatment { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public P24EditTicketFormDataModel()
        { }
    }
    #endregion

    #region Image
    public class P24CreateImageFormDataModel
    {
        [Required]
        public string OwnerCode { get; set; }

        [Required]
        [DataType(DataType.Upload)]
        public IFormFile[] UploadedFiles { get; set; }

        public P24CreateImageFormDataModel()
        { }
    }

    public class P24DeleteImageFormDataModel
    {
        [Required]
        public string ImageId { get; set; }

        [Required]
        public string OwnerCode { get; set; }

        public P24DeleteImageFormDataModel()
        { }
    }
    #endregion

}
