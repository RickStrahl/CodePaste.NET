using System;
using System.Web.Mvc;
using CodePasteBusiness;
using Westwind.Web.JsonSerializers;
using System.IO;
using Westwind.Utilities;
using System.Reflection;
using System.Collections.Generic;

namespace CodePasteMvc.Controllers
{
    [HandleError()]
    public class ApiController : ApiControllerBase
    {

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            // default the API format to Xml if not specified
            if (string.IsNullOrEmpty(this.Format))
                this.Format = "xml";
        }

        /// <summary>
        /// Returns recent 
        /// 
        /// Path: /api/feed
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration=300,VaryByParam="Format")]
        public ActionResult Feed()
        {
            using (busCodeSnippet busSnippet = new busCodeSnippet())
            {
                this.ViewData["busSnippet"] = busSnippet;
                var snippetList = busSnippet.GetSnippetList("recent", App.Configuration.MaxListDisplayCount.ToString());
                string format = Request.QueryString["Format"];
                if (string.IsNullOrEmpty(format))
                    this.Format = "rss";
                ActionResult actionResult = this.ApiResult(snippetList);
                return actionResult;
            }
        }


        /// <summary>
        /// Returns an individual snippet based on an id
        /// 
        /// /api/Snippet/3232  or /api/Snippet/Title
        /// </summary>
        /// <param name="snippetId"></param>
        /// <returns></returns>
        public ActionResult Snippet(string id)
        {
            CodePasteServiceBase service = new CodePasteServiceBase();

            if (string.IsNullOrEmpty(id))
                return this.ApiResult(service.GetNewSnippet());

            return this.ApiResult(service.GetSnippet(id));            
        }


        /// <summary>
        /// Creates a new snippet to post
        /// </summary>
        /// <param name="formVars"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Put | HttpVerbs.Post)]
        public ActionResult Snippet(FormCollection formVars)
        {
            string sessionKey = Request.Params["sessionKey"];            
            CodePasteServiceBase service = new CodePasteServiceBase();

            // load up snippet from posted values
            CodeSnippet snippet = new CodeSnippet();
            this.UpdateModel(snippet);
            
            return this.ApiResult(service.PostNewCodeSnippet(snippet, sessionKey));            
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult List(string filter, string filterParameter)
        {
            CodePasteServiceBase service = new CodePasteServiceBase();
            return this.ApiResult(service.ListSnippets(filter, filterParameter));
        }

        /// <summary>
        /// Returns a list of snippets based on search parameters passed
        /// </summary>
        /// <param name="formVars"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post | HttpVerbs.Get)]
        public ActionResult SnippetSearch(FormCollection formVars)
        {
            CodeSnippetSearchParameters parameters = new CodeSnippetSearchParameters();
            this.UpdateModel(parameters);

            CodePasteServiceBase service = new CodePasteServiceBase();
            return this.ApiResult( service.SearchSnippets(parameters) );
        }


        /// <summary>
        /// Remove a snippet that the signed in user has posted.
        /// 
        /// Requires a session key and the session key's account
        /// must match the account that has posted the snippet.
        /// </summary>
        /// <param name="formVars">Id,SessionKey</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Delete | HttpVerbs.Get)]
        public ActionResult DeleteSnippet(string id, string sessionKey)
        {
            CodePasteServiceBase service = new CodePasteServiceBase();
            return this.ApiResult(service.DeleteSnippet(id, sessionKey));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="formVars"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult GetSessionKey(FormCollection formVars)
        {
            string username = Request.Params["email"] ?? Request.Params["username"];
            string password = Request.Params["password"]; 

            CodePasteServiceBase service = new CodePasteServiceBase();
            return this.ApiResult(service.GetSessionKey(username,password));
        }

    }
}
