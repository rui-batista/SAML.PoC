using System.Collections.Generic;
using System.Security.Claims;

namespace SAML.PoC.SP3.Models
{
    public class ClaimsViewModel
    {
        public IEnumerable<Claim> Claims { get; set; }
    }
}