/*  P24/Ticket/Details.cshtml.cs
 *  Version: 1.0 (2022.12.04)
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
using Project24.Identity;
using Project24.Models.ClinicManager;
using Project24.Models.ClinicManager.DataModel;

namespace Project24.Pages.ClinicManager.Ticket
{
    [Authorize(Roles = P24RoleName.Manager)]
    public class DetailsModel : PageModel
    {
        public P24TicketDetailsViewModelEx TicketViewData { get; private set; }

        public P24ImageListingModel ListImageModel { get; private set; }


        public DetailsModel(ApplicationDbContext _context, UserManager<P24IdentityUser> _usermanager, ILogger<DetailsModel> _logger)
        {
            m_DbContext = _context;
            m_UserManager = _usermanager;
            m_Logger = _logger;
        }

        //https://eamonkeane.dev/how-to-view-sql-generated-by-entity-framework-core-using-logging/
        //https://eamonkeane.dev/3-ways-to-view-sql-generated-by-entity-framework-core-5/
        public async Task<IActionResult> OnGetAsync(string _code)
        {
            if (string.IsNullOrEmpty(_code))
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Ticket, "null", "List"));

            var ticket = await (from _ticket in m_DbContext.VisitingProfiles.Include(_t => _t.Customer)
                                where _ticket.Code == _code
                                select new P24TicketDetailsViewModelEx()
                                {
                                    Code = _ticket.Code,
                                    Diagnose = _ticket.Diagnose,
                                    Treatment = _ticket.ProposeTreatment,
                                    Notes = _ticket.Notes,
                                    AddedDate = _ticket.AddedDate,
                                    UpdatedDate = _ticket.UpdatedDate,
                                    DeletedDate = _ticket.DeletedDate,
                                    AddedUserName = _ticket.AddedUser.UserName,
                                    UpdatedUserName = _ticket.UpdatedUser.UserName,
                                    Customer = new P24CustomerDetailsViewModel()
                                    {
                                        Code = _ticket.Customer.Code,
                                        Fullname = _ticket.Customer.FullName,
                                        Gender = AppUtils.NormalizeGenderString(_ticket.Customer.Gender),
                                        DoB = _ticket.Customer.DateOfBirth,
                                        PhoneNumber = _ticket.Customer.PhoneNumber,
                                        Address = _ticket.Customer.Address,
                                        Notes = _ticket.Customer.Notes
                                    }
                                })
                         .FirstOrDefaultAsync();

            if (ticket == null)
                return Partial("_CommonNotFound", new CommonNotFoundModel(P24Constants.Ticket, _code, "List"));

            TicketViewData = ticket;

            var id = await (from _ticket in m_DbContext.VisitingProfiles
                            where _ticket.Code == _code
                            select _ticket.Id)
                     .FirstOrDefaultAsync();

            var images = await (from _image in m_DbContext.TicketImages
                                where _image.OwnerTicketId == id && _image.DeletedDate == DateTime.MinValue
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

        //        // Ajax call only;
        //        public async Task<JsonResult> OnGetFetchAsync(string _code)
        //        {
        //            if (string.IsNullOrEmpty(_code))
        //            {
        //                DailyIndexes dind = m_DbContext.DailyIndexes;
        //                string nextCustomerCode = string.Format(AppConfig.CustomerCodeFormatString, DateTime.Today, dind.CustomerIndex + 1);
        //                return new JsonResult(nextCustomerCode);
        //            }

        //            var customer = await (from _customer in m_DbContext.CustomerProfiles.Include(_c => _c.AddedUser).Include(_c => _c.UpdatedUser)
        //                                  where _customer.Code == _code
        //                                  select new P24CreateCustomerFormDataModel()
        //                                  {
        //                                      Code = _customer.Code,
        //                                      FullName = _customer.FullName,
        //                                      Gender = _customer.Gender,
        //                                      DateOfBirth = _customer.DateOfBirth,
        //                                      PhoneNumber = _customer.PhoneNumber,
        //                                      Address = _customer.Address,
        //                                      Notes = _customer.Notes
        //                                  })
        //                            .FirstOrDefaultAsync();

        //            return new JsonResult(customer);
        //        }


        private readonly ApplicationDbContext m_DbContext;
        private readonly UserManager<P24IdentityUser> m_UserManager;
        private readonly ILogger<DetailsModel> m_Logger;
    }

}
