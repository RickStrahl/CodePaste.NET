using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Westwind.Utilities;
using System.ServiceModel.Syndication;
using CodePasteBusiness;
using System.Text;
using System.Collections;
using System.IO;
using System.Xml;
using Westwind.Web.JsonSerializers;
using System.Text.RegularExpressions;
using Westwind.Web;

namespace CodePasteMvc.Controllers
{

    /// <summary>
    /// API controller that handles results based.
    /// 
    /// Reusable base class.
    /// </summary>    
    public class ApiControllerBase : baseController
    {
        /// <summary>
        /// Used to determine output format - json,xml,rss,atom
        /// </summary>
        protected string Format = string.Empty;


        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            // Create a custom Invoker that handles non-actionresult controller methods
            ActionInvoker = new ApiActionInvoker();

            base.Initialize(requestContext);

            // pick up the format
            Format = (Request.QueryString["Format"] ?? string.Empty).ToLower();

            //if (string.IsNullOrEmpty(this.Format))
            //    this.Format = "xml";
        }

        /// <summary>
        /// Checks 
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected internal virtual ActionResult ApiResult(object instance)
        {            
            if (string.IsNullOrEmpty(Format))
                return null;

            if (Format == "json")
                return Json(instance, JsonRequestBehavior.AllowGet);
            else if (Format == "xml")
            {
                string xmlResult = string.Empty;

                if (!SerializationUtils.SerializeObject(instance, out xmlResult))
                    throw new InvalidOperationException("Unable to serialize instance to Xml");
                return Content(xmlResult, "text/xml", Encoding.UTF8);
            }
            else if (Format == "rss" || Format == "atom")
            {
                return GetFeed(instance);
            }

            return null;
        }

        /// <summary>
        /// Returns and rss or atom feed 
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected internal ActionResult GetFeed(object instance)
        {
            try
            {
                string title = "CodePaste.NET";
                string action = RouteData.Values["listAction"] as string;
                if (string.IsNullOrEmpty(action))
                    action = RouteData.Values["Action"] as string ?? string.Empty;
                action = action.ToLower();

                if (action == "recent")
                    title = "CodePaste.NET Recent Snippets";
                else if (action == "mysnippets")
                    title = "CodePaste.NET - My Snippets";

                SyndicationFeed feed = new SyndicationFeed(title, "Paste and Link .NET Code", new Uri(Request.Url.AbsoluteUri));

                feed.BaseUri = new Uri("http://codepaste.net/recent");
                feed.LastUpdatedTime = DateTime.Now;           

                List<SyndicationItem> feedItems = new List<SyndicationItem>();
                foreach (CodeSnippetListItem item in (IEnumerable)instance)
                {

                    // remove lower ascii characters (< 32 exclude 9,10,13)
                    string code = Regex.Replace(item.Code, @"[\u0000-\u0008,\u000B,\u000C,\u000E-\u001F]", "");

                    SyndicationItem rssItem = new SyndicationItem()
                    {
                        Id = item.Id,
                        Title = SyndicationContent.CreateHtmlContent(item.Title),
                        Content = SyndicationContent.CreateHtmlContent(
                            //"<link href=\"" + WebUtils.ResolveServerUrl("~/css/csharp.css") + "\" rel=\"stylesheet\" type=\"text/css\">\r\n <style type='text/css'>.kwrd { color: blue; font-weight: bold }</style>\r\n" +            
                                    "<pre>\r\n" +
                                    HtmlUtils.HtmlEncode(code) +
                            //snippet.GetFormattedCode(item.Code, item.Language, item.ShowLineNumbers) +
                                    "</pre>"),
                        PublishDate = item.Entered,
                    };

                    if (!string.IsNullOrEmpty(item.Author))
                        rssItem.Authors.Add(new SyndicationPerson("", item.Author, null));

                    rssItem.Links.Add(new SyndicationLink(new Uri(WebUtils.GetFullApplicationPath() + "/" + item.Id),
                                                            "alternate", item.Title, "text/html", 1000));

                    feedItems.Add(rssItem);
                }

                feed.Items = feedItems;

                MemoryStream ms = new MemoryStream();
                var settings = new XmlWriterSettings()
                {
                    CheckCharacters = false                 
                };
                XmlWriter writer = XmlWriter.Create(ms,settings);
                if (Format == "rss")
                {
                    Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(feed);
                    rssFormatter.WriteTo(writer);
                }
                else
                {
                    Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(feed);
                    atomFormatter.WriteTo(writer);
                }
                writer.Flush();

                ms.Position = 0;

                return Content(Encoding.UTF8.GetString(ms.ToArray()), "application/xml");
            }
            catch (Exception ex)
            {
                Response.Write(HtmlUtils.DisplayMemo("Error: " +  ex.GetBaseException().Message + "\r\n" + ex.StackTrace));
                Response.End();
                return null;
                
            }

            return null;
        }

        /// <summary>
        /// Parses a single object value from the Request input stream into the
        /// specified type
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        protected internal object ParseObjectFromPostData(Type dataType)
        {
            StreamReader sr = new StreamReader(Request.InputStream);
            string data = sr.ReadToEnd();
            sr.Close();

            object result = null;

            if (Request.ContentType == "text/javascript" || Request.ContentType == "application/json")
            {
                JSONSerializer ser = new JSONSerializer(SupportedJsonParserTypes.WestWindJsonSerializer);
                result = ser.Deserialize(data, dataType);
            }
            else if (Request.ContentType == "text/xml")
            {
                result = SerializationUtils.DeSerializeObject(data, dataType);
            }
            else
                return ExceptionResult("Unsuppported data input format. Please provide input content-type as text/javascript, application/json or text/xml.");

            return result;
        }

        /// <summary>
        /// Ensure that exceptions are returned in the API format
        /// Excpetion is rendered as a Callback Exception
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);

            // only handle API results here: Format must be set
            // otherwise fall through and return
            if (string.IsNullOrEmpty(Format))
                return;

            Response.StatusCode = 500;

            CallbackException exception = new CallbackException(filterContext.Exception.Message);
            
            //if (HttpContext.IsDebuggingEnabled)                                
            //    exception.StackTrace = filterContext.Exception.StackTrace;

            exception.IsError = true;

            filterContext.Result = ApiResult(exception);
            filterContext.ExceptionHandled = true;
        }

        protected ActionResult ExceptionResult(Exception ex)
        {
            Response.StatusCode = 500;

            CallbackException exception = new CallbackException(ex.GetBaseException().Message);
            
            //if (HttpContext.IsDebuggingEnabled)
            //    exception.StackTrace = ex.StackTrace;

            exception.IsError = true;

            // return result based on ?Format= 
            return ApiResult(exception);
        }

        protected ActionResult ExceptionResult(string message)
        {
            Response.StatusCode = 500;

            CallbackException exception = new CallbackException(message);            
            exception.IsError = true;

            // return result based on ?Format=
            return ApiResult(exception);
        }

    }

    /// <summary>
    /// Custom ActionInvoker that allows Controller methods to return plain
    /// value results rather than action results that are formatted using
    /// API formatting rules of an API Controller
    /// </summary>
    public class ApiActionInvoker : ControllerActionInvoker
    {
        /// <summary>
        /// Overrides output creation if the result from a controller
        /// method doesn't return an ActionResult. Instead it turns the
        /// result into the appropriate API response.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="actionDescriptor"></param>
        /// <param name="actionReturnValue"></param>
        /// <returns></returns>
        protected override ActionResult CreateActionResult(ControllerContext controllerContext, ActionDescriptor actionDescriptor, object actionReturnValue)
        {

            // Allow for controllers to return plain values rather than an action result
            // Also handles exceptions from API controller
            if (!(actionReturnValue is ActionResult) && controllerContext.Controller is ApiControllerBase)
            {
                ApiController controller = controllerContext.Controller as ApiController;

                // Generate API results in XML/JSON
                return controller.ApiResult(actionReturnValue);
            }

            // Process as normal request
            return base.CreateActionResult(controllerContext, actionDescriptor, actionReturnValue);
        }
    }
}
