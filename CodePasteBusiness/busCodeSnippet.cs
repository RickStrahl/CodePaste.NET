using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Westwind.BusinessFramework.LinqToSql;
using Westwind.Utilities;
using Manoli.Utils.CSharpFormat;
using System.Transactions;
using System.Data.Common;
using System.Xml;
using System.Xml.Serialization;
using System.Web;
using System.Threading;
using System.IO;

namespace CodePasteBusiness
{
    public class busCodeSnippet : BusinessObjectLinq<CodeSnippet,CodePasteDataContext>
    {        

        /// <summary>
        /// Override NewEntity to assign new ID immediately
        /// </summary>
        /// <returns></returns>
        public override CodeSnippet NewEntity()
        {            
            if (base.NewEntity() == null)
                return null;

            Entity.Id = DataUtils.GenerateUniqueId(6);
            Entity.Entered = DateTime.Now;
            
            // Anonymous users will have this empty always
            Entity.UserId = string.Empty;     
                    
            return Entity;
        }

        protected override void OnLoaded(CodeSnippet snippet)
        {
            base.OnLoaded(snippet);
            
            snippet.User = GetUser(snippet);            
        }


        /// <summary>
        /// Delete a code snippet and all related comments.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override bool Delete(CodeSnippet entity)
        {
            if (entity == null)            
                entity = Entity;

            using (TransactionScope trans = new TransactionScope())
            {
                // base business layer delete behavior through DataContext
                if (!base.Delete(entity))
                    return false;

                // plain Ado.Net operation - no retrieval first to delete
                int result = ExecuteNonQuery("delete from comments where snippetId = @snippetId",
                                                  Context.Db.CreateParameter("@snippetId", entity.Id));
                if (result < 0)
                    return false;

                trans.Complete();
                return true;
            }
        }

        
        /// <summary>
        /// Overrides the Save behavior pre-processing values to be saved. 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override bool Save(CodeSnippet entity)
        {
            if (entity == null)
                entity = Entity;

            if (string.IsNullOrEmpty(entity.Author) && entity.User != null)
                entity.Author = entity.User.Name;
            
            entity.FormattedCode = GetFormattedCode(entity.Code,entity.Language,false);

            return base.Save(entity);
        }

        /// <summary>
        /// Returns a user instance
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public User GetUser(CodeSnippet snippet)
        {
            if (string.IsNullOrEmpty(snippet.UserId))
                return null;

            snippet.User = Context.Users.SingleOrDefault(usr => usr.Id == snippet.UserId);
            return snippet.User;
        }


        /// <summary>
        /// Returns a user for the current snippet entity and sets
        /// the User value.
        /// </summary>
        /// <returns></returns>
        public User GetUser()
        {
            Entity.User = GetUser(Entity);
            return Entity.User;
        }

        /// <summary>
        /// Strips sensitive user information from the user entity
        /// if it is set.
        /// </summary>
        public void StripSensitiveUserInformation()
        {
            if (Entity == null)
                return;

            Entity.User.Email = string.Empty;
            Entity.User.Password = string.Empty;
            Entity.User.IsAdmin = false;
        }

        /// <summary>
        /// Loads a list of comments
        /// </summary>
        /// <returns></returns>
        public List<Comment> GetComments(CodeSnippet snippet)
        {
            return Context.Comments
                                    .Where(cmt => cmt.SnippetId == snippet.Id)
                                    .ToList();            
        }

        /// <summary>
        /// Loads a list of comments for the currently loaded entity.
        /// </summary>
        /// <returns></returns>
        public List<Comment> GetComments()
        {
            return GetComments(Entity);
        }

        public List<AdditionalSnippet> GetAdditionalSnippets(CodeSnippet snippet)
        {
            return Context.AdditionalSnippets
                                    .Where(snip => snip.SnippetId == snippet.Id)
                                    .ToList();
        }
        public List<AdditionalSnippet> GetAdditionalSnippets()
        {
            return GetAdditionalSnippets(Entity);
        }

        /// <summary>
        /// Adds a new Additional Snippet into the page
        /// </summary>
        /// <param name="snippetId"></param>
        /// <param name="title"></param>
        /// <param name="code"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public bool AddAdditionalSnippet(string snippetId, string title, string code, string language)
        {            
            AdditionalSnippet snippet = new AdditionalSnippet()
            {
                SnippetId = snippetId,
                Title = title,
                Code = code,
                Language = language
            };
            Context.AdditionalSnippets.InsertOnSubmit(snippet);
            
            return SubmitChanges();
        }

        public bool AddComment(string commentText, string userId)
        {
            using (busComment busComment = CodePasteFactory.GetComment())
            {
                Comment comment = busComment.NewEntity();
                comment.CommentText = commentText;
                comment.SnippetId = Entity.Id;
                comment.UserId = userId;
                busUser user = CodePasteFactory.GetUser();
                comment.Author = user.GetNameFromUserId(userId);
                if (comment.Author == null)
                {
                    SetError("Couldn't add comment. Invalid User id.");
                    return false;
                }
                if (!busComment.Save())
                {
                    SetError(busComment.ErrorMessage);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Adds a snippet for a user linking a topic
        /// </summary>
        /// <param name="snippetId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool AddFavorite(string title, string snippetId, string userId)
        {
            // already exists -  remove it
            if (Context.Favorites.Any(fv => fv.SnippetId == snippetId && fv.UserId == userId))
            {
                int res = Context.Db.ExecuteNonQuery("delete from Favorites where snippetId=@0 and userId=@1", snippetId, userId);
                if (res == -1)
                    throw new InvalidOperationException(Context.Db.ErrorMessage);
                return true;                
            }

            Favorite fav = new Favorite()
            {
                Id = DataUtils.GenerateUniqueId(6),
                Title = title,
                SnippetId = snippetId,
                UserId = userId,
                Entered = DateTime.UtcNow
            };
            Context.Favorites.InsertOnSubmit(fav);

            return SubmitChanges();
        }

        /// <summary>
        /// Determines if a user has a snippet favorited
        /// </summary>
        /// <param name="snippetId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsFavorite(string snippetId, string userId)
        {
            return Context.Favorites.Any(fav => fav.SnippetId == snippetId && fav.UserId == userId);
        }

        /// <summary>
        /// Returns a list of favorites ordered by reverse date
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<CodeSnippetListItem> GetFavorites(string userId)
        {
            var favs = Context.CodeSnippets
                          .Join(Context.Favorites, snip => snip.Id, fav => fav.SnippetId, (snip, fav) => new { Snippet = snip, Favorite = fav })
                          .Where(m => m.Favorite.UserId == userId)
                          .OrderByDescending(m => m.Favorite.Entered)
                          .Select(m => new CodeSnippetListItem()
                          {
                              Id = m.Snippet.Id,
                              Title = m.Snippet.Title,
                              Code = m.Snippet.Code,
                              FormattedCode = m.Snippet.FormattedCode,
                              Author = m.Snippet.Author,
                              Entered = m.Snippet.Entered,
                              Tags = m.Snippet.Tags,
                              Language = m.Snippet.Language,
                              UserId = m.Snippet.UserId,
                              Views = m.Snippet.Views,
                              CommentCount = Context.Comments.Where(cmt => cmt.SnippetId == m.Snippet.Id).Count()
                          })
                          .ToList();
            return favs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        protected override void OnValidate(CodeSnippet entity)
        {
            base.OnValidate(entity); 
            
            bool isNew = entity.tstamp == null;

            if (string.IsNullOrEmpty(entity.Code))
                ValidationErrors.Add("Code snippet must be provided", "Code");
            else
                if (entity.Code.Length > App.Configuration.MaxCodeLength)
                    ValidationErrors.AddFormat("Code snippet length cannot exceed {0:n0} bytes. Your's is: {1:n0} bytes.",
                                                    "Code","Code",
                                                    App.Configuration.MaxCodeLength,entity.Code.Length);
            
            //if (string.IsNullOrEmpty(entity.Title))
            //    this.ValidationErrors.Add("Please provide a title for the snippet", "Title");
            
            if (entity.Tags != null && (entity.Tags.Contains("./") || entity.Tags.Contains(".\\") ))
                ValidationErrors.Add("Tags cannot contain path related characters","Tags");

            if (string.IsNullOrEmpty(entity.Language))
                ValidationErrors.Add("Please provide a code language or NoFormat for no formatting", "Language");                              
            

            if (isNew && !string.IsNullOrEmpty(entity.Code))
            { 
                if (EntityExists(entity))
                    ValidationErrors.Add("Snippet exists already","Code");
            }

            if (IsSpam(entity))
            {
                ValidationErrors.Add("Invalid entry","Code");
            }
        }

        /// <summary>
        /// Determines if the entity contains spam keywords
        /// </summary>
        /// <returns></returns>
        public bool IsSpam(CodeSnippet entity = null)
        {
            if (entity == null)
                entity = Entity;

            if (entity == null)
                return false;

            bool isAnonymous = string.IsNullOrEmpty(Entity.UserId); 
            
            string code = Entity.Code.ToLower();
            
            if (Entity.Tags!= null && (Entity.Tags.ToLower().Contains("http://") || Entity.Tags.ToLower().Contains("https://")) )
                return true;

            if (Entity.Title != null && (Entity.Title.ToLower().Contains("http://") ||Entity.Title.ToLower().Contains("https://")) )
                return true;

            // more than 3 links
            int count = code.Split(new[] { "http://", "https://" },StringSplitOptions.RemoveEmptyEntries).Length - 1;
            if (isAnonymous && count > 3)
                return true;

            var spamKeywords = Context.SpamKeywords.ToList();
            int res = spamKeywords.Count(kwd => code.Contains(kwd.Keyword.ToLower()));
            if (res > 0)
                return true;

            var lineCount = StringUtils.CountLines(code);
            
            if (isAnonymous && lineCount < 4 && (code.Contains("http://") || code.Contains("https://")))
                return true;

            // check for last line with http link
            var lines = StringUtils.GetLines(code);
            if (lines.Length > 0)
            {
                if (lines[lines.Length - 1].Contains("http://") || lines[lines.Length - 1].Contains("https://"))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check to see if this snippet was already entered
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool EntityExists(CodeSnippet entity)
        {
            int matches = Context.CodeSnippets.Where(snip => snip.Code == entity.Code && snip.Language == entity.Language).Count();
            if (matches > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Retrieves a list of snippets based on some simple criteria
        /// 
        /// recent
        /// category
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public List<CodeSnippetListItem> GetSnippetList(string filter, string filterParameter)
        {
            if (filterParameter == null)
                filterParameter = string.Empty;

            IQueryable<CodeSnippet> snippetQuery = null;            
            List<CodeSnippetListItem> snippetList = null;
            filter = filter.ToLower();
            filterParameter = filterParameter.ToLower();


            int recordCount = App.Configuration.MaxListDisplayCount;


            if (filter == "recent")
            {
                if (!string.IsNullOrEmpty(filterParameter))
                    int.TryParse(filterParameter, out recordCount);

                snippetQuery = Context.CodeSnippets
                           .OrderByDescending(snip => snip.Entered)
                           .Take(recordCount);
            }
            else if (filter == "tag")
            {
                string tag = (filterParameter ?? string.Empty).ToLower();


                snippetQuery = Context.CodeSnippets.Where(snip => ("," + snip.Tags.ToLower() + ",").Contains("," + tag + "," ) )
                                         .OrderByDescending( snip => snip.Entered)
                                         .Take(recordCount);                

            }
            else if (filter == "language")
            {
                string language = filterParameter ?? string.Empty;
                snippetQuery = Context.CodeSnippets.Where(snip => snip.Language == language )
                                         .OrderByDescending(snip => snip.Entered)
                                         .Take(recordCount);
            }
            else if (filter == "user")
            {

                string user = filterParameter ?? string.Empty;                
                snippetQuery = Context.CodeSnippets
                                    .Where( snip => snip.UserId == user || snip.Author == user)
                                    .OrderByDescending( snip => snip.Entered)
                                    .Take(recordCount);                
                
            }

            if (snippetQuery != null)
                snippetList = snippetQuery
                           .Select(snip => new CodeSnippetListItem()
                           {
                               Id = snip.Id,
                               Title = snip.Title,
                               Code = snip.Code,
                               FormattedCode = snip.FormattedCode,
                               Author = snip.Author,
                               Entered = snip.Entered,
                               Tags = snip.Tags,
                               UserId = snip.UserId,
                               Language = snip.Language,
                               Views = snip.Views,
                               CommentCount = Context.Comments.Where(cmt => cmt.SnippetId == snip.Id).Count()
                           }).ToList();



            return snippetList;
        }

        /// <summary>
        /// Searches for items based on a structure of search parameters.
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        public IQueryable<CodeSnippetListItem> GetSearchList(CodeSnippetSearchParameters parms)
        {
            var result = from snip in Context.CodeSnippets
                       select new CodeSnippetListItem()
                         {
                             Id = snip.Id,
                             Title = snip.Title,
                             Code = snip.Code,
                             FormattedCode = snip.FormattedCode,
                             Author = snip.Author,
                             Entered = snip.Entered,
                             Tags = snip.Tags,
                             Language = snip.Language,
                             UserId = snip.UserId,
                             Views = snip.Views,
                             CommentCount = Context.Comments.Where(cmt => cmt.SnippetId == snip.Id).Count()
                         };


            bool noParms = true;

            if (!string.IsNullOrEmpty(parms.SearchString))
            {
                result = result.Where(snip => snip.Title.Contains(parms.SearchString) || snip.Code.Contains(parms.SearchString));
                noParms = false;
            }

            if (parms.FromDate > DateTime.MinValue)
            {    
                result = result.Where(snip => snip.Entered > parms.FromDate);
                noParms = false;
            }

            if (parms.ToDate > DateTime.MinValue)
            {
                result = result.Where(snip => snip.Entered < parms.ToDate.Date.AddDays(1));
                noParms = false;
            }

            if (!string.IsNullOrEmpty(parms.Language))
            {
                // TODO: Normalize languages like csharp
                if (parms.Language.ToLower() == "csharp")
                    parms.Language = "c#";
                if (parms.Language.ToLower().StartsWith("vb"))
                    parms.Language = "vb.net";

                result = result.Where(snip => snip.Language == parms.Language);
                noParms = false;
            }

            if (!string.IsNullOrEmpty(parms.Author))
            {
                result = result.Where(snip => snip.Author.Contains(parms.Author));
                noParms = false;
            }

            if (!string.IsNullOrEmpty(parms.UserId))
            {
                result = result.Where(snip => snip.UserId == parms.UserId);
                noParms = false;
            }

            // TODO: Needs additional search/split logic
            if (!string.IsNullOrEmpty(parms.Tags))
            {
                string[] tags = parms.Tags.Split( new char[1] { ','}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string tag in tags)
                {
                    string trimmedTag = tag.Trim();
                    // TODO: Must fix searching with , delimiters - for now just search for text
                    result = result.Where(snip => snip.Tags.Contains(trimmedTag));
                }
                noParms = false;
            }

            if (noParms)
                return null;

            if (parms.SearchOrder == SearchOrderTypes.Entered)
                result = result.OrderByDescending(snip => snip.Entered);
            else if ((parms.SearchOrder == SearchOrderTypes.Title))
                result = result.OrderBy(snip => snip.Title);
            else if (parms.SearchOrder == SearchOrderTypes.Author)
                result = result.OrderBy(snip => snip.Author);
            else if (parms.SearchOrder == SearchOrderTypes.Views)
                result = result.OrderByDescending(snip => snip.Views);

            return result;
        }

        /// <summary>
        /// Returns snippets for a particular user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<CodeSnippetListItem> GetSnippetsForUser(string userId, int count)
        {
            if (count == 0)
                count = App.Configuration.MaxListDisplayCount * 10;

            var res = (from snip in Context.CodeSnippets
                       where snip.UserId == userId
                       orderby snip.Entered descending
                       select new CodeSnippetListItem()
                         {
                             Id = snip.Id,
                             Title = snip.Title,
                             Code = snip.Code,
                             FormattedCode = snip.FormattedCode,
                             Author = snip.Author,
                             Entered = snip.Entered,
                             Tags = snip.Tags,
                             Language = snip.Language,
                             UserId = snip.UserId,     
                             Views = snip.Views,
                             CommentCount = Context.Comments.Where(cmt => cmt.SnippetId == snip.Id).Count()
                         })
                       .Take(count).ToList();

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<CodeSnippetListItem> GetSnippetsForUser(string userId)
        {
            return GetSnippetsForUser(userId, 0);
        }

        /// <summary>
        /// Returns a specified number of lines from a codesnippet and encodes them
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public string GetCodeLines(string code, int count)
        {
            var sb = new StringBuilder(350);
            using (var reader = new StringReader(code))
            {
                int i = 0;
                while (i < count)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;
                    sb.AppendLine(line);
                    i++;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns a subset of lines formatted in source code
        /// </summary>
        /// <param name="code"></param>
        /// <param name="count"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public string GetFormattedCodeLines(string code, int count, string language)
        {
            if (string.IsNullOrEmpty(code))
                return string.Empty;

            StringBuilder sb = new StringBuilder(300);
            StringReader reader = new StringReader(code);
            int i = 0;
            while(i < count)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;
                sb.AppendLine(line);
                i++;
            }

            if (sb.Length > 0)
                sb.AppendLine("</pre>");

            return sb.ToString();


            // TODO: Temporarily removed
            //SourceFormat formatter = CodeFormatFactory.Create(language);            
            //return formatter.FormatCode(shortCode);
        }

        /// <summary>
        /// Returns formatted code for the specified language.
        /// 
        /// see CSharpFormat for supported types
        /// </summary>
        /// <param name="code"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public string GetFormattedCode(string code, string language, bool showLineNumbers = false, bool allowWordWrap = false)
        {                        
            SourceFormat formatter = CodeFormatFactory.Create(language);
            formatter.AllowWordWrapping = allowWordWrap;

            if (showLineNumbers)
                formatter.LineNumbers = true;

            return formatter.FormatCode(code);
        }

        /// <summary>
        /// Create a list of links from a Snippet's tags. Tag string should
        /// be comma delimited.
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public string GetTagLinkList(string tags)
        {
            if (tags == null)
                tags = Entity.Tags;

            if (string.IsNullOrEmpty(tags))
                return string.Empty;

            string[] tagStrings = tags.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder html = new StringBuilder();
            foreach (string tagString in tagStrings)
            {
                string urlAction = WebUtils.ResolveUrl("~/list/tag/") + StringUtils.UrlEncode(tagString.Trim());
                html.Append(HtmlUtils.Href(HttpUtility.HtmlEncode(tagString.Trim()), urlAction) + ", ");
            }

            if (html.Length > 2)
                html.Length -= 2;

            return html.ToString();
        }

        #region Administration Methods

        /// <summary>
        /// Clears out old snippets that are older than a the specified number of days
        /// and have less than the specified number of views
        /// </summary>
        /// <param name="hours">Age of anonymous snippets to delete</param>
        /// <param name="viewCount">Number of views to keep snippet from being deleted</param>
        /// <returns>number of records deleted or -1 on failure</returns>
        public int ClearAnonymousSnippets(int hours, int viewCount)
        { 
            int result = ExecuteNonQuery("delete from CodeSnippets where entered < @date and userid = ''",
                                                      Context.Db.CreateParameter("@date",DateTime.Now.AddHours(hours * -1) ),
                                                      Context.Db.CreateParameter("@viewCount",viewCount) );

            return result;
        }



        /// <summary>
        /// Returns all snippets that have the abuse flag set
        /// </summary>
        /// <returns></returns>
        public IQueryable<CodeSnippet> GetAbuseReportedSnippets()
        {
            return Context.CodeSnippets.Where(code => code.IsAbuse).OrderByDescending(code=> code.Entered);
        }

        #endregion



        #region Snippet Callback Operations

        /// <summary>
        /// Retrieves the total count of views
        /// </summary>
        /// <returns></returns>
        public int GetTotalSnippetCount()
        {            
            int result = 
                (from snip in Context.CodeSnippets
                where snip.Id == Entity.Id 
                group snip.Id by snip.Id into SummaryGroup                
                select SummaryGroup.Count()).FirstOrDefault();
            
            return Entity.Views + result;
        }

        /// <summary>
        /// Logs a snippet view
        /// </summary>
        /// <param name="snippetId"></param>
        /// <param name="ipAddress"></param>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public bool LogSnippetView(string snippetId, string ipAddress, string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return false;

            userAgent = userAgent.ToLower();

            if (!(userAgent.Contains("mozilla") && !userAgent.StartsWith("safari") &&
                !userAgent.StartsWith("blackberry") && !userAgent.StartsWith("t-mobile") &&
                !userAgent.StartsWith("htc") && !userAgent.StartsWith("opera")))
                return false;

            if (userAgent.Contains("spider") || userAgent.Contains("robot") || 
                userAgent.Contains("googlebot") || userAgent.Contains("crawler"))
                return false;

            // NOT COOL!
            //this.Context.LogSnippetClick(snippetId, ipAddress);

            // create a new context so this should be safe
            using (var ctx = CreateContext())
            {
                ctx.LogSnippetClick(snippetId, ipAddress);
            }

            return true;
        }

        /// <summary>
        /// Asynch version of LogSnippet which runs in the background off
        /// a separate thread from the thread pool.
        /// </summary>
        /// <param name="snippetId"></param>
        /// <param name="ipAddress"></param>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public void LogSnippetViewAsync(string snippetId, string ipAddress, string userAgent)
        {
            Func<string, string, string, bool> del = LogSnippetView;             
            var ia = del.BeginInvoke(snippetId, ipAddress,userAgent,null,null);
        }


        #endregion

        /// <summary>
        /// Fix up languages for Ace Editor from old
        /// language definitions.
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public string FixUpLanguage(string language)
        {
            if (language == "C#")
                language = "CSharp";

            return language;
        }
    }
    
    /// <summary>
    /// An individual Code Snippet returned as part of a list.
    /// Includes extra propreties for Comment Count and logic
    /// to retrieve an Author as a link (used in many places)
    /// including in desktop UI.
    /// </summary>
    [XmlType(TypeName="Snippet")]
    public class CodeSnippetListItem : CodeSnippet
    {        
        /// <summary>
        /// The number of comments that exist on this snippet        
        /// </summary>
        public int CommentCount { get; set; }        

        /// <summary>
        /// Returns an author either as a name
        /// </summary>
        /// <returns></returns>
        public string GetAuthorLink()
        {
            if (UserId == string.Empty)
                return HtmlUtils.HtmlEncode(Author);
            
            return HtmlUtils.Href( HtmlUtils.HtmlEncode(Author), WebUtils.ResolveUrl("~/list/user/") + UserId);
        }
    }

    /// <summary>
    /// Parameter object used to pass search parameters to the Snippet's
    /// search function. 
    /// </summary>
    public class CodeSnippetSearchParameters
    {
        /// <summary>
        /// A string to search for in the title and code of the snippet.
        /// </summary>
        public string SearchString { get; set; }
        
        /// <summary>
        /// Beginning date range for snippets to search.
        /// If not provided all snippets are searched.
        /// </summary>
        public DateTime FromDate { get; set; }

        /// <summary>
        /// Ending date range for snippets to be searched.
        /// If FromDate is provided and this is not up to
        /// current date is searched. 
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>
        /// A comma delimited list of tags to search for in
        /// the snippet's list of Tags.
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// The language searched for. Languages available are: <see cref="Enumeration SourceLanguages"/>
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// The author of the snippet to search for
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// A single user id of user to search for
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Determines in which order the result snippets are displayed.
        /// In HTTP interface this parameter can be passed as a string
        /// for the enumeration value.
        /// </summary>
        public SearchOrderTypes SearchOrder { get; set; }
 
        /// <summary>
        /// Determines the maximum records that are returned for this query.
        /// The default is around 100 (varies based on application configuration)
        /// </summary>
        public int DisplayCount {get; set;}

        /// <summary>
        /// The page number to display -1 ignore paging
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// output value that returns the total items in the query
        /// </summary>
        public int TotalResultCount { get; set; }

        public CodeSnippetSearchParameters()
        {
            DisplayCount = App.Configuration.MaxListDisplayCount;
            Page = -1;
        }
    }

    /// <summary>
    /// Determines how a search orders its results of snippets
    /// </summary>
    public enum SearchOrderTypes
    {
        /// <summary>
        /// By entered date last entered first
        /// </summary>
        Entered,
        /// <summary>
        /// By title
        /// </summary>
        Title, 
        /// <summary>
        /// By Author alphabetically
        /// </summary>
        Author,
        /// <summary>
        /// By the number of views descending
        /// </summary>
        Views,
        /// <summary>
        /// Default search order returned by the query
        /// </summary>
        None
    }
}
