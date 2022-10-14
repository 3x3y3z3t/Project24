//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using Project24.Models;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Project24.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly ILogger<HomeController> _logger;

//        public HomeController(ILogger<HomeController> logger)
//        {
//            _logger = logger;
//        }

//        [AllowAnonymous]
//        public IActionResult Index()
//        {
//            //if (!HttpContext.User.Identity.IsAuthenticated)
//            //{
//            //    return Redirect("Identity/Account/Login");
//            //}

//            //return Redirect("Dashboard/Index");
//            //return Redirect("Identity/Account/Manage");
//            //return View();




//            if (!HttpContext.User.Identity.IsAuthenticated)
//            {
//                return View("Login");
//            }

//            if (HttpContext.User.IsInRole(Constants.ROLE_ADMIN))
//            {
//                return View("Navigator");
//            }

//            if (HttpContext.User.IsInRole(Constants.ROLE_NAS_USER))
//            {
//                return View("Nas/Index");
//            }

//            if (HttpContext.User.IsInRole(Constants.ROLE_MANAGER))
//            {
//                return View("ClinicManager/Index");
//            }

//            return StatusCode(StatusCodes.Status403Forbidden);


//        }
        
//        // GET: /Login
//        public async Task<IActionResult> Login()
//        {
//            var lastCustomer = await _context.Customer.OrderByDescending(_customer => _customer.Id).FirstOrDefaultAsync();
//            if (lastCustomer == null)
//            {
//                ViewData["NextPatientId"] = 0;
//            }
//            else
//            {
//                Console.WriteLine(lastCustomer.Id);
//                ViewData["NextPatientId"] = lastCustomer.Id;
//            }



//            return View();
//        }

//        // POST: Customers/Create
//        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
//        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("Id,Name,Address,CustomerProfileId")] Customer customer)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Add(customer);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(customer);
//        }









//        public IActionResult Privacy()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }
//    }
//}
