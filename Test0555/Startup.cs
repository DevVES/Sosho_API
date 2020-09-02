using Microsoft.Owin;
using Owin;
using System;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;

[assembly: OwinStartup(typeof(Test0555.Startup))]



namespace Test0555
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            //enable cors origin requests
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
 
            var myProvider = new MyAuthorizationServerProvider();
            OAuthAuthorizationServerOptions options = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = myProvider
            };
            app.UseOAuthAuthorizationServer(options);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
 
 
            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
        }
    }



}
        
