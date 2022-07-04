using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SAML.PoC.SP2.Models;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

namespace SAML.PoC.SP2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Claims()
        {
            return View();
        }

        [Authorize(Roles = "role1")]
        public IActionResult Role1()
        {
            var model = new ClaimsViewModel
            {
                Claims = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                //.Where(c => c.Type == "Role")
            };

            return View(model);
        }

        [Authorize(Roles = "role2")]
        public IActionResult Role2()
        {
            var model = new ClaimsViewModel
            {
                Claims = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                //.Where(c => c.Type == "Role")
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}