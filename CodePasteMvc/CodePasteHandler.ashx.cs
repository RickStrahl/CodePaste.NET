using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Westwind.Web;
using CodePasteBusiness;
using System.Web.Security;
using Westwind.Utilities;
using System.Web.Mvc;

namespace CodePasteMvc
{

    /// <summary>
    /// Using West Wind Web Toolkit's Callback Handler and ajaxMethodCallback() (ww.jquery.js)
    /// to make easy callbacks
    /// </summary>
    public class CodePasteHandler : CallbackHandler
    {        
        /// <summary>
        /// Automatically retrieve userstate from FormsAuthentication
        /// </summary>
        public AppUserState AppUserState
        {
            get 
            {
                if (_appUserState == null)
                {
                    _appUserState = new AppUserState();
                    if (HttpContext.Current.User == null || HttpContext.Current.User.Identity == null)
                        return _appUserState;

                    if (HttpContext.Current.User != null && HttpContext.Current.User.Identity is ClaimsIdentity)
                    {
                        var user = HttpContext.Current.User.Identity as ClaimsIdentity;
                        if (user != null)
                        {   
                            var claim = user.Claims.FirstOrDefault(c => c.Type == "userState");
                            if (claim != null)
                            {
                                var userStateString = claim.Value;
                                if (!string.IsNullOrEmpty(userStateString))
                                    _appUserState.FromString(userStateString);
                            }
                        }
                    }
                }

                return _appUserState; 
            }
        }
        private AppUserState _appUserState = null;

        /// <summary>
        /// Saves the title from the snippet page in real time editing
        /// </summary>
        /// <param name="snippetId"></param>
        /// <param name="newTitle"></param>
        /// <returns></returns>
        [CallbackMethod]
        public string SaveTitle(string snippetId,string newTitle)
        {
            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                if (busSnippet.Load(snippetId) == null)
                    throw new ArgumentException("Invalid snippetId passed.");
                if (!IsEditAllowed(busSnippet.Entity) && !AppUserState.IsAdmin)
                    throw new AccessViolationException("You are not allowed to edit this snippet.");
                busSnippet.Entity.Title = newTitle;
                if (!busSnippet.Validate())
                    throw new InvalidOperationException(busSnippet.ErrorMessage);
                if (!busSnippet.Save())
                    throw new InvalidOperationException(busSnippet.ErrorMessage);
                return !string.IsNullOrEmpty(busSnippet.Entity.Title) ? busSnippet.Entity.Title : "No Title";
            }
        }

        /// <summary>
        /// Updates code edited on the client
        /// </summary>
        /// <param name="snippetId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [CallbackMethod]
        public string SaveCode(string snippetId, string code )
        {
            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                if (busSnippet.Load(snippetId) == null)
                    throw new ArgumentException("Invalid snippetId passed.");
                if (!IsEditAllowed(busSnippet.Entity) && !this.AppUserState.IsAdmin)
                    throw new AccessViolationException("You are not allowed to edit this snippet.");
                busSnippet.Entity.Code = StringUtils.NormalizeIndentation(code);

                if (busSnippet.IsSpam())
                    throw new InvalidOperationException("Invalid content.");

                if (!busSnippet.Save())
                    throw new InvalidOperationException("Unable to save snippet: " + busSnippet.ErrorMessage);
            }
            return "ok";
        }


        [CallbackMethod]
        public string ShowLineNumbers(string snippetId, bool show)
        {
            return "ok";
        }

        /// <summary>
        /// Updates the language edited on the client
        /// </summary>
        /// <param name="snippetId"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        [CallbackMethod]
        public string SaveLanguage(string snippetId, string lang)
        {
            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                if (busSnippet.Load(snippetId) == null)
                    throw new ArgumentException("Invalid snippetId passed.");
                if (!IsEditAllowed(busSnippet.Entity) && !AppUserState.IsAdmin)
                    throw new AccessViolationException("You are not allowed to edit this snippet.");
                busSnippet.Entity.Language = lang;
                if (!busSnippet.Save())
                    throw new InvalidOperationException("Unable to save snippet: " + busSnippet.ErrorMessage);
                return "ok";
            }
        }

        /// <summary>
        /// Saves the tag value from a user and returns the tag values entered
        /// as a linked string
        /// </summary>
        /// <param name="snippetId"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        [CallbackMethod]
        public string SaveTags(string snippetId, string tags)
        {
            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                if (busSnippet.Load(snippetId) == null)
                    throw new ArgumentException("Invalid snippetId passed.");
                if (!IsEditAllowed(busSnippet.Entity) && !AppUserState.IsAdmin)
                    throw new AccessViolationException("You are not allowed to edit this snippet.");
                busSnippet.Entity.Tags = tags;
                if (!busSnippet.Save())
                    throw new InvalidOperationException("Unable to save snippet: " + busSnippet.ErrorMessage);
                string tagResult = busSnippet.GetTagLinkList(tags);
                return tagResult;
            }
        }

        [CallbackMethod]
        public string SaveMainComment(string snippetId, string comment)
        {
            busCodeSnippet busSnippet = new busCodeSnippet();
            if (busSnippet.Load(snippetId) == null)
                throw new ArgumentException("Invalid snippetId passed.");

            if (!IsEditAllowed(busSnippet.Entity) && !AppUserState.IsAdmin)
                throw new AccessViolationException("You are not allowed to edit this snippet.");

            busSnippet.Entity.Comment = comment.Replace("\n","\r\n");
            if (!busSnippet.Save())
                throw new InvalidOperationException("Unable to save snippet: " + busSnippet.ErrorMessage);

            string tagResult = HtmlUtils.DisplayMemo(comment);
            return tagResult;
        }

        /// <summary>
        /// Retrieves origianl code for a snippet
        /// </summary>
        /// <param name="snippetId"></param>
        /// <returns></returns>
        [CallbackMethod]
        public string GetCode(string snippetId)
        {
            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                if (busSnippet.Load(snippetId) == null)
                    throw new ArgumentException("Invalid snippetId passed.");
                return busSnippet.Entity.Code;
            }
        }

        [CallbackMethod]
        public bool AddFavorite(string title, string snippetId)
        {
            if (AppUserState.IsEmpty())
                return false;

            using (var snippetBus = new busCodeSnippet())
            {
                return snippetBus.AddFavorite(title, snippetId, AppUserState.UserId);
            }
        }

        /// <summary>
        /// Marks a snippet as abuse
        /// </summary>
        /// <param name="snippetId"></param>
        /// <returns>Abuse status</returns>
        [CallbackMethod]
        public bool ReportAbuse(string snippetId)
        {
            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                if (busSnippet.Load(snippetId) == null)
                    throw new ArgumentException("Invalid snippetId passed.");
                var snippet = busSnippet.Entity;
                // switch value
                snippet.IsAbuse = !snippet.IsAbuse;
                if (snippet.IsAbuse)
                {
                    AppWebUtils.SendEmail("CodePaste.NET Abuse: " + busSnippet.Entity.Title, "Abuse reported for this snippet \r\n\r\n" + WebUtils.ResolveServerUrl("~/" + busSnippet.Entity.Id), App.Configuration.AdminEmailAddress);
                }
                if (!busSnippet.Save())
                    throw new ApplicationException(busSnippet.ErrorMessage);
                return snippet.IsAbuse;
            }
        }

        /// <summary>
        /// Returns a list of languages as key,value pair array
        /// </summary>
        /// <returns></returns>
        [CallbackMethod]
        public Dictionary<string, string> GetCodeLanguages()
        {
            return App.CodeLanguages;
        }

        /// <summary>
        /// Removes a snippet
        /// </summary>
        /// <param name="snippetId"></param>
        /// <returns></returns>
        [CallbackMethod]
        public bool RemoveSnippet(string snippetId)
        {
            using (busCodeSnippet Snippet = new busCodeSnippet())
            {
                if (Snippet.Load(snippetId) == null)
                    throw new InvalidOperationException("Unable to delete snippet");

                if (!this.AppUserState.IsAdmin && !this.IsEditAllowed(Snippet.Entity))
                    throw new UnauthorizedAccessException("Unauthorized Access: You have to be signed in as an administrator in delete snippets.");

                Snippet.Delete();
            }

            return true;
        }

        /// <summary>
        /// Saves a new comment for a snippet.
        /// 
        /// returns object with commentText and headerText properties
        /// </summary>
        /// <param name="commentText"></param>
        /// <param name="snippetId"></param>
        /// <returns></returns>
        [CallbackMethod]
        public object SaveComment(string commentText, string snippetId)
        {
            if (string.IsNullOrEmpty(commentText))
                throw new InvalidOperationException("Please enter some comment text before submitting.");

            using (busCodeSnippet Snippet = new busCodeSnippet())
            {
                if (Snippet.Load(snippetId) == null)
                    throw new InvalidOperationException("Invalid snippet specified");

                if (string.IsNullOrEmpty(this.AppUserState.Name))
                    throw new UnauthorizedAccessException("You have to be signed in in order to add comments.");

                if (!Snippet.AddComment(commentText, this.AppUserState.UserId))
                    throw new ApplicationException("Couldn't add comment: " + Snippet.ErrorMessage);
            }

            return  new
            {
                commentText = HtmlUtils.DisplayMemoEncoded(commentText),
                headerText = "by " + this.AppUserState.Name + " &nbsp;" + TimeUtils.FriendlyDateString(DateTime.Now,true)
            };
        }


        [CallbackMethod]
        public bool AddSpamFilter(string keyword)
        {
            if (!this.IsAdmin())
                throw new InvalidOperationException("Access denied.");

            if (string.IsNullOrEmpty(keyword))
                throw new InvalidOperationException("Can't add a blank keyword");
            
            var admin = new busAdministration();
            admin.AddSpamKeyword(keyword);

            return true;
        }

        public bool RemoveSpamKeyword(string keyword)
        {
            if (!this.IsAdmin())
                throw new InvalidOperationException("Access denied.");

            if (string.IsNullOrEmpty(keyword))
                throw new InvalidOperationException("Can't add a blank keyword");

            var admin = new busAdministration();
            if (!admin.RemoveSpamKeyword(keyword))
                throw new InvalidOperationException("Couldn't remove item: " + admin.ErrorMessage);

            return true;
            
        }

        [CallbackMethod]
        public IEnumerable<SpamKeyword> GetSpamKeywords()
        {
            if (!this.IsAdmin())
                throw new InvalidOperationException("Access denied.");

            var admin = new busAdministration();
            return admin.Context.SpamKeywords.OrderBy(kw => kw.Keyword);
        }


        /// <summary>
        /// Internally determines whether the snippet user is the same as the 
        /// logged in user.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool IsEditAllowed(CodeSnippet entity)
        {            
            if (string.IsNullOrEmpty(entity.UserId) || this.AppUserState.UserId != entity.UserId)
                return false;

            return true;        
        }

        private bool IsAdmin()
        {
            if (AppUserState == null || !AppUserState.IsAdmin)
                return false;

            return true;
        }

    }

}
