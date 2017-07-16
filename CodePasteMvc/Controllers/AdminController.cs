using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using Westwind.Utilities;
using Westwind.Web.Mvc;
using CodePasteBusiness;
using Microsoft.CSharp;

namespace CodePasteMvc.Controllers
{
    public class AdminController : ApiControllerBase
    {
        //
        // GET: /Admin/
        UsersViewModel ViewModel = new UsersViewModel();
        busUser busUser = null;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            ViewModel.ErrorDisplay = ErrorDisplay;
            ViewModel.AppUserState = AppUserState;
            busUser = CodePasteFactory.GetUser();

        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);


            ActionResult result = null;
            if (!AppUserState.IsAdmin)
                result = DisplayErrorPage("Access Denied", "This area of the site requires Admin access. Please <a href='" +
                                               Url.Content("~/account/logon") + "'>log in</a> first.","~/");

            filterContext.Result = result;            
        }

        public ActionResult Index()
        {
            PageEnvironment environment = new PageEnvironment();           
            return View(ViewModel);
        }

        
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Users(FormCollection formVars)
        {
            if (formVars == null || formVars.Count == 0)
            {
                ViewModel.UserList = busUser.GetUserList(null);             
            }

            ActionResult action = ApiResult(ViewModel.UserList);
            if (action != null)
                return action;

            return View(ViewModel);
        }


        public ActionResult AbuseSnippets()
        {
            var busSnippet = new busCodeSnippet();

            ViewBag.busSnippet = busSnippet;

            ViewModel.SnippetList = busSnippet.GetAbuseReportedSnippets();

            return View(ViewModel);
        }


        public ActionResult ShrinkDataBase()
        {
            busAdministration admin = new busAdministration();

            if (!admin.ShrinkDatabase())
                ErrorDisplay.ShowError(admin.ErrorMessage);
            else
                ErrorDisplay.ShowMessage("Database has been shrunk");

            return View("Index", ViewModel);
        }

        public ActionResult UpdateFormattedCode()
        {
            busAdministration admin = new busAdministration();
            if (!admin.UpdateFormattedCode())
                ErrorDisplay.ShowError(admin.ErrorMessage);
            else
                ErrorDisplay.ShowMessage("Snippets have been updated...");
            
            return View("Index", ViewModel);
        }

        [HttpGet]
        public ActionResult DeleteSpam()
        {
            var model = new DeleteSpamViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteSpam(DeleteSpamViewModel model)
        {
            if (!string.IsNullOrEmpty(Request.Form["btnDelete"]))
                return DeleteSpamItems(model);

            var busAdmin = new busAdministration();
            model.Snippets = busAdmin.GetSpamKeywords(model.SearchTerm);


            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteSpamItems(DeleteSpamViewModel model)
        {
            var snipKeys = Request.Form.AllKeys.Where(ky => ky.StartsWith("snippet_")).ToArray();

            for (int index = 0; index < snipKeys.Length; index++)
            {                
                snipKeys[index] =snipKeys[index].Replace("snippet_", "");
            }

            var adminBus = new busAdministration();

            if (!adminBus.DeleteSnippets(snipKeys))
                ErrorDisplay.ShowError(adminBus.ErrorMessage);
            else
                ErrorDisplay.ShowMessage(snipKeys.Length + " snippets deleted.");

            var busAdmin = new busAdministration();
            model.Snippets = busAdmin.GetSpamKeywords(model.SearchTerm);

            return View("DeleteSpam",model);
        }


        public ActionResult Configuration()
        {
            ConfigurationViewModel model = new ConfigurationViewModel();        
            model.DebugModeList = AppWebUtils.GetSelectListFromEnum(typeof(DebugModes),App.Configuration.DebugMode.ToString());
                       
            return View(model);
        }


        public ActionResult ClearAnonymousSnippets()
        {
            busCodeSnippet codeSnippet = CodePasteFactory.GetCodeSnippet();
            int result = codeSnippet.ClearAnonymousSnippets(App.Configuration.HoursToDeleteAnonymousSnippets,
                                                    App.Configuration.MinViewBeforeDeleteAnonymousSnippets) ;
            if (result < 0)
                ErrorDisplay.ShowError(codeSnippet.ErrorMessage);
            else
                ErrorDisplay.ShowMessage((result).ToString() + " old snippets have been cleared out.");

            return View("Index", ViewModel);
        }



        public ActionResult DatabaseHousekeeping()
        {
            busAdministration codeSnippet = CodePasteFactory.GetAdministration();
            int result = codeSnippet.DatabaseHouseKeeping();

            if (result < 0)
                ErrorDisplay.ShowError(codeSnippet.ErrorMessage);
            else
            {
                if (result < 0)
                    ErrorDisplay.ShowMessage("Database housekeeping failed: " + codeSnippet.ErrorMessage);
                else
                    ErrorDisplay.ShowMessage((result).ToString() + " snippets have been cleared out.");
            }

            return View("Index", ViewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Configuration(FormCollection formVars)
        {
            TryUpdateModel(App.Configuration);

            if (App.Configuration.Write())
                ErrorDisplay.ShowMessage("Configuration values written.");
            else
                ErrorDisplay.ShowError("Configuration values couldn't be saved:" + App.Configuration.ErrorMessage);

            return Configuration();
            
        }

        private ActionResult AdminRequired()
        {
            if (!AppUserState.IsAdmin)
                return DisplayErrorPage("Access Denied", "This area of the site requires Admin access.", "~/");

            return null;
        }

        public ActionResult UpdatePasswords()
        {
            var userBus = new busUser();
            foreach (var user in userBus.Context.Users)
            {
                userBus.Load(user.Id);
                userBus.Save(user);
            }

            ViewModel.ErrorDisplay.ShowMessage("User accounts have been updated.");
            return View("Index", ViewModel);
        }
    }


    public class UsersViewModel
    {
        public ErrorDisplay ErrorDisplay = null;
        public AppUserState AppUserState = null;
        public IEnumerable<User> UserList = null;
        public IEnumerable<CodeSnippet> SnippetList = null;
    }

    public class ConfigurationViewModel
    {
        public ApplicationConfiguration Configuration = App.Configuration;
        public List<SelectListItem> DebugModeList = null;
    }
}
