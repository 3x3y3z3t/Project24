/*  P24/Customer/Details.cshtml
 *  Version: 1.9 (2023.02.11)
 *
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project24.App;
using Project24.Data;
using Project24.Models;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;

namespace Project24.Pages.ClinicManager.Customer
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class DetailsModel : PageModel
    {
        public P24CustomerDetailsViewModelEx CustomerViewData { get; private set; }

        public P24ImageListingModel ListImageModel { get; private set; }


        public DetailsModel(ApplicationDbContext _context)
        {
            m_DbContext = _context;
        }


        public async Task<IActionResult> OnGetAsync(string _code)
        {
            if (string.IsNullOrEmpty(_code))
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Customer, "null", "List"));

            var customer = await (from _customer in m_DbContext.CustomerProfiles.Include(_c => _c.AddedUser).Include(_c => _c.EditedUser)
                                  where _customer.Code == _code
                                  select new P24CustomerDetailsViewModelEx()
                                  {
                                      Code = _customer.Code,
                                      Fullname = _customer.FullName,
                                      Gender = AppUtils.NormalizeGenderString(_customer.Gender),
                                      DoB = _customer.DateOfBirth,
                                      PhoneNumber = _customer.PhoneNumber,
                                      Address = _customer.Address,
                                      Note = _customer.Note,
                                      AddedDate = _customer.AddedDate,
                                      UpdatedDate = _customer.EditedDate,
                                      DeletedDate = _customer.DeletedDate,
                                      AddedUserName = _customer.AddedUser.UserName,
                                      UpdatedUserName = _customer.EditedUser.UserName
                                  })
                            .FirstOrDefaultAsync();

            if (customer == null)
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Customer, _code, "List"));

            var tickets = await (from _ticket in m_DbContext.TicketProfiles.Include(_t => _t.Customer)
                                 where _ticket.Customer.Code == customer.Code && _ticket.DeletedDate == DateTime.MinValue
                                 select new P24TicketDetailsViewModel()
                                 {
                                     Code = _ticket.Code,
                                     Symptom = _ticket.Symptom,
                                     Diagnose = _ticket.Diagnose,
                                     Treatment = _ticket.ProposeTreatment,
                                     Note = _ticket.Note,
                                     AddedDate = _ticket.AddedDate
                                 })
                          .ToListAsync();

            var images = await (from _image in m_DbContext.CustomerImages.Include(_i => _i.OwnerCustomer)
                                where _image.OwnerCustomer.Code == _code && _image.DeletedDate == DateTime.MinValue
                                select new P24ImageViewModel()
                                {
                                    Id = _image.Id,
                                    Path = _image.Path,
                                    Name = _image.Name
                                })
                         .ToListAsync();

            CustomerViewData = customer;
            CustomerViewData.Tickets = tickets;
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

            var customer = await (from _customer in m_DbContext.CustomerProfiles.Include(_c => _c.AddedUser).Include(_c => _c.EditedUser)
                                  where _customer.Code.Contains(_code) && _customer.PhoneNumber.Contains(_phone)
                                  select new P24CreateCustomerFormDataModel()
                                  {
                                      Code = _customer.Code,
                                      FullName = _customer.FullName,
                                      Gender = _customer.Gender,
                                      DateOfBirth = _customer.DateOfBirth,
                                      PhoneNumber = _customer.PhoneNumber,
                                      Address = _customer.Address,
                                      Note = _customer.Note
                                  })
                            .FirstOrDefaultAsync();

            return new JsonResult(customer);
        }


        private readonly ApplicationDbContext m_DbContext;
    }

}
