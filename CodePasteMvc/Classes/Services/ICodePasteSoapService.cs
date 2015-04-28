using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using CodePasteBusiness;
using System.Web.Services.Protocols;
using System.Xml;
using Westwind.Utilities;
using System.Xml.Serialization;
using System.Threading;

namespace CodePasteMvc
{
    [WebServiceBinding(
            Name = "CodePasteSoapService",
            Namespace = "http://codepaste.net/soap",
            ConformsTo = WsiProfiles.BasicProfile1_1,
            EmitConformanceClaims = true)]
    public interface ICodePasteSoapService
    {
        /// <summary>
        /// Returns an individual snippet based on an id
        /// </summary>
        /// <param name="snippetId"></param>
        /// <returns></returns>
        [WebMethod(Description = "Returns an individual snippet based on an Id")]
        CodeSnippet GetSnippet(string id);

        /// <summary>
        /// Returns a new empty snippet to the client. The snippet
        /// contains a new id
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "Returns a new empty snippet to the client. The snippet contains a new id.")]
        CodeSnippet GetNewSnippet();

        /// <summary>
        /// Returns a list of recent snippets
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "Returns a list of recent snippets")]
        List<CodeSnippetListItem> GetRecentSnippets();

        /// <summary>
        /// Returns a list of snippets for a given user's id
        /// </summary>
        /// <param name="userId">User id to return snippets for</param>
        /// <param name="count">Number of records to return. 0 returns 10 times default list size.</param>
        /// <returns></returns>
        [WebMethod(Description = "Returns a list of snippets for a given user's id")]
        List<CodeSnippetListItem> GetSnippetsForUser(string userId, int count);

        /// <summary>
        /// Allows searching of snippets by providing a search parameter structure
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "Allows searching of snippets by providing a search parameter structure")]
        List<CodeSnippetListItem> SearchSnippets(CodeSnippetSearchParameters searchParameters);

        /// <summary>
        /// Retrieve comments for a given snippet
        /// </summary>
        /// <param name="snippetId"></param>
        /// <returns></returns>        
        [WebMethod(Description = "Retrieve comments for a given snippet")]
        List<Comment> GetCommentsForSnippet(string snippetId);

        /// <summary>
        /// Allows posting of a new code snippet.
        /// </summary>
        /// <param name="snippet"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        [WebMethod(Description = "Allows posting of a new Code Snippet to the service")]
        CodeSnippet PostNewCodeSnippet(CodeSnippet snippet, string sessionKey);

        /// <summary>
        /// Allows deletion of an individual snippet by the author.
        /// </summary>
        /// <param name="snippetId"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        [WebMethod(Description = "Deletes an individual snippet")]
        bool DeleteSnippet(string snippetId, string sessionKey);

        /// <summary>
        /// Login method for update operations agains the API that returns a session
        /// key. Pass username/email and password.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [WebMethod(Description = "Login method for update operations agains the API that returns a session key")]        
        string GetSessionKey(string email, string password);


    }
}
