using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security;

using Microsoft.Owin.Security.OAuth;

//<package id="Microsoft.AspNet.Identity.Core" version="1.0.0" targetFramework="net45" />
//<package id="Microsoft.AspNet.Identity.EntityFramework" version="1.0.0" targetFramework="net45" />
//<package id="Microsoft.AspNet.Identity.Owin" version="1.0.0" targetFramework="net45" />

namespace Test0555
{
    public class MyAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated(); // 
        }
 
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            if (context.UserName == "admin" && context.Password == "admin")
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
                identity.AddClaim(new Claim("username", "admin"));
                identity.AddClaim(new Claim(ClaimTypes.Name, "Sourav Mondal"));
                context.Validated(identity);
            }

            if (context.UserName == "Darshan" && context.Password == "Darshan")
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
                identity.AddClaim(new Claim("username", "admin"));
                identity.AddClaim(new Claim(ClaimTypes.Name, "Darshan Soni"));
                context.Validated(identity);
            }


            else if (context.UserName == "user" && context.Password == "user")
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "user"));
                identity.AddClaim(new Claim("username", "user"));
                identity.AddClaim(new Claim(ClaimTypes.Name, "Suresh Sha"));
                context.Validated(identity);
            }

            else if (context.UserName == "Monark" && context.Password == "Monark")
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "user"));
                identity.AddClaim(new Claim("username", "user"));
                identity.AddClaim(new Claim(ClaimTypes.Name, "Monark Patel"));
                context.Validated(identity);
            }

            else if (context.UserName == "Prakash" && context.Password == "Prakash")
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "user"));
                identity.AddClaim(new Claim("username", "user"));
                identity.AddClaim(new Claim(ClaimTypes.Name, "Prakash Rajput"));
                context.Validated(identity);
            }
            else
            {
                context.SetError("invalid_grant", "Provided username and password is incorrect");
                return;
            }
        }
    }
}

