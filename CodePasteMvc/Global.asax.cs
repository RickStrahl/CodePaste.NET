using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CodePasteMvc.Controllers;
using Westwind.Utilities;
using Westwind.Utilities.Logging;
using CodePasteBusiness;
using Westwind.Web.JsonSerializers;
using Westwind.Web;

namespace CodePasteMvc
{

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.aspx/{*pathInfo}");
            routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");
            routes.IgnoreRoute("{resource}.{ext}");

            routes.MapRoute("New",
                       "New/{*pathinfo}",
                        new { controller = "Snippet", action = "New" }
            );
            
            routes.MapRoute("List",
                        "list/{listAction}/{listFilter}",
                        new { controller="Snippet", action="List", listAction="recent", listFilter="" }
                        );

            routes.MapRoute("Recent",
                            "recent",
                            new { controller = "Snippet", action = "Recent" });

            routes.MapRoute("MySnippets",
                            "mysnippets/{count}",
                            new { controller = "Snippet", action = "List", listAction="MySnippets", count = "100" });

            routes.MapRoute("MyFavorites",
                "myfavorites/{count}",
                new { controller = "Snippet", action = "List", listAction = "MyFavorites", count = "100" });

            routes.MapRoute("Search",
                    "search/{*pathinfo}",
                    new { controller = "Snippet", action = "Search" });


            routes.MapRoute("Admin",
                        "admin",
                        new { controller = "Admin", action = "Index" });            

            routes.MapRoute("Error",
                        "error/{action}",
                        new { controller="Error", action="ShowError", title="", message="", redirecto=""});

            // Main RSS Feed for the site - gets Recent items
            routes.MapRoute("Feed",
                "feed",
                new { controller = "Api", action = "Feed" });


            routes.MapRoute("ApiList",
                            "api/list/{filter}/{filterParameter}",
                            new { controller = "Api", action = "List", filter = "Recent", filterParameter = "" });

            routes.MapRoute("Api",
                "api/{action}/{id}",
                new { controller = "Api", action = "Feed", id = "", sessionKey = "" });





            //routes.MapRoute("ApiSession",
            //                "api/{action}/{id}/{sessionkey}",
            //                new { controller = "Api", action = "Feed", id = "", sessionKey = "" });            

            // generic REST API access
            //routes.MapRoute("Api",
            //        "api/{action}/{id}",
            //        new { controller="Api", action="recent", id="" });

            routes.MapRoute("CodeOnly",
                    "codeonly/{id}",
                    new { controller = "Snippet", action = "CodeOnly", id = "" });

            routes.MapRoute("ShowUrl",
                "ShowUrl",
                new { controller = "Snippet", action = "ShowUrl" });                

            
            // THIS ROUTE IS VERY LOOSE HENCE THE EXPLICIT DECLARATIONS ABOVE
            routes.MapRoute("Show",
                    "{id}",
                    new { controller = "Snippet", action = "Show", id = "" });

            routes.MapRoute("ShowHtml","showhtml/{id}",new { controller = "Snippet", action = "ShowHtml", id = "" });

            // never called
            routes.MapRoute(
                "Default",                                              // Route name
                "{controller}/{action}/{id}",                           // URL with parameters
                new { controller = "Snippet", action = "New", id = "" }  // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);

            
            //.RewriteRoutesForTesting(RouteTable.Routes);

            //JSONSerializer.DefaultJsonParserType = SupportedJsonParserTypes.JavaScriptSerializer;

            // Create a log manager based on setting in config file
            LogManager.Create();

            // Clear out expired anonymous snippets whenever app starts
            busCodeSnippet Snippet = new busCodeSnippet();
            Snippet.ClearAnonymousSnippets(App.Configuration.HoursToDeleteAnonymousSnippets,
                                           App.Configuration.MinViewBeforeDeleteAnonymousSnippets);

            // Clear Anonymous snippets
            scheduler = new Scheduler();
            scheduler.CheckFrequency = 3600 * 1000;
            scheduler.ExecuteScheduledEvent += Scheduler_ExecuteScheduledEvent;            
        }

        protected void Application_End()
        {
            scheduler?.Dispose();
        }
        

        private Scheduler scheduler;

        private void Scheduler_ExecuteScheduledEvent(object sender, EventArgs e)
        {
            try
            {
                var admin = new busCodeSnippet();
                admin.ClearAnonymousSnippets(App.Configuration.HoursToDeleteAnonymousSnippets, 9999999);
            }
            catch { }
        }

        protected void Application_Error()
        {
            try
            {
                Exception serverException = Server.GetLastError();

                WebErrorHandler errorHandler;

                // Try to log the inner Exception since that's what
                // contains the 'real' error.
                if (serverException.InnerException != null)
                    serverException = serverException.InnerException;

                errorHandler = new WebErrorHandler(serverException);
                
                // MUST clear out any previous response in case error occurred in a view
                // that already started rendering
                Response.Clear();


                // Log the error if specified
                if (LogManagerConfiguration.Current.LogErrors)
                {
                    errorHandler.Parse();

                    //try
                    //{
                        WebLogEntry entry = new WebLogEntry(serverException, this.Context);
                        entry.Details = errorHandler.ToString();

                        LogManager.Current.WriteEntry(entry);
                    //}
                    //catch {  /* Log Error shouldn't kill the error handler */ }
                }

                // Retrieve the detailed String information of the Error
                string ErrorDetail = errorHandler.ToString();

                // Optionally email it to the Admin contacts set up in WebStoreConfig
                if (!string.IsNullOrEmpty(App.Configuration.ErrorEmailAddress))
                    AppWebUtils.SendAdminEmail(App.Configuration.ApplicationTitle + "Error: " + Request.RawUrl, ErrorDetail, "", 
                                               App.Configuration.ErrorEmailAddress);


                // Debug modes handle different error display mechanisms
                // Default  - default ASP.Net - depends on web.config settings
                // Developer  - display a generic application error message with no error info
                // User  - display a detailed error message with full error info independent of web.config setting
                if (App.Configuration.DebugMode == DebugModes.DeveloperErrorMessage)
                {

                    Server.ClearError();
                    Response.TrySkipIisCustomErrors = true;
                    ErrorController.ShowErrorPage("Application Error", "<div class='codedisplay'><pre id='Code'>" + HttpUtility.HtmlEncode(ErrorDetail) + "</pre></div>");
                    return;
                }

                else if (App.Configuration.DebugMode == DebugModes.ApplicationErrorMessage)
                {
                    string StockMessage =
                            "The Server Administrator has been notified and the error logged.<p>" +
                            "Please continue by either clicking the back button or by returning to the home page.</p>" +
                            "<p><b><a href='" + Request.ApplicationPath + "'>Click here to continue...</a></b></p>";

                    // Handle some stock errors that may require special error pages
                    HttpException httpException = serverException as HttpException;
                    if (httpException != null)
                    {
                        int HttpCode = httpException.GetHttpCode();
                        Server.ClearError();

                        if (HttpCode == 404) // Page Not Found 
                        {
                            Response.StatusCode = 404;
                            ErrorController.ShowErrorPage("Page not found",
                                "You've accessed an invalid page on this Web server. " +
                                StockMessage,null);
                            return;
                        }
                        if (HttpCode == 401) // Access Denied 
                        {
                            Response.StatusCode = 401;
                            ErrorController.ShowErrorPage("Access Denied",
                                "You've accessed a resource that requires a valid login. " +
                                StockMessage);
                            return;
                        }
                    }

                    // Display a generic error message
                    Server.ClearError();
                    Response.StatusCode = 500;

                    Response.TrySkipIisCustomErrors = true;
                    
                    ErrorController.ShowErrorPage("Application Error",
                        "We're sorry, but an unhandled error occurred on the server. " +
                        StockMessage);

                    return;
                }

                return;
            }
            catch (Exception ex)
            {
                // Failure in the attempt to report failure - try to email
                if (!string.IsNullOrEmpty(App.Configuration.ErrorEmailAddress))
                {
                    AppWebUtils.SendAdminEmail(App.Configuration.ApplicationTitle + "Error: " + Request.RawUrl,
                            "Application_Error failed!\r\n\r\n" +
                            ex.ToString(), "",App.Configuration.ErrorEmailAddress);
                }

                // and display an error message
                Server.ClearError();
                Response.StatusCode = 500;
                Response.TrySkipIisCustomErrors = true;
                
                ErrorController.ShowErrorPage("Application Error Handler Failed",
                        "The application Error Handler failed with an exception." +
                        (App.Configuration.DebugMode == DebugModes.DeveloperErrorMessage ? "<pre>" + ex.ToString() + "</pre>" : ""),null);

            }
        }


        protected void Application_EndRequest(Object sender, EventArgs e)
        {
            // Request Logging
            if (LogManagerConfiguration.Current.LogWebRequests)
            {
                try
                {
                    WebLogEntry entry = new WebLogEntry()
                    {
                        ErrorLevel = ErrorLevels.Info,
                        Message = this.Context.Request.FilePath,
                        RequestDuration = (decimal)DateTime.Now.Subtract(Context.Timestamp).TotalMilliseconds
                    };
                    entry.UpdateFromRequest(this.Context);
                    LogManager.Current.WriteEntry(entry);
                }
                catch { ;}
            }
        }
    }
}