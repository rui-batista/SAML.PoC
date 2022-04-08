using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Saml2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SAML.PoC.SP1.Controllers
{
    [AllowAnonymous]
    [Route("IdPInitiated")]
    public class IdPInitiatedController : Controller
    {
        public IActionResult Initiate()
        {
            var serviceProviderRealm = "https://some-domain.com/some-service-provider";

            var binding = new Saml2PostBinding
            {
                RelayState = $"RPID={Uri.EscapeDataString(serviceProviderRealm)}"
            };

            var config = new Saml2Configuration
            {
                Issuer = "http://some-domain.com/this-application",
                //SingleSignOnDestination = new Uri("https://test-adfs.itfoxtec.com/adfs/ls/");
                SingleSignOnDestination = new Uri("https://localhost:44305/Auth/Login"),
                SigningCertificate = CertificateUtil.Load(
                    Startup.AppEnvironment.MapToPhysicalFilePath("itfoxtec.identity.saml2.testwebappcore_Certificate.pfx"),
                    "!QAZ2wsx"),
                SignatureAlgorithm = Saml2SecurityAlgorithms.RsaSha256Signature
            };

            //var appliesToAddress = "https://test-adfs.itfoxtec.com/adfs/services/trust";
            var appliesToAddress = "https://localhost:44305/";

            var response = new Saml2AuthnResponse(config)
            {
                Status = Saml2StatusCodes.Success
            };

            var claimsIdentity = new ClaimsIdentity(CreateClaims());
            response.NameId = new Saml2NameIdentifier(claimsIdentity.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value).Single(), NameIdentifierFormats.Persistent);
            response.ClaimsIdentity = claimsIdentity;
            var token = response.CreateSecurityToken(appliesToAddress);

            return binding.Bind(response).ToActionResult();
        }

        private static IEnumerable<Claim> CreateClaims()
        {
            yield return new Claim(ClaimTypes.NameIdentifier, "some-user-identity");
            yield return new Claim(ClaimTypes.Email, "some-user@domain.com");
        }
    }
}