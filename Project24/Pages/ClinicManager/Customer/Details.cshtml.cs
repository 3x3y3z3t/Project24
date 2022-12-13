/*  P24/Customer/Details.cshtml
 *  Version: 1.7 (2022.12.13)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project24.App;
using Project24.Data;
using Project24.Models;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;
using Project24.Models.Identity;

namespace Project24.Pages.ClinicManager.Customer
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class DetailsModel : PageModel
    {
        public P24CustomerDetailsViewModelEx CustomerViewData { get; private set; }

        public P24ImageListingModel ListImageModel { get; private set; }


        public DetailsModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, ILogger<DetailsModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _userManager;
            m_Logger = _logger;
        }


        public async Task<IActionResult> OnGetAsync(string _code)
        {
            if (string.IsNullOrEmpty(_code))
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Customer, "null", "List"));

            var customer = await (from _customer in m_DbContext.CustomerProfiles.Include(_c => _c.AddedUser).Include(_c => _c.UpdatedUser)
                                  where _customer.Code == _code
                                  select new P24CustomerDetailsViewModelEx()
                                  {
                                      Code = _customer.Code,
                                      Fullname = _customer.FullName,
                                      Gender = App.AppUtils.NormalizeGenderString(_customer.Gender),
                                      DoB = _customer.DateOfBirth,
                                      PhoneNumber = _customer.PhoneNumber,
                                      Address = _customer.Address,
                                      Notes = _customer.Notes,
                                      AddedDate = _customer.AddedDate,
                                      UpdatedDate = _customer.UpdatedDate,
                                      DeletedDate = _customer.DeletedDate,
                                      AddedUserName = _customer.AddedUser.UserName,
                                      UpdatedUserName = _customer.UpdatedUser.UserName
                                  })
                            .FirstOrDefaultAsync();

            if (customer == null)
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Customer, _code, "List"));

            CustomerViewData = customer;

            var id = await (from _customer in m_DbContext.CustomerProfiles
                            where _customer.Code == _code
                            select _customer.Id)
                     .FirstOrDefaultAsync();

            var images = await (from _image in m_DbContext.CustomerImages
                                where _image.OwnerCustomerId == id && _image.DeletedDate == DateTime.MinValue
                                select new P24ImageViewModel()
                                {
                                    Id = _image.Id,
                                    Path = _image.Path,
                                    Name = _image.Name
                                })
                         .ToListAsync();

            ListImageModel = new P24ImageListingModel()
            {
                Module = P24Module.Customer,
                OwnerCode = _code,
                Images = images
            };

            return Page();
        }

        // Ajax call only;
        public async Task<JsonResult> OnGetFetchAsync(string _code, string _phone)
        {
            if (_code == null)
                _code = "";
            if (_phone == null)
                _phone = "";

            if (_code == "" && _phone == "")
            {
                DailyIndexes dind = m_DbContext.DailyIndexes;
                string nextCustomerCode = string.Format(AppConfig.CustomerCodeFormatString, DateTime.Today, dind.CustomerIndex + 1);
                return new JsonResult(nextCustomerCode);
            }

            var customer = await (from _customer in m_DbContext.CustomerProfiles.Include(_c => _c.AddedUser).Include(_c => _c.UpdatedUser)
                                  where _customer.Code.Contains(_code) && _customer.PhoneNumber.Contains(_phone)
                                  select new P24CreateCustomerFormDataModel()
                                  {
                                      Code = _customer.Code,
                                      FullName = _customer.FullName,
                                      Gender = _customer.Gender,
                                      DateOfBirth = _customer.DateOfBirth,
                                      PhoneNumber = _customer.PhoneNumber,
                                      Address = _customer.Address,
                                      Notes = _customer.Notes
                                  })
                            .FirstOrDefaultAsync();

            return new JsonResult(customer);
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<DetailsModel> m_Logger;
    }

}
