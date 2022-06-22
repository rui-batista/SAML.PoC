using System.Collections.Generic;
using System.Security.Claims;

namespace SAML.PoC.SP2.Models
{
    public class ClaimsViewModel
    {
        public IEnumerable<Claim> Claims { get; set; }
    }
}