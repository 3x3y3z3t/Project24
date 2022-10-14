/*  Create.cshtml.cs
 *  Version: 1.1 (2022.09.09)
 *
 *  Contributor
 *      Arime-chan
 */
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Project24.Data;
using Project24.Identity;
using Project24.Models;

namespace Project24.Areas.ServiceManagement.Pages
{
    [Authorize(Roles = Constants.ROLE_MANAGER)]
    public class CreateModel : PageModel
    {
        public class DataModel
        {
            //[Required(ErrorMessage = "Mã dịch vụ không được để trống")]
            public string ServiceCode { get; set; }

            [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
            public string ServiceName { get; set; }

            [Required(ErrorMessage = "Giá không được để trống")]
            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:#,###}", NullDisplayText = "0")]
            [DataType(DataType.Currency, ErrorMessage = Constants.ERROR_INVALID_PRICE)]
            [Range(double.Epsilon, double.MaxValue, ErrorMessage = Constants.ERROR_INVALID_PRICE)]
            public float Price { get; set; }

            [Required(ErrorMessage = "Mật khẩu không được để trống")]
            [DataType(DataType.Password)]
            public string ManagerPassword { get; set; }

            public DataModel()
            { }
        }

        [BindProperty]
        public DataModel Data { get; set; }

        public string StatusMessage { get; set; }

        public CreateModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, ILogger<CreateModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_Logger = _logger;
        }

        public async Task OnGetAsync()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            P24IdentityUser currentUser = await m_UserManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Page(); // this should not happens;
            }

            if (ModelState.IsValid)
            {

                if (!await m_UserManager.CheckPasswordAsync(currentUser, Data.ManagerPassword))
                {
                    StatusMessage = "Error: " + Constants.ERROR_MANAGER_PASSWORD_INCORRECT;

                    await Utils.RecordAction(
                        m_DbContext,
                        currentUser.UserName,
                        ActionRecord.Operation_.CreateService,
                        ActionRecord.OperationStatus_.Failed,
                        Constants.ERROR_MANAGER_PASSWORD_INCORRECT
                    );

                    return Page();
                }

                ServiceDev service = new ServiceDev()
                {
                    ServiceCode = Data.ServiceCode,
                    Name = Data.ServiceName,
                    Price = Data.Price,
                    AddedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                m_DbContext.Add(service);

                await m_DbContext.SaveChangesAsync();

                return RedirectToPage("./Index");

                //foreach (var error in result.Errors)
                //{
                //    ModelState.AddModelError(string.Empty, error.Description);
                //}
            }

            // If we got this far, something failed, redisplay form
            StatusMessage = "Error: Lỗi không xác định.";

            m_Logger.LogError("Unknown Error during Create Service.");

            await Utils.RecordAction(
                m_DbContext,
                currentUser.UserName,
                ActionRecord.Operation_.CreateUser,
                ActionRecord.OperationStatus_.Failed,
                Constants.ERROR_UNKNOWN_ERROR_FROM_PROGRAM
            );

            return Page();
        }

        private readonly ApplicationDbContext m_DbContext;

        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<CreateModel> m_Logger;
    }

}
