using SAML.PoC.SP3.Models;
using System.IdentityModel.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web.Mvc;

namespace SAML.PoC.SP3.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Claims()
        {
            FederatedAuthentication.WSFederationAuthenticationModule.RedirectToIdentityProvider("mysamlapp", "https://localhost:8443/realms/teste/account/", true);
            ViewBag.ClaimsIdentity = Thread.CurrentPrincipal.Identity;
            return View();
        }

        [Authorize(Roles = "role1")]
        public ActionResult Role1()
        {
            ViewBag.ClaimsIdentity = Thread.CurrentPrincipal.Identity;
            var model = new ClaimsViewModel
            {
                Claims = ViewBag.ClaimsIdentity.Claims
                //.Where(c => c.Type == ClaimTypes.Role)
                ////.Where(c => c.Type == "Role")
            };

            return View(model);
        }

        [Authorize(Roles = "role2")]
        public ActionResult Role2()
        {
            ViewBag.ClaimsIdentity = Thread.CurrentPrincipal.Identity;
            var model = new ClaimsViewModel
            {
                Claims = ViewBag.ClaimsIdentity.Claims
                //.Where(c => c.Type == ClaimTypes.Role)
                ////.Where(c => c.Type == "Role")
            };

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}