using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using CodePasteBusiness;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using Owin.Security.Providers.GitHub;

namespace CodePasteMvc
{   
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/LogOn")
            });
            
            // these values are stored in CodePasteKeys.json
            // and are NOT included in repro - autocreated on first load
            if (!string.IsNullOrEmpty(App.Secrets.GitHubClientId))
            {
                app.UseGoogleAuthentication(                
                    clientId: App.Secrets.GoogleClientId,
                    clientSecret: App.Secrets.GoogleClientSecret);
            }

            if (!string.IsNullOrEmpty(App.Secrets.TwitterConsumerKey))
            {
                app.UseTwitterAuthentication(
                    consumerKey: App.Secrets.TwitterConsumerKey,
                    consumerSecret: App.Secrets.TwitterConsumerSecret);
            }

            if (!string.IsNullOrEmpty(App.Secrets.GitHubClientId))
            {
                app.UseGitHubAuthentication(
                    clientId: App.Secrets.GitHubClientId,
                    clientSecret: App.Secrets.GitHubClientSecret);
            }

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }
    }
}