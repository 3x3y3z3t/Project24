/*  P24/Customer/Details.cshtml
 *  Version: 1.4 (2022.11.28)
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
using Project24.Data;
using Project24.Identity;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;

namespace Project24.Pages.ClinicManager.Customer
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class DetailsrModel : PageModel
    {
        public P24CustomerDetailsViewModel CustomerViewData { get; private set; }

        public P24ImageListingModel ListImageModel { get; private set; }


        public DetailsrModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _userManager, ILogger<DetailsrModel> _logger)
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
                                  select new P24CustomerDetailsViewModel()
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
                Images = images,
                CustomerCode = _code,
                IsReadonly = false
            };

            return Page();
        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<DetailsrModel> m_Logger;
    }

}
