using System.Collections.Generic;
using System.Text;
using Westwind.BusinessFramework.LinqToSql;
using System.Linq;
using System.Threading;
using System;


namespace CodePasteBusiness
{
    public class busAdministration : BusinessObjectLinq<CodeSnippet,CodePasteDataContext>
    {

        public bool ShrinkDatabase()
        {
            string sql = @"truncate table applicationlog;
                           DBCC SHRINKDATABASE('CodePaste');";

            if (this.ExecuteNonQuery(sql) < 0)
            {
                return false;
            }

            return true;
        }

        public bool UpdateFormattedCode()
        {         
            var pks = Context.CodeSnippets
                             .Where(snip=> snip.FormattedCode == null  || snip.FormattedCode == "" )
                             .Select( snip=> snip.Id);
            
            try
            {
                foreach (var pk in pks)
                {
                    var snippet = new busCodeSnippet();
                    snippet.Load(pk);

                    // save forces snippet to save format
                    if (!snippet.Save())
                    {
                        SetError(snippet.ErrorMessage);
                        return false;                        
                    }
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                SetError(ex); 
                return false;
            }

            return true;
        }

        public IEnumerable<CodeSnippet> GetSpamKeywords(string keyword)
        {
            var snippets = Context.CodeSnippets
                .Where(snip => snip.Title.Contains(keyword) || 
                    snip.Code.Contains(keyword) || 
                    snip.Tags.Contains(keyword));

            return snippets;
        }

        public bool AddSpamKeyword(string keyword)
        {            
            // already there
            if (Context.SpamKeywords.Any(kw => kw.Keyword == keyword))
                return true;
            
            var spam = new SpamKeyword()
            {
                Keyword = keyword
            };
            Context.SpamKeywords.InsertOnSubmit(spam);
            Context.SubmitChanges();

            return true;
        }

        public bool RemoveSpamKeyword(string keyword)
        {
            int result = Context.Db.ExecuteNonQuery("delete from SpamKeywords where keyword=@keyword",
                                                    Context.Db.CreateParameter("@keyword", keyword));

            if (result < 0)
                throw new InvalidOperationException("couldn't delete keyword: " + Context.Db.ErrorMessage);

            return true;
        }


        public bool DeleteSnippets(string[] ids)
        {
            if (ids == null || ids.Length < 1)
                return true;

            var busSnippet = new busCodeSnippet();
            StringBuilder errors = new StringBuilder();

            foreach (string id in ids)
            {
                if (!busSnippet.Delete(id))
                    errors.AppendLine(busSnippet.ErrorMessage);
            }
            
            if (errors.Length > 0)
            {
                this.SetError(errors.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Deletes snippets that have a userId without an associated user
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public int DatabaseHouseKeeping()
        {
            // clear orphaned snippets
            int result = this.ExecuteNonQuery(
                "delete from codesnippets where IsNull(userId,'') != '' and userID not in (select Id from users)");

            if (result == -1)
                return -1;

            // clear snippets from new users that posted once and not again in last 6 months
            var sql =
                @"
delete from codesnippets where userid in (
	select userid	
			from codesnippets
			inner join users on codesnippets.userid = users.Id
			where 
				  IsNull(userId,'') != ''  and
				  users.entered < @date
		group by userid
		having Count(codeSnippets.id) < 2			
)";

            var result2 = ExecuteNonQuery(sql,
                     Context.Db.CreateParameter("@date", DateTime.UtcNow.AddMonths(6)));

            if (result2 == -1)
                return -1;

            // clear users older than 3 months that don't have any snippets associated
            sql = "delete from users where Id not in (select userid from codesnippets) and entered < @date";
            var result3 = ExecuteNonQuery(sql,
                Context.Db.CreateParameter("@date", DateTime.UtcNow.AddMonths(3)));

            if (result3 < 0)
            {
                return -1;
            }


            return result + result2;
        }

    }
}
