
using System;
using System.Web;
using System.Web.Mvc;
using CodePasteBusiness;
using Westwind.Web.Mvc;
using Westwind.Utilities;
using System.Text;
using System.Collections.Generic;
using System.Web.Routing;
using System.Linq;
using System.Collections;
using System.IO;
using Manoli.Utils.CSharpFormat;
using Westwind.Utilities.InternetTools;

namespace CodePasteMvc.Controllers
{    
    public class SnippetController : ApiControllerBase
    {

        
        public SnippetController()
        {

        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            this.ViewData["AllowEdit"] = false;            
        }


        public ActionResult Show(string id)
        {
            ShowSnippetViewModel model = new ShowSnippetViewModel(this);
            model.AppUserState = AppUserState;
            
            // Since this is our default handler anything invalid will
            // run through here. No path - go to new
            if (string.IsNullOrEmpty(id) || id == "0")
                return this.New();

            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                var snippet = busSnippet.Load(id);
                if (snippet == null)
                {
                    return this.DisplayErrorPage("Invalid Snippet Id specified",
                        "You specified a snippet id or link that is invalid and cannot be displayed. " +
                        "Please using the <a href='./recent' class='hoverbutton'>Recent Snippets</a> or " +
                        "<a href='mysnippets' class='hoverbutton'>My Snippets</a> buttons to look up valid snippets.", null);
                }

                bool allowWordWrap = false;
                bool showLineNumbers = busSnippet.Entity.ShowLineNumbers;

                string ua = Request.UserAgent.ToLower();
                if (ua.Contains("iphone") ||
                    ua.Contains("blackberry") ||
                    ua.Contains("mobile"))
                {
                    allowWordWrap = true;
                    showLineNumbers = false;
                }
                

                // Update the code so it's formatted
                model.FormattedCode = busSnippet.Entity.FormattedCode;
                if (!AppUserState.IsEmpty())
                    model.IsFavoritedByUser = busSnippet.IsFavorite(busSnippet.Entity.Id, AppUserState.UserId);


                if (!string.IsNullOrEmpty(AppUserState.UserId) &&
                    AppUserState.UserId == busSnippet.Entity.UserId || AppUserState.IsAdmin)
                    model.AllowEdit = true;

                // explicitly load up comments
                busSnippet.Entity.Comments = busSnippet.GetComments();

                // For API result we have to make sure email and password are not included            
                if (!string.IsNullOrEmpty(Format) && snippet.User != null)
                {
                    busSnippet.StripSensitiveUserInformation();  
                }
                if (snippet.User != null)
                {
                    if (!string.IsNullOrEmpty(snippet.User.Theme))
                        model.Theme = snippet.User.Theme;
                }

                ActionResult actionResult = this.ApiResult(busSnippet.Entity);
                if (actionResult != null)
                    return actionResult;

                model.Snippet = busSnippet.Entity;

                // Fix up for Ace Editor
                model.Snippet.Language = busSnippet.FixUpLanguage(model.Snippet.Language).ToLower();

                // Log views for all but poster
                if (model.Snippet.User == null ||
                    model.Snippet.User.Id != AppUserState.UserId)
                    busSnippet.LogSnippetView(busSnippet.Entity.Id, Request.UserHostAddress, Request.UserAgent);

                return View("Show",model);
            }
        }

        /// <summary>
        /// Displays a snippet as raw HTML
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ShowHtml(string id)
        {
            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                var snippet = busSnippet.Load(id);

                if (snippet == null)
                    return new HttpNotFoundResult();

                if (snippet.Language.ToLower() != "html")
                    return new HttpNotFoundResult("Invalid snippet type");

                return this.Content(snippet.Code);
            }
        }


        public ActionResult CodeOnly(string id)
        {
            ShowSnippetViewModel model = new ShowSnippetViewModel(this);
            model.AppUserState = this.AppUserState;

            // Since this is our default handler anything invalid will
            // run through here. No path - go to new
            if (string.IsNullOrEmpty(id) || id == "0")
                return this.New();

            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                if (busSnippet.Load(id) == null)
                {
                    ErrorDisplay.ShowError("Invalid snippet id specified.");
                    model.Snippet = new CodeSnippet();
                    return View(model);
                }

                model.Snippet = busSnippet.Entity;

                // Update the code so it's formatted
                model.FormattedCode = busSnippet.Entity.FormattedCode;

                if (!string.IsNullOrEmpty(AppUserState.UserId) && AppUserState.UserId == busSnippet.Entity.UserId || AppUserState.IsAdmin)
                    model.AllowEdit = true;

                ActionResult result = View(model);
                string output = result.ToString();

                return result;
            }
        }

        public ActionResult ShowUrl()
        {
            string url = Request.QueryString["url"];
            string lang = Request.QueryString["language"];
            if (string.IsNullOrEmpty(lang))
                lang = Request.QueryString["lang"] ?? string.Empty;

            if (lang.ToLower() == "csharp")
                lang = "C#";                

            ShowSnippetViewModel model = new ShowSnippetViewModel(this);
            model.AppUserState = this.AppUserState;

            ViewData["originalUrl"] = url;
            ViewData["fileName"] = Path.GetFileName(url);
            ViewData["language"] = lang;

            if (string.IsNullOrEmpty(url))
            {
                ViewData["languageList"] = this.GetLanguageList(lang);
                return View(model);
            }

            HttpClient client = new HttpClient();
            client.Timeout = 4000;
            
            string result = client.DownloadString(url);

            if (result == null)            
                return 
                    this.DisplayErrorPage("Unable to retrieve Code Url", client.ErrorMessage, null);

            if (result.Length > App.Configuration.MaxCodeLength)
                return this.DisplayErrorPage("Snippet is too large", "Your code snippet to display is too long. Snippets can be no larger than " + App.Configuration.MaxCodeLength.ToString("n0") + " bytes.",null);

            busCodeSnippet snippetBusiness = new busCodeSnippet();

            if (string.IsNullOrEmpty(lang))
            {
                string extension = Path.GetExtension(url).ToLower();
                
                if (extension.StartsWith("."))
                    lang = extension.Substring(1);
            }

            model.FormattedCode = snippetBusiness.GetFormattedCode(result, lang, false, false);
            
            snippetBusiness.Dispose();

            return this.View(model);
        }
        

        public ActionResult New(string message = null)
        {
            ViewData["UserState"] = AppUserState;

            var snippet = new CodeSnippet();
            snippet.Author = this.AppUserState.Name;

            string codeUrl = Request.QueryString["url"];            

            if (!string.IsNullOrEmpty(codeUrl))
            {                
                HttpClient client = new HttpClient();
                client.Timeout = 4000;
                snippet.Code = client.DownloadString(codeUrl);

                snippet.Title = Path.GetFileName(codeUrl); 
                string extension = Path.GetExtension(codeUrl);

                snippet.Language = CodeFormatFactory.GetStringLanguageFromExtension(extension);    
                Response.Write(snippet.Language);
            }

            if (!string.IsNullOrEmpty(message))            
                this.ErrorDisplay.ShowMessage(message);
            
            return this.View("New",snippet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult New(FormCollection formValues)
        {
            ViewData["UserState"] = AppUserState;
            ViewData["languageList"] = this.GetLanguageList();

            busCodeSnippet busSnippet = new busCodeSnippet();
            CodeSnippet snippet = busSnippet.NewEntity();
            if (snippet == null)
            {
                ErrorDisplay.ShowError("Couldn't load snippet");
                return View(new CodeSnippet());
            }

            UpdateModel(snippet);                      

            if (!ValidateForSpam(snippet))
            {
                this.ErrorDisplay.ShowError("Invalid data posted back.");
                return View(snippet);
            }
            
            if (!busSnippet.Validate())
            {
                foreach (ValidationError error in busSnippet.ValidationErrors)
                {
                    this.ErrorDisplay.AddMessage(error.Message, error.ControlID);
                }
                this.ErrorDisplay.ShowError("Please correct the following:");
                return View(snippet);
            }

            // Assign the user from the authenticated user if any - otherwise blank
            // in which case the user can't edit entries.
            snippet.UserId = this.AppUserState.UserId;

            // 
            if (!string.IsNullOrEmpty(snippet.UserId))
            {
                var userBus = new busUser();
                var user = userBus.Load(snippet.UserId);
                if (user.InActive)
                {
                    ErrorDisplay.HtmlEncodeMessage = false;
                    ErrorDisplay.ShowError(
@"Your email address has not been validated yet so you
can't create posts for this account yet. Please click 
the following link and then check your email for a
message to validate your email.<br><br>
<a href='" +  Url.Content("~/Account/ResetEmailValidation") + 
"' target='emailreset'>Send Validation Request Email</a>");

                    return View(snippet);
                }
            }

            // strip of leading indentation always when capturing a new snippet
            snippet.Code = StringUtils.NormalizeIndentation(snippet.Code);                            
                       
            if (!busSnippet.Save())
            {
                this.ErrorDisplay.ShowError("Couldn't save snippet: " + busSnippet.ErrorMessage);
                return View(snippet);
            }

            return this.RedirectToAction("Show", new { id = busSnippet.Entity.Id });
        }

        private bool ValidateForSpam(CodeSnippet snippet)
        {
            string strLength = Request.Form["qx"];
            if (string.IsNullOrEmpty(strLength))
                return false;
            
            int length = -1;
            int.TryParse(strLength,out length);

            //  Simplistic form of validation
            if (snippet.Code.Replace("\r\n","\r").Length != length)
                return false;

            return true;
        }
        
        /// <summary>
        /// /list/recent
        /// /list/tag/C#        
        /// </summary>
        /// <param name="listAction"></param>
        /// <param name="listFilter"></param>
        /// <returns></returns>        
        public ActionResult List(string listAction, string listFilter)
        {
            if (listAction == "MySnippets")
                return MySnippets();
            else if (listAction == "MyFavorites")
                return MyFavorites();

            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {                
                var snippetList = busSnippet.GetSnippetList(listAction, listFilter);
                busSnippet.Dispose();


                //if (listAction == "recent")
                //{
                //    // keep the snippetlist short
                //    if (snippetList != null)
                //        snippetList = snippetList.Take(20).ToList();
                //}
                this.ViewData["busSnippet"] = busSnippet;
                this.ViewData["SnippetList"] = snippetList;

                if (listAction == "tag")
                {
                    this.ViewData["PageTitle"] = "Recent snippets matching tags of " + listFilter;
                    if (snippetList.Count < 1)
                    {
                        Response.StatusCode = 404;
                        ErrorController.ShowErrorPage("Invalid tag", "You've entered a tag that is not valid or has no related entries.");
                    }
                }
                else if (listAction == "language")
                    this.ViewData["PageTitle"] = "Recent snippets matching language of " + listFilter;
                else if (listAction == "user")
                {
                    if (snippetList.Count > 0)
                        this.ViewData["PageTitle"] = "Recent snippets for: " + snippetList.First().Author;
                    else
                        this.ViewData["PageTitle"] = "Recent snippets for user: " + listFilter;
                }

                ActionResult actionResult = this.ApiResult(snippetList);
                if (actionResult != null)
                    return actionResult;

                return View("List");
            }
        }
        
        // caching won't work since showing user highlighted snippets
        // for now not an issue - in future have to address this
        //[OutputCache(Duration=90,VaryByParam="format")]  

        /// <summary>
        /// /recent
        /// </summary>
        /// <returns></returns>
        public ActionResult Recent()
        {
            this.ViewData["PageTitle"] = "Recent Snippets"; 
            return this.List("recent","");           
        }
        

        public ActionResult MySnippets()
        {            
            if (string.IsNullOrEmpty(this.AppUserState.UserId))
            {
                List<CodeSnippetListItem> snippetListEmpty = new List<CodeSnippetListItem>();
                this.ViewData["SnippetList"] = snippetListEmpty;
                this.ErrorDisplay.ShowError("You have to log in to see your snippets.");
                return View();
            }

            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                this.ViewData["busSnippet"] = busSnippet;
                var snippetList = busSnippet.GetSnippetsForUser(this.AppUserState.UserId);
                this.ViewData["SnippetList"] = snippetList;
                this.ViewData["PageTitle"] = "My Snippets";
                ActionResult actionResult = this.ApiResult(snippetList);
                if (actionResult != null)
                    return actionResult;
            }

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult MyFavorites()
        {
            if (string.IsNullOrEmpty(this.AppUserState.UserId))
            {
                List<CodeSnippetListItem> snippetListEmpty = new List<CodeSnippetListItem>();
                this.ViewData["SnippetList"] = snippetListEmpty;
                this.ErrorDisplay.ShowError("You have to log in to see your favorites.");
                return View();
            }

            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                this.ViewData["busSnippet"] = busSnippet;
                var snippetList = busSnippet.GetFavorites(this.AppUserState.UserId);
                this.ViewData["SnippetList"] = snippetList;
                this.ViewData["PageTitle"] = "My Favorites";
                ActionResult actionResult = this.ApiResult(snippetList);
                if (actionResult != null)
                    return actionResult;
            }

            return View();
        }




        //public ActionResult Search()
        //{
        //    ListSnippetViewModel model = new ListSnippetViewModel(this);
        //    model.UserState = this.UserState;
        //    model.ErrorDisplay = this.ErrorDisplay;
        //    model.PageTitle = "Search Code Snippets";

        //    busCodeSnippet busSnippet = new busCodeSnippet();

        //    model.Controller = this;
        //    model.busSnippet = busSnippet;
        //    model.SnippetList = new List<CodeSnippetListItem>();

        //    model.Parameters = new CodeSnippetSearchParameters();

        //    List<SelectListItem> searchOrderItems =
        //        (from order in Enum.GetNames(typeof(SearchOrderTypes))
        //         select new SelectListItem { Value = order, Text = order, Selected = order == "Entered" }).ToList();
        //    model.SearchOrderItems = searchOrderItems;

        //    return View("Search",model);        
        //}

        [AcceptVerbs(HttpVerbs.Post | HttpVerbs.Get)]
        [ValidateInput(false)]
        public ActionResult Search(FormCollection formVars)
        {
            ListSnippetViewModel model = new ListSnippetViewModel(this);
            model.AppUserState = this.AppUserState;
            model.ErrorDisplay = this.ErrorDisplay;
            model.PageTitle = "Search Code Snippets";
            
            model.SearchOrderItems = AppWebUtils.GetSelectListFromEnum(typeof(SearchOrderTypes), "Entered");

            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                model.Controller = this;
                model.busSnippet = busSnippet;

                model.Parameters = new CodeSnippetSearchParameters();

                this.TryUpdateModel(model.Parameters);

                var snippetList = busSnippet.GetSearchList(model.Parameters);
                int snippetListCount = 0;
                if (snippetList != null)
                    snippetListCount = snippetList.Count();

                if (formVars.Count > 0)
                {
                    if (snippetList == null)
                        this.ErrorDisplay.ShowError("Please provide at least one search criterion.");
                    else if (snippetListCount < 1)
                        this.ErrorDisplay.ShowError("No matches found for your search criteria.");
                }

                if (snippetList != null)
                {
                    model.Paging = new PagingDetails();
                    model.Paging.PageCount = (int)Math.Ceiling(Convert.ToDecimal(snippetListCount) / Convert.ToDecimal(model.Paging.PageSize));

                    int.TryParse(Request.Params["page"] ?? "1", out model.Paging.Page);

                    if (model.Paging.Page > 0 && snippetListCount > model.Paging.PageSize)
                    {
                        snippetList = snippetList.Skip((model.Paging.Page - 1) * model.Paging.PageSize)
                                                 .Take(model.Paging.PageSize);
                    }
                    model.SnippetList = snippetList.ToList();
                }
                else
                    model.SnippetList = new List<CodeSnippetListItem>();


                ActionResult result = this.ApiResult(model.SnippetList);
                if (result != null)
                    return result;
            }

            return View("Search",model);
        }



        private List<SelectListItem> GetLanguageList()
        {
            return 
                (from lang in App.CodeLanguages                  
                  select new SelectListItem()
                  {
                      Text = lang.Value,
                      Value = lang.Key                     
                  }).ToList();             
        }

        private List<SelectListItem> GetLanguageList(string selectedValue)
        {
            return
                (from lang in App.CodeLanguages
                 select new SelectListItem()
                 {
                     Text = lang.Value,
                     Value = lang.Key,
                     Selected = lang.Value == selectedValue
                 }).ToList();
        }
    }

}
