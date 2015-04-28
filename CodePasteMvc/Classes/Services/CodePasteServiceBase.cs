using System.Collections.Generic;
using System.Web.Services;
using CodePasteBusiness;
using System.Web.Services.Protocols;
using Westwind.Utilities;
using System.Threading;
using System;
using System.Linq;

namespace CodePasteMvc
{
    /// <summary>
    /// Base API Service implementation class that is meant to be subclassed.
    /// Subclass this implementation class for SOAP and RESTMVC services.
    /// </summary>
    public class CodePasteServiceBase : ICodePasteSoapService
    {
        /// <summary>
        /// Returns an individual snippet based on an id
        /// </summary>
        /// <param name="snippetId"></param>
        /// <returns></returns>
        public CodeSnippet GetSnippet(string id)
        {
            using (busCodeSnippet codesnippet = CodePasteFactory.GetCodeSnippet())
            {
                if (codesnippet.Load(id) == null)
                    this.ThrowException("Invalid code snippet id");
                codesnippet.GetComments();
                codesnippet.StripSensitiveUserInformation();
                return codesnippet.Entity;
            }
        }


        /// <summary>
        /// Returns a new empty snippet to the client. The snippet
        /// contains a new id
        /// </summary>
        /// <returns></returns>
        public CodeSnippet GetNewSnippet()
        {
            using (busCodeSnippet codesnippet = CodePasteFactory.GetCodeSnippet())
            {
                if (codesnippet.NewEntity() == null)
                    this.ThrowException("Unable to load new snippet: " + codesnippet.ErrorMessage);
                return codesnippet.Entity;
            }
        }

        /// <summary>
        /// Returns a list of recent snippets
        /// </summary>
        /// <returns></returns>        
        public List<CodeSnippetListItem> GetRecentSnippets()
        {
            using (busCodeSnippet codeSnippet = CodePasteFactory.GetCodeSnippet())
            {
                List<CodeSnippetListItem> items = codeSnippet.GetSnippetList("recent", null);
                return items;
            }
        }

        /// <summary>
        /// Generic snippet list method where filters are Recent, Tag, User, Language and parameters are empty, tag name, user id or language respectively
        /// </summary>
        /// <param name="filter">recent,tags,user</param>
        /// <param name="filterParameter">Additional parameter (tagname, userid)</param>
        /// <returns></returns>
        [WebMethod(Description="Generic snippet list method where filters are Recent, Tag, User, Language and parameters are empty, tag name, user id or language respectively")]
        public List<CodeSnippetListItem> ListSnippets(string filter, string filterParameter)
        {
            using (busCodeSnippet codeSnippet = CodePasteFactory.GetCodeSnippet())
            {
                List<CodeSnippetListItem> snippets = codeSnippet.GetSnippetList(filter, filterParameter);
                return snippets;
            }
        }

        /// <summary>
        /// Returns a list of snippets for a given user's id
        /// </summary>
        /// <param name="userId">User id to return snippets for</param>
        /// <param name="count">Number of records to return. 0 returns 10 times default list size.</param>
        /// <returns></returns>
        public List<CodeSnippetListItem> GetSnippetsForUser(string userId, int count)
        {
            using (busCodeSnippet codeSnippet = CodePasteFactory.GetCodeSnippet())
            {
                return codeSnippet.GetSnippetsForUser(userId, count);
            }
        }


        /// <summary>
        /// Allows searching of snippets by providing a search parameter structure
        /// </summary>
        /// <returns></returns>        
        public List<CodeSnippetListItem> SearchSnippets(CodeSnippetSearchParameters searchParameters)
        {
            using (busCodeSnippet codeSnippet = CodePasteFactory.GetCodeSnippet())
            {
                return codeSnippet.GetSearchList(searchParameters).ToList();
            }
        }

        /// <summary>
        /// Retrieve comments for a given snippet
        /// </summary>
        /// <param name="snippetId"></param>
        /// <returns></returns>
        public List<Comment> GetCommentsForSnippet(string snippetId)
        {
            using (busComment comment = CodePasteFactory.GetComment())
            {
                return comment.GetCommentsForSnippet(snippetId);
            }
        }

        /// <summary>
        /// Allows posting of a new code snippet.
        /// </summary>
        /// <param name="snippet"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>       
        public CodeSnippet PostNewCodeSnippet(CodeSnippet snippet, string sessionKey)
        {
            User user = this.ValidateToken(sessionKey);

            using (busCodeSnippet codeSnippet = CodePasteFactory.GetCodeSnippet())
            {
                if (snippet == null)
                    this.ThrowException("Invalid snippet instance data passed");
                CodeSnippet newSnippet = codeSnippet.NewEntity();
                // Force userId regardless of what the user has set
                newSnippet.UserId = user.Id;
                newSnippet.Author = user.Name;
                if (string.IsNullOrEmpty(newSnippet.Author))
                    newSnippet.Author = snippet.Author;

                if (string.IsNullOrEmpty(snippet.Language))
                    snippet.Language = "NoFormat";
                
                DataUtils.CopyObjectData(snippet, newSnippet, "Id,UserId,Entered,Views,UserId,User,Author,Comments");
                
                if (!codeSnippet.Validate())
                    this.ThrowException("Snippet validation failed: " + codeSnippet.ValidationErrors.ToString());

                if (codeSnippet.IsSpam())
                    this.ThrowException("Invalid Content.");
                
                if (!codeSnippet.Save())
                    this.ThrowException("Failed to save snippet: " + codeSnippet.ErrorMessage);
                return newSnippet;
            }
        }

        /// <summary>
        /// Allows deletion of an individual snippet by the author.
        /// </summary>
        /// <param name="snippetId"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public bool DeleteSnippet(string snippetId, string sessionKey)
        {
            User user = this.ValidateToken(sessionKey);

            using (busCodeSnippet codeSnippet = CodePasteFactory.GetCodeSnippet())
            {
                if (codeSnippet.Load(snippetId) == null)
                    this.ThrowException("Invalid snippet specified");
                if (codeSnippet.Entity.UserId != user.Id)
                    this.ThrowException("Access denied: You can only delete snippets you posted with this user account");
                return codeSnippet.Delete();
            }
        }

        // keep track of tokens requested and occasionally clear expired tokens
        public static int SessionKeyCounter = 0;

        /// <summary>
        /// Login method for update operations agains the API that returns a session
        /// key. Pass username/email and password.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string GetSessionKey(string email, string password)
        {
            User user = this.ValidateUser(email, password);

            using (busUserToken busToken = CodePasteFactory.GetUserToken())
            {
                string token = busToken.GetOrCreateToken(user.Id);
                if (token == null)
                    this.ThrowException("Unable to get new user token. " + busToken.ErrorMessage);
                // every 50 accesses clear out expired session keys
                Interlocked.Increment(ref SessionKeyCounter);
                if (SessionKeyCounter % 50 == 0)
                    busToken.DeleteExpiredTokens();
                return token;
            }
        }

        /// <summary>
        /// Validates a user and returns the user entity as a result or fails
        /// and throws a SOAP exception to fail the request.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private User ValidateUser(string email, string password)
        {
            busUser busUser = CodePasteFactory.GetUser();
            User user = busUser.ValidateUserAndLoad(email, password);
            if (user == null)
                this.ThrowException(busUser.ErrorMessage);

            return user;

        }

        /// <summary>
        /// Validates a user's token and throws an exception if the user is not valid.
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        private User ValidateToken(string sessionKey)
        {
            using (busUserToken busToken = CodePasteFactory.GetUserToken())
            {
                User user = busToken.GetUserFromToken(sessionKey);
                if (user == null)
                    this.ThrowException("Invalid session key. Please call GetSession() again to get a current session key");
                return user;
            }
        }

        /// <summary>
        /// Throws an exception for the specific service type we're dealing with
        /// </summary>
        /// <param name="message"></param>
        protected virtual void ThrowException(string message)
        {
            throw new ApplicationException(message);
        }


    }
}
