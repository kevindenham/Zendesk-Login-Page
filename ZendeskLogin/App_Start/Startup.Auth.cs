using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Owin;
using ZendeskLogin.Models;

namespace ZendeskLogin
{
    public partial class Startup
    {
        // Your Azure AD settings
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        public static readonly string Authority = aadInstance + tenantId;

        // This is the resource ID of the AAD Graph API.  We'll need this to request a token to call the Graph API.
        string graphResourceId = "https://graph.windows.net";

        public void ConfigureAuth(IAppBuilder app)
        {
            ApplicationDbContext db = new ApplicationDbContext();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = Authority,
                    PostLogoutRedirectUri = postLogoutRedirectUri,

                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
                       AuthorizationCodeReceived = (context) => 
                       {
                           var code = context.Code;
                           ClientCredential credential = new ClientCredential(clientId, appKey);
                           string signedInUserID = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
                           //AuthenticationContext authContext = new AuthenticationContext(Authority, new ADALTokenCache(signedInUserID));
                           //AuthenticationResult result = authContext.AcquireTokenByAuthorizationCode(
                           //code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, graphResourceId);
                           string SHARED_KEY = "ADD YOUR SHARED KEY FROM ZENDESK JWT SETTINGS";
                           string SUBDOMAIN = "YOUR ZENDESK SUBDOMAIN";
                           string email = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.Name).Value;

                           string firstname = "";
                           string lastname = "";
                           try
                           {
                               firstname = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.GivenName).Value;
                           } catch { }
                           try
                           {
                               lastname = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.Surname).Value;
                           } catch { }
                           TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
                           int timestamp = (int)t.TotalSeconds;

                           var payload = new Dictionary<string, object>() {
                            { "iat", timestamp },
                            { "jti", Guid.NewGuid().ToString() },
                            { "name", firstname + " " + lastname},
                            { "email", email }
                           };

                           string token = JWT.JsonWebToken.Encode(payload, SHARED_KEY, JWT.JwtHashAlgorithm.HS256);
                           string redirectUrl = "https://" + SUBDOMAIN + ".zendesk.com/access/jwt?jwt=" + token;
                           HttpContext.Current.Response.Redirect(redirectUrl);

                           return Task.FromResult(0);
                       }
                    }
                });
        }
    }
}
