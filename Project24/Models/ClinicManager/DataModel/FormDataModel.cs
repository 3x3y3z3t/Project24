/*  FormDataModel.cs
 *  Version: 1.0 (2022.11.28)
 *
 *  Contributor
 *      Arime-chan
 */

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Project24.Models.ClinicManager.DataModel
{
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
        public string Notes { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile[] UploadedFiles { get; set; }

        //[Required(ErrorMessage = Constants.ERROR_MANAGER_PASSWORD_REQUIRED)]
        //[DataType(DataType.Password)]
        //public string ManagerPassword { get; set; }

        public P24CreateCustomerFormDataModel()
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
        public string Notes { get; set; }

        public P24EditCustomerFormDataModel()
        { }
    }

    public class P24CreateImageFormDataModel
    {
        [Required]
        public string CustomerCode { get; set; }

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
        public string CustomerCode { get; set; }

        public P24DeleteImageFormDataModel()
        { }
    }

}
