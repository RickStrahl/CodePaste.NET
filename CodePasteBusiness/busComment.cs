using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Westwind.BusinessFramework.LinqToSql;
using Westwind.Utilities;
using Manoli.Utils.CSharpFormat;

namespace CodePasteBusiness
{
    public class busComment : BusinessObjectLinq<Comment,CodePasteDataContext>
    {

        public override Comment NewEntity()
        {
            Comment comment = base.NewEntity();

            comment.Id = DataUtils.GenerateUniqueId();
            comment.Author = string.Empty;
            comment.UserId = string.Empty;
            comment.CommentText = string.Empty;
            comment.Entered = DateTime.Now;

            return comment;
        }

        /// <summary>
        /// Deletes all comments for a specific user
        /// </summary>
        /// <param name="snippetId"></param>
        /// <returns></returns>
        public bool DeleteCommentsForSnippet(string snippetId)
        {
            int result =  this.ExecuteNonQuery("delete from Comments where snippetId = @snippetId",
                 this.Context.Db.CreateParameter("@snippetId", snippetId));
            
           return result > -1;                
        }

        /// <summary>
        /// Returns the comments for a given code snippet.
        /// </summary>
        /// <param name="snippetId"></param>
        /// <returns></returns>
        public List<Comment> GetCommentsForSnippet(string snippetId)
        {
            return this.Context.Comments.Where(cmt => cmt.SnippetId == snippetId).ToList();
         
        }
    }
}
