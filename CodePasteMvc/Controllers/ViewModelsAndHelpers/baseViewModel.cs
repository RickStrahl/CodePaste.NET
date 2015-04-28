using System;
using System.Web;
using System.Web.Mvc;
using Westwind.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;
using System.Text;
using Westwind.Utilities;
using System.Collections;
using System.Linq;
using System.ServiceModel.Syndication;
using CodePasteBusiness;
using System.Xml;
using System.IO;
using System.Collections.Generic;

namespace CodePasteMvc.Controllers
{
    public class baseViewModel
    {
        public ErrorDisplay ErrorDisplay = null;
        public AppUserState AppUserState = null;
        public string baseUrl = HttpContext.Current.Request.ApplicationPath;
        public string PageTitle = null;

        public PagingDetails Paging = null;
    }

    /// <summary>
    /// Contains information 
    /// </summary>
    public class PagingDetails
    {
        public bool RenderPager = true;

        public int Page = 1;        
        public int PageCount = 1;
        public int PageSize = App.Configuration.MaxListDisplayCount;
        public int TotalPages = 1;
        public int TotalItems = 0;

        public int MaxPageButtons = 10;

        /// <summary>
        /// Client handler function called on POST operation with page number as parameter
        /// </summary>
        public string ClientPageClickHandler = "pageClick";        
    }
}
