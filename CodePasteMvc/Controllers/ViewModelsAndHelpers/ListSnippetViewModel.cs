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
    public class ListSnippetViewModel :baseViewModel
    {
        public ListSnippetViewModel(SnippetController controller)
        {
            this.Controller = controller;
        }
        public SnippetController Controller = null;
        public busCodeSnippet busSnippet = null; 
        public List<CodeSnippetListItem> SnippetList = null;

        public CodeSnippetSearchParameters Parameters = null;
        public List<SelectListItem> SearchOrderItems = null;
    }
}
