using System;
using System.Web;
using System.Web.Mvc;
using CodePasteBusiness;
using Westwind.Web.Mvc;
using Westwind.Utilities;
using System.Text;
using System.Collections.Generic;
using System.Web.Routing;

namespace CodePasteMvc.Controllers
{
    /// <summary>
    /// Application specific UrlHelper Extensions
    /// </summary>
    public static class UrlHelperExtensions
    {

        /// <summary>
        /// Create a list of links from a Snippet's tags. Tag string should
        /// be comma delimited.
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static string GetTagLinkList(this UrlHelper urlHelper, string tags)
        {
            if (string.IsNullOrEmpty(tags))
                return string.Empty;

            string[] tagStrings = tags.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder html = new StringBuilder();
            foreach (string tagString in tagStrings)
            {
                string urlAction = urlHelper.Content("~/list/tag/") + tagString.Trim();
                //string urlAction = urlHelper.Action("List", "Snippet", 
                //                                     new { 
                //                                         listAction = "tag", 
                //                                         listFilter = tagString.Trim()
                //                                     });
                html.Append(HtmlUtils.Href(tagString.Trim(), urlAction) + ", ");
            }

            if (html.Length > 2)
                html.Length -= 2;

            return html.ToString();
        }
        
    }
}
