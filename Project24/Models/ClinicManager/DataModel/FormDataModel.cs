/*  FormDataModel.cs
 *  Version: 1.4 (2023.01.02)
 *
 *  Contributor
 *      Arime-chan
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Project24.Data;

namespace Project24.Models.ClinicManager.DataModel
{
    #region Customer
    public class P24CreateCustomerFormDataModel : IValidatableObject
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

        [Required(ErrorMessage = P24Message.PhoneNumberCannotBeEmpty)]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public P24CreateCustomerFormDataModel()
        { }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext _validationContext)
        {
            var dbContext = (ApplicationDbContext)_validationContext.GetService(typeof(ApplicationDbContext));
            List<ValidationResult> results = new List<ValidationResult>();

            var customerCode = (from _customer in dbContext.CustomerProfiles
                                where _customer.PhoneNumber == PhoneNumber
                                select _customer.Code)
                           .FirstOrDefault();

            if (customerCode != null && customerCode != Code)
            {
                string message = string.Format(P24Message.PhoneNumberExisted, customerCode);
                ValidationResult result = new ValidationResult(message, new[] { nameof(PhoneNumber) });
                results.Add(result);
            }

            return results;
        }
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
        [Required(AllowEmptyStrings = false)]
        public string Code { get; set; }

        public string Symptom { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = P24Message.DiagnoseCannotBeEmpty)]
        public string Diagnose { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = P24Message.TreatmentCannotBeEmpty)]
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

        public string Symptom { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = P24Message.DiagnoseCannotBeEmpty)]
        public string Diagnose { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = P24Message.DiagnoseCannotBeEmpty)]
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
