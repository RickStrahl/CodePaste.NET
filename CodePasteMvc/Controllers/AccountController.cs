using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CodePasteBusiness;
using Westwind.Web.Mvc;
using Westwind.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using FormCollection = System.Web.Mvc.FormCollection;

namespace CodePasteMvc.Controllers
{

    [HandleError]
    public class AccountController : baseController
    {
        private AccountViewModel ViewModel = new AccountViewModel();
        public busUser busUser = null;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            busUser = CodePasteFactory.GetUser();
            ViewModel.busUser = busUser;
            ViewModel.ErrorDisplay = ErrorDisplay;
            ViewModel.AppUserState = AppUserState;

            ViewData["UserState"] = AppUserState;
        }

        protected override void Dispose(bool disposing)
        {
            if (busUser != null)
            {
                busUser.Dispose();
                busUser = null;
            }
            base.Dispose(disposing);
        }


        #region Indivdual User Cookie Logins

        // This constructor is used by the MVC framework to instantiate the controller using
        // the default forms authentication and membership providers.
        public ActionResult LogOn(string message = null)
        {
            if (!string.IsNullOrEmpty(message))
                ViewModel.ErrorDisplay.ShowError(message);

            return View("LogOn", ViewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult LogOn(string email, string password, bool rememberMe, string returnUrl, bool emailPassword)
        {
            if (emailPassword)
            {
                if (ModelState.ContainsKey("emailpassword"))
                    ModelState.Remove("emailpassword");
                return EmailPassword(email);
            }

            var user = busUser.ValidateUserAndLoad(email, password);
            if (user == null)
            {
                ErrorDisplay.ShowError(busUser.ErrorMessage);
                return View(ViewModel);
            }

            AppUserState appUserState = new AppUserState()
            {
                Email = user.Email,
                Name = user.Name,
                UserId = user.Id,
                Theme = user.Theme,
                IsAdmin = user.IsAdmin
            };

            //IssueAuthTicket(appUserState, rememberMe);

            IdentitySignin(appUserState, user.OpenId, rememberMe);

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            returnUrl = WebUtils.ResolveServerUrl("~/new");
            return Redirect(returnUrl);

            //return RedirectToAction("New", "Snippet", null);
        }

        public ActionResult LogOff()
        {
            IdentitySignout();
            return RedirectToAction("LogOn");
        }

        public ActionResult Register(string id)
        {
            ViewData["IsNew"] = false;

            if (string.IsNullOrEmpty(id))
                id = AppUserState.UserId;

            if (string.IsNullOrEmpty(id))
            {
                busUser.NewEntity();
                ViewData["IsNew"] = true;
            }
            else
            {
                if (id != AppUserState.UserId)
                    return RedirectToAction("Register", new {id = ""});

                if (busUser.Load(id) == null)
                    return RedirectToAction("LogOn", "Account", new {returnUrl = Url.Action("Register", "Account")});
            }

            return View("Register", ViewModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Register(FormCollection formVars)
        {
            string id = formVars["Id"];

            if (!string.IsNullOrEmpty(formVars["btnDeleteAccount"]))
            {
                if (string.IsNullOrEmpty(AppUserState.UserId))
                    return View("Register", ViewModel);

                if (!busUser.Delete(AppUserState.UserId))
                    ViewModel.ErrorDisplay.ShowError("Unable to delete this account: " + busUser.ErrorMessage);
                else
                {
                    IdentitySignout();
                    return RedirectToAction("New", "Snippet");
                }

                return View("Register", ViewModel);
            }

            ViewData["IsNew"] = false;

            string confirmPassword = formVars["confirmPassword"];

            bool isNew = false;
            User user = null;
            if (string.IsNullOrEmpty(id) || busUser.Load(id) == null)
            {
                user = busUser.NewEntity();
                ViewData["IsNew"] = true;

                // not validated yet
                user.InActive = true;
                isNew = true;
            }
            else
                user = busUser.Entity;

            UpdateModel<User>(busUser.Entity,
                new string[] {"Name", "Email", "Password", "Theme"});

            if (ModelState.Count > 0)
                ErrorDisplay.AddMessages(ModelState);

            if (string.IsNullOrEmpty(user.OpenId) &&
                confirmPassword != user.Password)
                ErrorDisplay.AddMessage("Please make sure both password values match.", "confirmPassword");


            if (ErrorDisplay.DisplayErrors.Count > 0)
                return View("Register", ViewModel);

            if (!busUser.Validate())
            {
                ErrorDisplay.Message = "Please correct the following:";
                ErrorDisplay.AddMessages(busUser.ValidationErrors);
                return View("Register", ViewModel);
            }

            if (!busUser.Save())
            {
                ErrorDisplay.ShowError("Unable to save User: " + busUser.ErrorMessage);
                return View("Register", ViewModel);
            }

            AppUserState appUserState = new AppUserState();
            appUserState.FromUser(user);
            IdentitySignin(appUserState, appUserState.UserId);

            if (isNew)
            {
                SetAccountForEmailValidation();

                ErrorDisplay.HtmlEncodeMessage = false;
                ErrorDisplay.ShowMessage(
                    @"Thank you for creating an account...
<hr />
<p>Before you can post and save new CodePastes we need to
verify your email address.</p>
<p>We just sent you an email with a confirmation
code. Please follow the instructions in the email 
to validate your email address.</p>");

                return View("Register", ViewModel);
            }


            return RedirectToAction("New", "Snippet", null);
        }

        #endregion



        #region External Provider Logins

        // POST: /Account/ExternalLogin
        [AllowAnonymous]        
        [HttpPost]        
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider)
        {
            string returnUrl = WebUtils.ResolveServerUrl("~/new");
            //string returnUrl = Url.Action("New", "Snippet", null);

            return new ChallengeResult(provider,
                Url.Action("ExternalLoginCallback", "Account", 
                new {ReturnUrl = returnUrl}));
        }


        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = "~/";

            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
                return RedirectToAction("LogOn");

            // AUTHENTICATED!
            var providerKey = loginInfo.Login.ProviderKey;


            // Aplication specific code goes here.
            var userBus = new busUser();
            var user = userBus.ValidateUserWithExternalLogin(providerKey);
            if (user == null)
            {
                return RedirectToAction("LogOn", new
                {
                    message = "Unable to log in with " + loginInfo.Login.LoginProvider +
                              ". " + userBus.ErrorMessage
                });
            }

            // store on AppUser
            AppUserState appUserState = new AppUserState();
            appUserState.FromUser(user);

            // write the authentication cookie
            IdentitySignin(appUserState, providerKey, isPersistent: true);

            return Redirect(returnUrl);
        }

        // Initiate oAuth call for external Login
        // GET: /Account/ExternalLinkLogin
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLinkLogin(string provider)
        {
            var id = Request.Form["Id"];

            // create an empty AppUser with a new generated id
            AppUserState.UserId = id;
            AppUserState.Name = "";
            IdentitySignin(AppUserState, null);

            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("ExternalLinkLoginCallback"), AppUserState.UserId);
        }

// oAuth Callback for external login
// GET: /Manage/LinkLogin
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> ExternalLinkLoginCallback()
        {
            // Handle external Login Callback
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, AppUserState.UserId);
            if (loginInfo == null)
            {
                IdentitySignout(); // to be safe we log out
                return RedirectToAction("Register", new {message = "Unable to authenticate with external login."});
            }

            // Authenticated!
            string providerKey = loginInfo.Login.ProviderKey;
            string providerName = loginInfo.Login.LoginProvider;

            // Now load, create or update our custom user

            // normalize email and username if available
            if (string.IsNullOrEmpty(AppUserState.Email))
                AppUserState.Email = loginInfo.Email;
            if (string.IsNullOrEmpty(AppUserState.Name))
                AppUserState.Name = loginInfo.DefaultUserName;

            var userBus = new busUser();
            User user = null;

            if (!string.IsNullOrEmpty(AppUserState.UserId))
                user = userBus.Load(AppUserState.UserId);

            if (user == null && !string.IsNullOrEmpty(providerKey))
                user = userBus.LoadUserByProviderKey(providerKey);

            if (user == null && !string.IsNullOrEmpty(loginInfo.Email))
                user = userBus.LoadUserByEmail(loginInfo.Email);

            if (user == null)
            {
                user = userBus.NewEntity();
                userBus.SetUserForEmailValidation(user);
            }

            if (string.IsNullOrEmpty(user.Email))
                user.Email = AppUserState.Email;

            if (string.IsNullOrEmpty(user.Name))
                user.Name = AppUserState.Name ?? "Unknown (" + providerName + ")";


            if (loginInfo.Login != null)
            {
                user.OpenIdClaim = loginInfo.Login.ProviderKey;
                user.OpenId = loginInfo.Login.LoginProvider;
            }
            else
            {
                user.OpenId = null;
                user.OpenIdClaim = null;
            }

            // finally save user inf
            bool result = userBus.Save(user);

            // update the actual identity cookie
            AppUserState.FromUser(user);
            IdentitySignin(AppUserState, loginInfo.Login.ProviderKey);

            return RedirectToAction("Register");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalUnlinkLogin()
        {
            var userId = AppUserState.UserId;
            var user = busUser.Load(userId);
            if (user == null)
            {
                ErrorDisplay.ShowError("Couldn't find associated User: " + busUser.ErrorMessage);
                return RedirectToAction("Register", new {id = userId});
            }
            user.OpenId = string.Empty;
            user.OpenIdClaim = string.Empty;

            if (busUser.Save())
                return RedirectToAction("Register", new {id = userId});

            return RedirectToAction("Register", new {message = "Unable to unlink OpenId. " + busUser.ErrorMessage});
        }



        // **** Helpers 

// Used for XSRF protection when adding external logins
        private const string XsrfKey = "CodePaste_$31!.2*#";

        public class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties {RedirectUri = RedirectUri};
                if (UserId != null)
                    properties.Dictionary[XsrfKey] = UserId;

                var owin = context.HttpContext.GetOwinContext();
                owin.Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion

        #region SignIn and Signout

        /// <summary>
        /// Helper method that adds the Identity cookie to the request output
        /// headers. Assigns the userState to the claims for holding user
        /// data without having to reload the data from disk on each request.
        /// 
        /// AppUserState is read in as part of the baseController class.
        /// </summary>
        /// <param name="appUserState"></param>
        /// <param name="providerKey"></param>
        /// <param name="isPersistent"></param>
        public void IdentitySignin(AppUserState appUserState, string providerKey = null, bool isPersistent = false)
        {
            var claims = new List<Claim>();

            // create *required* claims
            claims.Add(new Claim(ClaimTypes.NameIdentifier, appUserState.UserId));
            claims.Add(new Claim(ClaimTypes.Name, appUserState.Name));

            // serialized AppUserState object
            claims.Add(new Claim("userState", appUserState.ToString()));

            var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

            // add to user here!
            AuthenticationManager.SignIn(new AuthenticationProperties()
            {
                AllowRefresh = true,
                IsPersistent = isPersistent,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            }, identity);
        }

        public void IdentitySignout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie,
                DefaultAuthenticationTypes.ExternalCookie);
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        #endregion


        public ActionResult EmailPassword(string email)
        {
            User user = busUser.LoadUserByEmail(email);
            if (user == null)
                ErrorDisplay.ShowError(
                    "Email address doesn't exist. Please make sure you have typed the address correctly");
            else
            {
                // Always create a new random password
                string password = StringUtils.NewStringId();
                user.Password = App.EncodePassword(password, user.Id);
                busUser.Save();

                if (AppWebUtils.SendEmail(App.Configuration.ApplicationTitle + " Email Information",
                    "Your CodePaste account password is: " + password + "\r\n\r\n" +
                    "You can log on at: " + WebUtils.GetFullApplicationPath() + "\r\n\r\n" +
                    "Please log in, view your profile and then change your password to something you can more easily remember.",
                    busUser.Entity.Email,
                    true))
                    ErrorDisplay.ShowMessage("Password information has been emailed to: " + busUser.Entity.Email);
                else
                    ErrorDisplay.ShowError("Emailing of password failed.");
            }

            return View(ViewModel);
        }


        //[AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ValidateEmail(string id = null)
        {
            var user = busUser.ValidateEmailAddress(id);
            if (user == null)
                throw new ApplicationException("Invalid email validator id.");

            return RedirectToAction("New", "Snippet",
                new {message = "You're ready to post: Your email address is validated."});
        }

        public ActionResult ResetEmailValidation()
        {
            if (string.IsNullOrEmpty(AppUserState.UserId))
                return new HttpUnauthorizedResult();

            var user = busUser.Load(AppUserState.UserId);

            SetAccountForEmailValidation();

            ErrorDisplay.ShowMessage(
                @"An email has been sent to validate your email address.
Please follow the instructions in the email to validate
your email address.");

            return Register(user.Id);
        }


        /// <summary>
        /// Makes user account inactive, sets a new validator
        /// and then emails the user.
        /// Assume busUser.Entity is set.
        /// </summary>        
        private void SetAccountForEmailValidation()
        {
            busUser.SetUserForEmailValidation();
            busUser.Save();

            var msg = string.Format(
                @"In order to validate your email address for CodePaste.net, please click or paste the
following link into your browser's address bar: 

{0}

Once you click the link you're ready to create new
code pastes with your account on the CodePaste.net site.

Sincerely,

The CodePaste.net Team
", WebUtils.ResolveServerUrl("~/Account/ValidateEmail/" + busUser.Entity.Validator));

            AppWebUtils.SendEmail("Codepaste.net Email Validation",
                msg, busUser.Entity.Email, true);
        }


    }



    public class AccountViewModel
    {
        public ErrorDisplay ErrorDisplay = null;
        public busUser busUser = null;
        public AppUserState AppUserState = null;
    }

}
